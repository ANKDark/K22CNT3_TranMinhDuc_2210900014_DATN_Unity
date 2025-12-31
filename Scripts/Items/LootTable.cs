using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct LootAttributeRange
{
    public Attributes attribute;
    public int min;
    public int max;
}

[System.Serializable]
public class LootItem
{
    public ItemObject item;
    [Range(0, 100)] public float dropChance;
    public List<LootAttributeRange> overrideBuffs;
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
                
                Item newItem = new Item(loot.item);

                if (loot.overrideBuffs != null && loot.overrideBuffs.Count > 0)
                {
                    foreach (var overrideBuff in loot.overrideBuffs)
                    {
                        foreach (var itemBuff in newItem.buffs)
                        {
                            if (itemBuff.attribute == overrideBuff.attribute)
                            {
                                itemBuff.min = overrideBuff.min;
                                itemBuff.max = overrideBuff.max;
                                itemBuff.GenerateValue(); 
                            }
                        }
                    }
                }

                return newItem;
            }
        }
        return null;
    }
}
