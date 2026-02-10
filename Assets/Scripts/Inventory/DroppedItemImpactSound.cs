using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DroppedItemImpactSound : MonoBehaviour
{
    private AudioSource source;
    private bool hasPlayed;

    // Set at spawn time
    private ItemAudioProfile profile;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
    }

    public void SetProfile(ItemAudioProfile p)
    {
        profile = p;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasPlayed) return;
        if (profile == null) return;
        if (profile.dropImpactClip == null) return;

        if (collision.relativeVelocity.magnitude < profile.minImpactVelocity)
            return;

        hasPlayed = true;
        source.PlayOneShot(profile.dropImpactClip, profile.dropVolume);
    }
}