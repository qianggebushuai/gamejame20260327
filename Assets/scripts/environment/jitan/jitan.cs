using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class jitan : MonoBehaviour
{
    [Header("祭坛状态")]
    [Tooltip("该祭坛当前是否已被成功激活/打开")]
    public bool isOpen = false;

    [Header("解谜联动设置")]
    [Tooltip("必须先开启数组里的所有祭坛，本祭坛才能交互成功。如果为空，则可直接交互。")]
    public jitan[] linkedAltars;

    [Header("物品生成设置")]
    [Tooltip("要弹出的物品预制体 (物品必须带 Rigidbody2D)")]
    public GameObject itemPrefab;
    [Tooltip("物品从哪个点弹出来？(建议在祭坛顶部放一个空物体)")]
    public Transform spawnPoint;
    [Tooltip("向上弹出的力度")]
    public float popForce = 5f;

    [Tooltip("最多可以成功交互/吐出物品的次数")]
    public int maxUses = 1;
    private int currentUses = 0;

    [Header("UI 交互提示设置")]
    public CanvasGroup promptCanvasGroup;
    public float fadeSpeed = 5f;

    [Header("视觉表现 (可选)")]
    public SpriteRenderer altarSpriteRenderer;
    public Sprite activeSprite; // 激活后改变贴图，让玩家知道它亮了

    // 内部状态
    private bool isPlayerInRange = false;
    private bool isInteracting = false;

    void Start()
    {
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }

        if (altarSpriteRenderer == null)
            altarSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 玩家在范围内 + 没在播放动画 + 还有剩余次数 + 按下F键
        if (isPlayerInRange && !isInteracting && currentUses < maxUses && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(HandleInteractionRoutine());
        }
    }

    // ================= 核心联动与生成逻辑 =================

    private IEnumerator HandleInteractionRoutine()
    {
        isInteracting = true;

        // 1. 播放 UI 按压反馈特效
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

        // 2. 【核心判断】：检查前置祭坛是否全都开启了
        if (!CheckLinkedAltars())
        {
            Debug.Log("交互失败！有前置祭坛尚未开启，没有发生任何事。");
            isInteracting = false;
            yield break; // 核心：条件不满足，直接退出协程，不执行下面的吐物品逻辑
        }

        // 3. 条件满足，执行成功逻辑
        InteractSuccess();

        // 4. 如果次数用光了，隐藏提示框
        if (currentUses >= maxUses)
        {
            StartCoroutine(FadePrompt(0f));
        }

        isInteracting = false;
    }

    /// <summary>
    /// 检查数组中所有的关联祭坛是否都已经 isOpen = true
    /// </summary>
    private bool CheckLinkedAltars()
    {
        // 如果数组为空，说明这是一个基础祭坛，不需要前置条件
        if (linkedAltars == null || linkedAltars.Length == 0) return true;

        // 遍历检查
        foreach (jitan altar in linkedAltars)
        {
            // 只要发现哪怕有一个没开启，就返回 false
            if (altar != null && altar.isOpen == false)
            {
                return false;
            }
        }
        return true; // 全部开启了
    }

    /// <summary>
    /// 交互成功：改变状态，弹射物品
    /// </summary>
    private void InteractSuccess()
    {
        // 标记自身为开启状态（这样别人检查我的时候就能通过了）
        isOpen = true;
        currentUses++;

        // 可选：改变贴图，表示祭坛被点亮了
        if (altarSpriteRenderer != null && activeSprite != null)
        {
            altarSpriteRenderer.sprite = activeSprite;
        }

        // 弹射生成物品
        if (itemPrefab != null && spawnPoint != null)
        {
            // 生成物品
            GameObject spawnedItem = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);

            // 获取刚体，给它一个爆发力
            Rigidbody2D rb = spawnedItem.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 生成一个主要向上，稍微带点左右随机偏移的方向，这样物品掉落显得自然
                float randomX = Random.Range(-0.5f, 0.5f);
                Vector2 popDirection = new Vector2(randomX, 1f).normalized;

                // AddForce 使用 Impulse (瞬间爆发力) 来模拟“砰”地弹出来
                rb.AddForce(popDirection * popForce, ForceMode2D.Impulse);
            }
            else
            {
                Debug.LogWarning("生成的物品没有 Rigidbody2D 组件，无法被弹出！");
            }
        }

        Debug.Log($"祭坛交互成功！(使用次数: {currentUses}/{maxUses})");
    }

    // ================= 触发器检测逻辑 =================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && currentUses < maxUses)
        {
            isPlayerInRange = true;
            StopCoroutine("FadePrompt");
            StartCoroutine(FadePrompt(1f));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            StopCoroutine("FadePrompt");
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
}