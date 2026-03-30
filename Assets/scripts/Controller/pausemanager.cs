using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI 面板设置")]
    [Tooltip("按下ESC呼出的菜单面板 (如：包含继续、返回主界面按钮的Panel)")]
    public GameObject pausePanel;

    [Tooltip("用于全屏变黑的图像 (需要一张占满全屏的Image)")]
    public Image blackScreenImage;

    [Header("场景与过渡设置")]
    [Tooltip("主界面的场景名称 (必须与 Build Settings 中一致)")]
    public string mainMenuSceneName = "topic";
    [Tooltip("黑屏渐变持续时间 (秒)")]
    public float fadeDuration = 1f;

    // 内部状态
    private bool isPaused = false;
    private bool isFading = false; // 防止在渐变时玩家乱按

    void Start()
    {
        // 游戏开始时，确保菜单隐藏，黑屏透明且不挡鼠标
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (blackScreenImage != null)
        {
            Color c = blackScreenImage.color;
            c.a = 0f;
            blackScreenImage.color = c;
            blackScreenImage.raycastTarget = false;
        }

        // 确保时间流速正常
        Time.timeScale = 1f;
    }

    void Update()
    {
        // 如果正在播放黑屏切换动画，禁止玩家按ESC
        if (isFading) return;

        // 按下 ESC 键切换菜单状态
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePauseMenu();
        }
    }

    /// <summary>
    /// 打开或关闭暂停菜单
    /// </summary>
    public void TogglePauseMenu()
    {
        isPaused = !isPaused;

        // 显示或隐藏面板
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        // 暂停或恢复游戏时间 (0为暂停，1为正常)
        Time.timeScale = isPaused ? 0f : 1f;
    }

    /// <summary>
    /// 按钮绑定函数：继续游戏
    /// </summary>
    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePauseMenu();
        }
    }

    /// <summary>
    /// 按钮绑定函数：返回主界面 (会先黑屏再切场景)
    /// </summary>
    public void QuitToMainMenu()
    {
        if (!isFading)
        {
            StartCoroutine(FadeToBlackAndLoadScene());
        }
    }

    /// <summary>
    /// 黑屏渐变并加载场景的协程
    /// </summary>
    private IEnumerator FadeToBlackAndLoadScene()
    {
        isFading = true;

        if (blackScreenImage != null)
        {
            // 开启射线检测，变黑期间阻挡玩家点击其他UI
            blackScreenImage.raycastTarget = true;

            float timer = 0f;
            Color color = blackScreenImage.color;

            while (timer < fadeDuration)
            {
                // 注意：因为此时 Time.timeScale 可能是 0，必须用 unscaledDeltaTime 才能计秒！
                timer += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                blackScreenImage.color = color;

                yield return null;
            }

            // 确保完全变黑
            color.a = 1f;
            blackScreenImage.color = color;
        }

        // 切场景前，务必把时间流速恢复为 1，否则到了主界面游戏也是卡死的！
        Time.timeScale = 1f;

        // 加载主界面场景
        SceneManager.LoadScene(mainMenuSceneName);
    }
}