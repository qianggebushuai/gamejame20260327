using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jigunacontinued : MonoBehaviour
{
    [Header("持续控制设置")]
    [Tooltip("要直接控制的水体")]
    public WaterBody targetWater;
    [Tooltip("每秒上升/下降的速度 (正数上升，负数下降)")]
    public float waterChangeSpeed = 2f;
    public GameObject obstacle;

    [Header("UI 交互提示设置")]
    [Tooltip("拖入包含提示文本的 CanvasGroup")]
    public CanvasGroup promptCanvasGroup;
    public float fadeSpeed = 5f;      // 文本淡入淡出的速度

    // 内部状态
    private bool isPlayerInRange = false;
    private bool isHolding = false;   // 玩家是否正在按住按键

    // UI 动画记录
    private Vector3 originalPromptScale;
    private Coroutine scaleCoroutine;
    void Start()
    {
        // 初始化 UI 状态
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
            originalPromptScale = promptCanvasGroup.transform.localScale;
        }
    }

    void Update()
    {
        // 只有玩家在范围内才允许交互
        if (isPlayerInRange)
        {
            // 检测是否正在持续按住 F 键
            if (Input.GetKey(KeyCode.F))
            {
                if (!isHolding)
                {
                    isHolding = true;
                    HoldUIEffect(true); // 触发UI按下效果
                }

                // 执行持续控制水位的逻辑
                ControlWaterContinuously();
            }
            // 当松开 F 键时
            else if (isHolding)
            {
                isHolding = false;
                HoldUIEffect(false); // 恢复UI原本大小
            }
        }
    }

    // ================= 核心机关逻辑 (持续触发) =================

    private void ControlWaterContinuously()
    {
        if (obstacle == null)
        {
            if (targetWater != null)
            {
                // 使用 Time.deltaTime 让水位平滑且匀速地变化
                // 因为是每帧调用，所以水位会随着按住的时间一点点改变
                float frameChangeAmount = waterChangeSpeed * Time.deltaTime;
                targetWater.ChangeWaterLevelBy(frameChangeAmount);
            }
        }
        else
        {
            // （可选）如果想在被挡住时加点限制或提示，可以写在这里
            // Debug.Log("通道被障碍物挡住了！无法操作阀门");
        }
    }

    // ================= 触发器检测逻辑 =================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            StopAllCoroutines();
            StartCoroutine(FadePrompt(1f)); // 显示文本
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // 如果玩家按着按键走出了范围，强制重置状态
            if (isHolding)
            {
                isHolding = false;
                HoldUIEffect(false);
            }

            StopAllCoroutines();
            StartCoroutine(FadePrompt(0f)); // 隐藏文本
        }
    }

    // ================= 动画与表现逻辑 =================

    /// <summary>
    /// 控制 UI 透明度平滑过渡的协程
    /// </summary>
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

    private void HoldUIEffect(bool isPressed)
    {
        if (promptCanvasGroup == null) return;

        Vector3 targetScale = isPressed ? originalPromptScale * 0.85f : originalPromptScale;

        // 【关键修复】：如果当前正在缩放，先强制停掉它，防止动画打架！
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        scaleCoroutine = StartCoroutine(ScaleUIRoutine(targetScale));
    }

    private IEnumerator ScaleUIRoutine(Vector3 targetScale)
    {
        Transform promptTransform = promptCanvasGroup.transform;

        float timer = 0;
        float punchTime = 0.1f; // 动画时间
        Vector3 startScale = promptTransform.localScale;

        while (timer < punchTime)
        {
            timer += Time.deltaTime;
            promptTransform.localScale = Vector3.Lerp(startScale, targetScale, timer / punchTime);
            yield return null;
        }
        promptTransform.localScale = targetScale;
    }
}
