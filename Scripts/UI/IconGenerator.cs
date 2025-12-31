using UnityEngine;
using System.IO;

public class IconGenerator : MonoBehaviour
{
    public Camera iconCamera;
    public Transform objectToSnapshot;
    public string fileName = "NewIcon";
    [ContextMenu("Take Snapshot")]
    public void TakeSnapshot()
    {
        if (iconCamera == null) iconCamera = GetComponentInChildren<Camera>();

        RenderTexture rt = RenderTexture.GetTemporary(256, 256, 24);
        iconCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(256, 256, TextureFormat.RGBA32, false);

        iconCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        screenShot.Apply();

        iconCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string path = Application.dataPath + "/Icons/";
        
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        
        File.WriteAllBytes(path + fileName + ".png", bytes);
        Debug.Log("Đã lưu icon tại: " + path + fileName + ".png");
        
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}