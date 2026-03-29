using UnityEngine;

public class SceneMusicController : MonoBehaviour
{
    [Header("Scene Music Settings")]
    public AudioClip normalMusic;
    public AudioClip bossMusic;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    [Header("Settings")]
    [SerializeField] private bool forcePlayOnStart = true;  // 强制重新播放

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager not found!");
            return;
        }
        if (forcePlayOnStart)
        {
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayMusic(normalMusic);
        }
        else
        {
            AudioManager.Instance.PlayMusic(normalMusic);
        }



    }

    public void PlayBossMusic()
    {
        if (bossMusic != null)
        {
            AudioManager.Instance.PlayMusicWithFade(bossMusic, fadeDuration);
        }
    }

    public void PlayNormalMusic()
    {
        if (normalMusic != null)
        {
            AudioManager.Instance.PlayMusicWithFade(normalMusic, fadeDuration);
        }
    }
}