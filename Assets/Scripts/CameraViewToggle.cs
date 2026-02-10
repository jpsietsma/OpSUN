using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CameraViewToggle : MonoBehaviour
{
    public CinemachineVirtualCamera thirdPersonCam;
    public CinemachineVirtualCamera firstPersonCam;

    [Header("Optional")]
    public GameObject[] hideInFirstPerson;
    public float firstPersonFOV = 65f;
    public float thirdPersonFOV = 60f;

    bool _isFirstPerson;

    void Start() => SetView(false);

    void Update()
    {
        // NEW Input System key press
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            SetView(!_isFirstPerson);
        }
    }

    void SetView(bool firstPerson)
    {
        _isFirstPerson = firstPerson;

        thirdPersonCam.Priority = firstPerson ? 0 : 10;
        firstPersonCam.Priority = firstPerson ? 10 : 0;

        if (firstPersonCam) firstPersonCam.m_Lens.FieldOfView = firstPersonFOV;
        if (thirdPersonCam) thirdPersonCam.m_Lens.FieldOfView = thirdPersonFOV;

        if (hideInFirstPerson != null)
            foreach (var go in hideInFirstPerson)
                if (go) go.SetActive(!firstPerson);
    }
}