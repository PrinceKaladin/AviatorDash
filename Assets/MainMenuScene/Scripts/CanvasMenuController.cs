using UnityEngine;
using System.Collections;

public class CanvasMenuController : MonoBehaviour
{
    [Header("Screens")]
    public GameObject Menu;
    public GameObject Settings;
    public GameObject HowToPlay;

    [Header("Background")]
    public RectTransform Background;

    [Header("Background Positions")]
    public Vector2 MenuBGPos;
    public Vector2 SettingsBGPos;
    public Vector2 HowToPlayBGPos;

    [Header("Logo")]
    public RectTransform Logo;
    public Vector2 LogoUpperPosition;
    public float LogoMoveDuration = 1.2f;
    public float LogoPause = 0.2f;

    [Header("Animation")]
    public float fadeDuration = 0.3f;
    public float moveDuration = 0.4f;

    private Coroutine currentRoutine;
    private Vector2 logoStartPos;

    private void Start()
    {
        if (Logo != null)
        {
            logoStartPos = Logo.anchoredPosition;
            StartCoroutine(LogoFloatRoutine());
        }
    }

    #region Public Methods

    public void ShowMenu()
    {
        SwitchScreen(Menu, MenuBGPos);
    }

    public void ShowSettings()
    {
        SwitchScreen(Settings, SettingsBGPos);
    }

    public void ShowHowToPlay()
    {
        SwitchScreen(HowToPlay, HowToPlayBGPos);
    }

    #endregion

    #region Core Logic

    private void SwitchScreen(GameObject target, Vector2 bgTargetPos)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(SwitchRoutine(target, bgTargetPos));
    }

    private IEnumerator SwitchRoutine(GameObject target, Vector2 bgTargetPos)
    {
        yield return StartCoroutine(HideScreen(Menu));
        yield return StartCoroutine(HideScreen(Settings));
        yield return StartCoroutine(HideScreen(HowToPlay));

        yield return StartCoroutine(MoveBackground(bgTargetPos));

        yield return StartCoroutine(ShowScreen(target));
    }

    #endregion

    #region Animations

    private IEnumerator ShowScreen(GameObject obj)
    {
        if (!obj) yield break;

        obj.SetActive(true);

        CanvasGroup cg = GetCanvasGroup(obj);
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private IEnumerator HideScreen(GameObject obj)
    {
        if (!obj || !obj.activeSelf) yield break;

        CanvasGroup cg = GetCanvasGroup(obj);

        float t = 0f;
        float startAlpha = cg.alpha;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        obj.SetActive(false);
    }

    private IEnumerator MoveBackground(Vector2 targetPos)
    {
        Vector2 startPos = Background.anchoredPosition;
        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            Background.anchoredPosition = Vector2.Lerp(startPos, targetPos, t / moveDuration);
            yield return null;
        }

        Background.anchoredPosition = targetPos;
    }

    #endregion

    #region Logo Animation

    private IEnumerator LogoFloatRoutine()
    {
        while (true)
        {
            yield return MoveLogo(logoStartPos, LogoUpperPosition);
            yield return new WaitForSeconds(LogoPause);
            yield return MoveLogo(LogoUpperPosition, logoStartPos);
            yield return new WaitForSeconds(LogoPause);
        }
    }

    private IEnumerator MoveLogo(Vector2 from, Vector2 to)
    {
        float t = 0f;
        while (t < LogoMoveDuration)
        {
            t += Time.deltaTime;
            Logo.anchoredPosition = Vector2.Lerp(from, to, t / LogoMoveDuration);
            yield return null;
        }
        Logo.anchoredPosition = to;
    }

    #endregion

    #region Helpers

    private CanvasGroup GetCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (!cg)
            cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }

    #endregion
}
