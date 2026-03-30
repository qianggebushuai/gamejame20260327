using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("重置设置")]
    [SerializeField] private string resetSceneName = "topic";

    [Header("重生设置")]
    public Dictionary<string, Vector3> sceneRespawnPoints = new Dictionary<string, Vector3>();

    [Header("玩家引用")]
    private GameObject player;
    private Player1 playerController;

    [Header("游戏状态")]
    public int lives = 3;
    public bool isGameOver = false;

    void Awake()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // 如果是重置场景
        if (currentScene == resetSceneName)
        {
            if (instance != null && instance != this)
            {
                Debug.Log($"[GameManager] 进入 {resetSceneName}，销毁旧 GameManager");

                SceneManager.sceneLoaded -= instance.OnSceneLoaded;

                DestroyImmediate(instance.gameObject);
                instance = null;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        FindPlayer();
        SaveCurrentSceneRespawnPoint();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // 只有当销毁的是当前实例时才取消订阅
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] 场景加载: {scene.name}");

        // 检查是否进入重置场景
        if (scene.name == resetSceneName)
        {
            ResetGameState();
        }

        FindPlayer();
        SaveCurrentSceneRespawnPoint();
    }

    /// <summary>
    /// 重置游戏状态
    /// </summary>
    private void ResetGameState()
    {
        lives = 3;
        isGameOver = false;
        sceneRespawnPoints.Clear();
        Debug.Log("[GameManager] 游戏状态已重置");
    }

    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<Player1>();
        }
    }

    private void SaveCurrentSceneRespawnPoint()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneRespawnPoints.ContainsKey(sceneName))
        {
            Debug.Log($"场景 [{sceneName}] 已有重生点，保持不变");
            return;
        }

        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");

        if (spawnPoint != null)
        {
            sceneRespawnPoints[sceneName] = spawnPoint.transform.position;
            Debug.Log($"场景 [{sceneName}] 重生点已保存：{spawnPoint.transform.position}");
        }
        else if (player != null)
        {
            sceneRespawnPoints[sceneName] = player.transform.position;
            Debug.Log($"场景 [{sceneName}] 使用玩家位置作为重生点：{player.transform.position}");
        }
        else
        {
            Debug.LogWarning($"场景 [{sceneName}] 找不到重生点和玩家！");
        }
    }

    private Vector3 GetCurrentRespawnPoint()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // 从字典中获取
        if (sceneRespawnPoints.ContainsKey(sceneName))
        {
            return sceneRespawnPoints[sceneName];
        }

        // 字典中没有，尝试查找
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        if (spawnPoint != null)
        {
            return spawnPoint.transform.position;
        }

        Debug.LogWarning($"场景 [{sceneName}] 没有重生点！返回零点");
        return Vector3.zero;
    }

    public void PlayerDie()
    {
        if (isGameOver) return;
        StartCoroutine(RespawnCoroutine());
    }
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("damage");
        RespawnPlayer();
        Debug.Log("玩家已重生，计时器已重置！");
    }

    public void RespawnPlayer()
    {
        if (player == null)
        {
            FindPlayer();
        }

        if (player != null)
        {
            // 获取当前场景的重生点
            Vector3 respawnPos = GetCurrentRespawnPoint();
            player.transform.position = respawnPos;

            // 重置速度
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            Debug.Log($"玩家已重生到：{respawnPos}");
        }
        else
        {
            Debug.LogWarning("找不到玩家！");
        }
    }

    /// <summary>
    /// 设置当前场景的重生点（存档点）
    /// </summary>
    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        sceneRespawnPoints[sceneName] = newRespawnPoint.position;
        Debug.Log($"场景 [{sceneName}] 重生点已更新：{newRespawnPoint.position}");
    }

    public void SetRespawnPoint(Vector3 position)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        sceneRespawnPoints[sceneName] = position;
        Debug.Log($"场景 [{sceneName}] 重生点已更新：{position}");
    }


    public void ResetTimer()
    {
        if (timecontroller.instance != null)
        {
            timecontroller.instance.ResetCountdown();
            timecontroller.instance.StartCountdown();
            Debug.Log("计时器已重置并开始！");
        }
        else
        {
            Debug.LogWarning("找不到 timecontroller 实例！");
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        timecontroller.instance?.StopCountdown();
        Debug.Log("游戏结束！");
    }

    public void RestartGame()
    {
        isGameOver = false;

        sceneRespawnPoints.Clear();
        lives = 3;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ResetTimer();
    }

    public void ClearSceneRespawnPoint(string sceneName)
    {
        if (sceneRespawnPoints.ContainsKey(sceneName))
        {
            sceneRespawnPoints.Remove(sceneName);
            Debug.Log($"场景 [{sceneName}] 重生点已清除");
        }
    }

    public void ClearAllRespawnPoints()
    {
        sceneRespawnPoints.Clear();
        Debug.Log("所有重生点已清除");
    }
}