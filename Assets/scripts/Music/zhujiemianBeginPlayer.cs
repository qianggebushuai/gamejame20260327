using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zhujiemianBeginPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MusicManager.Instance.Play("zhujiemian", fadeInDuration: 2f);
        MusicManager.Instance.Stop("summer");
        MusicManager.Instance.Play("winter");
        MusicManager.Instance.Stop("basic");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
