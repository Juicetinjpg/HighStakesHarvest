using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Displays quota completion or failure screens.
/// Shows results when a quota is completed or failed.
/// </summary>
public class QuotaResultScreen : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject successPanel;
    [SerializeField] private GameObject failurePanel;
    [SerializeField] private GameObject finalVictoryPanel;

    [Header("Success Screen Elements")]
    [SerializeField] private TextMeshProUGUI successTitleText;
    [SerializeField] private TextMeshProUGUI successMessageText;
    [SerializeField] private TextMeshProUGUI quotaPaidText;
    [SerializeField] private TextMeshProUGUI remainingMoneyText;
    [SerializeField] private Button continueButton;

    [Header("Failure Screen Elements")]
    [SerializeField] private TextMeshProUGUI failureTitleText;
    [SerializeField] private TextMeshProUGUI failureMessageText;
    [SerializeField] private TextMeshProUGUI debtOwedText;
    [SerializeField] private TextMeshProUGUI moneyEarnedText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Victory Screen Elements")]
    [SerializeField] private TextMeshProUGUI victoryTitleText;
    [SerializeField] private TextMeshProUGUI victoryMessageText;
    [SerializeField] private Button victoryMenuButton;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string farmSceneName = "FarmScene";

    void Start()
    {
        // Hide all panels initially
        if (successPanel != null) successPanel.SetActive(false);
        if (failurePanel != null) failurePanel.SetActive(false);
        if (finalVictoryPanel != null) finalVictoryPanel.SetActive(false);

        // Subscribe to quota events
        if (QuotaManager.Instance != null)
        {
            QuotaManager.Instance.OnQuotaCompleted += ShowSuccessScreen;
            QuotaManager.Instance.OnQuotaFailed += ShowFailureScreen;
        }

        // Setup button listeners
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (victoryMenuButton != null)
            victoryMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    void OnDestroy()
    {
        if (QuotaManager.Instance != null)
        {
            QuotaManager.Instance.OnQuotaCompleted -= ShowSuccessScreen;
            QuotaManager.Instance.OnQuotaFailed -= ShowFailureScreen;
        }
    }

    /// <summary>
    /// Show success screen when quota is completed
    /// </summary>
    private void ShowSuccessScreen(QuotaData completedQuota)
    {
        if (successPanel == null) return;

        // Check if this was the final quota
        if (QuotaManager.Instance.GetCurrentQuotaIndex() >= QuotaManager.Instance.GetTotalQuotas() - 1)
        {
            ShowFinalVictoryScreen();
            return;
        }

        // Pause game
        Time.timeScale = 0f;

        // Show panel
        successPanel.SetActive(true);

        // Update text
        if (successTitleText != null)
        {
            successTitleText.text = $"Quota Paid to {completedQuota.creditorName}!";
        }

        if (successMessageText != null)
        {
            successMessageText.text = completedQuota.successDialogue;
        }

        if (quotaPaidText != null)
        {
            quotaPaidText.text = $"Paid: ${completedQuota.quotaAmount}";
        }

        if (remainingMoneyText != null && MoneyManager.Instance != null)
        {
            int remaining = MoneyManager.Instance.GetMoney();
            remainingMoneyText.text = $"Remaining: ${remaining}";
        }

        Debug.Log($"Success screen shown for {completedQuota.creditorName}");
    }

    /// <summary>
    /// Show failure screen when quota is not met
    /// </summary>
    private void ShowFailureScreen(QuotaData failedQuota)
    {
        if (failurePanel == null) return;

        // Pause game
        Time.timeScale = 0f;

        // Show panel
        failurePanel.SetActive(true);

        // Update text
        if (failureTitleText != null)
        {
            failureTitleText.text = "Quota Failed!";
        }

        if (failureMessageText != null)
        {
            failureMessageText.text = failedQuota.failureDialogue;
        }

        if (debtOwedText != null)
        {
            debtOwedText.text = $"Owed: ${failedQuota.quotaAmount}";
        }

        if (moneyEarnedText != null && QuotaManager.Instance != null)
        {
            int earned = QuotaManager.Instance.GetMoneyProgressTowardQuota();
            moneyEarnedText.text = $"Earned: ${earned}";
        }

        Debug.Log($"Failure screen shown for {failedQuota.creditorName}");
    }

    /// <summary>
    /// Show final victory screen when all quotas are complete
    /// </summary>
    private void ShowFinalVictoryScreen()
    {
        if (finalVictoryPanel == null) return;

        // Pause game
        Time.timeScale = 0f;

        // Show panel
        finalVictoryPanel.SetActive(true);

        if (victoryTitleText != null)
        {
            victoryTitleText.text = "ALL DEBTS PAID!";
        }

        if (victoryMessageText != null)
        {
            victoryMessageText.text = "You've paid back everyone and saved your farm! " +
                                     "John Pork is finally free from debt and can rebuild " +
                                     "her relationships with those she wronged.";
        }

        Debug.Log("Victory! All quotas completed!");
    }

    // ==================== BUTTON CALLBACKS ====================

    private void OnContinueClicked()
    {
        // Resume game
        Time.timeScale = 1f;

        // Hide success panel
        if (successPanel != null)
            successPanel.SetActive(false);

        // The next quota should already be started by QuotaManager
        Debug.Log("Continuing to next quota...");
    }

    private void OnRetryClicked()
    {
        // Resume time
        Time.timeScale = 1f;

        // Restart the game
        if (FindFirstObjectByType<GameInitializer>() != null)
        {
            FindFirstObjectByType<GameInitializer>().ResetGame();
        }

        // Reload farm scene
        SceneManager.LoadScene(farmSceneName);
    }

    private void OnMainMenuClicked()
    {
        // Resume time
        Time.timeScale = 1f;

        // Load main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ==================== PUBLIC METHODS ====================

    /// <summary>
    /// Manually show success screen (for testing)
    /// </summary>
    public void TestSuccessScreen()
    {
        if (QuotaManager.Instance != null)
        {
            QuotaData currentQuota = QuotaManager.Instance.GetCurrentQuota();
            if (currentQuota != null)
            {
                ShowSuccessScreen(currentQuota);
            }
        }
    }

    /// <summary>
    /// Manually show failure screen (for testing)
    /// </summary>
    public void TestFailureScreen()
    {
        if (QuotaManager.Instance != null)
        {
            QuotaData currentQuota = QuotaManager.Instance.GetCurrentQuota();
            if (currentQuota != null)
            {
                ShowFailureScreen(currentQuota);
            }
        }
    }
}
