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
    private bool pendingQuotaEvaluation = false;

    [Header("Loss Screen")]
    [SerializeField] private GameObject losePrefab;

    [Header("Win Screen")]
    [SerializeField] private GameObject winPrefab;



    // Events for UI and game state updates
    public event Action<QuotaData> OnQuotaStarted;
    public event Action<QuotaData> OnQuotaCompleted;
    public event Action<QuotaData> OnQuotaFailed;
    public event Action<int> OnTurnChanged;
    public event Action<int> OnProgressChanged; // Money progress toward quota (current total money)

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
        pendingQuotaEvaluation = false;
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
        if (turnsRemaining < 0)
        {
            turnsRemaining = 0;
        }
        OnTurnChanged?.Invoke(turnsRemaining);

        Debug.Log($"Turn ended. Turns remaining: {turnsRemaining}");

        if (turnsRemaining <= 0)
        {
            // Defer quota evaluation until after the player has a chance to sell on the final turn.
            pendingQuotaEvaluation = true;
            Debug.Log("[QuotaManager] Final turn reached; quota evaluation deferred until next farm day.");
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

        // Quota is satisfied when the player's total money meets or exceeds the required amount.
        if (currentMoney >= currentQuota.quotaAmount)
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

        OnProgressChanged?.Invoke(currentMoney);

        // Check if quota met early
        if (turnsRemaining > 0 && currentMoney >= currentQuota.quotaAmount)
        {
            Debug.Log($"Quota achieved early! {currentMoney}/{currentQuota.quotaAmount}");
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
        pendingQuotaEvaluation = false;

        // Deduct the quota amount from player money
        MoneyManager.Instance.RemoveMoney(currentQuota.quotaAmount);

        // Give completion bonus if any
        if (currentQuota.completionBonus > 0)
        {
            MoneyManager.Instance.AddMoney(currentQuota.completionBonus);
        }

        OnQuotaCompleted?.Invoke(currentQuota);

        Debug.Log($"Quota Completed! Paid ${currentQuota.quotaAmount} to {currentQuota.creditorName}");

        // If this was the LAST quota, trigger win
        if (currentQuotaIndex >= quotas.Count - 1)
        {
            TriggerWin();
            return;
        }

        // Otherwise continue to next quota
        if (autoProgressToNextQuota)
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
        pendingQuotaEvaluation = false;

        TurnManager.Instance.gameOver = true;

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

    private void TriggerWin()
    {
        Debug.Log("[QuotaManager] ALL QUOTAS COMPLETED - YOU WIN!");

        isQuotaActive = false;
        pendingQuotaEvaluation = false;
        TurnManager.Instance.gameOver = true;

        // Spawn Win Prefab
        if (winPrefab != null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            GameObject instance;

            if (canvas != null)
                instance = Instantiate(winPrefab, canvas.transform);
            else
                instance = Instantiate(winPrefab);

            // Pause gameplay
            Time.timeScale = 0f;

            Debug.Log("[QuotaManager] Win prefab spawned.");
        }
        else
        {
            Debug.LogWarning("[QuotaManager] Win prefab not assigned!");
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
        return MoneyManager.Instance.GetMoney();
    }

    public bool IsQuotaActive()
    {
        return isQuotaActive;
    }

    public bool HasPendingQuotaEvaluation()
    {
        return pendingQuotaEvaluation;
    }

    /// <summary>
    /// Evaluate quota once the selling phase for the final turn is done.
    /// Returns true if an evaluation was attempted.
    /// </summary>
    public bool EvaluateQuotaIfDue()
    {
        if (!isQuotaActive)
        {
            pendingQuotaEvaluation = false;
            return false;
        }

        if (!pendingQuotaEvaluation && turnsRemaining > 0)
        {
            return false;
        }

        pendingQuotaEvaluation = false;
        CheckQuotaCompletion();
        return true;
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
