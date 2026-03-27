using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class timecontroller : MonoBehaviour
{
    public static timecontroller instance; 

    [Header("倒计时设置")]
    public float totalTime = 60f; 
    private float currentTime; 
    public bool isCountingDown = false;
    public float second;
    public float minute;
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
        if (isCountingDown && !isPaused)
        {
            currentTime -= Time.deltaTime;
            minute = currentTime / 60;
            second = currentTime % 60;
            if (textminute != null)
            {
                if (minute >= 10)
                {
                    textminute.text = minute.ToString("F0");
                }
                else
                {
                    textminute.text = "0"+minute.ToString("F0");
                }

            }
            if(textsecond != null)
            {
                if (second >= 10)
                {
                    textsecond.text = second.ToString("F0");
                }
                else
                {
                    textsecond.text = "0"+second.ToString("F0");
                }

            }

            if (currentTime <= 0)
            {
                currentTime = 0;
                isCountingDown = false;
                OnCountdownFinished?.Invoke(); // 触发倒计时结束事件
                Debug.Log("倒计时结束！");
            }
        }
    }

    // 开始倒计时
    public void StartCountdown()
    {
        isCountingDown = true;
        isPaused = false;
    }

    // 开始倒计时（自定义时间）
    public void StartCountdown(float time)
    {
        totalTime = time;
        currentTime = time;
        isCountingDown = true;
        isPaused = false;
    }

    // 暂停倒计时
    public void PauseCountdown()
    {
        isPaused = true;
    }

    // 恢复倒计时
    public void ResumeCountdown()
    {
        isPaused = false;
    }

    // 停止倒计时
    public void StopCountdown()
    {
        isCountingDown = false;
        isPaused = false;
    }

    // 重置倒计时
    public void ResetCountdown()
    {
        currentTime = totalTime;
        isCountingDown = false;
        isPaused = false;
    }

    // 添加时间
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

    // 获取格式化时间字符串 (分:秒)
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // 获取格式化时间字符串 (时:分:秒)
    public string GetFormattedTimeWithHours()
    {
        int hours = Mathf.FloorToInt(currentTime / 3600);
        int minutes = Mathf.FloorToInt((currentTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    // 获取进度百分比 (0-1)
    public float GetProgress()
    {
        return 1 - (currentTime / totalTime);
    }

    // 检查是否倒计时结束
    public bool IsFinished()
    {
        return currentTime <= 0;
    }

    // 检查是否正在倒计时
    public bool IsCountingDown()
    {
        return isCountingDown;
    }
}
