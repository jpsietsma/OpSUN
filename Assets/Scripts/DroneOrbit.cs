using UnityEngine;

public class DroneOrbit : MonoBehaviour
{
    [Header("Floating")]
    public float bobAmplitude = 0.5f;   // How high it moves up/down
    public float bobSpeed = 2f;         // How fast it bobs

    [Header("Target")]
    public Transform target;          // Usually your player

    [Header("Orbit Settings")]
    public float distance = 5f;       // How far from the player
    public float height = 2f;         // How high above the player
    public float orbitSpeed = 40f;    // Degrees per second

    [Header("Behavior")]
    public bool alwaysFaceTarget = true;

    private float _angle;             // Current orbit angle in degrees

    void LateUpdate()
    {
        if (target == null) return;

        _angle += orbitSpeed * Time.deltaTime;
        float rad = _angle * Mathf.Deg2Rad;

        // Floating offset using sine wave
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;

        Vector3 offset = new Vector3(
            Mathf.Sin(rad) * distance,
            height + bobOffset,           // ? height + floating
            Mathf.Cos(rad) * distance
        );

        transform.position = target.position + offset;

        if (alwaysFaceTarget)
        {
            Vector3 lookPoint = target.position + Vector3.up * 1.5f;
            transform.LookAt(lookPoint);
        }
    }
}