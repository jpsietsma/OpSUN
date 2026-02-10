using UnityEngine;

public class InventoryTestGiver : MonoBehaviour
{
    public InventorySystem inventory;
    public ItemDefinition backpack;
    public ItemDefinition helmet;

    private void Start()
    {
        inventory.TryAdd(backpack, 1);
        inventory.TryAdd(helmet, 1);
    }
}
