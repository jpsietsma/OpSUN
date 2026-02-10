using UnityEngine;
using UnityEngine.EventSystems;

public class UIRaycastProbe : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"POINTER ENTER: {gameObject.name}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"POINTER EXIT: {gameObject.name}");
    }
}