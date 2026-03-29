using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Horizontal Gradient")]
[RequireComponent(typeof(Graphic))]
public class UIGradient : BaseMeshEffect
{
    [Header("渐变颜色设置")]
    public Color leftColor = new Color(0f, 1f, 1f, 1f);   // 左边的颜色 (默认青色)
    public Color rightColor = new Color(0f, 0f, 1f, 1f);  // 右边的颜色 (默认深蓝)

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        // 获取当前UI元素的矩形范围
        Rect rect = GetComponent<RectTransform>().rect;

        UIVertex vertex = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            // 计算当前顶点在 X 轴上的比例 (0 到 1)
            float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, vertex.position.x);

            // 根据比例混合左右颜色，并叠加原本图片的基础颜色
            vertex.color = Color.Lerp(leftColor, rightColor, normalizedX) * vertex.color;

            vh.SetUIVertex(vertex, i);
        }
    }
}