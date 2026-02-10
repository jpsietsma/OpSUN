using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGiveStarterItems : MonoBehaviour
{
    [Header("Inventory System")]
    public InventorySystem inventory;

    public List<ItemDefinition> starterItems;

    private void Start()
    {
        foreach (ItemDefinition item in starterItems)
        {
            inventory.TryAdd(item, 1);
        }
    }
}
