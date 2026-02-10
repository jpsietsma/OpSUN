using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject root;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private ItemTooltipUI tooltipUI;

    [Header("Inventory Slot Buttons (create 40 in UI, script will hide extra)")]
    [SerializeField] private Button[] inventorySlotButtons;
    [SerializeField] private Image[] inventorySlotIcons;
    [SerializeField] private TMP_Text[] inventorySlotCounts;

    [Header("Equipment Slots")]
    [SerializeField] private EquipSlotButton[] equipSlotButtons;

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string gameplayMapName = "Player";
    [SerializeField] private string uiMapName = "UI";

    private bool isOpen;

    //private void OnEnable() => inventory.OnChanged += Refresh;
    //private void OnDisable() => inventory.OnChanged -= Refresh;

    private void OnEnable()
    {
        if (inventory != null) inventory.OnChanged += Refresh;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
    }

    private void Start()
    {
        root.SetActive(false);
        WireButtons();
        Refresh();
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        root.SetActive(isOpen);

        if (!isOpen)
            tooltipUI.Hide();

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap(isOpen ? uiMapName : gameplayMapName);

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;

        Refresh();
    }

    private bool _wired;

    private void WireButtons()
    {
        if (_wired) return;
        _wired = true;

        for (int i = 0; i < inventorySlotButtons.Length; i++)
        {
            int idx = i;

            // prevent duplicate listeners if WireButtons ever runs again
            inventorySlotButtons[i].onClick.RemoveAllListeners();
            inventorySlotButtons[i].onClick.AddListener(() => OnInventorySlotLeftClick(idx));

            // drag support
            var drag = inventorySlotButtons[i].GetComponent<InventorySlotDragSource>();
            if (drag == null) drag = inventorySlotButtons[i].gameObject.AddComponent<InventorySlotDragSource>();
            drag.inventory = inventory;
            drag.inventoryIndex = idx;
        }

        foreach (var e in equipSlotButtons)
        {
            var slot = e.slot;

            // prevent duplicate listeners if WireButtons ever runs again
            e.button.onClick.RemoveAllListeners();
            e.button.onClick.AddListener(() => inventory.TryUnequipToInventory(slot));
        }
    }

    public void OnInventorySlotLeftClick(int idx)
    {
        Debug.Log($"OnInventorySlotLeftClick idx={idx} frame={Time.frameCount}\n{System.Environment.StackTrace}");

        if (!isOpen) return;

        if (idx < 0 || idx >= inventory.Items.Count) return;

        var stack = inventory.Items[idx];
        if (stack.IsEmpty || stack.item == null) return;

        if (stack.item.itemType == ItemType.Consumable)
        {
            inventory.TryConsumeFromInventory(idx, inventory.playerVitals);
            return;
        }       


        // Equipment/backpack: keep your existing equip behavior
        inventory.TryEquipFromInventory(idx);
    }

    private void Refresh()
    {
        int cap = inventory.CurrentSlotCapacity;

        for (int i = 0; i < inventorySlotButtons.Length; i++)
        {
            bool active = i < cap;
            inventorySlotButtons[i].gameObject.SetActive(active);
            if (!active) continue;

            var stack = inventory.Items[i];

            // Tooltip hookup (hover)
            var trigger = inventorySlotButtons[i].GetComponent<ItemTooltipTrigger>();
            if (trigger == null) trigger = inventorySlotButtons[i].gameObject.AddComponent<ItemTooltipTrigger>();

            trigger.tooltipUI = tooltipUI;
            trigger.item = (!stack.IsEmpty) ? stack.item : null;

            if (stack.IsEmpty)
            {
                inventorySlotIcons[i].enabled = false;
                inventorySlotCounts[i].text = "";
            }
            else
            {
                inventorySlotIcons[i].enabled = true;
                inventorySlotIcons[i].sprite = stack.item.icon;
                inventorySlotCounts[i].text = stack.item.maxStack > 1 && stack.count > 1 ? stack.count.ToString() : "";
            }
        }

        foreach (var e in equipSlotButtons)
        {
            var stack = inventory.GetEquipped(e.slot);
            if (stack.IsEmpty)
            {
                e.icon.enabled = false;
                e.count.text = "";
            }
            else
            {
                e.icon.enabled = true;
                e.icon.sprite = stack.item.icon;
                e.count.text = "";
            }
        }
    }
}

[System.Serializable]
public class EquipSlotButton
{
    public EquipSlot slot;
    public Button button;
    public Image icon;
    public TMP_Text count;
}
