using UnityEngine;

public class Playerdivestate : Playerstate
{
    private float diveSwimSpeed = 0.4f;    
    private float oxygenDecreaseRate = 10f; 
    private float damageInterval = 1f;
    private float damageTimer = 0f;

    private float normalGravity;
    private float normalDrag;

    public Playerdivestate(Player1 _player, Playerstatemachine _statemachine, string _animboolname)
        : base(_player, _statemachine, _animboolname) { }

    public override void Enter()
    {
        base.Enter();
        normalGravity = player.rb.gravityScale;
        normalDrag = player.rb.drag;
        diveSwimSpeed = player.divespeed;
        player.rb.gravityScale = 0f; 
        player.rb.drag = 3f;
        damageTimer = 0f;
        Debug.Log("进入潜水状态");
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = normalGravity;
        player.rb.drag = normalDrag;
        Debug.Log("退出潜水状态");
    }

    public override void Update()
    {
        base.Update();

        if (!player.isInWater)
        {
            statemachine.changestate(player.airstate);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R))
        {
            statemachine.changestate(player.swimstate);
            return;
        }

        // 水下 8 向移动输入 (WASD / 上下左右)
        float xInput = Input.GetAxisRaw("Horizontal");
        float yInput = Input.GetAxisRaw("Vertical");

        if (player.currentWater != null)
        {
            float waterSurface = player.currentWater.GetWaterSurfaceY(player.transform.position.x);
            float playerY = player.transform.position.y;

            if (yInput > 0 && playerY >= waterSurface - 0.2f)
            {
                statemachine.changestate(player.swimstate);
                return;
            }
        }

        player.Setvelocity(xInput * diveSwimSpeed, yInput * diveSwimSpeed*10);

        ConsumeOxygen();
    }

    private void ConsumeOxygen()
    {
        player.currentoxegenvalue -= oxygenDecreaseRate * Time.deltaTime;
        player.currentoxegenvalue = Mathf.Max(0, player.currentoxegenvalue);

        if (player.currentoxegenvalue <= 0)
        {
            player.causedamage();
        }
    }

    private void TakeDrowningDamage()
    {
        Debug.Log("溺水伤害！");
        // player.damageofplayer();
    }
}