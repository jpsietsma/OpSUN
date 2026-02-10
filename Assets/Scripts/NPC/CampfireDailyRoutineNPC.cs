using UnityEngine;
using UnityEngine.AI;

public class CampfireDailyRoutineNPC : MonoBehaviour
{
    public enum State
    {
        DayWander,
        GoingToCampfire,
        LightingFire,
        GoingToSit,
        SittingNight
    }

    [Header("Day / Night")]
    public DayNightCycle dayNight;
    [Range(0f, 1f)] public float goHomeAtTime01 = 0.70f;
    [Range(0f, 1f)] public float morningAtTime01 = 0.28f;

    [Header("Navigation")]
    public NavMeshAgent agent;
    public Transform wanderCenter;
    public float wanderRadius = 15f;
    public Vector2 wanderPauseRange = new Vector2(1.5f, 5f);

    [Header("Campfire Points")]
    public Transform campfireGoToPoint;  // where NPC stands to light fire
    public Transform sitWalkPoint;       // a point ON the NavMesh near the log (walk target)
    public Transform sitSnapPoint;       // exact sit alignment point (can be the same as sitWalkPoint)
    public Transform lookAtPoint;        // optional

    [Header("Campfire Controller")]
    public FlameController flameController;

    [Header("Animator")]
    public Animator animator;
    public string speedParam = "Speed";
    public string sitBoolParam = "IsSitting";
    public string lightTriggerParam = "LightFire"; // optional trigger to start your LightFire chain

    [Header("Distances / Timing")]
    public float arriveDistance = 0.8f;
    public float sitArriveDistance = 0.6f;
    public float lightFireDuration = 2.0f; // if you don’t have animation events, this is a simple timer

    private State state;
    private float pauseUntil;
    private float lightUntil;

    private bool fireIsLit;

    void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        // IMPORTANT: NavMesh drives motion
        if (animator) animator.applyRootMotion = false;
    }

    void Start()
    {
        state = State.DayWander;
        SetSitting(false);
        fireIsLit = false;
        PickNewWanderDestination();
    }

    void Update()
    {
        if (!dayNight || !agent || !animator) return;

        float t = dayNight.time01;

        UpdateAnimatorSpeed();

        switch (state)
        {
            case State.DayWander:
                UpdateDayWander(t);
                break;

            case State.GoingToCampfire:
                UpdateGoingToCampfire(t);
                break;

            case State.LightingFire:
                UpdateLightingFire(t);
                break;

            case State.GoingToSit:
                UpdateGoingToSit(t);
                break;

            case State.SittingNight:
                UpdateSittingNight(t);
                break;
        }
    }

    // -------- Day Wander --------

    void UpdateDayWander(float t)
    {
        if (IsTimeToGoHome(t))
        {
            GoToCampfire();
            return;
        }

        if (Time.time < pauseUntil) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            pauseUntil = Time.time + Random.Range(wanderPauseRange.x, wanderPauseRange.y);
            PickNewWanderDestination();
        }
    }

    void PickNewWanderDestination()
    {
        if (!wanderCenter) return;

        Vector3 random = wanderCenter.position + Random.insideUnitSphere * wanderRadius;
        random.y = wanderCenter.position.y;

        if (NavMesh.SamplePosition(random, out var hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // -------- Go to Campfire --------

    void GoToCampfire()
    {
        state = State.GoingToCampfire;
        SetSitting(false);

        agent.isStopped = false;

        if (campfireGoToPoint)
            agent.SetDestination(campfireGoToPoint.position);
    }

    void UpdateGoingToCampfire(float t)
    {
        if (IsMorning(t))
        {
            StartDay();
            return;
        }

        if (!campfireGoToPoint) return;

        if (!agent.pathPending && agent.remainingDistance <= arriveDistance)
            BeginLightFire();
    }

    // -------- Light Fire --------

    void BeginLightFire()
    {
        state = State.LightingFire;

        agent.isStopped = true;
        agent.ResetPath();
        FaceLookAt();

        if (!string.IsNullOrEmpty(lightTriggerParam))
            animator.SetTrigger(lightTriggerParam);

        lightUntil = Time.time + lightFireDuration;
    }

    void UpdateLightingFire(float t)
    {
        FaceLookAt();

        if (IsMorning(t))
        {
            StartDay();
            return;
        }

        if (Time.time >= lightUntil)
        {
            // Turn fire ON once
            if (!fireIsLit && flameController != null)
            {
                flameController.ToggleFlame();
                fireIsLit = true;
            }

            GoToSit();
        }
    }

    // -------- Walk to Log --------

    void GoToSit()
    {
        state = State.GoingToSit;

        agent.isStopped = false;

        if (sitWalkPoint)
            agent.SetDestination(sitWalkPoint.position);
    }

    void UpdateGoingToSit(float t)
    {
        if (IsMorning(t))
        {
            StartDay();
            return;
        }

        if (!sitWalkPoint) return;

        if (!agent.pathPending && agent.remainingDistance <= sitArriveDistance)
            BeginSit();
    }

    // -------- Sit Night --------

    void BeginSit()
    {
        state = State.SittingNight;

        agent.isStopped = true;
        agent.ResetPath();

        // Snap only AFTER walking there (optional but recommended)
        if (sitSnapPoint != null)
        {
            agent.Warp(sitSnapPoint.position);
            transform.rotation = sitSnapPoint.rotation;
        }

        SetSitting(true);
    }

    void UpdateSittingNight(float t)
    {
        FaceLookAt();

        if (IsMorning(t))
        {
            SetSitting(false);

            // Turn fire OFF once
            if (fireIsLit && flameController != null)
            {
                flameController.ToggleFlame();
                fireIsLit = false;
            }

            StartDay();
        }
    }

    // -------- Reset to Day --------

    void StartDay()
    {
        state = State.DayWander;
        SetSitting(false);

        agent.isStopped = false;
        PickNewWanderDestination();
    }

    // -------- Helpers --------

    void UpdateAnimatorSpeed()
    {
        float normalized = agent.speed > 0.01f ? (agent.velocity.magnitude / agent.speed) : 0f;

        if (agent.isStopped || state == State.LightingFire || state == State.SittingNight)
            normalized = 0f;

        animator.SetFloat(speedParam, Mathf.Clamp01(normalized));
    }

    void SetSitting(bool sit)
    {
        if (!string.IsNullOrEmpty(sitBoolParam))
            animator.SetBool(sitBoolParam, sit);
    }

    void FaceLookAt()
    {
        if (!lookAtPoint) return;

        Vector3 dir = lookAtPoint.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion target = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 4f);
    }

    bool IsTimeToGoHome(float t) => t >= goHomeAtTime01 && t < 0.99f;

    bool IsMorning(float t) => t >= morningAtTime01 && t < goHomeAtTime01;
}
