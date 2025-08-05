using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System.Collections;

public class CombatLog : MonoBehaviour
{
    public static CombatLog Instance { get; private set; }

    [Header("UI Components")]
    public TextMeshProUGUI logText;
    public ScrollRect scrollRect;

    [Header("Performance Settings")]
    [SerializeField] private int maxLogEntries = 100;
    [SerializeField] private float scrollDelay = 0.1f;

    private StringBuilder logBuilder;
    private int currentEntryCount = 0;
    private Coroutine scrollCoroutine;
    private bool needsScroll = false;

    void Awake()
    {
        Instance = this;
        logBuilder = new StringBuilder();
    }

    void Start()
    {
        StartCoroutine(ScrollUpdateCoroutine());
    }

    public void AddLog(string message)
    {
        logBuilder.AppendLine(message);
        currentEntryCount++;

        if (currentEntryCount > maxLogEntries)
        {
            TrimOldEntries();
        }

        if (logText != null)
        {
            logText.text = logBuilder.ToString();
        }

        needsScroll = true;
    }

    public void ClearAll()
    {
        logBuilder.Clear();
        currentEntryCount = 0;

        if (logText != null)
        {
            logText.text = "";
        }

        needsScroll = true;
    }

    private void TrimOldEntries()
    {
        string currentLog = logBuilder.ToString();
        string[] lines = currentLog.Split('\n');

        int linesToKeep = maxLogEntries - 10;
        if (lines.Length > linesToKeep)
        {
            logBuilder.Clear();

            for (int i = lines.Length - linesToKeep; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    logBuilder.AppendLine(lines[i]);
                }
            }

            currentEntryCount = linesToKeep;
        }
    }

    private IEnumerator ScrollUpdateCoroutine()
    {
        while (true)
        {
            if (needsScroll && scrollRect != null)
            {
                yield return new WaitForEndOfFrame();
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
                needsScroll = false;
            }

            yield return new WaitForSeconds(scrollDelay);
        }
    }

    private void OnEnable()
    {
        if (scrollCoroutine == null)
        {
            scrollCoroutine = StartCoroutine(ScrollUpdateCoroutine());
        }
    }

    private void OnDisable()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
        }
    }
}