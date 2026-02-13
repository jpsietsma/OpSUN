using System;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFarmingInteractor : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private SeedToCropDatabase seedDb;

    [Header("Raycast")]
    [SerializeField] private float interactRange = 3.5f;
    [SerializeField] private LayerMask interactMask = ~0;

    [Header("Input (New Input System)")]
    [Tooltip("Right click (or whatever you bind) to plant seeds")]
    [SerializeField] private InputActionReference plantAction;

    [Tooltip("Optional harvest action (E or left click). If null, harvest will use E key fallback.")]
    [SerializeField] private InputActionReference harvestAction;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool allowRawMouseFallback = true; // bypass action maps if needed

    private IHeldItemProvider held;

    private void Awake()
    {
        held = GetComponentInChildren<IHeldItemProvider>(true);
        if (held == null)
            Debug.LogWarning("[PlayerFarmingInteractor] No IHeldItemProvider found in children (HeldItemController missing).");
    }

    private void OnEnable()
    {
        if (plantAction != null) plantAction.action.Enable();
        if (harvestAction != null) harvestAction.action.Enable();
    }

    private void OnDisable()
    {
        if (plantAction != null) plantAction.action.Disable();
        if (harvestAction != null) harvestAction.action.Disable();
    }

    private void Update()
    {
        bool plantPressed =
            (plantAction != null && plantAction.action != null && plantAction.action.WasPressedThisFrame()) ||
            (allowRawMouseFallback && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame);

        if (plantPressed)
        {
            if (debugLogs) Debug.Log("[Farming] Plant pressed");
            TryPlant();
        }

        bool harvestPressed =
            (harvestAction != null && harvestAction.action != null && harvestAction.action.WasPressedThisFrame()) ||
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame);

        if (harvestPressed)
            TryHarvest();
    }

    private void TryPlant()
    {
        if (inventory == null)
        {
            if (debugLogs) Debug.LogWarning("[Farming] inventory is NULL");
            return;
        }

        if (cam == null)
        {
            if (debugLogs) Debug.LogWarning("[Farming] cam is NULL");
            return;
        }

        // 1) Raycast
        if (!Raycast(out var hit))
        {
            if (debugLogs) Debug.LogWarning("[Farming] Raycast hit NOTHING (range/mask/collider/camera issue)");
            return;
        }

        if (debugLogs)
            Debug.Log($"[Farming] Raycast hit: {hit.collider.name} (layer {LayerMask.LayerToName(hit.collider.gameObject.layer)})");

        // 2) Find plot
        var plot = hit.collider.GetComponentInParent<CropPlot>();
        if (plot == null)
        {
            if (debugLogs) Debug.LogWarning("[Farming] Hit object is NOT under a CropPlot (CropPlot component missing or not parent)");
            return;
        }

        if (!plot.CanPlant)
        {
            if (debugLogs) Debug.LogWarning("[Farming] Plot found, but CanPlant = false (already planted?)");
            return;
        }

        // 3) Held item
        var seedItem = held != null ? held.GetHeldItemDefinition() : null;
        if (seedItem == null)
        {
            if (debugLogs) Debug.LogWarning("[Farming] Held item is NULL (heldItemProvider not set or not implementing IHeldItemProvider)");
            return;
        }

        if (debugLogs) Debug.Log($"[Farming] Held item: {seedItem.name}, isSeed={seedItem.isSeed}");

        if (!seedItem.isSeed)
        {
            if (debugLogs) Debug.LogWarning("[Farming] Held item is not marked isSeed");
            return;
        }

        // 4) Seed -> Crop mapping
        if (seedDb == null)
        {
            if (debugLogs) Debug.LogWarning("[Farming] seedDb is NULL");
            return;
        }

        if (!seedDb.TryGetCrop(seedItem, out var cropDef) || cropDef == null)
        {
            if (debugLogs) Debug.LogWarning($"[Farming] No crop mapping for seed: {seedItem.name}");
            return;
        }

        if (debugLogs) Debug.Log($"[Farming] Found cropDef: {cropDef.name}");

        // 5) Consume seed
        //if (!inventory.TryRemoveItem(seedItem, 1))
        //{
        //    if (debugLogs) Debug.LogWarning("[Farming] TryRemoveItem failed (seed not actually in inventory?)");
        //    return;
        //}

        // Consume 1 seed from inventory WITHOUT shrinking the Items list
        if (!ConsumeItem_NoShrink(seedItem, 1))
        {
            if (debugLogs) Debug.LogWarning("[Farming] ConsumeItem_NoShrink failed (seed not in inventory?)");
            return;
        }

        ForceInventoryRefresh();

        // 6) Plant
        if (!plot.TryPlant(cropDef))
        {
            if (debugLogs) Debug.LogWarning("[Farming] plot.TryPlant failed (refunding seed)");
            inventory.TryAddItem(seedItem, 1);
            return;
        }

        // 7) Save harvest info
        var info = plot.GetComponent<CropPlotHarvestInfo>();
        if (info == null) info = plot.gameObject.AddComponent<CropPlotHarvestInfo>();
        info.harvestItem = seedItem.harvestResult;
        info.harvestAmount = Mathf.Max(1, seedItem.harvestAmount);

        if (debugLogs) Debug.Log("[Farming] PLANTED SUCCESS");
    }

    private void TryHarvest()
    {
        if (!Raycast(out var hit)) return;

        var plot = hit.collider.GetComponentInParent<CropPlot>();
        if (plot == null || !plot.ReadyToHarvest) return;

        // Determine what the player gets:
        // We use the held seed's harvest result if they are holding seeds,
        // OR you can change this to store harvest info in CropDefinition instead.
        // Best: put harvestResult on the seed item definition.
        ItemDefinition seedItem = held != null ? held.GetHeldItemDefinition() : null;

        // If not holding the seed anymore, still allow harvest:
        // We'll search the plot's currentStageInstance? We didn't store crop item there.
        // So: simplest: require harvestResult on seed items AND allow harvesting with ANY item:
        // We'll just try to find any entry where crop matches the plot's cropDef by adding a getter.
        // To keep this simple and copy-paste safe, we instead require player to have ANY seed in database:
        // -> We'll use a tiny helper stored per plot below in Step 6 (CropPlotHarvestInfo).
        var harvestInfo = plot.GetComponent<CropPlotHarvestInfo>();
        if (harvestInfo == null || harvestInfo.harvestItem == null || harvestInfo.harvestAmount <= 0)
        {
            Debug.LogWarning("[Farming] Missing CropPlotHarvestInfo on plot (needed to know what to harvest).");
            return;
        }

        inventory.TryAddItem(harvestInfo.harvestItem, harvestInfo.harvestAmount);
        plot.ClearPlot();
    }

    private bool ConsumeItem_NoShrink(ItemDefinition item, int amount)
    {
        if (inventory == null || item == null || amount <= 0) return false;
        if (inventory.Items == null) return false;

        int remaining = amount;

        // IMPORTANT: do NOT RemoveAt() — keep list size stable for InventoryUI
        for (int i = 0; i < inventory.Items.Count && remaining > 0; i++)
        {
            var stack = inventory.Items[i];
            if (stack == null) continue;

            if (stack.item != item) continue;
            if (stack.count <= 0) continue;

            int take = Mathf.Min(stack.count, remaining);
            stack.count -= take;
            remaining -= take;

            // If stack is now empty, keep the slot but clear it
            if (stack.count <= 0)
            {
                stack.count = 0;
                stack.item = null;
            }
        }

        // We intentionally do NOT invoke inventory.OnChanged here (can't from outside).
        // Your UI already refreshes on open (and your inventory likely refreshes elsewhere).
        return remaining == 0;
    }

    private void ForceInventoryRefresh()
    {
        if (inventory == null) return;

        try
        {
            // InventorySystem has: public event Action OnChanged;
            // Outside classes can't invoke it normally, so we invoke the backing delegate via reflection.
            var t = inventory.GetType();
            var field = t.GetField("OnChanged", BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null)
            {
                if (debugLogs) Debug.LogWarning("[Farming] ForceInventoryRefresh: couldn't find OnChanged field via reflection.");
                return;
            }

            var del = field.GetValue(inventory) as Delegate;
            del?.DynamicInvoke();
        }
        catch (Exception e)
        {
            if (debugLogs) Debug.LogWarning("[Farming] ForceInventoryRefresh failed: " + e.Message);
        }
    }

    private bool Raycast(out RaycastHit hit)
    {
        hit = default;

        if (cam == null) return false;

        var ray = new Ray(cam.transform.position, cam.transform.forward);
        return Physics.Raycast(ray, out hit, interactRange, interactMask, QueryTriggerInteraction.Ignore);
    }
}

// Your hotbar/held item script should implement this:
public interface IHeldItemProvider
{
    ItemDefinition GetHeldItemDefinition();
}
