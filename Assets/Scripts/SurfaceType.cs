using UnityEngine;

public enum SurfaceKind
{
    Default,
    Sand,
    Wood,
    Concrete,
    Gravel,
    Snow,
    Metal
}

public class SurfaceType : MonoBehaviour
{
    public SurfaceKind surface = SurfaceKind.Default;
}