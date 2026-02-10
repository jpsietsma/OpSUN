using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("Scene To Load")]
    [Tooltip("Exact scene name in Build Settings (case-sensitive).")]
    [SerializeField] private string sceneToLoad = "SinglePlayerGame";

    [Header("Tips")]
    [SerializeField] private List<LoadingTipDefinition> tips = new List<LoadingTipDefinition>();

    [Header("UI Refs")]
    [SerializeField] private GameObject loadingRoot;     // optional: whole panel/root
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private Image tipImage;

    [Header("Progress UI")]
    [SerializeField] private Slider progressSlider;      // should be 0..1
    [SerializeField] private TMP_Text progressText;      // "73%"
    [SerializeField] private TMP_Text continueText;      // "Press any key to continue"

    [Header("Progress Animation")]
    [Tooltip("How quickly the bar catches up to real progress. Higher = snappier.")]
    [SerializeField] private float barSmoothing = 10f;

    [Header("Behavior")]
    [Tooltip("Small delay so the loading screen appears before loading begins.")]
    [SerializeField] private float showDelay = 0.05f;

    private float _displayedProgress = 0f;

    private void Start()
    {
        if (loadingRoot != null)
            loadingRoot.SetActive(true);

        if (continueText != null)
            continueText.gameObject.SetActive(false);

        ApplyRandomTip();
        StartCoroutine(LoadSceneRoutine());
    }

    private void ApplyRandomTip()
    {
        LoadingTipDefinition chosen = PickRandomTip();

        if (chosen == null)
        {
            if (titleText != null) titleText.text = "";
            if (tipText != null) tipText.text = "Loading...";
            if (tipImage != null) tipImage.enabled = false;
            return;
        }

        if (titleText != null)
            titleText.text = string.IsNullOrWhiteSpace(chosen.title) ? "" : chosen.title;

        if (tipText != null)
            tipText.text = string.IsNullOrWhiteSpace(chosen.tipText) ? "Loading..." : chosen.tipText;

        if (tipImage != null)
        {
            if (chosen.tipSprite != null)
            {
                tipImage.enabled = true;
                tipImage.sprite = chosen.tipSprite;
                tipImage.preserveAspect = true;
            }
            else
            {
                tipImage.enabled = false;
            }
        }
    }

    private LoadingTipDefinition PickRandomTip()
    {
        if (tips == null || tips.Count == 0)
            return null;

        List<LoadingTipDefinition> enabledTips = null;

        for (int i = 0; i < tips.Count; i++)
        {
            if (tips[i] == null) continue;
            if (!tips[i].enabledForRandom) continue;

            enabledTips ??= new List<LoadingTipDefinition>();
            enabledTips.Add(tips[i]);
        }

        if (enabledTips == null || enabledTips.Count == 0)
            return null;

        return enabledTips[Random.Range(0, enabledTips.Count)];
    }

    private IEnumerator LoadSceneRoutine()
    {
        // Let UI render first
        if (showDelay > 0f)
            yield return new WaitForSeconds(showDelay);
        else
            yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;

        // 1) Load until Unity reaches 90% (0.9)
        while (op.progress < 0.9f)
        {
            float target = Mathf.InverseLerp(0f, 0.9f, op.progress); // 0..1
            AnimateProgressToward(target);
            yield return null;
        }

        // 2) Force target to 100% and animate up to it
        float finalTarget = 1f;
        while (_displayedProgress < 0.999f)
        {
            AnimateProgressToward(finalTarget);
            yield return null;
        }

        // 3) Show prompt and wait for any key/button
        if (continueText != null)
            continueText.gameObject.SetActive(true);

        // Make sure UI shows exactly 100%
        SetProgressUI(1f);

        // Wait for player input (New Input System)
        yield return WaitForAnyInput();

        // 4) Activate the scene
        op.allowSceneActivation = true;
    }

    private void AnimateProgressToward(float target01)
    {
        // Smoothly approach target
        _displayedProgress = Mathf.MoveTowards(
            _displayedProgress,
            target01,
            Time.unscaledDeltaTime * barSmoothing
        );

        SetProgressUI(_displayedProgress);
    }

    private void SetProgressUI(float progress01)
    {
        if (progressSlider != null)
            progressSlider.value = progress01;

        if (progressText != null)
            progressText.text = $"{Mathf.RoundToInt(progress01 * 100f)}%";
    }

    private IEnumerator WaitForAnyInput()
    {
        // Wait a frame so we don't instantly continue from the click that opened the loading screen
        yield return null;

        while (true)
        {
            // Keyboard: any key
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                yield break;

            // Mouse buttons
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame ||
                    Mouse.current.rightButton.wasPressedThisFrame ||
                    Mouse.current.middleButton.wasPressedThisFrame ||
                    Mouse.current.forwardButton.wasPressedThisFrame ||
                    Mouse.current.backButton.wasPressedThisFrame)
                    yield break;
            }

            // Gamepad: any "typical" button
            if (Gamepad.current != null)
            {
                var gp = Gamepad.current;

                if (gp.buttonSouth.wasPressedThisFrame ||
                    gp.buttonNorth.wasPressedThisFrame ||
                    gp.buttonWest.wasPressedThisFrame ||
                    gp.buttonEast.wasPressedThisFrame ||
                    gp.startButton.wasPressedThisFrame ||
                    gp.selectButton.wasPressedThisFrame ||
                    gp.leftShoulder.wasPressedThisFrame ||
                    gp.rightShoulder.wasPressedThisFrame ||
                    gp.leftStickButton.wasPressedThisFrame ||
                    gp.rightStickButton.wasPressedThisFrame ||
                    gp.dpad.up.wasPressedThisFrame ||
                    gp.dpad.down.wasPressedThisFrame ||
                    gp.dpad.left.wasPressedThisFrame ||
                    gp.dpad.right.wasPressedThisFrame)
                    yield break;
            }

            // (Optional) If you want joystick movement / mouse movement to count as "continue",
            // uncomment these:

            /*
            if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.5f)
                yield break;

            if (Gamepad.current != null)
            {
                if (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.25f ||
                    Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.25f)
                    yield break;
            }
            */

            yield return null;
        }
    }
}
