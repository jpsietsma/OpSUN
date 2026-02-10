using UnityEngine;
using UnityEngine.EventSystems;

public class ItemTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Set at runtime")]
    public ItemTooltipUI tooltipUI;
    public ItemDefinition item;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipUI == null || item == null) return;
        tooltipUI.Show(item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipUI == null) return;
        tooltipUI.Hide();
    }
}
