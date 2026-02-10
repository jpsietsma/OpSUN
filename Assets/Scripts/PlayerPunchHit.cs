using UnityEngine;

public class PlayerPunchHit : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Transform hitOrigin;   // where the punch check starts (hand, chest, etc.)

    [Header("Hit Settings")]
    [SerializeField] private float hitRadius = 0.35f;
    [SerializeField] private float hitRange = 1.0f;
    [SerializeField] private LayerMask hitMask = ~0; // set this to Enemy layer(s) in Inspector

    [Header("Anti multi-hit per punch")]
    [SerializeField] private bool onlyOneHitPerPunch = true;

    private bool _hasHitThisPunch;

    private void Awake()
    {
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
        if (hitOrigin == null) hitOrigin = transform; // fallback
    }

    // Call this from an Animation Event at the impact frame
    public void AnimEvent_PunchHit()
    {
        if (playerStats == null) return;

        if (onlyOneHitPerPunch && _hasHitThisPunch)
            return;

        Vector3 origin = hitOrigin.position;
        Vector3 center = origin + transform.forward * hitRange;

        Collider[] hits = Physics.OverlapSphere(center, hitRadius, hitMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return;

        // Optional: choose closest
        Collider best = hits[0];
        float bestDist = (best.transform.position - origin).sqrMagnitude;

        for (int i = 1; i < hits.Length; i++)
        {
            float d = (hits[i].transform.position - origin).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = hits[i];
            }
        }

        if (best.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(playerStats.punchDamage);
            _hasHitThisPunch = true;
        }
    }

    // Call this from an Animation Event at the start of the punch (or when trigger fires)
    public void AnimEvent_PunchStart()
    {
        _hasHitThisPunch = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform t = hitOrigin != null ? hitOrigin : transform;
        Vector3 center = t.position + transform.forward * hitRange;
        Gizmos.DrawWireSphere(center, hitRadius);
    }
#endif
}
