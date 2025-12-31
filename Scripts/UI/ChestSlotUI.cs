using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ChestSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image icon;
    public TextMeshProUGUI amountText;
    [HideInInspector] public InventorySlot slotData;
    private ChestUI parentUI;

    public void Init(InventorySlot _slot, ChestUI _parent)
    {
        slotData = _slot;
        parentUI = _parent;
        UpdateSlotVisual();
    }

    public void UpdateSlotVisual()
    {
        if (slotData == null)
        {
            return;
        }

        if (slotData.ItemObject == null)
        {   
            icon.gameObject.SetActive(false);
            amountText.text = "";
            return;
        }
        icon.sprite = slotData.ItemObject.uiDisplay;
        amountText.text = slotData.amount > 1 ? slotData.amount.ToString() : "";
        icon.gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        parentUI.hoveredSlot = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        parentUI.hoveredSlot = null;
    }
}
