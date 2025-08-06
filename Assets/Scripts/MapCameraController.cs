using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    public static MapCameraController Instance { get; private set; }

    [Header("Camera Settings")]
    [SerializeField] private Camera mapCamera;
    [SerializeField] private float smoothTime = 0.3f;

    [Header("View Settings")]
    [SerializeField] private float viewportSize = 8f; // Orthosize
    [SerializeField] private Vector2 mapBounds = new Vector2(12, 8); // Map limits

    [Header("Input Settings")]
    [SerializeField] private bool allowManualPan = false; // Disable input for now
    [SerializeField] private float panSpeed = 1f;
    [SerializeField] private GameObject mapContainer;

    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;

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
        if (allowManualPan && Input.GetMouseButton(0) && mapContainer.activeSelf)
        {
            Vector3 mouseDelta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0) * panSpeed;
            targetPosition -= mouseDelta;
            ClampToMapBounds();
        }
        // Optionally handle zoom or other inputs here
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

    public Vector3 GetTargetPosition()
    {
        return targetPosition;
    }
}