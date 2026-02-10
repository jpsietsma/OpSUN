using UnityEngine;


public class PrefabCoordinates : MonoBehaviour
{
    public Vector3 WorldPosition;

    public float X => WorldPosition.x;
    public float Y => WorldPosition.y;
    public float Z => WorldPosition.z;

    void Awake()
    {
        UpdateCoordinates();
    }

    void UpdateCoordinates()
    {
        WorldPosition = transform.position;
    }
}