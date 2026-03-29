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

            Vector2 windVelocity = GetWindDirection() * windForce;

            rb.AddForce(windVelocity, ForceMode2D.Force);
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
                originalGravityDict[player] = rb.gravityScale;

                rb.gravityScale = 0.2f;

                rb.velocity = new Vector2(rb.velocity.x, 0.2f);
            }

            Debug.Log("检测到玩家进入，重力已设为 0");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null && playersInArea.Contains(player))
        {
            playersInArea.Remove(player);

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null && originalGravityDict.ContainsKey(player))
            {
                rb.gravityScale = 8;

                originalGravityDict.Remove(player);
            }

        }
    }
}