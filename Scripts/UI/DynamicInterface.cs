using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicInterface : UserInterface
{
    [Header("Inventory Settings")]
    public GameObject inventoryPrefab;

    [Header("UI Layout Settings")]
    public int X_SPACE_BETWEEN_ITEMS = 100;
    public int NUMBER_OF_COLUMN = 5;
    public int Y_SPACE_BETWEEN_ITEMS = 100;
    public int X_START = 0;
    public int Y_START = 0;

    private Dictionary<InventorySlot, SlotUpdated> slotUpdateDelegates = new Dictionary<InventorySlot, SlotUpdated>();

    public override void CreateSlots()
    {
        slotOnInterface = new Dictionary<GameObject, InventorySlot>();

        for (int i = 0; i < inventory.GetSlots.Length; i++)
        {
            var obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);

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
        var textUI = obj.GetComponentInChildren<TMPro.TextMeshProUGUI>();

        if (slot.item != null && slot.item.Id >= 0)
        {
            image.sprite = slot.ItemObject.uiDisplay;
            image.color = Color.white;
            if (textUI != null)
                textUI.text = slot.amount == 1 ? "" : slot.amount.ToString("n0");
        }
        else
        {
            image.sprite = null;
            image.color = new Color(1, 1, 1, 0);
            if (textUI != null)
                textUI.text = "";
        }
    }

    private Vector3 GetPosition(int index)
    {
        int row = index / NUMBER_OF_COLUMN;
        int col = index % NUMBER_OF_COLUMN;
        return new Vector3(
            X_START + X_SPACE_BETWEEN_ITEMS * col,
            Y_START - Y_SPACE_BETWEEN_ITEMS * row,
            0f
        );
    }
}
