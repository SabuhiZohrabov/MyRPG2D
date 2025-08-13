using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[System.Serializable]
public class MapDataCollection
{
    public List<MapData> maps = new List<MapData>();
}

public class MapDataManager : MonoBehaviour
{
    public static MapDataManager Instance { get; private set; }

    private MapDataCollection mapCollection;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMapsFromFile();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadMapsFromFile()
    {
        string fileName = StoryManager.SelectedStoryId + "Gamemaps";
        string resourcePath = "GameData/" + fileName;
        
        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
        
        if (jsonFile != null)
        {
            string jsonContent = jsonFile.text;
            mapCollection = JsonUtility.FromJson<MapDataCollection>(jsonContent);

            if (mapCollection == null)
            {
                Debug.LogError($"[MapDataManager] Failed to parse JSON from {fileName}.json");
                mapCollection = new MapDataCollection();
            }
            else
            {
                Debug.Log($"[MapDataManager] Loaded {mapCollection.maps.Count} maps from Resources");
            }
        }
        else
        {
            Debug.LogError($"[MapDataManager] Map file {fileName}.json not found in Resources/GameData folder");
            mapCollection = new MapDataCollection();
        }
    }

    public void SaveMapsToFile()
    {
        if (mapCollection == null)
        {
            Debug.LogError("[MapDataManager] No map data to save");
            return;
        }

        string fileName = StoryManager.SelectedStoryId + "Gamemaps";
        string jsonContent = JsonUtility.ToJson(mapCollection, true);
        
        // Save to persistent data path (writable location)
        string savePath = Path.Combine(Application.streamingAssetsPath, fileName + ".json");
        
        try
        {
            File.WriteAllText(savePath, jsonContent);
            Debug.Log($"[MapDataManager] Successfully saved {mapCollection.maps.Count} maps to {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MapDataManager] Failed to save maps to file: {e.Message}");
        }
    }


    // Public API for MapManager
    public List<MapData> GetAllMaps()
    {
        return mapCollection?.maps ?? new List<MapData>();
    }

    public MapData GetMapById(string mapId)
    {
        return mapCollection?.maps?.FirstOrDefault(m => m.mapId == mapId);
    }

    public void AddMap(MapData newMap)
    {
        Debug.LogWarning("[MapDataManager] AddMap: Runtime map modifications are not supported when loading from Resources");
    }

    public void UpdateMap(MapData updatedMap)
    {
        if (mapCollection?.maps == null) return;

        int index = mapCollection.maps.FindIndex(m => m.mapId == updatedMap.mapId);
        if (index >= 0)
        {
            mapCollection.maps[index] = updatedMap;
            //SaveMapsToFile();
        }
        else
        {
            Debug.LogWarning($"[MapDataManager] Map '{updatedMap.mapId}' not found for update");
        }
    }

    public void DeleteMap(string mapId)
    {
        Debug.LogWarning("[MapDataManager] DeleteMap: Runtime map modifications are not supported when loading from Resources");
    }

    // Force save (for editor)
    public void ForceSave()
    {
        if (mapCollection != null)
        {
            SaveMapsToFile();
        }
    }

//#if UNITY_EDITOR
//    [UnityEditor.MenuItem("Tools/Map System/Force Save Maps")]
//    static void ForceSaveMaps()
//    {
//        if (Instance != null)
//        {
//            Instance.ForceSave();
//        }
//        else
//        {
//            Debug.LogWarning("MapDataManager instance not found");
//        }
//    }
//#endif
}