using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wind : MonoBehaviour
{
    private Animator anim;
    private BoxCollider2D bc;
    [SerializeField] private float delay = 2f;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        bc = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (tag != "wall")
        {
            bc.enabled = false;
        }
        else
        {
            bc.enabled = true;
        }
        if (ScreenCoverTransition2D.instance != null &&
            ScreenCoverTransition2D.instance.currentState == ScreenCoverTransition2D.State.Idle)
        {
            delay -= Time.deltaTime;
            if (delay < 0)
            {
                anim.SetBool("Grow", true);
            }
        }
    }
}