using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BeachAmbientZone : MonoBehaviour
{
    public string playerTag = "Player";
    AmbientFade _fade;

    void Awake()
    {
        // Make sure the collider is a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        _fade = GetComponent<AmbientFade>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            _fade?.FadeIn();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            _fade?.FadeOut();
    }
}