using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Map Configuration")]
    [SerializeField] public string currentMapId = "main_world";

    [Header("Visual Settings")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform mapContainer;

    [Header("Default Sprites")]
    [SerializeField] private Sprite grassSprite;
    [SerializeField] private Sprite forestSprite;
    [SerializeField] private Sprite mountainSprite;
    [SerializeField] private Sprite waterSprite;
    [SerializeField] private Sprite desertSprite;
    [SerializeField] private Sprite caveSprite;
    [SerializeField] private Sprite townSprite;
    [SerializeField] private Sprite blockedSprite;

    // Runtime data - no longer serialized
    [System.NonSerialized]
    public List<MapData> maps = new List<MapData>();

    private MapData currentMap;
    private Dictionary<Vector2Int, GameObject> tileGameObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<string, MapData> mapDict = new Dictionary<string, MapData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Wait for MapDataManager to initialize
        StartCoroutine(InitializeAfterDataManager());
    }

    System.Collections.IEnumerator InitializeAfterDataManager()
    {
        // Wait for MapDataManager to load
        while (MapDataManager.Instance == null)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        LoadMapsFromDataManager();
        LoadMap(currentMapId);
    }

    void LoadMapsFromDataManager()
    {
        if (MapDataManager.Instance == null) return;

        maps = MapDataManager.Instance.GetAllMaps();
        InitializeMaps();

        Debug.Log($"[MapManager] Loaded {maps.Count} maps from JSON");
    }

    void InitializeMaps()
    {
        mapDict.Clear();
        foreach (var map in maps)
        {
            if (!string.IsNullOrEmpty(map.mapId))
            {
                mapDict[map.mapId] = map;
            }
        }
    }

    public MapData GetMapById(string mapId)
    {
        return mapDict.TryGetValue(mapId, out var map) ? map : null;
    }

    public void LoadMap(string mapId)
    {
        var map = GetMapById(mapId);
        if (map == null)
        {
            Debug.LogError($"Map not found: {mapId}");
            return;
        }

        currentMap = map;
        currentMapId = mapId;
        GenerateMapVisuals();
    }

    private void GenerateMapVisuals()
    {
        ClearMapVisuals();

        if (currentMap == null || currentMap.tiles == null) return;

        foreach (var tile in currentMap.tiles)
        {
            CreateTileGameObject(tile);
        }
    }

    private void CreateTileGameObject(MapTile tile)
    {
        if (tilePrefab == null) return;

        Vector3 worldPos = new Vector3(tile.gridPosition.x * tileSize, tile.gridPosition.y * tileSize, 0);
        GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, mapContainer);

        tileObj.name = $"Tile_{tile.gridPosition.x}_{tile.gridPosition.y}";

        var spriteRenderer = tileObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = GetSpriteForTileType(tile.tileType, tile.tileSprite);
            spriteRenderer.sortingOrder = 0;
        }

        var collider = tileObj.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = !tile.isWalkable;
        }

        tileGameObjects[tile.gridPosition] = tileObj;
    }

    private Sprite GetSpriteForTileType(TileType tileType, Sprite customSprite)
    {
        if (customSprite != null) return customSprite;

        return tileType switch
        {
            TileType.Grass => grassSprite,
            TileType.Forest => forestSprite,
            TileType.Mountain => mountainSprite,
            TileType.Water => waterSprite,
            TileType.Desert => desertSprite,
            TileType.Cave => caveSprite,
            TileType.Town => townSprite,
            TileType.Blocked => blockedSprite,
            _ => grassSprite
        };
    }

    private void ClearMapVisuals()
    {
        foreach (var tileObj in tileGameObjects.Values)
        {
            if (tileObj != null)
            {
                DestroyImmediate(tileObj);
            }
        }
        tileGameObjects.Clear();
    }

    public MapTile GetTileAt(Vector2Int position)
    {
        if (currentMap?.tiles == null) return null;
        return currentMap.tiles.FirstOrDefault(t => t.gridPosition == position);
    }

    public bool IsWalkable(Vector2Int position)
    {
        var tile = GetTileAt(position);
        return tile?.isWalkable ?? false;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / tileSize),
            Mathf.RoundToInt(worldPos.y / tileSize)
        );
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * tileSize, gridPos.y * tileSize, 0);
    }

    // JSON-based map operations
    public void CreateNewMap(string mapId, string mapName, int width, int height)
    {
        var newMap = new MapData
        {
            mapId = mapId,
            mapName = mapName,
            width = width,
            height = height,
            tiles = new MapTile[width * height]
        };

        // Initialize with default tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = y * width + x;
                newMap.tiles[index] = new MapTile
                {
                    gridPosition = new Vector2Int(x, y),
                    tileType = TileType.Grass,
                    isWalkable = true
                };
            }
        }

        // Add to JSON
        MapDataManager.Instance.AddMap(newMap);

        // Refresh local data
        LoadMapsFromDataManager();
    }

    public void ResizeMap(int mapIndex, int newWidth, int newHeight)
    {
        if (mapIndex < 0 || mapIndex >= maps.Count) return;

        var map = maps[mapIndex];
        var oldTiles = map.tiles?.ToList() ?? new List<MapTile>();

        map.width = newWidth;
        map.height = newHeight;
        map.tiles = new MapTile[newWidth * newHeight];

        // Initialize all tiles
        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                int index = y * newWidth + x;
                var gridPos = new Vector2Int(x, y);

                // Try to find existing tile
                var existingTile = oldTiles.FirstOrDefault(t => t.gridPosition == gridPos);

                if (existingTile != null)
                {
                    map.tiles[index] = existingTile;
                }
                else
                {
                    // Create new tile
                    map.tiles[index] = new MapTile
                    {
                        gridPosition = gridPos,
                        tileType = TileType.Grass,
                        isWalkable = true
                    };
                }
            }
        }

        // Save to JSON
        MapDataManager.Instance.UpdateMap(map);

        if (map.mapId == currentMapId)
        {
            GenerateMapVisuals();
        }
    }

    public void SetTile(int mapIndex, Vector2Int position, TileType tileType, bool isWalkable, Sprite customSprite = null)
    {
        if (mapIndex < 0 || mapIndex >= maps.Count) return;

        var map = maps[mapIndex];
        if (map.tiles == null) return;

        int index = position.y * map.width + position.x;
        if (index < 0 || index >= map.tiles.Length) return;

        if (map.tiles[index] == null)
        {
            map.tiles[index] = new MapTile { gridPosition = position };
        }

        map.tiles[index].tileType = tileType;
        map.tiles[index].isWalkable = isWalkable;
        if (customSprite != null)
        {
            map.tiles[index].tileSprite = customSprite;
        }

        // Save to JSON
        MapDataManager.Instance.UpdateMap(map);

        // Update visual if this is current map
        if (map.mapId == currentMapId)
        {
            if (tileGameObjects.TryGetValue(position, out var tileObj))
            {
                var spriteRenderer = tileObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = GetSpriteForTileType(tileType, customSprite);
                }

                var collider = tileObj.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = !isWalkable;
                }
            }
        }
    }

    public void DeleteMap(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= maps.Count) return;

        string mapId = maps[mapIndex].mapId;
        MapDataManager.Instance.DeleteMap(mapId);
        LoadMapsFromDataManager();
    }

    // Force save all changes
    [ContextMenu("Save All Maps")]
    public void SaveAllMaps()
    {
        if (MapDataManager.Instance != null)
        {
            MapDataManager.Instance.ForceSave();
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (currentMap == null) return;

        Gizmos.color = Color.white;

        // Draw grid lines
        for (int x = 0; x <= currentMap.width; x++)
        {
            Vector3 start = new Vector3(x * tileSize, 0, 0);
            Vector3 end = new Vector3(x * tileSize, currentMap.height * tileSize, 0);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= currentMap.height; y++)
        {
            Vector3 start = new Vector3(0, y * tileSize, 0);
            Vector3 end = new Vector3(currentMap.width * tileSize, y * tileSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }
#endif
}