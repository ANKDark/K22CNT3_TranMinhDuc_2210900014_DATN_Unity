using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class GroundItem : MonoBehaviour
{
    public ItemObject item;

    [HideInInspector] public UniqueID uid;

    private void Reset()
    {
        RefreshModel();
    }

    private void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            RefreshModel();
#endif
    }

    private void Start()
    {
        uid = GetComponent<UniqueID>();
        if (Application.isPlaying && transform.childCount == 0)
        {
            RefreshModel();
        }

        if (uid == null)
        {
            return;
        }

        SaveData saveData = SaveSystem.Load();
        if (saveData.collectedItems.Contains(uid.uniqueId))
        {
            Destroy(gameObject);
        }
    }

    public Item itemInstance;

    public void SetItem(Item newItem)
    {
        itemInstance = newItem;
        if (itemInstance.worldModel != null)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            GameObject model = Instantiate(itemInstance.worldModel, transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
        }
    }

    private void RefreshModel()
    {
        GameObject prefabToSpawn = null;
        if (itemInstance != null && itemInstance.worldModel != null)
            prefabToSpawn = itemInstance.worldModel;
        else if (item != null && item.worldModel != null)
            prefabToSpawn = item.worldModel;

        if (prefabToSpawn == null) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
            else DestroyImmediate(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
             GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn, this.transform);
             modelInstance.transform.localPosition = Vector3.zero;
             modelInstance.transform.localRotation = Quaternion.identity;
             modelInstance.transform.localScale = Vector3.one;
        }
        else
#endif
        {
            GameObject modelInstance = Instantiate(prefabToSpawn, transform);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one;
        }
    }
}