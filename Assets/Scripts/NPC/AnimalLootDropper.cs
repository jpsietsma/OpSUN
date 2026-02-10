using UnityEngine;

[System.Serializable]
public class AnimalLootEntry
{
    public ItemDefinition item;
    [Min(0)] public int minAmount = 1;
    [Min(0)] public int maxAmount = 1;
    [Range(0f, 1f)] public float dropChance = 1f;
}

public class AnimalLootDropper : MonoBehaviour
{
    [Header("Loot Table")]
    public AnimalLootEntry[] loot;

    [Header("Spawn Refs")]
    public Transform dropOrigin;                 // optional: a DropPoint on the animal
    public GameObject droppedItemPrefab;         // SAME prefab used by ItemDropper (Prefabs/DroppedItem)

    [Header("Drop Settings")]
    public float scatterRadius = 0.6f;
    public float dropUpOffset = 0.2f;

    [Header("Ground Snap")]
    public bool snapToGround = true;
    public LayerMask groundLayers = ~0;
    public float rayStartHeight = 3f;
    public float rayDistance = 12f;
    public float groundOffset = 0.03f;

    private bool _dropped;

    public void DropLoot()
    {
        if (_dropped) return;
        _dropped = true;

        if (droppedItemPrefab == null)
        {
            Debug.LogWarning($"{name}: AnimalLootDropper missing droppedItemPrefab.");
            return;
        }

        if (loot == null || loot.Length == 0) return;

        Vector3 origin = dropOrigin != null ? dropOrigin.position : transform.position;

        foreach (var entry in loot)
        {
            if (entry == null || entry.item == null) continue;
            if (Random.value > entry.dropChance) continue;

            int min = Mathf.Min(entry.minAmount, entry.maxAmount);
            int max = Mathf.Max(entry.minAmount, entry.maxAmount);
            int amount = Random.Range(min, max + 1);
            if (amount <= 0) continue;

            SpawnDroppedItem(entry.item, amount, origin);
        }
    }

    private void SpawnDroppedItem(ItemDefinition item, int amount, Vector3 origin)
    {
        // Scatter
        Vector2 r = Random.insideUnitCircle * scatterRadius;
        Vector3 spawnPos = origin + new Vector3(r.x, 0f, r.y) + Vector3.up * dropUpOffset;

        // Snap to ground to prevent floating
        if (snapToGround)
        {
            Vector3 rayStart = spawnPos + Vector3.up * rayStartHeight;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, groundLayers, QueryTriggerInteraction.Ignore))
            {
                spawnPos = hit.point + Vector3.up * groundOffset;
            }
        }

        Quaternion rot = Quaternion.identity;
        var go = Instantiate(droppedItemPrefab, spawnPos, rot);

        // Set pickup data
        var pickup = go.GetComponent<WorldItemPickup>();
        if (pickup != null)
        {
            pickup.itemDefinition = item;
            pickup.amount = amount;
        }

        // Spawn model from ItemDefinition.worldPrefab via DroppedItemVisual.Apply(item)
        var visual = go.GetComponent<DroppedItemVisual>();
        if (visual != null)
        {
            visual.Apply(item);
        }

        // Impact sound profile from the spawned visual instance
        var impact = go.GetComponent<DroppedItemImpactSound>();
        if (impact != null && visual != null && visual.CurrentVisualInstance != null)
        {
            var profile = visual.CurrentVisualInstance.GetComponentInChildren<ItemAudioProfile>(true);
            impact.SetProfile(profile);
        }

        // Store pickup audio info on the pickup script too
        if (pickup != null && visual != null && visual.CurrentVisualInstance != null)
        {
            var profile = visual.CurrentVisualInstance.GetComponentInChildren<ItemAudioProfile>(true);
            pickup.SetAudioProfile(profile);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = dropOrigin != null ? dropOrigin.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, scatterRadius);
    }
#endif
}
