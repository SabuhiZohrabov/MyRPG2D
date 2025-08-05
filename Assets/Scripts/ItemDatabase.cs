using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    [SerializeField] private List<ItemSO> items;

    private Dictionary<string, ItemSO> itemDict;

    private void Awake()
    {
        if (!Application.isPlaying) return; // Editor mode check

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Dictionary yarad
        itemDict = items.ToDictionary(item => item.itemId, item => item);
    }

    public ItemSO GetItemById(string itemId)
    {
        if (itemDict.TryGetValue(itemId, out var item))
            return item;

        Debug.LogWarning($"Item not found: {itemId}");
        return null;
    }
}
