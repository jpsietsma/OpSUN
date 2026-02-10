using UnityEngine;

public class CursorLockManager : MonoBehaviour
{
    [Header("State")]
    [Tooltip("Set true while inventory/menu is open.")]
    public bool uiOpen;

    [Header("Options")]
    public bool lockOnStart = true;
    public KeyCode toggleCursorKey = KeyCode.Escape;

    void Start()
    {
        if (lockOnStart)
            SetLocked(true);
    }

    void Update()
    {
        // Optional: escape toggles cursor (handy in builds)
        if (Input.GetKeyDown(toggleCursorKey))
        {
            uiOpen = !uiOpen;
            SetLocked(!uiOpen);
        }

        // Enforce state
        if (uiOpen && Cursor.lockState != CursorLockMode.None)
            SetLocked(false);
        else if (!uiOpen && Cursor.lockState != CursorLockMode.Locked)
            SetLocked(true);
    }

    public void SetUIOpen(bool open)
    {
        uiOpen = open;
        SetLocked(!uiOpen);
    }

    private void SetLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
        // If you use Time.timeScale = 0 for menus, that is fine; look uses delta not time.
    }
}
