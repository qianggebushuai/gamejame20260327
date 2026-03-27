using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Playerdashshadow : MonoBehaviour
{
    [SerializeField] private GameObject shadow;
    private Player1 player;
    void Start()
    {
        player = GetComponentInParent<Player1>();  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void createshadpw()
    {
        Instantiate(shadow, transform.position, Quaternion.identity);
        shadow.transform.localScale = player.transform.localScale;
    }
}
