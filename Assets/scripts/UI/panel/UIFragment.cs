using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 碎片行为
/// </summary>
public class UIFragment : MonoBehaviour
{
    private Vector2 velocity;
    private float rotationSpeed;
    private float shrinkSpeed;
    private float gravity = 800f;
    private RectTransform rectTransform;
    private Image image;
    private float lifetime = 2f;
    private float elapsed = 0f;

    public void Initialize(Vector2 initialVelocity, float rotation, float shrink)
    {
        velocity = initialVelocity;
        rotationSpeed = rotation;
        shrinkSpeed = shrink;

        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        // 应用重力
        velocity.y -= gravity * Time.deltaTime;

        // 移动
        rectTransform.anchoredPosition += velocity * Time.deltaTime;

        // 旋转
        rectTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // 缩小
        float scale = Mathf.Lerp(1f, 0f, elapsed / lifetime);
        rectTransform.localScale = Vector3.one * scale;

        // 淡出
        if (image != null)
        {
            Color color = image.color;
            color.a = Mathf.Lerp(1f, 0f, elapsed / lifetime);
            image.color = color;
        }

        // 销毁
        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}