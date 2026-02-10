using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryInputHook : MonoBehaviour
{
    [SerializeField] private InventoryUI inventoryUI;

    // This is called automatically by PlayerInput when:
    // - Behavior = "Send Messages", AND
    // - You have an action named "Inventory"
    public void OnInventory(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (inventoryUI != null) inventoryUI.Toggle();
    }
}
