using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class FinalAltar : MonoBehaviour
{
    [Header("解谜联动设置")]
    [Tooltip("必须先开启数组里的所有祭坛，本祭坛才能激活")]
    public jitan[] linkedAltars;

    [Header("场景切换设置")]
    [Tooltip("要加载的目标场景名称")]
    public string targetSceneName;
    [Tooltip("场景切换前的等待时间")]
    public float delayBeforeTransition = 2f;
    [Tooltip("屏幕变黑的时间")]
    public float fadeToBlackDuration = 1.5f;

    [Header("粒子特效设置")]
    [Tooltip("红色粒子预制体")]
    public GameObject redParticlePrefab;
    [Tooltip("粒子生成点")]
    public Transform particleSpawnPoint;
    [Tooltip("粒子数量")]
    public int particleCount = 20;
    [Tooltip("粒子生成间隔")]
    public float particleSpawnInterval = 0.05f;
    [Tooltip("粒子上升速度")]
    public float particleRiseSpeed = 3f;
    [Tooltip("粒子存活时间")]
    public float particleLifetime = 2f;

    [Header("UI 交互提示设置")]
    public CanvasGroup promptCanvasGroup;
    public float fadeSpeed = 5f;

    [Header("屏幕渐黑设置")]
    [Tooltip("黑屏物体（初始应为未激活状态）")]
    public GameObject blackScreenObject;
    [Tooltip("黑屏的 CanvasGroup")]
    public CanvasGroup blackScreenCanvasGroup;

    [Header("音效设置(可选)")]
    public AudioClip activationSound;
    public AudioClip transitionSound;
    private AudioSource audioSource;

    // 内部状态
    private bool isPlayerInRange = false;
    private bool isActivated = false;
    private bool isInteracting = false;

    void Start()
    {
        // 初始化交互提示
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }

        // 确保黑屏初始为隐藏状态
        if (blackScreenObject != null)
        {
            blackScreenObject.SetActive(false);
        }

        // 获取或添加 AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (activationSound != null || transitionSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isPlayerInRange && !isInteracting && !isActivated && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(HandleFinalInteraction());
        }
    }

    #region 核心交互逻辑

    private IEnumerator HandleFinalInteraction()
    {
        isInteracting = true;

        // 1. UI 按压反馈
        yield return StartCoroutine(PlayButtonFeedback());

        // 2. 检查所有前置祭坛
        if (!CheckAllAltarsOpen())
        {
            Debug.Log("[FinalAltar] 激活失败！有祭坛尚未开启。");
            isInteracting = false;
            yield break;
        }

        // 3. 激活成功
        isActivated = true;
        Debug.Log("[FinalAltar] 所有祭坛已开启，最终祭坛激活！");

        // 隐藏交互提示
        StartCoroutine(FadePrompt(0f));

        // 播放激活音效
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        // 4. 生成红色粒子
        yield return StartCoroutine(SpawnRisingParticles());

        // 5. 等待一段时间
        yield return new WaitForSeconds(delayBeforeTransition);

        // 播放过渡音效
        if (audioSource != null && transitionSound != null)
        {
            audioSource.PlayOneShot(transitionSound);
        }

        // 6. 屏幕逐渐变黑
        yield return StartCoroutine(FadeToBlack());

        // 7. 加载新场景
        LoadTargetScene();
    }

    /// <summary>
    /// 检查所有关联祭坛是否都已开启
    /// </summary>
    private bool CheckAllAltarsOpen()
    {
        if (linkedAltars == null || linkedAltars.Length == 0)
        {
            Debug.LogWarning("[FinalAltar] 没有设置关联祭坛，默认允许激活。");
            return true;
        }

        foreach (jitan altar in linkedAltars)
        {
            if (altar != null && !altar.isOpen)
            {
                Debug.Log($"[FinalAltar] 祭坛 '{altar.name}' 尚未开启。");
                return false;
            }
        }

        return true;
    }

    #endregion

    #region 粒子效果

    private IEnumerator SpawnRisingParticles()
    {
        if (redParticlePrefab == null)
        {
            Debug.LogWarning("[FinalAltar] 未设置红色粒子预制体！");
            yield break;
        }

        Vector3 spawnPos = particleSpawnPoint != null ? particleSpawnPoint.position : transform.position;

        for (int i = 0; i < particleCount; i++)
        {
            // 随机偏移生成位置
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0f,
                0f
            );

            GameObject particle = Instantiate(redParticlePrefab, spawnPos + randomOffset, Quaternion.identity);

            // 添加上升行为
            FinalAltarParticle fp = particle.AddComponent<FinalAltarParticle>();
            fp.Initialize(particleRiseSpeed, particleLifetime);

            yield return new WaitForSeconds(particleSpawnInterval);
        }
    }

    #endregion

    #region 屏幕渐黑与场景加载

    private IEnumerator FadeToBlack()
    {
        bool end=true;
        for (int i = 0; i <10 ; i++)
        {
            if (!BoolManager.Instance.GetBool(i))
            {
                end = false;break;
            }
        }
        if (end)
        {
            DialogueManager.Instance.StartDialogueWithNPC("42");
        }
        else
        {
            DialogueManager.Instance.StartDialogueWithNPC("43");
        }
        yield return new WaitForSeconds(5f);
        if (blackScreenObject == null || blackScreenCanvasGroup == null)
        {
            Debug.LogWarning("[FinalAltar] 未设置黑屏物体或 CanvasGroup，跳过渐黑效果。");
            yield break;
        }

        // 先设置 alpha 为 0，再激活物体
        blackScreenCanvasGroup.alpha = 0f;
        blackScreenObject.SetActive(true);
        blackScreenCanvasGroup.blocksRaycasts = true; // 阻止玩家操作
        
        float timer = 0f;
        while (timer < fadeToBlackDuration)
        {
            timer += Time.deltaTime;
            blackScreenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeToBlackDuration);
            yield return null;
        }

        timecontroller.instance.currentTime = timecontroller.instance.totalTime;


        blackScreenCanvasGroup.alpha = 1f;
        Debug.Log("[FinalAltar] 屏幕已完全变黑");
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[FinalAltar] 未设置目标场景名称！");
            return;
        }

        Debug.Log($"[FinalAltar] 正在加载场景: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }

    #endregion

    #region UI 交互提示

    private IEnumerator PlayButtonFeedback()
    {
        if (promptCanvasGroup == null) yield break;

        Transform pt = promptCanvasGroup.transform;
        Vector3 origScale = pt.localScale;
        float timer = 0f;
        float punchTime = 0.1f;

        // 放大
        while (timer < punchTime)
        {
            timer += Time.deltaTime;
            pt.localScale = Vector3.Lerp(origScale, origScale * 1.3f, timer / punchTime);
            yield return null;
        }

        // 缩小
        timer = 0f;
        while (timer < punchTime)
        {
            timer += Time.deltaTime;
            pt.localScale = Vector3.Lerp(origScale * 1.3f, origScale, timer / punchTime);
            yield return null;
        }

        pt.localScale = origScale;
    }

    private IEnumerator FadePrompt(float targetAlpha)
    {
        if (promptCanvasGroup == null) yield break;

        while (Mathf.Abs(promptCanvasGroup.alpha - targetAlpha) > 0.01f)
        {
            promptCanvasGroup.alpha = Mathf.MoveTowards(promptCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
        promptCanvasGroup.alpha = targetAlpha;
    }

    #endregion

    #region 触发器检测

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isActivated)
        {
            isPlayerInRange = true;
            StopCoroutine("FadePrompt");
            StartCoroutine(FadePrompt(1f));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            StopCoroutine("FadePrompt");
            StartCoroutine(FadePrompt(0f));
        }
    }

    #endregion
}

/// <summary>
/// 最终祭坛红色粒子行为
/// </summary>
public class FinalAltarParticle : MonoBehaviour
{
    private float riseSpeed;
    private float lifetime;
    private float timer;
    private SpriteRenderer spriteRenderer;
    private float initialAlpha;
    private Vector3 initialScale;

    public void Initialize(float speed, float life)
    {
        riseSpeed = speed + Random.Range(-0.5f, 0.5f);
        lifetime = life;
        timer = lifetime;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            initialAlpha = spriteRenderer.color.a;

            // 确保是红色
            Color c = spriteRenderer.color;
            c.r = 1f;
            c.g = Random.Range(0f, 0.3f);
            c.b = 0f;
            spriteRenderer.color = c;
        }

        initialScale = transform.localScale;

        // 随机大小
        float scale = Random.Range(0.5f, 1.2f);
        transform.localScale *= scale;
    }

    void Update()
    {
        // 上升
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime, Space.World);

        // 轻微左右摆动
        float wobble = Mathf.Sin(Time.time * 3f + transform.position.y * 2f) * 0.3f;
        transform.Translate(Vector3.right * wobble * Time.deltaTime, Space.World);

        // 计时
        timer -= Time.deltaTime;
        float lifeRatio = timer / lifetime;

        // 淡出 + 缩小
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = Mathf.Lerp(0f, initialAlpha, lifeRatio);
            spriteRenderer.color = c;
        }

        transform.localScale = initialScale * Mathf.Lerp(0.3f, 1f, lifeRatio);

        // 销毁
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}