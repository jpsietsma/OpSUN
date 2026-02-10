using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerToolSwing : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HotbarUI hotbar;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private Animator animator;

    [Header("Chop System")]
    [Tooltip("Handles chopping terrain-painted trees by converting instances to interactive prefabs.")]
    [SerializeField] private TerrainTreeChopSystem terrainTreeChopSystem;

    [Header("Damage")]
    [SerializeField] private float damagePerHit = 1f;

    [Header("Animator")]
    [SerializeField] private string swingTriggerName = "SwingPickaxe";

    [Header("Input")]
    [Tooltip("If empty, will use Left Mouse Button")]
    [SerializeField] private InputActionReference swingAction;

    [Header("Tuning")]
    [SerializeField] private float swingCooldown = 0.35f;

    private float _nextSwingTime;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (terrainTreeChopSystem == null)
            terrainTreeChopSystem = GetComponentInChildren<TerrainTreeChopSystem>();
    }

    private void OnEnable()
    {
        if (swingAction != null && swingAction.action != null)
            swingAction.action.Enable();
    }

    private void OnDisable()
    {
        if (swingAction != null && swingAction.action != null)
            swingAction.action.Disable();
    }

    private void Update()
    {
        if (!CanAcceptInput()) return;

        if (!SwingPressedThisFrame()) return;

        var item = GetActiveHotbarItem();
        if (item == null) return;

        // Only tools can swing
        if (item.itemType != ItemType.Tool) return;

        if (Time.time < _nextSwingTime) return;
        _nextSwingTime = Time.time + swingCooldown;

        if (animator != null)
            animator.SetTrigger(swingTriggerName);
    }

    /// <summary>
    /// Animation Event: Call this at the impact frame of your swing animation.
    /// This keeps your existing system (click triggers animation) but applies damage at the right moment.
    /// </summary>
    public void OnToolHit()
    {
        if (!CanAcceptInput()) return;

        var item = GetActiveHotbarItem();
        if (item == null) return;

        // Only tools can hit
        if (item.itemType != ItemType.Tool) return;

        // If you later add tool sub-types (Axe vs Pickaxe), filter here.

        if (terrainTreeChopSystem != null)
            terrainTreeChopSystem.TryChop(damagePerHit);
    }

    private bool CanAcceptInput()
    {
        // In your project, inventory open = cursor unlocked
        if (Cursor.lockState == CursorLockMode.None) return false;
        return true;
    }

    private bool SwingPressedThisFrame()
    {
        if (swingAction != null && swingAction.action != null)
            return swingAction.action.WasPressedThisFrame();

        // Fallback: Left mouse
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private ItemDefinition GetActiveHotbarItem()
    {
        if (hotbar == null || inventory == null) return null;

        int slot = hotbar.SelectedIndex;
        if (slot < 0) return null;

        // Use your binding array
        if (hotbar.boundInventoryIndex == null || slot >= hotbar.boundInventoryIndex.Length) return null;

        int invIdx = hotbar.boundInventoryIndex[slot];
        if (invIdx < 0 || invIdx >= inventory.Items.Count) return null;

        var stack = inventory.Items[invIdx];
        if (stack.IsEmpty || stack.item == null) return null;

        return stack.item;
    }
}
