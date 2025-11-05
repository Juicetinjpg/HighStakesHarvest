using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlackJackGameManager : MonoBehaviour
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
    }

    // Start a new round
    public void DealClicked()
    {
        ResetUI();

        deal.gameObject.SetActive(false); // hide Deal when round starts
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

        pot = 40;
        playerscript.AdjustMoney(-20);

        UpdateUI();

        CheckBlackjack(); // handle instant 21
    }

    public void HitClicked()
    {
        if (roundEnded) return;

        playerscript.HitOneCard();
        UpdateUI();

        if (playerscript.handValue >= 21)
        {
            playerHasStood = true; // auto-stand if bust or 21
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

    // Dealer plays automatically
    private IEnumerator DealerTurn()
    {
        // Reveal the dealer's hidden card
        if (hideCard != null)
            hideCard.SetActive(false);

        DealerText.gameObject.SetActive(true);

        // Give a moment to see the revealed card
        yield return new WaitForSeconds(.5f);

        // If player busted, dealer doesn't need to draw
        if (playerscript.handValue > 21)
        {
            DetermineRoundOutcome();  // Remove roundEnded = true from here
            yield break;
        }

        // Dealer draws until 17 or higher
        while (dealerscript.handValue < 17 && dealerscript.cardIndex < dealerscript.hand.Length)
        {
            dealerscript.HitOneCard();
            UpdateUI();
            yield return new WaitForSeconds(.5f);
        }

        // Add delay after dealer finishes drawing before showing result
        yield return new WaitForSeconds(.5f);

        DetermineRoundOutcome();  // Remove roundEnded = true from here too
    }

    private void CheckBlackjack()
    {
        // Only end immediately if DEALER has blackjack
        if (dealerscript.handValue == 21)
        {
            playerHasStood = true;
            hit.gameObject.SetActive(false);
            stand.gameObject.SetActive(false);
            StartCoroutine(DealerTurnAfterBlackjack());
        }
        // If only player has blackjack, they can't hit anyway (already at 21)
        // but let them click stand to see the result
        else if (playerscript.handValue == 21)
        {
            // Player has blackjack but dealer doesn't
            // Disable hit (can't improve on 21) but leave stand active
            hit.gameObject.SetActive(false);
            // Player must click stand to proceed
        }
    }

    private IEnumerator DealerTurnAfterBlackjack()
    {
        yield return new WaitForSeconds(.5f); // Show the blackjack for 2 seconds
        StartCoroutine(DealerTurn());
    }
    private IEnumerator DealerTurnWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(DealerTurn());
    }

    private void DetermineRoundOutcome()
    {
        StartCoroutine(ShowOutcomeWithDelay());
    }

    private IEnumerator ShowOutcomeWithDelay()
    {
        // Delay before showing any outcome screen
        yield return new WaitForSeconds(1.5f);

        bool playerBust = playerscript.handValue > 21;
        bool dealerBust = dealerscript.handValue > 21;

        if ((playerBust && dealerBust) || (playerscript.handValue == dealerscript.handValue))
        {
            DrawScreen.SetActive(true);
            playerscript.AdjustMoney(pot / 2);
        }
        else if (playerBust || (!dealerBust && dealerscript.handValue > playerscript.handValue))
        {
            LoseScreen.SetActive(true);
        }
        else
        {
            WinScreen.SetActive(true);
            playerscript.AdjustMoney(pot);
        }

        hit.gameObject.SetActive(false);
        stand.gameObject.SetActive(false);
        deal.gameObject.SetActive(true);

        roundEnded = true;
    }
    private bool IsBlackjack(PlayerScript player)
    {
        return player.handValue == 21 && player.cardIndex == 2;
    }

    private void UpdateUI()
    {
        HandText.text = "Hand: " + playerscript.handValue;
        DealerText.text = "Hand: " + dealerscript.handValue;
        CashText.text = "Cash: " + playerscript.GetMoney().ToString();
        BetText.text = "Bet: " + pot.ToString();
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
