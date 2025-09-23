using System;
using System.Collections.Generic;

[Serializable]
public class ShopItem
{
    public string itemId;
    public int price;

    public ShopItem()
    {
    }

    public ShopItem(string itemId, int price)
    {
        this.itemId = itemId;
        this.price = price;
    }
}

[Serializable]
public class ShopData
{
    public string shopId;
    public string shopName;
    public List<ShopItem> items;

    public ShopData()
    {
        items = new List<ShopItem>();
    }
}

[Serializable]
public class ShopDataList
{
    public List<ShopData> shops;

    public ShopDataList()
    {
        shops = new List<ShopData>();
    }
}