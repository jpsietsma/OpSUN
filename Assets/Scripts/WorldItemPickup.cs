using UnityEngine;

public class WorldItemPickup : MonoBehaviour, IPickupable
{
    [Header("Item")]
    public ItemDefinition itemDefinition;
    public int amount = 1;

    [Header("Refs")]
    public InventorySystem inventory;   // assign in Inspector (or auto-find)

    private ItemAudioProfile audioProfile;

    public void SetAudioProfile(ItemAudioProfile profile)
    {
        audioProfile = profile;
    }

    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventorySystem>();

        // NEW: if this pickup already has a visual with ItemAudioProfile, grab it
        if (audioProfile == null)
            audioProfile = GetComponentInChildren<ItemAudioProfile>(true);
    }

    public string GetPromptText()
    {
        string name = itemDefinition != null ? itemDefinition.displayName : "Item";
        return $"Press [E] to pick up {name}";
    }

    public void Pickup()
    {
        if (inventory == null || itemDefinition == null) return;

        bool added = inventory.TryAddItem(itemDefinition, amount);
        if (added)
        {
            if (audioProfile != null && audioProfile.pickupClip != null)
            {
                AudioSource.PlayClipAtPoint(audioProfile.pickupClip, transform.position, audioProfile.pickupVolume);
            }

            Destroy(gameObject); // only disappear if successfully added
        }
        else
        {
            // optional: later we can show "Inventory Full"
        }
    }
}
