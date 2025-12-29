using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class PlaneController : MonoBehaviour
{
    public float forwardSpeed = 5f;
    public float liftForce = 20f;
    public float gravity = 30f;
    public float maxUpSpeed = 6f;
    public float maxDownSpeed = -10f;
    public float targetFallSpeed = -2f;
    public float fallSmoothTime = 0.15f;

    [Header("Rotation")]
    public float maxTiltAngle = 10f;
    public float tiltSmooth = 5f;

    [Header("Crash Rotation")]
    public float crashTorque = 200f;
    public float maxAngularSpeed = 360f;

    [Header("Audio")]
    public AudioClip flightSound;
    public AudioClip crashSound;
    [Range(0f, 1f)]
    public float flightVolume = 0.6f;
    [Range(0f, 1f)]
    public float crashVolume = 1f;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private float verticalVelocity;
    private float fallVelocityRef;
    private bool isCrashed;
    public bool isStopGame;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        rb.gravityScale = 0f;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.clip = flightSound;
        audioSource.volume = flightVolume;

        StartCoroutine(WaitForPlayerDataAndInitializeSound());
    }

    private IEnumerator WaitForPlayerDataAndInitializeSound()
    {
        while (PlayerData.Instance == null)
        {
            yield return null;
        }
        UpdateSoundState();
        if (PlayerData.Instance.SoundOn && flightSound != null && !isCrashed)
        {
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (audioSource.isPlaying && audioSource.clip == flightSound && !isCrashed)
            {
                audioSource.Pause();
            }
        }
        else
        {
            if (audioSource.clip == flightSound &&
                !audioSource.isPlaying &&
                !isCrashed &&
                PlayerData.Instance != null &&
                PlayerData.Instance.SoundOn &&
                flightSound != null)
            {
                audioSource.UnPause();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isStopGame)
        {
            rb.linearVelocity = new Vector2(forwardSpeed, 0f);
            return;
        }

        if (isCrashed)
        {
            verticalVelocity -= gravity * Time.fixedDeltaTime;
            verticalVelocity = Mathf.Clamp(verticalVelocity, maxDownSpeed, 0f);
            rb.linearVelocity = new Vector2(0f, verticalVelocity);
            rb.AddTorque(Random.Range(-crashTorque, crashTorque) * Time.fixedDeltaTime, ForceMode2D.Force);
            rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxAngularSpeed, maxAngularSpeed);
            return;
        }

        bool isPressed = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        if (isPressed)
            verticalVelocity += liftForce * Time.fixedDeltaTime;
        else
            verticalVelocity = Mathf.SmoothDamp(verticalVelocity, targetFallSpeed, ref fallVelocityRef, fallSmoothTime);

        verticalVelocity = Mathf.Clamp(verticalVelocity, maxDownSpeed, maxUpSpeed);
        rb.linearVelocity = new Vector2(forwardSpeed, verticalVelocity);

        float targetZ = (verticalVelocity / maxUpSpeed) * maxTiltAngle;
        rb.MoveRotation(Mathf.LerpAngle(rb.rotation, targetZ, tiltSmooth * Time.fixedDeltaTime));

        ClampPosition();
    }

    private void ClampPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        float height = 1.7f * cam.orthographicSize;
        float yMin = cam.transform.position.y - height / 2f;
        float yMax = cam.transform.position.y + height / 2f;
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, yMin, yMax);
        transform.position = pos;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Crash();
    }

    private void Crash()
    {
        if (isCrashed) return;
        isCrashed = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddTorque(Random.Range(-crashTorque, crashTorque), ForceMode2D.Impulse);

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        if (PlayerData.Instance != null && PlayerData.Instance.SoundOn && crashSound != null)
        {
            audioSource.loop = false;
            audioSource.clip = crashSound;
            audioSource.volume = crashVolume;
            audioSource.Play();
        }

        if (PlayerData.Instance != null &&
            PlayerData.Instance.VibrationOn &&
            Application.isMobilePlatform)
        {
            Handheld.Vibrate();
        }

        Debug.Log("CRASH!");
        StartCoroutine(GameOverDelay());
    }

    private IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(2f);
        GameManager.Instance.GameOver();
    }

    public void UpdateSoundState()
    {
        if (audioSource != null && PlayerData.Instance != null)
        {
            audioSource.mute = !PlayerData.Instance.SoundOn;
        }
    }

    private void OnEnable()
    {
        if (PlayerData.Instance != null)
        {
            UpdateSoundState();
        }
    }
}