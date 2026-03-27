using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class playerspawnstate : Playerstate
{
    public playerspawnstate(Player1 _player, Playerstatemachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        statemachine.changestate(player.idlestate);
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

    }
}
