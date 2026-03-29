using UnityEngine;

public class BossExitTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("场景的音乐控制器")]
    public SceneMusicController musicController;

    [Header("Settings")]
    [Tooltip("是否只触发一次")]
    public bool triggerOnce = true;

    [Tooltip("触发后是否禁用碰撞体")]
    public bool disableAfterTrigger = true;

    private bool hasTriggered = false;

    private void Start()
    {
        // 如果没有手动指定，自动查找
        if (musicController == null)
        {
            musicController = FindObjectOfType<SceneMusicController>();
        }

        // 确保这是一个触发器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查是否是玩家
        if (!collision.CompareTag("Player")) return;

        // 检查是否已触发
        if (triggerOnce && hasTriggered) return;

        // 切换回普通音乐
        if (musicController != null)
        {
            musicController.PlayNormalMusic();
            hasTriggered = true;

            Debug.Log("Player exited Boss area!");

            // 禁用碰撞体
            if (disableAfterTrigger)
            {
                GetComponent<Collider2D>().enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("SceneMusicController not found!");
        }
    }

    private void OnDrawGizmos()
    {
        // 在编辑器中绘制触发区域
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}