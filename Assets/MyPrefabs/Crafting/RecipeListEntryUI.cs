using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeListEntryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button button;

    private ItemRecipe boundRecipe;
    private System.Action<ItemRecipe> onClicked;

    public void Bind(ItemRecipe recipe, System.Action<ItemRecipe> clickedCallback)
    {
        boundRecipe = recipe;
        onClicked = clickedCallback;

        if (titleText != null)
            titleText.text = recipe != null ? recipe.itemDefinition.displayName : "(Missing Recipe)";
         
        if (iconImage != null)
            iconImage.sprite = recipe != null ? recipe.itemDefinition.icon : null;

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (boundRecipe != null)
                onClicked?.Invoke(boundRecipe);
        });
    }
}
