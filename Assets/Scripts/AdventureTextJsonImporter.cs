using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class AdventureTextJsonImporter
{
    public static List<AdventureTextData> Import()
    {
        string path = Path.Combine(Application.streamingAssetsPath, StoryManager.SelectedStoryId + "Gameadventures.json");
        string jsonText = File.ReadAllText(path);

        return AdventureTextJsonLoader.LoadFromJson(jsonText);
    }
}
