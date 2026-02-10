using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIRaycastDump : MonoBehaviour
{
    public Key dumpKey = Key.F9;

    void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current[dumpKey].wasPressedThisFrame) return;

        if (EventSystem.current == null)
        {
            Debug.Log("NO EventSystem.current");
            return;
        }

        var ped = new PointerEventData(EventSystem.current);
        ped.position = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        Debug.Log($"--- UI RAYCAST DUMP @ {ped.position} (hits={results.Count}) ---");
        for (int i = 0; i < results.Count; i++)
        {
            var r = results[i];
            Debug.Log($"{i}: {r.gameObject.name} | canvas={r.module?.GetType().Name} | sortLayer={r.sortingLayer} order={r.sortingOrder} depth={r.depth} dist={r.distance}");
        }
    }
}
