using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("UI")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Slider uiSlider;

    [Header("Audio")]
    public AudioMixer audioMixer;

    private static readonly string MasterVolume = "MasterVolume";
    private static readonly string MasterVol = "MasterVol";
    private static readonly string BGMVolume = "BGMVolume";
    private static readonly string BGMVol = "BGMVol";
    private static readonly string SFXVolume = "SFXVolume";
    private static readonly string SFXVol = "SFXVol";
    private static readonly string UIVolume = "UIVolume";
    private static readonly string UIVol = "UIVol";

    private void Awake()
    {
        float msaved = PlayerPrefs.GetFloat(MasterVol, 1f);
        float bsaved = PlayerPrefs.GetFloat(BGMVol, 1f);
        float ssaved = PlayerPrefs.GetFloat(SFXVol, 1f);
        float usaved = PlayerPrefs.GetFloat(UIVol, 1f);

        if (masterSlider != null)
        {
            masterSlider.minValue = 0.0001f;
            masterSlider.maxValue = 1f;
            masterSlider.wholeNumbers = false;
            masterSlider.value = msaved;
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0.0001f;
            bgmSlider.maxValue = 1f;
            bgmSlider.wholeNumbers = false;
            bgmSlider.value = bsaved;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0.0001f;
            sfxSlider.maxValue = 1f;
            sfxSlider.wholeNumbers = false;
            sfxSlider.value = ssaved;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
        if (uiSlider != null)
        {
            uiSlider.minValue = 0.0001f;
            uiSlider.maxValue = 1f;
            uiSlider.wholeNumbers = false;
            uiSlider.value = usaved;
            uiSlider.onValueChanged.AddListener(SetUIVolume);
        }

        SetMasterVolume(msaved);
        SetBGMVolume(bsaved);
        SetSFXVolume(ssaved);
        SetUIVolume(usaved);
    }

    public void SetMasterVolume(float linear)
    {
        float dB = Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(MasterVolume, dB);
        PlayerPrefs.SetFloat(MasterVol, linear);
    }

    public void SetBGMVolume(float linear)
    {
        float dB = Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(BGMVolume, dB);
        PlayerPrefs.SetFloat(BGMVol, linear);
    }

    public void SetSFXVolume(float linear)
    {
        float dB = Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(SFXVolume, dB);
        PlayerPrefs.SetFloat(SFXVol, linear);
    }

    public void SetUIVolume(float linear)
    {
        float dB = Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(UIVolume, dB);
        PlayerPrefs.SetFloat(UIVol, linear);
    }
}
