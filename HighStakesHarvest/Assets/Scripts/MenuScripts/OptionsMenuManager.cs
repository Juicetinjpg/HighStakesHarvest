using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenuManager : MonoBehaviour
{
    private PauseManager pauseManager;

    void Start()
    {
        pauseManager = FindObjectOfType<PauseManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToPauseMenu();
        }
    }

    public void BackToPauseMenu()
    {
        // If we are in MainMenu, simply close the options menu
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Destroy(gameObject);
            return;
        }

        // Otherwise, return to Pause Menu
        if (pauseManager != null)
            pauseManager.ShowPauseMenu();

        gameObject.SetActive(false);
    }
}
