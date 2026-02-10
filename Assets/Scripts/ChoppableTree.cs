using UnityEngine;

public class ChoppableTree : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 5f;
    public float currentHealth = 5f;

    [Header("Drops")]
    public GameObject logPrefab;
    public int logsToSpawn = 3;
    public float dropRadius = 1.2f;

    [Header("Optional FX")]
    public GameObject stumpPrefab;
    public GameObject chopFxPrefab;

    private bool _dead;

    private void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        if (currentHealth <= 0f) currentHealth = maxHealth;
    }

    public void ApplyChop(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (_dead) return;

        currentHealth -= damage;

        if (chopFxPrefab != null)
            Instantiate(chopFxPrefab, hitPoint, Quaternion.LookRotation(hitNormal));

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (_dead) return;
        _dead = true;

        // Spawn stump (optional)
        if (stumpPrefab != null)
            Instantiate(stumpPrefab, transform.position, transform.rotation);

        // Spawn logs
        if (logPrefab != null && logsToSpawn > 0)
        {
            for (int i = 0; i < logsToSpawn; i++)
            {
                Vector3 offset = Random.insideUnitSphere;
                offset.y = 0f;
                offset = offset.normalized * Random.Range(0.2f, dropRadius);
                Instantiate(logPrefab, transform.position + offset + Vector3.up * 0.2f, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }
}
