using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAudio : MonoBehaviour
{
    public AudioSource source;

    public void PlayClickSound()
    {
        if (source)
            source.Play();
    }
}
