using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obstacle : MonoBehaviour
{
    [Header("机关逻辑设置")]
    public GameObject explosionprefab;
    [Header("UI 交互提示设置")]
    [Tooltip("拖入包含提示文本的 CanvasGroup")]
    public CanvasGroup promptCanvasGroup;
    public float fadeSpeed = 5f;     

    private bool isPlayerInRange = false;
    private bool isInteracted = false;
    [Header("爆炸")]
    [SerializeField] private float minSpeedToSpawn = 5f;
    [SerializeField] private float spawnForce = 10f;
    [SerializeField] private int spawnCount = 3;
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
        // 确保你的玩家标签是 "Player"
        if (collision.CompareTag("Player") && !isInteracted)
        {
            isPlayerInRange = true;
            StopAllCoroutines(); // 停止可能的淡出动画
            StartCoroutine(FadePrompt(1f)); // 淡入显示文本 (Alpha -> 1)
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // 如果你想让机关可以反复开关，取消下面这行的注释
            // isInteracted = false; 

            StopAllCoroutines();
            StartCoroutine(FadePrompt(0f)); // 淡出隐藏文本 (Alpha -> 0)
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

        StartCoroutine (SpawnDebris());
        StartCoroutine(FadePrompt(0f));
    }
    private IEnumerator SpawnDebris()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject newIce = Instantiate(explosionprefab, transform.position, Quaternion.identity);

            Rigidbody2D iceRb = newIce.GetComponent<Rigidbody2D>();

            if (iceRb != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;

                iceRb.AddForce(randomDirection * Random.Range(0, spawnForce), ForceMode2D.Impulse);
            }
        }
        Destroy(gameObject);
        yield return null;

    }
}
