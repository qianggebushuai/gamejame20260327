using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windarea : MonoBehaviour
{
    [Header("风场设置")]
    public winddir direction = winddir.right;
    public float windForce = 10f;

    [Header("粒子特效设置")]
    [SerializeField] private GameObject particlePrefab; // 粒子预制体
    [SerializeField] private float spawnInterval = 0.2f; // 生成间隔
    [SerializeField] private int particlesPerSpawn = 3; // 每次生成数量
    [SerializeField] private float particleLifetime = 2f; // 粒子存活时间
    [SerializeField] private float particleSpeed = 5f; // 粒子移动速度
    [SerializeField] private float particleSpeedVariation = 1f; // 速度随机变化范围
    [SerializeField] private bool enableParticles = true; // 是否启用粒子

    public enum winddir { up, down, left, right };

    private List<Player1> playersInArea = new List<Player1>();
    private Dictionary<Player1, float> originalGravityDict = new Dictionary<Player1, float>();

    private Collider2D windCollider;
    private float spawnTimer;

    void Start()
    {
        windCollider = GetComponent<Collider2D>();
        if (windCollider != null)
        {
            windCollider.isTrigger = true;
        }
    }

    void Update()
    {
        // 粒子生成逻辑
        if (enableParticles && particlePrefab != null && windCollider != null)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnParticles();
                spawnTimer = spawnInterval;
            }
        }
    }

    void FixedUpdate()
    {
        playersInArea.RemoveAll(p => p == null);

        foreach (Player1 player in playersInArea)
        {
            ApplyWind(player);
        }
    }

    #region 粒子系统

    private void SpawnParticles()
    {
        for (int i = 0; i < particlesPerSpawn; i++)
        {
            Vector2 spawnPos = GetRandomPositionInCollider();
            GameObject particle = Instantiate(particlePrefab, spawnPos, Quaternion.identity);

            // 添加粒子移动组件
            WindParticle wp = particle.AddComponent<WindParticle>();
            wp.Initialize(GetWindDirection(), particleSpeed, particleSpeedVariation, particleLifetime);
        }
    }

    private Vector2 GetRandomPositionInCollider()
    {
        Bounds bounds = windCollider.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector2(x, y);
    }

    #endregion

    #region 风场逻辑

    private Vector2 GetWindDirection()
    {
        switch (direction)
        {
            case winddir.up: return Vector2.up;
            case winddir.down: return Vector2.down;
            case winddir.left: return Vector2.left;
            case winddir.right: return Vector2.right;
            default: return Vector2.zero;
        }
    }

    private void ApplyWind(Player1 player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (direction == winddir.left || direction == winddir.right)
            {
                Vector2 windVelocity = GetWindDirection() * windForce;
                rb.AddForce(windVelocity, ForceMode2D.Force);
            }
            else
            {
                float verticalVelocity = GetWindDirection().y * windForce;
                rb.velocity = new Vector2(rb.velocity.x, verticalVelocity);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null && !playersInArea.Contains(player))
        {
            playersInArea.Add(player);

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (direction == winddir.left || direction == winddir.right)
                {
                    originalGravityDict[player] = rb.gravityScale;
                    rb.gravityScale = 1f;
                    rb.velocity = new Vector2(rb.velocity.x / 100f, 0f);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null && playersInArea.Contains(player))
        {
            playersInArea.Remove(player);

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (originalGravityDict.ContainsKey(player))
                {
                    rb.gravityScale = 8;
                    originalGravityDict.Remove(player);
                }
            }
        }
    }

    #endregion
}

/// <summary>
/// 风场粒子移动组件
/// </summary>
public class WindParticle : MonoBehaviour
{
    private Vector2 moveDirection;
    private float speed;
    private float lifetime;
    private SpriteRenderer spriteRenderer;
    private float initialAlpha;
    private float timer;

    public void Initialize(Vector2 direction, float baseSpeed, float speedVariation, float life)
    {
        moveDirection = direction;
        speed = baseSpeed + Random.Range(-speedVariation, speedVariation);
        lifetime = life;
        timer = lifetime;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            initialAlpha = spriteRenderer.color.a;
        }

        // 随机初始旋转
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        // 随机大小变化
        float scale = Random.Range(0.8f, 1.2f);
        transform.localScale *= scale;
    }

    void Update()
    {
        // 移动
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // 轻微摆动
        float wobble = Mathf.Sin(Time.time * 5f + transform.position.x) * 0.5f;
        Vector2 wobbleDir = new Vector2(-moveDirection.y, moveDirection.x); // 垂直于风向
        transform.Translate(wobbleDir * wobble * Time.deltaTime, Space.World);

        // 旋转
        transform.Rotate(0, 0, speed * 20f * Time.deltaTime);

        // 淡出效果
        timer -= Time.deltaTime;
        if (spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(0f, initialAlpha, timer / lifetime);
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }

        // 销毁
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}