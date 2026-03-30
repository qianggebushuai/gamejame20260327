using UnityEngine;
using UnityEngine.SceneManagement;

public class ImageMover : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("开始移动前的等待时间（秒）")]
    public float startDelay = 1f;

    [Tooltip("正常移动速度")]
    public float normalSpeed = 2f;

    [Tooltip("加速后的移动速度")]
    public float boostSpeed = 8f;

    [Header("目标位置")]
    [Tooltip("目标位置的Y坐标")]
    public float targetY = 10f;

    [Header("到达后设置")]
    [Tooltip("到达目标后的等待时间（秒）")]
    public float endDelay = 1f;

    [Header("场景跳转")]
    [Tooltip("目标场景的名称")]
    public string targetSceneName;

    // 状态管理
    private enum State { WaitingToStart, Moving, WaitingAtTarget, Transitioning }
    private State currentState = State.WaitingToStart;

    private float waitTimer = 0f;
    private bool hasTriggered = false;

    void Start()
    {
        waitTimer = startDelay;

        if (startDelay > 0)
        {
            Debug.Log($"等待 {startDelay} 秒后开始移动... (按空格键加速)");
        }
    }

    void Update()
    {
        if (hasTriggered) return;

        // 全局ESC跳过
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TriggerSceneChange();
            return;
        }

        switch (currentState)
        {
            case State.WaitingToStart:
                HandleStartDelay();
                break;

            case State.Moving:
                HandleMovement();
                break;

            case State.WaitingAtTarget:
                HandleEndDelay();
                break;
        }
    }

    // 处理开始前的等待
    private void HandleStartDelay()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            currentState = State.Moving;
            Debug.Log($"开始移动！按住空格键加速 (正常速度:{normalSpeed}, 加速:{boostSpeed})");
        }
    }

    // 处理移动逻辑
    private void HandleMovement()
    {
        // 检测是否按住空格键加速
        float currentSpeed = Input.GetKey(KeyCode.Space) ? boostSpeed : normalSpeed;

        // 向上移动
        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(currentPos.x, targetY, currentPos.z);

        transform.position = Vector3.MoveTowards(currentPos, targetPos, currentSpeed * Time.deltaTime);

        // 检查是否到达目标
        bool reachedTarget = Mathf.Abs(transform.position.y - targetY) < 0.01f;

        if (reachedTarget)
        {
            currentState = State.WaitingAtTarget;
            waitTimer = endDelay;
            Debug.Log($"到达目标！等待 {endDelay} 秒后跳转... (按ESC立即跳转)");
        }
    }

    // 处理到达后的等待
    private void HandleEndDelay()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            TriggerSceneChange();
        }
    }

    // 执行场景跳转
    private void TriggerSceneChange()
    {
        if (hasTriggered) return;

        hasTriggered = true;
        currentState = State.Transitioning;

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("目标场景名称未设置！请在Inspector中配置 Target Scene Name");
            enabled = false; // 禁用脚本防止重复报错
            return;
        }

        Debug.Log($"跳转到场景: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }

    // 可视化辅助
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Vector3 targetPos = new Vector3(transform.position.x, targetY, transform.position.z);
        Gizmos.DrawLine(transform.position, targetPos);
        Gizmos.DrawSphere(targetPos, 0.2f);
    }
}