using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InventoryObject))]
public class InventoryEditor : Editor
{
    private ItemObject itemToAdd;
    private int amountToAdd = 1;

    public override void OnInspectorGUI()
    {
        // Draw the default inspector (so existing slots show up)
        base.OnInspectorGUI();

        InventoryObject inventory = (InventoryObject)target;

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("⚡ Quick Add Item", EditorStyles.boldLabel);
        
        // Validation: Check if Database is linked
        if (inventory.database == null)
        {
            EditorGUILayout.HelpBox("Please assign an Item Database before adding items!", MessageType.Error);
            return;
        }

        // GUI for input
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        itemToAdd = (ItemObject)EditorGUILayout.ObjectField("Item To Add", itemToAdd, typeof(ItemObject), false);
        amountToAdd = EditorGUILayout.IntField("Amount", amountToAdd);

        if (GUILayout.Button("Add To Inventory"))
        {
            if (itemToAdd != null && amountToAdd > 0)
            {
                AddItem(inventory, itemToAdd, amountToAdd);
            }
            else
            {
                Debug.LogWarning("Please select an Item and Amount > 0.");
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void AddItem(InventoryObject inventory, ItemObject itemObject, int amount)
    {
        // 0. Update Serialized Object to make sure we have latest data
        serializedObject.Update();

        SerializedProperty containerProp = serializedObject.FindProperty("container");
        SerializedProperty slotsProp = containerProp.FindPropertyRelative("Slots");

        // 1. Find the item ID from the database
        int itemId = -1;
        bool foundInDb = false;
        
        // We use the reference from the target object because simpler than serializing the whole DB search
        for (int i = 0; i < inventory.database.ItemObjects.Length; i++)
        {
            if (inventory.database.ItemObjects[i] == itemObject)
            {
                itemId = i;
                foundInDb = true;
                break;
            }
        }

        if (!foundInDb)
        {
            Debug.LogError($"Item '{itemObject.name}' not found in the assigned Database!");
            return;
        }

        bool added = false;

        // Try to stack first if stackable
        if (itemObject.stackable)
        {
            for (int i = 0; i < slotsProp.arraySize; i++)
            {
                SerializedProperty slotProp = slotsProp.GetArrayElementAtIndex(i);
                SerializedProperty itemProp = slotProp.FindPropertyRelative("item");
                SerializedProperty idProp = itemProp.FindPropertyRelative("Id");
                
                // Check if slot has same Item ID that is not empty (-1)
                if (idProp.intValue == itemId)
                {
                    SerializedProperty amountProp = slotProp.FindPropertyRelative("amount");
                    amountProp.intValue += amount;
                    added = true;
                    Debug.Log($"Stacked {amount} {itemObject.name} to slot {i}.");
                    break;
                }
            }
        }

        // If not stacked, find empty slot
        if (!added)
        {
            for (int i = 0; i < slotsProp.arraySize; i++)
            {
                SerializedProperty slotProp = slotsProp.GetArrayElementAtIndex(i);
                SerializedProperty itemProp = slotProp.FindPropertyRelative("item");
                SerializedProperty idProp = itemProp.FindPropertyRelative("Id");

                // Check if slot is empty (ID <= -1 or null item)
                // Note: SerializedProperty for int default is 0, but our constructor sets -1. 
                // However, 'item' property might be partially initialized. 
                // Let's rely on the Id property check.
                if (idProp.intValue <= -1)
                {
                    // Update Item fields manually
                    SerializedProperty nameProp = itemProp.FindPropertyRelative("name");
                    SerializedProperty descProp = itemProp.FindPropertyRelative("description");
                    
                    idProp.intValue = itemId;
                    nameProp.stringValue = itemObject.name;
                    
                    // Assign description from data
                    // Note: accessing data.description from ItemObject
                    if (itemObject.data != null)
                        descProp.stringValue = itemObject.data.description;
                    else
                        descProp.stringValue = "";

                    // Update Amount
                    SerializedProperty amountProp = slotProp.FindPropertyRelative("amount");
                    amountProp.intValue = amount;

                    // Initialize Buffs array size if needed (Optional but recommended)
                    SerializedProperty buffsProp = itemProp.FindPropertyRelative("buffs");
                    if (itemObject.data.buffs != null)
                    {
                        buffsProp.arraySize = itemObject.data.buffs.Length;
                        for(int b=0; b < itemObject.data.buffs.Length; b++)
                        {
                           SerializedProperty buffElement = buffsProp.GetArrayElementAtIndex(b);
                           // Copy buff values... (Simplification: Just setting size usually enough for now, 
                           // ideally we copy Min/Max/Attribute)
                           var srcBuff = itemObject.data.buffs[b];
                           buffElement.FindPropertyRelative("attribute").enumValueIndex = (int)srcBuff.attribute;
                           buffElement.FindPropertyRelative("min").intValue = srcBuff.min;
                           buffElement.FindPropertyRelative("max").intValue = srcBuff.max;
                           buffElement.FindPropertyRelative("value").intValue = UnityEngine.Random.Range(srcBuff.min, srcBuff.max); 
                        }
                    }

                    added = true;
                    Debug.Log($"Added {amount} {itemObject.name} into new slot {i}.");
                    break;
                }
            }
        }

        if (added)
        {
            serializedObject.ApplyModifiedProperties(); // THIS triggers the Inspector update!
        }
        else
        {
            Debug.LogWarning("Inventory Full! Could not add item.");
        }
    }
}
