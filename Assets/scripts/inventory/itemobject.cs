using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemobject : MonoBehaviour
{
    public itemdata item;
    public CircleCollider2D cc;
    void Start()
    {
        cc = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (item != null)
            {
                inventory.instance.AddItem(item);
            }
        }
    }
}
