using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerdiestate : Playerstate
{

    private float dietimer = 3f;

    
    public Playerdiestate(Player1 _player, Playerstatemachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.anim.SetBool("Dieway", player.isdiedofswim);
        player.cc.enabled = false;
    }

    public override void Exit()
    {
        player.cc.enabled = true;
        base.Exit();
        
        
    }

    public override void Update()
    {
        base.Update();
        dietimer -= Time.deltaTime;
        player.rb.velocity = Vector2.zero;
        if (dietimer < 0)
        {
            statemachine.changestate(player.spawnstate);
        }
    }
}
