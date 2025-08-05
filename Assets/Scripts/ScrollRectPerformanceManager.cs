using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ScrollRectPerformanceManager : MonoBehaviour
{
    [Header("ScrollRect Performance Settings")]
    [SerializeField] private List<ScrollRect> managedScrollRects = new List<ScrollRect>();
    [SerializeField] private float scrollThreshold = 0.01f;
    [SerializeField] private float inactiveTimeout = 2f;
    [SerializeField] private int maxUpdatesPerFrame = 1;

    private Dictionary<ScrollRect, ScrollRectData> scrollData = new Dictionary<ScrollRect, ScrollRectData>();
    private Queue<ScrollRect> updateQueue = new Queue<ScrollRect>();
    private Coroutine updateCoroutine;

    [System.Serializable]
    private class ScrollRectData
    {
        public Vector2 lastPosition;
        public float lastUpdateTime;
        public bool isScrolling;
        public bool needsUpdate;
        public ScrollRect.MovementType originalMovementType;
        public float originalElasticity;
        public float originalDecelerationRate;
    }

    void Start()
    {
        Debug.Log("=== ScrollRect Debug Start ===");

        ScrollRect[] foundScrollRects = FindObjectsByType<ScrollRect>(FindObjectsSortMode.None);
        Debug.Log($"Found {foundScrollRects.Length} ScrollRects in scene:");

        for (int i = 0; i < foundScrollRects.Length; i++)
        {
            Debug.Log($"ScrollRect {i}: {foundScrollRects[i].name} on GameObject: {foundScrollRects[i].gameObject.name}");
        }

        InitializeScrollRects();
        StartUpdateCoroutine();

        Debug.Log($"Managed ScrollRects count: {managedScrollRects.Count}");
        Debug.Log("=== ScrollRect Debug End ===");
    }

    void InitializeScrollRects()
    {
        if (managedScrollRects.Count == 0)
        {
            managedScrollRects.AddRange(FindObjectsByType<ScrollRect>(FindObjectsSortMode.None));
        }

        foreach (var scrollRect in managedScrollRects)
        {
            if (scrollRect != null)
            {
                var data = new ScrollRectData
                {
                    lastPosition = scrollRect.normalizedPosition,
                    lastUpdateTime = Time.time,
                    isScrolling = false,
                    needsUpdate = false,
                    originalMovementType = scrollRect.movementType,
                    originalElasticity = scrollRect.elasticity,
                    originalDecelerationRate = scrollRect.decelerationRate
                };

                scrollData[scrollRect] = data;
                OptimizeScrollRectSettings(scrollRect);
            }
        }
    }

    void OptimizeScrollRectSettings(ScrollRect scrollRect)
    {
        scrollRect.elasticity = 0.05f;
        scrollRect.decelerationRate = 0.2f;
        scrollRect.scrollSensitivity = 1f;

        if (scrollRect.viewport != null)
        {
            var mask = scrollRect.viewport.GetComponent<Mask>();
            if (mask != null)
            {
                mask.showMaskGraphic = false;
            }
        }
    }

    void StartUpdateCoroutine()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
        updateCoroutine = StartCoroutine(UpdateScrollRectsCoroutine());
    }

    IEnumerator UpdateScrollRectsCoroutine()
    {
        while (true)
        {
            int updatesThisFrame = 0;

            while (updateQueue.Count > 0 && updatesThisFrame < maxUpdatesPerFrame)
            {
                var scrollRect = updateQueue.Dequeue();
                if (scrollRect != null && scrollData.ContainsKey(scrollRect))
                {
                    ProcessScrollRectUpdate(scrollRect);
                    updatesThisFrame++;
                }
            }

            CheckScrollRectStates();
            yield return null;
        }
    }

    void CheckScrollRectStates()
    {
        foreach (var kvp in scrollData)
        {
            var scrollRect = kvp.Key;
            var data = kvp.Value;

            if (scrollRect == null) continue;

            Vector2 currentPos = scrollRect.normalizedPosition;
            float positionDelta = Vector2.Distance(currentPos, data.lastPosition);

            if (positionDelta > scrollThreshold)
            {
                if (!data.isScrolling)
                {
                    data.isScrolling = true;
                    OnScrollStart(scrollRect);
                }

                data.lastPosition = currentPos;
                data.lastUpdateTime = Time.time;
                data.needsUpdate = true;

                if (!updateQueue.Contains(scrollRect))
                {
                    updateQueue.Enqueue(scrollRect);
                }
            }
            else if (data.isScrolling && Time.time - data.lastUpdateTime > inactiveTimeout)
            {
                data.isScrolling = false;
                OnScrollEnd(scrollRect);
            }
        }
    }

    void ProcessScrollRectUpdate(ScrollRect scrollRect)
    {
        var data = scrollData[scrollRect];
        if (!data.needsUpdate) return;

        if (scrollRect.content != null)
        {
            var contentTransform = scrollRect.content;

            for (int i = 0; i < contentTransform.childCount; i++)
            {
                var child = contentTransform.GetChild(i);
                OptimizeChildForScrolling(child, scrollRect);
            }
        }

        data.needsUpdate = false;
    }

    void OptimizeChildForScrolling(Transform child, ScrollRect scrollRect)
    {
        var childRect = child.GetComponent<RectTransform>();
        if (childRect == null) return;

        Vector3[] corners = new Vector3[4];
        childRect.GetWorldCorners(corners);

        Vector3[] viewportCorners = new Vector3[4];
        scrollRect.viewport.GetWorldCorners(viewportCorners);

        bool isVisible = IsRectVisible(corners, viewportCorners);

        var canvasGroup = child.GetComponent<CanvasGroup>();
        if (canvasGroup == null && !isVisible)
        {
            canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = isVisible ? 1f : 0f;
            canvasGroup.interactable = isVisible;
            canvasGroup.blocksRaycasts = isVisible;
        }
    }

    bool IsRectVisible(Vector3[] childCorners, Vector3[] viewportCorners)
    {
        float childMinX = Mathf.Min(childCorners[0].x, childCorners[2].x);
        float childMaxX = Mathf.Max(childCorners[0].x, childCorners[2].x);
        float childMinY = Mathf.Min(childCorners[0].y, childCorners[2].y);
        float childMaxY = Mathf.Max(childCorners[0].y, childCorners[2].y);

        float viewportMinX = Mathf.Min(viewportCorners[0].x, viewportCorners[2].x);
        float viewportMaxX = Mathf.Max(viewportCorners[0].x, viewportCorners[2].x);
        float viewportMinY = Mathf.Min(viewportCorners[0].y, viewportCorners[2].y);
        float viewportMaxY = Mathf.Max(viewportCorners[0].y, viewportCorners[2].y);

        return childMaxX >= viewportMinX && childMinX <= viewportMaxX &&
               childMaxY >= viewportMinY && childMinY <= viewportMaxY;
    }

    void OnScrollStart(ScrollRect scrollRect)
    {
        var data = scrollData[scrollRect];
        scrollRect.movementType = data.originalMovementType;
        scrollRect.elasticity = data.originalElasticity;
        scrollRect.decelerationRate = data.originalDecelerationRate;
    }

    void OnScrollEnd(ScrollRect scrollRect)
    {
        OptimizeScrollRectSettings(scrollRect);

        if (scrollRect.content != null)
        {
            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                var child = scrollRect.content.GetChild(i);
                var canvasGroup = child.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
            }
        }
    }

    public void AddScrollRect(ScrollRect scrollRect)
    {
        if (scrollRect != null && !managedScrollRects.Contains(scrollRect))
        {
            managedScrollRects.Add(scrollRect);

            var data = new ScrollRectData
            {
                lastPosition = scrollRect.normalizedPosition,
                lastUpdateTime = Time.time,
                isScrolling = false,
                needsUpdate = false,
                originalMovementType = scrollRect.movementType,
                originalElasticity = scrollRect.elasticity,
                originalDecelerationRate = scrollRect.decelerationRate
            };

            scrollData[scrollRect] = data;
            OptimizeScrollRectSettings(scrollRect);
        }
    }

    public void RemoveScrollRect(ScrollRect scrollRect)
    {
        if (scrollRect != null)
        {
            managedScrollRects.Remove(scrollRect);
            scrollData.Remove(scrollRect);
        }
    }

    void OnDestroy()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
    }

    void OnDisable()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(UpdateScrollRectsCoroutine());
        }
    }

    void OnEnable()
    {
        if (gameObject.activeInHierarchy)
        {
            StartUpdateCoroutine();
        }
    }
}