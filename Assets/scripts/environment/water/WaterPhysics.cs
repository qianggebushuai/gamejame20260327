using UnityEngine;

public class WaterPhysics : MonoBehaviour
{
    [Header("浮力设置")]
    public float surfaceBuoyancy = 15f; // 水面浮力
    public float underwaterBuoyancy = 5f; // 水下浮力
    public float waterDrag = 3f; // 水中阻力
    public float waterAngularDrag = 1f; // 水中角阻力

    [Header("潜水设置")]
    public KeyCode diveKey = KeyCode.LeftShift; // 下潜键
    public float diveForce = 10f; // 下潜力度
    public float maxDiveSpeed = 5f; // 最大下潜速度

    private Rigidbody2D rb;
    private float normalDrag;
    private float normalAngularDrag;
    private bool isInWater = false;
    private WaterBody currentWater;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        normalDrag = rb.drag;
        normalAngularDrag = rb.angularDrag;
    }

    void FixedUpdate()
    {
        if (isInWater && currentWater != null)
        {
            ApplyBuoyancy();
            HandleDiving();
        }
    }

    void ApplyBuoyancy()
    {
        float waterSurface = currentWater.GetWaterSurfaceY();
        float playerY = transform.position.y;

        // 计算玩家相对水面的位置
        float distanceToSurface = waterSurface - playerY;

        if (distanceToSurface > 0) // 玩家在水下
        {
            // 在水面附近浮力更强
            float buoyancy = distanceToSurface < 0.5f ? surfaceBuoyancy : underwaterBuoyancy;

            // 应用浮力
            rb.AddForce(Vector2.up * buoyancy, ForceMode2D.Force);
        }
    }

    void HandleDiving()
    {
        if (Input.GetKey(diveKey))
        {
            // 按住 Shift 下潜
            float diveVelocity = Mathf.Max(rb.velocity.y, -maxDiveSpeed);
            rb.AddForce(Vector2.down * diveForce, ForceMode2D.Force);
            rb.velocity = new Vector2(rb.velocity.x, diveVelocity);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        WaterBody water = collision.GetComponent<WaterBody>();
        if (water != null)
        {
            EnterWater(water);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        WaterBody water = collision.GetComponent<WaterBody>();
        if (water != null)
        {
            ExitWater();
        }
    }

    void EnterWater(WaterBody water)
    {
        isInWater = true;
        currentWater = water;

        // 设置水中物理参数
        rb.drag = waterDrag;
        rb.angularDrag = waterAngularDrag;
        rb.gravityScale = 0.3f; // 降低重力

        Debug.Log("进入水中");
    }

    void ExitWater()
    {
        isInWater = false;
        currentWater = null;

        // 恢复正常物理参数
        rb.drag = normalDrag;
        rb.angularDrag = normalAngularDrag;
        rb.gravityScale = 1f;

        Debug.Log("离开水中");
    }

    public bool IsInWater()
    {
        return isInWater;
    }

    public bool IsUnderwater()
    {
        if (!isInWater || currentWater == null) return false;
        return transform.position.y < currentWater.GetWaterSurfaceY();
    }
}