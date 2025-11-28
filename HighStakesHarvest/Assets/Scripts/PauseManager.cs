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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private bool IsInMainMenu()
    {
        return SceneManager.GetActiveScene().name == "MainMenu";
    }

    void Update()
    {
        // Disable pause system in MainMenu
        if (IsInMainMenu())
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuInstance != null && pauseMenuInstance.activeSelf)
                Resume();
            else
                Pause();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pauseMenuInstance != null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                pauseMenuInstance.transform.SetParent(canvas.transform, false);
        }

        // If entering MainMenu, destroy existing pause menu instance
        if (IsInMainMenu() && pauseMenuInstance != null)
        {
            Destroy(pauseMenuInstance);
            pauseMenuInstance = null;
            Time.timeScale = 1f;
            isPaused = false;
        }
    }

    public void Pause()
    {
        // Do not open pause menu in MainMenu
        if (IsInMainMenu())
            return;

        if (pauseMenuInstance == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            Transform parent = canvas != null ? canvas.transform : null;

            pauseMenuInstance = Instantiate(pauseMenuPrefab, parent);

            var canvasComp = pauseMenuInstance.GetComponentInChildren<Canvas>();
            if (canvasComp != null)
                canvasComp.sortingOrder = 100;
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
        if (IsInMainMenu())
            return;

        if (pauseMenuInstance != null)
            pauseMenuInstance.SetActive(true);
    }
}
