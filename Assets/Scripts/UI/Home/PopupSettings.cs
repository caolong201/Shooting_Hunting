using UnityEngine;
using UnityEngine.UI;

public class PopupSettings : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private GameObject musicIconOn;
    [SerializeField] private GameObject musicIconOff;

    [Header("Sound FX")]
    [SerializeField] private Slider soundFxSlider;
    [SerializeField] private GameObject soundIconOn;
    [SerializeField] private GameObject soundIconOff;

    private bool isInitialized;

    private void Initialize()
    {
        if (isInitialized)
            return;

        isInitialized = true;

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            float musicVolume = AudioManager.IsInstanceValid()
                ? AudioManager.Instance.MusicVolume
                : PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            musicSlider.SetValueWithoutNotify(musicVolume);
            UpdateIconState(musicIconOn, musicIconOff, musicVolume);
        }

        if (soundFxSlider != null)
        {
            soundFxSlider.onValueChanged.AddListener(OnSoundFxSliderChanged);
            float sfxVolume = AudioManager.IsInstanceValid()
                ? AudioManager.Instance.SfxVolume
                : PlayerPrefs.GetFloat("SfxVolume", 1f);
            soundFxSlider.SetValueWithoutNotify(sfxVolume);
            UpdateIconState(soundIconOn, soundIconOff, sfxVolume);
        }
    }

    public void Show()
    {
        Initialize();

        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.PlayClick();

        gameObject.SetActive(true);
    }

    public void Close()
    {
        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.PlayClick();

        gameObject.SetActive(false);
    }

    private void OnMusicSliderChanged(float value)
    {
        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.SetMusicVolume(value);

        UpdateIconState(musicIconOn, musicIconOff, value);
    }

    private void OnSoundFxSliderChanged(float value)
    {
        if (AudioManager.IsInstanceValid())
            AudioManager.Instance.SetSfxVolume(value);

        UpdateIconState(soundIconOn, soundIconOff, value);
    }

    private static void UpdateIconState(GameObject iconOn, GameObject iconOff, float value)
    {
        bool isOn = value > 0f;

        if (iconOn != null)
            iconOn.SetActive(isOn);

        if (iconOff != null)
            iconOff.SetActive(!isOn);
    }
}
