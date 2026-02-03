using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

public enum InterfaceType
{
    Inventory,
    Equipment,
    Chest
}

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    public Inventory container = new Inventory();
    public string savePath;
    public InterfaceType type;
    public ItemDatabaseObject database;
    public InventorySlot[] GetSlots { get { return container.Slots; } }

    public bool AddItem(Item _item, int _amount)
    {
        if (EmptySlotCount <= 0) return false;
        InventorySlot slot = FindItemOnInventory(_item);
        if (!database.GetItem[_item.Id].stackable || slot == null)
        {
            SetEmptySlot(_item, _amount);
            return true;
        }
        slot.AddAmount(_amount);
        return true;
    }

    public int EmptySlotCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < GetSlots.Length; i++)
            {
                if (GetSlots[i].item == null || GetSlots[i].item.Id <= -1) counter++;
            }
            return counter;
        }
    }

    public InventorySlot FindItemOnInventory(Item _item)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].item != null && GetSlots[i].item.Id == _item.Id)
            {
                return GetSlots[i];
            }
        }
        return null;
    }

    public bool IsEmpty()
    {
        foreach (var slot in container.Slots)
        {
            if (slot.item != null && slot.item.Id >= 0 && slot.amount > 0)
            {
                return false;
            }
        }
        return true;
    }

    public InventorySlot SetEmptySlot(Item _item, int _amount)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].item == null || GetSlots[i].item.Id <= -1)
            {
                GetSlots[i].UpdateSlot(_item, _amount);
                return GetSlots[i];
            }
        }
        return null;
    }

    public void SwapItems(InventorySlot item1, InventorySlot item2)
    {
        if (item2.CanPlaceInSlot(item1.ItemObject) && item1.item != null && item2.item != null && item1.item.Id == item2.item.Id)
        {
            var itemObj = item1.ItemObject;
            if (itemObj != null && itemObj.stackable)
            {
                item2.AddAmount(item1.amount);
                item1.RemoveItem();
                return;
            }
        }

        if (item2.CanPlaceInSlot(item1.ItemObject) && item1.CanPlaceInSlot(item2.ItemObject))
        {
            if (item2.parent != null && item2.parent.inventory.type == InterfaceType.Equipment && item1.amount > 1 && (item2.item == null || item2.item.Id < 0))
            {
                item2.UpdateSlot(item1.item, 1);
                item1.AddAmount(-1);
                return;
            }

            InventorySlot temp = new InventorySlot(item2.item, item2.amount);
            item2.UpdateSlot(item1.item, item1.amount);
            item1.UpdateSlot(temp.item, temp.amount);
        }
    }

    public void RemoveItem(Item _item)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].item == _item)
            {
                GetSlots[i].UpdateSlot(null, 0);
            }
        }
    }

    [ContextMenu("Save")]
    public void Save()
    {
        IFormatter formatter = new BinaryFormatter();
        string path = string.Concat(Application.persistentDataPath, savePath);
        using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            formatter.Serialize(stream, container);
        }
    }

    [ContextMenu("Load")]
    public void Load()
    {
        string path = string.Concat(Application.persistentDataPath, savePath);
        if (File.Exists(path))
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                Inventory newContainer = (Inventory)formatter.Deserialize(stream);
                for (int i = 0; i < GetSlots.Length; i++)
                {
                    if (i < newContainer.Slots.Length)
                    {
                        GetSlots[i].UpdateSlot(newContainer.Slots[i].item, newContainer.Slots[i].amount);
                    }
                    else
                    {
                        GetSlots[i].UpdateSlot(new Item(), 0); // Clear slot if save file doesn't have data for it
                    }
                }
            }
        }
    }
    
    public void RefreshInventory()
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            GetSlots[i].UpdateSlot(GetSlots[i].item, GetSlots[i].amount);
        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        container.Clear();
    }

    public void InitializeBuffs()
    {
        foreach (var slot in GetSlots)
        {
            if (slot.item != null && slot.item.Id >= 0 && slot.item.buffs != null)
            {
                foreach (var buff in slot.item.buffs)
                {
                    if (buff.value == 0 && (buff.min != 0 || buff.max != 0))
                    {
                        buff.GenerateValue();
                    }
                }
            }
        }
    }
}

public delegate void SlotUpdated(InventorySlot _slot);

[System.Serializable]
public class Inventory
{
    public InventorySlot[] Slots;

    public Inventory()
    {
        Slots = new InventorySlot[60];
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i] = new InventorySlot();
        }
    }

    public void Clear()
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i].RemoveItem();
        }
    }
}

[System.Serializable]
public class InventorySlot
{
    public ItemType[] AllowedItems = new ItemType[0];
    [System.NonSerialized]
    public UserInterface parent;
    [System.NonSerialized]
    public GameObject slotDisplay;
    [System.NonSerialized]
    public InventoryObject inventory;
    [System.NonSerialized]
    public SlotUpdated OnAfterUpdate;
    [System.NonSerialized]
    public SlotUpdated OnBeforeUpdate;

    public Item item;
    public int amount;

    public ItemObject ItemObject
    {
        get
        {
            if (item == null || item.Id < 0) return null;

            if (inventory != null && inventory.database != null)
                return inventory.database.GetItem[item.Id];

            if (parent != null && parent.inventory != null && parent.inventory.database != null)
                return parent.inventory.database.GetItem[item.Id];
            return null;
        }
    }

    public InventorySlot()
    {
        UpdateSlot(new Item(), 0);
    }

    public InventorySlot(Item _item, int _amount)
    {
        UpdateSlot(_item, _amount);
    }

    public void UpdateSlot(Item _item, int _amount)
    {
        if (OnBeforeUpdate != null) OnBeforeUpdate(this);
        item = _item;
        amount = _amount;

        var db = inventory?.database ?? parent?.inventory?.database;
        if (item != null && item.Id >= 0 && db != null)
        {
            ItemObject obj = db.GetItem[item.Id];
            if (obj != null)
            {
                item.worldModel = obj.worldModel;
                item.name = obj.data.name;
            }
        }

        if (OnAfterUpdate != null) OnAfterUpdate(this);
    }

    public void RemoveItem()
    {
        UpdateSlot(new Item(), 0);
    }

    public void AddAmount(int value)
    {
        UpdateSlot(item, amount += value);
    }

    public bool CanPlaceInSlot(ItemObject _item)
    {
        if (AllowedItems.Length <= 0 || _item == null || _item.data.Id < 0) return true;

        for (int i = 0; i < AllowedItems.Length; i++)
        {
            if (_item.type == AllowedItems[i]) return true;
        }
        return false;
    }
}
