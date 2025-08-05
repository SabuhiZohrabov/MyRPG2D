using UnityEngine;
using System.Collections;

public class MapCameraController : MonoBehaviour
{
    public static MapCameraController Instance { get; private set; }

    [Header("Camera Settings")]
    [SerializeField] private Camera mapCamera;
    [SerializeField] private float followSpeed = 2f;
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
    private bool isAnimating = false;

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
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed)
        {
            panDirection += Vector3.up;
            panInput = true;
        }
        if (keyboard.sKey.isPressed)
        {
            panDirection += Vector3.down;
            panInput = true;
        }
        if (keyboard.aKey.isPressed)
        {
            panDirection += Vector3.left;
            panInput = true;
        }
        if (keyboard.dKey.isPressed)
        {
            panDirection += Vector3.right;
            panInput = true;
        }

        // Apply manual pan
        if (panInput)
        {
            isFollowingPlayer = false;
            Vector3 panAmount = panDirection.normalized * panSpeed * Time.deltaTime;
            targetPosition = ClampToBounds(transform.position + panAmount);
        }

        // Reset to player
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            CenterOnCurrentPlayer();
        }
#else
        // Legacy Input System
        if (Input.GetKey(panUpKey))
        {
            panDirection += Vector3.up;
            panInput = true;
        }
        if (Input.GetKey(panDownKey))
        {
            panDirection += Vector3.down;
            panInput = true;
        }
        if (Input.GetKey(panLeftKey))
        {
            panDirection += Vector3.left;
            panInput = true;
        }
        if (Input.GetKey(panRightKey))
        {
            panDirection += Vector3.right;
            panInput = true;
        }

        // Apply manual pan
        if (panInput)
        {
            isFollowingPlayer = false;
            Vector3 panAmount = panDirection.normalized * panSpeed * Time.deltaTime;
            targetPosition = ClampToBounds(transform.position + panAmount);
        }

        // Reset to player
        if (Input.GetKeyDown(resetViewKey))
        {
            CenterOnCurrentPlayer();
        }
#endif
    }

    void UpdateCameraPosition()
    {
        if (isAnimating) return;

        Vector3 currentPos = transform.position;
        Vector3 newPosition = Vector3.SmoothDamp(currentPos, targetPosition, ref velocity, smoothTime);

        transform.position = ClampToBounds(newPosition);
    }

    public void FollowPlayer(Vector3 playerWorldPosition, bool animate = true)
    {
        Vector3 cameraPosition = new Vector3(playerWorldPosition.x, playerWorldPosition.y, transform.position.z);
        targetPosition = ClampToBounds(cameraPosition);

        isFollowingPlayer = true;

        if (animate && !isAnimating)
        {
            StartCoroutine(AnimateToPosition(targetPosition));
        }
    }

    public void CenterOnCurrentPlayer()
    {
        if (PlayerMapVisualizer.Instance != null)
        {
            var playerPos = PlayerMapVisualizer.Instance.GetCurrentGridPosition();
            if (MapManager.Instance != null)
            {
                Vector3 worldPos = MapManager.Instance.GridToWorld(playerPos);
                FollowPlayer(worldPos, true);
            }
        }
    }

    IEnumerator AnimateToPosition(Vector3 target)
    {
        isAnimating = true;

        Vector3 startPos = transform.position;
        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            Vector3 currentPos = Vector3.Lerp(startPos, target, t);
            transform.position = ClampToBounds(currentPos);

            yield return null;
        }

        transform.position = ClampToBounds(target);
        isAnimating = false;
    }

    Vector3 ClampToBounds(Vector3 position)
    {
        // Calculate camera bounds based on orthographic size
        float halfHeight = mapCamera.orthographicSize;
        float halfWidth = halfHeight * mapCamera.aspect;

        // Clamp to map bounds
        float clampedX = Mathf.Clamp(position.x, halfWidth, mapBounds.x - halfWidth);
        float clampedY = Mathf.Clamp(position.y, halfHeight, mapBounds.y - halfHeight);

        return new Vector3(clampedX, clampedY, position.z);
    }

    public void SetMapBounds(Vector2 newBounds)
    {
        mapBounds = newBounds;
    }

    public void SetViewportSize(float size)
    {
        viewportSize = size;
        if (mapCamera != null)
        {
            mapCamera.orthographicSize = viewportSize;
        }
    }

    public void SetFollowSpeed(float speed)
    {
        followSpeed = speed;
        smoothTime = 1f / speed;
    }

    public bool IsFollowingPlayer()
    {
        return isFollowingPlayer;
    }

    // Call this when player moves
    public void OnPlayerMoved(Vector3 playerWorldPosition)
    {
        if (isFollowingPlayer)
        {
            FollowPlayer(playerWorldPosition, true);
        }
    }

    void OnDrawGizmos()
    {
        // Draw camera bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(mapBounds.x / 2, mapBounds.y / 2, 0),
            new Vector3(mapBounds.x, mapBounds.y, 0));

        // Draw viewport
        if (mapCamera != null)
        {
            Gizmos.color = Color.green;
            float halfHeight = mapCamera.orthographicSize;
            float halfWidth = halfHeight * mapCamera.aspect;
            Gizmos.DrawWireCube(transform.position, new Vector3(halfWidth * 2, halfHeight * 2, 0));
        }
    }
}