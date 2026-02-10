using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class BearHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Death")]
    public float destroyAfterSeconds = 0f; // 0 = do not destroy
    public string deathBoolParam = "IsDead"; // OR use a trigger if you prefer
    public string deathTriggerParam = "Die"; // if you use trigger
    public bool useTriggerInsteadOfBool = false;

    [Header("Disable On Death")]
    public MonoBehaviour[] scriptsToDisable;   // BearWanderAI, BearAttackDamage, etc.
    public Collider[] collidersToDisable;      // bear colliders
    public NavMeshAgent agentToDisable;        // optional, auto-found

    private Animator _animator;
    private bool _dead;

    void Awake()
    {
        currentHealth = maxHealth;

        _animator = GetComponentInChildren<Animator>();
        if (agentToDisable == null)
            agentToDisable = GetComponent<NavMeshAgent>();
    }

    public void TakeDamage(int amount)
    {
        if (_dead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"Bear took {amount}. HP now {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            TakeDamage(999999);

            var flameController = GetComponent<FlameController>();
            flameController.ToggleFlame();
        }            
    }

    private void Die()
    {
        _dead = true;

        var loot = GetComponent<AnimalLootDropper>();
        if (loot != null)
            loot.DropLoot();

        // Stop movement immediately
        if (agentToDisable != null)
        {
            agentToDisable.isStopped = true;
            agentToDisable.velocity = Vector3.zero;
            agentToDisable.enabled = false;
        }

        // Stop AI/attack scripts
        if (scriptsToDisable != null)
        {
            foreach (var s in scriptsToDisable)
                if (s != null) s.enabled = false;
        }

        // Disable colliders so it doesn't block / can't be re-hit
        if (collidersToDisable != null)
        {
            foreach (var c in collidersToDisable)
                if (c != null) c.enabled = false;
        }

        // Play death animation
        if (_animator != null)
        {
            if (useTriggerInsteadOfBool)
                _animator.SetTrigger(deathTriggerParam);
            else
                _animator.SetBool(deathBoolParam, true);
        }

        if (destroyAfterSeconds > 0f)
            Destroy(gameObject, destroyAfterSeconds);
    }

    public bool IsDead => _dead;
}