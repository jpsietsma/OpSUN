using UnityEngine;

public class BearAttackDamage : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 15;

    [Header("Hit Check")]
    public Transform hitOrigin;                 // optional: a child transform in front of mouth/claws
    public float hitRadius = 1.2f;
    public float hitForwardOffset = 1.0f;
    public LayerMask targetLayers = ~0;         // set to Player layer for best results

    [Header("Safety")]
    public float perAttackHitCooldown = 0.2f;   // prevents double-hit if multiple events
    private float _nextAllowedHitTime = 0f;

    // This is the method you will call from the animation event
    public void AttackHit()
    {
        if (Time.time < _nextAllowedHitTime) return;
        _nextAllowedHitTime = Time.time + perAttackHitCooldown;

        Vector3 origin = (hitOrigin != null) ? hitOrigin.position : transform.position;
        Vector3 forward = transform.forward;

        // Put the hit sphere in front of the bear
        Vector3 hitPos = origin + forward * hitForwardOffset;

        Collider[] hits = Physics.OverlapSphere(hitPos, hitRadius, targetLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            // Prefer IDamageable on the hit object or its parents
            IDamageable dmg = hits[i].GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damage);
                break; // one target per swing
            }
        }
    }

    // Debug visualization in editor
    void OnDrawGizmosSelected()
    {
        Vector3 origin = (hitOrigin != null) ? hitOrigin.position : transform.position;
        Vector3 hitPos = origin + transform.forward * hitForwardOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPos, hitRadius);
    }
}