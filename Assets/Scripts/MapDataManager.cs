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

    [Header("File Settings")]
    private string mapsFileName = StoryManager.SelectedStoryId + "Gamemaps.json";
    [SerializeField] private bool useStreamingAssets = true; // Development
    [SerializeField] private bool usePersistentData = false; // Runtime saves

    private MapDataCollection mapCollection;
    private string filePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFilePath();
            LoadMapsFromFile();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeFilePath()
    {
        if (useStreamingAssets)
        {
            // Development phase - StreamingAssets folder
            filePath = Path.Combine(Application.streamingAssetsPath, mapsFileName);
        }
        else if (usePersistentData)
        {
            // Runtime saves - PersistentDataPath
            filePath = Path.Combine(Application.persistentDataPath, mapsFileName);
        }
        else
        {
            // Default - StreamingAssets
            filePath = Path.Combine(Application.streamingAssetsPath, mapsFileName);
        }

        Debug.Log($"[MapDataManager] File path: {filePath}");
    }

    public void LoadMapsFromFile()
    {
        try
        {
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                mapCollection = JsonUtility.FromJson<MapDataCollection>(jsonContent);

                if (mapCollection == null)
                {
                    mapCollection = new MapDataCollection();
                }

                Debug.Log($"[MapDataManager] Loaded {mapCollection.maps.Count} maps from file");
            }
            else
            {
                Debug.Log($"[MapDataManager] Map file not found, creating default collection");
                mapCollection = new MapDataCollection();
                CreateDefaultMaps();
                SaveMapsToFile();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MapDataManager] Error loading maps: {e.Message}");
            mapCollection = new MapDataCollection();
            CreateDefaultMaps();
        }
    }

    public void SaveMapsToFile()
    {
        try
        {
            // Ensure directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string jsonContent = JsonUtility.ToJson(mapCollection, true);
            File.WriteAllText(filePath, jsonContent);

            Debug.Log($"[MapDataManager] Saved {mapCollection.maps.Count} maps to file");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MapDataManager] Error saving maps: {e.Message}");
        }
    }

    void CreateDefaultMaps()
    {
        // Create main world map
        var mainWorld = new MapData
        {
            mapId = "main_world",
            mapName = "Main World",
            width = 12,
            height = 8
        };

        CreateDefaultTiles(mainWorld);
        mapCollection.maps.Add(mainWorld);

        //// Create underground map
        //var underground = new MapData
        //{
        //    mapId = "underground",
        //    mapName = "Underground",
        //    width = 10,
        //    height = 6
        //};

        //CreateDefaultTiles(underground, TileType.Cave);
        //mapCollection.maps.Add(underground);
    }

    void CreateDefaultTiles(MapData map, TileType defaultType = TileType.Grass)
    {
        map.tiles = new MapTile[map.width * map.height];

        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                int index = y * map.width + x;

                // Create borders as blocked
                bool isBorder = (x == 0 || x == map.width - 1 || y == 0 || y == map.height - 1);
                TileType tileType = isBorder ? TileType.Mountain : defaultType;
                bool walkable = !isBorder;

                // Add some variety
                if (!isBorder && Random.value < 0.1f)
                {
                    tileType = (TileType)Random.Range(0, (int)TileType.Blocked);
                    walkable = tileType != TileType.Water && tileType != TileType.Mountain;
                }

                map.tiles[index] = new MapTile
                {
                    gridPosition = new Vector2Int(x, y),
                    tileType = tileType,
                    isWalkable = walkable
                };
            }
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
        if (mapCollection == null)
        {
            mapCollection = new MapDataCollection();
        }

        // Check for duplicate IDs
        if (mapCollection.maps.Any(m => m.mapId == newMap.mapId))
        {
            Debug.LogWarning($"[MapDataManager] Map with ID '{newMap.mapId}' already exists!");
            return;
        }

        mapCollection.maps.Add(newMap);
        SaveMapsToFile();
    }

    public void UpdateMap(MapData updatedMap)
    {
        if (mapCollection?.maps == null) return;

        int index = mapCollection.maps.FindIndex(m => m.mapId == updatedMap.mapId);
        if (index >= 0)
        {
            mapCollection.maps[index] = updatedMap;
            SaveMapsToFile();
        }
        else
        {
            Debug.LogWarning($"[MapDataManager] Map '{updatedMap.mapId}' not found for update");
        }
    }

    public void DeleteMap(string mapId)
    {
        if (mapCollection?.maps == null) return;

        int removed = mapCollection.maps.RemoveAll(m => m.mapId == mapId);
        if (removed > 0)
        {
            SaveMapsToFile();
            Debug.Log($"[MapDataManager] Deleted map '{mapId}'");
        }
    }

    // Force save (for editor)
    public void ForceSave()
    {
        if (mapCollection != null)
        {
            SaveMapsToFile();
        }
    }


    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveMapsToFile();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveMapsToFile();
        }
    }

    void OnDestroy()
    {
        SaveMapsToFile();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Map System/Force Save Maps")]
    static void ForceSaveMaps()
    {
        if (Instance != null)
        {
            Instance.ForceSave();
        }
        else
        {
            Debug.LogWarning("MapDataManager instance not found");
        }
    }
#endif
}