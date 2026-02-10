using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [System.Serializable]
    public class DoorGet
    {
        public GameObject Door;
        public int CloseValue;
        public int OpenValue;
        public bool isDoorOpen;
        public GameObject RotationOrigin;

        [Header("Optional Animation (if assigned, overrides rotation)")]
        public Animator animator;
        [Tooltip("Animator Trigger name for opening (leave blank to not use)")]
        public string openTrigger = "Open";
        [Tooltip("Animator Trigger name for closing (leave blank to not use)")]
        public string closeTrigger = "Close";
    }

    public List<DoorGet> UseDoors = new List<DoorGet>();

    public bool door_in_use;

    [Header("Audio (Optional)")]
    [Tooltip("AudioSource used to play the open sound (assign an AudioClip on the AudioSource).")]
    public AudioSource doorOpenAudioSource;

    [Tooltip("AudioSource used to play the close sound (assign an AudioClip on the AudioSource).")]
    public AudioSource doorCloseAudioSource;

    public Coroutine DoorStartUsing;

    public void MoveMyDoor()
    {
        foreach (var door in UseDoors)
        {
            if (door.Door == gameObject)
            {
                if (door.isDoorOpen == false && !door_in_use)
                {
                    door_in_use = true;
                    door.isDoorOpen = true;

                    // Play audio (optional)
                    if (doorOpenAudioSource != null)
                        doorOpenAudioSource.Play();

                    // 1) Prefer animation if defined
                    if (DoorHasUsableAnimator(door, opening: true))
                    {
                        FireDoorAnimation(door, opening: true);
                        DoorStartUsing = StartCoroutine(ReleaseDoorUseAfterAnim(door));
                    }
                    // 2) Fallback to current rotation coroutine logic
                    else
                    {
                        DoorStartUsing = StartCoroutine(OpenDoor(door.OpenValue, door.Door, door.RotationOrigin));
                    }
                }

                if (door.isDoorOpen == true && !door_in_use)
                {
                    door_in_use = true;
                    door.isDoorOpen = false;

                    // Play audio (optional)
                    if (doorCloseAudioSource != null)
                        doorCloseAudioSource.Play();

                    // 1) Prefer animation if defined
                    if (DoorHasUsableAnimator(door, opening: false))
                    {
                        FireDoorAnimation(door, opening: false);
                        DoorStartUsing = StartCoroutine(ReleaseDoorUseAfterAnim(door));
                    }
                    // 2) Fallback to current rotation coroutine logic
                    else
                    {
                        DoorStartUsing = StartCoroutine(CloseDoor(door.CloseValue, door.Door, door.OpenValue, door.RotationOrigin));
                    }
                }
            }
        }
    }

    public void ActionDoor()
    {
        foreach (var door in UseDoors)
        {
            door.Door.GetComponent<Door>().MoveMyDoor();
        }
    }

    private bool DoorHasUsableAnimator(DoorGet door, bool opening)
    {
        if (door == null) return false;
        if (door.animator == null) return false;

        string trig = opening ? door.openTrigger : door.closeTrigger;
        if (string.IsNullOrWhiteSpace(trig)) return false;

        return true;
    }

    private void FireDoorAnimation(DoorGet door, bool opening)
    {
        // Safety: keep it dead simple: set the trigger.
        // (If you want to clear both triggers first, you can, but leaving it alone avoids side-effects.)
        string trig = opening ? door.openTrigger : door.closeTrigger;
        door.animator.SetTrigger(trig);
    }

    private IEnumerator ReleaseDoorUseAfterAnim(DoorGet door)
    {
        // Simple "animation mode" lockout:
        // Wait roughly one frame, then allow again once the animator has time to transition.
        // (If you want exact timing, expose a float per-door and wait that long.)
        yield return null;
        yield return new WaitForSeconds(0.25f);

        door_in_use = false;

        // Keep behavior consistent with your existing coroutines:
        if (DoorStartUsing != null)
            StopCoroutine(DoorStartUsing);
    }

    public IEnumerator OpenDoor(int Angle, GameObject currentDoor, GameObject RotationOri)
    {
    repeatLoop:
        yield return new WaitForSeconds(0.01f);

        if (Angle > 0)
        {
            RotationOri.transform.Rotate(new Vector3(0, 0, 95 * Time.deltaTime));

            if (Angle < RotationOri.transform.localEulerAngles.z)
            {
                door_in_use = false;
                StopCoroutine(DoorStartUsing);
            }
            if (Angle != RotationOri.transform.localEulerAngles.y)
            {
                goto repeatLoop;
            }
        }
        if (Angle < 0)
        {
            RotationOri.transform.Rotate(new Vector3(0, 0, -95 * Time.deltaTime));

            if ((360 + Angle) > RotationOri.transform.localEulerAngles.z)
            {
                door_in_use = false;
                StopCoroutine(DoorStartUsing);
            }
            if (Angle != RotationOri.transform.localEulerAngles.y)
            {
                goto repeatLoop;
            }
        }
    }

    public IEnumerator CloseDoor(int Angle, GameObject currentDoor, int OpenValue, GameObject RotationOri)
    {
    repeatLoop:
        yield return new WaitForSeconds(0.008f);

        if (OpenValue == 88)
        {
            RotationOri.transform.Rotate(new Vector3(0, 0, -95 * Time.deltaTime));

            if ((Angle + 2) > RotationOri.transform.localEulerAngles.z)
            {
                door_in_use = false;
                RotationOri.transform.localEulerAngles = new Vector3(RotationOri.transform.localEulerAngles.x, RotationOri.transform.localEulerAngles.y, Angle);
                StopCoroutine(DoorStartUsing);
            }
            if (Angle != RotationOri.transform.localEulerAngles.z)
            {
                goto repeatLoop;
            }
        }
        if (OpenValue == -88)
        {
            RotationOri.transform.Rotate(new Vector3(0, 0, 95 * Time.deltaTime));

            if (RotationOri.transform.localEulerAngles.z > 358)
            {
                door_in_use = false;
                RotationOri.transform.localEulerAngles = new Vector3(RotationOri.transform.localEulerAngles.x, RotationOri.transform.localEulerAngles.y, Angle);
                StopCoroutine(DoorStartUsing);
            }
            if (Angle != RotationOri.transform.localEulerAngles.z)
            {
                goto repeatLoop;
            }
        }

        if (Angle != RotationOri.transform.localEulerAngles.z)
        {
            goto repeatLoop;
        }
    }
}
