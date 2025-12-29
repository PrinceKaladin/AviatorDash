using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToggleSettingButton : MonoBehaviour
{
    public enum SettingType
    {
        Music,
        Sound,
        Vibration
    }

    [Header("Setting Type")]
    public SettingType settingType;

    [Header("UI")]
    public Image buttonImage;
    public Sprite onSprite;
    public Sprite offSprite;

    private Coroutine waitRoutine;

    private void Awake()
    {
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        TryRefreshState();
    }

    private void OnDisable()
    {
        if (waitRoutine != null)
        {
            StopCoroutine(waitRoutine);
            waitRoutine = null;
        }
    }

    public void Toggle()
    {
        if (PlayerData.Instance == null)
            return;

        switch (settingType)
        {
            case SettingType.Music:
                PlayerData.Instance.MusicOn = !PlayerData.Instance.MusicOn;
                break;

            case SettingType.Sound:
                PlayerData.Instance.SoundOn = !PlayerData.Instance.SoundOn;
                break;

            case SettingType.Vibration:
                PlayerData.Instance.VibrationOn = !PlayerData.Instance.VibrationOn;
                break;
        }

        RefreshState();
    }

    private void TryRefreshState()
    {
        if (PlayerData.Instance == null)
        {
            if (waitRoutine == null)
                waitRoutine = StartCoroutine(WaitAndRetry());

            return;
        }

        RefreshState();
    }

    private IEnumerator WaitAndRetry()
    {
        yield return null;

        waitRoutine = null;

        if (isActiveAndEnabled)
            TryRefreshState();
    }

    private void RefreshState()
    {
        if (buttonImage == null)
            return;

        bool isOn = GetCurrentState();
        buttonImage.sprite = isOn ? onSprite : offSprite;
    }

    private bool GetCurrentState()
    {
        return settingType switch
        {
            SettingType.Music => PlayerData.Instance.MusicOn,
            SettingType.Sound => PlayerData.Instance.SoundOn,
            SettingType.Vibration => PlayerData.Instance.VibrationOn,
            _ => true
        };
    }
}
