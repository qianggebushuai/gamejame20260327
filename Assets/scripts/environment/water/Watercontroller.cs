// 挂载在 Water 物体上
using UnityEngine;

public class WaterController : MonoBehaviour
{

    public PhysicsWaterWave physicsWave; 

    void OnPlayerEnterWater(Vector3 position, float velocity)
    {
        if (physicsWave != null)
        {
            physicsWave.Splash(position.x, velocity);
        }

    }
}