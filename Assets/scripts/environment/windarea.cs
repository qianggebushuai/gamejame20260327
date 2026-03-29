using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windarea : MonoBehaviour
{
    [Header("风场设置")]
    public winddir direction = winddir.right;
    public float windForce = 10f;

    public enum winddir { up, down, left, right };

    private List<Player1> playersInArea = new List<Player1>();

    // 记录玩家进入风场前的原本重力
    private Dictionary<Player1, float> originalGravityDict = new Dictionary<Player1, float>();

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void FixedUpdate()
    {
        playersInArea.RemoveAll(p => p == null);

        foreach (Player1 player in playersInArea)
        {
            ApplyWind(player);
        }
    }

    private Vector2 GetWindDirection()
    {
        switch (direction)
        {
            case winddir.up: return Vector2.up;
            case winddir.down: return Vector2.down;
            case winddir.left: return Vector2.left;
            case winddir.right: return Vector2.right;
            default: return Vector2.zero;
        }
    }

    private void ApplyWind(Player1 player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (direction == winddir.left || direction == winddir.right)
            {
                Vector2 windVelocity = GetWindDirection() * windForce;
                rb.AddForce(windVelocity, ForceMode2D.Force);
            }
            else
            {
                float verticalVelocity = GetWindDirection().y * windForce;
                rb.velocity = new Vector2(rb.velocity.x, verticalVelocity);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null && !playersInArea.Contains(player))
        {
            playersInArea.Add(player);

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 只有左右风向才修改重力
                if (direction == winddir.left || direction == winddir.right)
                {
                    originalGravityDict[player] = rb.gravityScale; // 记录原重力
                    rb.gravityScale = 1f; 

                    rb.velocity = new Vector2(rb.velocity.x / 100f, 0f);
                    Debug.Log("检测到玩家进入左右风场，重力已设为 0");
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null && playersInArea.Contains(player))
        {
            playersInArea.Remove(player);

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 如果字典里有记录（说明进入时是左右风向，修改了重力），则恢复原来的重力
                if (originalGravityDict.ContainsKey(player))
                {
                    rb.gravityScale = 8; // 恢复原本的重力（不再写死为8）
                    originalGravityDict.Remove(player);
                }
            }
        }
    }
}