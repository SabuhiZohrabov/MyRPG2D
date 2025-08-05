using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MapTile
{
    public Vector2Int gridPosition;     // (x, y) koordinat
    public TileType tileType;          // Forest, Mountain, Water
    public Sprite tileSprite;
    public bool isWalkable;
}

[System.Serializable]
public class MapData
{
    public string mapId;               // "main_world", "underground"
    public string mapName;             // "Main worls", "Undergroud"
    public int width;
    public int height;
    public MapTile[] tiles;            // Tiles
}

public enum TileType
{
    Grass, Forest, Mountain, Water, Desert, Cave, Town, Blocked
}
