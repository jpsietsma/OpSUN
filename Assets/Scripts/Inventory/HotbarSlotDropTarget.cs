using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarSlotDropTarget : MonoBehaviour, IDropHandler
{
    public HotbarUI hotbar;
    public int hotbarSlot;

    private void Awake()
    {
        if (hotbar == null)
            hotbar = GetComponentInParent<HotbarUI>();   // finds HotbarUI up the hierarchy
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (hotbar == null) return;
        if (!UIDragPayload.active) return;

        if (Cursor.lockState != CursorLockMode.None)
        {
            UIDragPayload.Clear();
            return;
        }

        if (UIDragPayload.sourceType == UIDragPayload.SourceType.Inventory)
            hotbar.SetBinding(hotbarSlot, UIDragPayload.inventoryIndex);
        else if (UIDragPayload.sourceType == UIDragPayload.SourceType.Hotbar)
            hotbar.SwapBinding(hotbarSlot, UIDragPayload.sourceIndex);

        UIDragPayload.Clear();
    }
}
