using UnityEngine;

public class WaterRelated : MonoBehaviour
{
    public WaterBody waterToDecrease; // 水源 (左侧/高处)
    public WaterBody waterToIncrease; // 目标 (右侧/低处)
    public BoxCollider2D pipeBounds;  // 管道的范围边界

    [Header("水流传输设置")]
    public float flowSpeed = 2f;      // 水流传输速度
    public bool isValveOpen = false;  // 阀门是否打开

    void Update()
    {
        if (isValveOpen)
        {
            TransmitWater();
        }
    }

    // 可以在UI按钮上调用这个方法来开关水闸
    public void ToggleValve()
    {
        isValveOpen = !isValveOpen;
    }

    private void TransmitWater()
    {
        if (waterToDecrease == null || waterToIncrease == null || pipeBounds == null) return;

        float pipeBottomY = pipeBounds.bounds.min.y;

        if (waterToDecrease.waterSurfaceY > waterToIncrease.waterSurfaceY &&
            waterToDecrease.waterSurfaceY > pipeBottomY)
        {
            float flowAmount = flowSpeed * Time.deltaTime;

            float heightDifference = waterToDecrease.waterSurfaceY - waterToIncrease.waterSurfaceY;
            if (heightDifference < flowAmount * 2f)
            {
                flowAmount = heightDifference / 2f;
                isValveOpen = false; 
            }
            waterToDecrease.ChangeWaterLevelBy(-flowAmount);
            waterToIncrease.ChangeWaterLevelBy(flowAmount);
        }
    }
}