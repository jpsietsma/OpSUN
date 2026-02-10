using UnityEngine;

public class ItemAudioProfile : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip dropImpactClip;
    public AudioClip pickupClip;

    [Header("Tuning")]
    [Range(0f, 1f)] public float dropVolume = 0.8f;
    [Range(0f, 1f)] public float pickupVolume = 0.8f;

    public float minImpactVelocity = 1.0f;
}