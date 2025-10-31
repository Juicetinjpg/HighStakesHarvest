using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Initializes the game systems when starting a new run.
/// Place this on a GameObject in your first scene (main menu or farm scene).
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Auto-Initialize Settings")]
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private bool startFirstQuotaAutomatically = true;

    [Header("Scene Names")]
    [SerializeField] private string farmSceneName = "FarmScene";
    [SerializeField] private string casinoSceneName = "CasinoScene";

    void Start()
    {
        if (initializeOnStart)
        {
            InitializeGame();
        }
    }

    /// <summary>
    /// Initialize all game systems for a new run
    /// </summary>
    public void InitializeGame()
    {
        Debug.Log("Initializing High Stakes Harvest...");

        // Verify all managers exist
        if (MoneyManager.Instance == null)
        {
            Debug.LogError("MoneyManager not found! Please add MoneyManager to the scene.");
            return;
        }

        if (QuotaManager.Instance == null)
        {
            Debug.LogError("QuotaManager not found! Please add QuotaManager to the scene.");
            return;
        }

        if (TurnManager.Instance == null)
        {
            Debug.LogWarning("TurnManager not found. It may be initialized later.");
        }

        // Reset money to starting amount
        MoneyManager.Instance.ResetMoney();

        // Start first quota if enabled
        if (startFirstQuotaAutomatically)
        {
            QuotaManager.Instance.StartQuota(0);
        }

        Debug.Log("Game initialized successfully!");
    }

    /// <summary>
    /// Start a new game run (call this from main menu "New Game" button)
    /// </summary>
    public void StartNewGame()
    {
        InitializeGame();
        
        // Load farm scene
        SceneManager.LoadScene(farmSceneName);
    }

    /// <summary>
    /// Continue existing game (for future save system)
    /// </summary>
    public void ContinueGame()
    {
        // TODO: Load saved game data
        SceneManager.LoadScene(farmSceneName);
    }

    /// <summary>
    /// Reset everything for a fresh start
    /// </summary>
    public void ResetGame()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.ResetMoney();
        }

        if (QuotaManager.Instance != null)
        {
            QuotaManager.Instance.StartQuota(0);
        }

        Debug.Log("Game reset to initial state");
    }
}
