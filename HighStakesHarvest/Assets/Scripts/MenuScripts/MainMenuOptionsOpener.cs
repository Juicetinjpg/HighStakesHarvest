using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuOptionsOpener : MonoBehaviour
{
    [Header("Assign your Options Menu Prefab here")]
    public GameObject optionsMenuPrefab;

    private GameObject spawnedOptionsMenu;

    private void Update()
    {
        // Allow ESC to close the options menu
        if (spawnedOptionsMenu != null && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseOptionsMenu();
        }
    }

    // This is called by the button OnClick event
    public void OpenOptionsMenu()
    {
        if (spawnedOptionsMenu != null)
            return; // already open

        // Find the canvas in the scene
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("MainMenuOptionsOpener: No Canvas found in scene!");
            return;
        }

        // Spawn the menu under the canvas
        spawnedOptionsMenu = Instantiate(optionsMenuPrefab, canvas.transform);
    }

    public void CloseOptionsMenu()
    {
        if (spawnedOptionsMenu != null)
        {
            Destroy(spawnedOptionsMenu);
            spawnedOptionsMenu = null;
        }
    }
}
