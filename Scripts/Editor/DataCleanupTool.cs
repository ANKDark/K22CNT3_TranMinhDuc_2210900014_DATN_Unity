using UnityEngine;
using UnityEditor;

public class DataCleanupTool : EditorWindow
{
    [MenuItem("Tools/Dungeon Dark/Clear All Data & Build Ready")]
    public static void ClearAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        SaveSystem.DeleteAllSaves();
        
        EditorUtility.DisplayDialog("Dọn dẹp thành công", 
            "Đã xóa toàn bộ PlayerPrefs và các file trong persistentDataPath.\nBây giờ bạn có thể Build game sạch hoàn toàn.", "OK");
        
        Debug.Log("<color=green><b>[DataCleanupTool]</b></color> Toàn bộ dữ liệu save và preferences đã được xóa sạch.");
    }

    [MenuItem("Tools/Dungeon Dark/Open Save Folder")]
    public static void OpenSaveFolder()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
