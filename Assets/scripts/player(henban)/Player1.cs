using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player1 : entity
{
    public bool isdead=false;


    [SerializeField] public Transform summoncheck;
    [SerializeField] private float colorlosingspeed;
    private SpriteRenderer sr;
    
    [Header("Attack details")]
    public float[] Attackmovement;
    public float counterattackduration;
   

    [Header("Speed info")]
    
    public float jumpforce;

    [Header("Dash info")]
    public float dashspeed;
    public float dashduration;
    public bool isdashing=false;
    public bool canclone;
    public bool candoublejump=true;
    public bool isWallDashing = false;
    private float dashtimer;
    public int wallDashDirection;           // 墙壁冲刺方向 (-1 或 1)
    public float wallDashDuration = 0.2f;   // 墙壁冲刺持续时间
    public float wallDashSpeedH = 15f;      // 墙壁冲刺水平速度
    public float wallDashSpeedV = 12f;      // 墙壁冲刺垂直速度
    public float dashcooldown = 0.5f;
    float dashcooldowntimer;
    public bool candash=true;
    public int playerLayer;
    [Header("layermaskchange")] 
    public ScreenCoverTransition2D ctl;
    public int groupALayer;
    public int groupBLayer;
    [Header("Wall Bounce Settings - 墙壁反弹")]
    public float wallBounceSpeedH = 10f;   
    public float wallBounceSpeedV = 12f;
    public bool cancontrol = true;
    float dietime = 5f;
    [Header("氧气条")]
    public float maxoxegenvalue=50f;
    public float currentoxegenvalue;
    [Header("水中设置")]
    public bool isInWater = false;
    public bool isUnderwater = false;
    public WaterBody currentWater;
    public float swimspeed=1f;
    public float divespeed=0.4f;
    public float waterSurfaceOffset = 0.5f; // 玩家中心到头顶的距离

    [Header("水中物理")]
    private float normalGravity;
    private float normalDrag;
    public float dashdirection { get; private set; }

    #region Conponents
    public WaterDetector detecter;
    public Playerstatemachine statemachine { get; private set; }
    public Playeridlestate idlestate { get; private set; }
    public Playermovestate movestate { get; private set; }
    public Playerjumpstate jumpstate { get; private set; }
    public Playerairstate airstate { get; private set; }
    public Playerdashstate dashstate { get; private set; }
    public Playerwallslidstate wallslide { get; private set; }
    public Playerwalljump walljump { get; private set; }

    public playerspawnstate spawnstate { get; private set; }

    public Playerprimaryattack primaryattack { get; private set; }
    public Playerhitstate hitstate { get; private set; }
    public Playercounterattackstate counterattack { get; private set; }
    public Playeraimswordstate aimsword { get; private set; }
    public playercatchswordstate catchsword { get; private set; }
    public Playerdiestate diestate { get; private set; }
    public Plyersummonswordstate summonstate { get; private set; }

    public Playerswimstate swimstate { get; private set; }

    public Playerdivestate divestate { get; private set; }
    #endregion
    public string lastanimboolname { get; private set; }
    public float jumpchecktime = 0.1f;
    

    protected override void Awake()
    {
        base.Awake();
        statemachine = new Playerstatemachine();
        idlestate = new Playeridlestate(this, statemachine, "Idle");
        movestate = new Playermovestate(this, statemachine, "Move");
        jumpstate=  new Playerjumpstate(this, statemachine, "Jump");
        airstate=new Playerairstate(this, statemachine, "Jump");
        dashstate = new Playerdashstate(this, statemachine, "Dash");
        wallslide = new Playerwallslidstate(this, statemachine, "Wallslide");
        walljump = new Playerwalljump(this, statemachine, "Jump");
        primaryattack = new Playerprimaryattack(this, statemachine, "Attack");
        hitstate=new Playerhitstate(this, statemachine, "Hit");
        counterattack=new Playercounterattackstate(this, statemachine, "Counterattack");
         aimsword=new Playeraimswordstate(this, statemachine, "Aimsword");
        catchsword=new playercatchswordstate(this, statemachine, "Catchsword");
        diestate=new Playerdiestate(this, statemachine, "Die");
        summonstate=new Plyersummonswordstate(this, statemachine, "Summon");
        spawnstate = new playerspawnstate(this, statemachine, "Spawn");
        swimstate = new Playerswimstate(this, statemachine, "Swim");
        divestate =new Playerdivestate(this, statemachine, "Dive");
        
    }
    protected override void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        detecter = GetComponent<WaterDetector>();
        playerLayer = LayerMask.NameToLayer("Player");
        groupALayer = LayerMask.NameToLayer("SPRING");
        groupBLayer = LayerMask.NameToLayer("WINTER");
        base.Start();
        dashcooldowntimer = dashcooldown;
        currentoxegenvalue = maxoxegenvalue;
        normalGravity = rb.gravityScale;
        normalDrag = rb.drag;
        statemachine.Initialized(idlestate);
    }
    protected override void Update()
    {
        if (ctl == null)
        {
            ctl = GameObject.FindGameObjectWithTag("ctl").GetComponent<ScreenCoverTransition2D>();
        }
        if (ctl.IsUpdating())
        {
            Setvelocity(0, 0);
            return;
        } 
        if (ctl.IsOverlap() == 0)
        {
            whatIsGround = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("SPRING");
            Physics2D.IgnoreLayerCollision(playerLayer, groupALayer, false);
            Physics2D.IgnoreLayerCollision(playerLayer, groupBLayer, true);
        }
        else
        {
            whatIsGround = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("WINTER");
            Physics2D.IgnoreLayerCollision(playerLayer, groupALayer, true);
            Physics2D.IgnoreLayerCollision(playerLayer, groupBLayer, false);
        }
        base.Update();
        checkfordash();
        UpdateWaterState();
        quickfall();

        statemachine.currentstate.Update();

    }
    public void turnblue(float duration, float spreadTime) => StartCoroutine(CircularBlueSpread(duration, spreadTime));

    private Sprite circleSprite;
    private void recoveroxegen()
    {
        if (!isInWater)
        {
            if (currentoxegenvalue < maxoxegenvalue)
            {
                currentoxegenvalue += Time.deltaTime;
            }
            else
            {
                currentoxegenvalue = maxoxegenvalue;
            }
        }

    }
    IEnumerator CircularBlueSpread(float totalDuration, float spreadTime)
    {
        yield return null;
    }
    public void causedamage()
    {
        GameManager.instance.lives -= 1;
        if (GameManager.instance.lives != 0)
        {
            statemachine.changestate(diestate);
            GameManager.instance.PlayerDie();
        }
        else
        {
            statemachine.changestate(diestate);
            GameManager.instance.RestartGame();
        }
    }
    public virtual void assignanimname(string _aimboolname)
    {
        lastanimboolname = _aimboolname;
    
   

    }

    public virtual void damageofplayer()
    {
        fx.StartCoroutine("FlashFX");
        StartCoroutine("hitknockback");
        statemachine.changestate(hitstate);
    }
    private void checkfordash()
    {
        if (!candash)
        {
            dashcooldown -= Time.deltaTime;
            if (dashcooldown < 0)
            {
                candash = true;
                dashcooldown = dashcooldowntimer;
            }
        }
        if (iswalldetected())
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift)&& candash)
        {

            dashdirection = Input.GetAxisRaw("Horizontal");
            if (dashdirection == 0)
            {
                dashdirection = facingdirection;
            }
            statemachine.changestate(dashstate);
        }



    }
    private void quickfall()
    {
        if (Input.GetKeyDown(KeyCode.S)&&rb.velocity.y<0)
        {
            Setvelocity(rb.velocity.x,rb.velocity.y*4);
        }


    }
    public void Animationtrigger() => statemachine.currentstate.Animationfinshedtrigger();

    public override void die()
    {
        isdead = true;
        statemachine.changestate(diestate);
        base.die();
        disappear();
       
        //dietime -= Time.deltaTime;
        if (dietime < 0)
        {
            Destroy(gameObject);
        }


        

    }
    private void disappear()
    {
        sr.color = new Color(1, 1, 1, sr.color.a - (Time.deltaTime * colorlosingspeed));
    }
    void UpdateWaterState()
    {
        if (isInWater && currentWater != null)
        {
            float playerTop = transform.position.y + waterSurfaceOffset;
            isUnderwater = playerTop < currentWater.GetWaterSurfaceY(transform.position.x); ;
        }
        else
        {
            isUnderwater = false;
        }
    }
    public void EnterWater(WaterBody water)
    {
        isInWater = true;
        currentWater = water;

        if (statemachine.currentstate != swimstate &&
            statemachine.currentstate != divestate &&
            statemachine.currentstate != diestate)
        {
            statemachine.changestate(swimstate);
        }

        Debug.Log("进入水中");
    }

    public void ExitWater()
    {
        isInWater = false;
        currentWater = null;

        Debug.Log("离开水中");
    }

    /// <summary>
    /// 检测是否在水面
    /// </summary>
    public bool IsAtWaterSurface()
    {
        if (!isInWater || currentWater == null) return false;

        float waterSurface = currentWater.GetWaterSurfaceY(transform.position.x);
        float playerTop = transform.position.y + 0.5f; // 玩家头部位置

        return playerTop >= waterSurface;
    }


}
