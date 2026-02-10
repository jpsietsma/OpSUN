using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotRightClickDrop : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex;

    public InventoryUI inventoryUI;
    public ItemDropper dropper;

    // drop 1 by default (if maxStack == 1 it drops 1 anyway)
    public int dropAmount = 1;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left && eventData.button != PointerEventData.InputButton.Right) return;
        if (inventoryUI == null || dropper == null) return;

        //Right click drops from inventory
        if (eventData.button == PointerEventData.InputButton.Right)
            dropper.DropFromInventoryIndex(slotIndex, dropAmount);
    }
}