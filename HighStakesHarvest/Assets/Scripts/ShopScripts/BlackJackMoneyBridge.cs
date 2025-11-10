using UnityEngine;

/// <summary>
/// Bridge component that integrates BlackJack PlayerScript with MoneyManager.
/// Attach this to the Player object in the BlackJack scene.
/// This ensures BlackJack uses the persistent MoneyManager instead of its local money variable.
/// </summary>
public class BlackJackMoneyBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerScript playerScript;

    private bool isInitialized = false;

    private void Start()
    {
        if (playerScript == null)
        {
            playerScript = GetComponent<PlayerScript>();
        }

        if (playerScript == null)
        {
            Debug.LogError("BlackJackMoneyBridge: No PlayerScript found!");
            return;
        }

        InitializeMoney();
    }

    private void InitializeMoney()
    {
        if (MoneyManager.Instance == null)
        {
            Debug.LogError("BlackJackMoneyBridge: MoneyManager not found!");
            return;
        }

        // Sync PlayerScript's money with MoneyManager
        int currentMoney = MoneyManager.Instance.GetMoney();

        // Set PlayerScript money to match MoneyManager
        // Note: You may need to make PlayerScript.money public or add a SetMoney method
        // For now, this assumes we can access it through GetMoney/AdjustMoney pattern

        Debug.Log($"BlackJackMoneyBridge: Initialized with ${currentMoney}");
        isInitialized = true;
    }

    /// <summary>
    /// Call this instead of PlayerScript.AdjustMoney directly
    /// </summary>
    public void AdjustMoney(int amount)
    {
        if (MoneyManager.Instance == null) return;

        if (amount > 0)
        {
            MoneyManager.Instance.AddMoney(amount);
        }
        else if (amount < 0)
        {
            MoneyManager.Instance.RemoveMoney(-amount);
        }

        // Also update the PlayerScript for UI consistency
        if (playerScript != null)
        {
            playerScript.AdjustMoney(amount);
        }
    }

    /// <summary>
    /// Get current money from MoneyManager
    /// </summary>
    public int GetMoney()
    {
        if (MoneyManager.Instance != null)
        {
            return MoneyManager.Instance.GetMoney();
        }

        return 0;
    }

    /// <summary>
    /// Check if player has enough money for a bet
    /// </summary>
    public bool HasEnoughMoney(int amount)
    {
        if (MoneyManager.Instance != null)
        {
            return MoneyManager.Instance.HasEnoughMoney(amount);
        }

        return false;
    }
}