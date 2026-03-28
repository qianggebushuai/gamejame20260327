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
    }

    public override void Exit()
    {

        base.Exit();
        
        
    }

    public override void Update()
    {
        base.Update();
        dietimer -= Time.deltaTime;
        if (dietimer < 0)
        {
            statemachine.changestate(player.spawnstate);
        }
    }
}
