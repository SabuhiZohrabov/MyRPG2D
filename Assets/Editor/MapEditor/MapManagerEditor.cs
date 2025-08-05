#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MapManager))]
public class MapManagerEditor : Editor
{
    private MapManager mapManager;
    private int selectedMapIndex = 0;
    private TileType selectedTileType = TileType.Grass;
    private bool isWalkable = true;
    private bool showGrid = true;
    private Vector2 scrollPosition;

    private readonly Color[] tileColors = new Color[]
    {
        Color.green,      // Grass
        new Color(0, 0.5f, 0), // Forest
        Color.gray,       // Mountain  
        Color.blue,       // Water
        Color.yellow,     // Desert
        new Color(0.3f, 0.2f, 0.1f), // Cave
        Color.red,        // Town
        Color.black       // Blocked
    };

    void OnEnable()
    {
        mapManager = (MapManager)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Visual Map Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawMapSelection();
        EditorGUILayout.Space();

        if (mapManager.maps != null && mapManager.maps.Count > 0 && selectedMapIndex < mapManager.maps.Count)
        {
            DrawMapSettings();
            EditorGUILayout.Space();

            DrawTileEditor();
            EditorGUILayout.Space();

            DrawVisualGrid();
        }
        else
        {
            DrawCreateFirstMap();
        }

        EditorGUILayout.Space();
        DrawDefaultSettings();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawMapSelection()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Maps:", GUILayout.Width(50));

        if (mapManager.maps != null && mapManager.maps.Count > 0)
        {
            string[] mapNames = new string[mapManager.maps.Count];
            for (int i = 0; i < mapManager.maps.Count; i++)
            {
                mapNames[i] = string.IsNullOrEmpty(mapManager.maps[i].mapName)
                    ? $"Map {i}"
                    : mapManager.maps[i].mapName;
            }

            selectedMapIndex = EditorGUILayout.Popup(selectedMapIndex, mapNames);
            selectedMapIndex = Mathf.Clamp(selectedMapIndex, 0, mapManager.maps.Count - 1);
        }

        if (GUILayout.Button("New Map", GUILayout.Width(80)))
        {
            CreateNewMap();
        }

        if (mapManager.maps != null && mapManager.maps.Count > 0)
        {
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                DeleteMap(selectedMapIndex);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    void DrawMapSettings()
    {
        var map = mapManager.maps[selectedMapIndex];

        EditorGUILayout.LabelField($"Map Settings: {map.mapName}", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        map.mapId = EditorGUILayout.TextField("Map ID:", map.mapId);
        map.mapName = EditorGUILayout.TextField("Map Name:", map.mapName);

        //EditorGUILayout.BeginHorizontal();
        int newWidth = EditorGUILayout.IntField("Width:", map.width);
        int newHeight = EditorGUILayout.IntField("Height:", map.height);
        //EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            if (newWidth != map.width || newHeight != map.height)
            {
                if (EditorUtility.DisplayDialog("Resize Map",
                    $"Resize map from {map.width}x{map.height} to {newWidth}x{newHeight}?\nThis may lose tile data.",
                    "Resize", "Cancel"))
                {
                    mapManager.ResizeMap(selectedMapIndex, newWidth, newHeight);
                }
            }
            EditorUtility.SetDirty(mapManager);
        }
    }

    void DrawTileEditor()
    {
        EditorGUILayout.LabelField("Tile Editor", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Brush:", GUILayout.Width(50));
        selectedTileType = (TileType)EditorGUILayout.EnumPopup(selectedTileType, GUILayout.Width(100));
        isWalkable = EditorGUILayout.Toggle("Walkable", isWalkable, GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();

        // Color legend
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Colors:", GUILayout.Width(50));
        for (int i = 0; i < tileColors.Length; i++)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = tileColors[i];
            EditorGUILayout.LabelField(((TileType)i).ToString().Substring(0, 1),
                EditorStyles.miniButton, GUILayout.Width(25));
            GUI.backgroundColor = oldColor;
        }
        EditorGUILayout.EndHorizontal();

        showGrid = EditorGUILayout.Toggle("Show Grid", showGrid);
    }

    void DrawVisualGrid()
    {
        var map = mapManager.maps[selectedMapIndex];
        if (map == null || map.width <= 0 || map.height <= 0) return;

        EditorGUILayout.LabelField("Visual Grid Editor", EditorStyles.boldLabel);

        // Initialize tiles if null
        if (map.tiles == null || map.tiles.Length != map.width * map.height)
        {
            mapManager.ResizeMap(selectedMapIndex, map.width, map.height);
        }

        float cellSize = 25f;
        float gridWidth = map.width * cellSize;
        float gridHeight = map.height * cellSize;

        // Scroll area for large maps
        Rect scrollRect = GUILayoutUtility.GetRect(gridWidth + 20, Mathf.Min(gridHeight + 20, 400));
        scrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition,
            new Rect(0, 0, gridWidth, gridHeight));

        // Draw grid
        for (int y = map.height - 1; y >= 0; y--) // Top to bottom
        {
            for (int x = 0; x < map.width; x++)
            {
                Rect cellRect = new Rect(x * cellSize, (map.height - 1 - y) * cellSize, cellSize, cellSize);

                int tileIndex = y * map.width + x;
                MapTile tile = map.tiles[tileIndex];

                if (tile == null)
                {
                    tile = new MapTile
                    {
                        gridPosition = new Vector2Int(x, y),
                        tileType = TileType.Grass,
                        isWalkable = true
                    };
                    map.tiles[tileIndex] = tile;
                }

                // Draw tile
                Color tileColor = tileColors[(int)tile.tileType];
                if (!tile.isWalkable)
                {
                    tileColor = Color.Lerp(tileColor, Color.black, 0.3f);
                }

                EditorGUI.DrawRect(cellRect, tileColor);

                // Grid lines
                if (showGrid)
                {
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 1), Color.black);
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 1, cellRect.height), Color.black);
                }

                // Handle clicks
                if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                {
                    mapManager.SetTile(selectedMapIndex, new Vector2Int(x, y), selectedTileType, isWalkable);
                    EditorUtility.SetDirty(mapManager);
                    Event.current.Use();
                }

                // Show coordinates on hover
                if (cellRect.Contains(Event.current.mousePosition))
                {
                    GUI.Label(cellRect, $"{x},{y}", EditorStyles.miniLabel);
                }
            }
        }

        GUI.EndScrollView();

        EditorGUILayout.HelpBox($"Click tiles to paint with {selectedTileType} (Walkable: {isWalkable})",
            MessageType.Info);
    }

    void DrawCreateFirstMap()
    {
        EditorGUILayout.HelpBox("No maps found. Create your first map!", MessageType.Info);
        if (GUILayout.Button("Create First Map", GUILayout.Height(30)))
        {
            CreateNewMap();
        }
    }

    void DrawDefaultSettings()
    {
        EditorGUILayout.LabelField("Default Settings", EditorStyles.boldLabel);
        DrawDefaultInspector();
    }

    void CreateNewMap()
    {
        if (mapManager.maps == null)
        {
            mapManager.maps = new List<MapData>();
        }

        var newMap = new MapData
        {
            mapId = $"map_{mapManager.maps.Count + 1}",
            mapName = $"Map {mapManager.maps.Count + 1}",
            width = 8,
            height = 6,
        };

        mapManager.maps.Add(newMap);
        mapManager.ResizeMap(mapManager.maps.Count - 1, newMap.width, newMap.height);
        selectedMapIndex = mapManager.maps.Count - 1;

        EditorUtility.SetDirty(mapManager);
    }

    void DeleteMap(int index)
    {
        if (index >= 0 && index < mapManager.maps.Count)
        {
            if (EditorUtility.DisplayDialog("Delete Map",
                $"Delete map '{mapManager.maps[index].mapName}'?", "Delete", "Cancel"))
            {
                mapManager.maps.RemoveAt(index);
                selectedMapIndex = Mathf.Clamp(selectedMapIndex, 0, mapManager.maps.Count - 1);
                EditorUtility.SetDirty(mapManager);
            }
        }
    }
}
#endif