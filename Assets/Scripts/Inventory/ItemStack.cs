[System.Serializable]
public class ItemStack
{
    public ItemDefinition item;
    public int count;

    public bool IsEmpty => item == null || count <= 0;

    public ItemStack() { item = null; count = 0; }
    public ItemStack(ItemDefinition item, int count) { this.item = item; this.count = count; }
}