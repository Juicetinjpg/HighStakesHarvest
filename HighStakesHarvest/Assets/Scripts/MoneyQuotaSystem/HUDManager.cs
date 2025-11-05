using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the HUD display for quota information, current money, turns left, and time remaining.
/// As specified in the GDD, displays on top right:
/// - Quota amount
/// - Current Money
/// - Turns Left
/// - Time Left
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI quotaText;
    [SerializeField] private TextMeshProUGUI currentMoneyText;
    [SerializeField] private TextMeshProUGUI turnsLeftText;
    [SerializeField] private TextMeshProUGUI timeLeftText;
    
    [Header("Optional: Progress Bar")]
    [SerializeField] private Slider quotaProgressBar;
    [SerializeField] private TextMeshProUGUI progressPercentText;

    [Header("Optional: Creditor Info")]
    [SerializeField] private TextMeshProUGUI creditorNameText;
    [SerializeField] private Image creditorPortrait;

    [Header("Display Settings")]
    [SerializeField] private bool showProgressBar = true;
    [SerializeField] private bool showCreditorInfo = true;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;

    void Start()
    {
        // Subscribe to events
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(MoneyManager.Instance.GetMoney());
        }

        if (QuotaManager.Instance != null)
        {
            QuotaManager.Instance.OnQuotaStarted += OnQuotaStarted;
            QuotaManager.Instance.OnTurnChanged += UpdateTurnsDisplay;
            QuotaManager.Instance.OnProgressChanged += UpdateProgressDisplay;
            
            // Initial update
            UpdateAllDisplays();
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnTimeChanged += UpdateTimeDisplay;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
        }

        if (QuotaManager.Instance != null)
        {
            QuotaManager.Instance.OnQuotaStarted -= OnQuotaStarted;
            QuotaManager.Instance.OnTurnChanged -= UpdateTurnsDisplay;
            QuotaManager.Instance.OnProgressChanged -= UpdateProgressDisplay;
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnTimeChanged -= UpdateTimeDisplay;
        }
    }

    /// <summary>
    /// Update all HUD displays
    /// </summary>
    private void UpdateAllDisplays()
    {
        if (QuotaManager.Instance != null)
        {
            QuotaData currentQuota = QuotaManager.Instance.GetCurrentQuota();
            
            if (currentQuota != null)
            {
                UpdateQuotaDisplay(currentQuota.quotaAmount);
                UpdateTurnsDisplay(QuotaManager.Instance.GetTurnsRemaining());
                UpdateProgressDisplay(QuotaManager.Instance.GetMoneyProgressTowardQuota());
                
                if (showCreditorInfo)
                {
                    UpdateCreditorDisplay(currentQuota);
                }
            }
        }

        if (MoneyManager.Instance != null)
        {
            UpdateMoneyDisplay(MoneyManager.Instance.GetMoney());
        }

        if (TurnManager.Instance != null)
        {
            UpdateTimeDisplay(TurnManager.Instance.GetTurnTimeRemaining());
        }
    }

    /// <summary>
    /// Update quota amount display
    /// </summary>
    private void UpdateQuotaDisplay(int quotaAmount)
    {
        if (quotaText != null)
        {
            quotaText.text = $"Quota: ${quotaAmount}";
        }
    }

    /// <summary>
    /// Update current money display
    /// </summary>
    private void UpdateMoneyDisplay(int money)
    {
        if (currentMoneyText != null)
        {
            currentMoneyText.text = $"Money: ${money}";
        }
    }

    /// <summary>
    /// Update turns remaining display
    /// </summary>
    private void UpdateTurnsDisplay(int turnsLeft)
    {
        if (turnsLeftText != null)
        {
            turnsLeftText.text = $"Turns Left: {turnsLeft}";
            
            // Change color based on urgency
            if (turnsLeft <= 2)
            {
                turnsLeftText.color = negativeColor;
            }
            else if (turnsLeft <= 5)
            {
                turnsLeftText.color = Color.yellow;
            }
            else
            {
                turnsLeftText.color = neutralColor;
            }
        }
    }

    /// <summary>
    /// Update time remaining in current turn
    /// </summary>
    private void UpdateTimeDisplay(float timeRemaining)
    {
        if (timeLeftText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            
            timeLeftText.text = $"Time: {minutes:00}:{seconds:00}";
            
            // Change color when time is running out
            if (timeRemaining <= 10f)
            {
                timeLeftText.color = negativeColor;
            }
            else if (timeRemaining <= 30f)
            {
                timeLeftText.color = Color.yellow;
            }
            else
            {
                timeLeftText.color = neutralColor;
            }
        }
    }

    /// <summary>
    /// Update progress toward quota
    /// </summary>
    private void UpdateProgressDisplay(int progressAmount)
    {
        if (!showProgressBar || QuotaManager.Instance == null) return;

        int quotaAmount = QuotaManager.Instance.GetCurrentQuotaAmount();
        
        if (quotaProgressBar != null)
        {
            float progress = Mathf.Clamp01((float)progressAmount / quotaAmount);
            quotaProgressBar.value = progress;
        }

        if (progressPercentText != null)
        {
            float percent = ((float)progressAmount / quotaAmount) * 100f;
            progressPercentText.text = $"{Mathf.RoundToInt(percent)}%";
            
            // Color code based on progress
            if (percent >= 100f)
            {
                progressPercentText.color = positiveColor;
            }
            else if (percent >= 75f)
            {
                progressPercentText.color = Color.yellow;
            }
            else
            {
                progressPercentText.color = neutralColor;
            }
        }
    }

    /// <summary>
    /// Update creditor information display
    /// </summary>
    private void UpdateCreditorDisplay(QuotaData quota)
    {
        if (creditorNameText != null)
        {
            creditorNameText.text = quota.creditorName;
        }

        if (creditorPortrait != null && quota.creditorPortrait != null)
        {
            creditorPortrait.sprite = quota.creditorPortrait;
            creditorPortrait.enabled = true;
        }
    }

    /// <summary>
    /// Called when a new quota starts
    /// </summary>
    private void OnQuotaStarted(QuotaData quota)
    {
        UpdateAllDisplays();
        
        // Optionally show intro dialogue
        if (!string.IsNullOrEmpty(quota.introDialogue))
        {
            ShowDialogue(quota.introDialogue);
        }
    }

    /// <summary>
    /// Show dialogue (placeholder - implement your own dialogue system)
    /// </summary>
    private void ShowDialogue(string dialogue)
    {
        Debug.Log($"[Dialogue] {dialogue}");
        // TODO: Implement dialogue popup system
    }

    // ==================== PUBLIC METHODS ====================

    /// <summary>
    /// Manually refresh all displays
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateAllDisplays();
    }

    /// <summary>
    /// Show/hide the HUD
    /// </summary>
    public void SetHUDVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
