using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Game Entry System - Players enter the scene and choose to pay to play
/// Place this in Slots and Blackjack scenes
/// </summary>
public class GameEntrySystem : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private GameType gameType = GameType.Slots;
    [SerializeField] private int entryFee = 70; // 70 for Slots, 150 for Blackjack
    [SerializeField] private string gameName = "Slots";
    [SerializeField] private string lobbySceneName = "CasinoLobby";
    
    [Header("UI Panels")]
    [SerializeField] private GameObject entryPanel; // Main entry panel
    [SerializeField] private GameObject gameplayUI; // Actual game UI
    [SerializeField] private GameObject insufficientFundsPanel;
    
    [Header("Entry Panel UI")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text entryFeeText;
    [SerializeField] private Text playerMoneyText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button returnToLobbyButton;
    
    [Header("Insufficient Funds UI")]
    [SerializeField] private Text insufficientFundsText;
    [SerializeField] private Button backToLobbyButton;
    
    [Header("Game Controllers")]
    [SerializeField] private MonoBehaviour slotsController; // Your SlotsGameController
    [SerializeField] private MonoBehaviour blackjackController; // Your BlackjackMoneyIntegration
    
    [Header("Visual Settings")]
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private float fadeInDuration = 0.5f;
    
    private bool hasAccessToGame = false;
    private int sessionPayments = 0; // Track payments in this session
    
    public enum GameType
    {
        Slots,
        Blackjack
    }
    
    void Start()
    {
        InitializeEntrySystem();
        
        // Subscribe to money changes
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
        }
    }
    
    void OnDestroy()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
        }
    }
    
    private void InitializeEntrySystem()
    {
        // Set game-specific values
        if (gameType == GameType.Slots)
        {
            entryFee = 70;
            gameName = "Slots";
            if (titleText) titleText.text = "WELCOME TO SLOTS";
            if (descriptionText) descriptionText.text = "Try your luck on our premium slot machines!\n\n• 3-Reel Classic Slots\n• Multiple Winning Combinations\n• Bonus Multipliers\n• Progressive Jackpots";
        }
        else if (gameType == GameType.Blackjack)
        {
            entryFee = 150;
            gameName = "Blackjack";
            if (titleText) titleText.text = "WELCOME TO BLACKJACK";
            if (descriptionText) descriptionText.text = "Play against the dealer in classic Blackjack!\n\n• Professional Table\n• Double Down Available\n• Insurance Bets\n• Blackjack Pays 3:2";
        }
        
        // Setup UI
        ShowEntryPanel();
        HideGameplay();
        
        // Setup buttons
        if (playButton) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (returnToLobbyButton) returnToLobbyButton.onClick.AddListener(ReturnToLobby);
        if (backToLobbyButton) backToLobbyButton.onClick.AddListener(ReturnToLobby);
        
        // Update displays
        UpdateMoneyDisplay(MoneyManager.Instance != null ? MoneyManager.Instance.GetMoney() : 0);
        UpdateEntryFeeDisplay();
        
        // Start with fade in effect
        StartCoroutine(FadeInEffect());
    }
    
    private void ShowEntryPanel()
    {
        if (entryPanel) entryPanel.SetActive(true);
        if (insufficientFundsPanel) insufficientFundsPanel.SetActive(false);
        
        // Update button state based on money
        UpdatePlayButtonState();
    }
    
    private void HideGameplay()
    {
        if (gameplayUI) gameplayUI.SetActive(false);
        
        // Disable game controllers initially
        if (slotsController) slotsController.enabled = false;
        if (blackjackController) blackjackController.enabled = false;
    }
    
    private void OnPlayButtonClicked()
    {
        // Check if player has enough money
        if (MoneyManager.Instance == null)
        {
            Debug.LogError("MoneyManager not found!");
            return;
        }
        
        int currentMoney = MoneyManager.Instance.GetMoney();
        
        if (currentMoney < entryFee)
        {
            ShowInsufficientFunds(currentMoney);
            return;
        }
        
        // Process payment
        if (MoneyManager.Instance.RemoveMoney(entryFee))
        {
            // Payment successful
            StartCoroutine(ProcessPaymentAndStartGame());
        }
        else
        {
            ShowInsufficientFunds(currentMoney);
        }
    }
    
    private IEnumerator ProcessPaymentAndStartGame()
    {
        sessionPayments++;
        
        // Show payment confirmation
        if (titleText) titleText.text = "PAYMENT SUCCESSFUL!";
        if (descriptionText) 
        {
            descriptionText.text = $"Paid ${entryFee}\n\nStarting {gameName}...";
            descriptionText.color = Color.green;
        }
        
        // Disable buttons during transition
        if (playButton) playButton.interactable = false;
        if (returnToLobbyButton) returnToLobbyButton.interactable = false;
        
        // Wait for player to see confirmation
        yield return new WaitForSeconds(1.5f);
        
        // Fade out entry panel
        yield return StartCoroutine(FadeOutPanel());
        
        // Start the game
        StartGame();
    }
    
    private void StartGame()
    {
        hasAccessToGame = true;
        
        // Hide entry panel
        if (entryPanel) entryPanel.SetActive(false);
        
        // Show game UI
        if (gameplayUI) gameplayUI.SetActive(true);
        
        // Enable appropriate game controller
        if (gameType == GameType.Slots && slotsController)
        {
            slotsController.enabled = true;
            
            // If it has a method to initialize, call it
            var initMethod = slotsController.GetType().GetMethod("InitializeGame");
            if (initMethod != null)
            {
                initMethod.Invoke(slotsController, null);
            }
        }
        else if (gameType == GameType.Blackjack && blackjackController)
        {
            blackjackController.enabled = true;
            
            // If it has a method to initialize, call it
            var initMethod = blackjackController.GetType().GetMethod("InitializeGame");
            if (initMethod != null)
            {
                initMethod.Invoke(blackjackController, null);
            }
        }
        
        Debug.Log($"{gameName} started! Player has access to play.");
    }
    
    private void ShowInsufficientFunds(int currentMoney)
    {
        if (insufficientFundsPanel)
        {
            insufficientFundsPanel.SetActive(true);
            if (entryPanel) entryPanel.SetActive(false);
            
            if (insufficientFundsText)
            {
                int needed = entryFee - currentMoney;
                insufficientFundsText.text = $"INSUFFICIENT FUNDS\n\n" +
                    $"Entry Fee: ${entryFee}\n" +
                    $"Your Money: ${currentMoney}\n" +
                    $"You need ${needed} more\n\n" +
                    $"Return to the lobby and earn more money!";
            }
        }
    }
    
    private void UpdateMoneyDisplay(int currentMoney)
    {
        if (playerMoneyText)
        {
            playerMoneyText.text = $"Your Money: ${currentMoney}";
            
            // Color code based on ability to afford
            if (currentMoney >= entryFee)
            {
                playerMoneyText.color = Color.green;
            }
            else
            {
                playerMoneyText.color = Color.red;
            }
        }
        
        UpdatePlayButtonState();
    }
    
    private void UpdateEntryFeeDisplay()
    {
        if (entryFeeText)
        {
            entryFeeText.text = $"Entry Fee: ${entryFee}";
        }
    }
    
    private void UpdatePlayButtonState()
    {
        if (playButton && MoneyManager.Instance != null)
        {
            int currentMoney = MoneyManager.Instance.GetMoney();
            playButton.interactable = currentMoney >= entryFee;
            
            // Update button text
            Text buttonText = playButton.GetComponentInChildren<Text>();
            if (buttonText)
            {
                if (currentMoney >= entryFee)
                {
                    buttonText.text = $"PAY ${entryFee} TO PLAY";
                    buttonText.color = Color.white;
                }
                else
                {
                    buttonText.text = "INSUFFICIENT FUNDS";
                    buttonText.color = Color.gray;
                }
            }
        }
    }
    
    private IEnumerator FadeInEffect()
    {
        if (backgroundOverlay)
        {
            Color startColor = backgroundOverlay.color;
            startColor.a = 0;
            backgroundOverlay.color = startColor;
            
            float elapsed = 0;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                startColor.a = Mathf.Lerp(0, 0.9f, elapsed / fadeInDuration);
                backgroundOverlay.color = startColor;
                yield return null;
            }
        }
    }
    
    private IEnumerator FadeOutPanel()
    {
        CanvasGroup canvasGroup = entryPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = entryPanel.AddComponent<CanvasGroup>();
        }
        
        float elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeInDuration);
            yield return null;
        }
    }
    
    private void ReturnToLobby()
    {
        if (!string.IsNullOrEmpty(lobbySceneName))
        {
            SceneManager.LoadScene(lobbySceneName);
        }
    }
    
    /// <summary>
    /// Call this to show the entry panel again (useful if player wants to leave the game)
    /// </summary>
    public void ShowEntryPanelAgain()
    {
        hasAccessToGame = false;
        ShowEntryPanel();
        HideGameplay();
        
        // Reset UI text
        if (titleText) titleText.text = $"PLAY AGAIN?";
        if (descriptionText) 
        {
            descriptionText.text = $"Your session has ended.\nPay to play another round of {gameName}!";
            descriptionText.color = Color.white;
        }
    }
    
    /// <summary>
    /// Get whether player currently has access to the game
    /// </summary>
    public bool HasGameAccess()
    {
        return hasAccessToGame;
    }
    
    /// <summary>
    /// Get total payments made in this session
    /// </summary>
    public int GetSessionPayments()
    {
        return sessionPayments;
    }
}