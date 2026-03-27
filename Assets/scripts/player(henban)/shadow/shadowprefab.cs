using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shadowprefab : MonoBehaviour
{
    [Header("淡出设置")]
    [SerializeField] private float fadeTime = 0.5f;        // 淡出持续时间
    [SerializeField] private float initialAlpha = 0.8f;    // 初始透明度

    private SpriteRenderer spriteRenderer;
    private Color initialColor;
    private float timer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {

            initialColor = spriteRenderer.color;
            initialColor.a = initialAlpha;
            spriteRenderer.color = initialColor;
        }

        Destroy(gameObject, fadeTime);
    }

    void Update()
    {
        if (spriteRenderer != null)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(initialAlpha, 0f, timer / fadeTime);

            Color currentColor = spriteRenderer.color;
            currentColor.a = alpha;
            spriteRenderer.color = currentColor;
        }
    }
}