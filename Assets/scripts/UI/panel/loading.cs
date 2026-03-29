using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenFader : MonoBehaviour
{
    [Header("UI元素配置")]
    public Image[] loadingImage;
    public bool allowClickToSkip = true;

    [Header("动画时长配置（秒）")]
    public float fadeInDuration = 1.5f;
    public float holdDuration = 3f;
    public float fadeOutDuration = 1.5f;

    [Header("场景配置")]
    public string targetSceneName = "MainMenu";

    private CanvasGroup[] canvasGroups; // 改为数组
    private bool isAnimationFinished = false;

    void Start()
    {
        // 初始化数组
        canvasGroups = new CanvasGroup[loadingImage.Length];

        for (int i = 0; i < loadingImage.Length; i++)
        {
            // 获取或添加CanvasGroup组件
            canvasGroups[i] = loadingImage[i].GetComponent<CanvasGroup>();
            if (canvasGroups[i] == null)
            {
                canvasGroups[i] = loadingImage[i].gameObject.AddComponent<CanvasGroup>();
            }

            // 初始化透明度
            canvasGroups[i].alpha = 0f;
            loadingImage[i].gameObject.SetActive(true);
        }

        StartCoroutine(FadeInOutSequence());
    }

    void Update()
    {
        if (allowClickToSkip && !isAnimationFinished && Input.GetMouseButtonDown(0))
        {
            StopAllCoroutines();
            // 隐藏所有图片
            foreach (var cg in canvasGroups)
            {
                cg.alpha = 0f;
            }
            isAnimationFinished = true;
            LoadTargetScene();
        }
    }

    IEnumerator FadeInOutSequence()
    {
        // 依次播放每张图片的淡入淡出
        for (int i = 0; i < canvasGroups.Length; i++)
        {
            // 淡入
            yield return StartCoroutine(FadeImage(canvasGroups[i], 0f, 1f, fadeInDuration));
            // 停留
            yield return new WaitForSeconds(holdDuration);
            // 淡出
            yield return StartCoroutine(FadeImage(canvasGroups[i], 1f, 0f, fadeOutDuration));
        }

        isAnimationFinished = true;
        LoadTargetScene();
    }

    IEnumerator FadeImage(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cg.alpha = endAlpha;
    }

    void LoadTargetScene()
    {
        if (SceneUtility.GetBuildIndexByScenePath(targetSceneName) != -1)
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"错误：场景 {targetSceneName} 未添加到Build Settings中！");
        }
    }
}