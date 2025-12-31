using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipmentVisuals : MonoBehaviour
{
    public InventoryObject equipment;

    private Dictionary<ItemType, GameObject> currentEquippedParts =
        new Dictionary<ItemType, GameObject>();
    private BoneCombiner boneCombiner;

    private Dictionary<ItemType, string> folderMap = new Dictionary<ItemType, string>
    {
        { ItemType.ArmArmor, "Arm Armor" },
        { ItemType.BeltArmor, "Belt Armor" },
        { ItemType.ChestArmor, "Chest Armor" },
        { ItemType.FeetArmor, "Feet Armor" },
    };

    private void Awake()
    {
        boneCombiner = new BoneCombiner(gameObject);

        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            currentEquippedParts[type] = null;
    }

    private void Start()
    {
        for (int i = 0; i < equipment.GetSlots.Length; i++)
        {
            equipment.GetSlots[i].OnBeforeUpdate += OnRemoveItem;
            equipment.GetSlots[i].OnAfterUpdate += OnAddItem;
        }

        RestoreEquippedItems();
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame)
        {
            RestoreEquippedItems();
        }
    }

    public void OnRemoveItem(InventorySlot _slot)
    {
        if (_slot.ItemObject == null)
            return;
        if (_slot.parent.inventory.type != InterfaceType.Equipment)
            return;

        ItemType equipType = _slot.AllowedItems[0];

        if (currentEquippedParts.ContainsKey(equipType) && currentEquippedParts[equipType] != null)
        {
            currentEquippedParts[equipType].SetActive(false);
            currentEquippedParts[equipType] = null;
        }
    }

    public void OnAddItem(InventorySlot _slot)
    {
        if (this == null || gameObject == null)
            return;
        if (_slot == null || _slot.ItemObject == null)
            return;
        if (_slot.parent == null || _slot.parent.inventory == null)
            return;
        if (_slot.parent.inventory.type != InterfaceType.Equipment)
            return;

        EquipItem(_slot);
    }

    private void EquipItem(InventorySlot _slot)
    {
        if (this == null || gameObject == null)
            return;
        if (_slot == null || _slot.ItemObject == null)
            return;

        ItemType equipType = _slot.AllowedItems[0];
        GameObject newPart = null;

        if (equipType == ItemType.Sword)
        {
            string swordPath =
                "Base Character Root/spine_01/spine_02/spine_03/spine_04/shoulder_r/upperarm_r/forearm_r/hand_r/Swords";
            Transform swordFolder = transform.Find(swordPath);

            if (swordFolder != null)
            {
                Transform newPartTransform = swordFolder.Find(_slot.ItemObject.name);
                if (newPartTransform != null)
                    newPart = newPartTransform.gameObject;
                else if (_slot.ItemObject.characterDisplay != null)
                    newPart = Instantiate(_slot.ItemObject.characterDisplay, swordFolder);
            }
        }
        else if (_slot.ItemObject.characterDisplay != null)
        {
            if (equipType == ItemType.HeadArmor || equipType == ItemType.LegsArmor)
            {
                Transform newPartTransform = transform.Find(_slot.ItemObject.name);
                if (newPartTransform != null)
                    newPart = newPartTransform.gameObject;
            }
            else if (folderMap.ContainsKey(equipType))
            {
                Transform modelFolder = transform.Find(folderMap[equipType]);
                if (modelFolder != null)
                {
                    Transform newPartTransform = modelFolder.Find(_slot.ItemObject.name);
                    if (newPartTransform != null)
                        newPart = newPartTransform.gameObject;
                    else
                        newPart = Instantiate(_slot.ItemObject.characterDisplay, modelFolder);
                }
            }
        }

        if (newPart != null)
        {
            if (currentEquippedParts[equipType] != null)
                currentEquippedParts[equipType].SetActive(false);

            newPart.SetActive(true);
            currentEquippedParts[equipType] = newPart;
        }
    }

    public void RestoreEquippedItems()
    {
        foreach (var kvp in currentEquippedParts)
        {
            if (kvp.Value != null)
                kvp.Value.SetActive(false);
        }

        foreach (var slot in equipment.GetSlots)
        {
            if (slot != null && slot.ItemObject != null && slot.ItemObject.characterDisplay != null)
            {
                EquipItem(slot);
            }
        }
    }

    private void OnApplicationQuit()
    {
        foreach (var kvp in currentEquippedParts)
        {
            if (kvp.Value != null)
                kvp.Value.SetActive(false);
        }
    }
}
