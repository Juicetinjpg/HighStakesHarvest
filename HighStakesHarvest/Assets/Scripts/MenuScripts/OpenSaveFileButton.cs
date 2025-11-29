using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics; // needed for Process
using Debug = UnityEngine.Debug; // resolve ambiguity

public class OpenSaveFileButton : MonoBehaviour
{
    public Button openButton;

    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        if (openButton != null)
            openButton.onClick.AddListener(OpenSaveFile);
    }

    private void OpenSaveFile()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Save file does not exist!");
            return;
        }

        try
        {
            // Fully qualify the Process call
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = savePath,
                UseShellExecute = true
            });

            Debug.Log("Save file opened: " + savePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to open save file: " + ex.Message);
        }
    }
}
