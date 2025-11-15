using UnityEngine;
using UnityEngine.UI;      // For Button
using TMPro;               // For TMP_Text
using System.Collections;

/// <summary>
/// Handles payment for Slots when pulling the lever.
/// Attach this to the same GameObject (or a manager) and hook it up from SlotController.
/// It ONLY handles money + UI; SlotController still controls spinning/rows.
/// </summary>
public class SlotsPaymentHandler : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int spinCost = 70;
    [SerializeField] private bool firstSpinFree = false;

    [Header("UI Display (TextMeshPro)")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text messageText;

    [Header("UI Panels")]
    [SerializeField] private GameObject insufficientFundsPanel;
    [SerializeField] private Button backToLobbyButton;

    private bool hasPaidOnce = false;

    void Start()
    {
        // Setup back button
        if (backToLobbyButton != null)
        {
            backToLobbyButton.onClick.RemoveAllListeners();
            backToLobbyButton.onClick.AddListener(ReturnToLobby);
        }

        // Hide insufficient funds panel
        if (insufficientFundsPanel != null)
        {
            insufficientFundsPanel.SetActive(false);
        }

        // Subscribe to money changes
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(MoneyManager.Instance.GetMoney());
        }

        // Update cost display
        if (costText != null)
        {
            costText.text = $"Cost per Spin: ${spinCost}";
        }

        // First spin message
        if (firstSpinFree && !hasPaidOnce)
        {
            ShowMessage("First spin is FREE! Pull the lever!", Color.cyan);
        }
        else
        {
            ShowMessage($"Pull the lever to spin for ${spinCost}!", Color.white);
        }
    }

    void OnDestroy()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
        }
    }

    /// <summary>
    /// Called by SlotController before starting a spin.
    /// Returns true if the spin is allowed (paid or free), false if blocked.
    /// </summary>
    public bool TryPayForSpin()
    {
        // First spin can be free
        if (firstSpinFree && !hasPaidOnce)
        {
            hasPaidOnce = true;
            ShowMessage("FREE SPIN!", Color.green);
            return true;
        }

        // Check if player has enough money
        if (MoneyManager.Instance == null || !MoneyManager.Instance.HasEnoughMoney(spinCost))
        {
            ShowInsufficientFunds();
            return false;
        }

        // Charge the player
        if (MoneyManager.Instance.RemoveMoney(spinCost))
        {
            hasPaidOnce = true;
            ShowMessage($"Paid ${spinCost} - Good luck!", Color.white);
            return true;
        }

        ShowInsufficientFunds();
        return false;
    }

    private void ShowInsufficientFunds()
    {
        if (insufficientFundsPanel != null)
        {
            insufficientFundsPanel.SetActive(true);

            // If your panel has a TMP_Text child, update it
            TMP_Text panelText = insufficientFundsPanel.GetComponentInChildren<TMP_Text>();
            if (panelText != null)
            {
                int currentMoney = MoneyManager.Instance != null ? MoneyManager.Instance.GetMoney() : 0;
                int needed = spinCost - currentMoney;
                panelText.text = $"INSUFFICIENT FUNDS\n\nYou need ${needed} more to spin.\n\nReturn to lobby to earn more money!";
            }
        }

        int money = MoneyManager.Instance != null ? MoneyManager.Instance.GetMoney() : 0;
        ShowMessage($"Need ${spinCost} to play! You have ${money}", Color.red);
    }

    private void UpdateMoneyDisplay(int currentMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: ${currentMoney}";
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
}
