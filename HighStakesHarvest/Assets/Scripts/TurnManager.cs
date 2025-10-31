using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Enhanced TurnManager that integrates with QuotaManager
/// Handles turn progression and notifies quota system
/// </summary>
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Turn Settings")]
    [SerializeField] private float turnTimeLimit = 60f; // Time limit per turn in seconds

    private float currentTurnTimeRemaining;
    private bool isTurnActive = false;

    // Events
    public System.Action OnTurnStarted;
    public System.Action OnTurnEnded;
    public System.Action<float> OnTurnTimeChanged; // Remaining time

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Check current scene and start turn if in farm scene
        if (SceneManager.GetActiveScene().name == "FarmScene")
        {
            StartTurn();
        }
    }

    void Update()
    {
        if (isTurnActive)
        {
            currentTurnTimeRemaining -= Time.deltaTime;
            OnTurnTimeChanged?.Invoke(currentTurnTimeRemaining);

            if (currentTurnTimeRemaining <= 0)
            {
                currentTurnTimeRemaining = 0;
                EndTurn();
            }
        }

        // Debug: Press Space to manually end turn
        if (Input.GetKeyDown(KeyCode.Space) && isTurnActive)
        {
            EndTurn();
        }
    }

    /// <summary>
    /// Start a new turn
    /// </summary>
    public void StartTurn()
    {
        if (isTurnActive)
        {
            Debug.LogWarning("Turn already active!");
            return;
        }

        currentTurnTimeRemaining = turnTimeLimit;
        isTurnActive = true;
        OnTurnStarted?.Invoke();

        Debug.Log($"Turn started. Time limit: {turnTimeLimit}s");
    }

    /// <summary>
    /// End the current turn
    /// </summary>
    public void EndTurn()
    {
        if (!isTurnActive)
        {
            Debug.LogWarning("No active turn to end!");
            return;
        }

        isTurnActive = false;

        // Advance all plants
        if (PlantManager.Instance != null)
        {
            var plants = PlantManager.Instance.Plants;
            foreach (var go in plants)
            {
                if (go == null) continue;
                Plant plantComp = go.GetComponent<Plant>();
                if (plantComp != null)
                    plantComp.AdvanceTurn();
            }
        }

        // Notify QuotaManager to decrement turns
        if (QuotaManager.Instance != null)
        {
            QuotaManager.Instance.DecrementTurn();
        }

        OnTurnEnded?.Invoke();

        Debug.Log("Turn ended");

        // Note: Scene transition to casino should be handled by a UI button
        // that calls LoadCasinoScene() 
    }

    /// <summary>
    /// Load casino scene (called by button)
    /// </summary>
    public void LoadCasinoScene()
    {
        SceneManager.LoadScene("CasinoScene");
    }

    /// <summary>
    /// Load farm scene and start new turn (called when leaving casino)
    /// </summary>
    public void LoadFarmScene()
    {
        SceneManager.sceneLoaded += OnFarmSceneLoaded;
        SceneManager.LoadScene("FarmScene");
    }

    private void OnFarmSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "FarmScene")
        {
            StartTurn();
            SceneManager.sceneLoaded -= OnFarmSceneLoaded;
        }
    }

    // ==================== GETTERS ====================

    public float GetTurnTimeRemaining()
    {
        return currentTurnTimeRemaining;
    }

    public float GetTurnTimeLimit()
    {
        return turnTimeLimit;
    }

    public bool IsTurnActive()
    {
        return isTurnActive;
    }

    // ==================== TESTING ====================

    public void SetTurnTimeLimit(float seconds)
    {
        turnTimeLimit = seconds;
    }

    public void AddTime(float seconds)
    {
        if (isTurnActive)
        {
            currentTurnTimeRemaining += seconds;
        }
    }
}
