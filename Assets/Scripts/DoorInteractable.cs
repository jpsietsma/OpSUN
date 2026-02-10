using UnityEngine;

public class DoorToggle : MonoBehaviour, IPickupable
{
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;

    // Debounce in case Pickup() gets called twice in a frame
    private int lastToggleFrame = -999;

    private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public string GetPromptText()
    {
        if (animator == null) return "";

        bool open = animator.GetBool(IsOpenHash);
        return open ? "Press [E] to close door" : "Press [E] to open door";
    }

    public void Pickup()
    {
        if (animator == null) return;

        // If called twice same frame, ignore extra call
        if (Time.frameCount == lastToggleFrame) return;
        lastToggleFrame = Time.frameCount;

        // Optional: block toggles while transitioning
        if (animator.IsInTransition(0)) return;

        bool open = animator.GetBool(IsOpenHash);

        if (!open)
        {
            audioSource.PlayOneShot(openClip);
        }
        else
        {
            audioSource.PlayOneShot(closeClip);
        }

        animator.SetBool(IsOpenHash, !open);
    }
}