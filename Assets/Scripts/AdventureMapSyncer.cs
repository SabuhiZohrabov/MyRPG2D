using UnityEngine;
using System.Collections;

public class AdventureMapSyncer : MonoBehaviour
{
    public static AdventureMapSyncer Instance { get; private set; }

    [Header("Sync Settings")]
    [SerializeField] private float teleportAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve teleportCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private string currentAdventureId;
    private bool isAnimating = false;

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
        // Subscribe to adventure events - COMMENTED OUT TO PREVENT LOOP
        // if (AdventureManager.Instance != null)
        // {
        //     AdventureManager.Instance.OnAdventureChanged += OnAdventureChanged;
        // }
    }

    public void OnAdventureLinkSelected(string linkId)
    {
        if (isAnimating) return;

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
        if (isAnimating) return; // Prevent multiple calls during animation

        string targetMapId = adventureData.mapId;
        Vector2Int targetPosition = adventureData.mapPosition;

        // Check if player is already at this position
        if (PlayerMapVisualizer.Instance != null)
        {
            var currentPos = PlayerMapVisualizer.Instance.GetCurrentGridPosition();
            var currentMapId = PlayerMapVisualizer.Instance.GetCurrentMapId();

            if (currentMapId == targetMapId && currentPos == targetPosition)
            {
                Debug.Log($"[AdventureMapSyncer] Player already at {targetMapId}:{targetPosition}");
                return; // Already at target position
            }
        }

        Debug.Log($"[AdventureMapSyncer] Moving player to {targetMapId} at {targetPosition}");

        // Switch map if needed
        if (MapManager.Instance != null)
        {
            var currentMap = MapManager.Instance.GetMapById(MapManager.Instance.currentMapId);
            if (currentMap == null || currentMap.mapId != targetMapId)
            {
                MapManager.Instance.LoadMap(targetMapId);
            }
        }

        // Move player visually
        if (PlayerMapVisualizer.Instance != null)
        {
            StartCoroutine(AnimatePlayerToPosition(targetMapId, targetPosition));
        }

        currentAdventureId = adventureData.id;
    }

    private IEnumerator AnimatePlayerToPosition(string mapId, Vector2Int targetPosition)
    {
        isAnimating = true;

        var visualizer = PlayerMapVisualizer.Instance;
        if (visualizer != null)
        {
            Vector2Int currentPos = visualizer.GetCurrentGridPosition();

            // Animate movement
            //float elapsed = 0f;
            //while (elapsed < teleportAnimationDuration)
            //{
            //    elapsed += Time.deltaTime;
            //    float t = teleportCurve.Evaluate(elapsed / teleportAnimationDuration);

            //    Vector2 lerpedPos = Vector2.Lerp(currentPos, targetPosition, t);
            //    Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(lerpedPos.x), Mathf.RoundToInt(lerpedPos.y));

            //    visualizer.ShowPlayerAt(mapId, gridPos, false); // No animation, we're handling it
            //    yield return null;
            //}

            // Ensure final position is exact
            visualizer.ShowPlayerAt(mapId, targetPosition, false);
            yield return null;
        }

        isAnimating = false;
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

    public bool IsAnimating()
    {
        return isAnimating;
    }

    void OnDestroy()
    {
        if (AdventureManager.Instance != null)
        {
            AdventureManager.Instance.OnAdventureChanged -= OnAdventureChanged;
        }
    }
}