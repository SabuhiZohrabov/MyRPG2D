using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Main Panels")]
    public GameObject mapPanel;
    public GameObject adventurePanel;
    public GameObject characterPanel;
    public GameObject combatPanel;

    [Header("Character Panels")]
    public GameObject attributePanel;
    public GameObject inventoryPanel;
    public GameObject skillPanel;


    [Header("End Game UI")]
    public GameObject endGamePanel;
    public TextMeshProUGUI endGameMessage;

    [Header("Battle Won UI")]
    public GameObject BattleWonPanel;
    public TextMeshProUGUI BattleWonMessage;
    public TextMeshProUGUI WonExpMessage;
    public TextMeshProUGUI WonLootMessage;

    private void Awake()
    {
        if (!Application.isPlaying) return;
        Instance = this;
    }

    void Start()
    {
        CloseAllPanels();
        if (adventurePanel != null)
            adventurePanel.SetActive(true);
    }

    public void OpenPanel(string panelName)
    {
        CloseAllPanels();

        switch (panelName)
        {
            case "MapPanel":
                mapPanel.SetActive(true);
                break;
            case "AdventurePanel":
                adventurePanel.SetActive(true);
                break;
            case "CharacterPanel":
                characterPanel.SetActive(true);
                break;
        }
    }

    private void CloseAllPanels()
    {
        mapPanel.SetActive(false);
        adventurePanel.SetActive(false);
        characterPanel.SetActive(false);
        combatPanel.SetActive(false);
        BattleWonPanel.SetActive(false);
        endGamePanel.SetActive(false);
        attributePanel.SetActive(false);
        inventoryPanel.SetActive(false);
        skillPanel.SetActive(false);
    }

    public void ShowEndGame(string message)
    {
        if (endGamePanel != null)
            endGamePanel.SetActive(true);

        if (endGameMessage != null)
            endGameMessage.text = message;
    }
    public void ShowWonGame(string message, int xpAmount = 0, string lootSummary = "")
    {
        if (BattleWonPanel != null)
            BattleWonPanel.SetActive(true);

        if (BattleWonMessage != null)
            BattleWonMessage.text = message;

        if (WonExpMessage != null)
            WonExpMessage.text = $"You gained {xpAmount} XP!";

        if (WonLootMessage != null)
            WonLootMessage.text = lootSummary;

    }

    public void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuClicked()
    {
        SceneManager.LoadScene("MainMenu"); // change this if your main menu scene name differs
    }

    public void OnContinueClicked()
    {
        CloseAllPanels();
        if (adventurePanel != null)
            adventurePanel.SetActive(true);
        //AdventureManager.Instance.ShowTextById(AdventureManager.Instance.currentAdventureTextData.nextLinkOnVictory);
    }
}
