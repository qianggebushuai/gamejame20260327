using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windareadirection : MonoBehaviour
{
    [Header("引用的风场组件")]
    public windarea wd;

    [Header("不同状态下的风向")]
    public windarea.winddir springdir = windarea.winddir.up;
    public windarea.winddir winterdir = windarea.winddir.left;

    void Start()
    {
        if (wd == null)
        {
            wd = GetComponent<windarea>();
        }
    }

    void Update()
    {
        if (ScreenCoverTransition2D.instance == null) return;

        if (ScreenCoverTransition2D.instance.currentState == ScreenCoverTransition2D.State.Covered)
        {
            switchto(winterdir);
        }
        else
        {
            // 比如其他状态代表春天
            switchto(springdir);
        }
    }

    private void switchto(windarea.winddir targetDir)
    {
        if (wd != null && wd.direction != targetDir)
        {
            wd.direction = targetDir;

            RotateVisuals(targetDir);

            Debug.Log($"[季节变化] 风场方向已切换为: {targetDir}");
        }
    }


    private void RotateVisuals(windarea.winddir targetDir)
    {
        float zRotation = 0f;

        switch (targetDir)
        {
            case windarea.winddir.right: zRotation = 0f; break;
            case windarea.winddir.up: zRotation = 90f; break;
            case windarea.winddir.left: zRotation = 180f; break;
            case windarea.winddir.down: zRotation = -90f; break;
        }

        transform.rotation = Quaternion.Euler(0, 0, zRotation);
    }
}
