using UnityEngine;

public class OptionsMenuOpener : MonoBehaviour
{
    [Header("Assign your Options Menu prefab here")]
    public GameObject optionsMenuPrefab;

    private GameObject optionsMenuInstance;

    public void OpenOptionsMenu()
    {
        // Get the PauseManager instance
        PauseManager pauseManager = FindObjectOfType<PauseManager>();
        if (pauseManager == null)
        {
            Debug.LogWarning("PauseManager not found in scene!");
            return;
        }

        // Hide the currently active pause menu (if it exists)
        var pauseMenu = pauseManager.GetCurrentPauseMenu();
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        // Create the options menu inside the same Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found to parent the Options Menu!");
            return;
        }

        if (optionsMenuInstance == null)
        {
            optionsMenuInstance = Instantiate(optionsMenuPrefab, canvas.transform, false);
        }

        optionsMenuInstance.SetActive(true);
    }
}
