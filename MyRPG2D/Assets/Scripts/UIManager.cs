using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject mapPanel;
    public GameObject adventurePanel;
    public GameObject characterPanel;

    void Start()
    {
        CloseAllPanels();
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
    }
}
