using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBeginPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MusicManager.Instance.Play("basic");
        MusicManager.Instance.Play("summer");
        MusicManager.Instance.Play("winter");
        MusicManager.Instance.Stop("zhujiemian");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
