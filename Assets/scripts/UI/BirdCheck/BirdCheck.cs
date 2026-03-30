using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BirdCheck : MonoBehaviour
{

    public Image[] bird = new Image[10];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i=0;i<10;++i)
        {
            if(BoolManager.Instance.GetBool(i))
            {
                bird[i].enabled = true;
            }
            else
            {
                bird[i].enabled = false;
            }
        }
    }
}
