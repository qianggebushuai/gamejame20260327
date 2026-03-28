using UnityEngine;

public class Playerswimstate : Playerstate
{
    private float swimSpeed = 1f;           // 稍微加快点水平速度
    private float floatUpSpeedLimit = 8f;   // 上浮时的最大速度限制 (之前是3，太慢了)
    private float surfaceFloatForce = 15f;  // 浮力加速度

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
        player.currentoxegenvalue = player.maxoxegenvalue;
        Debug.Log("退出游泳状态");
    }

    public override void Update()
    {
        base.Update();

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

        if (Input.GetKeyDown(KeyCode.R))
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
                // 用 Lerp 平滑降速，停留在水面
                yVelocity = Mathf.Lerp(yVelocity, 0, Time.deltaTime * 10f);

                // 在水面上按空格可以跳跃出水
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    yVelocity = 12f; // 赋予一个冲出水面的向上初速度
                }
            }
        }

        // 应用速度
        player.Setvelocity(xInput * swimSpeed, yVelocity);
    }
}