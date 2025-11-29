using UnityEngine;

public class OptionsButtonMainMenu : MonoBehaviour
{
    [Header("Assign your Options Menu Prefab")]
    public GameObject optionsMenuPrefab;

    private GameObject optionsMenuInstance;

    public void OpenOptionsMenu()
    {
        // Prevent duplicates
        if (optionsMenuInstance != null)
            return;

        // Find canvas in the scene
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("OptionsButtonMainMenu: No Canvas found in scene!");
            return;
        }

        // Spawn the options menu under the canvas
        optionsMenuInstance = Instantiate(optionsMenuPrefab, canvas.transform);
    }
}
