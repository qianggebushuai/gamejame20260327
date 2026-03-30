using UnityEngine;

public class Playerswimstate : Playerstate
{
    private float swimSpeed = 1f;
    private float oxygenDecreaseRate = 1f;
    private float floatUpSpeedLimit = 8f;  
    private float surfaceFloatForce = 15f; 

    private float normalGravity;
    private float normalDrag;

    public Playerswimstate(Player1 _player, Playerstatemachine _statemachine, string _animboolname)
        : base(_player, _statemachine, _animboolname) { }

    public override void Enter()
    {
        base.Enter();
        normalGravity = player.rb.gravityScale;
        normalDrag = player.rb.drag;

        player.rb.gravityScale = 0f; 
        player.rb.drag = 2f;
        swimSpeed = player.swimspeed;

        player.candash = false;
        player.candoublejump = false;
        Debug.Log("进入游泳状态");
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = normalGravity;
        player.rb.drag = normalDrag;
        player.candash = true;
        player.candoublejump = true;
        Debug.Log("退出游泳状态");
    }

    public override void Update()
    {
        base.Update();
        ConsumeOxygen();
        // 离开水面
        if (!player.isInWater)
        {
            statemachine.changestate(player.airstate);
            return;
        }

        if (player.isgrounddetected() && !player.isInWater)
        {
            statemachine.changestate(player.idlestate);
            return;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            statemachine.changestate(player.divestate);
            return;
        }

        float xInput = Input.GetAxisRaw("Horizontal");
        float yVelocity = player.rb.velocity.y;

        if (player.currentWater != null)
        {
            float waterSurface = player.currentWater.GetWaterSurfaceY(player.transform.position.x);
            float playerY = player.transform.position.y;
            float distanceToSurface = waterSurface - playerY;

            // 1. 在深水中 (需要上浮)
            if (distanceToSurface > 0.2f)
            {
                yVelocity += surfaceFloatForce * Time.deltaTime;
                yVelocity = Mathf.Min(yVelocity, floatUpSpeedLimit);
            }
            // 2. 接近水面 (稳定漂浮)
            else if (Mathf.Abs(distanceToSurface) <= 0.2f)
            {
                yVelocity = Mathf.Lerp(yVelocity, 0, Time.deltaTime * 10f);

                // 在水面上按空格可以跳跃出水
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    yVelocity = 30f; 
                }
            }
        }

        // 应用速度
        player.Setvelocity(xInput * swimSpeed, yVelocity);
    }
    private void ConsumeOxygen()
    {
        if (player.detecter.body != null)
        {
            if (!player.detecter.body.cancomsumeo2whenswim) return;
        }
        else
        {
            return;
        }

            player.currentoxegenvalue -= oxygenDecreaseRate * Time.deltaTime;
        player.currentoxegenvalue = Mathf.Max(0, player.currentoxegenvalue);

        if (player.currentoxegenvalue <= 0)
        {
            player.isdiedofswim = true;
            player.causedamage();
        }
    }

}