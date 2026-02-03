using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string path = Application.persistentDataPath + "/save.json";

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public static SaveData Load()
    {
        if (!File.Exists(path)) return new SaveData();

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool HasSave()
    {
        return File.Exists(path);
    }

    public static void DeleteSave()
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    public static void DeleteAllSaves()
    {
        DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);
        if (!di.Exists) return;
        FileInfo[] files = di.GetFiles();
        foreach (FileInfo file in files)
        {
            file.Delete();
        }
        
        if (File.Exists(path))
            File.Delete(path);
            
        Debug.Log("Deleted all save items & chests.");
    }
}
