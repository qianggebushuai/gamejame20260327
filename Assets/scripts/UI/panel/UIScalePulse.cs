using UnityEngine;

/// <summary>
/// UI 脉冲缩放动画
/// </summary>
public class UIScalePulse : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Scale Type")]
    [SerializeField] private ScaleType scaleType = ScaleType.Sine;

    public enum ScaleType
    {
        Sine,           // 正弦波
        PingPong,       // 来回
        Elastic         // 弹性
    }

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        float scale = 1f;

        switch (scaleType)
        {
            case ScaleType.Sine:
                // 正弦波缩放
                scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
                break;

            case ScaleType.PingPong:
                // 来回缩放
                scale = Mathf.Lerp(minScale, maxScale, Mathf.PingPong(Time.time * pulseSpeed, 1f));
                break;

            case ScaleType.Elastic:
                // 弹性缩放
                float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
                scale = Mathf.Lerp(minScale, maxScale, EaseOutElastic(t));
                break;
        }

        transform.localScale = originalScale * scale;
    }

    private float EaseOutElastic(float t)
    {
        float p = 0.3f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - p / 4f) * (2f * Mathf.PI) / p) + 1f;
    }
}