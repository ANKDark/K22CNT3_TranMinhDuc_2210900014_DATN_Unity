using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class ItemPickUp : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI itemTypeText;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public Image itemImage;

    public void ShowPickUp(ItemObject item)
    {
        itemTypeText.text = item.type.ToString();
        itemNameText.text = item.name;
        itemDescriptionText.text = item.data.description;
        if(item.uiDisplay != null)
        {
            itemImage.sprite = item.uiDisplay;
            itemImage.gameObject.SetActive(true);
        }
        else
        {
            itemImage.gameObject.SetActive(false);
        }
        gameObject.SetActive(true);
    }

    public void HidePickUp()
    {
        gameObject.SetActive(false);
    }
}
