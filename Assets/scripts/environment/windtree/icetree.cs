using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 确保挂有 Collider2D (并勾选 Is Trigger) 以便进行雷达扫描范围
[RequireComponent(typeof(Collider2D))]
public class icetree : MonoBehaviour
{
    [Header("生长设置")]
    [SerializeField] private float growtime = 5f;
    public Sprite seed;
    public Sprite tree;
    public Transform[] spawnposition;
    public GameObject ice;

    [Header("视觉过渡设置")]
    [Tooltip("过渡动画持续的时间")]
    public float transitionDuration = 1.5f;

    [Header("水体影响设置")]
    [Tooltip("树长大后，范围内的水面下降多少高度？")]
    public float waterDecreaseAmount = 2f;

    private SpriteRenderer sr;
    private Collider2D triggerCollider;
    private bool hasGrown = false;

    [Header("物理与落地检测")]
    public LayerMask whatisground;
    public float groundCheckDistance;
    public Rigidbody2D rb;

    public virtual bool isgrounddetected() => Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatisground);

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        triggerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        sr.sprite = seed;
        sr.color = Color.white;
    }

    void Update()
    {
        // 处理未落地时的下落表现
        if (!isgrounddetected())
        {
            rb.velocity = new Vector2(0, -1);
        }
        else
        {
            rb.velocity = new Vector2(0, 0);
        }

        if (hasGrown) return;

        // 仅在冬季 (Covered 状态) 且已落地时才进行生长倒计时
        if (ScreenCoverTransition2D.instance != null)
        {
            if (ScreenCoverTransition2D.instance.currentState == ScreenCoverTransition2D.State.Covered)
            {
                growtime -= Time.deltaTime;
            }
        }

        // 倒计时结束且在地面上，触发长大
        if (growtime <= 0 && isgrounddetected())
        {
            GrowUp();
        }
    }

    private void GrowUp()
    {
        hasGrown = true;
        sr.sprite = tree;

        // 1. 播放颜色渐变特效
        StartCoroutine(ColorTransitionEffect());

        // 2. 生成冰块
        spawn();

        // 3. 【新增】检测周围水体并使其水位下降
        DecreaseSurroundingWater();
    }

    private IEnumerator ColorTransitionEffect()
    {
        float timer = 0f;

        Color startColor = new Color(2f, 2f, 2f, 0f); // 高亮透明开始
        Color endColor = Color.white;                 // 恢复原本颜色

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionDuration;

            sr.color = Color.Lerp(startColor, endColor, progress);

            yield return null;
        }

        sr.color = endColor;
    }

    public void spawn()
    {
        if (spawnposition == null || ice == null) return;

        for (int i = 0; i < spawnposition.Length; i++)
        {
            // 建议：如果你不希望树被破坏时冰块跟着消失，最好不要把冰块设为树的子物体。
            // 使用 Transform.position 代替直接传入 Transform 作为父节点
            Instantiate(ice, spawnposition[i].position, Quaternion.identity);
        }
    }

    /// <summary>
    /// 【新增】扫描触发器内的所有水体，并让其水位下降
    /// </summary>
    private void DecreaseSurroundingWater()
    {
        if (triggerCollider == null) return;

        // 像雷达一样扫描触发器范围内的所有物体
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();

        int count = triggerCollider.OverlapCollider(filter, results);

        for (int i = 0; i < count; i++)
        {
            // 检查扫描到的物体身上是否有 WaterBody 脚本
            WaterBody water = results[i].GetComponent<WaterBody>();
            if (water != null)
            {
                // 调用 WaterBody 内部写好的平滑水位变化逻辑
                // 因为是下降，所以传入的是负数 (-waterDecreaseAmount)
                water.ChangeWaterLevelBy(-waterDecreaseAmount);
                Debug.Log($"冰树长成！吸取水分，已让 {results[i].name} 的水位下降了 {waterDecreaseAmount} 米。");
            }
        }
    }
}