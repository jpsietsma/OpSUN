using UnityEngine;

public enum ItemType { Consumable, Equipment, Tool, Backpack, Weapon, Ammo, Misc, CraftingResource, WorkbenchBasic, WorkbenchAdvanced, dev_teleport }
public enum EquipSlot { None, Pants, Boots, Shirt, Jacket, Helmet, Gloves, Glasses, Backpack }

[CreateAssetMenu(menuName = "Game/Items/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public ItemType itemType;
    public EquipSlot equipSlot;
    public int maxStack = 1;

    [TextArea] 
    public string description;

    [Header("Backpack")]
    public int extraSlots = 0; // used if equipSlot == Backpack

    [Header("World Visuals")]
    public GameObject worldPrefab;          // 3D model prefab to show in world/drops
    public Color worldTint = Color.white;   // optional tint
    public GameObject heldPrefab;      // optional: first-person held model (recommended)

    [Header("Held Pose (local to HandSocket)")]
    public Vector3 heldLocalPosition;
    public Vector3 heldLocalEuler;
    public Vector3 heldLocalScale = Vector3.one;

    [Header("Consumable Effects")]

    [Tooltip("Amount of health to restore when consumed")]
    public float healthBuff;

    [Tooltip("Amount of health to remove when consumed")]
    public float healthDebuff;

    [Tooltip("Amount of hunger to restore when consumed")]
    public float hungerBuff;

    [Tooltip("Amount of hunger to remove when consumed")]
    public float hungerDebuff;

    [Tooltip("Amount of thirst to restore when consumed")]
    public float thirstBuff;

    [Tooltip("Amount of thirst to remove when consumed")]
    public float thirstDebuff;

    [Tooltip("Amount of stamina to restore when consumed")]
    public float staminaBuff;

    [Tooltip("Amount of stamina to remove when consumed")]
    public float staminaDebuff;
}