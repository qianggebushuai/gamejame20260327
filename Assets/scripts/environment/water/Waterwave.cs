using UnityEngine;

public class PhysicsWaterWave : MonoBehaviour
{
    [Header("波浪设置")]
    public int wavePointCount = 20;
    public float waveWidth = 10f;
    public float springConstant = 0.02f;
    public float damping = 0.04f;
    public float spread = 0.05f;

    private EdgeCollider2D edgeCollider;
    private LineRenderer lineRenderer;

    private float[] velocities;
    private float[] accelerations;
    private float[] leftDeltas;
    private float[] rightDeltas;
    private Vector2[] wavePoints;
    private float baseHeight;

    void Start()
    {
        baseHeight = transform.position.y;

        SetupCollider();
        SetupLineRenderer();
        InitializeWavePoints();
    }

    void SetupCollider()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
        {
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        }
        edgeCollider.isTrigger = true;
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = wavePointCount;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    void InitializeWavePoints()
    {
        wavePoints = new Vector2[wavePointCount];
        velocities = new float[wavePointCount];
        accelerations = new float[wavePointCount];
        leftDeltas = new float[wavePointCount];
        rightDeltas = new float[wavePointCount];

        for (int i = 0; i < wavePointCount; i++)
        {
            float x = (i / (float)(wavePointCount - 1) - 0.5f) * waveWidth;
            wavePoints[i] = new Vector2(x, baseHeight);
            velocities[i] = 0f;
            accelerations[i] = 0f;
        }

        UpdateColliderAndRenderer();
    }

    void FixedUpdate()
    {
        UpdateWaves();
        UpdateColliderAndRenderer();
    }

    void UpdateWaves()
    {
        // 计算加速度和速度
        for (int i = 0; i < wavePointCount; i++)
        {
            float force = springConstant * (baseHeight - wavePoints[i].y);
            accelerations[i] = force;

            velocities[i] += accelerations[i];
            velocities[i] *= (1f - damping);

            wavePoints[i].y += velocities[i];
        }

        // 波浪传播
        for (int j = 0; j < 8; j++) // 多次迭代使波浪更平滑
        {
            for (int i = 0; i < wavePointCount; i++)
            {
                if (i > 0)
                {
                    leftDeltas[i] = spread * (wavePoints[i].y - wavePoints[i - 1].y);
                    velocities[i - 1] += leftDeltas[i];
                }
                if (i < wavePointCount - 1)
                {
                    rightDeltas[i] = spread * (wavePoints[i].y - wavePoints[i + 1].y);
                    velocities[i + 1] += rightDeltas[i];
                }
            }
        }
    }

    void UpdateColliderAndRenderer()
    {
        Vector3[] positions = new Vector3[wavePointCount];

        for (int i = 0; i < wavePointCount; i++)
        {
            positions[i] = new Vector3(
                transform.position.x + wavePoints[i].x,
                wavePoints[i].y,
                0f
            );
        }

        lineRenderer.SetPositions(positions);
        edgeCollider.points = wavePoints;
    }

    // 触发波纹
    public void Splash(float xPosition, float force)
    {
        int index = Mathf.RoundToInt((xPosition / waveWidth + 0.5f) * (wavePointCount - 1));
        index = Mathf.Clamp(index, 0, wavePointCount - 1);

        velocities[index] = force;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float force = Mathf.Clamp(rb.velocity.magnitude, -5f, 5f);
                Splash(collision.transform.position.x - transform.position.x, force);
            }
        }
    }
}