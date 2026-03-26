using UnityEngine;

public class ScreenCoverTransition2D : MonoBehaviour
{
    [Header("按键绑定")]
    [Tooltip("第一种运动模式（旋转缩放）的按键")]
    public KeyCode mode1Key = KeyCode.G;
    [Tooltip("第二种运动模式（倾斜平移覆盖）的按键")]
    public KeyCode mode2Key = KeyCode.H;

    [Header("模式1 - 旋转缩放模式")]
    [Tooltip("基础缩放速度倍率，实际速度 = 基础速度 × 当前大小")]
    public float mode1BaseScaleSpeed = 2f;
    [Tooltip("打开时的旋转速度（度/秒）")]
    public float mode1OpenRotateSpeed = 360f;
    [Tooltip("关闭时的旋转速度（度/秒）")]
    public float mode1CloseRotateSpeed = 360f;

    [Header("模式2 - 倾斜平移覆盖模式")]
    [Tooltip("倾斜角度（度），建议45度")]
    public float mode2TiltAngle = 45f;
    [Tooltip("打开时的移动速度（世界单位/秒）")]
    public float mode2OpenMoveSpeed = 10f;
    [Tooltip("关闭时的移动速度（世界单位/秒）")]
    public float mode2CloseMoveSpeed = 10f;

    private enum State { Idle, Opening, Closing, Covered }
    private enum Mode { None, Mode1, Mode2 }

    private State currentState = State.Idle;
    private Mode currentMode = Mode.None;

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    private float screenHeightWorld;
    private float screenWidthWorld;
    private Vector3 screenCenter;
    private float screenDiagonal;

    private float mode1TargetScale;
    private float mode1CurrentRotation;

    private Vector3 mode2StartPos;
    private Vector3 mode2CoverPos;
    private Vector3 mode2CurrentPos;
    private float mode2CoverScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("此脚本必须挂载在带有SpriteRenderer的2D物体上！");
            enabled = false;
            return;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("场景中必须有主摄像机（Main Camera）！");
            enabled = false;
            return;
        }


        InitMode2Open();
        InitMode1Close();
        InitMode2Close();
        InitMode1Open();

    }

    void CalculateScreenBounds()
    {
        screenHeightWorld = 2f * mainCamera.orthographicSize;
        screenWidthWorld = screenHeightWorld * mainCamera.aspect;
        screenCenter = mainCamera.transform.position;
        screenCenter.z = 0;
        screenDiagonal = Mathf.Sqrt(screenWidthWorld * screenWidthWorld + screenHeightWorld * screenHeightWorld);
    }

    void Update()
    {
        // 处理动画
        if (currentState == State.Opening || currentState == State.Closing)
        {
            switch (currentMode)
            {
                case Mode.Mode1:
                    UpdateMode1();
                    break;
                case Mode.Mode2:
                    UpdateMode2();
                    break;
            }
        }

        // 检测按键 - 这个不能漏！
        CheckInput();
    }

    void CheckInput()
    {
        if (Input.GetKeyDown(mode1Key))
        {
            TriggerMode1();
        }

        if (Input.GetKeyDown(mode2Key))
        {
            TriggerMode2();
        }
    }

    public void TriggerMode1()
    {
        if (IsUpdating()) return;

        if (currentState == State.Idle)
        {
            StartOpen(Mode.Mode1);
        }
        else if (currentState == State.Covered)
        {
            StartClose(Mode.Mode1);
        }
    }

    public void TriggerMode2()
    {
        if (IsUpdating()) return;

        if (currentState == State.Idle)
        {
            StartOpen(Mode.Mode2);
        }
        else if (currentState == State.Covered)
        {
            StartClose(Mode.Mode2);
        }
    }

    void StartOpen(Mode mode)
    {
        currentMode = mode;
        currentState = State.Opening;
        spriteRenderer.enabled = true;

        switch (mode)
        {
            case Mode.Mode1:
                InitMode1Open();
                break;
            case Mode.Mode2:
                InitMode2Open();
                break;
        }
    }

    void StartClose(Mode mode)
    {
        currentMode = mode;
        currentState = State.Closing;

        switch (mode)
        {
            case Mode.Mode1:
                InitMode1Close();
                break;
            case Mode.Mode2:
                InitMode2Close();
                break;
        }
    }

    void InitMode1Open()
    {
        CalculateScreenBounds();

        float spriteWidth = spriteRenderer.sprite.bounds.size.x;
        float spriteHeight = spriteRenderer.sprite.bounds.size.y;
        float scaleX = screenWidthWorld / spriteWidth;
        float scaleY = screenHeightWorld / spriteHeight;
        mode1TargetScale = Mathf.Max(scaleX, scaleY) * 1.2f;

        transform.position = screenCenter;
        transform.localScale = Vector3.one * 0.01f;
        mode1CurrentRotation = 0f;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void InitMode1Close()
    {

        CalculateScreenBounds();

        transform.position = screenCenter;
        transform.localScale = Vector3.one * mode1TargetScale;
        transform.rotation = Quaternion.Euler(0, 0, mode1CurrentRotation);
    }

    public void UpdateMode1()
    {
        if (currentState == State.Opening)
        {
            float currentScale = transform.localScale.x;
            float dynamicSpeed = mode1BaseScaleSpeed * (0.5f + currentScale);
            float newScale = Mathf.MoveTowards(currentScale, mode1TargetScale, dynamicSpeed * Time.deltaTime);

            mode1CurrentRotation += mode1OpenRotateSpeed * Time.deltaTime;

            transform.localScale = Vector3.one * newScale;
            transform.rotation = Quaternion.Euler(0, 0, mode1CurrentRotation);

            if (Mathf.Abs(newScale - mode1TargetScale) < 0.01f)
            {
                FinishOpen();
            }
        }
        else
        {
            float currentScale = transform.localScale.x;
            float dynamicSpeed = mode1BaseScaleSpeed * (0.5f + currentScale);
            float newScale = Mathf.MoveTowards(currentScale, 0f, dynamicSpeed * Time.deltaTime);

            mode1CurrentRotation += mode1CloseRotateSpeed * Time.deltaTime;

            transform.localScale = Vector3.one * newScale;
            transform.rotation = Quaternion.Euler(0, 0, mode1CurrentRotation);

            if (newScale <= 0.01f)
            {
                FinishClose();
            }
        }
    }

    float CalculateTiltedCoverScale(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Abs(Mathf.Cos(rad));
        float sin = Mathf.Abs(Mathf.Sin(rad));

        float spriteSize = spriteRenderer.sprite.bounds.size.x;
        float rotatedSize = spriteSize * (cos + sin);

        float scaleX = screenWidthWorld / rotatedSize;
        float scaleY = screenHeightWorld / rotatedSize;

        return Mathf.Max(scaleX, scaleY) * 1.2f;
    }

    void CalculateMode2Params()
    {
        mode2CoverScale = CalculateTiltedCoverScale(mode2TiltAngle) * 1.4f;

        float spriteWorldSize = spriteRenderer.sprite.bounds.size.x * mode2CoverScale;
        float rad = mode2TiltAngle * Mathf.Deg2Rad;
        float cos = Mathf.Abs(Mathf.Cos(rad));
        float sin = Mathf.Abs(Mathf.Sin(rad));
        float aabbSize = spriteWorldSize * (cos + sin) * 1f;

        Vector3 moveDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;
        mode2CoverPos = screenCenter;
        float offset = screenDiagonal * 0.44f + aabbSize * 0.44f;
        mode2StartPos = screenCenter - moveDir * offset;
    }

    void InitMode2Open()
    {
        CalculateScreenBounds();
        CalculateMode2Params();

        transform.rotation = Quaternion.Euler(0, 0, mode2TiltAngle);
        transform.localScale = Vector3.one * mode2CoverScale;
        transform.position = mode2StartPos;
        mode2CurrentPos = mode2StartPos;
    }

    void InitMode2Close()
    {
        CalculateScreenBounds();
        CalculateMode2Params();
        transform.rotation = Quaternion.Euler(0, 0, mode2TiltAngle);
        transform.localScale = Vector3.one * mode2CoverScale;
        transform.position = mode2CoverPos;
        mode2CurrentPos = mode2CoverPos;
    }

    public void UpdateMode2()
    {
        if (currentState == State.Opening)
        {
            mode2CurrentPos = Vector3.MoveTowards(mode2CurrentPos, mode2CoverPos, mode2OpenMoveSpeed * Time.deltaTime);
            transform.position = mode2CurrentPos;

            if (Vector3.Distance(mode2CurrentPos, mode2CoverPos) < 0.1f)
            {
                FinishOpen();
            }
        }
        else
        {
            mode2CurrentPos = Vector3.MoveTowards(mode2CurrentPos, mode2StartPos, mode2CloseMoveSpeed * Time.deltaTime);
            transform.position = mode2CurrentPos;

            if (Vector3.Distance(mode2CurrentPos, mode2StartPos) < 0.1f)
            {
                FinishClose();
            }
        }
    }

    void FinishOpen()
    {
        currentState = State.Covered;
        currentMode = Mode.None;
    }

    void FinishClose()
    {
        currentState = State.Idle;
        currentMode = Mode.None;
        spriteRenderer.enabled = false;
    }

    public bool IsUpdating()
    {
        return currentState == State.Opening || currentState == State.Closing;
    }

    public int IsOverlap()
    {
        if (IsUpdating()) return -1;
        if (currentState == State.Covered) return 1;
        return 0;
    }

    public void ForceReset()
    {
        currentState = State.Idle;
        currentMode = Mode.None;
        spriteRenderer.enabled = false;
        mode1CurrentRotation = 0f;
    }
}