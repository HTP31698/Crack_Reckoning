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

    private const string MasterVolume = "MasterVolume";
    private const string MasterVol = "MasterVol";
    private const string BGMVolume = "BGMVolume";
    private const string BGMVol = "BGMVol";
    private const string SFXVolume = "SFXVolume";
    private const string SFXVol = "SFXVol";
    private const string UIVolume = "UIVolume";
    private const string UIVol = "UIVol";

    private const string MixerResourcePath = "Audio/Setting";

    private bool slidersBound;

    private void Awake()
    {
        EnsureMixer();
        ApplyAllFromPrefs();
    }

    private void OnEnable()
    {
        EnsureMixer();
        ApplyAllFromPrefs();

        if (slidersBound) return;

        BindSlider(masterSlider, PlayerPrefs.GetFloat(MasterVol, 1f), SetMasterVolume);
        BindSlider(bgmSlider, PlayerPrefs.GetFloat(BGMVol, 1f), SetBGMVolume);
        BindSlider(sfxSlider, PlayerPrefs.GetFloat(SFXVol, 1f), SetSFXVolume);
        BindSlider(uiSlider, PlayerPrefs.GetFloat(UIVol, 1f), SetUIVolume);

        slidersBound = true;
    }

    private void EnsureMixer()
    {
        if (audioMixer == null)
            audioMixer = Resources.Load<AudioMixer>(MixerResourcePath);
    }

    private void ApplyAllFromPrefs()
    {
        ApplyMixer(MasterVolume, PlayerPrefs.GetFloat(MasterVol, 1f));
        ApplyMixer(BGMVolume, PlayerPrefs.GetFloat(BGMVol, 1f));
        ApplyMixer(SFXVolume, PlayerPrefs.GetFloat(SFXVol, 1f));
        ApplyMixer(UIVolume, PlayerPrefs.GetFloat(UIVol, 1f));
    }

    private static void BindSlider(Slider slider, float value, UnityEngine.Events.UnityAction<float> handler)
    {
        if (slider == null) return;
        slider.minValue = 0.0001f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.AddListener(handler);
    }

    private void ApplyMixer(string param, float linear)
    {
        if (audioMixer == null) return;
        float dB = Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(param, dB);
    }

    public void SetMasterVolume(float linear)
    {
        ApplyMixer(MasterVolume, linear);
        PlayerPrefs.SetFloat(MasterVol, linear);
    }

    public void SetBGMVolume(float linear)
    {
        ApplyMixer(BGMVolume, linear);
        PlayerPrefs.SetFloat(BGMVol, linear);
    }

    public void SetSFXVolume(float linear)
    {
        ApplyMixer(SFXVolume, linear);
        PlayerPrefs.SetFloat(SFXVol, linear);
    }

    public void SetUIVolume(float linear)
    {
        ApplyMixer(UIVolume, linear);
        PlayerPrefs.SetFloat(UIVol, linear);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyVolumeBeforeSceneLoad()
    {
        ApplyPrefsToMixer();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyVolumeAfterSceneLoad()
    {
        ApplyPrefsToMixer();
        VolumeBootstrapper.Ensure();
    }

    private static void ApplyPrefsToMixer()
    {
        var mixer = Resources.Load<AudioMixer>(MixerResourcePath);
        if (mixer == null) return;

        TrySetMixer(mixer, MasterVolume, PlayerPrefs.GetFloat(MasterVol, 1f));
        TrySetMixer(mixer, BGMVolume, PlayerPrefs.GetFloat(BGMVol, 1f));
        TrySetMixer(mixer, SFXVolume, PlayerPrefs.GetFloat(SFXVol, 1f));
        TrySetMixer(mixer, UIVolume, PlayerPrefs.GetFloat(UIVol, 1f));
    }

    private static void TrySetMixer(AudioMixer mixer, string param, float linear)
    {
        if (mixer == null) return;
        float dB = Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
        mixer.SetFloat(param, dB);
    }

    internal static void ForceApplyFromPrefs()
    {
        ApplyPrefsToMixer();
    }
}

internal class VolumeBootstrapper : MonoBehaviour
{
    private static VolumeBootstrapper instance;
    private int framesApplied;

    internal static void Ensure()
    {
        if (instance != null) return;
        var go = new GameObject("~VolumeBootstrapper");
        DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        instance = go.AddComponent<VolumeBootstrapper>();
    }

    private void LateUpdate()
    {
        if (framesApplied >= 3) return;
        VolumeController.ForceApplyFromPrefs();
        framesApplied++;
    }
}
