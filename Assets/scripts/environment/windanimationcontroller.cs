using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windanimationcontroller : MonoBehaviour
{
    GameObject wind;
    public void setcanclimb()
    {
        wind.tag = "wall";
    }
    void Start()
    {
        wind = GetComponentInParent<wind>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
