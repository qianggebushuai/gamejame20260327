using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioSource musicSource;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("Sound Effects")]
    public AudioSource sfxSource;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    [Header("Common SFX - 常用音效")]
    public AudioClip buttonClickSFX;     // 按钮点击
    public AudioClip shootSFX;           // 射击
    public AudioClip reloadSFX;          // 换弹
    public AudioClip emptyClipSFX;       // 空弹匣
    public AudioClip playerHurtSFX;      // 玩家受伤

    [Header("Music Clips")]
    public AudioClip victoryMusic;

    private AudioClip currentMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化 AudioSource
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    #region 快捷音效方法

    /// <summary>
    /// 播放按钮点击音效
    /// </summary>
    public void PlayButtonClickSFX()
    {
        PlaySFX(buttonClickSFX);
    }

    /// <summary>
    /// 播放射击音效
    /// </summary>
    public void PlayShootSFX()
    {
        PlaySFX(shootSFX);
    }

    /// <summary>
    /// 播放换弹音效
    /// </summary>
    public void PlayReloadSFX()
    {
        PlaySFX(reloadSFX);
    }

    /// <summary>
    /// 播放空弹匣音效
    /// </summary>
    public void PlayEmptyClipSFX()
    {
        PlaySFX(emptyClipSFX);
    }

    /// <summary>
    /// 播放玩家受伤音效
    /// </summary>
    public void PlayPlayerHurtSFX()
    {
        PlaySFX(playerHurtSFX);
    }

    #endregion

    #region Music Control

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        if (currentMusic == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
        currentMusic = clip;
    }

    public void PlayMusicWithFade(AudioClip clip, float fadeDuration = 1f)
    {
        if (clip == null) return;
        if (currentMusic == clip && musicSource.isPlaying) return;

        StopAllCoroutines();
        StartCoroutine(CrossfadeMusic(clip, fadeDuration));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2f));
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();
        currentMusic = newClip;

        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / (duration / 2f));
            yield return null;
        }

        musicSource.volume = musicVolume;
    }

    public void StopMusic()
    {
        musicSource.Stop();
        currentMusic = null;
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void FadeOutMusic(float duration = 1f)
    {
        StartCoroutine(FadeOut(musicSource, duration));
    }

    public void FadeInMusic(AudioClip clip, float duration = 1f)
    {
        musicSource.clip = clip;
        musicSource.volume = 0f;
        musicSource.Play();
        currentMusic = clip;
        StartCoroutine(FadeIn(musicSource, duration));
    }

    #endregion

    #region Sound Effects

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    #endregion

    #region Volume Control

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    public void MuteAll()
    {
        musicSource.mute = true;
        sfxSource.mute = true;
    }

    public void UnmuteAll()
    {
        musicSource.mute = false;
        sfxSource.mute = false;
    }

    #endregion

    #region Fade Effects

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
        source.volume = startVolume;
        currentMusic = null;
    }

    private IEnumerator FadeIn(AudioSource source, float duration)
    {
        float targetVolume = musicVolume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    #endregion
}