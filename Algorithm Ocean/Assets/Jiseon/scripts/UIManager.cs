using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Logo Image")]
    [SerializeField] public Image logoImage;

    [Header("Init Panel")]
    [SerializeField] public GameObject initPanel;
    [SerializeField] public CanvasGroup initPanelCanvasGroup;

    [Header("Logo Fade Settings")]
    [SerializeField] public float fadeInTime = 1f;
    [SerializeField] public float stayTime = 1.2f;
    [SerializeField] public float fadeOutTime = 1f;

    [Header("Init Panel Fade Settings")]
    [SerializeField] public float initPanelFadeInTime = 1f;

    private void Start()
    {
        initPanel.SetActive(false);
        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        yield return LogoFadeRoutine();
        yield return InitPanelFadeInRoutine();
    }

    private IEnumerator LogoFadeRoutine()
    {
        logoImage.gameObject.SetActive(true);
        SetLogoAlpha(0f);

        yield return FadeLogo(0f, 1f, fadeInTime);
        yield return new WaitForSeconds(stayTime);
        yield return FadeLogo(1f, 0f, fadeOutTime);

        logoImage.gameObject.SetActive(false);
    }

    private IEnumerator InitPanelFadeInRoutine()
    {
        initPanel.SetActive(true);

        initPanelCanvasGroup.alpha = 0f;
        initPanelCanvasGroup.interactable = false;
        initPanelCanvasGroup.blocksRaycasts = false;

        float elapsed = 0f;

        while (elapsed < initPanelFadeInTime)
        {
            elapsed += Time.deltaTime;
            initPanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / initPanelFadeInTime);
            yield return null;
        }

        initPanelCanvasGroup.alpha = 1f;
        initPanelCanvasGroup.interactable = true;
        initPanelCanvasGroup.blocksRaycasts = true;
    }

    private IEnumerator FadeLogo(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            SetLogoAlpha(alpha);
            yield return null;
        }

        SetLogoAlpha(to);
    }

    private void SetLogoAlpha(float alpha)
    {
        Color color = logoImage.color;
        color.a = alpha;
        logoImage.color = color;
    }
}