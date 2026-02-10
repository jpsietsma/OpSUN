using UnityEngine;

public class HeldItemController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HotbarUI hotbar;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private Transform handSocket;

    [Header("Options")]
    [SerializeField] private bool hideIfInventoryOpen = true;

    private GameObject currentHeldInstance;
    private ItemDefinition currentItem;

    private void OnEnable()
    {
        if (inventory != null) inventory.OnChanged += RefreshHeldFromSelection;
        if (hotbar != null) hotbar.OnSelectionChanged += OnHotbarSelectionChanged;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.OnChanged -= RefreshHeldFromSelection;
        if (hotbar != null) hotbar.OnSelectionChanged -= OnHotbarSelectionChanged;
    }

    private void Start()
    {
        RefreshHeldFromSelection();
    }

    private void Update()
    {
        if (!hideIfInventoryOpen) return;

        // Your InventoryUI unlocks cursor when open; this is a reliable “UI open” signal in your project
        bool inventoryOpen = Cursor.lockState == CursorLockMode.None;

        if (currentHeldInstance != null)
            currentHeldInstance.SetActive(!inventoryOpen);
    }

    private void OnHotbarSelectionChanged(int _)
    {
        RefreshHeldFromSelection();
    }

    private void RefreshHeldFromSelection()
    {
        if (hotbar == null || inventory == null || handSocket == null)
        {
            ClearHeld();
            return;
        }

        // Get the inventory stack bound to the selected hotbar slot
        var stack = hotbar.GetBoundStack(hotbar.SelectedIndex);

        if (stack.IsEmpty || stack.item == null)
        {
            ClearHeld();
            return;
        }

        // If already holding same item, do nothing
        if (currentItem == stack.item && currentHeldInstance != null) return;

        EquipHeld(stack.item);
    }

    private void EquipHeld(ItemDefinition item)
    {
        ClearHeld();

        currentItem = item;

        // Prefer heldPrefab; fallback to worldPrefab if you want
        var prefab = item.heldPrefab != null ? item.heldPrefab : item.worldPrefab;
        if (prefab == null) return;

        currentHeldInstance = Instantiate(prefab, handSocket);
        currentHeldInstance.transform.localPosition = item.heldLocalPosition;
        currentHeldInstance.transform.localRotation = Quaternion.Euler(item.heldLocalEuler);
        currentHeldInstance.transform.localScale = item.heldLocalScale;

        // Disable physics/colliders on held visual
        foreach (var rb in currentHeldInstance.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        foreach (var col in currentHeldInstance.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }
    }

    private void ClearHeld()
    {
        currentItem = null;

        if (currentHeldInstance != null)
            Destroy(currentHeldInstance);

        currentHeldInstance = null;
    }
}
