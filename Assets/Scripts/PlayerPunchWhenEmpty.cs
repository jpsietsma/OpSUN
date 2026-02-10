using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPunchWhenHandsEmpty : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform handSocket;     // drag HandSocket here
    [SerializeField] private Animator animator;        // drag model animator or leave blank
    [SerializeField] private PlayerStats playerStats;

    [Header("Animator")]
    [SerializeField] private string punchTriggerName = "Punch";

    [Header("Cooldown")]
    [SerializeField] private float punchCooldown = 0.45f;

    [Header("Optional")]
    [SerializeField] private bool blockWhenCursorUnlocked = true;

    [Header("Punch Lock")]
    [Tooltip("If you forget to add the PunchEnd animation event, this will still unlock after this time.")]
    [SerializeField] private float fallbackUnlockSeconds = 0.75f;

    private float _nextPunchTime;
    private bool _punchInProgress;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerStats == null)
            playerStats = GetComponentInChildren<PlayerStats>();

        if (handSocket == null)
        {
            // Auto-find a transform named "HandSocket"
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "HandSocket")
                {
                    handSocket = t;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (blockWhenCursorUnlocked && Cursor.lockState != CursorLockMode.Locked)
            return;

        // NEW: don't allow starting another punch while one is already in progress
        if (_punchInProgress)
            return;

        if (Time.time < _nextPunchTime)
            return;

        // If ANYTHING is in the HandSocket, we assume you're holding an item.
        if (handSocket != null && handSocket.childCount > 0)
            return;

        // Require stamina to punch (spend ONCE, when starting the punch)
        if (playerStats != null && !playerStats.TrySpendStamina(playerStats.punchStaminaCost))
            return;

        _nextPunchTime = Time.time + punchCooldown;

        _punchInProgress = true;

        // Fallback unlock in case you forget the PunchEnd animation event
        if (fallbackUnlockSeconds > 0f)
            Invoke(nameof(UnlockPunch), fallbackUnlockSeconds);

        if (animator != null)
            animator.SetTrigger(punchTriggerName);
    }

    private void UnlockPunch()
    {
        _punchInProgress = false;
    }

    // OPTIONAL: If you already have PunchStart event, you can call this too.
    public void AnimEvent_PunchStart()
    {
        _punchInProgress = true;
        CancelInvoke(nameof(UnlockPunch));
        if (fallbackUnlockSeconds > 0f)
            Invoke(nameof(UnlockPunch), fallbackUnlockSeconds);
    }

    // IMPORTANT: Add an Animation Event at the end of the punch clip to call this.
    public void AnimEvent_PunchEnd()
    {
        CancelInvoke(nameof(UnlockPunch));
        _punchInProgress = false;
    }
}
