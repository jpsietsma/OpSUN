using UnityEngine;

public static class UIDragPayload
{
    public enum SourceType { Inventory, Hotbar }

    public static SourceType sourceType;
    public static int sourceIndex;         // inventory index OR hotbar slot index
    public static int inventoryIndex;      // resolved inventory index (for Hotbar source too)
    public static bool active;

    public static void Begin(SourceType type, int srcIndex, int invIndex)
    {
        sourceType = type;
        sourceIndex = srcIndex;
        inventoryIndex = invIndex;
        active = true;
    }

    public static void Clear()
    {
        active = false;
        sourceIndex = -1;
        inventoryIndex = -1;
    }
}
