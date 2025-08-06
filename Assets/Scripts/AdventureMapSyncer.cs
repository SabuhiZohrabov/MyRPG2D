using UnityEngine;

public class AdventureMapSyncer : MonoBehaviour
{
    public static AdventureMapSyncer Instance { get; private set; }

    private string currentAdventureId;

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

    public void OnAdventureLinkSelected(string linkId)
    {
        // Find the adventure data for this link
        var adventureData = GetAdventureById(linkId);
        if (adventureData == null)
        {
            Debug.LogWarning($"[AdventureMapSyncer] Adventure not found: {linkId}");
            return;
        }

        // Check if adventure has map data
        if (string.IsNullOrEmpty(adventureData.mapId))
        {
            Debug.LogWarning($"[AdventureMapSyncer] Adventure '{linkId}' has no map data");
            return;
        }

        MovePlayerToAdventure(adventureData);
    }

    public void MovePlayerToAdventure(AdventureTextData adventureData)
    {
        if (adventureData == null || string.IsNullOrEmpty(adventureData.mapId)) return;

        string targetMapId = adventureData.mapId;
        Vector2Int targetPosition = adventureData.mapPosition;

        Debug.Log($"[AdventureMapSyncer] Moving player to {targetMapId} at {targetPosition}");

        // Switch map if needed
        if (MapManager.Instance != null && MapDataManager.Instance != null)
        {
            var currentMap = MapDataManager.Instance.GetMapById(MapManager.Instance.currentMapId);
            if (currentMap == null || currentMap.mapId != targetMapId)
            {
                MapManager.Instance.LoadMap(targetMapId);
            }
        }

        // Move player visually (no animation)
        if (PlayerMapVisualizer.Instance != null)
        {
            PlayerMapVisualizer.Instance.ShowPlayerAt(targetMapId, targetPosition);
        }

        currentAdventureId = adventureData.id;
    }

    public AdventureTextData GetAdventureMapData(string adventureId)
    {
        return GetAdventureById(adventureId);
    }

    private AdventureTextData GetAdventureById(string adventureId)
    {
        // Get adventure data from AdventureManager
        if (AdventureManager.Instance != null)
        {
            return AdventureManager.Instance.GetAdventureById(adventureId);
        }
        return null;
    }

    public void UpdateMapPosition()
    {
        if (string.IsNullOrEmpty(currentAdventureId)) return;

        var adventureData = GetAdventureById(currentAdventureId);
        if (adventureData != null && !string.IsNullOrEmpty(adventureData.mapId))
        {
            if (PlayerMapVisualizer.Instance != null)
            {
                PlayerMapVisualizer.Instance.ShowPlayerAt(adventureData.mapId, adventureData.mapPosition);
            }
        }
    }

    private void OnAdventureChanged(string newAdventureId)
    {
        // Only update if adventure actually changed
        if (currentAdventureId == newAdventureId) return;

        currentAdventureId = newAdventureId;

        // Don't auto-move player here - let OnTextLinkClicked handle movement
        // UpdateMapPosition(); // Remove this line to prevent loop

        Debug.Log($"[AdventureMapSyncer] Adventure changed to: {newAdventureId}");
    }

    public string GetCurrentAdventureId()
    {
        return currentAdventureId;
    }

    void OnDestroy()
    {
        if (AdventureManager.Instance != null)
        {
            AdventureManager.Instance.OnAdventureChanged -= OnAdventureChanged;
        }
    }
}