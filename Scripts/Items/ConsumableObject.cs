using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory System/Items/Consumable")]
public class ConsumableObject : ItemObject
{
    [Header("Consumable Data")]
    public int restoreHealth;
    public float healDuration = 0f;

    public int restoreMana;
    public float manaRegenMultiplier = 1f;
    public float manaBuffDuration = 0f;
    
    [Header("Permanent Stats (Tăng chỉ số vĩnh viễn)")]
    public List<ItemBuff> permanentBuffs;

    public void Awake()
    {
        type = ItemType.Consumable;
    }
}
