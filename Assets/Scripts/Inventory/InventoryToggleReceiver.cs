using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggleReceiver : MonoBehaviour
{
    [SerializeField] private InventoryUI inventoryUI;

    // Send Messages uses InputValue (not CallbackContext)
    public void OnToggleInventory(InputValue value)
    {
        if (!value.isPressed) return;
        inventoryUI?.Toggle();
    }
}