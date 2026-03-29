using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 尝试从碰到的物体上获取 Player1 脚本
        Player1 player = collision.GetComponent<Player1>();

        if (player != null)
        {
            player.isdiedofswim = false;
            player.statemachine.changestate(player.diestate);
            player.causedamage();
        }
    }

}