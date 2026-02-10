using UnityEngine;
using StarterAssets;

[DefaultExecutionOrder(50)]
public class SwimMotor_TPS : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerSwimDetector swimDetector;
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private StarterAssetsInputs inputs;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    [Header("Swim Movement")]
    [SerializeField] private float swimSpeed = 3.5f;
    [SerializeField] private float swimAcceleration = 10f;

    [Header("Buoyancy (Float)")]
    [SerializeField] private float targetCameraSubmerge = 0.35f;
    [SerializeField] private float floatStrength = 6f;
    [SerializeField] private float floatDamping = 4f;

    [Header("Bob")]
    [SerializeField] private float bobAmplitude = 0.06f;
    [SerializeField] private float bobFrequency = 1.2f;

    [Header("Optional vertical control")]
    [SerializeField] private bool allowVerticalControl = true;
    [SerializeField] private float verticalSwimSpeed = 1.75f;

    [Header("Look While Swimming")]
    [SerializeField] private bool allowLookWhileSwimming = true;

    [Tooltip("Assign the Starter Assets Cinemachine camera target (usually a child named CinemachineCameraTarget).")]
    [SerializeField] private Transform cinemachineCameraTarget;

    [Tooltip("How fast the player rotates to camera yaw while swimming.")]
    [SerializeField] private float rotateToCameraYawSpeed = 12f;

    [Tooltip("How fast pitch follows input (degrees per second).")]
    [SerializeField] private float pitchSpeed = 200f;

    [Tooltip("Clamp pitch (degrees). Starter Assets typical values: -30..70")]
    [SerializeField] private float pitchMin = -30f;
    [SerializeField] private float pitchMax = 70f;

    [Header("Animator Params")]
    [SerializeField] private string moveXParam = "MoveX";
    [SerializeField] private string moveZParam = "MoveZ";
    [SerializeField] private string isSwimmingBool = "IsSwimming";

    private bool _wasSwimming;
    private Vector3 _horizontalVel;
    private float _verticalVel;

    private float _pitch; // local pitch for CinemachineCameraTarget

    private void Reset()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        characterController = GetComponent<CharacterController>();
        inputs = GetComponent<StarterAssetsInputs>();
        if (Camera.main != null) cameraTransform = Camera.main.transform;
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (thirdPersonController == null) thirdPersonController = GetComponent<ThirdPersonController>();
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (inputs == null) inputs = GetComponent<StarterAssetsInputs>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (cinemachineCameraTarget != null)
            _pitch = cinemachineCameraTarget.localEulerAngles.x;
        _pitch = NormalizeAngle(_pitch);
    }

    private void Update()
    {
        if (swimDetector == null || characterController == null || inputs == null || cameraTransform == null)
            return;

        bool swimming = swimDetector.IsSwimming;

        if (swimming != _wasSwimming)
        {
            _wasSwimming = swimming;

            if (thirdPersonController != null)
                thirdPersonController.enabled = !swimming;

            _horizontalVel = Vector3.zero;
            _verticalVel = 0f;

            if (animator != null && !string.IsNullOrEmpty(isSwimmingBool))
                animator.SetBool(isSwimmingBool, swimming);

            // Sync pitch when entering swim so there's no jump
            if (swimming && cinemachineCameraTarget != null)
            {
                _pitch = NormalizeAngle(cinemachineCameraTarget.localEulerAngles.x);
            }
        }

        if (!swimming)
            return;

        // 1) KEEP LOOK WORKING (yaw + pitch)
        if (allowLookWhileSwimming)
            UpdateLook();

        // 2) HORIZONTAL SWIM (camera-relative)
        Vector2 move = inputs.move;
        Vector3 inputDir = new Vector3(move.x, 0f, move.y);

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 desiredDir = (camRight * inputDir.x + camForward * inputDir.z);
        if (desiredDir.sqrMagnitude > 1f) desiredDir.Normalize();

        Vector3 desiredVel = desiredDir * swimSpeed;
        _horizontalVel = Vector3.MoveTowards(_horizontalVel, desiredVel, swimAcceleration * Time.deltaTime);

        // 3) BUOYANCY
        float surfaceY = swimDetector.CurrentWaterSurfaceY;
        if (float.IsNegativeInfinity(surfaceY)) return;

        float desiredCamY = surfaceY - targetCameraSubmerge;
        float camY = cameraTransform.position.y;

        float error = desiredCamY - camY;
        float accel = (floatStrength * error) - (floatDamping * _verticalVel);
        _verticalVel += accel * Time.deltaTime;

        // 4) OPTIONAL VERTICAL INPUT
        float userVertical = 0f;
        if (allowVerticalControl)
        {
            if (inputs.jump) userVertical += 1f;
            //if (inputs.crouch) userVertical -= 1f;
        }
        userVertical *= verticalSwimSpeed;

        // 5) BOB
        float bob = Mathf.Sin(Time.time * (Mathf.PI * 2f) * bobFrequency) * bobAmplitude;

        // 6) APPLY MOVE
        Vector3 motion =
            new Vector3(_horizontalVel.x, 0f, _horizontalVel.z) +
            Vector3.up * (_verticalVel + userVertical);

        motion *= Time.deltaTime;
        motion.y += bob;

        characterController.Move(motion);

        // 7) Drive blend tree params
        if (animator != null)
        {
            animator.SetFloat(moveXParam, move.x, 0.1f, Time.deltaTime);
            animator.SetFloat(moveZParam, move.y, 0.1f, Time.deltaTime);
        }
    }

    private void UpdateLook()
    {
        // StarterAssetsInputs.look is a Vector2 from mouse/right-stick
        Vector2 look = inputs.look;

        // Yaw: rotate player root to camera yaw (keeps movement direction sane)
        float targetYaw = cameraTransform.eulerAngles.y;
        Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateToCameraYawSpeed * Time.deltaTime);

        // Pitch: apply to Cinemachine camera target (like Starter Assets does)
        if (cinemachineCameraTarget != null)
        {
            _pitch += -look.y * pitchSpeed * Time.deltaTime; // invert like typical TPS
            _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);

            cinemachineCameraTarget.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }

    private static float NormalizeAngle(float angle)
    {
        // convert 0..360 into -180..180
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}
