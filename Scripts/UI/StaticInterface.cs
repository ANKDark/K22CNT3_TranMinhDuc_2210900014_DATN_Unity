using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StaticInterface : UserInterface
{
    public GameObject[] slots;

    private Dictionary<InventorySlot, SlotUpdated> slotUpdateDelegates = new Dictionary<InventorySlot, SlotUpdated>();

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

            var slot = inventory.GetSlots[i];
            slot.slotDisplay = obj;
            slot.parent = this;
            slot.inventory = inventory;

            SlotUpdated updateDel = (s) =>
            {
                if (obj != null)
                    UpdateSlotDisplay(obj, s);
            };

            slot.OnAfterUpdate += updateDel;
            slotUpdateDelegates[slot] = updateDel;

            slotOnInterface.Add(obj, slot);

            UpdateSlotDisplay(obj, slot);
        }
    }

    private void UpdateSlotDisplay(GameObject obj, InventorySlot slot)
    {
        if (obj == null || slot == null) return;
        if (obj.transform.childCount == 0) return;

        var image = obj.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
        if (slot.item != null && slot.item.Id >= 0)
        {
            image.sprite = slot.ItemObject.uiDisplay;
            image.color = Color.white;
        }
        else
        {
            image.sprite = null;
            image.color = new Color(1, 1, 1, 0);
        }
    }
}
