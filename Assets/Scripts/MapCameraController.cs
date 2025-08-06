using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    public static MapCameraController Instance { get; private set; }

    [Header("Camera Settings")]
    [SerializeField] private Camera mapCamera;
    //[SerializeField] private float followSpeed = 2f;
    [SerializeField] private float smoothTime = 0.3f;

    [Header("View Settings")]
    [SerializeField] private float viewportSize = 8f; // Orthosize
    [SerializeField] private Vector2 mapBounds = new Vector2(12, 8); // Map limits

    [Header("Pan Settings")]
    [SerializeField] private bool allowManualPan = false; // Disable input for now
    [SerializeField] private float panSpeed = 5f;

    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private bool isFollowingPlayer = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Get main camera if not assigned
            if (mapCamera == null)
            {
                mapCamera = Camera.main;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (mapCamera != null)
        {
            mapCamera.orthographic = true;
            mapCamera.orthographicSize = viewportSize;
        }

        // Set initial position
        CenterOnCurrentPlayer();
    }

    void Update()
    {
        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        if (!allowManualPan) return;

        bool panInput = false;
        Vector3 panDirection = Vector3.zero;

        // Manual pan controls - Input System compatible
#if ENABLE_INPUT_SYSTEM
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) { panDirection += Vector3.up; panInput = true; }
            if (keyboard.sKey.isPressed) { panDirection += Vector3.down; panInput = true; }
            if (keyboard.aKey.isPressed) { panDirection += Vector3.left; panInput = true; }
            if (keyboard.dKey.isPressed) { panDirection += Vector3.right; panInput = true; }
        }
#else
        // Legacy Input System
        if (Input.GetKey(KeyCode.W)) { panDirection += Vector3.up; panInput = true; }
        if (Input.GetKey(KeyCode.S)) { panDirection += Vector3.down; panInput = true; }
        if (Input.GetKey(KeyCode.A)) { panDirection += Vector3.left; panInput = true; }
        if (Input.GetKey(KeyCode.D)) { panDirection += Vector3.right; panInput = true; }
#endif

        if (panInput)
        {
            isFollowingPlayer = false;
            Vector3 moveAmount = panDirection.normalized * panSpeed * Time.deltaTime;
            targetPosition = transform.position + moveAmount;
            ClampToMapBounds();
        }

        // Reset to follow player on space
#if ENABLE_INPUT_SYSTEM
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.Space))
#endif
        {
            CenterOnCurrentPlayer();
        }
    }

    void UpdateCameraPosition()
    {
        if (mapCamera == null) return;

        Vector3 currentPos = mapCamera.transform.position;
        targetPosition.z = currentPos.z; // Maintain Z position

        // Smooth movement towards target
        if (Vector3.Distance(currentPos, targetPosition) > 0.01f)
        {
            mapCamera.transform.position = Vector3.SmoothDamp(currentPos, targetPosition, ref velocity, smoothTime);
        }
    }

    public void OnPlayerMoved(Vector3 playerWorldPosition)
    {
        if (!isFollowingPlayer) return;

        targetPosition = playerWorldPosition;
        targetPosition.z = mapCamera.transform.position.z; // Keep camera Z
        ClampToMapBounds();
    }

    public void CenterOnCurrentPlayer()
    {
        if (PlayerMapVisualizer.Instance != null && MapManager.Instance != null)
        {
            Vector2Int playerGrid = PlayerMapVisualizer.Instance.GetCurrentGridPosition();
            Vector3 playerWorld = MapManager.Instance.GridToWorld(playerGrid);

            targetPosition = playerWorld;
            targetPosition.z = mapCamera.transform.position.z;
            ClampToMapBounds();

            isFollowingPlayer = true;
        }
    }

    void ClampToMapBounds()
    {
        // Clamp camera position within map bounds
        float halfWidth = mapBounds.x * 0.5f;
        float halfHeight = mapBounds.y * 0.5f;

        targetPosition.x = Mathf.Clamp(targetPosition.x, -halfWidth, halfWidth);
        targetPosition.y = Mathf.Clamp(targetPosition.y, -halfHeight, halfHeight);
    }

    public void SetMapBounds(Vector2 newBounds)
    {
        mapBounds = newBounds;
        ClampToMapBounds();
    }

    public void SetFollowMode(bool followPlayer)
    {
        isFollowingPlayer = followPlayer;
        if (followPlayer)
        {
            CenterOnCurrentPlayer();
        }
    }

    public void SetCameraSize(float newSize)
    {
        viewportSize = newSize;
        if (mapCamera != null)
        {
            mapCamera.orthographicSize = viewportSize;
        }
    }

    // Jump camera immediately to position (no smooth movement)
    public void JumpToPosition(Vector3 worldPosition)
    {
        worldPosition.z = mapCamera.transform.position.z;
        targetPosition = worldPosition;
        ClampToMapBounds();

        if (mapCamera != null)
        {
            mapCamera.transform.position = targetPosition;
        }

        velocity = Vector3.zero; // Reset velocity for smooth movement
    }

    // Debug method
    [ContextMenu("Center on Player")]
    public void DebugCenterOnPlayer()
    {
        CenterOnCurrentPlayer();
    }

    public bool IsFollowingPlayer()
    {
        return isFollowingPlayer;
    }

    public Vector3 GetTargetPosition()
    {
        return targetPosition;
    }
}