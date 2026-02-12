using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Farming/Seed To Crop Database")]
public class SeedToCropDatabase : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public ItemDefinition seedItem;
        public CropDefinition crop;
    }

    public Entry[] entries;

    public bool TryGetCrop(ItemDefinition seed, out CropDefinition crop)
    {
        if (seed == null)
        {
            crop = null;
            return false;
        }

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].seedItem == seed)
            {
                crop = entries[i].crop;
                return crop != null;
            }
        }

        crop = null;
        return false;
    }
}