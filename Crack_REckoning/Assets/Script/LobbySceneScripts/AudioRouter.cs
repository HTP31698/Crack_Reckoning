using UnityEngine;
using UnityEngine.Audio;

public static class AudioRouter
{
    private const string MixerResourcePath = "Audio/Setting";
    private const string MixerName = "Setting";
    private const string BGMGroupName = "BGM";
    private const string SFXGroupName = "SFX";
    private const string UIGroupName = "UI";
    private const string MasterGroupName = "Master";

    private static AudioMixer cachedMixer;
    private static AudioMixerGroup bgmGroup;
    private static AudioMixerGroup sfxGroup;
    private static AudioMixerGroup uiGroup;
    private static AudioMixerGroup masterGroup;

    public static AudioMixerGroup BGM { get { EnsureLoaded(); return bgmGroup; } }
    public static AudioMixerGroup SFX { get { EnsureLoaded(); return sfxGroup; } }
    public static AudioMixerGroup UI { get { EnsureLoaded(); return uiGroup; } }
    public static AudioMixerGroup Master { get { EnsureLoaded(); return masterGroup; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        EnsureLoaded();
    }

    private static void EnsureLoaded()
    {
        if (cachedMixer != null && bgmGroup != null && sfxGroup != null && uiGroup != null)
            return;

        if (cachedMixer == null)
            cachedMixer = Resources.Load<AudioMixer>(MixerResourcePath);

        if (cachedMixer == null)
        {
            var mixers = Resources.FindObjectsOfTypeAll<AudioMixer>();
            foreach (var mixer in mixers)
            {
                if (mixer == null) continue;
                if (mixer.name == MixerName) { cachedMixer = mixer; break; }
                if (cachedMixer == null) cachedMixer = mixer;
            }
        }

        if (cachedMixer == null) return;

        bgmGroup = FindGroup(cachedMixer, BGMGroupName);
        sfxGroup = FindGroup(cachedMixer, SFXGroupName);
        uiGroup = FindGroup(cachedMixer, UIGroupName);
        masterGroup = FindGroup(cachedMixer, MasterGroupName);
    }

    private static AudioMixerGroup FindGroup(AudioMixer mixer, string name)
    {
        var groups = mixer.FindMatchingGroups(name);
        if (groups == null || groups.Length == 0) return null;
        foreach (var g in groups)
        {
            if (g.name == name) return g;
        }
        return groups[0];
    }

    public static void RouteSFX(AudioSource source) { if (source != null && SFX != null) source.outputAudioMixerGroup = SFX; }
    public static void RouteBGM(AudioSource source) { if (source != null && BGM != null) source.outputAudioMixerGroup = BGM; }
    public static void RouteUI(AudioSource source) { if (source != null && UI != null) source.outputAudioMixerGroup = UI; }

    public static void PlaySFXAt(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        var go = new GameObject("OneShotSFX");
        go.transform.position = position;
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = volume;
        src.spatialBlend = 0f;
        src.outputAudioMixerGroup = SFX;
        src.Play();
        Object.Destroy(go, clip.length + 0.1f);
    }
}
