using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Item", menuName = "Inventory System/Items/Quest Item")]
public class QuestItemObject : ItemObject
{
    public void Awake()
    {
        type = ItemType.QuestItem;
    }
}
