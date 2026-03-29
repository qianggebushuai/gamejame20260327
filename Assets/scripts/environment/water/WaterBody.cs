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
    public bool cancomsumeo2whenswim = true;

    [Header("水位升降与限制")]
    public float waterChangeSpeed = 2f;
    [Tooltip("水体 Transform.y 允许的最高世界坐标")]
    public float maxYPosition = 10f;
    [Tooltip("水体 Transform.y 允许的最低世界坐标")]
    public float minYPosition = -10f;

    [Header("视觉与材质设置")]
    public SpriteRenderer waterRenderer;
    private Material waterMat;

    private float waveSpeed;
    private float waveAmplitude;
    private float waveFrequency;

    private List<Rigidbody2D> objectsInWater = new List<Rigidbody2D>();
    private BoxCollider2D waterCollider;

    // 内部运行状态
    private bool isChangingLevel = false;
    private float targetWorldY;

    void Start()
    {
        waterCollider = GetComponent<BoxCollider2D>();
        waterCollider.isTrigger = true;

        if (waterRenderer != null)
        {
            waterMat = waterRenderer.material;
            SyncWaveProperties();
        }

        UpdateWaterBounds();
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
            if (waterMat.HasFloat("_WaveSpeed") &&
                waterMat.HasFloat("_WaveAmplitude") &&
                waterMat.HasFloat("_WaveFrequency"))
            {
                if (waterMat.GetFloat("_WaveSpeed") != 0f)
                {
                    waveSpeed = waterMat.GetFloat("_WaveSpeed");
                    waveAmplitude = waterMat.GetFloat("_WaveAmplitude");
                    waveFrequency = waterMat.GetFloat("_WaveFrequency");
                }
            }
            else
            {
                Debug.LogWarning($"WaterBody: Material '{waterMat.name}' 的 Shader 缺少波浪属性，跳过同步。");
            }
        }
    }

    private void UpdateWaterBounds()
    {
        if (waterCollider != null)
        {
            waterSurfaceY = waterCollider.bounds.max.y;
            waterBottomY = waterCollider.bounds.min.y;
        }
    }

    // ================= 核心：动态波浪水面计算 =================

    public float GetWaterSurfaceY(float worldXPosition)
    {
        float baseSurfaceY = waterCollider != null ? waterCollider.bounds.max.y : transform.position.y;

        if (waterMat == null || waveAmplitude == 0) return baseSurfaceY;

        float localX = transform.InverseTransformPoint(new Vector3(worldXPosition, 0, 0)).x;
        float waveOffsetLocal = Mathf.Sin((localX * waveFrequency) + (Time.time * waveSpeed)) * waveAmplitude;
        float waveOffsetWorld = waveOffsetLocal * transform.lossyScale.y;

        return baseSurfaceY + waveOffsetWorld;
    }

    // ================= 水位升降与限制逻辑 =================

    public void MoveWaterToY(float targetY, float customSpeed = -1f)
    {
        // 【核心修改】：在设定目标时，使用 Mathf.Clamp 强制将目标限制在 min 和 max 之间
        // 这样无论是接收机关信号、瀑布水滴还是持续按键，水面永远不会超出你规定的界限
        targetWorldY = Mathf.Clamp(targetY, minYPosition, maxYPosition);

        if (customSpeed > 0) waterChangeSpeed = customSpeed;

        isChangingLevel = true;
    }

    public void ChangeWaterLevelBy(float amount)
    {
        MoveWaterToY(transform.position.y + amount);
    }

    private void HandleWaterLevelChange()
    {
        float step = waterChangeSpeed * Time.fixedDeltaTime;
        Vector3 newPos = transform.position;

        // 平滑移动向被 Clamp 过的安全目标值
        newPos.y = Mathf.MoveTowards(transform.position.y, targetWorldY, step);

        // 双重保险：确保实际坐标也绝对不会越界
        newPos.y = Mathf.Clamp(newPos.y, minYPosition, maxYPosition);

        transform.position = newPos;

        // 因为目标值已经是合法的，所以只需判断是否到达目标即可
        if (Mathf.Abs(transform.position.y - targetWorldY) < 0.01f)
        {
            isChangingLevel = false;
        }
    }

    // ================= 物理浮力逻辑 =================

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

    // ================= 编辑器辅助可视化 =================

    /// <summary>
    /// 当在 Unity 编辑器中选中该水体时，画出高度限制的辅助线
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 画出允许的最高位置 (红线)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(transform.position.x - 5f, maxYPosition, 0), new Vector3(transform.position.x + 5f, maxYPosition, 0));

        // 画出允许的最低位置 (蓝线)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(transform.position.x - 5f, minYPosition, 0), new Vector3(transform.position.x + 5f, minYPosition, 0));
    }
}