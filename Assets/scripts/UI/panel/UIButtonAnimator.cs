using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// UI 按钮动画效果
/// </summary>
public class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation")]
    [SerializeField] private bool enableScale = true;
    [SerializeField] private float hoverScale = 1.1f;      // 悬停时的缩放
    [SerializeField] private float clickScale = 0.9f;      // 点击时的缩放
    [SerializeField] private float scaleSpeed = 10f;       // 缩放速度

    [Header("Rotation Animation")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationAngle = 5f;     // 旋转角度
    [SerializeField] private float rotationSpeed = 5f;     // 旋转速度

    [Header("Color Animation")]
    [SerializeField] private bool enableColorChange = false;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Punch Animation (可选)")]
    [SerializeField] private bool enablePunch = false;
    [SerializeField] private float punchScale = 1.2f;
    [SerializeField] private float punchDuration = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private Image image;
    private Button button;
    private bool isHovering = false;
    private bool isPressed = false;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;

        image = GetComponent<Image>();
        button = GetComponent<Button>();

        if (enableColorChange && image != null)
        {
            normalColor = image.color;
        }
    }

    private void Update()
    {
        // 平滑缩放
        if (enableScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
        }

        // 平滑旋转
        if (enableRotation)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 平滑颜色变化
        if (enableColorChange && image != null)
        {
            Color targetColor = isHovering ? hoverColor : normalColor;
            image.color = Color.Lerp(image.color, targetColor, scaleSpeed * Time.deltaTime);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        isHovering = true;

        if (enableScale)
        {
            targetScale = originalScale * hoverScale;
        }

        if (enableRotation)
        {
            targetRotation = Quaternion.Euler(0, 0, rotationAngle);
        }

        // 播放悬停音效
        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        isHovering = false;
        isPressed = false;

        targetScale = originalScale;
        targetRotation = originalRotation;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        isPressed = true;

        if (enableScale)
        {
            targetScale = originalScale * clickScale;
        }

        // 播放点击音效
        PlaySound(clickSound);

        // Punch 效果
        if (enablePunch)
        {
            StopAllCoroutines();
            StartCoroutine(PunchAnimation());
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        isPressed = false;

        if (isHovering)
        {
            targetScale = originalScale * hoverScale;
        }
        else
        {
            targetScale = originalScale;
        }
    }

    /// <summary>
    /// Punch 弹性动画
    /// </summary>
    private IEnumerator PunchAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration / 2f);
            transform.localScale = Vector3.Lerp(startScale, originalScale * punchScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration / 2f);
            transform.localScale = Vector3.Lerp(originalScale * punchScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}