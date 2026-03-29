using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : MonoBehaviour
{
    public BoxCollider2D bc;
    public Sprite open;
    public Sprite close;
    public SpriteRenderer sr;
    void Start()
    {
        bc = GetComponent<BoxCollider2D>();
        bc.enabled = true;
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = true;
        sr.sprite = close;
    }

    public void opendoor()
    {
        if(bc.enabled ==true)
        {
            bc.enabled = false;
            sr.sprite = open;
        }
        else
        {
            bc.enabled = true;
            sr.sprite = close;
        }

    }
    void Update()
    {
        
    }
}
