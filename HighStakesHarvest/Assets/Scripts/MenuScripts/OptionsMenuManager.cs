using UnityEngine;

public class OptionsMenuManager : MonoBehaviour
{
    private PauseManager pauseManager;

    void Start()
    {
        pauseManager = FindObjectOfType<PauseManager>();
    }

    void Update()
    {
        // If Esc is pressed, go back to Pause Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToPauseMenu();
        }
    }

    public void BackToPauseMenu()
    {
        if (pauseManager != null)
        {
            // Re-enable pause menu
            pauseManager.ShowPauseMenu();
        }

        // Disable options menu
        gameObject.SetActive(false);
    }
}
