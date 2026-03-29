using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterdrop : MonoBehaviour
{
    public float lifeTime = 2f;       
    public float waterAmount = 0.05f;
    public float rotatespeed;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1f; 
        rotatespeed = Random.Range(20f, 360f);

        StartCoroutine(LifeRoutine());
    }
    private void Update()
    {
        transform.Rotate(0, 0, rotatespeed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player1>())
        {
            Player1 player = collision.GetComponent<Player1>();
            player.isdiedofswim = true;
            player.causedamage();
        }
        if (collision.tag=="tree")
        {
            if (collision.GetComponent<windtree>().hasGrown==true|| collision.GetComponent<icetree>().hasGrown == true)
            {
                Destroy(gameObject);
            }

        }

        WaterBody water = collision.GetComponent<WaterBody>();
        if (water != null)
        {
            water.ChangeWaterLevelBy(waterAmount);
            Destroy(gameObject); 
        }
    }

    private IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
