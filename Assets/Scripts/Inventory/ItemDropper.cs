using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [Header("Refs")]
    public InventorySystem inventory;
    public Transform dropOrigin;                // usually camera transform or player transform
    public GameObject droppedItemPrefab;        // your Prefabs/DroppedItem

    [Header("Drop Settings")]
    public float dropForwardDistance = 1.5f;
    public float dropUpOffset = 0.2f;

    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventorySystem>();
    }

    public bool DropFromInventoryIndex(int index, int amount)
    {
        if (inventory == null || droppedItemPrefab == null || dropOrigin == null) return false;

        if (!inventory.TryRemoveAt(index, amount, out var item, out var removedAmount))
            return false;

        Vector3 spawnPos = dropOrigin.position + dropOrigin.forward * dropForwardDistance + Vector3.up * dropUpOffset;
        Quaternion rot = Quaternion.identity;

        var go = Instantiate(droppedItemPrefab, spawnPos, rot);

        var pickup = go.GetComponent<WorldItemPickup>();
        if (pickup != null)
        {
            pickup.itemDefinition = item;
            pickup.amount = removedAmount;
        }

        var visual = go.GetComponent<DroppedItemVisual>();
        if (visual != null)
        {
            visual.Apply(item);
        }

        // Pull audio profile from the spawned visual (the item’s prefab)
        var impact = go.GetComponent<DroppedItemImpactSound>();
        if (impact != null && visual != null && visual.CurrentVisualInstance != null)
        {
            var profile = visual.CurrentVisualInstance.GetComponentInChildren<ItemAudioProfile>(true);
            impact.SetProfile(profile);
        }

        // Also store pickup audio info on the pickup script
        if (pickup != null && visual != null && visual.CurrentVisualInstance != null)
        {
            var profile = visual.CurrentVisualInstance.GetComponentInChildren<ItemAudioProfile>(true);
            pickup.SetAudioProfile(profile);
        }

        return true;
    }
}
