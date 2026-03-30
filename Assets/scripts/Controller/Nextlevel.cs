using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 必须引用UI命名空间
using System.Collections;

public class LevelExitTrigger : MonoBehaviour
{
    [Header("过关设置")]
    [SerializeField] private string nextSceneName; 
    [SerializeField] private float fadeDuration = 1f; 

    private bool isTriggered = false; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            Debug.Log("准备前往下一关: " + nextSceneName);

            string sceneToLoad = string.IsNullOrEmpty(nextSceneName) ? SceneManager.GetActiveScene().name : nextSceneName;

            AutoFadeTransition.StartTransition(sceneToLoad, fadeDuration);
        }
    }
}


public class AutoFadeTransition : MonoBehaviour
{
    private string targetScene;
    private float duration;
    private Image fadeImage;


    public static void StartTransition(string sceneName, float time)
    {
        GameObject fadeObj = new GameObject("LevelTransitionFader");

        DontDestroyOnLoad(fadeObj);

        Canvas canvas = fadeObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // 设为极高，保证遮住所有游戏画面和 UI

        Image img = fadeObj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0); 
        img.raycastTarget = true; 

        // 4. 挂载本脚本，并传递参数
        AutoFadeTransition fader = fadeObj.AddComponent<AutoFadeTransition>();
        fader.targetScene = sceneName;
        fader.duration = time;
        fader.fadeImage = img;
    }

    private void Start()
    {
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        Color c = fadeImage.color;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; 
            c.a = Mathf.Lerp(0f, 1f, timer / duration);
            fadeImage.color = c;
            yield return null; 
        }

        c.a = 1f;
        fadeImage.color = c;


        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return null;

        timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(1f, 0f, timer / duration);
            fadeImage.color = c;
            yield return null;
        }

        Destroy(gameObject);
    }
}