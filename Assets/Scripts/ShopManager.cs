using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ShopManager
{
    private static ShopManager _instance;
    public static ShopManager Instance
    {
        get
        {
            if (!Application.isPlaying) return null;
            return _instance ?? (_instance = new ShopManager());
        }
    }

    private ShopDataList shopDataList;
    private Dictionary<string, ShopData> shopDict;

    private ShopManager()
    {
        LoadShopsFromJSON();
    }

    private void LoadShopsFromJSON()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "shops.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError("shops.json file not found in StreamingAssets folder");
            shopDataList = new ShopDataList();
            shopDict = new Dictionary<string, ShopData>();
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(filePath);
            shopDataList = JsonUtility.FromJson<ShopDataList>(jsonContent);

            // Create dictionary for fast access
            shopDict = shopDataList.shops.ToDictionary(shop => shop.shopId, shop => shop);

            Debug.Log($"Loaded {shopDataList.shops.Count} shops from JSON");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading shops.json: {e.Message}");
            shopDataList = new ShopDataList();
            shopDict = new Dictionary<string, ShopData>();
        }
    }

    // Get all shops
    public List<ShopData> GetAllShops()
    {
        return shopDataList?.shops ?? new List<ShopData>();
    }

    // Get shop by ID
    public ShopData GetShopById(string shopId)
    {
        if (shopDict.TryGetValue(shopId, out var shop))
            return shop;

        Debug.LogWarning($"Shop not found: {shopId}");
        return null;
    }

    // Get all item IDs from a specific shop
    public List<string> GetShopItems(string shopId)
    {
        var shop = GetShopById(shopId);
        return shop?.items ?? new List<string>();
    }

    // Check if item exists in any shop
    public bool IsItemInAnyShop(string itemId)
    {
        foreach (var shop in shopDataList.shops)
        {
            if (shop.items.Contains(itemId))
                return true;
        }
        return false;
    }

    // Get shops that contain a specific item
    public List<ShopData> GetShopsContainingItem(string itemId)
    {
        var shopsWithItem = new List<ShopData>();

        foreach (var shop in shopDataList.shops)
        {
            if (shop.items.Contains(itemId))
            {
                shopsWithItem.Add(shop);
            }
        }

        return shopsWithItem;
    }

    // Check if specific shop contains an item
    public bool DoesShopContainItem(string shopId, string itemId)
    {
        var shop = GetShopById(shopId);
        return shop != null && shop.items.Contains(itemId);
    }

    // Reload shops from JSON (useful for runtime updates)
    public void ReloadShops()
    {
        LoadShopsFromJSON();
    }
}