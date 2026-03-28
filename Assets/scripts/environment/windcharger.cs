using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windcharger : MonoBehaviour
{
    public windarea windarea;
    private Player1 player;
    [Header("UI 交互提示设置")]
    [Tooltip("拖入包含提示文本的 CanvasGroup")]
    public CanvasGroup promptCanvasGroup;
    public float fadeSpeed = 5f;

    // 内部状态
    private bool isPlayerInRange = false;
    private bool isInteracted = false;

    void Start()
    {

        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (isPlayerInRange && !isInteracted && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(HandleInteractionRoutine());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isInteracted)
        {
            isPlayerInRange = true;
            StopAllCoroutines();
            StartCoroutine(FadePrompt(1f));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            isInteracted = false;

            StopAllCoroutines();
            StartCoroutine(FadePrompt(0f));
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

    private IEnumerator HandleInteractionRoutine()
    {
        isInteracted = true;

        if (promptCanvasGroup != null)
        {
            Transform promptTransform = promptCanvasGroup.transform;
            Vector3 originalScale = promptTransform.localScale;

            float timer = 0;
            float punchTime = 0.1f;
            while (timer < punchTime)
            {
                timer += Time.deltaTime;
                promptTransform.localScale = Vector3.Lerp(originalScale, originalScale * 1.3f, timer / punchTime);
                yield return null;
            }
            timer = 0;
            while (timer < punchTime)
            {
                timer += Time.deltaTime;
                promptTransform.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale, timer / punchTime);
                yield return null;
            }
            promptTransform.localScale = originalScale;
        }

        windarea.enabled = true;
        StartCoroutine(FadePrompt(0f));
    }
    
}
