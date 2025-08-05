using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    void Start()
    {
        if (!Application.isPlaying) return;
        InventoryManager.Instance.AddItem("minipotion", 5);
        InventoryManager.Instance.AddItem("shortsword", 1);
        InventoryManager.Instance.AddItem("woodenstaff", 1);
    }
}
