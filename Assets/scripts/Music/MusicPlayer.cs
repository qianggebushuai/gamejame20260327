using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音乐管理器 - 单例模式，跨场景持久化
/// 支持 Inspector 预配置音轨 + 运行时动态添加
/// </summary>
public class MusicManager : MonoBehaviour
{
    #region 单例模式
    private static MusicManager _instance;
    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MusicManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("MusicManager");
                    _instance = go.AddComponent<MusicManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    #region 预配置音轨（Inspector 可编辑）
    [System.Serializable]
    public class PreconfiguredTrack
    {
        [Header("基础设置")]
        [Tooltip("音轨唯一标识，如 BGM、SFX、Ambient")]
        public string trackId;

        [Tooltip("拖入音频文件（WAV/MP3/OGG）")]
        public AudioClip clip;

        [Header("播放设置")]
        [Tooltip("是否循环播放")]
        public bool loop = true;

        [Tooltip("默认音量"), Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip("场景开始时自动播放")]
        public bool playOnStart = false;

        [Tooltip("淡入时长（秒）")]
        public float fadeInDuration = 0f;

        // 运行时内部使用
        [System.NonSerialized] public AudioSource audioSource;
    }

    [Header("预配置音轨列表")]
    [SerializeField]
    private List<PreconfiguredTrack> preconfiguredTracks = new List<PreconfiguredTrack>();
    #endregion

    #region 全局设置
    [Header("全局设置")]
    [SerializeField, Range(0f, 1f)]
    private float masterVolume = 1f;

    [SerializeField]
    private bool persistAcrossScenes = true;
    #endregion

    #region 运行时数据
    private Dictionary<string, PreconfiguredTrack> trackDict = new Dictionary<string, PreconfiguredTrack>();
    private Dictionary<string, AudioSource> dynamicTracks = new Dictionary<string, AudioSource>();
    #endregion

    #region 生命周期
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        InitializeTracks();
    }

    private void Start()
    {
        // 自动播放标记为 playOnStart 的音轨
        foreach (var track in preconfiguredTracks)
        {
            if (track.playOnStart && track.clip != null)
            {
                Play(track.trackId, fadeInDuration: track.fadeInDuration);
            }
        }
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
    #endregion

    #region 初始化
    private void InitializeTracks()
    {
        trackDict.Clear();

        foreach (var track in preconfiguredTracks)
        {
            if (string.IsNullOrEmpty(track.trackId))
            {
                Debug.LogWarning("[MusicManager] 发现未命名的音轨，已跳过");
                continue;
            }

            if (trackDict.ContainsKey(track.trackId))
            {
                Debug.LogWarning($"[MusicManager] 重复的音轨ID: {track.trackId}");
                continue;
            }

            // 创建 AudioSource
            track.audioSource = gameObject.AddComponent<AudioSource>();
            track.audioSource.playOnAwake = false;
            track.audioSource.clip = track.clip;
            track.audioSource.loop = track.loop;  // 应用 loop 设置
            track.audioSource.volume = track.volume * masterVolume;

            trackDict.Add(track.trackId, track);
        }
    }
    #endregion

    #region 播放控制
    public void Play(string trackId, AudioClip overrideClip = null, float fadeInDuration = -1)
    {
        if (trackDict.TryGetValue(trackId, out var track))
        {
            PlayTrack(track, overrideClip, fadeInDuration);
            return;
        }

        if (dynamicTracks.TryGetValue(trackId, out var source))
        {
            PlayDynamic(source, overrideClip, fadeInDuration);
            return;
        }

        Debug.LogError($"[MusicManager] 未找到音轨: {track.trackId}");
    }

    private void PlayTrack(PreconfiguredTrack track, AudioClip overrideClip, float fadeDuration)
    {
        if (track.audioSource == null) return;

        var clip = overrideClip ?? track.clip;
        if (clip == null) return;

        if (overrideClip != null) track.audioSource.clip = clip;

        track.audioSource.Stop();

        float targetVol = track.volume * masterVolume;
        float fadeTime = fadeDuration >= 0 ? fadeDuration : track.fadeInDuration;

        if (fadeTime > 0)
        {
            track.audioSource.volume = 0;
            track.audioSource.Play();
            StartCoroutine(FadeVolume(track.audioSource, targetVol, fadeTime));
        }
        else
        {
            track.audioSource.volume = targetVol;
            track.audioSource.Play();
        }
    }

    private void PlayDynamic(AudioSource source, AudioClip clip, float fade)
    {
        if (clip != null) source.clip = clip;
        if (source.clip == null) return;

        source.Stop();
        if (fade > 0)
        {
            float target = source.volume;
            source.volume = 0;
            source.Play();
            StartCoroutine(FadeVolume(source, target, fade));
        }
        else
        {
            source.Play();
        }
    }

    public void Pause(string trackId)
    {
        if (TryGetSource(trackId, out var s)) s.Pause();
    }

    public void Resume(string trackId)
    {
        if (TryGetSource(trackId, out var s)) s.UnPause();
    }

    public void Stop(string trackId, float fadeOut = 0f)
    {
        if (!TryGetSource(trackId, out var s)) return;

        if (fadeOut > 0 && s.isPlaying)
            StartCoroutine(FadeAndStop(s, fadeOut));
        else
            s.Stop();
    }

    public void StopAll(float fadeOut = 0f)
    {
        foreach (var t in preconfiguredTracks)
            if (t.audioSource?.isPlaying == true) Stop(t.trackId, fadeOut);

        foreach (var s in dynamicTracks.Values)
            if (s.isPlaying)
            {
                if (fadeOut > 0) StartCoroutine(FadeAndStop(s, fadeOut));
                else s.Stop();
            }
    }
    #endregion

    #region 音量控制
    public void SetVolume(string trackId, float vol)
    {
        vol = Mathf.Clamp01(vol);

        if (trackDict.TryGetValue(trackId, out var t))
        {
            t.volume = vol;
            if (t.audioSource) t.audioSource.volume = vol * masterVolume;
        }
        else if (dynamicTracks.TryGetValue(trackId, out var s))
        {
            s.volume = vol * masterVolume;
        }
    }

    public float GetVolume(string trackId)
    {
        if (trackDict.TryGetValue(trackId, out var t)) return t.volume;
        if (dynamicTracks.TryGetValue(trackId, out var s)) return s.volume / masterVolume;
        return 0f;
    }

    public void SetMasterVolume(float vol)
    {
        masterVolume = Mathf.Clamp01(vol);
        foreach (var t in preconfiguredTracks)
            if (t.audioSource) t.audioSource.volume = t.volume * masterVolume;
    }

    public float GetMasterVolume() => masterVolume;

    public void FadeVolume(string trackId, float target, float duration)
    {
        if (TryGetSource(trackId, out var s))
            StartCoroutine(FadeVolume(s, target * masterVolume, duration));
    }
    #endregion

    #region 动态音轨
    public string AddTrack(string id = null, bool loop = true, float vol = 1f)
    {
        if (string.IsNullOrEmpty(id)) id = $"Dynamic_{dynamicTracks.Count + 1}";
        if (trackDict.ContainsKey(id) || dynamicTracks.ContainsKey(id))
        {
            Debug.LogWarning($"[MusicManager] ID已存在: {id}");
            return null;
        }

        var s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = loop;
        s.volume = vol * masterVolume;

        dynamicTracks.Add(id, s);
        return id;
    }

    public void RemoveTrack(string id)
    {
        if (dynamicTracks.TryGetValue(id, out var s))
        {
            s.Stop();
            Destroy(s);
            dynamicTracks.Remove(id);
        }
    }
    #endregion

    #region 状态查询
    public bool IsPlaying(string trackId) => TryGetSource(trackId, out var s) && s.isPlaying;

    public float GetProgress(string trackId)
    {
        if (!TryGetSource(trackId, out var s) || s.clip == null) return 0f;
        return s.time / s.clip.length;
    }

    public void SetProgress(string trackId, float p)
    {
        if (!TryGetSource(trackId, out var s) || s.clip == null) return;
        s.time = Mathf.Clamp01(p) * s.clip.length;
    }
    #endregion

    #region 辅助
    private bool TryGetSource(string id, out AudioSource s)
    {
        if (trackDict.TryGetValue(id, out var t)) { s = t.audioSource; return s != null; }
        return dynamicTracks.TryGetValue(id, out s);
    }

    private IEnumerator FadeVolume(AudioSource s, float target, float d)
    {
        float start = s.volume, elapsed = 0f;
        while (elapsed < d)
        {
            elapsed += Time.unscaledDeltaTime;
            s.volume = Mathf.Lerp(start, target, elapsed / d);
            yield return null;
        }
        s.volume = target;
    }

    private IEnumerator FadeAndStop(AudioSource s, float d)
    {
        float start = s.volume, elapsed = 0f;
        while (elapsed < d)
        {
            elapsed += Time.unscaledDeltaTime;
            s.volume = Mathf.Lerp(start, 0f, elapsed / d);
            yield return null;
        }
        s.volume = 0;
        s.Stop();
    }
    #endregion
}