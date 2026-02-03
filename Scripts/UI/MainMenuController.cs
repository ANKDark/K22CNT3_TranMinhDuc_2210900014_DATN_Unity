using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public GameObject continueButton;

    private void Start()
    {
        bool canContinue = SaveSystem.HasSave() || PlayerPrefs.GetInt("HasSaveGame", 0) == 1;
        continueButton.SetActive(canContinue);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PlayNewGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GameManager.Instance.NewGame();
    }

    public void ContinueGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GameManager.Instance.ContinueGame();
    }

    public void OpenGuideWebsite()
    {
        Application.OpenURL("http://darkdungeon.ankdark.id.vn/guide");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
