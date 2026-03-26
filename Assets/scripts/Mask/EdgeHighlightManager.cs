using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 使用Shader实现更精确的边缘高亮效果
/// 挂载到一个空物体上，由ScreenCoverTransition2D调用
/// </summary>
public class EdgeHighlightManager : MonoBehaviour
{
    [Header("高亮设置")]
    public Color outlineColor = new Color(1f, 0.9f, 0f, 1f);
    public float outlineWidth = 0.02f;
    public float pulseSpeed = 3f;

    private Material outlineShaderMaterial;
    private List<HighlightData> activeHighlights = new List<HighlightData>();

    private struct HighlightData
    {
        public GameObject highlightObject;
        public SpriteRenderer originalRenderer;
    }

    void Awake()
    {
        // 尝试使用自定义Shader，如果没有则用备用方案
        Shader outlineShader = Shader.Find("Custom/SpriteOutline2D");
        if (outlineShader != null)
        {
            outlineShaderMaterial = new Material(outlineShader);
            outlineShaderMaterial.SetColor("_OutlineColor", outlineColor);
            outlineShaderMaterial.SetFloat("_OutlineWidth", outlineWidth);
            outlineShaderMaterial.SetFloat("_PulseSpeed", pulseSpeed);
        }
    }

    /// <summary>
    /// 为指定的SpriteRenderer列表创建边缘高亮
    /// </summary>
    public void CreateHighlights(SpriteRenderer[] renderers, int sortingOrderOffset = 100)
    {
        ClearHighlights();

        foreach (SpriteRenderer sr in renderers)
        {
            if (sr == null || sr.sprite == null) continue;
            CreateSingleHighlight(sr, sortingOrderOffset);
        }
    }

    void CreateSingleHighlight(SpriteRenderer original, int sortingOrderOffset)
    {
        if (outlineShaderMaterial != null)
        {
            // 使用Shader方式（更精确）
            CreateShaderHighlight(original, sortingOrderOffset);
        }
        else
        {
            // 使用多副本偏移方式（兼容性好）
            CreateOffsetHighlight(original, sortingOrderOffset);
        }
    }

    void CreateShaderHighlight(SpriteRenderer original, int sortingOrderOffset)
    {
        GameObject highlightObj = new GameObject($"ShaderHighlight_{original.name}");
        highlightObj.transform.position = original.transform.position;
        highlightObj.transform.rotation = original.transform.rotation;
        highlightObj.transform.localScale = original.transform.lossyScale * 1.02f;

        SpriteRenderer hlRenderer = highlightObj.AddComponent<SpriteRenderer>();
        hlRenderer.sprite = original.sprite;
        hlRenderer.material = new Material(outlineShaderMaterial);
        hlRenderer.sortingLayerName = original.sortingLayerName;
        hlRenderer.sortingOrder = original.sortingOrder + sortingOrderOffset;

        activeHighlights.Add(new HighlightData
        {
            highlightObject = highlightObj,
            originalRenderer = original
        });
    }

    void CreateOffsetHighlight(SpriteRenderer original, int sortingOrderOffset)
    {
        GameObject parent = new GameObject($"OffsetHighlight_{original.name}");
        parent.transform.position = original.transform.position;
        parent.transform.rotation = original.transform.rotation;
        parent.transform.localScale = original.transform.lossyScale;

        // 8方向偏移创建边缘
        Vector2[] directions = {
            new Vector2(-1, 0), new Vector2(1, 0),
            new Vector2(0, -1), new Vector2(0, 1),
            new Vector2(-1, -1), new Vector2(1, -1),
            new Vector2(-1, 1), new Vector2(1, 1)
        };

        foreach (Vector2 dir in directions)
        {
            GameObject edgePart = new GameObject("Edge");
            edgePart.transform.SetParent(parent.transform, false);
            edgePart.transform.localPosition = (Vector3)(dir.normalized * outlineWidth);

            SpriteRenderer edgeRenderer = edgePart.AddComponent<SpriteRenderer>();
            edgeRenderer.sprite = original.sprite;
            edgeRenderer.color = outlineColor;
            edgeRenderer.sortingLayerName = original.sortingLayerName;
            edgeRenderer.sortingOrder = original.sortingOrder + sortingOrderOffset;
        }

        activeHighlights.Add(new HighlightData
        {
            highlightObject = parent,
            originalRenderer = original
        });
    }

    /// <summary>
    /// 更新高亮闪烁
    /// </summary>
    public void UpdateHighlightPulse()
    {
        float pulse = Mathf.Lerp(0.3f, 1.0f,
            (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) * 0.5f);

        foreach (var data in activeHighlights)
        {
            if (data.highlightObject == null) continue;

            SpriteRenderer[] renderers = data.highlightObject.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in renderers)
            {
                if (outlineShaderMaterial != null)
                {
                    // Shader内部处理闪烁
                    continue;
                }

                Color c = outlineColor;
                c.a = pulse;
                sr.color = c;
            }
        }
    }

    /// <summary>
    /// 跟踪原始物体位置（如果它们会动的话）
    /// </summary>
    public void UpdateHighlightPositions()
    {
        foreach (var data in activeHighlights)
        {
            if (data.highlightObject == null || data.originalRenderer == null) continue;

            data.highlightObject.transform.position = data.originalRenderer.transform.position;
            data.highlightObject.transform.rotation = data.originalRenderer.transform.rotation;
        }
    }

    public void ClearHighlights()
    {
        foreach (var data in activeHighlights)
        {
            if (data.highlightObject != null)
            {
                Destroy(data.highlightObject);
            }
        }
        activeHighlights.Clear();
    }

    void OnDestroy()
    {
        ClearHighlights();
        if (outlineShaderMaterial != null)
        {
            Destroy(outlineShaderMaterial);
        }
    }
}