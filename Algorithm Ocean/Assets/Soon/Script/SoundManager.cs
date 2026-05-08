using UnityEngine;

/// <summary>
/// BGM + SFX 통합 사운드 매니저.
/// 씬 전환 시에도 유지되며, 볼륨은 각각 독립 제어.
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource 자동 생성 (인스펙터에 없으면)
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
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;  // 이미 같은 곡 재생 중

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void PauseBgm()
    {
        if (bgmSource.isPlaying) bgmSource.Pause();
    }

    public void ResumeBgm()
    {
        if (bgmSource.clip != null && !bgmSource.isPlaying) bgmSource.UnPause();
    }

    public void StopBgm()
    {
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
        // SFX는 PlayOneShot이라 재생 중인 소리에는 즉시 반영 안 됨
        // (다음 PlaySfx 호출부터 적용)
    }

    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

    private void ApplyVolumes()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume;
    }
}