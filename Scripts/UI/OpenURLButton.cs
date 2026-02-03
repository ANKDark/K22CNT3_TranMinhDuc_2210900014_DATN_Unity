using UnityEngine;

public class OpenURLButton : MonoBehaviour
{
    [SerializeField] private string url = "http://darkdungeon.ankdark.id.vn/guide";

    public void OpenURL()
    {
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
        }
        else
        {
            Debug.LogWarning("OpenURLButton: URL null hoặc rỗng!");
        }
    }

    public void OpenSpecificURL(string customUrl)
    {
        if (!string.IsNullOrEmpty(customUrl))
        {
            Application.OpenURL(customUrl);
        }
    }
}
