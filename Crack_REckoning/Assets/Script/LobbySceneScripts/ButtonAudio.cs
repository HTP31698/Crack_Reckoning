using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAudio : MonoBehaviour
{
    public AudioSource source;

    private void Awake()
    {
        AudioRouter.RouteUI(source);
    }

    public void PlayClickSound()
    {
        if (source)
            source.Play();
    }
}
