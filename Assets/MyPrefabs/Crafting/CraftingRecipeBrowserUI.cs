using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingRecipeBrowserUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private List<ItemRecipe> recipes = new List<ItemRecipe>();

    [Header("List")]
    [SerializeField] private Transform listContent;                  // ScrollView/Viewport/Content
    [SerializeField] private RecipeListEntryUI entryPrefab;

    [Header("Details")]
    [SerializeField] private Image selectedIcon;
    [SerializeField] private TMP_Text selectedTitle;
    [SerializeField] private TMP_Text selectedDescription;

    [Header("Requirements UI")]
    [SerializeField] private Transform requirementsGrid;      // RequirementsGrid transform
    [SerializeField] private CraftingRequirementRowUI requirementPrefab;

    private CraftingRequirementRowUI[] requirementRows;

    private readonly List<RecipeListEntryUI> spawnedEntries = new();

    private void Start()
    {
        RebuildList();

        if (recipes != null && recipes.Count > 0)
            ShowDetails(recipes[0]);
        else
            ClearDetails();
    }

    private void Awake()
    {
        BuildRequirementRows();
    }

    private void BuildRequirementRows()
    {
        requirementRows = new CraftingRequirementRowUI[5];

        for (int i = 0; i < 5; i++)
        {
            var row = Instantiate(requirementPrefab, requirementsGrid);
            row.Clear();
            requirementRows[i] = row;
        }
    }

    public void RebuildList()
    {
        // Clear old
        for (int i = 0; i < spawnedEntries.Count; i++)
        {
            if (spawnedEntries[i] != null)
                Destroy(spawnedEntries[i].gameObject);
        }
        spawnedEntries.Clear();

        if (listContent == null || entryPrefab == null || recipes == null)
            return;

        // Spawn new
        foreach (var r in recipes)
        {
            var entry = Instantiate(entryPrefab, listContent);
            entry.Bind(r, OnRecipeClicked);

            spawnedEntries.Add(entry);
        }
    }

    private void OnRecipeClicked(ItemRecipe recipe)
    {
        ShowDetails(recipe);
    }

    private void ShowDetails(ItemRecipe recipe)
    {
        if (recipe == null)
        {
            ClearDetails();
            return;
        }

        if (selectedIcon != null)
            selectedIcon.sprite = recipe.itemDefinition.icon;

        if (selectedTitle != null)
            selectedTitle.text = recipe.itemDefinition.displayName;

        if (selectedDescription != null)
            selectedDescription.text = recipe.itemDefinition.description;

        // Map 5 fields for requirements
        requirementRows[0].Set(recipe.resource1, recipe.amount1);
        requirementRows[1].Set(recipe.resource2, recipe.amount2);
        requirementRows[2].Set(recipe.resource3, recipe.amount3);
        requirementRows[3].Set(recipe.resource4, recipe.amount4);
        requirementRows[4].Set(recipe.resource5, recipe.amount5);
    }

    private void ClearDetails()
    {
        if (selectedIcon != null) selectedIcon.sprite = null;
        if (selectedTitle != null) selectedTitle.text = "";
        if (selectedDescription != null) selectedDescription.text = "";
    }
}
