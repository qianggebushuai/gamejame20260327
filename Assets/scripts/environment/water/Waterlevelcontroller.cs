using UnityEngine;

public class WaterLevelController : MonoBehaviour
{
    public WaterBody waterBody;
    public float changeAmount = 0.5f;
    public float changeSpeed = 1f;

    private float targetWaterLevel;
    private bool isChanging = false;

    void Start()
    {
        if (waterBody != null)
        {
            targetWaterLevel = waterBody.GetWaterSurfaceY();
        }
    }

    void Update()
    {
        // 测试按键
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RaiseWater(changeAmount);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            LowerWater(changeAmount);
        }

        // 平滑改变水位
        if (isChanging && waterBody != null)
        {
            float currentLevel = waterBody.GetWaterSurfaceY();
            float newLevel = Mathf.MoveTowards(currentLevel, targetWaterLevel, changeSpeed * Time.deltaTime);
            waterBody.SetWaterLevel(newLevel);

            if (Mathf.Approximately(newLevel, targetWaterLevel))
            {
                isChanging = false;
            }
        }
    }

    public void RaiseWater(float amount)
    {
        targetWaterLevel += amount;
        isChanging = true;
        Debug.Log("水位上升到: " + targetWaterLevel);
    }

    public void LowerWater(float amount)
    {
        targetWaterLevel -= amount;
        isChanging = true;
        Debug.Log("水位下降到: " + targetWaterLevel);
    }

    public void SetWaterLevel(float level)
    {
        targetWaterLevel = level;
        isChanging = true;
    }
}