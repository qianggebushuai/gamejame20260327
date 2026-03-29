using UnityEngine;

/// <summary>
/// UI 持续旋转动画
/// </summary>
public class UIRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 0, 50f);  // 每秒旋转角度
    [SerializeField] private bool rotateInLocalSpace = true;

    [Header("Wave Rotation (摆动)")]
    [SerializeField] private bool enableWaveRotation = false;
    [SerializeField] private float waveAmplitude = 15f;     // 摆动幅度
    [SerializeField] private float waveFrequency = 1f;      // 摆动频率

    private Vector3 initialRotation;

    private void Start()
    {
        initialRotation = transform.localEulerAngles;
    }

    private void Update()
    {
        if (enableWaveRotation)
        {
            // 摆动旋转
            float wave = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
            transform.localRotation = Quaternion.Euler(initialRotation + new Vector3(0, 0, wave));
        }
        else
        {
            // 持续旋转
            if (rotateInLocalSpace)
            {
                transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
            }
            else
            {
                transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);
            }
        }
    }
}