using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;

public abstract class UserInterface : MonoBehaviour
{
    public InventoryPlayer inventoryPlayer;
    public InventoryObject inventory;
    public Dictionary<GameObject, InventorySlot> slotOnInterface = new Dictionary<GameObject, InventorySlot>();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        inventoryPlayer = FindFirstObjectByType<InventoryPlayer>();
    }

    void Start()
    {
        if (inventoryPlayer == null)
            inventoryPlayer = FindFirstObjectByType<InventoryPlayer>();

        CreateSlots();
        for (int i = 0; i < inventory.container.Slots.Length; i++)
        {
            inventory.container.Slots[i].parent = this;
            inventory.GetSlots[i].OnAfterUpdate += OnSlotUpdate;
        }
        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
        
        slotOnInterface.UpdateSlotDisplay();
    }

    protected void OnSlotUpdate(InventorySlot _slot)
    {
        if (_slot.slotDisplay == null)
        {
            return;
        }

        Image image = null;
        Transform iconTransform = _slot.slotDisplay.transform.Find("Icon");
        if (iconTransform != null)
        {
            image = iconTransform.GetComponent<Image>();
        }
        
        // Fallback to first child if "Icon" not found
        if (image == null && _slot.slotDisplay.transform.childCount > 0)
        {
            image = _slot.slotDisplay.transform.GetChild(0).GetComponent<Image>();
        }

        if (image == null) return;

        var textUI = _slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>();

        if (_slot.item != null && _slot.item.Id >= 0)
        {
            image.sprite = _slot.ItemObject.uiDisplay;
            image.color = new Color(1f, 1f, 1f, 1f);
            if (textUI != null) textUI.text = _slot.amount == 1 ? "" : _slot.amount.ToString("n0");
        }
        else
        {
            var placeholder = _slot.slotDisplay.GetComponent<SlotPlaceholder>();
            if (placeholder != null && placeholder.placeholderSprite != null)
            {
                image.sprite = placeholder.placeholderSprite;
                image.color = Color.white;
            }
            else
            {
                image.sprite = null;
                image.color = new Color(1f, 1f, 1f, 0f);
            }
            if (textUI != null) textUI.text = "";
        }
    }

    public abstract void CreateSlots();

    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null) trigger = obj.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    public void OnEnter(GameObject obj)
    {
        MouseData.slotHoveredOver = obj;
        if (slotOnInterface[obj].item.Id >= 0)
        {
            if (ItemTooltip.instance == null)
            {
                ItemTooltip.instance = FindFirstObjectByType<ItemTooltip>(FindObjectsInactive.Include);
            }

            if (ItemTooltip.instance != null)
            {
                ItemTooltip.instance.Show(
                    slotOnInterface[obj].ItemObject.data.name ?? "Không có tên item",
                    slotOnInterface[obj].item.BuffsToString(),
                    slotOnInterface[obj].item.description ?? "Không có mô tả"
                );
            }
            else
            {
                Debug.LogWarning("ItemTooltip.instance is NULL! Make sure the Tooltip object is in the Scene and Active.");
            }
        }
    }

    public void OnExit(GameObject obj)
    {
        MouseData.slotHoveredOver = null;
        if (ItemTooltip.instance != null)
            ItemTooltip.instance.Hide();
    }

    public void OnExitInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = null;
    }

    public void OnEnterInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = obj.GetComponent<UserInterface>();
    }

    public void OnDragStart(GameObject obj)
    {
        MouseData.tempItemBeingDragged = CreateTempItem(obj);
    }

    public GameObject CreateTempItem(GameObject obj)
    {
        GameObject tempItem = null;
        if (slotOnInterface[obj].item.Id >= 0)
        {
            tempItem = new GameObject("MouseItem");
            var rt = tempItem.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50, 50);
            tempItem.transform.SetParent(transform.parent);

            if (slotOnInterface[obj].item.Id >= 0)
            {
                var img = tempItem.AddComponent<Image>();
                img.sprite = slotOnInterface[obj].ItemObject.uiDisplay;
                img.raycastTarget = false;
            }
        }
        return tempItem;
    }

    public void OnDragEnd(GameObject obj)
    {
        Destroy(MouseData.tempItemBeingDragged);
        if (MouseData.interfaceMouseIsOver == null)
        {
            slotOnInterface[obj].RemoveItem(); return;
        }
        if (MouseData.slotHoveredOver)
        {
            InventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotOnInterface[MouseData.slotHoveredOver]; inventory.SwapItems(slotOnInterface[obj], mouseHoverSlotData);
        }
    }
    public void OnDrag(GameObject obj)
    {
        if (MouseData.tempItemBeingDragged != null)
        {
            MouseData.tempItemBeingDragged.GetComponent<RectTransform>().position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        }
    }

    public void OnPointerClick(GameObject obj, BaseEventData data)
    {
        PointerEventData pData = (PointerEventData)data;
        if (pData.button == PointerEventData.InputButton.Right)
        {
            if (inventoryPlayer != null && slotOnInterface.ContainsKey(obj))
            {
                inventoryPlayer.UseItem(slotOnInterface[obj]);
            }
        }
    }

    public static class MouseData
    {
        public static UserInterface interfaceMouseIsOver;
        public static GameObject tempItemBeingDragged;
        public static GameObject slotHoveredOver;
    }
}

public static class ExtensionMethods
{
    public static void UpdateSlotDisplay(this Dictionary<GameObject, InventorySlot> _slotOnInterface)
    {
        foreach (KeyValuePair<GameObject, InventorySlot> _slot in _slotOnInterface)
        {
            var image = _slot.Key.transform.GetChild(0).GetComponent<Image>();
            var textUI = _slot.Key.GetComponentInChildren<TextMeshProUGUI>();

            if (_slot.Value.item != null && _slot.Value.item.Id >= 0)
            {
                image.sprite = _slot.Value.ItemObject.uiDisplay;
                image.color = new Color(1f, 1f, 1f, 1f);
                textUI.text = _slot.Value.amount == 1 ? "" : _slot.Value.amount.ToString("n0");
            }
            else
            {
                var placeholder = _slot.Key.GetComponent<SlotPlaceholder>();
                if (placeholder != null && placeholder.placeholderSprite != null)
                {
                    image.sprite = placeholder.placeholderSprite;
                    image.color = Color.white;
                }
                else
                {
                    image.sprite = null;
                    image.color = new Color(1f, 1f, 1f, 0f);
                }
                textUI.text = "";
            }
        }
    }
}