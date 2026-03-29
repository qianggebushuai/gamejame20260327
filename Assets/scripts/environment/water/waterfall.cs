using UnityEngine;

public class Waterfall : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject waterDropPrefab;           // 拖入你的水珠预制体
    public float spawnRatePerSecond = 50f;        // 每秒生成多少滴
    public float spawnAreaWidth = 4f;            // 瀑布宽度（左右范围）
    [Tooltip("水珠出生时的初始速度（往下，稍微增加速度能让效果更真实）")]
    public float initialVerticalSpeed = -10f;

    private float spawnTimer = 0f;

    void Update()
    {
        spawnTimer += Time.deltaTime;
        float interval = 1f / spawnRatePerSecond;

        while (spawnTimer >= interval)
        {
            SpawnDrop();
            spawnTimer -= interval;
        }
    }

    private void SpawnDrop()
    {
        if (waterDropPrefab == null) return;

        float randomX = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(randomX, 0);

        GameObject drop = Instantiate(waterDropPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(0, initialVerticalSpeed);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position + new Vector3(-spawnAreaWidth / 2f, 0, 0),
                        transform.position + new Vector3(spawnAreaWidth / 2f, 0, 0));
    }
}