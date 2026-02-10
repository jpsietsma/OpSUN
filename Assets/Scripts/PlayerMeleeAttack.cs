using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCamera;                 // drag your player camera here
    public Transform attackOriginOverride;       // optional (hand/weapon origin)

    [Header("Attack Settings")]
    public float range = 2.2f;
    public float radius = 0.35f;                // spherecast thickness
    public int damage = 25;
    public float cooldown = 0.6f;

    [Header("Hit Filtering")]
    public LayerMask hittableLayers = ~0;        // set to Animals layer
    public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Optional Keyboard Fallback")]
    public bool allowKeyboardKey = false;
    public Key keyboardAttackKey = Key.F;

    private float _nextAttackTime;

    void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        // New Input System devices
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool keyPressed = allowKeyboardKey && Keyboard.current != null && Keyboard.current[keyboardAttackKey].wasPressedThisFrame;

        if (mousePressed || keyPressed)
            TryAttack();
    }

    private void TryAttack()
    {
        if (Time.time < _nextAttackTime) return;
        _nextAttackTime = Time.time + cooldown;

        Transform originT = attackOriginOverride != null
            ? attackOriginOverride
            : (playerCamera != null ? playerCamera.transform : transform);

        Vector3 origin = originT.position;
        Vector3 dir = originT.forward;

        if (Physics.SphereCast(origin, radius, dir, out RaycastHit hit, range, hittableLayers, triggerInteraction))
        {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(damage);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Transform originT = attackOriginOverride != null
            ? attackOriginOverride
            : (playerCamera != null ? playerCamera.transform : transform);

        if (originT == null) return;

        Gizmos.color = Color.cyan;
        Vector3 start = originT.position;
        Vector3 end = start + originT.forward * range;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, radius);
    }
#endif
}
