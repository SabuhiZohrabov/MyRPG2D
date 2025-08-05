using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    public void StartGame()
    {
        if (!Application.isPlaying) return;
        
        SceneManager.LoadScene("GameScene");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game!");
        Application.Quit();
    }
    public void ResetGame()
    {
        DatabaseManager.Instance.ResetGame();
    }

}
