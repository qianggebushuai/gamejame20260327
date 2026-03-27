using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ice : MonoBehaviour
{
    private BoxCollider2D bc;
    [SerializeField] private GameObject iceprefab;
    [SerializeField] private float minSpeedToSpawn = 5f;
    [SerializeField] private float spawnForce = 10f;
    [SerializeField] private int spawnCount = 3; 
    [SerializeField] private string playerTag = "Player"; 

    void Start()
    {
        bc = GetComponentInChildren<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {

            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                float playerSpeed = playerRb.velocity.magnitude;
                Debug.Log(playerSpeed);
                if (playerSpeed >= minSpeedToSpawn)
                {
                    SpawnIce();
                    Debug.Log("spawn");
                }
            }
        }
    }
    private void SpawnIce()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject newIce = Instantiate(iceprefab, transform.position, Quaternion.identity);

            Rigidbody2D iceRb = newIce.GetComponent<Rigidbody2D>();

            if (iceRb != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;

                iceRb.AddForce(randomDirection * Random.Range(0,spawnForce), ForceMode2D.Impulse);
            }
        }
        Destroy(gameObject);
    }
}