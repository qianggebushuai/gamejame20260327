using UnityEngine;
using System.Collections;

/// <summary>
/// 综合 UI 动画效果
/// </summary>
public class UIAnimator : MonoBehaviour
{
    [Header("Rotation Animation")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 0, 50f);
    [SerializeField] private bool rotationWave = false;
    [SerializeField] private float rotationWaveAmplitude = 15f;
    [SerializeField] private float rotationWaveFrequency = 1f;

    [Header("Scale Animation")]
    [SerializeField] private bool enableScale = true;
    [SerializeField] private float scaleMin = 0.95f;
    [SerializeField] private float scaleMax = 1.05f;
    [SerializeField] private float scaleSpeed = 2f;

    [Header("Float Animation (上下浮动)")]
    [SerializeField] private bool enableFloat = false;
    [SerializeField] private float floatAmplitude = 10f;
    [SerializeField] private float floatSpeed = 1f;

    [Header("Fade Animation (淡入淡出)")]
    [SerializeField] private bool enableFade = false;
    [SerializeField] private float fadeMin = 0.5f;
    [SerializeField] private float fadeMax = 1f;
    [SerializeField] private float fadeSpeed = 2f;

    private Vector3 originalPosition;
    private Vector3 originalRotation;
    private Vector3 originalScale;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localEulerAngles;
        originalScale = transform.localScale;

        if (enableFade)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void Update()
    {
        // 旋转动画
        if (enableRotation)
        {
            if (rotationWave)
            {
                float wave = Mathf.Sin(Time.time * rotationWaveFrequency) * rotationWaveAmplitude;
                transform.localRotation = Quaternion.Euler(originalRotation + new Vector3(0, 0, wave));
            }
            else
            {
                transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
            }
        }

        // 缩放动画
        if (enableScale)
        {
            float scale = Mathf.Lerp(scaleMin, scaleMax, (Mathf.Sin(Time.time * scaleSpeed) + 1f) / 2f);
            transform.localScale = originalScale * scale;
        }

        // 浮动动画
        if (enableFloat)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.localPosition = originalPosition + new Vector3(0, yOffset, 0);
        }

        // 淡入淡出动画
        if (enableFade && canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(fadeMin, fadeMax, (Mathf.Sin(Time.time * fadeSpeed) + 1f) / 2f);
        }
    }

    /// <summary>
    /// 重置到初始状态
    /// </summary>
    public void ResetToOriginal()
    {
        transform.localPosition = originalPosition;
        transform.localRotation = Quaternion.Euler(originalRotation);
        transform.localScale = originalScale;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// 播放进入动画
    /// </summary>
    public void PlayEnterAnimation()
    {
        StartCoroutine(EnterAnimation());
    }

    private IEnumerator EnterAnimation()
    {
        // 从小到大 + 旋转进入
        transform.localScale = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0, 0, 360);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 使用缓动函数
            float easedT = EaseOutBack(t);

            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, easedT);
            transform.localRotation = Quaternion.Lerp(Quaternion.Euler(0, 0, 360), Quaternion.Euler(originalRotation), t);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }

        transform.localScale = originalScale;
        transform.localRotation = Quaternion.Euler(originalRotation);
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}