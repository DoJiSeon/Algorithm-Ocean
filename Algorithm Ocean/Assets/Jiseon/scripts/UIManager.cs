using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Logo Image")]
    [SerializeField] public Image logoImage;

    [Header("Init Panel")]
    [SerializeField] public GameObject initPanel;
    [SerializeField] public CanvasGroup initPanelCanvasGroup;
    [SerializeField] public GameObject buttonObject;
    [SerializeField] private BottleSpawner bottleSpawner;

    [Header("Logo Fade Settings")]
    [SerializeField] public float fadeInTime = 1f;
    [SerializeField] public float stayTime = 1.2f;
    [SerializeField] public float fadeOutTime = 1f;

    [Header("Init Panel Fade Settings")]
    [SerializeField] public float initPanelFadeInTime = 1f;
    [SerializeField] public float initPanelFloatingAmplitude = 10f;
    [SerializeField] public float initPanelFloatingFrequency = 0.35f;
    [SerializeField] public float initPanelHorizontalAmplitude = 5f;
    [SerializeField] public float initPanelHorizontalFrequency = 0.25f;
    [SerializeField] public float initPanelRotationAmplitude = 2f;
    [SerializeField] public float initPanelRotationFrequency = 0.3f;

    [Header("Floating Capsule")]
    [SerializeField] public GameObject floatingObject;
    [SerializeField] public float floatingFadeInTime = 0.35f;
    [SerializeField] public float floatingStayTime = 1.2f;
    [SerializeField] public float floatingFadeOutTime = 0.45f;
    [SerializeField] public float floatingAmplitude = 18f;
    [SerializeField] public float floatingFrequency = 0.45f;
    [SerializeField] public float floatingHorizontalAmplitude = 8f;
    [SerializeField] public float floatingHorizontalFrequency = 0.35f;
    [SerializeField] public float floatingRotationAmplitude = 4f;
    [SerializeField] public float floatingRotationFrequency = 0.4f;

    private Coroutine floatingRoutine;
    private Coroutine initPanelFloatingRoutine;
    private Coroutine buttonFloatingRoutine;
    private bool hasRequestedBottleSpawn;

    private void Start()
    {
        initPanel.SetActive(false);
        if (floatingObject != null)
        {
            floatingObject.SetActive(false);
        }

        if (buttonObject != null)
        {
            buttonObject.SetActive(false);
        }

        StartCoroutine(StartRoutine());
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
        {
            PlayFloatingCapsule();
        }
    }

    public void PlayFloatingCapsule()
    {
        if (floatingObject == null)
        {
            Debug.LogWarning("Floating object is not assigned.");
            return;
        }

        if (floatingRoutine != null)
        {
            StopCoroutine(floatingRoutine);
        }

        floatingRoutine = StartCoroutine(FloatingCapsuleRoutine());
    }

    private IEnumerator StartRoutine()
    {
        yield return LogoFadeRoutine();
        yield return InitPanelFadeInRoutine();
        yield return WaitForInitPanelClosedThenShowButtonRoutine();
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

        StartInitPanelFloating();
        float elapsed = 0f;

        while (elapsed < initPanelFadeInTime)
        {
            elapsed += Time.deltaTime;
            float progress = initPanelFadeInTime <= 0f ? 1f : elapsed / initPanelFadeInTime;

            initPanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
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

    private IEnumerator FloatingCapsuleRoutine()
    {
        CanvasGroup canvasGroup = floatingObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = floatingObject.AddComponent<CanvasGroup>();
        }

        RectTransform rectTransform = floatingObject.GetComponent<RectTransform>();
        Vector3 startPosition = GetFloatingStartPosition(floatingObject, rectTransform);
        Quaternion startRotation = GetFloatingStartRotation(floatingObject, rectTransform);

        floatingObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float totalTime = floatingFadeInTime + floatingStayTime + floatingFadeOutTime;
        float elapsed = 0f;

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            float alpha = GetFloatingAlpha(elapsed);
            float verticalOffset = Mathf.Sin(elapsed * Mathf.PI * 2f * floatingFrequency) * floatingAmplitude;
            float horizontalOffset = Mathf.Sin(elapsed * Mathf.PI * 2f * floatingHorizontalFrequency) * floatingHorizontalAmplitude;
            float rotationOffset = Mathf.Sin(elapsed * Mathf.PI * 2f * floatingRotationFrequency) * floatingRotationAmplitude;

            canvasGroup.alpha = alpha;
            SetFloatingPosition(floatingObject, rectTransform, startPosition, horizontalOffset, verticalOffset);
            SetFloatingRotation(floatingObject, rectTransform, startRotation, rotationOffset);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        SetFloatingPosition(floatingObject, rectTransform, startPosition, 0f, 0f);
        SetFloatingRotation(floatingObject, rectTransform, startRotation, 0f);
        floatingObject.SetActive(false);
        floatingRoutine = null;
    }

    private float GetFloatingAlpha(float elapsed)
    {
        if (elapsed < floatingFadeInTime)
        {
            return floatingFadeInTime <= 0f ? 1f : Mathf.Lerp(0f, 1f, elapsed / floatingFadeInTime);
        }

        float fadeOutStart = floatingFadeInTime + floatingStayTime;
        if (elapsed < fadeOutStart)
        {
            return 1f;
        }

        return floatingFadeOutTime <= 0f ? 0f : Mathf.Lerp(1f, 0f, (elapsed - fadeOutStart) / floatingFadeOutTime);
    }

    private void StartInitPanelFloating()
    {
        if (initPanelFloatingRoutine != null)
        {
            StopCoroutine(initPanelFloatingRoutine);
        }

        initPanelFloatingRoutine = StartCoroutine(InitPanelFloatingRoutine());
    }

    private IEnumerator InitPanelFloatingRoutine()
    {
        RectTransform rectTransform = initPanel.GetComponent<RectTransform>();
        Vector3 startPosition = GetFloatingStartPosition(initPanel, rectTransform);
        Quaternion startRotation = GetFloatingStartRotation(initPanel, rectTransform);
        float elapsed = 0f;

        while (initPanel.activeInHierarchy)
        {
            elapsed += Time.deltaTime;
            float verticalOffset = Mathf.Sin(elapsed * Mathf.PI * 2f * initPanelFloatingFrequency) * initPanelFloatingAmplitude;
            float horizontalOffset = Mathf.Sin(elapsed * Mathf.PI * 2f * initPanelHorizontalFrequency) * initPanelHorizontalAmplitude;
            float rotationOffset = Mathf.Sin(elapsed * Mathf.PI * 2f * initPanelRotationFrequency) * initPanelRotationAmplitude;

            SetFloatingPosition(initPanel, rectTransform, startPosition, horizontalOffset, verticalOffset);
            SetFloatingRotation(initPanel, rectTransform, startRotation, rotationOffset);
            yield return null;
        }

        SetFloatingPosition(initPanel, rectTransform, startPosition, 0f, 0f);
        SetFloatingRotation(initPanel, rectTransform, startRotation, 0f);
        initPanelFloatingRoutine = null;
    }

    private IEnumerator WaitForInitPanelClosedThenShowButtonRoutine()
    {
        while (initPanel != null && initPanel.activeInHierarchy)
        {
            yield return null;
        }

        BeginBottleSpawning();

        if (buttonObject == null)
        {
            yield break;
        }

        buttonObject.SetActive(true);
        StartButtonFloating();
    }

    private void BeginBottleSpawning()
    {
        if (hasRequestedBottleSpawn)
        {
            return;
        }

        hasRequestedBottleSpawn = true;

        if (bottleSpawner == null)
        {
            bottleSpawner = FindFirstObjectByType<BottleSpawner>();
        }

        if (bottleSpawner == null)
        {
            Debug.LogWarning("BottleSpawner is not assigned and could not be found in the scene.", this);
            return;
        }

        bottleSpawner.BeginSpawning();
    }

    private void StartButtonFloating()
    {
        if (buttonFloatingRoutine != null)
        {
            StopCoroutine(buttonFloatingRoutine);
        }

        buttonFloatingRoutine = StartCoroutine(ButtonFloatingRoutine());
    }

    private IEnumerator ButtonFloatingRoutine()
    {
        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        Vector3 startPosition = GetFloatingStartPosition(buttonObject, rectTransform);
        Quaternion startRotation = GetFloatingStartRotation(buttonObject, rectTransform);
        float elapsed = 0f;

        while (buttonObject.activeInHierarchy)
        {
            elapsed += Time.deltaTime;
            float verticalOffset = Mathf.Sin(elapsed * Mathf.PI * 2f * initPanelFloatingFrequency) * initPanelFloatingAmplitude;
            float horizontalOffset = Mathf.Sin(elapsed * Mathf.PI * 2f * initPanelHorizontalFrequency) * initPanelHorizontalAmplitude;
            float rotationOffset = Mathf.Sin(elapsed * Mathf.PI * 2f * initPanelRotationFrequency) * initPanelRotationAmplitude;

            SetFloatingPosition(buttonObject, rectTransform, startPosition, horizontalOffset, verticalOffset);
            SetFloatingRotation(buttonObject, rectTransform, startRotation, rotationOffset);
            yield return null;
        }

        SetFloatingPosition(buttonObject, rectTransform, startPosition, 0f, 0f);
        SetFloatingRotation(buttonObject, rectTransform, startRotation, 0f);
        buttonFloatingRoutine = null;
    }

    private Vector3 GetFloatingStartPosition(GameObject target, RectTransform rectTransform)
    {
        return rectTransform != null ? rectTransform.anchoredPosition3D : target.transform.localPosition;
    }

    private Quaternion GetFloatingStartRotation(GameObject target, RectTransform rectTransform)
    {
        return rectTransform != null ? rectTransform.localRotation : target.transform.localRotation;
    }

    private void SetFloatingPosition(GameObject target, RectTransform rectTransform, Vector3 startPosition, float horizontalOffset, float verticalOffset)
    {
        Vector3 position = startPosition + new Vector3(horizontalOffset, verticalOffset, 0f);

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition3D = position;
            return;
        }

        target.transform.localPosition = position;
    }

    private void SetFloatingRotation(GameObject target, RectTransform rectTransform, Quaternion startRotation, float rotationOffset)
    {
        Quaternion rotation = startRotation * Quaternion.Euler(0f, 0f, rotationOffset);

        if (rectTransform != null)
        {
            rectTransform.localRotation = rotation;
            return;
        }

        target.transform.localRotation = rotation;
    }
}
