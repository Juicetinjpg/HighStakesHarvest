using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles payment/session access for Blackjack.
/// Attach this to a GameObject in the Blackjack scene and assign it to BlackJackGameManager.
/// </summary>
public class BlackjackPaymentHandler : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int entryFee = 150;
    [SerializeField] private bool firstGameFree = false;
    [SerializeField] private int gamesPerEntry = 5; // How many hands per paid entry (0 = unlimited)

    [Header("Blackjack References")]
    [SerializeField] private BlackJackGameManager gameManager; // optional reference

    [Header("UI Display")]
    [SerializeField] private Text moneyText;
    [SerializeField] private Text entryFeeText;
    [SerializeField] private Text messageText;

    [Header("UI Panels")]
    [SerializeField] private GameObject paymentPanel;
    [SerializeField] private GameObject insufficientFundsPanel;
    [SerializeField] private Button payButton;
    [SerializeField] private Button backToLobbyButton;

    private bool hasAccessToTable = false;
    private bool hasUsedFreeGame = false;
    private int gamesPlayedThisSession = 0;

    private int totalWinnings = 0;
    private int totalLosses = 0;

    void Start()
    {
        SetupButtons();

        // Subscribe to money changes
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(MoneyManager.Instance.GetMoney());
        }

        // Show initial state
        if (!hasAccessToTable)
        {
            ShowMessage($"Pay ${entryFee} to access the Blackjack table.", Color.yellow);
        }

        // Update fee display
        if (entryFeeText != null)
        {
            entryFeeText.text = $"Table Entry: ${entryFee}";
        }
    }

    void OnDestroy()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
        }
    }

    private void SetupButtons()
    {
        // Setup payment button
        if (payButton != null)
        {
            payButton.onClick.RemoveAllListeners();
            payButton.onClick.AddListener(PayEntryFee);
        }

        // Setup back button
        if (backToLobbyButton != null)
        {
            backToLobbyButton.onClick.RemoveAllListeners();
            backToLobbyButton.onClick.AddListener(ReturnToLobby);
        }

        // Hide panels initially
        if (paymentPanel != null) paymentPanel.SetActive(false);
        if (insufficientFundsPanel != null) insufficientFundsPanel.SetActive(false);
    }

    /// <summary>
    /// Called by BlackJackGameManager.DealClicked before starting a new hand.
    /// Returns true if the player has/gets access, false if the game should not start.
    /// </summary>
    public bool TryPayEntry()
    {
        // Already has access this session
        if (hasAccessToTable)
        {
            return true;
        }

        // First game can be free
        if (firstGameFree && !hasUsedFreeGame)
        {
            hasUsedFreeGame = true;
            hasAccessToTable = true;
            gamesPlayedThisSession = 0;
            ShowMessage("FREE GAME! Table access granted for this session.", Color.cyan);
            return true;
        }

        // Otherwise, show payment UI and block the game start
        ShowPaymentRequired();
        return false;
    }

    private void ShowPaymentRequired()
    {
        if (paymentPanel != null)
        {
            paymentPanel.SetActive(true);

            Text panelText = paymentPanel.GetComponentInChildren<Text>();
            if (panelText != null)
            {
                panelText.text = $"BLACKJACK TABLE\n\nEntry Fee: ${entryFee}\n\nPay to access the table and play multiple hands!";
            }

            if (payButton != null)
            {
                int currentMoney = MoneyManager.Instance != null ? MoneyManager.Instance.GetMoney() : 0;
                payButton.interactable = currentMoney >= entryFee;

                Text buttonText = payButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = currentMoney >= entryFee ? $"Pay ${entryFee}" : "Insufficient Funds";
                }
            }
        }

        ShowMessage($"Pay ${entryFee} to access the Blackjack table.", Color.yellow);
    }

    private void PayEntryFee()
    {
        if (MoneyManager.Instance == null || !MoneyManager.Instance.HasEnoughMoney(entryFee))
        {
            ShowInsufficientFunds();
            return;
        }

        if (MoneyManager.Instance.RemoveMoney(entryFee))
        {
            hasAccessToTable = true;
            gamesPlayedThisSession = 0;

            if (paymentPanel != null)
            {
                paymentPanel.SetActive(false);
            }

            ShowMessage($"Paid ${entryFee} - Table access granted! Press DEAL to start.", Color.green);
        }
        else
        {
            ShowInsufficientFunds();
        }
    }

    private void ShowInsufficientFunds()
    {
        if (insufficientFundsPanel != null)
        {
            insufficientFundsPanel.SetActive(true);

            Text panelText = insufficientFundsPanel.GetComponentInChildren<Text>();
            if (panelText != null)
            {
                int currentMoney = MoneyManager.Instance != null ? MoneyManager.Instance.GetMoney() : 0;
                int needed = entryFee - currentMoney;
                panelText.text = $"INSUFFICIENT FUNDS\n\nYou need ${needed} more to play Blackjack.\n\nReturn to lobby to earn more money!";
            }
        }

        if (paymentPanel != null)
        {
            paymentPanel.SetActive(false);
        }
    }

    private void UpdateMoneyDisplay(int currentMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: ${currentMoney}";
        }

        if (paymentPanel != null && paymentPanel.activeSelf)
        {
            if (payButton != null)
            {
                payButton.interactable = currentMoney >= entryFee;
            }
        }
    }

    private void ShowMessage(string message, Color color)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;

            StopCoroutine("HideMessage");
            StartCoroutine("HideMessage");
        }

        Debug.Log(message);
    }

    private IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(3f);
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    private void ReturnToLobby()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("CasinoScene");
    }

    /// <summary>
    /// Call this from BlackJackGameManager when a round ends.
    /// </summary>
    public void OnGameResult(bool won, int amount)
    {
        if (won)
        {
            totalWinnings += amount;
            ShowMessage($"Won ${amount}!", Color.green);
        }
        else
        {
            totalLosses += amount;
            ShowMessage($"Lost ${amount}.", Color.red);
        }

        gamesPlayedThisSession++;

        if (gamesPerEntry > 0 && gamesPlayedThisSession >= gamesPerEntry)
        {
            hasAccessToTable = false;
            gamesPlayedThisSession = 0;
            ShowMessage("Table session ended. Pay again to continue playing.", Color.yellow);
        }

        int netResult = totalWinnings - totalLosses;
        Debug.Log($"Blackjack session: Won ${totalWinnings}, Lost ${totalLosses}, Net: {(netResult >= 0 ? "+" : "")}{netResult}");
    }

    public bool HasTableAccess()
    {
        return hasAccessToTable;
    }
}
