using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages quota progression throughout the game run.
/// Tracks current quota, turns remaining, and handles quota completion/failure.
/// Integrates with TurnManager to update turns.
/// </summary>
public class QuotaManager : MonoBehaviour
{
    public static QuotaManager Instance { get; private set; }

    [Header("Quota Configuration")]
    [SerializeField] private List<QuotaData> quotas = new List<QuotaData>();
    [SerializeField] private bool autoProgressToNextQuota = true;

    [Header("Current Run State")]
    private int currentQuotaIndex = 0;
    private int turnsRemaining;
    private int startingMoneyForQuota;

    [Header("Loss Screen")]
    [SerializeField] private GameObject losePrefab;


    // Events for UI and game state updates
    public event Action<QuotaData> OnQuotaStarted;
    public event Action<QuotaData> OnQuotaCompleted;
    public event Action<QuotaData> OnQuotaFailed;
    public event Action<int> OnTurnChanged;
    public event Action<int> OnProgressChanged; // Money progress toward quota

    private bool isQuotaActive = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Subscribe to TurnManager if available
        if (TurnManager.Instance != null)
        {
            // We'll manually call DecrementTurn from TurnManager.EndTurn()
        }

        // Subscribe to money changes
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += CheckQuotaProgress;
        }
    }

    void OnDestroy()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= CheckQuotaProgress;
        }
    }

    /// <summary>
    /// Start the first quota or a specific quota
    /// </summary>
    public void StartQuota(int quotaIndex = 0)
    {
        if (quotaIndex < 0 || quotaIndex >= quotas.Count)
        {
            Debug.LogError($"Invalid quota index: {quotaIndex}");
            return;
        }

        currentQuotaIndex = quotaIndex;
        QuotaData currentQuota = quotas[currentQuotaIndex];

        turnsRemaining = currentQuota.turnsAllowed;
        startingMoneyForQuota = MoneyManager.Instance.GetMoney();
        isQuotaActive = true;

        OnQuotaStarted?.Invoke(currentQuota);
        OnTurnChanged?.Invoke(turnsRemaining);

        Debug.Log($"Quota Started: {currentQuota.creditorName} - Need ${currentQuota.quotaAmount} in {turnsRemaining} turns");
    }

    /// <summary>
    /// Start the next quota in the sequence
    /// </summary>
    public void StartNextQuota()
    {
        if (currentQuotaIndex + 1 < quotas.Count)
        {
            StartQuota(currentQuotaIndex + 1);
        }
        else
        {
            Debug.Log("All quotas completed! Game won!");
            // You can trigger a win screen here
        }
    }

    /// <summary>
    /// Called by TurnManager when a turn ends
    /// </summary>
    public void DecrementTurn()
    {
        if (!isQuotaActive) return;

        turnsRemaining--;
        OnTurnChanged?.Invoke(turnsRemaining);

        Debug.Log($"Turn ended. Turns remaining: {turnsRemaining}");

        if (turnsRemaining <= 0)
        {
            CheckQuotaCompletion();
        }
    }

    /// <summary>
    /// Check if the player has met the quota
    /// </summary>
    private void CheckQuotaCompletion()
    {
        QuotaData currentQuota = GetCurrentQuota();
        if (currentQuota == null) return;

        int currentMoney = MoneyManager.Instance.GetMoney();
        int moneyEarnedThisQuota = currentMoney - startingMoneyForQuota;

        if (moneyEarnedThisQuota >= currentQuota.quotaAmount)
        {
            CompleteQuota();
        }
        else
        {
            FailQuota();
        }
    }

    /// <summary>
    /// Check progress toward quota and trigger events
    /// </summary>
    private void CheckQuotaProgress(int currentMoney)
    {
        if (!isQuotaActive) return;

        QuotaData currentQuota = GetCurrentQuota();
        if (currentQuota == null) return;

        int moneyEarnedThisQuota = currentMoney - startingMoneyForQuota;
        OnProgressChanged?.Invoke(moneyEarnedThisQuota);

        // Check if quota met early
        if (turnsRemaining > 0 && moneyEarnedThisQuota >= currentQuota.quotaAmount)
        {
            Debug.Log($"Quota achieved early! {moneyEarnedThisQuota}/{currentQuota.quotaAmount}");
            // Optionally show notification but don't complete until season ends
        }
    }

    /// <summary>
    /// Complete the current quota
    /// </summary>
    private void CompleteQuota()
    {
        QuotaData currentQuota = GetCurrentQuota();
        if (currentQuota == null) return;

        isQuotaActive = false;

        // Deduct the quota amount from player money
        MoneyManager.Instance.RemoveMoney(currentQuota.quotaAmount);

        // Give completion bonus if any
        if (currentQuota.completionBonus > 0)
        {
            MoneyManager.Instance.AddMoney(currentQuota.completionBonus);
        }

        OnQuotaCompleted?.Invoke(currentQuota);

        Debug.Log($"Quota Completed! Paid ${currentQuota.quotaAmount} to {currentQuota.creditorName}");

        // Auto-progress to next quota if enabled
        if (autoProgressToNextQuota && currentQuotaIndex + 1 < quotas.Count)
        {
            StartNextQuota();
        }
    }

    /// <summary>
    /// Fail the current quota (game over)
    /// </summary>
    private void FailQuota()
    {
        QuotaData currentQuota = GetCurrentQuota();
        if (currentQuota == null) return;

        isQuotaActive = false;

        OnQuotaFailed?.Invoke(currentQuota);

        Debug.Log($"Quota Failed! Could not pay ${currentQuota.quotaAmount} to {currentQuota.creditorName}");

        // --- Spawn Lose Prefab ---
        if (losePrefab != null)
        {
            // Try to find a Canvas in the scene
            Canvas canvas = FindObjectOfType<Canvas>();
            GameObject instance;

            if (canvas != null)
            {
                instance = Instantiate(losePrefab, canvas.transform);
            }
            else
            {
                // If no canvas exists, instantiate at root
                instance = Instantiate(losePrefab);
            }

            // Optionally: pause the game
            Time.timeScale = 0f;

            Debug.Log("[QuotaManager] Lose prefab spawned.");
        }
        else
        {
            Debug.LogWarning("[QuotaManager] Lose prefab not assigned!");
        }
    }


    // ==================== PUBLIC GETTERS ====================

    public QuotaData GetCurrentQuota()
    {
        if (currentQuotaIndex >= 0 && currentQuotaIndex < quotas.Count)
        {
            return quotas[currentQuotaIndex];
        }
        return null;
    }

    public int GetCurrentQuotaAmount()
    {
        QuotaData quota = GetCurrentQuota();
        return quota != null ? quota.quotaAmount : 0;
    }

    public int GetTurnsRemaining()
    {
        return turnsRemaining;
    }

    public int GetMoneyProgressTowardQuota()
    {
        int currentMoney = MoneyManager.Instance.GetMoney();
        return currentMoney - startingMoneyForQuota;
    }

    public bool IsQuotaActive()
    {
        return isQuotaActive;
    }

    public int GetCurrentQuotaIndex()
    {
        return currentQuotaIndex;
    }

    public int GetTotalQuotas()
    {
        return quotas.Count;
    }

    public Season GetCurrentSeason()
    {
        QuotaData quota = GetCurrentQuota();
        return quota != null ? quota.season : Season.Spring;
    }

    // ==================== TESTING/DEBUG ====================

    /// <summary>
    /// Skip to next quota (for testing)
    /// </summary>
    public void SkipQuota()
    {
        CompleteQuota();
    }

    /// <summary>
    /// Add extra turns (for testing)
    /// </summary>
    public void AddTurns(int amount)
    {
        turnsRemaining += amount;
        OnTurnChanged?.Invoke(turnsRemaining);
    }
}
