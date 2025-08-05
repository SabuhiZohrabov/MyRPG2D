using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class AdventureTextJsonImporter
{
    public static List<AdventureTextData> Import()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "adventure.json");
        string jsonText = File.ReadAllText(path);
                
        return AdventureTextJsonLoader.LoadFromJson(jsonText);
    }
}
