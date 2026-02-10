using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroppedItemSettler : MonoBehaviour
{
    [Header("Settle Timing")]
    public float settleDelay = 1.0f;

    [Header("After Settle Settings")]
    public float settledDrag = 5f;
    public float settledAngularDrag = 20f;
    public bool freezeRotationAfterSettle = true;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Invoke(nameof(Settle), settleDelay);
    }

    private void Settle()
    {
        if (rb == null) return;

        rb.linearDamping = settledDrag;
        rb.angularDamping = settledAngularDrag;

        if (freezeRotationAfterSettle)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotationX;
            rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
        }
    }
}

