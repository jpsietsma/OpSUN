using UnityEngine;
using StarterAssets;

[DefaultExecutionOrder(200)] // run after Starter Assets movement & camera
public class TPS_SwimInWaterTrigger : MonoBehaviour
{
    [Header("Required Refs (drag from Player)")]
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private StarterAssetsInputs inputs;
    [SerializeField] private Animator animator;

    [Header("Water")]
    [Tooltip("Player is swimming while inside a trigger collider tagged with this.")]
    [SerializeField] private string waterTag = "Water";

    [Tooltip("Starting depth below water surface (meters), measured to the CharacterController CENTER.")]
    [SerializeField] private float startSubmergeDepth = 0.9f;

    [Tooltip("Minimum depth below surface (smaller = closer to surface).")]
    [SerializeField] private float minSubmergeDepth = 0.35f;

    [Tooltip("Maximum depth below surface (bigger = deeper).")]
    [SerializeField] private float maxSubmergeDepth = 3.0f;

    [Tooltip("How quickly depth changes when holding Space/Ctrl.")]
    [SerializeField] private float verticalChangeSpeed = 1.8f;

    [Tooltip("How strongly we correct back to target depth (prevents sinking).")]
    [SerializeField] private float antiSinkStrength = 18f;

    [Header("Swim Speed (uses TPS controller speeds)")]
    [Tooltip("TPS MoveSpeed while swimming (normal swim).")]
    [SerializeField] private float swimMoveSpeed = 2.8f;

    [Tooltip("TPS SprintSpeed while swimming (fast swim).")]
    [SerializeField] private float swimSprintSpeed = 5.0f;

    [Header("Stamina")]
    [Tooltip("Max stamina used for fast swimming.")]
    [SerializeField] private float staminaMax = 5.0f;

    [Tooltip("Stamina drained per second while sprinting in water.")]
    [SerializeField] private float staminaDrainPerSec = 1.25f;

    [Tooltip("Stamina regen per second when not sprinting (in water or out).")]
    [SerializeField] private float staminaRegenPerSec = 0.9f;

    [Tooltip("If stamina hits 0, you must regen to at least this before sprint works again.")]
    [SerializeField] private float staminaResumeThreshold = 0.35f;

    [Header("Animation Params")]
    [SerializeField] private string isSwimmingBool = "IsSwimming";
    [SerializeField] private string moveXParam = "MoveX";

    [SerializeField] private string moveZParam = "MoveZ";

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = false;

    // Public read-only (handy for UI later)
    public bool IsSwimming { get; private set; }
    public float Stamina01 => staminaMax <= 0f ? 0f : Mathf.Clamp01(_stamina / staminaMax);

    private Collider _currentWaterTrigger;
    private float _waterSurfaceY = float.NegativeInfinity;

    private float _targetSubmergeDepth;
    private float _stamina;
    private bool _sprintLockedOut;

    // Store original TPS speeds so we can restore when leaving water
    private float _origMoveSpeed;
    private float _origSprintSpeed;
    private bool _savedOrigSpeeds;

    private void Reset()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        characterController = GetComponent<CharacterController>();
        inputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (thirdPersonController == null) thirdPersonController = GetComponent<ThirdPersonController>();
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (inputs == null) inputs = GetComponent<StarterAssetsInputs>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        _stamina = staminaMax;
        _targetSubmergeDepth = Mathf.Clamp(startSubmergeDepth, minSubmergeDepth, maxSubmergeDepth);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(waterTag)) return;

        _currentWaterTrigger = other;
        _waterSurfaceY = GetSurfaceYFromTrigger(other);

        SetSwimming(true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(waterTag)) return;

        _currentWaterTrigger = other;
        _waterSurfaceY = GetSurfaceYFromTrigger(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_currentWaterTrigger == other)
        {
            _currentWaterTrigger = null;
            _waterSurfaceY = float.NegativeInfinity;
            SetSwimming(false);
        }
    }

    private void Update()
    {
        // Stamina updates always (so it regens on land too)
        UpdateStamina();

        if (!IsSwimming)
            return;

        // Depth control: Space = up (shallower), Ctrl = down (deeper)
        float depthDelta = 0f;
        if (inputs != null)
        {
            if (inputs.jump) depthDelta -= 1f;   // up => reduce depth
            if (inputs.crouch) depthDelta += 1f; // down => increase depth
        }

        if (Mathf.Abs(depthDelta) > 0.01f)
        {
            _targetSubmergeDepth += depthDelta * verticalChangeSpeed * Time.deltaTime;
            _targetSubmergeDepth = Mathf.Clamp(_targetSubmergeDepth, minSubmergeDepth, maxSubmergeDepth);
        }

        // Drive your swim blend tree floats using Starter Assets move input
        if (animator != null && inputs != null)
        {
            Vector2 m = inputs.move;
            animator.SetFloat(moveXParam, m.x, 0.1f, Time.deltaTime);
            animator.SetFloat(moveZParam, m.y, 0.1f, Time.deltaTime);

        }

        // Apply swim speeds to TPS controller (keeps look working)
        ApplySwimSpeeds();
    }

    private void LateUpdate()
    {
        if (!IsSwimming) return;
        if (_currentWaterTrigger == null) return;
        if (characterController == null) return;

        if (float.IsNegativeInfinity(_waterSurfaceY))
            _waterSurfaceY = GetSurfaceYFromTrigger(_currentWaterTrigger);

        // Keep controller CENTER at (surfaceY - targetDepth)
        float targetCenterY = _waterSurfaceY - _targetSubmergeDepth;

        float currentCenterY = transform.position.y + characterController.center.y;
        float error = targetCenterY - currentCenterY;

        // Strong upward correction if we're too low (prevents sinking)
        float correction = error;

        if (error > 0f)
        {
            // Below desired depth => push up strongly
            correction = Mathf.Min(error * antiSinkStrength, 2.0f);
        }
        else
        {
            // Above desired depth => gentle pull down (prevents bobbing above surface)
            correction = Mathf.Max(error, -0.35f);
        }

        characterController.Move(new Vector3(0f, correction, 0f));
    }

    private void UpdateStamina()
    {
        bool sprintRequested = inputs != null && inputs.sprint;
        bool sprintingInWater = IsSwimming && sprintRequested && !_sprintLockedOut;

        if (sprintingInWater && staminaMax > 0f)
        {
            _stamina -= staminaDrainPerSec * Time.deltaTime;
            if (_stamina <= 0f)
            {
                _stamina = 0f;
                _sprintLockedOut = true; // lock sprint until we regen a bit
            }
        }
        else
        {
            _stamina += staminaRegenPerSec * Time.deltaTime;
            _stamina = Mathf.Min(_stamina, staminaMax);

            if (_sprintLockedOut && _stamina >= staminaResumeThreshold)
                _sprintLockedOut = false;
        }

        // If sprint is locked out, force it off so TPS doesn't sprint on land/water by accident
        if (_sprintLockedOut && inputs != null)
            inputs.sprint = false;
    }

    private void ApplySwimSpeeds()
    {
        if (thirdPersonController == null) return;

        // Save original speeds once when we first enter swim
        if (!_savedOrigSpeeds)
        {
            _origMoveSpeed = thirdPersonController.MoveSpeed;
            _origSprintSpeed = thirdPersonController.SprintSpeed;
            _savedOrigSpeeds = true;
        }

        // While swimming: base swim speed always, fast swim only if sprint + stamina available
        thirdPersonController.MoveSpeed = swimMoveSpeed;

        bool sprintAllowed = inputs != null && inputs.sprint && !_sprintLockedOut && _stamina > 0f;
        thirdPersonController.SprintSpeed = sprintAllowed ? swimSprintSpeed : swimMoveSpeed;
    }

    private void RestoreOriginalSpeeds()
    {
        if (thirdPersonController == null) return;
        if (!_savedOrigSpeeds) return;

        thirdPersonController.MoveSpeed = _origMoveSpeed;
        thirdPersonController.SprintSpeed = _origSprintSpeed;
        _savedOrigSpeeds = false;
    }

    private void SetSwimming(bool value)
    {
        if (IsSwimming == value) return;
        IsSwimming = value;

        // Reset depth target when entering water
        if (IsSwimming)
        {
            _targetSubmergeDepth = Mathf.Clamp(startSubmergeDepth, minSubmergeDepth, maxSubmergeDepth);
        }
        else
        {
            // Leaving water: restore original TPS speeds
            RestoreOriginalSpeeds();
        }

        if (animator != null && !string.IsNullOrWhiteSpace(isSwimmingBool))
            animator.SetBool(isSwimmingBool, IsSwimming);

        if (logStateChanges)
            Debug.Log($"[TPS_SwimInWaterTrigger] IsSwimming = {IsSwimming}");
    }

    private float GetSurfaceYFromTrigger(Collider waterTrigger)
    {
        // bounds.max.y is the top face of the trigger volume in world space
        return waterTrigger.bounds.max.y;
    }
}
