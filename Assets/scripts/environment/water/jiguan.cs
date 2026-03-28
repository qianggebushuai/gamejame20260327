using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class jiguan : MonoBehaviour
{
    [Header("机关逻辑设置")]
    public GameObject obstacle;
    public WaterRelated waterrelated;

    [Header("UI 交互提示设置")]
    [Tooltip("拖入包含提示文本的 CanvasGroup")]
    public CanvasGroup promptCanvasGroup;
    public float fadeSpeed = 5f;      // 文本淡入淡出的速度

    // 内部状态
    private bool isPlayerInRange = false;
    private bool isInteracted = false; 

    void Start()
    {
        if (waterrelated == null)
            waterrelated = GetComponent<WaterRelated>();

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
        isInteracted = true; // 锁定输入，防止动画期间再次按下F

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

        opengate();

        StartCoroutine(FadePrompt(0f));
    }


    private void opengate()
    {
        if (obstacle == null)
        {
            if (waterrelated != null)
            {
                waterrelated.isValveOpen = true;
                Debug.Log("机关已触发，水闸开启！");
            }
        }
        else
        {
            Debug.Log("通道被障碍物挡住了！");
        }
    }
}
