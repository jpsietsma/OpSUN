using UnityEngine;

public class DroppedItemVisual : MonoBehaviour
{
    [Header("Where the visual model should be placed")]
    public Transform visualRoot;

    public GameObject CurrentVisualInstance => currentVisualInstance;

    private GameObject currentVisualInstance;

    private void Awake()
    {
        if (visualRoot == null)
            visualRoot = transform;
    }

    public void Apply(ItemDefinition item)
    {
        if (item == null) return;

        // Clear old visual
        if (currentVisualInstance != null)
            Destroy(currentVisualInstance);

        // Spawn item-specific model (if provided)
        if (item.worldPrefab != null)
        {
            currentVisualInstance = Instantiate(item.worldPrefab, visualRoot);
            currentVisualInstance.transform.localPosition = Vector3.zero;
            currentVisualInstance.transform.localRotation = Quaternion.identity;
            currentVisualInstance.transform.localScale = Vector3.one;

            // Apply tint to all renderers that support color
            var renderers = currentVisualInstance.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    if (mat != null && mat.HasProperty("_Color"))
                        mat.color = item.worldTint;
                }
            }
        }
    }
}