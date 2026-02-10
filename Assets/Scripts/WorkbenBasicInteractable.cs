using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorkbenchBasicInteractable : MonoBehaviour, IHoldInteractable
{
    [Header("Workbench")]
    public ItemDefinition itemDefinition;     // assign your WorkbenchBasic item definition
    public GameObject craftingOverlayPanel;        // drag your CraftingOverlay panel here

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string gameplayMapName = "Player";
    [SerializeField] private string uiMapName = "UI";

    [Header("Crafting Recipes Available")]
    [SerializeField] private List<ItemRecipe> availableRecipes;
        
    [Header("Hold Settings")]
    [SerializeField] private float holdDuration = 1f;
    public float HoldDuration => holdDuration;

    public string GetHoldPromptText()
    {
        string n = itemDefinition != null ? itemDefinition.displayName : "Workbench";
        return $"Hold [E] to use {n}";
    }

    public void OnHoldComplete()
    {
        if (craftingOverlayPanel != null)
            craftingOverlayPanel.SetActive(true);

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap(uiMapName);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (!craftingOverlayPanel.activeSelf) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseCrafting();
        }
    }

    private void CloseCrafting()
    {
        craftingOverlayPanel.SetActive(false);

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap(gameplayMapName);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}