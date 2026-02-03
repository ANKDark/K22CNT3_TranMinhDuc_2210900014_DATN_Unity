using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DynamicInterface : UserInterface
{
    [Header("Inventory UI")]
    public GameObject inventorySlotPrefab;

    public override void CreateSlots()
    {
        slotOnInterface = new Dictionary<GameObject, InventorySlot>();

        for (int i = 0; i < inventory.GetSlots.Length; i++)
        {
            GameObject obj = Instantiate(inventorySlotPrefab, transform);

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });
            AddEvent(obj, EventTriggerType.PointerClick, (data) => { OnPointerClick(obj, data); });

            InventorySlot slot = inventory.GetSlots[i];
            slot.slotDisplay = obj;
            slot.parent = this;
            slot.inventory = inventory;

            slotOnInterface.Add(obj, slot);

            OnSlotUpdate(slot);
        }
    }
}
