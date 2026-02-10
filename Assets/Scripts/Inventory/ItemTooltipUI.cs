using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemTooltipUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform root;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Follow Mouse")]
    [SerializeField] private Vector2 offset = new Vector2(16, -16);
    [SerializeField] private Vector2 paddingFromScreenEdge = new Vector2(12, 12);

    [Header("Timing")]
    [SerializeField] private float showDelay = 0.25f;
    [SerializeField] private float fadeInTime = 0.12f;
    [SerializeField] private float fadeOutTime = 0.10f;

    private Coroutine showRoutine;
    private Coroutine fadeRoutine;
    private ItemDefinition pendingItem;
    private Canvas canvas;
    private bool isVisible;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        HideImmediate();
    }

    private void Update()
    {
        if (!isVisible) return;
        FollowMouse();
    }

    //public void Show(ItemDefinition item)
    //{
    //    if (item == null) { Hide(); return; }

    //    // Only show tooltip for consumables (per your request)
    //    if (item.itemType != ItemType.Consumable)
    //    {
    //        Hide();
    //        return;
    //    }

    //    titleText.text = item.displayName;
    //    bodyText.text = BuildConsumableText(item);
    //    descriptionText.text = item.description;

    //    root.gameObject.SetActive(true);
    //    canvasGroup.alpha = 1f;
    //    isVisible = true;

    //    FollowMouse();
    //}

    //public void Hide()
    //{
    //    isVisible = false;
    //    root.gameObject.SetActive(false);
    //}

    public void Show(ItemDefinition item)
    {
        // Only show for valid consumables (per your current behavior)
        if (item == null || item.itemType != ItemType.Consumable)
        {
            Hide();
            return;
        }

        pendingItem = item;

        // restart delay timer
        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(ShowAfterDelay());
    }

    public void Hide()
    {
        pendingItem = null;

        if (showRoutine != null) { StopCoroutine(showRoutine); showRoutine = null; }

        // fade out if currently visible
        if (root != null && root.gameObject.activeSelf)
            StartFade(0f, fadeOutTime, deactivateAtEnd: true);
    }

    private void HideImmediate()
    {
        isVisible = false;
        if (root != null) root.gameObject.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    private System.Collections.IEnumerator ShowAfterDelay()
    {
        float t = 0f;
        while (t < showDelay)
        {
            // if hover canceled during delay
            if (pendingItem == null) yield break;
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // still hovering? show it
        if (pendingItem == null) yield break;

        titleText.text = pendingItem.displayName;
        bodyText.text = BuildConsumableText(pendingItem);
        descriptionText.text = pendingItem.description;

        root.gameObject.SetActive(true);
        isVisible = true;

        // start from 0 alpha and fade in
        canvasGroup.alpha = 0f;
        StartFade(1f, fadeInTime, deactivateAtEnd: false);

        FollowMouse();
    }

    private void StartFade(float targetAlpha, float duration, bool deactivateAtEnd)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(targetAlpha, duration, deactivateAtEnd));
    }

    private System.Collections.IEnumerator FadeTo(float targetAlpha, float duration, bool deactivateAtEnd)
    {
        if (canvasGroup == null) yield break;

        float start = canvasGroup.alpha;
        float t = 0f;

        // Handle instant
        if (duration <= 0.0001f)
        {
            canvasGroup.alpha = targetAlpha;
            if (deactivateAtEnd && Mathf.Approximately(targetAlpha, 0f))
            {
                isVisible = false;
                root.gameObject.SetActive(false);
            }
            yield break;
        }

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, a);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (deactivateAtEnd && Mathf.Approximately(targetAlpha, 0f))
        {
            isVisible = false;
            root.gameObject.SetActive(false);
        }
    }


    private void FollowMouse()
    {
        if (root == null) return;
        if (Mouse.current == null) return; // no mouse device (rare)

        Vector2 mouse = Mouse.current.position.ReadValue();
        Vector2 pos = mouse + offset;

        // Clamp tooltip within screen bounds
        Vector2 size = root.sizeDelta;
        float maxX = Screen.width - paddingFromScreenEdge.x - size.x;
        float minX = paddingFromScreenEdge.x;
        float maxY = Screen.height - paddingFromScreenEdge.y;
        float minY = paddingFromScreenEdge.y + size.y;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        root.position = pos;
    }

    private string BuildConsumableText(ItemDefinition item)
    {
        // Rich text colors
        const string GREEN = "#44FF66";
        const string RED = "#FF4B4B";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        AppendStat(sb, "Health", item.healthBuff, item.healthDebuff, GREEN, RED);
        AppendStat(sb, "Hunger", item.hungerBuff, item.hungerDebuff, GREEN, RED);
        AppendStat(sb, "Thirst", item.thirstBuff, item.thirstDebuff, GREEN, RED);
        AppendStat(sb, "Stamina", item.staminaBuff, item.staminaDebuff, GREEN, RED);

        // If nothing to show, show description (optional)
        if (sb.Length == 0 && !string.IsNullOrWhiteSpace(item.description))
            sb.Append(item.description);

        return sb.ToString().TrimEnd();
    }

    private void AppendStat(System.Text.StringBuilder sb, string label, float buff, float debuff, string green, string red)
    {
        // Show buff only if > 0
        if (buff > 0)
            sb.AppendLine($"{label}: <color={green}>+{buff}</color>");

        // Show debuff only if > 0 (display as -value)
        if (debuff > 0)
            sb.AppendLine($"{label}: <color={red}>-{debuff}</color>");
    }
}
