using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CopyTurnLogToClipboard : MonoBehaviour
{
    [Header("Optional: Assign a Button (auto-detects if empty)")]
    public Button button;

    private string logPath;

    private void Start()
    {
        // Auto-detect button if not manually assigned
        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("[CopyTurnLogToClipboard] No Button component found!");
            return;
        }

        button.onClick.AddListener(CopyLog);

        logPath = Path.Combine(Application.persistentDataPath, "turnLog.txt");
    }

    private void CopyLog()
    {
        if (!File.Exists(logPath))
        {
            Debug.LogWarning("[CopyTurnLogToClipboard] Log file does not exist: " + logPath);
            return;
        }

        string content = File.ReadAllText(logPath);

        if (string.IsNullOrEmpty(content))
        {
            Debug.LogWarning("[CopyTurnLogToClipboard] Log file is empty.");
            return;
        }

        // Copy to clipboard
        GUIUtility.systemCopyBuffer = content;

        Debug.Log("[CopyTurnLogToClipboard] Log file copied to clipboard.");
    }
}
