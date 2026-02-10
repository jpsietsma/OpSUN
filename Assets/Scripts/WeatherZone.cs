using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeatherZone : MonoBehaviour
{
    public WeatherManager manager;
    public WeatherManager.ZoneType zone = WeatherManager.ZoneType.Default;
    public string playerTag = "Player";

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (manager != null) manager.SetZone(zone);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (manager != null) manager.SetZone(WeatherManager.ZoneType.Default);
    }
}