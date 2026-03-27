using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScreenCoverTransition2D : MonoBehaviour
{
    [Header("按键绑定")]
    public KeyCode mode1Key = KeyCode.G;
    public KeyCode mode2Key = KeyCode.H;

    [Header("高亮预览设置")]
    [Tooltip("要高亮的目标图层/物体")]
    public SpriteRenderer[] opentargetLayersToHighlight;
    public SpriteRenderer[] closetargetLayersToHighlight;

    [Tooltip("高亮持续时间（秒）")]
    public float highlightDuration = 1.5f;
    [Tooltip("高亮边缘颜色")]
    public Color highlightColor = new Color(1f, 0.9f, 0f, 1f);
    [Tooltip("高亮边缘宽度")]
    public float highlightWidth = 0.05f;
    [Tooltip("高亮闪烁速度")]
    public float highlightBlinkSpeed = 5f;

    [Header("模式1 - 旋转缩放模式")]
    public float mode1BaseScaleSpeed = 2f;
    public float mode1OpenRotateSpeed = 360f;
    public float mode1CloseRotateSpeed = 360f;

    [Header("模式2 - 倾斜平移覆盖模式")]
    public float mode2TiltAngle = 45f;
    public float mode2OpenMoveSpeed = 10f;
    public float mode2CloseMoveSpeed = 10f;
    public static ScreenCoverTransition2D instance;
    private enum State { Idle, Highlighting, Opening, Closing, Covered }
    private enum Mode { None, Mode1, Mode2 }

    private State currentState = State.Idle;
    private State previewState = State.Idle;
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

    private List<LineRenderer> outlineRenderers = new List<LineRenderer>();
    private Mode pendingMode = Mode.None;
    GameObject player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
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

        spriteRenderer.enabled = false;
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
        if (player != null && currentState == State.Idle)
        {
            transform.position = player.transform.position;
        }

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
        if (IsUpdating() || currentState == State.Highlighting) return;

        if (currentState == State.Idle)
        {
            StartHighlightThenOpen(Mode.Mode1);
        }
        else if (currentState == State.Covered)
        {
            StartHighlightThenClose(Mode.Mode1);
        }
    }

    public void TriggerMode2()
    {
        if (IsUpdating() || currentState == State.Highlighting) return;

        if (currentState == State.Idle)
        {
            StartHighlightThenOpen(Mode.Mode2);
        }
        else if (currentState == State.Covered)
        {
            StartHighlightThenClose(Mode.Mode2);
        }
    }

    #region 边缘高亮

    void StartHighlightThenOpen(Mode mode)
    {
        pendingMode = mode;

        if (opentargetLayersToHighlight == null || opentargetLayersToHighlight.Length == 0)
        {
            StartOpen(mode);
            return;
        }
        currentState = State.Highlighting;
        StartCoroutine(HighlightCoroutineOpen());
    }
    void StartHighlightThenClose(Mode mode)
    {
        pendingMode = mode;

        if (opentargetLayersToHighlight == null || opentargetLayersToHighlight.Length == 0)
        {
            StartClose(mode);
            return;
        }
        currentState = State.Highlighting;
        StartCoroutine(HighlightCoroutineClose());
    }
    IEnumerator HighlightCoroutineOpen()
    {
        CreateOutlineHighlights();

        float elapsed = 0f;

        while (elapsed < highlightDuration)
        {
            elapsed += Time.deltaTime;
            UpdateOutlineBlink();
            yield return null;
        }

        ClearHighlights();
        StartOpen(pendingMode);
    }
    IEnumerator HighlightCoroutineClose()
    {
        CreateOutlineHighlights();

        float elapsed = 0f;

        while (elapsed < highlightDuration)
        {
            elapsed += Time.deltaTime;
            UpdateOutlineBlink();
            yield return null;
        }

        ClearHighlights();
        StartClose(pendingMode);
    }
    void CreateOutlineHighlights()
    {
        ClearHighlights();
        if(previewState == State.Idle)
        {
            foreach (SpriteRenderer target in opentargetLayersToHighlight)
            {
                if (target == null || target.sprite == null) continue;
                CreateSpriteOutline(target);
            }
        }
        else
        {
            foreach (SpriteRenderer target in closetargetLayersToHighlight)
            {
                if (target == null || target.sprite == null) continue;
                CreateSpriteOutline(target);
            }
        }

    }

    void CreateSpriteOutline(SpriteRenderer original)
    {
        Vector2[] outlinePoints = GetSpriteOutlinePoints(original);

        if (outlinePoints == null || outlinePoints.Length < 3) return;

        GameObject outlineObj = new GameObject($"Outline_{original.name}");
        outlineObj.transform.position = Vector3.zero;

        LineRenderer lr = outlineObj.AddComponent<LineRenderer>();

        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = outlinePoints.Length;
        lr.startWidth = highlightWidth;
        lr.endWidth = highlightWidth;
        lr.startColor = highlightColor;
        lr.endColor = highlightColor;
        lr.sortingOrder = original.sortingOrder + 100;
        lr.sortingLayerName = original.sortingLayerName;

        lr.material = new Material(Shader.Find("Sprites/Default"));

        Vector3[] worldPoints = new Vector3[outlinePoints.Length];
        for (int i = 0; i < outlinePoints.Length; i++)
        {
            worldPoints[i] = original.transform.TransformPoint(outlinePoints[i]);
            worldPoints[i].z = original.transform.position.z - 0.01f;
        }

        lr.SetPositions(worldPoints);
        outlineRenderers.Add(lr);
    }

    Vector2[] GetSpriteOutlinePoints(SpriteRenderer sr)
    {
        Sprite sprite = sr.sprite;

        if (sprite.GetPhysicsShapeCount() > 0)
        {
            List<Vector2> shapePoints = new List<Vector2>();
            sprite.GetPhysicsShape(0, shapePoints);

            if (shapePoints.Count > 0)
            {
                return shapePoints.ToArray();
            }
        }
        Bounds bounds = sprite.bounds;
        Vector2 min = bounds.min;
        Vector2 max = bounds.max;

        // 返回矩形的四个角点
        return new Vector2[]
        {
            new Vector2(min.x, min.y),
            new Vector2(max.x, min.y),
            new Vector2(max.x, max.y),
            new Vector2(min.x, max.y)
        };
    }

    void UpdateOutlineBlink()
    {
        float alpha = Mathf.Lerp(0.3f, 1f,
            (Mathf.Sin(Time.time * highlightBlinkSpeed * Mathf.PI) + 1f) * 0.5f);

        Color blinkColor = highlightColor;
        blinkColor.a = alpha;

        foreach (LineRenderer lr in outlineRenderers)
        {
            if (lr == null) continue;
            lr.startColor = blinkColor;
            lr.endColor = blinkColor;
        }
    }

    void ClearHighlights()
    {
        foreach (LineRenderer lr in outlineRenderers)
        {
            if (lr != null)
            {
                Destroy(lr.gameObject);
            }
        }
        outlineRenderers.Clear();
    }

    #endregion

    #region 遮罩动画系统

    void StartOpen(Mode mode)
    {
        previewState = currentState;
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
        previewState = currentState;
        currentMode = Mode.None;
    }

    void FinishClose()
    {
        currentState = State.Idle;
        previewState = currentState;
        currentMode = Mode.None;
        spriteRenderer.enabled = false;
    }

    #endregion

    public bool IsUpdating()
    {
        return currentState == State.Opening || currentState == State.Closing;
    }

    public bool IsHighlighting()
    {
        return currentState == State.Highlighting;
    }

    public int IsOverlap()
    {
        if (IsUpdating() || IsHighlighting()) return -1;
        if (currentState == State.Covered) return 1;
        return 0;
    }

    public void ForceReset()
    {
        StopAllCoroutines();
        ClearHighlights();
        currentState = State.Idle;
        currentMode = Mode.None;
        pendingMode = Mode.None;
        spriteRenderer.enabled = false;
        mode1CurrentRotation = 0f;
    }
}