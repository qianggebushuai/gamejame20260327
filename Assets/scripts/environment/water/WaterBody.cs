using UnityEngine;
using System.Collections.Generic;

public class WaterBody : MonoBehaviour
{
    [Header("水体设置")]
    public float waterSurfaceY; // 水面高度
    public float waterBottomY; // 水底高度

    [Header("水体属性")]
    public float buoyancyStrength = 20f; // 浮力强度
    public float waterDrag = 3f; // 水中阻力
    public float surfaceFloatHeight = 0.3f; // 漂浮时露出水面的高度

    [Header("视觉设置")]
    public SpriteRenderer waterRenderer;
    public Color waterColor = new Color(0.2f, 0.5f, 1f, 0.5f);

    // 当前在水中的物体
    private List<Rigidbody2D> objectsInWater = new List<Rigidbody2D>();

    // 碰撞器引用
    private BoxCollider2D waterCollider;

    void Start()
    {
        waterCollider = GetComponent<BoxCollider2D>();
        waterCollider.isTrigger = true;

        // 初始化水面和水底位置
        UpdateWaterBounds();

        if (waterRenderer != null)
        {
            waterRenderer.color = waterColor;
        }
    }

    void FixedUpdate()
    {
        // 对所有在水中的物体应用浮力
        foreach (Rigidbody2D rb in objectsInWater)
        {
            if (rb != null)
            {
                ApplyBuoyancy(rb);
            }
        }
    }

    /// <summary>
    /// 更新水体边界
    /// </summary>
    void UpdateWaterBounds()
    {
        if (waterCollider != null)
        {
            Vector2 worldCenter = (Vector2)transform.position + waterCollider.offset;
            waterSurfaceY = worldCenter.y + waterCollider.size.y / 2f;
            waterBottomY = worldCenter.y - waterCollider.size.y / 2f;
        }
    }

    /// <summary>
    /// 应用浮力
    /// </summary>
    void ApplyBuoyancy(Rigidbody2D rb)
    {
        float objectY = rb.transform.position.y;
        float submergedDepth = waterSurfaceY - objectY;

        // 完全在水面以上
        if (submergedDepth <= 0) return;

        // 计算浸没比例 (0-1)
        float submergedRatio = Mathf.Clamp01(submergedDepth / 1f);

        // 浮力 = 浮力强度 * 浸没比例
        float buoyancy = buoyancyStrength * submergedRatio;

        // 抵消重力
        float gravityCompensation = Mathf.Abs(Physics2D.gravity.y) * rb.mass;

        // 总向上的力
        float totalUpForce = buoyancy + gravityCompensation * submergedRatio;

        // 在水面附近时，增加稳定力
        if (submergedDepth < surfaceFloatHeight && submergedDepth > 0)
        {
            // 让玩家稳定在水面
            float stabilizeForce = (surfaceFloatHeight - submergedDepth) * 20f;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Lerp(rb.velocity.y, 0, Time.fixedDeltaTime * 5f));
        }

        rb.AddForce(Vector2.up * totalUpForce, ForceMode2D.Force);
    }

    /// <summary>
    /// 设置水位高度
    /// </summary>
    public void SetWaterLevel(float newSurfaceY)
    {
        float heightDifference = newSurfaceY - waterSurfaceY;

        // 调整碰撞器
        Vector2 newSize = waterCollider.size;
        newSize.y += heightDifference;
        waterCollider.size = newSize;

        Vector2 newOffset = waterCollider.offset;
        newOffset.y += heightDifference / 2f;
        waterCollider.offset = newOffset;

        // 更新水面渲染（如果有）
        if (waterRenderer != null)
        {
            Vector3 scale = waterRenderer.transform.localScale;
            scale.y += heightDifference;
            waterRenderer.transform.localScale = scale;
        }

        UpdateWaterBounds();
    }

    /// <summary>
    /// 添加水量
    /// </summary>
    public void AddWater(float amount)
    {
        SetWaterLevel(waterSurfaceY + amount);
    }

    /// <summary>
    /// 移除水量
    /// </summary>
    public void RemoveWater(float amount)
    {
        SetWaterLevel(waterSurfaceY - amount);
    }

    public float GetWaterSurfaceY()
    {
        return waterSurfaceY;
    }

    public float GetWaterBottomY()
    {
        return waterBottomY;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null && !objectsInWater.Contains(rb))
        {
            objectsInWater.Add(rb);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            objectsInWater.Remove(rb);
        }
    }
}