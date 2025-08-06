using UnityEngine;

public class PlayerMapVisualizer : MonoBehaviour
{
    public static PlayerMapVisualizer Instance { get; private set; }

    [Header("Player Visual Settings")]
    [SerializeField] private GameObject playerIconPrefab;
    [SerializeField] private Transform playerIconParent;
    [SerializeField] private float iconScale = 0.8f;

    private GameObject currentPlayerIcon;
    private Vector2Int currentGridPosition;
    private string currentMapId;

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

    public void ShowPlayerAt(string mapId, Vector2Int gridPosition)
    {
        currentMapId = mapId;
        currentGridPosition = gridPosition;

        // Create player icon if doesn't exist
        if (currentPlayerIcon == null)
        {
            CreatePlayerIcon();
        }

        // Make sure icon is active
        if (currentPlayerIcon != null)
        {
            currentPlayerIcon.SetActive(true);
        }

        // Position the icon
        if (MapManager.Instance != null)
        {
            Vector3 worldPosition = MapManager.Instance.GridToWorld(gridPosition);
            worldPosition.z = -1f; // Always on top

            if (currentPlayerIcon != null)
            {
                currentPlayerIcon.transform.position = worldPosition;
            }

            // Notify camera to follow player
            if (MapCameraController.Instance != null)
            {
                MapCameraController.Instance.OnPlayerMoved(worldPosition);
            }
        }

        Debug.Log($"[PlayerMapVisualizer] Player positioned at {mapId}:{gridPosition}");
    }

    private void CreatePlayerIcon()
    {
        if (currentPlayerIcon != null)
        {
            Destroy(currentPlayerIcon);
        }

        if (playerIconPrefab != null)
        {
            // Use provided prefab
            Transform parent = playerIconParent != null ? playerIconParent : transform;
            currentPlayerIcon = Instantiate(playerIconPrefab, parent);
        }
        else
        {
            // Create simple colored square as fallback
            currentPlayerIcon = new GameObject("PlayerIcon");
            Transform parent = playerIconParent != null ? playerIconParent : transform;
            currentPlayerIcon.transform.SetParent(parent);

            // Add visual component
            var spriteRenderer = currentPlayerIcon.AddComponent<SpriteRenderer>();

            // Create simple colored texture
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.blue);
            texture.Apply();

            // Create sprite from texture
            Sprite playerSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            spriteRenderer.sprite = playerSprite;
            spriteRenderer.sortingOrder = 10; // Always on top
        }

        // Apply scale
        if (currentPlayerIcon != null)
        {
            currentPlayerIcon.transform.localScale = Vector3.one * iconScale;
        }
    }

    public void HidePlayer()
    {
        if (currentPlayerIcon != null)
        {
            currentPlayerIcon.SetActive(false);
        }
    }

    public Vector2Int GetCurrentGridPosition()
    {
        return currentGridPosition;
    }

    public bool IsPlayerVisible()
    {
        return currentPlayerIcon != null && currentPlayerIcon.activeInHierarchy;
    }

    void OnDestroy()
    {
        if (currentPlayerIcon != null)
        {
            Destroy(currentPlayerIcon);
        }
    }
}