using System.Collections.Generic;
using UnityEngine;

public static class AdventureTextJsonImporter
{
    public static List<AdventureTextData> Import()
    {
        string fileName = StoryManager.SelectedStoryId + "Gameadventures";
        string resourcePath = "GameData/" + fileName;
        
        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
        
        if (jsonFile != null)
        {
            string jsonText = jsonFile.text;
            return AdventureTextJsonLoader.LoadFromJson(jsonText);
        }
        else
        {
            Debug.LogError($"Failed to load {fileName}.json from Resources folder");
            return new List<AdventureTextData>();
        }
    }
}
