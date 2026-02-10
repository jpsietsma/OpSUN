using System;
using System.Collections.Generic;
using UnityEngine;

public enum ToolType { Extraction, Repair, Special }

[CreateAssetMenu(menuName = "Game/Tools/Tool Definition")]
public class ToolDefinition : ScriptableObject
{
    public ToolType toolType;
    public bool hasExtraction;
    public bool requiresFuel;
    public int numberOfUses;

    [Tooltip("List of items that can be harvested")]
    public List<ItemDefinition> extractionItems;

    [Tooltip("Fuel item required for tool usage")]
    public ItemDefinition fuelItem;

    [Header("Tool Sounds")]
    public AudioClip useSound;
    public AudioClip breakSound;
    
}
