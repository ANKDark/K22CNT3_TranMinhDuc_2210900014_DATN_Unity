using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    ArmArmor,
    BeltArmor,
    ChestArmor,
    FeetArmor,
    HeadArmor,
    LegsArmor,
    Sword,

    Consumable,
    QuestItem,
    Miscellaneous
}

public enum Attributes
{
    Strength,
    Agility,
    Intellect,
    Stamina,
    Health,
    Mana
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory System/Items/Item")]
public class ItemObject : ScriptableObject
{
    public Sprite uiDisplay;
    public GameObject characterDisplay;
    public bool stackable;
    public GameObject worldModel;

    public List<string> boneNames = new List<string>();
    public ItemType type;

    [TextArea(15, 20)]
    public string description;

    public Item data = new Item();

    public Item CreateItem()
    {
        return new Item(this);
    }
}

public static class ItemExtensions
{
    public static string BuffsToString(this Item item)
    {
        if (item.buffs == null || item.buffs.Length == 0)
            return "Không có chỉ số";

        List<string> buffStrings = new List<string>();
        foreach (var buff in item.buffs)
        {
            string sign = buff.value >= 0 ? "+ " : "";
            buffStrings.Add($"{sign}{buff.value} {buff.attribute}");
        }
        return string.Join("\n", buffStrings);

    }
}

[System.Serializable]
public class Item
{
    public string name;
    public int Id = -1;
    public ItemBuff[] buffs;
    public string description;
    [System.NonSerialized] public GameObject worldModel;

    public Item()
    {
        Id = -1;
        name = "";
        worldModel = null;
        buffs = new ItemBuff[0];
    }

    public Item(ItemObject item)
    {
        name = item.name;
        description = item.description;
        Id = item.data.Id != -1 ? item.data.Id : UnityEngine.Random.Range(1000, 9999);
        worldModel = item.worldModel;

        buffs = new ItemBuff[item.data.buffs.Length];
        for (int i = 0; i < buffs.Length; i++)
        {
            buffs[i] = new ItemBuff(item.data.buffs[i].min, item.data.buffs[i].max)
            {
                attribute = item.data.buffs[i].attribute
            };
        }
    }
}

[System.Serializable]
public class ItemBuff : IModifier
{
    public Attributes attribute;
    public int value;
    public int min;
    public int max;

    public ItemBuff(int _min, int _max)
    {
        min = _min;
        max = _max;
        GenerateValue();
    }

    public void AddValue(ref int baseValue)
    {
        baseValue += value;
    }

    public void GenerateValue()
    {
        value = UnityEngine.Random.Range(min, max);
    }
}