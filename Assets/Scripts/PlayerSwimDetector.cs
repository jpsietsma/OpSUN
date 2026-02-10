using UnityEngine;

public class PlayerSwimDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;     // Drag your camera here
    [SerializeField] private Animator animator;          // Optional (set IsSwimming)

    [Header("Water Detection")]
    [Tooltip("Assign to your Water layer.")]
    [SerializeField] private LayerMask waterLayer;

    [Tooltip("If true, only raycast for water surface while inside a trigger on Water layer.")]
    [SerializeField] private bool requireWaterTrigger = true;

    [Tooltip("How far above the camera to start the downward raycast (in meters).")]
    [SerializeField] private float rayStartAboveCamera = 5f;

    [Tooltip("How far down to raycast from the start point.")]
    [SerializeField] private float rayDistance = 25f;

    [Header("Swim Threshold")]
    [Tooltip("Camera must be this far UNDER the surface to count as swimming (helps avoid flicker at the surface).")]
    [SerializeField] private float submergeBuffer = 0.05f;

    [Header("Animator")]
    [Tooltip("Animator bool to set when swimming.")]
    [SerializeField] private string isSwimmingBool = "IsSwimming";

    public bool IsInWaterTrigger { get; private set; }
    public bool IsSwimming { get; private set; }
    public float CurrentWaterSurfaceY { get; private set; } = float.NegativeInfinity;

    private void Reset()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    private void Update()
    {
        // If we're requiring a trigger and not inside water, we're not swimming.
        if (requireWaterTrigger && !IsInWaterTrigger)
        {
            SetSwimming(false);
            CurrentWaterSurfaceY = float.NegativeInfinity;
            return;
        }

        // Find water surface height at the camera's XZ by raycasting down onto Water layer.
        if (TryGetWaterSurfaceY(playerCamera.position, out float surfaceY))
        {
            CurrentWaterSurfaceY = surfaceY;

            // Swimming when water surface is ABOVE camera height (plus buffer).
            bool shouldSwim = surfaceY > (playerCamera.position.y + submergeBuffer);
            SetSwimming(shouldSwim);
        }
        else
        {
            // No water hit -> not swimming
            CurrentWaterSurfaceY = float.NegativeInfinity;
            SetSwimming(false);
        }
    }

    private bool TryGetWaterSurfaceY(Vector3 cameraPos, out float surfaceY)
    {
        // Start ray above the camera and cast downward.
        Vector3 rayOrigin = cameraPos + Vector3.up * rayStartAboveCamera;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, waterLayer, QueryTriggerInteraction.Collide))
        {
            surfaceY = hit.point.y;
            return true;
        }

        surfaceY = 0f;
        return false;
    }

    private void SetSwimming(bool swimming)
    {
        if (IsSwimming == swimming) return;

        IsSwimming = swimming;

        if (animator != null && !string.IsNullOrWhiteSpace(isSwimmingBool))
            animator.SetBool(isSwimmingBool, IsSwimming);
    }

    // Trigger detection (recommended so you don't raycast for water when you're nowhere near it)
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & waterLayer) != 0)
            IsInWaterTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & waterLayer) != 0)
            IsInWaterTrigger = false;
    }

    // Optional: Draw a debug line in Scene view
    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;

        Gizmos.color = IsSwimming ? Color.cyan : Color.gray;

        Vector3 origin = playerCamera.position + Vector3.up * rayStartAboveCamera;
        Vector3 end = origin + Vector3.down * rayDistance;
        Gizmos.DrawLine(origin, end);
    }
}
