using UnityEngine;
using System.Collections.Generic;

public class WaterBody : MonoBehaviour
{
    [Header("水面边界 (动态更新)")]
    public float waterSurfaceY;
    public float waterBottomY;

    [Header("水体属性")]
    public float buoyancyStrength = 20f;
    public float surfaceFloatHeight = 0.3f;

    [Header("视觉与材质设置")]
    public SpriteRenderer waterRenderer;
    private Material waterMat;

    private float waveSpeed;
    private float waveAmplitude;
    private float waveFrequency;

    private List<Rigidbody2D> objectsInWater = new List<Rigidbody2D>();
    private BoxCollider2D waterCollider;

    // 水位动态升降控制
    public float waterChangeSpeed = 2f;
    private bool isChangingLevel = false;
    private float targetWorldY;

    void Start()
    {
        waterCollider = GetComponent<BoxCollider2D>();
        waterCollider.isTrigger = true;

        // 获取实例化的材质，并读取Shader中的初始数据
        if (waterRenderer != null)
        {
            waterMat = waterRenderer.material;
            SyncWaveProperties();
        }

        UpdateWaterBounds();
    }

    void Update()
    {
        // 如果你在编辑器里经常调节材质波浪参数，取消下一行的注释以便实时预览
        // #if UNITY_EDITOR
        // SyncWaveProperties();
        // #endif
    }

    void FixedUpdate()
    {
        UpdateWaterBounds();

        if (isChangingLevel) HandleWaterLevelChange();

        objectsInWater.RemoveAll(rb => rb == null);

        foreach (Rigidbody2D rb in objectsInWater)
        {
            ApplyBuoyancy(rb);
        }
    }

    /// <summary>
    /// 从 Shader 读取波纹参数
    /// </summary>
    private void SyncWaveProperties()
    {
        if (waterMat != null)
        {
            waveSpeed = waterMat.GetFloat("_WaveSpeed");
            waveAmplitude = waterMat.GetFloat("_WaveAmplitude");
            waveFrequency = waterMat.GetFloat("_WaveFrequency");
        }
    }

    private void UpdateWaterBounds()
    {
        if (waterCollider != null)
        {
            // 基础最高点（不受波浪影响的水平面）
            waterSurfaceY = waterCollider.bounds.max.y;
            waterBottomY = waterCollider.bounds.min.y;
        }
    }

    // ================= 核心：动态波浪水面计算 =================

    /// <summary>
    /// 获取特定 X 坐标下的波浪水面高度
    /// </summary>
    /// <param name="worldXPosition">查询者的世界X坐标</param>
    public float GetWaterSurfaceY(float worldXPosition)
    {
        float baseSurfaceY = waterCollider.bounds.max.y;

        // 如果没有材质或者波幅为0，直接返回平滑水面
        if (waterMat == null || waveAmplitude == 0) return baseSurfaceY;

        // 1. 将世界的X坐标转为水体的本地X坐标（因为Shader用的是 v.vertex.x）
        float localX = transform.InverseTransformPoint(new Vector3(worldXPosition, 0, 0)).x;

        // 2. C# 复刻 Shader 算法: wave = sin((x * freq) + (time * speed)) * amp
        // 注意：Unity Shader 中的 _Time.y 等同于 C# 中的 Time.time
        float waveOffsetLocal = Mathf.Sin((localX * waveFrequency) + (Time.time * waveSpeed)) * waveAmplitude;

        // 3. 将本地的波动高度乘以水体本身的 Y 轴缩放，还原到世界空间的高低变化
        float waveOffsetWorld = waveOffsetLocal * transform.lossyScale.y;

        // 返回基础高度 + 波浪偏移高度
        return baseSurfaceY + waveOffsetWorld;
    }

    // ==========================================================

    private void HandleWaterLevelChange()
    {
        float step = waterChangeSpeed * Time.fixedDeltaTime;
        Vector3 newPos = transform.position;
        newPos.y = Mathf.MoveTowards(transform.position.y, targetWorldY, step);
        transform.position = newPos;

        if (Mathf.Abs(transform.position.y - targetWorldY) < 0.01f)
            isChangingLevel = false;
    }

    public void MoveWaterToY(float targetY, float customSpeed = -1f)
    {
        targetWorldY = targetY;
        if (customSpeed > 0) waterChangeSpeed = customSpeed;
        isChangingLevel = true;
    }

    public void ChangeWaterLevelBy(float amount)
    {
        MoveWaterToY(transform.position.y + amount);
    }

    void ApplyBuoyancy(Rigidbody2D rb)
    {
        if (rb.CompareTag("Player")) return;

        float objectY = rb.transform.position.y;
        float objectX = rb.transform.position.x;

        float dynamicSurfaceY = GetWaterSurfaceY(objectX);
        float submergedDepth = dynamicSurfaceY - objectY;

        if (submergedDepth <= 0) return;

        float submergedRatio = Mathf.Clamp01(submergedDepth / 1f);
        float buoyancy = buoyancyStrength * submergedRatio;
        float gravityCompensation = Mathf.Abs(Physics2D.gravity.y) * rb.mass;
        float totalUpForce = buoyancy + gravityCompensation * submergedRatio;

        if (submergedDepth < surfaceFloatHeight && submergedDepth > 0)
        {
            float dampenFactor = rb.velocity.y * -2f;
            totalUpForce += dampenFactor;
        }

        rb.AddForce(Vector2.up * totalUpForce, ForceMode2D.Force);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null && !objectsInWater.Contains(rb)) objectsInWater.Add(rb);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null) objectsInWater.Remove(rb);
    }
}