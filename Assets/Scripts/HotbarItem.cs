using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Hotbar Item")]
public class HotbarItem : ScriptableObject
{
    public string displayName;
    public Sprite icon;
}