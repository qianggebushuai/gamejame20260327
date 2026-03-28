using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Startrainandchangewater : MonoBehaviour
{
    [Header("UI 交互提示")]
    [Tooltip("拖入包含提示文本的 CanvasGroup")]
    public CanvasGroup promptCanvasGroup;
    public float fadeSpeed = 5f;

    [Header("下雨演出设置")]
    [Tooltip("拖入场景中的下雨粒子特效 (Particle System)")]
    public ParticleSystem rainParticle;
    [Tooltip("按下开关后，下雨多久（秒）水面才开始上涨？")]
    public float delayBeforeWaterRise = 3f;
    [Tooltip("下雨持续总时间（秒），时间到后雨会自动停。设为 0 表示一直下")]
    public float rainTotalDuration = 10f;

    [Header("水位变化设置")]
    [Tooltip("拖入需要上涨的水池")]
    public WaterBody targetWater;
    [Tooltip("水位要上涨多少？(正数上升，负数下降)")]
    public float waterRiseAmount = 5f;

    // 内部状态
    private bool isPlayerInRange = false;
    private bool isInteracted = false; // 防止重复按下

    void Start()
    {
        // 初始化 UI 隐藏
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }

        // 确保一开始是没有下雨的
        if (rainParticle != null && rainParticle.isPlaying)
        {
            rainParticle.Stop();
        }
    }

    void Update()
    {
        // 玩家在范围内 + 还没触发过 + 按下 F 键
        if (isPlayerInRange && !isInteracted && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(HandleInteractionAndRainRoutine());
        }
    }

    // ================= 触发器检测逻辑 =================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isInteracted)
        {
            isPlayerInRange = true;
            StopCoroutine("FadePrompt");
            StartCoroutine(FadePrompt(1f)); // 淡入文本
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            StopCoroutine("FadePrompt");
            StartCoroutine(FadePrompt(0f)); // 淡出文本
        }
    }

    private IEnumerator FadePrompt(float targetAlpha)
    {
        if (promptCanvasGroup == null) yield break;

        while (Mathf.Abs(promptCanvasGroup.alpha - targetAlpha) > 0.01f)
        {
            promptCanvasGroup.alpha = Mathf.MoveTowards(promptCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
        promptCanvasGroup.alpha = targetAlpha;
    }

    // ================= 核心演出逻辑 (动画 -> 下雨 -> 涨水) =================

    private IEnumerator HandleInteractionAndRainRoutine()
    {
        isInteracted = true; // 锁定机关，防止连按

        // 1. UI 确认特效（文字放大回弹）
        if (promptCanvasGroup != null)
        {
            Transform pt = promptCanvasGroup.transform;
            Vector3 origScale = pt.localScale;

            float timer = 0, punchTime = 0.1f;
            while (timer < punchTime)
            {
                timer += Time.deltaTime;
                pt.localScale = Vector3.Lerp(origScale, origScale * 1.3f, timer / punchTime);
                yield return null;
            }
            timer = 0;
            while (timer < punchTime)
            {
                timer += Time.deltaTime;
                pt.localScale = Vector3.Lerp(origScale * 1.3f, origScale, timer / punchTime);
                yield return null;
            }
            pt.localScale = origScale;
        }

        // 2. 隐藏提示框
        StartCoroutine(FadePrompt(0f));

        // 3. 开始下雨演出！
        if (rainParticle != null)
        {
            rainParticle.Play();
            Debug.Log("机关触发：开始下雨！");
        }

        // 4. 等待设定的时间 (模拟雨水慢慢积攒的过程)
        yield return new WaitForSeconds(delayBeforeWaterRise);

        // 5. 触发水位上升
        if (targetWater != null)
        {
            // 调用我们之前在 WaterBody 里写好的平滑改变水位函数
            targetWater.ChangeWaterLevelBy(waterRiseAmount);
            Debug.Log("雨水积攒完毕：水位开始平滑上升！");
        }

        // 6. (可选) 处理雨停逻辑
        if (rainTotalDuration > 0 && rainParticle != null)
        {
            // 等待剩余的时间：总时间 - 已经下雨的时间
            float remainingRainTime = rainTotalDuration - delayBeforeWaterRise;
            if (remainingRainTime > 0)
            {
                yield return new WaitForSeconds(remainingRainTime);
            }

            rainParticle.Stop();
            Debug.Log("雨停了。");
        }
    }
}