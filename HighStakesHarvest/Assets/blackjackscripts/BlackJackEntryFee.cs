using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Simple entry fee system for Blackjack - attach to any GameObject in the scene
/// This intercepts the DEAL/SHUFFLE button to handle payment
/// </summary>
public class BlackjackEntryFee : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int entryFee = 150;
    [SerializeField] private int handsPerEntry = 5; // How many hands they can play per payment

    [Header("References")]
    [SerializeField] private Button dealButton; // Your SHUFFLE/DEAL button
    [SerializeField] private BlackJackGameManager gameManager;
    [SerializeField] private Text tableEntryText; // The "Table Entry: $150" text

    [Header("Payment UI")]
    [SerializeField] private GameObject paymentPopup; // Simple popup panel
    [SerializeField] private Text paymentText;
    [SerializeField] private Button payButton;
    [SerializeField] private Button cancelButton;

    private bool hasPaid = false;
    private int handsRemaining = 0;
    private System.Action originalDealAction;

    void Start()
    {
        // Create payment popup if it doesn't exist
        if (paymentPopup == null)
        {
            CreateSimplePaymentUI();
        }
        else
        {
            paymentPopup.SetActive(false);
        }

        // Intercept the DEAL button
        if (dealButton != null && gameManager != null)
        {
            // Store the original action
            dealButton.onClick.RemoveAllListeners();
            dealButton.onClick.AddListener(OnDealButtonPressed);
        }

        // Setup payment buttons
        if (payButton != null)
        {
            payButton.onClick.RemoveAllListeners();
            payButton.onClick.AddListener(ProcessPayment);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelPayment);
        }

        UpdateDisplay();
    }

    private void OnDealButtonPressed()
    {
        // Check if player has access
        if (hasPaid && handsRemaining > 0)
        {
            // They have access, start the game
            handsRemaining--;
            gameManager.DealClicked();
            
            // Update display
            UpdateDisplay();
            
            // Check if this was their last hand
            if (handsRemaining == 0)
            {
                hasPaid = false;
                StartCoroutine(ShowExpiredMessage());
            }
        }
        else
        {
            // Need to pay first
            ShowPaymentPopup();
        }
    }

    private void ShowPaymentPopup()
    {
        if (paymentPopup != null)
        {
            paymentPopup.SetActive(true);

            // Check if player can afford it
            int playerMoney = GetPlayerMoney();
            
            if (paymentText != null)
            {
                paymentText.text = $"Pay ${entryFee} for {handsPerEntry} hands?\n\nYour money: ${playerMoney}";
            }

            if (payButton != null)
            {
                payButton.interactable = playerMoney >= entryFee;
                Text btnText = payButton.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = playerMoney >= entryFee ? $"Pay ${entryFee}" : "Not Enough";
                }
            }
        }
        else
        {
            Debug.LogWarning("No payment popup! Creating one...");
            CreateSimplePaymentUI();
            ShowPaymentPopup();
        }
    }

    private void ProcessPayment()
    {
        int playerMoney = GetPlayerMoney();
        
        if (playerMoney < entryFee)
        {
            Debug.Log("Not enough money!");
            return;
        }

        // Remove money
        if (MoneyManager.Instance != null)
        {
            if (MoneyManager.Instance.RemoveMoney(entryFee))
            {
                PaymentSuccess();
            }
        }
        else if (gameManager != null && gameManager.playerscript != null)
        {
            // Fallback to player script
            gameManager.playerscript.AdjustMoney(-entryFee);
            PaymentSuccess();
        }
    }

    private void PaymentSuccess()
    {
        hasPaid = true;
        handsRemaining = handsPerEntry;
        
        if (paymentPopup != null)
        {
            paymentPopup.SetActive(false);
        }

        UpdateDisplay();
        
        // Now actually start the game
        gameManager.DealClicked();
        handsRemaining--;
        UpdateDisplay();
    }

    private void CancelPayment()
    {
        if (paymentPopup != null)
        {
            paymentPopup.SetActive(false);
        }
    }

    private int GetPlayerMoney()
    {
        if (MoneyManager.Instance != null)
        {
            return MoneyManager.Instance.GetMoney();
        }
        else if (gameManager != null && gameManager.playerscript != null)
        {
            return gameManager.playerscript.GetMoney();
        }
        return 0;
    }

    private void UpdateDisplay()
    {
        if (tableEntryText != null)
        {
            if (hasPaid)
            {
                tableEntryText.text = $"Hands Remaining: {handsRemaining}";
                tableEntryText.color = Color.green;
            }
            else
            {
                tableEntryText.text = $"Table Entry: ${entryFee}";
                tableEntryText.color = Color.white;
            }
        }
    }

    private IEnumerator ShowExpiredMessage()
    {
        if (tableEntryText != null)
        {
            tableEntryText.text = "Session Expired - Pay Again";
            tableEntryText.color = Color.yellow;
            yield return new WaitForSeconds(3f);
            UpdateDisplay();
        }
    }

    private void CreateSimplePaymentUI()
    {
        // Create a simple payment popup programmatically
        GameObject canvas = GameObject.Find("Canvas") ?? GameObject.Find("UI Canvas");
        if (canvas == null)
        {
            Debug.LogError("No Canvas found for payment UI!");
            return;
        }

        // Create popup panel
        paymentPopup = new GameObject("PaymentPopup");
        paymentPopup.transform.SetParent(canvas.transform, false);
        
        Image bg = paymentPopup.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.9f);
        
        RectTransform rt = paymentPopup.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        // Create inner panel
        GameObject innerPanel = new GameObject("Panel");
        innerPanel.transform.SetParent(paymentPopup.transform, false);
        
        Image panelBg = innerPanel.AddComponent<Image>();
        panelBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        RectTransform panelRt = innerPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.3f, 0.3f);
        panelRt.anchorMax = new Vector2(0.7f, 0.7f);
        panelRt.sizeDelta = Vector2.zero;
        panelRt.anchoredPosition = Vector2.zero;

        // Create text
        GameObject textObj = new GameObject("PaymentText");
        textObj.transform.SetParent(innerPanel.transform, false);
        
        paymentText = textObj.AddComponent<Text>();
        paymentText.text = $"Pay ${entryFee} for {handsPerEntry} hands?";
        paymentText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        paymentText.fontSize = 24;
        paymentText.color = Color.white;
        paymentText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.1f, 0.4f);
        textRt.anchorMax = new Vector2(0.9f, 0.8f);
        textRt.sizeDelta = Vector2.zero;
        textRt.anchoredPosition = Vector2.zero;

        // Create Pay button
        GameObject payBtnObj = new GameObject("PayButton");
        payBtnObj.transform.SetParent(innerPanel.transform, false);
        
        payButton = payBtnObj.AddComponent<Button>();
        Image payBtnImg = payBtnObj.AddComponent<Image>();
        payBtnImg.color = Color.green;
        
        GameObject payBtnText = new GameObject("Text");
        payBtnText.transform.SetParent(payBtnObj.transform, false);
        Text payText = payBtnText.AddComponent<Text>();
        payText.text = $"Pay ${entryFee}";
        payText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        payText.fontSize = 20;
        payText.color = Color.white;
        payText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform payBtnRt = payBtnObj.GetComponent<RectTransform>();
        payBtnRt.anchorMin = new Vector2(0.1f, 0.1f);
        payBtnRt.anchorMax = new Vector2(0.45f, 0.3f);
        payBtnRt.sizeDelta = Vector2.zero;
        
        RectTransform payTextRt = payBtnText.GetComponent<RectTransform>();
        payTextRt.anchorMin = Vector2.zero;
        payTextRt.anchorMax = Vector2.one;
        payTextRt.sizeDelta = Vector2.zero;
        payTextRt.anchoredPosition = Vector2.zero;

        // Create Cancel button
        GameObject cancelBtnObj = new GameObject("CancelButton");
        cancelBtnObj.transform.SetParent(innerPanel.transform, false);
        
        cancelButton = cancelBtnObj.AddComponent<Button>();
        Image cancelBtnImg = cancelBtnObj.AddComponent<Image>();
        cancelBtnImg.color = Color.red;
        
        GameObject cancelBtnText = new GameObject("Text");
        cancelBtnText.transform.SetParent(cancelBtnObj.transform, false);
        Text cancelText = cancelBtnText.AddComponent<Text>();
        cancelText.text = "Cancel";
        cancelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        cancelText.fontSize = 20;
        cancelText.color = Color.white;
        cancelText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform cancelBtnRt = cancelBtnObj.GetComponent<RectTransform>();
        cancelBtnRt.anchorMin = new Vector2(0.55f, 0.1f);
        cancelBtnRt.anchorMax = new Vector2(0.9f, 0.3f);
        cancelBtnRt.sizeDelta = Vector2.zero;
        
        RectTransform cancelTextRt = cancelBtnText.GetComponent<RectTransform>();
        cancelTextRt.anchorMin = Vector2.zero;
        cancelTextRt.anchorMax = Vector2.one;
        cancelTextRt.sizeDelta = Vector2.zero;
        cancelTextRt.anchoredPosition = Vector2.zero;

        // Setup button clicks
        payButton.onClick.AddListener(ProcessPayment);
        cancelButton.onClick.AddListener(CancelPayment);

        // Hide initially
        paymentPopup.SetActive(false);
    }
}
