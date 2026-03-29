using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BUTTONpause : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject buttonpanel;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSFX;

    private bool iscallup = false;

    // 运行时查找的按钮引用
    private Button resumeButton;
    private Button restartButton;
    private Button mainMenuButton;
    private Button quitButton;

    void Start()
    {
        buttonpanel.SetActive(false);
        Time.timeScale = 1f;

        // 自动查找按钮
        FindButtons();
    }

    /// <summary>
    /// 自动查找暂停面板中的按钮
    /// </summary>
    private void FindButtons()
    {
        if (buttonpanel == null)
        {
            Debug.LogError("buttonpanel 未设置！");
            return;
        }

        // 获取面板中的所有按钮
        Button[] buttons = buttonpanel.GetComponentsInChildren<Button>(true);

        Debug.Log("找到 " + buttons.Length + " 个按钮");

        foreach (Button btn in buttons)
        {
            string btnName = btn.gameObject.name.ToLower();
            Debug.Log("按钮: " + btn.gameObject.name);

            if (btnName.Contains("resume") || btnName.Contains("continue") || btnName.Contains("继续"))
            {
                resumeButton = btn;
                Debug.Log(">>> 设置为 ResumeButton");
            }
            else if (btnName.Contains("restart") || btnName.Contains("retry") || btnName.Contains("重启") || btnName.Contains("重新"))
            {
                restartButton = btn;
                Debug.Log(">>> 设置为 RestartButton");
            }
            else if (btnName.Contains("menu") || btnName.Contains("main") || btnName.Contains("主菜单") || btnName.Contains("返回"))
            {
                mainMenuButton = btn;
                Debug.Log(">>> 设置为 MainMenuButton");
            }
            else if (btnName.Contains("quit") || btnName.Contains("exit") || btnName.Contains("退出"))
            {
                quitButton = btn;
                Debug.Log(">>> 设置为 QuitButton");
            }
        }

        // 绑定点击事件
        BindButtonEvents();
    }

    /// <summary>
    /// 绑定按钮事件
    /// </summary>
    private void BindButtonEvents()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
            Debug.Log("ResumeButton 事件已绑定");
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
            Debug.Log("RestartButton 事件已绑定");
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
            Debug.Log("MainMenuButton 事件已绑定");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            calluppause();
        }

        // 暂停时手动检测点击
        if (iscallup && Input.GetMouseButtonDown(0))
        {
            CheckButtonClick();
        }
    }

    private void CheckButtonClick()
    {
        if (EventSystem.current == null) return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            // 获取点击物体或其父物体上的 Button
            Button clickedButton = result.gameObject.GetComponent<Button>();
            if (clickedButton == null)
            {
                clickedButton = result.gameObject.GetComponentInParent<Button>();
            }

            if (clickedButton != null)
            {
                // 直接调用按钮的 onClick 事件
                clickedButton.onClick.Invoke();
                Debug.Log("触发按钮: " + clickedButton.gameObject.name);
                return;
            }
        }
    }

    public void calluppause()
    {
        if (iscallup)
        {
            buttonpanel.SetActive(false);
            Time.timeScale = 1f;
            iscallup = false;
            Cursor.visible = false;
            Debug.Log("关闭暂停");
        }
        else
        {
            buttonpanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.visible = true;
            iscallup = true;
            Debug.Log("打开暂停");
        }
    }

    public void ResumeGame()
    {
        Debug.Log("=== ResumeGame 执行 ===");
        PlayButtonSound();

        buttonpanel.SetActive(false);
        Time.timeScale = 1f;
        iscallup = false;
        Cursor.visible = false;
    }

    public void RestartGame()
    {
        Debug.Log("=== RestartGame 执行 ===");
        PlayButtonSound();

        Time.timeScale = 1f;
        iscallup = false;

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void GoToMainMenu()
    {
        PlayButtonSound();

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
        iscallup = false;


    }

    public void QuitGame()
    {
        Debug.Log("=== QuitGame 执行 ===");
        PlayButtonSound();

        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayButtonSound()
    {
        if (buttonClickSFX != null)
        {
            AudioSource.PlayClipAtPoint(buttonClickSFX, Camera.main.transform.position);
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}