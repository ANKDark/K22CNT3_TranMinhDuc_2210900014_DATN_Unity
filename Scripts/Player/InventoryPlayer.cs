using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPlayer : MonoBehaviour
{
    public InventoryObject inventory;
    public InventoryObject equipment;
    public Attribute[] attributes;

    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();

        if (inventory != null)
        {
            foreach (var slot in inventory.GetSlots)
                slot.inventory = inventory;
        }

        if (equipment != null)
        {
            foreach (var slot in equipment.GetSlots)
                slot.inventory = equipment;
        }

        for (int i = 0; i < attributes.Length; i++)
        {
            attributes[i].SetParent(this);
            attributes[i].value.UpdateModifiedValue();
        }

    }

    private void Start()
    {
        for (int i = 0; i < equipment.GetSlots.Length; i++)
        {
            equipment.GetSlots[i].OnBeforeUpdate += OnRemoveItem;
            equipment.GetSlots[i].OnAfterUpdate += OnAddItem;
        }

        for (int i = 0; i < equipment.GetSlots.Length; i++)
        {
            var slot = equipment.GetSlots[i];
            if (slot != null && slot.ItemObject != null)
            {
                OnAddItem(slot);
            }
        }
    }

    private void Update()
    {
        if (playerStats != null && playerStats.isPlayerDead) return;
    }

    public bool TryAddItem(Item item)
    {
        return inventory.AddItem(item, 1);
    }

    public void OnRemoveItem(InventorySlot _slot)
    {
        if (_slot.ItemObject == null) return;
        InventoryObject sourceInv = _slot.inventory;
        if (sourceInv == null && _slot.parent != null) 
            sourceInv = _slot.parent.inventory;

        if (sourceInv == null || sourceInv.type != InterfaceType.Equipment) return;

        foreach (var buff in _slot.item.buffs)
        {
            foreach (var attr in attributes)
            {
                if (attr.type == buff.attribute)
                    attr.value.RemoveModifier(buff);
            }
        }
    }

    public void OnAddItem(InventorySlot _slot)
    {
        if (this == null || gameObject == null) return;
        if (_slot == null || _slot.ItemObject == null) return;
        InventoryObject sourceInv = _slot.inventory;
        if (sourceInv == null && _slot.parent != null)
            sourceInv = _slot.parent.inventory;

        if (sourceInv == null || sourceInv.type != InterfaceType.Equipment) return;

        foreach (var buff in _slot.item.buffs)
        {
            foreach (var attr in attributes)
            {
                if (attr.type == buff.attribute)
                    attr.value.AddModifier(buff);
            }
        }
    }

    public void AttributeModified(Attribute attribute) { }

    public int GetAttributeValue(Attributes type)
    {
        if (attributes == null) return 0;
        for (int i = 0; i < attributes.Length; i++)
        {
            if (attributes[i].type == type)
                return attributes[i].value.ModifiedValue;
        }
        return 0;
    }

    private void OnApplicationQuit()
    {
        inventory.Clear();
        equipment.Clear();
    }
}

[System.Serializable]
public class Attribute
{
    [System.NonSerialized] public InventoryPlayer parent;
    public Attributes type;
    public ModifiableInt value;

    public void SetParent(InventoryPlayer _parent)
    {
        parent = _parent;
        if (value == null)
            value = new ModifiableInt(AttributeModified);
        else
            value.RegsiterModEvent(AttributeModified);

        value.UpdateModifiedValue();
    }

    public void AttributeModified()
    {
        parent.AttributeModified(this);
    }
}