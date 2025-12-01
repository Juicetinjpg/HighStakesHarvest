using UnityEngine;
using System;

/// <summary>
/// Manages player money persistently across all scenes.
/// Singleton pattern ensures money persists between farm and casino.
/// </summary>
public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("Money Settings")]
    [SerializeField] private int startingMoney = 100;
    
    private int currentMoney;
    private bool spendingLocked = false;

    // Events for UI updates
    public event Action<int> OnMoneyChanged;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize money
        currentMoney = startingMoney;
    }

    /// <summary>
    /// Prevent spending while a quota is being evaluated (e.g., during casino phase after final farm turn).
    /// </summary>
    public void SetSpendingLocked(bool locked)
    {
        spendingLocked = locked;
        Debug.Log($"MoneyManager spending lock {(locked ? "ENABLED" : "DISABLED")}");
    }

    public bool IsSpendingLocked()
    {
        return spendingLocked;
    }

    /// <summary>
    /// Get the current money amount
    /// </summary>
    public int GetMoney()
    {
        return currentMoney;
    }

    /// <summary>
    /// Add money to the player's total
    /// </summary>
    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Use RemoveMoney() for negative amounts");
            return;
        }

        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"Money added: +${amount}. Total: ${currentMoney}");
    }

    /// <summary>
    /// Remove money from the player's total
    /// </summary>
    public bool RemoveMoney(int amount, bool ignoreLock = false)
    {
        if (spendingLocked && !ignoreLock)
        {
            Debug.LogWarning("Spending is currently locked until the quota is evaluated.");
            return false;
        }

        if (amount < 0)
        {
            Debug.LogWarning("Amount should be positive when removing money");
            return false;
        }

        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            Debug.Log($"Money removed: -${amount}. Total: ${currentMoney}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Not enough money! Required: ${amount}, Available: ${currentMoney}");
            return false;
        }
    }

    /// <summary>
    /// Set money to a specific amount (useful for testing or cheats)
    /// </summary>
    public void SetMoney(int amount)
    {
        currentMoney = Mathf.Max(0, amount);
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"Money set to: ${currentMoney}");
    }

    /// <summary>
    /// Check if player has enough money
    /// </summary>
    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }

    /// <summary>
    /// Reset money to starting amount (for new runs)
    /// </summary>
    public void ResetMoney()
    {
        currentMoney = startingMoney;
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"Money reset to starting amount: ${startingMoney}");
    }
}
