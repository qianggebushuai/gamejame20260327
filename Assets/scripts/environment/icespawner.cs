using UnityEngine;

public class WaterRaycastSpawner : MonoBehaviour
{
    [Header("射线检测设置")]
    [Tooltip("射线向下的检测长度")]
    public float rayDistance = 10f;
    [Tooltip("指定水体所在的图层，防止射线被玩家或机关自己挡住")]
    public LayerMask waterLayer;

    [Header("生成设置")]
    [Tooltip("要生成物品的各个位置节点")]
    public Transform[] spawnPoints;
    [Tooltip("要生成的所有预制体/物品")]
    public GameObject[] prefabsToSpawn;

    // 安全锁：防止一帧内或连续帧疯狂无限生成
    private bool hasSpawned = false;

    void Update()
    {
        // 如果还没有触发过生成，就持续向下发射射线检测
        if (!hasSpawned)
        {
            DetectWaterAndSpawn();
        }
    }

    /// <summary>
    /// 向下发射射线，如果碰到WaterBody则触发生成逻辑
    /// </summary>
    private void DetectWaterAndSpawn()
    {
        // 发射2D射线：起点为物体当前位置，方向向下，长度为rayDistance，仅检测waterLayer图层
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, waterLayer);

        // 如果射线打到了物体
        if (hit.collider != null)
        {
            // 检查打到的物体身上有没有 WaterBody 脚本
            WaterBody water = hit.collider.GetComponent<WaterBody>();

            if (water != null)
            {
                Debug.Log("射线检测到了水体！开始生成物品...");
                SpawnAllItems();

                // 锁死开关，这辈子只生成一次 (如果需要重复触发，可以写一个重置函数把这个设为false)
                hasSpawned = true;
            }
        }
    }

    /// <summary>
    /// 核心生成逻辑：遍历每一个生成点，并在该点生成所有的物品
    /// </summary>
    private void SpawnAllItems()
    {
        // 安全校验：如果数组为空直接返回，防止报错
        if (spawnPoints == null || prefabsToSpawn == null) return;
        if (spawnPoints.Length == 0 || prefabsToSpawn.Length == 0) return;

        // 外层循环：遍历每一个 Transform 位置
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue; // 防止数组里有空位

            // 内层循环：在这个位置，生成所有的 GameObject
            foreach (GameObject prefab in prefabsToSpawn)
            {
                if (prefab == null) continue; // 防止数组里有空预制体

                // 在指定位置生成物品，并且保持预制体原本的旋转角度
                Instantiate(prefab, spawnPoint.position, prefab.transform.rotation);
            }
        }
    }

    /// <summary>
    /// (可选) 重置生成状态，如果你希望它下一次碰到水还能再生成一遍，就调用这个函数
    /// </summary>
    public void ResetSpawner()
    {
        hasSpawned = false;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayDistance);
    }
}