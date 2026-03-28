using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterPipe : MonoBehaviour
{
    [Header("连接的水体")]
    public WaterBody leftWater;
    public WaterBody rightWater;

    [Header("管道范围设置")]
    [Tooltip("用一个BoxCollider2D来定义管道的范围（无需开启物理碰撞）")]
    public BoxCollider2D pipeBounds;

    [Header("视觉设置")]
    public Material waterMaterial;
    public float uvScrollSpeed = 0.5f; 

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh waterMesh;
    private float scrollOffset;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (waterMaterial != null) meshRenderer.material = waterMaterial;

        waterMesh = new Mesh();
        waterMesh.MarkDynamic(); 
        meshFilter.mesh = waterMesh;
    }

    void Update()
    {
        if (leftWater == null || rightWater == null || pipeBounds == null) return;

        DrawPipeWater();
    }

    private void DrawPipeWater()
    {
        // 1. 获取管道的世界坐标边界
        float pipeLeftX = pipeBounds.bounds.min.x;
        float pipeRightX = pipeBounds.bounds.max.x;
        float pipeTopY = pipeBounds.bounds.max.y;
        float pipeBottomY = pipeBounds.bounds.min.y;

        // 2. 计算左侧和右侧的水面绘制高度
        float drawLeftY = Mathf.Clamp(leftWater.waterSurfaceY, pipeBottomY, pipeTopY);
        float drawRightY = Mathf.Clamp(rightWater.waterSurfaceY, pipeBottomY, pipeTopY);

        // 如果两端水面都低于管道底部，说明管道里没水，清空网格并返回
        if (drawLeftY <= pipeBottomY + 0.01f && drawRightY <= pipeBottomY + 0.01f)
        {
            waterMesh.Clear();
            return;
        }

        // 3. 将世界坐标转换为这个物体的本地坐标 (Local Space)
        Vector3 bottomLeft = transform.InverseTransformPoint(new Vector3(pipeLeftX, pipeBottomY, 0));
        Vector3 bottomRight = transform.InverseTransformPoint(new Vector3(pipeRightX, pipeBottomY, 0));
        Vector3 topLeft = transform.InverseTransformPoint(new Vector3(pipeLeftX, drawLeftY, 0));
        Vector3 topRight = transform.InverseTransformPoint(new Vector3(pipeRightX, drawRightY, 0));

        // 4. 构建网格顶点数据
        Vector3[] vertices = new Vector3[4] { bottomLeft, bottomRight, topLeft, topRight };

        // 构建三角形 (0=左下, 1=右下, 2=左上, 3=右上)
        int[] triangles = new int[6] { 0, 2, 1, 1, 2, 3 };

        // 5. 构建 UV 贴图坐标 (实现贴图滚动)
        // 计算水流方向：如果左边水高，水往右流(正向)，否则往左流(反向)
        float flowDirection = (leftWater.waterSurfaceY > rightWater.waterSurfaceY) ? 1f : -1f;

        // 只有两端水位有落差时才滚动贴图
        if (Mathf.Abs(leftWater.waterSurfaceY - rightWater.waterSurfaceY) > 0.1f)
        {
            scrollOffset += Time.deltaTime * uvScrollSpeed * flowDirection;
        }

        // 设置 UV，使其不被拉伸变形
        float pipeWidth = pipeRightX - pipeLeftX;
        float uvLeft = scrollOffset;
        float uvRight = scrollOffset + pipeWidth; // 宽度决定UV重复次数

        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(uvLeft, 0),
            new Vector2(uvRight, 0),
            new Vector2(uvLeft, (drawLeftY - pipeBottomY)),
            new Vector2(uvRight, (drawRightY - pipeBottomY))
        };

        // 6. 刷新网格
        waterMesh.Clear();
        waterMesh.vertices = vertices;
        waterMesh.triangles = triangles;
        waterMesh.uv = uvs;
    }
}