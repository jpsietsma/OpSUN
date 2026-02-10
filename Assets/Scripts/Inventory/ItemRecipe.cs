using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Items/Crafting Recipe")]
public class ItemRecipe : ScriptableObject
{
    [Header("Crafting Details")]
    [Tooltip("ItemDefinition of item to craft")]
    public ItemDefinition itemDefinition;

    [Tooltip("Base time in seconds to craft (Required)")]
    public int craftingTime = 1;

    [Tooltip("The number of items produced when crafted (Required)")]
    public int itemsProduced = 1;

    [Header("Player Details")]
    [Tooltip("Minimum player crafting level (Required)")]
    public int minCraftingLevel = 1;

    [Header("Required Crafting Resources")]  
    [Tooltip("Resource 1 (Required)")]
    public ItemDefinition resource1;
    [Tooltip("Amount (Required)")]
    public int amount1 = 1;

    [Tooltip("Resource 2")]
    public ItemDefinition resource2;
    [Tooltip("Amount")]
    public int amount2;

    [Tooltip("Resource 3")]
    public ItemDefinition resource3;
    [Tooltip("Amount")]
    public int amount3;

    [Tooltip("Resource 4")]
    public ItemDefinition resource4;
    [Tooltip("Amount")]
    public int amount4;

    [Tooltip("Resource 5")]
    public ItemDefinition resource5;
    [Tooltip("Amount")]
    public int amount5;

}