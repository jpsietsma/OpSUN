using UnityEngine;

public class DoubleDoorToggle : MonoBehaviour, IPickupable
{
    [Header("Assign BOTH door animators here")]
    [SerializeField] private Animator leftDoorAnimator;
    [SerializeField] private Animator rightDoorAnimator;

    [Header("Assign door open/close sounds here")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;

    // Optional: if one door is reversed, you can flip its open direction in the animation itself
    private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

    public void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public string GetPromptText()
    {
        // Use left door as the "truth" for prompt (either works)
        if (leftDoorAnimator == null) return "";
        bool open = leftDoorAnimator.GetBool(IsOpenHash);
        return open ? "Press [E] to close door" : "Press [E] to open door";
    }

    public void Pickup()
    {
        if (leftDoorAnimator == null || rightDoorAnimator == null) return;

        // If either is mid-transition, ignore input so they stay in sync
        if (leftDoorAnimator.IsInTransition(0) || rightDoorAnimator.IsInTransition(0)) return;

        bool open = leftDoorAnimator.GetBool(IsOpenHash);
        bool next = !open;

        leftDoorAnimator.SetBool(IsOpenHash, next);
        rightDoorAnimator.SetBool(IsOpenHash, next);

        // PLAY SOUND ONCE
        if (audioSource != null)
        {
            audioSource.PlayOneShot(next ? openClip : closeClip);
        }
    }
}