using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DeleteSaveFile : MonoBehaviour
{
    [Header("Optional: Assign a Button (otherwise script finds it automatically)")]
    public Button deleteButton;

    private void Start()
    {
        // Auto-hook if no button assigned
        if (deleteButton == null)
            deleteButton = GetComponent<Button>();

        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteSave);
        else
            Debug.LogError("[DeleteSaveFile] No button found!");
    }

    private void DeleteSave()
    {
        string path = Path.Combine(Application.persistentDataPath, "saveData.json");

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("[DeleteSaveFile] Save file deleted at: " + path);
        }
        else
        {
            Debug.Log("[DeleteSaveFile] No save file to delete.");
        }

        Debug.Log("[DeleteSaveFile] Save reset complete.");
    }
}
