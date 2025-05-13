using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game!");
        Application.Quit();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
}
