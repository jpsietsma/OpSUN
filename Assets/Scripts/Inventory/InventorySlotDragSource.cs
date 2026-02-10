using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotDragSource : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public InventorySystem inventory;
    public int inventoryIndex;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"POINTER DOWN on inventory slot {inventoryIndex} (cursorLock={Cursor.lockState})");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"BEGIN DRAG inventory slot {inventoryIndex} (cursorLock={Cursor.lockState})");

        // Only allow drag when inventory UI is open (your Toggle unlocks cursor)
        if (Cursor.lockState != CursorLockMode.None) return;
        if (inventory == null) return;

        if (inventoryIndex < 0 || inventoryIndex >= inventory.Items.Count) return;

        var stack = inventory.Items[inventoryIndex];
        if (stack.IsEmpty) return;

        UIDragPayload.Begin(UIDragPayload.SourceType.Inventory, inventoryIndex, inventoryIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // optional: Debug.Log($"DRAGGING slot {inventoryIndex}");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"END DRAG inventory slot {inventoryIndex}");
        //UIDragPayload.Clear();
    }
}
