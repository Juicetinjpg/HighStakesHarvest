using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Modified BlackJackGameManager that integrates with MoneyManager.
/// This is an updated version of your existing BlackJackGameManager.
/// Replace the money handling code in your existing script with this approach.
/// </summary>
public class BlackJackGameManagerIntegrated : MonoBehaviour
{
    public Button deal;
    public Button hit;
    public Button stand;

    public PlayerScript playerscript;
    public PlayerScript dealerscript;

    public Text CashText;
    public Text HandText;
    public Text BetText;
    public Text DealerText;
    public GameObject hideCard;

    public GameObject WinScreen;
    public GameObject LoseScreen;
    public GameObject DrawScreen;

    private int pot = 0;
    private int betAmount = 20; // Default bet
    private bool playerHasStood = false;
    private bool roundEnded = false;

    private void Start()
    {
        // Remove all previous listeners
        deal.onClick.RemoveAllListeners();
        hit.onClick.RemoveAllListeners();
        stand.onClick.RemoveAllListeners();

        // Add listeners
        deal.onClick.AddListener(DealClicked);
        hit.onClick.AddListener(HitClicked);
        stand.onClick.AddListener(StandClicked);

        ResetUI();

        // Subscribe to money changes
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateCashDisplay;
        }
    }

    private void OnDestroy()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateCashDisplay;
        }
    }

    public void DealClicked()
    {
        // Check if player has enough money
        if (MoneyManager.Instance == null || !MoneyManager.Instance.HasEnoughMoney(betAmount))
        {
            Debug.LogWarning("Not enough money to play!");
            return;
        }

        ResetUI();

        deal.gameObject.SetActive(false);
        hit.gameObject.SetActive(true);
        stand.gameObject.SetActive(true);

        roundEnded = false;
        playerHasStood = false;

        DealerText.gameObject.SetActive(false);
        GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();

        playerscript.DealInitialHand();
        dealerscript.DealInitialHand();

        if (hideCard != null)
            hideCard.SetActive(true);

        // Deduct bet from MoneyManager
        pot = betAmount * 2; // Pot includes dealer's match
        MoneyManager.Instance.RemoveMoney(betAmount);

        UpdateUI();

        CheckBlackjack();
    }

    public void HitClicked()
    {
        if (roundEnded) return;

        playerscript.HitOneCard();
        UpdateUI();

        if (playerscript.handValue >= 21)
        {
            playerHasStood = true;
            StartCoroutine(DealerTurn());
        }
    }

    public void StandClicked()
    {
        if (roundEnded) return;

        playerHasStood = true;
        hit.gameObject.SetActive(false);
        stand.gameObject.SetActive(false);

        StartCoroutine(DealerTurn());
    }

    private IEnumerator DealerTurn()
    {
        if (hideCard != null)
            hideCard.SetActive(false);

        DealerText.gameObject.SetActive(true);

        yield return new WaitForSeconds(.5f);

        if (playerscript.handValue > 21)
        {
            DetermineRoundOutcome();
            yield break;
        }

        while (dealerscript.handValue < 17 && dealerscript.cardIndex < dealerscript.hand.Length)
        {
            dealerscript.HitOneCard();
            UpdateUI();
            yield return new WaitForSeconds(.5f);
        }

        yield return new WaitForSeconds(.5f);

        DetermineRoundOutcome();
    }

    private void CheckBlackjack()
    {
        if (dealerscript.handValue == 21)
        {
            playerHasStood = true;
            hit.gameObject.SetActive(false);
            stand.gameObject.SetActive(false);
            StartCoroutine(DealerTurnAfterBlackjack());
        }
        else if (playerscript.handValue == 21)
        {
            hit.gameObject.SetActive(false);
        }
    }

    private IEnumerator DealerTurnAfterBlackjack()
    {
        yield return new WaitForSeconds(.5f);
        StartCoroutine(DealerTurn());
    }

    private void DetermineRoundOutcome()
    {
        StartCoroutine(ShowOutcomeWithDelay());
    }

    private IEnumerator ShowOutcomeWithDelay()
    {
        yield return new WaitForSeconds(1.5f);

        bool playerBust = playerscript.handValue > 21;
        bool dealerBust = dealerscript.handValue > 21;

        if ((playerBust && dealerBust) || (playerscript.handValue == dealerscript.handValue))
        {
            // Draw - return bet
            DrawScreen.SetActive(true);
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(pot / 2);
            }
        }
        else if (playerBust || (!dealerBust && dealerscript.handValue > playerscript.handValue))
        {
            // Lose - no money back
            LoseScreen.SetActive(true);
        }
        else
        {
            // Win - get pot
            WinScreen.SetActive(true);
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(pot);
            }
        }

        hit.gameObject.SetActive(false);
        stand.gameObject.SetActive(false);
        deal.gameObject.SetActive(true);

        roundEnded = true;
    }

    private void UpdateUI()
    {
        HandText.text = "Hand: " + playerscript.handValue;
        DealerText.text = "Hand: " + dealerscript.handValue;
        BetText.text = "Bet: " + pot.ToString();
        UpdateCashDisplay(MoneyManager.Instance != null ? MoneyManager.Instance.GetMoney() : 0);
    }

    private void UpdateCashDisplay(int currentMoney)
    {
        CashText.text = "Cash: " + currentMoney.ToString();
    }

    private void ResetUI()
    {
        hit.gameObject.SetActive(false);
        stand.gameObject.SetActive(false);

        WinScreen.SetActive(false);
        LoseScreen.SetActive(false);
        DrawScreen.SetActive(false);

        if (hideCard != null)
            hideCard.SetActive(false);
    }
}