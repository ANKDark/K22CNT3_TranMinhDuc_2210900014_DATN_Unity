using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [Header("Settings")]
    public string firstSceneName = "MainMenu";

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(firstSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == firstSceneName)
        {
            if (PersistentUI.instance != null)
            {
                PersistentUI.instance.ShowCanvas();
                PersistentUI.instance.ShowCamera();
            }
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
