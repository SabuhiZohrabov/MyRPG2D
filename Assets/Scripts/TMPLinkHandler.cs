using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TMPLinkHandler : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI textMeshPro;
    public AdventureManager adventureManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, eventData.position, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();
            adventureManager.OnTextLinkClicked(linkID);
        }
    }
}
