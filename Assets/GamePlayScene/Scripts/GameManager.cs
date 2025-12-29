using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public GameObject Header;
    public GameObject Num;
    public GameObject PauseMenu;
    public GameObject GameOverMenu;
    public GameObject Background;

    [Header("Score")]
    public TextMeshProUGUI ScoreTextHeader;
    public TextMeshProUGUI ScoreTextGameOver;
    public TextMeshProUGUI BestScoreText;

    [Header("Countdown")]
    public TextMeshProUGUI CountdownText;

    [Header("Settings")]
    public float fadeDuration = 0.5f;

    private float distanceTraveled = 0f;
    private int score = 0;
    private Vector3 lastPosition;

    [HideInInspector] public bool isPlaying = false;
    [HideInInspector] public bool isPaused = false;

    public PlaneController _planeController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (Header != null) Header.SetActive(false);
        if (Num != null) Num.SetActive(false);
        if (PauseMenu != null) PauseMenu.SetActive(false);
        if (GameOverMenu != null) GameOverMenu.SetActive(false);
        if (Background != null) Background.SetActive(false);
    }

    private void Start()
    {
        Play();
        if (_planeController != null)
        {
            lastPosition = _planeController.transform.position;
            distanceTraveled = 0f;
            score = 0;
        }

        StartCoroutine(CountdownCoroutine(() =>
        {
            isPlaying = true;
            if (_planeController != null)
            {
                _planeController.isStopGame = false;
                lastPosition = _planeController.transform.position;
            }
        }));
    }

    private void Update()
    {
        if (isPlaying && !isPaused)
        {
            UpdateScore();
        }
    }

    private void UpdateScore()
    {
        if (_planeController == null) return;

        Vector3 currentPos = _planeController.transform.position;
        float deltaX = currentPos.x - lastPosition.x;

        if (deltaX > 0.01f)
        {
            distanceTraveled += deltaX;
            score = Mathf.FloorToInt(distanceTraveled);
        }

        lastPosition = currentPos;

        if (ScoreTextHeader != null)
            ScoreTextHeader.text = $"Score: {score}";

        if (GameOverMenu != null && GameOverMenu.activeSelf && ScoreTextGameOver != null)
            ScoreTextGameOver.text = $"Score: {score}";
    }

    #region Public Methods

    public void Play()
    {
        if (!isPlaying)
        {
            distanceTraveled = 0f;
            score = 0;
            if (_planeController != null)
                lastPosition = _planeController.transform.position;
        }

        StartCoroutine(CountdownCoroutine(() =>
        {
            isPlaying = true;
            isPaused = false;
            Time.timeScale = 1f;
            if (_planeController != null) _planeController.isStopGame = false;
            if (PauseMenu != null) StartCoroutine(HideScreen(PauseMenu));
        }));
    }

    public void Pause()
    {
        if (!isPlaying || isPaused) return;

        isPaused = true;
        if (_planeController != null) _planeController.isStopGame = true;
        if (Background != null) Background.SetActive(true);
        if (Header != null) Header.SetActive(false);
        Time.timeScale = 0f;
        if (PauseMenu != null) StartCoroutine(ShowScreen(PauseMenu));
    }

    public void Resume()
    {
        if (!isPlaying || !isPaused) return;

        if (PauseMenu != null) StartCoroutine(HideScreen(PauseMenu));
        if (Background != null) Background.SetActive(false);

        StartCoroutine(CountdownCoroutine(() =>
        {
            isPaused = false;
            Time.timeScale = 1f;
            if (_planeController != null)
            {
                _planeController.isStopGame = false;
                lastPosition = _planeController.transform.position;
            }
        }));
    }

    public void GameOver()
    {
        isPlaying = false;
        isPaused = false;
        Time.timeScale = 0f;

        if (Background != null) Background.SetActive(true);
        if (Header != null) Header.SetActive(false);

        if (GameOverMenu != null)
        {
            StartCoroutine(ShowScreen(GameOverMenu));

            if (ScoreTextGameOver != null)
                ScoreTextGameOver.text = $"Score: {score}";

            int bestScore = PlayerPrefs.GetInt("BestScore", 0);
            if (score > bestScore)
            {
                bestScore = score;
                PlayerPrefs.SetInt("BestScore", bestScore);
                PlayerPrefs.Save();
            }

            if (BestScoreText != null)
                BestScoreText.text = $"Best: {bestScore}";
        }
    }

    #endregion

    #region Countdown 

    private IEnumerator CountdownCoroutine(System.Action onComplete = null)
    {

        if (Header != null) Header.SetActive(false);
        if (Num != null) Num.SetActive(true);
        if (_planeController != null) _planeController.isStopGame = true;

        string[] countdown = { "3", "2", "1", "GO!" };
        foreach (string num in countdown)
        {
            if (CountdownText != null) CountdownText.text = num;
            yield return new WaitForSecondsRealtime(1f); 
        }

        if (Num != null) Num.SetActive(false);
        if (Header != null) Header.SetActive(true);
        if (_planeController != null) _planeController.isStopGame = false;

        onComplete?.Invoke();
    }

    #endregion

    #region UI Animations 

    private CanvasGroup GetCanvasGroup(GameObject obj)
    {
        if (obj == null) return null;
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }

    private IEnumerator ShowScreen(GameObject obj)
    {
        if (obj == null) yield break;
        obj.SetActive(true);

        CanvasGroup cg = GetCanvasGroup(obj);
        if (cg == null) yield break;

        cg.alpha = 0f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; 
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private IEnumerator HideScreen(GameObject obj)
    {
        if (obj == null || !obj.activeSelf) yield break;

        CanvasGroup cg = GetCanvasGroup(obj);
        if (cg == null) yield break;

        float t = 0f;
        float startAlpha = cg.alpha;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; 
            cg.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        obj.SetActive(false);
    }

    #endregion
}