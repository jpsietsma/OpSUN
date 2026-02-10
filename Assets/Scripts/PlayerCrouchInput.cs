using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;

public class PlayerCrouchInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Input")]
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private StarterAssetsInputs inputs;

    [SerializeField] private string moveXParam = "MoveX";
    [SerializeField] private string moveZParam = "MoveZ";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (animator != null && inputs != null)
        {
            Vector2 m = inputs.move;
            animator.SetFloat(moveXParam, m.x, 0.1f, Time.deltaTime);
            animator.SetFloat(moveZParam, m.y, 0.1f, Time.deltaTime);

        }
    }

    private void OnEnable()
    {
        crouchAction.action.Enable();
        crouchAction.action.performed += OnCrouchPressed;
        crouchAction.action.canceled += OnCrouchReleased;
    }

    private void OnDisable()
    {
        crouchAction.action.performed -= OnCrouchPressed;
        crouchAction.action.canceled -= OnCrouchReleased;
        crouchAction.action.Disable();
    }

    private void OnCrouchPressed(InputAction.CallbackContext ctx)
    {
        animator.SetBool("IsCrouching", true);
    }

    private void OnCrouchReleased(InputAction.CallbackContext ctx)
    {
        animator.SetBool("IsCrouching", false);
    }
}
