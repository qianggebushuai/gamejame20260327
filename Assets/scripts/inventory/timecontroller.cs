using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // 【新增】必须引入这个命名空间才能使用 Slider
using UnityEngine.SceneManagement;

public class timecontroller : MonoBehaviour
{
    public static timecontroller instance;

    [Header("玩家引用 (用于获取氧气)")]
    public Player1 player; // 你的玩家脚本名称，确保与你的实际类名一致

    [Header("UI 滑动条设置")]
    [Tooltip("时间进度条 (填入 UI Slider)")]
    public Slider timeSlider;
    [Tooltip("氧气进度条 (填入 UI Slider)")]
    public Slider oxygenSlider;

    [Header("倒计时设置")]
    public float totalTime = 60f;
    public float currentTime;
    public bool isCountingDown = false;

    // 【修改】将时间改为 int 类型，避免浮点数四舍五入导致的显示错误
    private int minute;
    private int second;

    public TextMeshProUGUI textminute;
    public TextMeshProUGUI textsecond;

    [Header("倒计时状态")]
    public bool isPaused = false;

    public delegate void CountdownFinished();
    public event CountdownFinished OnCountdownFinished;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        currentTime = totalTime;
    }

    void Update()
    {
        // ================= 1. 时间逻辑与时间滑动条 =================
        if (isCountingDown && !isPaused)
        {
            currentTime -= Time.deltaTime;

            // 【修复】使用 Mathf.FloorToInt 向下取整，防止 1.9秒被四舍五入成 2秒
            minute = Mathf.FloorToInt(currentTime / 60);
            second = Mathf.FloorToInt(currentTime % 60);

            // 更新文本
            if (textminute != null)
            {
                textminute.text = minute < 10 ? "0" + minute.ToString() : minute.ToString();
            }
            if (textsecond != null)
            {
                textsecond.text = second < 10 ? "0" + second.ToString() : second.ToString();
            }

            // 【新增】更新时间滑动条 (比例 0~1)
            if (timeSlider != null)
            {
                // 计算比例：当前时间 / 总时间
                timeSlider.value = currentTime / totalTime;
            }

            // 倒计时结束
            if (currentTime <= 0)
            {
                currentTime = 0;
                isCountingDown = false;

                if (timeSlider != null) timeSlider.value = 0f; // 确保彻底归零

                OnCountdownFinished?.Invoke();
                Debug.Log("倒计时结束！");
            }
        }

        // ================= 2. 玩家氧气滑动条 =================
        UpdateOxygenSlider();
    }

    /// <summary>
    /// 更新氧气条显示
    /// </summary>
    private void UpdateOxygenSlider()
    {
        // 确保玩家引用和滑动条都不为空
        if (player != null && oxygenSlider != null)
        {
            // 防止除以0的报错
            if (player.maxoxegenvalue > 0)
            {
                // 计算比例：当前氧气 / 最大氧气 (结果自动在 0~1 之间)
                oxygenSlider.value = player.currentoxegenvalue / player.maxoxegenvalue;
            }
        }
        else
        {
            // 如果跨场景导致 player 丢失，尝试重新自动获取一下
            if (player == null)
            {
                player = FindObjectOfType<Player1>();
            }
        }
    }

    // ================= 下面是你原有的公共方法 (保持不变) =================

    public void StartCountdown()
    {
        isCountingDown = true;
        isPaused = false;
    }

    public void StartCountdown(float time)
    {
        totalTime = time;
        currentTime = time;
        isCountingDown = true;
        isPaused = false;
    }

    public void PauseCountdown()
    {
        isPaused = true;
    }

    public void ResumeCountdown()
    {
        isPaused = false;
    }

    public void StopCountdown()
    {
        isCountingDown = false;
        isPaused = false;
    }

    public void ResetCountdown()
    {
        currentTime = totalTime;
        isCountingDown = false;
        isPaused = false;
    }

    public void AddTime(float time)
    {
        currentTime += time;
    }

    public void ReduceTime(float time)
    {
        currentTime -= time;
        if (currentTime < 0)
            currentTime = 0;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public float GetProgress()
    {
        return 1 - (currentTime / totalTime);
    }

    public bool IsFinished()
    {
        return currentTime <= 0;
    }

    public bool IsCountingDown()
    {
        return isCountingDown;
    }
}