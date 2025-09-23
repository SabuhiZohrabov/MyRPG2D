using System;
using System.Collections.Generic;

[Serializable]
public class ShopData
{
    public string shopId;
    public string shopName;
    public List<string> items;

    public ShopData()
    {
        items = new List<string>();
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