using UnityEngine;

public class DeadMenuController : MonoBehaviour
{
    private void Start()
    {

    }

    public void PlayNewGame()
    {
        GameManager.Instance.NewGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        GameManager.Instance.RestartLevel();
    }
}
