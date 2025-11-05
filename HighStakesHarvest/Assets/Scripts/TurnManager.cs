using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TurnManager with timer-based turns and scene management
/// Now supports both old Plant and new Plant components
/// </summary>
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Turn Settings")]
    [SerializeField] private float turnTimeLimit = 60f;

    [Header("Player Reference")]
    [SerializeField] private MonoBehaviour playerMovementScript; // Assign in Inspector

    [Header("Season Settings (Optional)")]
    public string currentSeason = "Spring";
    public bool enableSeasons = false;
    public int turnsPerSeason = 28;
    private int turnCount = 0;
    private string[] seasons = { "Spring", "Summer", "Fall", "Winter" };

    private float currentTurnTimeRemaining;
    private bool isTurnActive = false;

    public System.Action OnTurnStarted;
    public System.Action OnTurnEnded;
    public System.Action<float> OnTurnTimeChanged;
    public System.Action<string> OnSeasonChanged; // New event for season changes

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Listen for scene loads so we can reset the turn when FarmScene is loaded
        SceneManager.sceneLoaded += OnAnySceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnAnySceneLoaded;
    }

    void Start()
    {
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

    public void StartTurn()
    {
        if (isTurnActive)
        {
            Debug.LogWarning("Turn already active!");
            return;
        }

        currentTurnTimeRemaining = turnTimeLimit;
        isTurnActive = true;

        // Enable player movement
        EnablePlayerMovement(true);

        OnTurnStarted?.Invoke();

        Debug.Log($"Turn started. Time limit: {turnTimeLimit}s (Season: {currentSeason})");
    }

    // Force-reset the turn timer and start the turn regardless of previous state
    public void ResetAndStartTurn()
    {
        currentTurnTimeRemaining = turnTimeLimit;
        isTurnActive = true;

        EnablePlayerMovement(true);
        OnTurnStarted?.Invoke();

        Debug.Log($"Turn timer reset and started due to FarmScene load. (Season: {currentSeason})");
    }

    public void EndTurn()
    {
        if (!isTurnActive)
        {
            Debug.LogWarning("No active turn to end!");
            return;
        }

        isTurnActive = false;

        // Disable player movement 
        EnablePlayerMovement(false);

        Debug.Log("=== Turn Ending ===");

        // Advance all plants - supports BOTH old and new plant systems
        AdvanceAllPlants();

        // Increment turn counter
        turnCount++;

        // Check for season change
        if (enableSeasons && turnCount % turnsPerSeason == 0)
        {
            AdvanceSeason();
        }

        // Notify QuotaManager to decrement turns
        if (QuotaManager.Instance != null)
        {
            QuotaManager.Instance.DecrementTurn();
        }

        OnTurnEnded?.Invoke();

        Debug.Log("Turn ended - Player movement disabled");

        // Automatically load casino after short delay
        Invoke("LoadCasinoScene", 1.5f);
    }

    /// <summary>
    /// Advances all plants - works with both old Plant and new Plant
    /// </summary>
    private void AdvanceAllPlants()
    {
        if (PlantManager.Instance == null)
        {
            Debug.LogWarning("PlantManager not found!");
            return;
        }

        var plants = PlantManager.Instance.Plants;
        int oldPlantCount = 0;
        int newPlantCount = 0;

        foreach (var plantObj in plants)
        {
            if (plantObj == null) continue;

            // Try new Plant first
            Plant Plant = plantObj.GetComponent<Plant>();
            if (Plant != null)
            {
                Plant.AdvanceTurn();
                newPlantCount++;
                continue;
            }

            // Fall back to old Plant for backward compatibility
            Plant oldPlant = plantObj.GetComponent<Plant>();
            if (oldPlant != null)
            {
                oldPlant.AdvanceTurn();
                oldPlantCount++;
            }
        }

        Debug.Log($"Advanced {newPlantCount} new plants and {oldPlantCount} old plants");
    }

    /// <summary>
    /// Advances to the next season
    /// </summary>
    private void AdvanceSeason()
    {
        int currentSeasonIndex = System.Array.IndexOf(seasons, currentSeason);
        int nextSeasonIndex = (currentSeasonIndex + 1) % seasons.Length;
        currentSeason = seasons[nextSeasonIndex];

        Debug.Log($"🍂 Season changed to: {currentSeason}");

        // Update all Plants with new season
        if (PlantManager.Instance != null)
        {
            var plants = PlantManager.Instance.Plants;

            foreach (var plantObj in plants)
            {
                if (plantObj == null) continue;

                Plant plant = plantObj.GetComponent<Plant>();
                if (plant != null)
                {
                    plant.currentSeason = currentSeason;
                }
            }
        }

        OnSeasonChanged?.Invoke(currentSeason);
    }

    private void EnablePlayerMovement(bool enabled)
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = enabled;
            Debug.Log($"Player movement {(enabled ? "enabled" : "disabled")}");
        }
        else
        {
            Debug.LogWarning("Player movement script not assigned!");
        }
    }

    public void LoadCasinoScene()
    {
        SceneManager.LoadScene("CasinoScene");
    }

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

    private void OnAnySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "FarmScene")
        {
            // Reset timer any time FarmScene is loaded
            ResetAndStartTurn();
        }
    }

    // Getters
    public float GetTurnTimeRemaining() { return currentTurnTimeRemaining; }
    public float GetTurnTimeLimit() { return turnTimeLimit; }
    public bool IsTurnActive() { return isTurnActive; }
    public string GetCurrentSeason() { return currentSeason; }
    public int GetTurnCount() { return turnCount; }

    // Setters for testing/debugging
    public void SetTurnTimeLimit(float seconds) { turnTimeLimit = seconds; }
    public void AddTime(float seconds)
    {
        if (isTurnActive)
            currentTurnTimeRemaining += seconds;
    }

    /// <summary>
    /// Manually set the season (for testing or story events)
    /// </summary>
    public void SetSeason(string season)
    {
        if (System.Array.IndexOf(seasons, season) == -1)
        {
            Debug.LogWarning($"Invalid season: {season}");
            return;
        }

        currentSeason = season;
        Debug.Log($"Season manually set to: {currentSeason}");

        // Update all plants with new season
        if (PlantManager.Instance != null)
        {
            var plants = PlantManager.Instance.Plants;
            foreach (var plantObj in plants)
            {
                if (plantObj == null) continue;

                Plant plant = plantObj.GetComponent<Plant>();
                if (plant != null)
                {
                    plant.currentSeason = currentSeason;
                }
            }
        }

        OnSeasonChanged?.Invoke(currentSeason);
    }
}