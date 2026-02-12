using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [Header("Base inventory")]
    [SerializeField] private int baseSlots = 20;

    public int CurrentSlotCapacity { get; private set; }
    public List<ItemStack> Items { get; private set; } = new();

    public PlayerStats playerVitals;

    private readonly Dictionary<EquipSlot, ItemStack> equipped = new();

    public event Action OnChanged;

    private void Awake()
    {
        foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
        {
            if (slot == EquipSlot.None) continue;
            equipped[slot] = new ItemStack();
        }

        RebuildInventoryCapacity();
    }

    public ItemStack GetEquipped(EquipSlot slot) => equipped[slot];

    public bool TryRemoveItem(ItemDefinition item, int amount)
    {
        if (item == null || amount <= 0) return false;

        int remaining = amount;

        for (int i = Items.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var stack = Items[i];
            if (stack == null || stack.item != item) continue;

            int take = Mathf.Min(stack.count, remaining);
            stack.count -= take;
            remaining -= take;

            if (stack.count <= 0)
                Items.RemoveAt(i);
        }

        if (remaining == 0)
        {
            OnChanged?.Invoke();   // <-- legal here
            return true;
        }

        return false;
    }

    public bool TryAddItem(ItemDefinition item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        // Assumes you have: Items[] as an array/list of stacks, and stack has IsEmpty, item, count
        // And ItemData has maxStack (your UI references item.maxStack)
        for (int i = 0; i < Items.Count; i++)
        {
            var stack = Items[i];

            // stack with same item and space
            if (!stack.IsEmpty && stack.item == item && stack.count < item.maxStack)
            {
                int canAdd = Mathf.Min(amount, item.maxStack - stack.count);
                stack.count += canAdd;
                amount -= canAdd;
                Items[i] = stack;     // important if Stack is a struct
                if (amount <= 0) { OnChanged?.Invoke(); return true; }
            }
        }

        // fill empty slots
        for (int i = 0; i < Items.Count; i++)
        {
            var stack = Items[i];
            if (stack.IsEmpty)
            {
                int canAdd = Mathf.Min(amount, item.maxStack);
                stack.item = item;
                stack.count = canAdd;
                Items[i] = stack;     // important if Stack is a struct
                amount -= canAdd;
                if (amount <= 0) { OnChanged?.Invoke(); return true; }
            }
        }

        // no room
        OnChanged?.Invoke();
        return false;
    }

    public bool TryAdd(ItemDefinition item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        if (item.maxStack > 1)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (!Items[i].IsEmpty && Items[i].item == item && Items[i].count < item.maxStack)
                {
                    int space = item.maxStack - Items[i].count;
                    int add = Mathf.Min(space, amount);
                    Items[i].count += add;
                    amount -= add;
                    if (amount <= 0) { OnChanged?.Invoke(); return true; }
                }
            }
        }

        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].IsEmpty)
            {
                int add = Mathf.Min(item.maxStack, amount);
                Items[i] = new ItemStack(item, add);
                amount -= add;
                if (amount <= 0) { OnChanged?.Invoke(); return true; }
            }
        }

        OnChanged?.Invoke();
        return false;
    }

    public bool TryRemoveAt(int inventoryIndex, int amount, out ItemDefinition removedItem, out int removedAmount)
    {
        removedItem = null;
        removedAmount = 0;

        if (inventoryIndex < 0 || inventoryIndex >= Items.Count) return false;
        if (amount <= 0) return false;

        var stack = Items[inventoryIndex];
        if (stack.IsEmpty || stack.item == null) return false;

        removedItem = stack.item;

        int take = Mathf.Min(amount, stack.count);
        stack.count -= take;
        removedAmount = take;

        if (stack.count <= 0)
            Items[inventoryIndex] = new ItemStack();
        else
            Items[inventoryIndex] = stack;

        OnChanged?.Invoke();
        return true;
    }

    public bool TryEquipFromInventory(int inventoryIndex)
    {
        if (inventoryIndex < 0 || inventoryIndex >= Items.Count) return false;
        var stack = Items[inventoryIndex];
        if (stack.IsEmpty) return false;

        var item = stack.item;
        if (item.itemType != ItemType.Equipment && item.itemType != ItemType.Backpack) return false;
        if (item.equipSlot == EquipSlot.None) return false;

        EquipSlot slot = item.equipSlot;

        if (!equipped[slot].IsEmpty)
        {
            if (!TryAdd(equipped[slot].item, equipped[slot].count))
                return false;
        }

        equipped[slot] = new ItemStack(item, 1);

        stack.count -= 1;
        if (stack.count <= 0) Items[inventoryIndex] = new ItemStack();

        if (slot == EquipSlot.Backpack) RebuildInventoryCapacity();

        OnChanged?.Invoke();
        return true;
    }

    public bool TryUnequipToInventory(EquipSlot slot)
    {
        if (slot == EquipSlot.None) return false;
        if (equipped[slot].IsEmpty) return false;

        var item = equipped[slot].item;

        if (!TryAdd(item, 1)) return false;

        equipped[slot] = new ItemStack();

        if (slot == EquipSlot.Backpack) RebuildInventoryCapacity();

        OnChanged?.Invoke();
        return true;
    }

    public bool TryConsumeFromInventory(int index, PlayerStats vitals)
    {
        Debug.Log($"CONSUME called index={index} frame={Time.frameCount}");

        if (!TryRemoveAt(index, 1, out var item, out _))
            return false;

        if (item.itemType != ItemType.Consumable || vitals == null)
            return false;

        vitals.ApplyConsumable(item);
        return true;
    }

    private void RebuildInventoryCapacity()
    {
        int extra = 0;
        var backpack = equipped[EquipSlot.Backpack];
        if (!backpack.IsEmpty && backpack.item != null)
            extra = Mathf.Max(0, backpack.item.extraSlots);

        CurrentSlotCapacity = baseSlots + extra;

        if (Items.Count < CurrentSlotCapacity)
        {
            while (Items.Count < CurrentSlotCapacity) Items.Add(new ItemStack());
        }
        else if (Items.Count > CurrentSlotCapacity)
        {
            for (int i = CurrentSlotCapacity; i < Items.Count; i++)
            {
                if (!Items[i].IsEmpty)
                {
                    CurrentSlotCapacity = Items.Count;
                    return;
                }
            }
            Items.RemoveRange(CurrentSlotCapacity, Items.Count - CurrentSlotCapacity);
        }
    }
}
