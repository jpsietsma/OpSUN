using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast")]
    public Camera playerCamera;
    public float interactDistance = 3f;
    public LayerMask interactLayerMask = ~0; // Everything by default

    [Header("UI")]
    public TMP_Text pickupPromptText;

    [Header("Pause Menu")]
    public GameObject pauseMenu;

    [Header("Hold UI")]
    public GameObject holdRoot;     // parent object for the hold UI (or just use the Image GO)
    public Image holdFillImage;     // the Image set to Filled Radial360

    private bool interactLatch = false;     // true = we've already fired for the current key press
    private float nextInteractTime = 0f;    // optional safety cooldown
    public float interactCooldown = 0.15f;  // 150ms (tweak 0.1–0.25)
    private IHoldInteractable currentHold;
    private float holdTimer;
    private bool holdCompleted = false;

    private IPickupable current;

    private void Start()
    {
        if (pickupPromptText != null)
            pickupPromptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        //Press escape key for pause menu
        if (pauseMenu != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePauseMenu();

            HidePrompt();
            HideHoldUI();

            return;
        }

        UpdateLookTarget();
        HandleInteractInput();
    }

    private void UpdateLookTarget()
    {
        current = null;
        currentHold = null;

        if (playerCamera == null)
        {
            HidePrompt();
            HideHoldUI();
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask))
        {
            // Prefer hold interactables
            currentHold = hit.collider.GetComponentInParent<IHoldInteractable>();
            if (currentHold == null)
                current = hit.collider.GetComponentInParent<IPickupable>();
        }

        if (currentHold != null)
        {
            ShowPrompt(currentHold.GetHoldPromptText());
        }
        else if (current != null)
        {
            ShowPrompt(current.GetPromptText());
        }
        else
        {
            HidePrompt();
            HideHoldUI();
            holdTimer = 0f;
        }
    }

    private void HandleInteractInput()
    {
        if (Keyboard.current == null) return;

        // HOLD interaction (WorkbenchBasic etc.)
        if (currentHold != null)
        {

            //Start showing UI when E is held
            if (Keyboard.current.eKey.isPressed)
            {
                if (holdCompleted == true)
                    return;


                ShowHoldUI();

                float dur = Mathf.Max(0.01f, currentHold.HoldDuration);
                holdTimer += Time.deltaTime;

                if (holdFillImage != null)
                    holdFillImage.fillAmount = Mathf.Clamp01(holdTimer / dur);

                if (holdTimer >= dur)
                {
                    // Complete
                    currentHold.OnHoldComplete();

                    // Reset hold UI/timer so it doesn't instantly re-trigger
                    holdTimer = 0f;
                    HideHoldUI();
                    holdCompleted = true;
                }
            }

            // Reset if released early
            if (Keyboard.current.eKey.wasReleasedThisFrame)
            {
                holdTimer = 0f;
                HideHoldUI();
                holdCompleted = false;
            }

            return; // don’t also treat as tap pickup
        }

        // TAP interaction (your existing pickup behavior)
        if (current == null) return;

        //E key is pressed on pickup
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            current.Pickup();
        }
    }

    private void ShowHoldUI()
    {
        if (holdRoot != null && !holdRoot.activeSelf)
            holdRoot.SetActive(true);
        if (holdFillImage != null && !holdFillImage.gameObject.activeSelf)
            holdFillImage.gameObject.SetActive(true);
    }

    private void TogglePauseMenu()
    {
        if (pauseMenu == null) return;

        pauseMenu.SetActive(!pauseMenu.activeSelf);
    }

    private void HideHoldUI()
    {
        if (holdFillImage != null)
            holdFillImage.fillAmount = 0f;

        if (holdRoot != null && holdRoot.activeSelf)
            holdRoot.SetActive(false);
        else if (holdFillImage != null && holdFillImage.gameObject.activeSelf)
            holdFillImage.gameObject.SetActive(false);
    }

    private void ShowPrompt(string text)
    {
        if (pickupPromptText == null) return;

        pickupPromptText.text = text;
        if (!pickupPromptText.gameObject.activeSelf)
            pickupPromptText.gameObject.SetActive(true);
    }

    private void HidePrompt()
    {
        if (pickupPromptText == null) return;

        if (pickupPromptText.gameObject.activeSelf)
            pickupPromptText.gameObject.SetActive(false);
    }
}

public interface IPickupable
{
    string GetPromptText();
    void Pickup();
}
