using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingRequirementRowUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    public void Set(ItemDefinition item, int amount)
    {
        if (item == null || amount <= 0)
        {
            // Keep row but hide it / clear it
            if (icon != null) icon.sprite = null;
            if (label != null) label.text = "";
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (icon != null)
            icon.sprite = item.icon;

        if (label != null)
            label.text = $"{item.displayName} ({amount})";
    }

    public void Clear()
    {
        if (icon != null) icon.sprite = null;
        if (label != null) label.text = "";
        gameObject.SetActive(false);
    }
}

