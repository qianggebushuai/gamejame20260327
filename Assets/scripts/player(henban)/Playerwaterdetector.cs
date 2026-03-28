using UnityEngine;
public class WaterDetector : MonoBehaviour
{
    private Player1 player;
    public WaterBody body;
    void Start()
    {
        player = GetComponent<Player1>();
        if (player == null)
        {
            player = GetComponentInParent<Player1>();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        WaterBody water = collision.GetComponent<WaterBody>();
        body = water;
        if (water != null && player != null)
        {
            player.EnterWater(water);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        WaterBody water = collision.GetComponent<WaterBody>();
        body = null;
        if (water != null && player != null)
        {
            player.ExitWater();
        }
    }
}