using UnityEngine;

public class Playerdivestate : Playerstate
{
    private float diveSwimSpeed = 0.4f;       // 水下自由游动的速度
    private float oxygenDecreaseRate = 10f; // 耗氧速度 (可按需调整)
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

        player.rb.gravityScale = 0f; // 水下也是零重力，完全用代码控制
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

        // 如果意外离开水体，切回空中状态
        if (!player.isInWater)
        {
            statemachine.changestate(player.airstate);
            return;
        }

        // 主动退出潜水 (按空格或者再按一次 R，浮出水面)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R))
        {
            statemachine.changestate(player.swimstate);
            return;
        }

        // 水下 8 向移动输入 (WASD / 上下左右)
        float xInput = Input.GetAxisRaw("Horizontal");
        float yInput = Input.GetAxisRaw("Vertical");

        // 如果玩家想游出水面
        if (player.currentWater != null)
        {
            float waterSurface = player.currentWater.GetWaterSurfaceY();
            float playerY = player.transform.position.y;

            // 如果玩家往上游，且头部已经突破水面了，自动切回表面游泳状态
            if (yInput > 0 && playerY >= waterSurface - 0.2f)
            {
                statemachine.changestate(player.swimstate);
                return;
            }
        }

        // 设置水底游动速度
        player.Setvelocity(xInput * diveSwimSpeed, yInput * diveSwimSpeed);

        ConsumeOxygen();
    }

    private void ConsumeOxygen()
    {
        player.currentoxegenvalue -= oxygenDecreaseRate * Time.deltaTime;
        player.currentoxegenvalue = Mathf.Max(0, player.currentoxegenvalue);

        if (player.currentoxegenvalue <= 0)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                TakeDrowningDamage();
                damageTimer = 0f;
            }
        }
    }

    private void TakeDrowningDamage()
    {
        Debug.Log("溺水伤害！");
        // player.damageofplayer();
    }
}