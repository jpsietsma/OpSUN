using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarSlotClick : MonoBehaviour, IPointerClickHandler
{
    public int hotbarSlot;

    public HotbarUI hotbarUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left && eventData.button != PointerEventData.InputButton.Right) return;
        if (hotbarUI == null || hotbarSlot == null) return;

        //Right click clears hotbar
        if (eventData.button == PointerEventData.InputButton.Right)
            hotbarUI.SetBinding(hotbarSlot, -1);
    }
}