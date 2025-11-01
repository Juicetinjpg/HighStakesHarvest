using UnityEngine;
using UnityEngine.SceneManagement;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Turn Settings")]
    [SerializeField] private float turnTimeLimit = 60f;
    
    [Header("Player Reference")]
    [SerializeField] private MonoBehaviour playerMovementScript; // Assign in Inspector
    
    private float currentTurnTimeRemaining;
    private bool isTurnActive = false;

    public System.Action OnTurnStarted;
    public System.Action OnTurnEnded;
    public System.Action<float> OnTurnTimeChanged;

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

        Debug.Log($"Turn started. Time limit: {turnTimeLimit}s");
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

        Debug.Log("Turn ended - Player movement disabled");

        // Automatically load casino after short delay
        Invoke("LoadCasinoScene", 1.5f);
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

    // Getters
    public float GetTurnTimeRemaining() { return currentTurnTimeRemaining; }
    public float GetTurnTimeLimit() { return turnTimeLimit; }
    public bool IsTurnActive() { return isTurnActive; }
    
    // Testing
    public void SetTurnTimeLimit(float seconds) { turnTimeLimit = seconds; }
    public void AddTime(float seconds) 
    { 
        if (isTurnActive) 
            currentTurnTimeRemaining += seconds; 
    }
}