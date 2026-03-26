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

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        foreach (Player1 player in playersInArea)
        {
            if (player != null)
            {
                ApplyWind(player);
            }
        }
    }

    private Vector2 GetWindDirection()
    {
        switch (direction)
        {
            case winddir.up:
                return Vector2.up;
            case winddir.down:
                return Vector2.down;
            case winddir.left:
                return Vector2.left;
            case winddir.right:
                return Vector2.right;
            default:
                return Vector2.zero;
        }
    }

    private void ApplyWind(Player1 player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 windVelocity = GetWindDirection() * windForce;
            rb.velocity += windVelocity * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null && !playersInArea.Contains(player))
        {
            playersInArea.Add(player);
            Debug.Log("检测到player");
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        Player1 player = other.GetComponent<Player1>();
        if (player != null && playersInArea.Contains(player))
        {
            playersInArea.Remove(player);
        }
    }
}