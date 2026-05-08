using UnityEngine;

/// <summary>
/// Manages BGM and SFX playback.
/// Persists across scene changes and controls each volume separately.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Default Volume")]
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 0.6f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1.0f;

    [Header("Default BGM (Optional)")]
    [SerializeField] private AudioClip defaultBgm;

    private AudioClip pausedBgmClip;
    private float pausedBgmTime;
    private bool bgmPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create AudioSources automatically when they are not assigned in the Inspector.
        if (bgmSource == null) bgmSource = CreateSource("BgmSource", true);
        if (sfxSource == null) sfxSource = CreateSource("SfxSource", false);

        ApplyVolumes();

        if (defaultBgm != null) PlayBgm(defaultBgm);
    }

    private AudioSource CreateSource(string name, bool loop)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.loop = loop;
        src.playOnAwake = false;
        return src;
    }

    // ========== BGM ==========
    public void PlayBgm(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.loop = true;
        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void PauseBgm()
    {
        if (!bgmSource.isPlaying) return;

        pausedBgmClip = bgmSource.clip;
        pausedBgmTime = bgmSource.time;
        bgmPaused = true;
        bgmSource.Pause();
    }

    public void ResumeBgm()
    {
        if (bgmSource.isPlaying) return;
        if (bgmSource.clip == null && pausedBgmClip == null) return;

        if (bgmPaused && pausedBgmClip != null)
        {
            bgmSource.clip = pausedBgmClip;
            bgmSource.time = Mathf.Clamp(pausedBgmTime, 0f, Mathf.Max(0f, pausedBgmClip.length - 0.01f));
        }

        bgmSource.UnPause();

        if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }

        bgmPaused = false;
    }

    public void StopBgm()
    {
        bgmPaused = false;
        pausedBgmClip = null;
        pausedBgmTime = 0f;
        bgmSource.Stop();
    }

    // ========== SFX ==========
    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    // ========== Volume ==========
    public void SetBgmVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        bgmSource.volume = bgmVolume;
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        // PlayOneShot applies the volume to future SFX playback.
    }

    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

    private void ApplyVolumes()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume;
    }
}
