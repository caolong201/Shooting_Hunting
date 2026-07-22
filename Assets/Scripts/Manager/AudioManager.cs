using UnityEngine;

public class AudioManager : SingletonMonoAwake<AudioManager>
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const float DefaultMusicVolume = 0.7f;
    private const float DefaultSfxVolume = 1f;

    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip clickClip;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }

    public override void OnAwake()
    {
        SetupAudioSources();
        LoadVolumes();
    }

    private void SetupAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    private void LoadVolumes()
    {
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume);
        ApplyMusicVolume();
        ApplySfxVolume();
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume();
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        ApplySfxVolume();
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        PlayerPrefs.Save();
    }

    private void ApplyMusicVolume()
    {
        if (musicSource == null)
            return;

        musicSource.volume = MusicVolume;

        if (musicClip == null)
            return;

        if (musicSource.clip == null)
            musicSource.clip = musicClip;

        if (MusicVolume <= 0f)
        {
            if (musicSource.isPlaying)
                musicSource.Pause();
            return;
        }

        if (!musicSource.isPlaying)
        {
            if (musicSource.time > 0f)
                musicSource.UnPause();
            else
                musicSource.Play();
        }
    }

    private void ApplySfxVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = SfxVolume;
    }

    public void PlayClick()
    {
        if (clickClip == null || sfxSource == null || SfxVolume <= 0f)
            return;

        sfxSource.PlayOneShot(clickClip);
    }
}
