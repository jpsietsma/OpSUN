using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BearWanderAI : MonoBehaviour
{
    private enum State { Wander, Idle, Chase, Attack }

    [Header("References")]
    public Transform player;              // assign in Inspector (recommended)
    public string playerTag = "Player";   // fallback if player not assigned

    [Header("Detection")]
    public float aggroRadius = 12f;
    public float loseRadius = 18f;
    public float loseSightTime = 2.0f;

    [Header("Attack")]
    public float attackRange = 2.2f;
    public float attackCooldown = 2.0f;
    public float attackLockTime = 0.9f; // how long we "commit" to the attack before moving again
    public string attackBoolParam = "IsAttacking"; // matches your Animator parameter name

    [Header("Wander Settings")]
    public float wanderRadius = 20f;
    public float minWanderDistance = 5f;
    public int maxTries = 10;

    [Header("Idle Time")]
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;

    [Header("Movement Speeds")]
    public float walkSpeed = 2f;
    public float runSpeed = 4.5f;

    [Header("Animation")]
    public Animator animator;

    [Header("Optional")]
    public bool faceTargetWhileChasing = true;
    public float turnSpeed = 8f;

    private NavMeshAgent agent;
    private float idleTimer;
    private float loseTimer;
    private float attackCooldownTimer;
    private float attackLockTimer;
    private State state = State.Wander;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Auto-find player by tag if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }

        // Safety: snap to navmesh if needed
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
                transform.position = hit.position;
        }

        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.stoppingDistance = 0.3f;

        PickNewDestination();
        state = State.Wander;
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return;

        // keep animator alive
        if (animator != null)
        {
            animator.enabled = true;
            if (animator.speed == 0f) animator.speed = 1f;
        }

        // timers
        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;

        // Always drive locomotion speed
        UpdateAnimatorSpeed();

        // Global: check aggro (unless already chasing/attacking)
        if ((state == State.Wander || state == State.Idle) && PlayerInAggroRange())
        {
            StartChase();
        }

        switch (state)
        {
            case State.Attack:
                UpdateAttack();
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Idle:
                UpdateIdle();
                break;

            case State.Wander:
                UpdateWander();
                break;
        }
    }

    // -------------------------
    // Wander / Idle
    // -------------------------

    private void UpdateWander()
    {
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            StartIdle();
        }
    }

    private void StartIdle()
    {
        state = State.Idle;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
        agent.isStopped = true;

        if (animator != null) animator.SetFloat("Speed", 0f);
        SetAttacking(false);
    }

    private void UpdateIdle()
    {
        // If player comes close, chase will start via global check.
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            PickNewDestination();
            state = State.Wander;
        }
    }

    private void PickNewDestination()
    {
        Vector3 dest;
        if (TryGetRandomDestination(out dest))
        {
            agent.isStopped = false;
            agent.speed = walkSpeed;
            agent.stoppingDistance = 0.3f;
            agent.SetDestination(dest);
            SetAttacking(false);
        }
        else
        {
            StartIdle();
        }
    }

    private bool TryGetRandomDestination(out Vector3 result)
    {
        Vector3 origin = transform.position;

        for (int i = 0; i < maxTries; i++)
        {
            Vector3 rand = origin + Random.insideUnitSphere * wanderRadius;
            rand.y = origin.y;

            if (NavMesh.SamplePosition(rand, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                if (Vector3.Distance(origin, hit.position) >= minWanderDistance)
                {
                    result = hit.position;
                    return true;
                }
            }
        }

        result = origin;
        return false;
    }

    // -------------------------
    // Chase
    // -------------------------

    private bool PlayerInAggroRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= aggroRadius;
    }

    private void StartChase()
    {
        state = State.Chase;
        loseTimer = 0f;

        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.stoppingDistance = Mathf.Max(attackRange * 0.9f, 1.5f);

        SetAttacking(false);
    }

    private void UpdateChase()
    {
        if (player == null)
        {
            GiveUpChase();
            return;
        }

        // Keep chasing
        agent.SetDestination(player.position);

        // Face player while chasing
        if (faceTargetWhileChasing)
        {
            FaceTarget(player.position, turnSpeed);
        }

        float dist = Vector3.Distance(transform.position, player.position);

        // ATTACK CHECK
        if (dist <= attackRange && attackCooldownTimer <= 0f)
        {
            StartAttack();
            return;
        }

        // Lose logic
        if (dist > loseRadius)
        {
            loseTimer += Time.deltaTime;
            if (loseTimer >= loseSightTime)
            {
                GiveUpChase();
            }
        }
        else
        {
            loseTimer = 0f;
        }
    }

    private void GiveUpChase()
    {
        agent.stoppingDistance = 0.3f;
        SetAttacking(false);
        PickNewDestination();
        state = State.Wander;
    }

    // -------------------------
    // Attack
    // -------------------------

    private void StartAttack()
    {
        state = State.Attack;

        // Stop moving while attacking
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Face player before swing
        if (player != null)
            FaceTarget(player.position, turnSpeed * 2f);

        // Trigger animator
        SetAttacking(true);

        // Lock in place for a short time
        attackLockTimer = attackLockTime;

        // Cooldown before next attack
        attackCooldownTimer = attackCooldown;
    }

    private void UpdateAttack()
    {
        // Keep facing player during the swing (optional)
        if (player != null)
            FaceTarget(player.position, turnSpeed * 2f);

        attackLockTimer -= Time.deltaTime;
        if (attackLockTimer <= 0f)
        {
            // End attack and resume chase (or give up if player is gone)
            SetAttacking(false);

            if (player == null)
            {
                GiveUpChase();
                return;
            }

            agent.isStopped = false;
            agent.speed = runSpeed;
            agent.stoppingDistance = Mathf.Max(attackRange * 0.9f, 1.5f);
            state = State.Chase;
        }
    }

    private void SetAttacking(bool value)
    {
        if (animator == null) return;
        animator.SetBool(attackBoolParam, value);
    }

    private void FaceTarget(Vector3 worldPos, float slerpSpeed)
    {
        Vector3 to = worldPos - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(to);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * slerpSpeed);
    }

    // -------------------------
    // Animator
    // -------------------------

    private void UpdateAnimatorSpeed()
    {
        if (animator == null) return;

        // During attack, we don't want speed-based transitions to fight the attack
        if (state == State.Attack)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        float speed = agent.isStopped ? 0f : agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }

    // -------------------------
    // Gizmos
    // -------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
