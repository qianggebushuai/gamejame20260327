using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Image 透明度渐变控制器
/// 支持：淡入、淡出、循环闪烁、自动销毁等模式
/// </summary>
public class ImageFadeController : MonoBehaviour
{
    [Header("目标组件")]
    [Tooltip("目标Image组件，为空则自动获取当前物体上的Image")]
    public Image targetImage;

    [Header("透明速度设置")]
    [Tooltip("渐变速度（每秒变化的alpha值），值越大变化越快")]
    [Range(0.1f, 10f)]
    public float fadeSpeed = 1f;

    [Tooltip("是否忽略Time.timeScale（暂停时仍继续渐变）")]
    public bool ignoreTimeScale = false;

    [Header("循环模式")]
    [Tooltip("是否循环渐变（透明→不透明→透明）")]
    public bool loop = false;

    [Tooltip("循环模式：正向(0→1)或往返(0→1→0)")]
    public LoopMode loopMode = LoopMode.PingPong;

    public enum LoopMode { Forward, PingPong }

    [Header("自动销毁")]
    [Tooltip("完全透明后是否自动销毁物体")]
    public bool destroyWhenInvisible = false;

    [Tooltip("完全透明后是否禁用组件而非销毁")]
    public bool disableWhenInvisible = false;

    // 运行时状态
    public float targetAlpha = 1f;      // 目标透明度
    public float currentAlpha = 1f;     // 当前透明度
    private bool isFading = false;       // 是否正在渐变
    private bool isPaused = false;       // 是否暂停

    void Awake()
    {
        // 自动获取Image组件
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
            if (targetImage == null)
            {
                Debug.LogError($"[{nameof(ImageFadeController)}] 未找到Image组件，请手动赋值或添加到Image物体上", this);
                enabled = false;
                return;
            }
            
        }

        // 记录初始透明度
        currentAlpha = targetImage.color.a;

        FadeOut();
    }

    void Update()
    {
        if (!isFading || isPaused) return;

        float deltaTime = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

        // 向目标透明度渐变
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * deltaTime);

        // 应用透明度
        ApplyAlpha(currentAlpha);

        // 检查是否到达目标
        if (Mathf.Approximately(currentAlpha, targetAlpha))
        {
            OnFadeComplete();
        }
    }

    /// <summary>
    /// 开始淡出（变透明）
    /// </summary>
    public void FadeOut()
    {
        targetAlpha = 0f;
        isFading = true;
        isPaused = false;
    }

    /// <summary>
    /// 开始淡入（变不透明）
    /// </summary>
    public void FadeIn()
    {
        targetAlpha = 1f;
        isFading = true;
        isPaused = false;
    }

    /// <summary>
    /// 渐变到指定透明度
    /// </summary>
    public void FadeTo(float alpha)
    {
        targetAlpha = Mathf.Clamp01(alpha);
        isFading = true;
        isPaused = false;
    }

    /// <summary>
    /// 立即设置透明度（无渐变）
    /// </summary>
    public void SetAlphaImmediate(float alpha)
    {
        currentAlpha = Mathf.Clamp01(alpha);
        targetAlpha = currentAlpha;
        ApplyAlpha(currentAlpha);
        isFading = false;
    }

    /// <summary>
    /// 暂停渐变
    /// </summary>
    public void Pause()
    {
        isPaused = true;
    }

    /// <summary>
    /// 继续渐变
    /// </summary>
    public void Resume()
    {
        isPaused = false;
    }

    /// <summary>
    /// 停止渐变
    /// </summary>
    public void Stop()
    {
        isFading = false;
    }

    // 应用透明度到Image
    private void ApplyAlpha(float alpha)
    {
        Color color = targetImage.color;
        color.a = alpha;
        targetImage.color = color;
    }

    // 渐变完成处理
    private void OnFadeComplete()
    {
        if (loop)
        {
            // 循环模式：反转目标
            if (loopMode == LoopMode.PingPong)
            {
                targetAlpha = (targetAlpha > 0.5f) ? 0f : 1f;
            }
            else
            {
                currentAlpha = (targetAlpha > 0.5f) ? 0f : 1f;
            }
        }
        else
        {
            isFading = false;

            // 自动销毁或禁用
            if (destroyWhenInvisible && currentAlpha <= 0.01f)
            {
                Destroy(gameObject);
            }
            else if (disableWhenInvisible && currentAlpha <= 0.01f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}