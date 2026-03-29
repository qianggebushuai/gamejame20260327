using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 主菜单开场动画控制器
/// </summary>
public class MainMenuAnimator : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private List<RectTransform> otherUIElements;
    [Header("全黑渐变特效")]
    [SerializeField] private Image fullScreenBlackImage; 
    [SerializeField] private float fadeToBlackDuration = 1.5f; 
    [SerializeField] private float holdBlackScreenTime = 1f; 
    [Header("Title")]
    [SerializeField] private RectTransform titleTransform;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionForce = 1000f;
    [SerializeField] private float explosionRadius = 500f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float shrinkSpeed = 2f;

    [Header("Timing")]
    [SerializeField] private float delayBetweenExplosions = 0.15f;
    [SerializeField] private float finalExplosionDelay = 0.5f;
    [SerializeField] private float sceneLoadDelay = 1f;

    [Header("Effects")]
    [SerializeField] private GameObject explosionEffectPrefab;  // 爆炸粒子特效
    [SerializeField] private GameObject sparkEffectPrefab;      // 火花特效
    [SerializeField] private Color explosionColor = Color.white;
    public Material curtainMat;

    [Header("Screen Effects")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float whiteFlashDuration = 0.1f;

    [Header("Camera Shake")]
    [SerializeField] private float shakeIntensity = 20f;
    [SerializeField] private float shakeDuration = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioClip buttonExplodeSFX;
    [SerializeField] private AudioClip finalExplodeSFX;
    [SerializeField] private AudioClip whooshSFX;

    [Header("Scene Settings")]
    [SerializeField] private string targetScene = "Level1";

    private Canvas canvas;
    private bool isAnimating = false;

    private void Start()
    {
        if (fullScreenBlackImage != null)
        {
            fullScreenBlackImage.gameObject.SetActive(true);
            fullScreenBlackImage.color = new Color(0, 0, 0, 0); 
            fullScreenBlackImage.raycastTarget = false; 
        }
        canvas = GetComponentInParent<Canvas>();

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnNewGameClicked);
        }
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnNewGameClicked);
        }
    }

    /// <summary>
    /// 点击新游戏按钮
    /// </summary>
    public void OnNewGameClicked()
    {
        if (isAnimating) return;
        isAnimating = true;

        // 禁用所有按钮
        DisableAllButtons();

        // 开始爆炸序列
        StartCoroutine(ExplosionSequence());
    }

    /// <summary>
    /// 爆炸序列
    /// </summary>
    private IEnumerator ExplosionSequence()
    {
        // 播放开始音效
        PlaySFX(whooshSFX);

        // 收集要爆炸的按钮（除了新游戏按钮）
        List<RectTransform> buttonsToExplode = new List<RectTransform>();

        if (continueButton != null) buttonsToExplode.Add(continueButton.GetComponent<RectTransform>());
        if (settingsButton != null) buttonsToExplode.Add(settingsButton.GetComponent<RectTransform>());
        if (quitButton != null) buttonsToExplode.Add(quitButton.GetComponent<RectTransform>());

        // 添加其他 UI 元素
        if (otherUIElements != null)
        {
            buttonsToExplode.AddRange(otherUIElements);
        }

        // 标题先飞走
        if (titleTransform != null)
        {
            StartCoroutine(FlyAwayElement(titleTransform, Vector2.up * 2000f, 0.5f));
            yield return new WaitForSeconds(0.2f);
        }

        // 逐个爆炸按钮
        foreach (RectTransform button in buttonsToExplode)
        {
            if (button != null && button.gameObject.activeSelf)
            {
                StartCoroutine(ExplodeButton(button));
                PlaySFX(buttonExplodeSFX);
                ShakeCamera(shakeIntensity * 0.5f, shakeDuration * 0.5f);
                yield return new WaitForSeconds(delayBetweenExplosions);
            }
        }

        // 等待一下
        yield return new WaitForSeconds(finalExplosionDelay);

        // 新游戏按钮特殊爆炸
        if (newGameButton != null)
        {
            yield return StartCoroutine(FinalButtonExplosion(newGameButton.GetComponent<RectTransform>()));
        }

        // 白色闪光
        yield return StartCoroutine(WhiteFlash());

        // 原有淡出到黑色（可保留或替换，这里保留后叠加全屏黑幕）
        yield return StartCoroutine(FadeToBlack());
        float currentRadius = 1.5f;
        while (currentRadius > 0f)
        {
            currentRadius -= Time.deltaTime * 1f;
            SetRadius(currentRadius);
            yield return null;
        }
        SetRadius(0f);

        yield return new WaitForSeconds(2f);

        if (fullScreenBlackImage != null)
        {
            yield return StartCoroutine(FadeToFullBlack());
        }

        yield return new WaitForSeconds(sceneLoadDelay);
        SceneManager.LoadScene(targetScene);
    }

    private IEnumerator FadeToFullBlack()
    {
        if (fullScreenBlackImage == null) yield break;

        float elapsedTime = 0f;
        Color targetColor = new Color(0, 0, 0, 1); 
        Color startColor = fullScreenBlackImage.color;

        // 渐变过程
        while (elapsedTime < fadeToBlackDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeToBlackDuration;
            fullScreenBlackImage.color = Color.Lerp(startColor, targetColor, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        fullScreenBlackImage.color = targetColor;
        yield return new WaitForSeconds(holdBlackScreenTime);
    }


    /// <summary>
    /// 爆炸单个按钮
    /// </summary>
    private IEnumerator ExplodeButton(RectTransform button)
    {
        // 生成爆炸特效
        SpawnExplosionEffect(button.position);

        // 生成碎片
        SpawnFragments(button);

        // 隐藏原按钮
        button.gameObject.SetActive(false);

        yield return null;
    }

    /// <summary>
    /// 最终按钮爆炸（更华丽）
    /// </summary>
    private void SetRadius(float r)
    {
        // 这里的字符串必须和 Shader 里的属性名 "_Radius" 完全一致
        if (curtainMat != null) curtainMat.SetFloat("_Radius", r);
    }
    private IEnumerator FinalButtonExplosion(RectTransform button)
    {
        // 先放大
        float elapsed = 0f;
        float growDuration = 0.3f;
        Vector3 originalScale = button.localScale;
        Vector3 targetScale = originalScale * 1.5f;

        // 闪烁效果
        Image buttonImage = button.GetComponent<Image>();
        Color originalColor = buttonImage != null ? buttonImage.color : Color.white;

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / growDuration;

            // 放大
            button.localScale = Vector3.Lerp(originalScale, targetScale, t);

            // 闪烁
            if (buttonImage != null)
            {
                float flash = Mathf.PingPong(Time.time * 20f, 1f);
                buttonImage.color = Color.Lerp(originalColor, Color.white, flash);
            }

            yield return null;
        }

        // 播放最终爆炸音效
        PlaySFX(finalExplodeSFX);

        // 强烈相机震动
        ShakeCamera(shakeIntensity, shakeDuration);

        // 生成大量碎片
        SpawnFragments(button, 20);

        // 多个爆炸特效
        SpawnExplosionEffect(button.position);
        yield return new WaitForSeconds(0.05f);
        SpawnExplosionEffect(button.position + Vector3.left * 50f);
        SpawnExplosionEffect(button.position + Vector3.right * 50f);

        // 隐藏按钮
        button.gameObject.SetActive(false);
    }

    /// <summary>
    /// 生成碎片
    /// </summary>
    private void SpawnFragments(RectTransform source, int count = 8)
    {
        if (source == null) return;

        Image sourceImage = source.GetComponent<Image>();
        Color fragmentColor = sourceImage != null ? sourceImage.color : Color.white;

        for (int i = 0; i < count; i++)
        {
            GameObject fragment = new GameObject($"Fragment_{i}");
            fragment.transform.SetParent(canvas.transform, false);

            // 设置位置
            RectTransform fragRect = fragment.AddComponent<RectTransform>();
            fragRect.position = source.position;
            fragRect.sizeDelta = new Vector2(
                Random.Range(20f, 50f),
                Random.Range(20f, 50f)
            );

            // 添加图片
            Image fragImage = fragment.AddComponent<Image>();
            fragImage.color = fragmentColor;

            // 添加碎片行为
            UIFragment fragScript = fragment.AddComponent<UIFragment>();

            // 随机方向
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomForce = Random.Range(explosionForce * 0.5f, explosionForce);
            float randomRotation = Random.Range(-rotationSpeed, rotationSpeed);

            fragScript.Initialize(randomDir * randomForce, randomRotation, shrinkSpeed);
        }
    }

    /// <summary>
    /// 生成爆炸特效
    /// </summary>
    private void SpawnExplosionEffect(Vector3 position)
    {
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, position, Quaternion.identity, canvas.transform);
            Destroy(effect, 2f);
        }
        else
        {
            // 没有预制体时，创建简单的爆炸效果
            StartCoroutine(SimpleExplosionEffect(position));
        }
    }

    /// <summary>
    /// 简单爆炸效果（无需预制体）
    /// </summary>
    private IEnumerator SimpleExplosionEffect(Vector3 position)
    {
        // 创建扩散圆环
        GameObject ring = new GameObject("ExplosionRing");
        ring.transform.SetParent(canvas.transform, false);
        ring.transform.position = position;

        RectTransform ringRect = ring.AddComponent<RectTransform>();
        ringRect.sizeDelta = new Vector2(10f, 10f);

        Image ringImage = ring.AddComponent<Image>();
        ringImage.color = explosionColor;

        // 扩散动画
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 扩大
            float size = Mathf.Lerp(10f, 200f, t);
            ringRect.sizeDelta = new Vector2(size, size);

            // 淡出
            Color color = ringImage.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            ringImage.color = color;

            yield return null;
        }

        Destroy(ring);

        // 生成多个小粒子
        for (int i = 0; i < 12; i++)
        {
            StartCoroutine(SpawnParticle(position));
        }
    }

    /// <summary>
    /// 生成单个粒子
    /// </summary>
    private IEnumerator SpawnParticle(Vector3 position)
    {
        GameObject particle = new GameObject("Particle");
        particle.transform.SetParent(canvas.transform, false);
        particle.transform.position = position;

        RectTransform particleRect = particle.AddComponent<RectTransform>();
        float size = Random.Range(5f, 15f);
        particleRect.sizeDelta = new Vector2(size, size);

        Image particleImage = particle.AddComponent<Image>();
        particleImage.color = new Color(
            Random.Range(0.8f, 1f),
            Random.Range(0.5f, 1f),
            Random.Range(0.2f, 0.5f),
            1f
        );

        Vector2 velocity = Random.insideUnitCircle.normalized * Random.Range(200f, 500f);
        float gravity = 500f;
        float lifetime = Random.Range(0.5f, 1f);
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;

            // 移动
            velocity.y -= gravity * Time.deltaTime;
            particleRect.anchoredPosition += velocity * Time.deltaTime;

            // 缩小和淡出
            float t = elapsed / lifetime;
            float scale = Mathf.Lerp(1f, 0f, t);
            particleRect.localScale = Vector3.one * scale;

            Color color = particleImage.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            particleImage.color = color;

            yield return null;
        }

        Destroy(particle);
    }

    /// <summary>
    /// 元素飞走效果
    /// </summary>
    private IEnumerator FlyAwayElement(RectTransform element, Vector2 direction, float duration)
    {
        Vector2 startPos = element.anchoredPosition;
        Vector2 endPos = startPos + direction;
        Quaternion startRot = element.localRotation;
        Quaternion endRot = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 加速移动
            float easedT = t * t;
            element.anchoredPosition = Vector2.Lerp(startPos, endPos, easedT);
            element.localRotation = Quaternion.Lerp(startRot, endRot, t);

            // 淡出
            CanvasGroup cg = element.GetComponent<CanvasGroup>();
            if (cg == null) cg = element.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        element.gameObject.SetActive(false);
    }

    /// <summary>
    /// 白色闪光
    /// </summary>
    private IEnumerator WhiteFlash()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.white;

        float elapsed = 0f;

        // 快速显示白色
        while (elapsed < whiteFlashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / whiteFlashDuration);
            fadeImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        fadeImage.color = Color.white;
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// 淡出到黑色
    /// </summary>
    private IEnumerator FadeToBlack()
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            fadeImage.color = Color.Lerp(Color.white, Color.black, t);
            yield return null;
        }

        fadeImage.color = Color.black;
    }

    /// <summary>
    /// 相机震动
    /// </summary>
    private void ShakeCamera(float intensity, float duration)
    {
        StartCoroutine(ShakeCameraCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCameraCoroutine(float intensity, float duration)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null) yield break;

        Vector2 originalPos = canvasRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(intensity, 0f, elapsed / duration);

            float offsetX = Random.Range(-1f, 1f) * strength;
            float offsetY = Random.Range(-1f, 1f) * strength;

            canvasRect.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);

            yield return null;
        }

        canvasRect.anchoredPosition = originalPos;
    }

    /// <summary>
    /// 禁用所有按钮
    /// </summary>
    private void DisableAllButtons()
    {
        if (newGameButton != null) newGameButton.interactable = false;
        if (continueButton != null) continueButton.interactable = false;
        if (settingsButton != null) settingsButton.interactable = false;
        if (quitButton != null) quitButton.interactable = false;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }
}