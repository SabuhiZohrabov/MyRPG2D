using UnityEngine;
using System.Collections;

public class PlayerMapVisualizer : MonoBehaviour
{
    public static PlayerMapVisualizer Instance { get; private set; }

    [Header("Player Visual Settings")]
    [SerializeField] private GameObject playerIconPrefab;
    [SerializeField] private Transform playerIconParent;
    [SerializeField] private float iconScale = 0.8f;
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float bounceSpeed = 2f;

    [Header("Animation Settings")]
    [SerializeField] private float moveAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private GameObject currentPlayerIcon;
    private Vector2Int currentGridPosition;
    private string currentMapId;
    private Coroutine bounceCoroutine;
    private Coroutine moveCoroutine;

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
        // Create initial player icon
        CreatePlayerIcon();
    }

    public void ShowPlayerAt(string mapId, Vector2Int gridPosition, bool animate = true)
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

            if (animate && currentPlayerIcon != null)
            {
                StartCoroutine(AnimateToPosition(worldPosition));
            }
            else if (currentPlayerIcon != null)
            {
                currentPlayerIcon.transform.position = worldPosition;
                StartBounceAnimation();
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
            // Create default player icon
            currentPlayerIcon = new GameObject("PlayerIcon");
            currentPlayerIcon.transform.SetParent(playerIconParent != null ? playerIconParent : transform);

            var spriteRenderer = currentPlayerIcon.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateDefaultPlayerSprite();
            spriteRenderer.sortingOrder = 10;
            spriteRenderer.sortingLayerName = "UI";
        }

        if (currentPlayerIcon != null)
        {
            currentPlayerIcon.transform.localScale = Vector3.one * iconScale;
        }

        Debug.Log("[PlayerMapVisualizer] Player icon created");
    }

    private Sprite CreateDefaultPlayerSprite()
    {
        // Create a simple colored circle as default player icon
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];

        Vector2 center = new Vector2(16, 16);
        float radius = 12f;

        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);

                if (distance <= radius)
                {
                    // Create a gradient circle
                    float normalizedDistance = distance / radius;
                    Color circleColor = Color.Lerp(Color.yellow, Color.red, normalizedDistance);
                    colors[y * 32 + x] = circleColor;
                }
                else
                {
                    colors[y * 32 + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }

    private IEnumerator AnimateToPosition(Vector3 targetPosition)
    {
        if (currentPlayerIcon == null) yield break;

        // Stop existing move animation
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        Vector3 startPosition = currentPlayerIcon.transform.position;
        float elapsed = 0f;

        // Stop bounce briefly during movement
        StopBounceAnimation();

        while (elapsed < moveAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = moveCurve.Evaluate(elapsed / moveAnimationDuration);

            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, t);
            currentPlayerIcon.transform.position = currentPos;

            yield return null;
        }

        currentPlayerIcon.transform.position = targetPosition;

        // Resume bounce animation
        StartBounceAnimation();

        moveCoroutine = null;
    }

    private void StartBounceAnimation()
    {
        StopBounceAnimation();

        if (currentPlayerIcon != null)
        {
            bounceCoroutine = StartCoroutine(BounceAnimation());
        }
    }

    private void StopBounceAnimation()
    {
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
            bounceCoroutine = null;
        }
    }

    private IEnumerator BounceAnimation()
    {
        if (currentPlayerIcon == null) yield break;

        Vector3 basePosition = currentPlayerIcon.transform.position;

        while (currentPlayerIcon != null && currentPlayerIcon.activeInHierarchy)
        {
            float bounce = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            Vector3 newPosition = new Vector3(basePosition.x, basePosition.y + bounce, basePosition.z);
            currentPlayerIcon.transform.position = newPosition;

            yield return null;
        }
    }

    public void UpdatePlayerAppearance(Sprite newSprite = null, Color? newColor = null)
    {
        if (currentPlayerIcon == null) return;

        var spriteRenderer = currentPlayerIcon.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (newSprite != null)
            {
                spriteRenderer.sprite = newSprite;
            }

            if (newColor.HasValue)
            {
                spriteRenderer.color = newColor.Value;
            }
        }
    }

    public void SetPlayerIconScale(float scale)
    {
        iconScale = scale;
        if (currentPlayerIcon != null)
        {
            currentPlayerIcon.transform.localScale = Vector3.one * iconScale;
        }
    }

    public void SetBounceSettings(float height, float speed)
    {
        bounceHeight = height;
        bounceSpeed = speed;
    }

    public Vector2Int GetCurrentGridPosition()
    {
        return currentGridPosition;
    }

    public string GetCurrentMapId()
    {
        return currentMapId;
    }

    public void HidePlayer()
    {
        if (currentPlayerIcon != null)
        {
            currentPlayerIcon.SetActive(false);
            StopBounceAnimation();
        }
    }

    public void ShowPlayer()
    {
        if (currentPlayerIcon != null)
        {
            currentPlayerIcon.SetActive(true);
            StartBounceAnimation();
        }
    }

    public bool IsPlayerVisible()
    {
        return currentPlayerIcon != null && currentPlayerIcon.activeInHierarchy;
    }

    //// Context menu for testing
    //[ContextMenu("Test Player Movement")]
    //void TestPlayerMovement()
    //{
    //    if (Application.isPlaying)
    //    {
    //        Vector2Int testPosition = new Vector2Int(
    //            Random.Range(0, 10),
    //            Random.Range(0, 6)
    //        );
    //        ShowPlayerAt("main_world", testPosition, true);
    //    }
    //}

    void OnDestroy()
    {
        StopBounceAnimation();
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
    }
}