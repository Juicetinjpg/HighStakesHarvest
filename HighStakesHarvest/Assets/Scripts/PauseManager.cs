using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuPrefab;
    private GameObject pauseMenuInstance;
    private bool isPaused = false;

    private static PauseManager instance;

    void Awake()
    {
        // Singleton setup so this persists between scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // When a new scene loads, make sure pauseMenuInstance reattaches properly
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Use the actual pause menu instance active state
            if (pauseMenuInstance != null && pauseMenuInstance.activeSelf)
                Resume();
            else
                Pause();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If pause menu already exists, reattach it to the new scene's Canvas
        if (pauseMenuInstance != null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                pauseMenuInstance.transform.SetParent(canvas.transform, false);
        }
    }

    public void Pause()
    {
        if (pauseMenuInstance == null)
        {
            // Find current scene's UI Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            Transform parent = canvas != null ? canvas.transform : null;

            // Spawn inside Canvas or at root if none
            pauseMenuInstance = Instantiate(pauseMenuPrefab, parent);

            // Ensure it's visible on top
            var canvasComp = pauseMenuInstance.GetComponentInChildren<Canvas>();
            if (canvasComp != null)
            {
                canvasComp.sortingOrder = 100;
            }
        }

        pauseMenuInstance.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        if (pauseMenuInstance != null)
            pauseMenuInstance.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public GameObject GetCurrentPauseMenu()
    {
        return pauseMenuInstance;
    }

    public void ShowPauseMenu()
    {
        if (pauseMenuInstance != null)
        {
            pauseMenuInstance.SetActive(true);
        }
    }

}
