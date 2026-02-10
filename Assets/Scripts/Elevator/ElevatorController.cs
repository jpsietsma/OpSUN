using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ElevatorController : MonoBehaviour
{
    [Header("State")]
    [Tooltip("Current floor index the elevator is at (0-based).")]
    public int currentFloor = 0;

    [Header("Elevator Car")]
    public GameObject elevatorCar;

    [Header("Floors / Stops (order = floor index)")]
    public List<Transform> floorStops = new List<Transform>();

    [Header("Buttons")]
    [Tooltip("Outside call buttons (order = floor index). Can be a parent; collider can be on a child.")]
    public List<GameObject> outsideSwitches = new List<GameObject>();

    [Tooltip("Inside button that moves elevator to the other/next floor. Can be a parent; collider can be on a child.")]
    public GameObject insideNextSwitch;

    [Header("Movement")]
    public float moveSpeed = 2.0f;
    public float arriveDistance = 0.02f;

    [Header("Doors (inside + outside)")]
    public Animator insideDoorAnimator;
    public Animator outsideDoorAnimator;

    [Tooltip("Bool parameter used by BOTH animators to open/close doors.")]
    public string doorOpenBool = "IsOpen";

    [Header("Audio")]
    [Tooltip("AudioSource used to LOOP while the elevator is moving.")]
    public AudioSource elevatorRunningLoopSource;

    [Tooltip("Clip to loop while elevator is moving.")]
    public AudioClip elevatorRunningLoopClip;

    [Tooltip("Optional AudioSource for door one-shots. If null, uses loop source.")]
    public AudioSource doorOneShotSource;

    [Tooltip("Optional clip to play when doors open.")]
    public AudioClip doorOpenClip;

    [Tooltip("Optional clip to play when doors close.")]
    public AudioClip doorCloseClip;

    [Header("Timings")]
    public float closeThenMoveDelay = 0.35f;
    public float arriveThenOpenDelay = 0.05f;

    [Header("Interaction (RAW E key)")]
    public float interactRange = 3f;

    [Tooltip("Optional. If left at 'Nothing', raycast will hit ALL layers.")]
    public LayerMask buttonLayerMask;

    [Tooltip("Optional. If null uses Camera.main.")]
    public Camera raycastCamera;

    [Header("Debug")]
    public bool debugLogs = true;

    private Coroutine _moveRoutine;
    private bool _isMoving;
    private bool _doorsOpen;

    private void Awake()
    {
        if (doorOneShotSource == null)
            doorOneShotSource = elevatorRunningLoopSource;

        StopRunningLoop();
        _doorsOpen = false;
        SetDoors(false);
    }

    private void Update()
    {
        // RAW New Input System key press (no action maps)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (debugLogs) Debug.Log("[Elevator] E pressed");
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (_isMoving)
        {
            if (debugLogs) Debug.Log("[Elevator] Ignored: elevator moving");
            return;
        }

        Camera cam = raycastCamera != null ? raycastCamera : Camera.main;
        if (cam == null)
        {
            if (debugLogs) Debug.LogWarning("[Elevator] No camera found (assign Raycast Camera or tag one MainCamera).");
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.yellow, 0.25f);

        // If mask is "Nothing" (0), raycast ALL layers
        int maskToUse = (buttonLayerMask.value == 0) ? Physics.DefaultRaycastLayers : buttonLayerMask.value;

        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange, maskToUse, QueryTriggerInteraction.Ignore))
        {
            if (debugLogs) Debug.Log("[Elevator] Raycast hit nothing (range/collider/layers).");
            return;
        }

        GameObject hitObj = hit.collider.gameObject;
        if (debugLogs) Debug.Log($"[Elevator] Hit collider: {hitObj.name}");

        // OUTSIDE: match if hit object is the same OR a child of the listed switch
        for (int i = 0; i < outsideSwitches.Count; i++)
        {
            var sw = outsideSwitches[i];
            if (sw == null) continue;

            if (hitObj == sw || hitObj.transform.IsChildOf(sw.transform))
            {
                if (debugLogs) Debug.Log($"[Elevator] Outside switch matched floor {i}");
                CallToFloor(i);
                return;
            }
        }

        // INSIDE: same/child match
        if (insideNextSwitch != null && (hitObj == insideNextSwitch || hitObj.transform.IsChildOf(insideNextSwitch.transform)))
        {
            if (debugLogs) Debug.Log("[Elevator] Inside switch matched");
            GoToOtherOrNextFloor();
            return;
        }

        if (debugLogs) Debug.Log("[Elevator] Hit object is not one of the assigned switches.");
    }

    // ===== Outside button pressed =====
    public void CallToFloor(int targetFloor)
    {
        if (_isMoving)
        {
            if (debugLogs) Debug.Log("[Elevator] CallToFloor ignored: already moving");
            return;
        }

        if (!ValidateSetup())
        {
            if (debugLogs) Debug.Log("[Elevator] CallToFloor aborted: ValidateSetup failed");
            return;
        }

        if (targetFloor < 0 || targetFloor >= floorStops.Count)
        {
            if (debugLogs) Debug.LogWarning($"[Elevator] CallToFloor invalid targetFloor {targetFloor}. floorStops.Count={floorStops.Count}");
            return;
        }

        if (debugLogs) Debug.Log($"[Elevator] CallToFloor target={targetFloor} current={currentFloor}");

        if (targetFloor == currentFloor)
        {
            if (debugLogs) Debug.Log("[Elevator] Already at this floor -> opening doors");
            OpenDoors();
            return;
        }

        if (debugLogs) Debug.Log("[Elevator] Starting move sequence...");
        StartMoveSequence(targetFloor);
    }

    // ===== Inside button pressed =====
    public void GoToOtherOrNextFloor()
    {
        if (_isMoving) return;
        if (!ValidateSetup()) return;
        if (floorStops.Count < 2) return;

        int target = (floorStops.Count == 2)
            ? (currentFloor == 0 ? 1 : 0)
            : (currentFloor + 1) % floorStops.Count;

        StartMoveSequence(target);
    }

    private void StartMoveSequence(int targetFloor)
    {
        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);

        _moveRoutine = StartCoroutine(MoveSequence(targetFloor));
    }

    private IEnumerator MoveSequence(int targetFloor)
    {
        _isMoving = true;

        if (_doorsOpen)
        {
            CloseDoors();
            yield return new WaitForSeconds(closeThenMoveDelay);
        }

        PlayRunningLoop();
        yield return MoveCarToStop(targetFloor);
        StopRunningLoop();

        currentFloor = targetFloor;

        yield return new WaitForSeconds(arriveThenOpenDelay);
        OpenDoors();

        _isMoving = false;
        _moveRoutine = null;
    }

    private IEnumerator MoveCarToStop(int targetFloor)
    {
        Transform stop = floorStops[targetFloor];
        if (stop == null) yield break;

        Vector3 targetPos = stop.position;
        Transform carT = elevatorCar.transform;

        while (Vector3.Distance(carT.position, targetPos) > arriveDistance)
        {
            carT.position = Vector3.MoveTowards(carT.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        carT.position = targetPos;
    }

    // ===== Doors =====
    private void OpenDoors()
    {
        _doorsOpen = true;
        SetDoors(true);
        PlayDoorOneShot(doorOpenClip);
    }

    private void CloseDoors()
    {
        _doorsOpen = false;
        SetDoors(false);
        PlayDoorOneShot(doorCloseClip);
    }

    private void SetDoors(bool open)
    {
        if (!string.IsNullOrEmpty(doorOpenBool))
        {
            if (insideDoorAnimator != null) insideDoorAnimator.SetBool(doorOpenBool, open);
            if (outsideDoorAnimator != null) outsideDoorAnimator.SetBool(doorOpenBool, open);
        }
    }

    // ===== Audio =====
    private void PlayRunningLoop()
    {
        if (elevatorRunningLoopSource == null || elevatorRunningLoopClip == null) return;

        elevatorRunningLoopSource.clip = elevatorRunningLoopClip;
        elevatorRunningLoopSource.loop = true;

        if (!elevatorRunningLoopSource.isPlaying)
            elevatorRunningLoopSource.Play();
    }

    private void StopRunningLoop()
    {
        if (elevatorRunningLoopSource != null && elevatorRunningLoopSource.isPlaying)
            elevatorRunningLoopSource.Stop();
    }

    private void PlayDoorOneShot(AudioClip clip)
    {
        if (clip == null) return;

        var src = doorOneShotSource != null ? doorOneShotSource : elevatorRunningLoopSource;
        if (src == null) return;

        src.PlayOneShot(clip);
    }

    private bool ValidateSetup()
    {
        bool ok = true;

        if (elevatorCar == null)
        {
            ok = false;
            if (debugLogs) Debug.LogWarning("[Elevator] Setup error: elevatorCar is NOT assigned.");
        }

        if (floorStops == null || floorStops.Count == 0)
        {
            ok = false;
            if (debugLogs) Debug.LogWarning("[Elevator] Setup error: floorStops is empty / not assigned.");
        }
        else
        {
            for (int i = 0; i < floorStops.Count; i++)
            {
                if (floorStops[i] == null)
                {
                    ok = false;
                    if (debugLogs) Debug.LogWarning($"[Elevator] Setup error: floorStops[{i}] is NULL.");
                }
            }
        }

        if (insideDoorAnimator == null)
        {
            if (debugLogs) Debug.LogWarning("[Elevator] Warning: insideDoorAnimator not assigned (doors may not animate).");
        }

        if (outsideDoorAnimator == null)
        {
            if (debugLogs) Debug.LogWarning("[Elevator] Warning: outsideDoorAnimator not assigned (doors may not animate).");
        }

        if (!string.IsNullOrEmpty(doorOpenBool))
        {
            // We can't truly "validate" parameter existence safely, but we can warn if animators are missing.
            if ((insideDoorAnimator == null && outsideDoorAnimator == null) && debugLogs)
                Debug.LogWarning("[Elevator] Door bool name is set but no door animators are assigned.");
        }

        return ok;
    }
}
