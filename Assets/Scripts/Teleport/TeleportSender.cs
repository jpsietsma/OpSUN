using UnityEngine;

public class TeleportSender : MonoBehaviour, IHoldInteractable
{
    [Header("Teleport Setup")]
    public Transform playerRoot;
    public TeleportReceiver teleportLocationNode;

    [Header("Hold Settings")]
    [SerializeField] private float holdDuration = 1.25f;
    public float HoldDuration => holdDuration;

    private Vector3 _destinationLocation;

    void Awake()
    {
        CacheDestination();
    }

    void OnValidate()
    {
        // keeps destination updated in-editor when you drag references around
        CacheDestination();
    }

    private void CacheDestination()
    {
        if (teleportLocationNode != null)
            _destinationLocation = teleportLocationNode.transform.position;
    }

    public string GetHoldPromptText()
    {
        return "Hold [E] to teleport to " + teleportLocationNode.itemDefinition.displayName;
    }

    public void OnHoldComplete()
    {
        Debug.Log("Teleport hold complete!");
        SendPlayerToNode();
    }

    private void SendPlayerToNode()
    {
        Debug.Log("Teleporting now...");

        if (playerRoot == null || teleportLocationNode == null) return;

        Vector3 dest = teleportLocationNode.transform.position + Vector3.up * 0.1f;

        // If using CharacterController (Starter Assets), disable it before moving
        if (playerRoot.TryGetComponent<CharacterController>(out var cc))
        {
            cc.enabled = false;
            playerRoot.position = dest;
            cc.enabled = true;
            return;
        }

        // If using Rigidbody movement
        if (playerRoot.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = dest;
            return;
        }

        // Fallback
        playerRoot.position = dest;
    }
}
