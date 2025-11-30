using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class GoToMainMenu : MonoBehaviour
{
    [Header("Optional: Assign a Button (otherwise script finds it automatically)")]
    public Button mainMenuButton;

    private void Start()
    {
        // Auto-hook if no button assigned
        if (mainMenuButton == null)
            mainMenuButton = GetComponent<Button>();

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnButtonClick);
        else
            Debug.LogError("GoToMainMenu: No Button component found on this GameObject.");
    }

    private void OnButtonClick()
    {
        // Delete current save file
        string path = Path.Combine(Application.persistentDataPath, "saveData.json");

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("[GoToMainMenu] Save file deleted at: " + path);
        }
        else
        {
            Debug.Log("[GoToMainMenu] No save file to delete.");
        }

        Debug.Log("[GoToMainMenu] Save reset complete.");

        // Load the MainMenu scene
        SceneManager.LoadScene("MainMenu");
    }
}
