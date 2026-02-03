using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StaticInterface : UserInterface
{
    public GameObject[] slots;

    public override void CreateSlots()
    {
        slotOnInterface = new Dictionary<GameObject, InventorySlot>();

        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });

        int maxSlots = Mathf.Min(slots.Length, inventory.GetSlots.Length);

        for (int i = 0; i < maxSlots; i++)
        {
            var obj = slots[i];

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });
            AddEvent(obj, EventTriggerType.PointerClick, (data) => { OnPointerClick(obj, data); });


            var slot = inventory.GetSlots[i];
            slot.slotDisplay = obj;
            slot.parent = this;
            slot.inventory = inventory;

            slotOnInterface.Add(obj, slot);

            OnSlotUpdate(slot);
        }
    }
}
