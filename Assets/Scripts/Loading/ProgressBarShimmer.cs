using UnityEngine;

public class ProgressBarShimmer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform shimmerRect;   // the highlight image rect
    [SerializeField] private RectTransform fillRect;      // the fill area rect (where shimmer moves)

    [Header("Motion")]
    [SerializeField] private float speed = 250f;          // pixels per second
    [SerializeField] private float padding = 50f;         // extra travel beyond edges

    private float _x;

    private void Reset()
    {
        shimmerRect = transform as RectTransform;
    }

    private void Update()
    {
        if (shimmerRect == null || fillRect == null) return;

        float width = fillRect.rect.width;
        if (width <= 0f) return;

        _x += speed * Time.unscaledDeltaTime;

        // Loop from left to right across the fill rect
        float start = -padding;
        float end = width + padding;
        float range = end - start;

        float posX = start + (_x % range);

        // Place shimmer within the fill (local space)
        var p = shimmerRect.anchoredPosition;
        p.x = posX;
        shimmerRect.anchoredPosition = p;
    }
}
 