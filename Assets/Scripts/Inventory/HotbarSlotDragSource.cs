using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarSlotDragSource : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public HotbarUI hotbar;
    public int hotbarSlot;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (hotbar == null) return;

        // Only allow when inventory is open (cursor unlocked)
        if (Cursor.lockState != CursorLockMode.None) return;

        int invIdx = hotbar.boundInventoryIndex[hotbarSlot];
        if (invIdx < 0) return; // empty slot

        UIDragPayload.Begin(UIDragPayload.SourceType.Hotbar, hotbarSlot, invIdx);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // If dropped nowhere, clear
        UIDragPayload.Clear();
    }
}
