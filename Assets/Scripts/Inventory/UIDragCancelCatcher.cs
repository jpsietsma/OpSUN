using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragCancelCatcher : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // Dropped on background / empty area
        UIDragPayload.Clear();
    }
}