using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 主菜单控制器
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmPanel;
    public Material curtainMat;
    [Header("Confirm Panel")]
    [SerializeField] private Text confirmText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Settings Panel")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button deleteSaveButton;

    [Header("Continue Button Text")]
    [SerializeField] private Text continueButtonText;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip menuMusic;

    private string targetSceneName;

    private void Start()
    {
        InitializeUI();
        PlayMenuMusic();
    }

    private void InitializeUI()
    {
        newGameButton.onClick.AddListener(OnNewGameClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(OnSettingsBackClicked);
        if (deleteSaveButton != null)
            deleteSaveButton.onClick.AddListener(OnDeleteSaveClicked);

        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(OnConfirmYes);
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(OnConfirmNo);

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.Instance != null ? AudioManager.Instance.musicVolume : 0.5f;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance != null ? AudioManager.Instance.sfxVolume : 0.8f;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        ShowMainPanel();
    }

    private void PlayMenuMusic()
    {
        if (AudioManager.Instance != null && menuMusic != null)
        {
            AudioManager.Instance.PlayMusic(menuMusic);
        }
    }


    #region Button Callbacks

    private void OnNewGameClicked()
    {
        PlayButtonClickSFX();

        if (GameProgress.HasSaveData())
        {
                GameProgress.DeleteSave();
                LoadScene("Level0");
        }
        else
        {
            LoadScene("Level0");
        }
    }

    private void OnContinueClicked()
    {
        PlayButtonClickSFX();

        if (GameProgress.HasSaveData())
        {
            string levelName = GameProgress.GetLastLevel();
            LoadScene(levelName);
        }
        else
        {
            LoadScene("level0");
        }
    }

    private void OnSettingsClicked()
    {
        PlayButtonClickSFX();
        ShowSettingsPanel();
    }

    public void OnQuitClicked()
    {
        PlayButtonClickSFX();
        QuitGame();

    }

    private void OnSettingsBackClicked()
    {
        PlayButtonClickSFX();
        ShowMainPanel();
    }

    private void OnDeleteSaveClicked()
    {
        PlayButtonClickSFX();

        ShowConfirmPanel("确定要删除所有存档吗？\n此操作无法撤销！", () =>
        {
            GameProgress.DeleteSave();
            ShowMainPanel();
        });
    }

    private void OnConfirmYes()
    {
        PlayButtonClickSFX();
        // 由 ShowConfirmPanel 传入的回调执行
    }

    private void OnConfirmNo()
    {
        PlayButtonClickSFX();
        ShowMainPanel();
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    #endregion

    #region Panel Management

    private void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    private void ShowSettingsPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    private void ShowConfirmPanel(string message, System.Action onConfirm)
    {
        if (confirmPanel == null) return;

        if (confirmText != null)
            confirmText.text = message;

        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        confirmPanel.SetActive(true);

        // 临时绑定确认回调
        confirmYesButton.onClick.RemoveAllListeners();
        confirmYesButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
        });
        confirmYesButton.onClick.AddListener(PlayButtonClickSFX);

        confirmNoButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.AddListener(OnConfirmNo);
    }

    #endregion

    #region Scene Loading

    private void LoadScene(string sceneName)
    {
        targetSceneName = sceneName;
        StartCoroutine(LoadSceneWithFade(sceneName));
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        // 淡出
        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeOut());
        }

        // 淡出音乐
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutMusic(fadeDuration);
        }

        yield return new WaitForSeconds(fadeDuration);

        // 加载场景
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        fadeCanvasGroup.alpha = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    #endregion

    #region Utility

    private void PlayButtonClickSFX()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSFX();
        }
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion
}