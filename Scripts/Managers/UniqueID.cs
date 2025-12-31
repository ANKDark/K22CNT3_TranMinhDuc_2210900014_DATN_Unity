using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class UniqueID : MonoBehaviour
{
    public string uniqueId;

    private void Reset()
    {
        Generate();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueId))
        {
            if (PrefabUtility.IsPartOfPrefabInstance(this))
            {
                uniqueId = System.Guid.NewGuid().ToString();
            }
            else
            {
                Generate();
            }
        }

        if (!Application.isPlaying)
            EditorUtility.SetDirty(this);
    }
#endif

    private void Generate()
    {
        uniqueId = System.Guid.NewGuid().ToString();
    }
}
