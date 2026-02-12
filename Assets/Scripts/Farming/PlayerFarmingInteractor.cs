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

    [Header("Held Item Provider")]
    [Tooltip("Drag your Hotbar/Equipment script here that knows what the active item is.")]
    [SerializeField] private MonoBehaviour heldItemProvider;

    private IHeldItemProvider held;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (inventory == null) inventory = GetComponentInChildren<InventorySystem>();

        held = heldItemProvider as IHeldItemProvider;
        if (heldItemProvider != null && held == null)
            Debug.LogWarning("[PlayerFarmingInteractor] heldItemProvider does not implement IHeldItemProvider.");
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
        if (plantAction != null && plantAction.action.WasPressedThisFrame())
            TryPlant();

        bool harvestPressed =
            (harvestAction != null && harvestAction.action.WasPressedThisFrame()) ||
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame);

        if (harvestPressed)
            TryHarvest();
    }

    private void TryPlant()
    {
        if (!Raycast(out var hit)) return;

        var plot = hit.collider.GetComponentInParent<CropPlot>();
        if (plot == null || !plot.CanPlant) return;

        var seedItem = held != null ? held.GetHeldItemDefinition() : null;
        if (seedItem == null || !seedItem.isSeed) return;

        if (seedDb == null || !seedDb.TryGetCrop(seedItem, out var cropDef) || cropDef == null)
        {
            Debug.LogWarning($"[Farming] No crop mapping found for seed: {seedItem.name}");
            return;
        }

        // Consume 1 seed from inventory
        if (!inventory.TryRemoveItem(seedItem, 1))
            return;

        if (plot.TryPlant(cropDef))
        {
            // planted successfully
            var info = plot.GetComponent<CropPlotHarvestInfo>();
            if (info == null) info = plot.gameObject.AddComponent<CropPlotHarvestInfo>();

            info.harvestItem = seedItem.harvestResult;
            info.harvestAmount = Mathf.Max(1, seedItem.harvestAmount);
        }
        else
        {
            // if planting failed, refund
            inventory.TryAddItem(seedItem, 1);
        }
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
