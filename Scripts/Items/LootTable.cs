using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LootItem
{
    public ItemObject item;
    [Range(0, 100)] public float dropChance;
}

[CreateAssetMenu(fileName = "New Loot Table", menuName = "Inventory System/Items/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<LootItem> lootItems;

    public Item GetDroppedItem(out ItemObject sourceItem)
    {
        sourceItem = null;
        float roll = Random.Range(0f, 100f);
        float cumulativeProbability = 0f;

        foreach (var loot in lootItems)
        {
            cumulativeProbability += loot.dropChance;
            
            if (roll <= cumulativeProbability)
            {
                sourceItem = loot.item;
                return new Item(loot.item);
            }
        }
        return null;
    }
}
