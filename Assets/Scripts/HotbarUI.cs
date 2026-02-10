using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class HotbarUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Image slotBackground;
        public Image icon;
        public TMP_Text count;
    }

    public event Action<int> OnSelectionChanged;

    [Header("Inventory Sync")]
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private InventoryUI inventoryUI; // optional, just for "only allow drag when open"
    public int[] boundInventoryIndex = new int[8]; // -1 = empty

    [Header("UI")]
    public SlotUI[] slots = new SlotUI[8];

    [Header("Visuals")]
    public Color normalColor = new Color(1, 1, 1, 0.6f);
    public Color selectedColor = new Color(1, 1, 1, 1f);

    public int SelectedIndex { get; private set; }

    void Start()
    {
        SelectedIndex = 0;

        for (int i = 0; i < boundInventoryIndex.Length; i++)
            boundInventoryIndex[i] = -1;

        if (inventory != null)
            inventory.OnChanged += RefreshAll;

        RefreshAll();
        RefreshSelection();
    }

    void Update()
    {
        HandleNumberKeys();
        HandleScrollWheel();
    }

    void HandleNumberKeys()
    {
        if (Keyboard.current == null) return;

        // 1–8 -> slots 0–7
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SetSelected(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) SetSelected(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) SetSelected(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) SetSelected(3);
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) SetSelected(4);
        else if (Keyboard.current.digit6Key.wasPressedThisFrame) SetSelected(5);
        else if (Keyboard.current.digit7Key.wasPressedThisFrame) SetSelected(6);
        else if (Keyboard.current.digit8Key.wasPressedThisFrame) SetSelected(7);
    }

    void HandleScrollWheel()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        int dir = scroll > 0 ? -1 : 1; // scroll up = previous slot
        int next = (SelectedIndex + dir) % slots.Length;
        if (next < 0) next += slots.Length;

        SetSelected(next);
    }

    public ItemStack GetBoundStack(int hotbarSlot)
    {
        if (inventory == null) return new ItemStack();
        if (hotbarSlot < 0 || hotbarSlot >= boundInventoryIndex.Length)
            return new ItemStack();

        int invIdx = boundInventoryIndex[hotbarSlot];
        if (invIdx < 0 || invIdx >= inventory.Items.Count)
            return new ItemStack();

        return inventory.Items[invIdx];
    }

    public void SetSelected(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        if (SelectedIndex == index) return;

        SelectedIndex = index;
        RefreshSelection();

        OnSelectionChanged?.Invoke(SelectedIndex);
        // Hook point: equip/use selected item here
        // Debug.Log($"Selected slot: {SelectedIndex} item: {(items[SelectedIndex] ? items[SelectedIndex].displayName : "None")}");
    }

    void RefreshAll()
    {
        for (int i = 0; i < slots.Length; i++)
            RefreshSlot(i);

        // Clear bindings that point to invalid/empty inventory slots
        for (int i = 0; i < boundInventoryIndex.Length; i++)
        {
            int invIdx = boundInventoryIndex[i];
            if (invIdx < 0 || inventory == null) continue;

            if (invIdx >= inventory.Items.Count || inventory.Items[invIdx].IsEmpty)
                boundInventoryIndex[i] = -1;
        }
    }

    void RefreshSlot(int i)
    {
        var s = slots[i];

        ItemStack stack = new ItemStack();

        if (inventory != null && boundInventoryIndex != null && i < boundInventoryIndex.Length)
        {
            int invIdx = boundInventoryIndex[i];
            if (invIdx >= 0 && invIdx < inventory.Items.Count)
                stack = inventory.Items[invIdx];
        }

        if (s.icon != null)
        {
            bool hasIcon = !stack.IsEmpty && stack.item != null && stack.item.icon != null;
            s.icon.enabled = hasIcon;
            s.icon.sprite = hasIcon ? stack.item.icon : null;
        }

        if (s.count != null)
        {
            bool showCount = !stack.IsEmpty && stack.item != null && stack.item.maxStack > 1 && stack.count > 1;
            s.count.gameObject.SetActive(showCount);
            if (showCount) s.count.text = stack.count.ToString();
        }
    }

    void RefreshSelection()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].slotBackground != null)
                slots[i].slotBackground.color = (i == SelectedIndex) ? selectedColor : normalColor;
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnChanged -= RefreshAll;
    }

    public void SetBinding(int hotbarSlot, int inventoryIndex)
    {
        if (hotbarSlot < 0 || hotbarSlot >= boundInventoryIndex.Length) return;

        // allow clearing with -1
        boundInventoryIndex[hotbarSlot] = inventoryIndex;
        RefreshSlot(hotbarSlot);
    }

    public void SwapBinding(int hotbarSlotA, int hotbarSlotB)
    {
        if (hotbarSlotA < 0 || hotbarSlotA >= boundInventoryIndex.Length) return;
        if (hotbarSlotB < 0 || hotbarSlotB >= boundInventoryIndex.Length) return;

        int tmp = boundInventoryIndex[hotbarSlotA];
        boundInventoryIndex[hotbarSlotA] = boundInventoryIndex[hotbarSlotB];
        boundInventoryIndex[hotbarSlotB] = tmp;

        RefreshSlot(hotbarSlotA);
        RefreshSlot(hotbarSlotB);
    }
}
