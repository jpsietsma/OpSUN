using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbientFade : MonoBehaviour
{
    public float maxVolume = 0.6f;
    public float fadeSeconds = 1.5f;

    AudioSource _audio;
    float _target;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _audio.loop = true;
        _audio.playOnAwake = true;
        _audio.volume = 0f;     // start silent
        _target = 0f;
    }

    void Update()
    {
        float step = (fadeSeconds <= 0f) ? 999f : (Time.deltaTime / fadeSeconds);
        _audio.volume = Mathf.MoveTowards(_audio.volume, _target, step);
    }

    public void FadeIn() => _target = maxVolume;
    public void FadeOut() => _target = 0f;
}