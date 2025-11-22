using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


// - PlayerScript (handles DealInitialHand, HitOneCard, handValue, cardIndex, AdjustMoney, GetMoney())
// - BuffManager with methods: AddBuff(ScriptableBuff), HasBuff(ScriptableBuff)
// - ScriptableBuff scriptable objects used as buff definitions.

public class BlackJackGameManager : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button deal;
    public Button hit;
    public Button stand;

    [Header("Player / Dealer")]
    public PlayerScript playerscript;
    public PlayerScript dealerscript;
    public DealerEmotionSystem dealerEmotion;

    [Header("UI Text")]
    public Text CashText;
    public Text HandText;
    public Text BetText;
    public Text DealerText;
    public GameObject hideCard;
    public Text DealerEmotionText;

    [Header("Outcome Screens")]
    public GameObject WinScreen;
    public GameObject LoseScreen;
    public GameObject DrawScreen;

    [Header("Buff System (optional)")]
    [SerializeField] private BuffManager buffManager;
    [Tooltip("Optional UI text to show which buff (if any) was awarded this round.")]
    public Text buffRewardText;

    private void Awake()
    {
        // Attempt to auto-locate BuffManager if not assigned
        if (buffManager == null)
        {
            buffManager = Object.FindFirstObjectByType<BuffManager>();
            if (buffManager == null)
            {
                Debug.LogWarning("BlackjackGameManager could not find BuffManager in this scene.");
            }
            else
            {
                Debug.Log("BlackjackGameManager found BuffManager automatically.");
            }
        }
    }

    // Buff tier type: configured in inspector
    [System.Serializable]
    public class BuffTier
    {
        public string tierName;
        public int minScore;
        public int maxScore;
        public List<ScriptableBuff> possibleBuffs = new List<ScriptableBuff>();
        [Range(0f, 1f)]
        public float dropChance = 1f; // optional: chance to drop from this tier
    }

    [Header("Configure Blackjack Buff Tiers")]
    [SerializeField]
    private List<BuffTier> buffTiers = new List<BuffTier>();

    private int pot = 0;
    private bool playerHasStood = false;
    private bool roundEnded = false;

    private void Start()
    {
        // Safety: remove existing listeners then add ours
        if (deal != null) deal.onClick.RemoveAllListeners();
        if (hit != null) hit.onClick.RemoveAllListeners();
        if (stand != null) stand.onClick.RemoveAllListeners();

        if (deal != null) deal.onClick.AddListener(DealClicked);
        if (hit != null) hit.onClick.AddListener(HitClicked);
        if (stand != null) stand.onClick.AddListener(StandClicked);

        // Auto-find BuffManager if not assigned
        if (buffManager == null)
        {
            buffManager = GetComponent<BuffManager>();
            if (buffManager != null)
                Debug.Log("BlackJackGameManager: Auto-found BuffManager on GameObject.");
        }

        // Optional: sort tiers by minScore descending for checking (not required, but predictable)
        buffTiers.Sort((a, b) => b.minScore.CompareTo(a.minScore));

        ResetUI();
        UpdateUI();
    }

    // Start a new round
    public void DealClicked()
    {
        ResetUI();

        if (deal != null) deal.gameObject.SetActive(false); // hide Deal when round starts
        if (hit != null) hit.gameObject.SetActive(true);
        if (stand != null) stand.gameObject.SetActive(true);

        roundEnded = false;
        playerHasStood = false;

        if (DealerText != null) DealerText.gameObject.SetActive(false);

        // Shuffle deck
        var deck = GameObject.Find("Deck");
        if (deck != null)
        {
            var deckScript = deck.GetComponent<DeckScript>();
            if (deckScript != null) deckScript.Shuffle();
        }

        // Deal hands
        playerscript.DealInitialHand();
        dealerscript.DealInitialHand();

        if (hideCard != null) hideCard.SetActive(true);

        pot = 40;
        playerscript.AdjustMoney(-20);

        UpdateUI();

        // Initialize dealer emotion system based on dealer's total hand value
        if (dealerEmotion != null)
        {
            // Pass the total hand value (both cards combined)
            dealerEmotion.EvaluateInitialHand(dealerscript.handValue, 0);

            if (DealerEmotionText != null)
            {
                DealerEmotionText.text = dealerEmotion.GetDealerStatement();
                DealerEmotionText.gameObject.SetActive(true);
            }
        }

        CheckBlackjack(); // handle instant 21 cases
    }

    public void HitClicked()
    {
        if (roundEnded) return;

        playerscript.HitOneCard();
        UpdateUI();

        // Immediately disable hit button if player busts or hits 21
        if (playerscript.handValue >= 21)
        {
            if (hit != null) hit.gameObject.SetActive(false);
            if (stand != null) stand.gameObject.SetActive(false);

            playerHasStood = true; // auto-stand if bust or hit 21
            StartCoroutine(DealerTurn());
        }
    }

    public void StandClicked()
    {
        if (roundEnded) return;

        playerHasStood = true;
        if (hit != null) hit.gameObject.SetActive(false);
        if (stand != null) stand.gameObject.SetActive(false);

        StartCoroutine(DealerTurn());
    }

    // Dealer plays automatically
    private IEnumerator DealerTurn()
    {
        // Reveal the dealer's hidden card
        if (hideCard != null)
            hideCard.SetActive(false);

        if (DealerText != null) DealerText.gameObject.SetActive(true);

        // small delay for reveal
        yield return new WaitForSeconds(.5f);

        // If player busted, we can finish early
        if (playerscript.handValue > 21)
        {
            DetermineRoundOutcome();
            yield break;
        }

        // Dealer draws until 17 or higher
        while (dealerscript.handValue < 17 && dealerscript.cardIndex < dealerscript.hand.Length)
        {
            dealerscript.HitOneCard();
            UpdateUI();
            yield return new WaitForSeconds(.5f);
        }

        // Small delay then show result
        yield return new WaitForSeconds(.5f);

        DetermineRoundOutcome();
    }

    private void CheckBlackjack()
    {
        // If dealer has blackjack immediately: force dealer turn handling
        if (dealerscript.handValue == 21 && IsBlackjack(dealerscript))
        {
            playerHasStood = true;
            if (hit != null) hit.gameObject.SetActive(false);
            if (stand != null) stand.gameObject.SetActive(false);
            StartCoroutine(DealerTurnAfterBlackjack());
        }
        else if (playerscript.handValue == 21 && IsBlackjack(playerscript))
        {
            // Player has blackjack (21 with exactly 2 cards) - cannot hit further
            if (hit != null) hit.gameObject.SetActive(false);
            // Player may still press Stand to conclude
        }
    }

    private IEnumerator DealerTurnAfterBlackjack()
    {
        // small pause to show cards
        yield return new WaitForSeconds(.5f);
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
        // Delay before showing outcome
        yield return new WaitForSeconds(1.5f);

        bool playerBust = playerscript.handValue > 21;
        bool dealerBust = dealerscript.handValue > 21;

        // Reset buff UI message
        if (buffRewardText != null) buffRewardText.text = "";

        if ((playerBust && dealerBust) || (playerscript.handValue == dealerscript.handValue))
        {
            // Draw or both bust
            DrawScreen.SetActive(true);
            playerscript.AdjustMoney(pot / 2);
        }
        else if (playerBust || (!dealerBust && dealerscript.handValue > playerscript.handValue))
        {
            // Player loses
            LoseScreen.SetActive(true);
        }
        else
        {
            // Player wins
            WinScreen.SetActive(true);
            playerscript.AdjustMoney(pot);

            // **** Award buff based on player's final score (only when player wins) ****
            AwardBuffForBlackjackWin(playerscript.handValue);
        }

        if (hit != null) hit.gameObject.SetActive(false);
        if (stand != null) stand.gameObject.SetActive(false);
        if (deal != null) deal.gameObject.SetActive(true);

        roundEnded = true;

        UpdateUI();
    }

    private bool IsBlackjack(PlayerScript player)
    {
        // Blackjack is 21 with exactly 2 cards
        return player.handValue == 21 && player.cardIndex == 2;
    }

    private void UpdateUI()
    {
        if (HandText != null) HandText.text = "Hand: " + playerscript.handValue;
        if (DealerText != null) DealerText.text = "Hand: " + dealerscript.handValue;
        if (CashText != null) CashText.text = "Cash: " + playerscript.GetMoney().ToString();
        if (BetText != null) BetText.text = "Bet: " + pot.ToString();
    }

    private void ResetUI()
    {
        if (hit != null) hit.gameObject.SetActive(false);
        if (stand != null) stand.gameObject.SetActive(false);

        if (WinScreen != null) WinScreen.SetActive(false);
        if (LoseScreen != null) LoseScreen.SetActive(false);
        if (DrawScreen != null) DrawScreen.SetActive(false);

        if (hideCard != null) hideCard.SetActive(false);

        if (buffRewardText != null) buffRewardText.text = "";
    }

    // ---------------------
    // Buff awarding helpers
    // ---------------------

    // Awards a buff based on player's final score when the player wins.
    // Score ranges:
    // 0-13   -> Common
    // 14-18 -> Uncommon
    // 19-20 -> Rare
    // 21    -> Epic
    private void AwardBuffForBlackjackWin(int finalScore)
    {
        if (buffManager == null)
        {
            Debug.LogWarning("BuffManager not assigned — cannot award buffs.");
            if (buffRewardText != null) buffRewardText.text = "Buff: None";
            return;
        }

        // Find a tier which contains the final score
        BuffTier matchedTier = null;
        foreach (var tier in buffTiers)
        {
            if (finalScore >= tier.minScore && finalScore <= tier.maxScore)
            {
                matchedTier = tier;
                break;
            }
        }

        if (matchedTier == null)
        {
            Debug.Log($"No buff tier matched for final score {finalScore}. No buff awarded.");
            if (buffRewardText != null) buffRewardText.text = "Buff: None";
            return;
        }

        // Drop chance check
        float roll = UnityEngine.Random.Range(0f, 1f);
        if (roll > matchedTier.dropChance)
        {
            Debug.Log($"Buff drop roll failed for tier {matchedTier.tierName} (rolled {roll:F2}, needed <= {matchedTier.dropChance:F2}).");
            if (buffRewardText != null) buffRewardText.text = "Buff: None";
            return;
        }

        // Collect available buffs (not already active)
        List<ScriptableBuff> available = new List<ScriptableBuff>();
        foreach (var buff in matchedTier.possibleBuffs)
        {
            if (buff == null) continue;
            if (!buffManager.HasBuff(buff))
                available.Add(buff);
        }

        if (available.Count == 0)
        {
            Debug.Log($"All buffs in tier {matchedTier.tierName} are already active. No buff awarded.");
            if (buffRewardText != null) buffRewardText.text = "Buff: None";
            return;
        }

        // Choose random buff from available
        int index = UnityEngine.Random.Range(0, available.Count);
        ScriptableBuff selected = available[index];

        // Award it
        buffManager.AddBuff(selected);
        Debug.Log($"Awarded buff '{selected.BuffName}' from tier {matchedTier.tierName} for player score {finalScore}.");

        if (buffRewardText != null)
        {
            string buffDescription = GetBuffDescription(selected);
            buffRewardText.text = $"Buff Won: {selected.BuffName}\n{buffDescription}";
        }
    }

    // Helper method to get buff description
    private string GetBuffDescription(ScriptableBuff buff)
    {
        // Try to cast to specific buff types to get detailed info
        if (buff is QuantityBuff quantityBuff)
        {
            float percentage = (quantityBuff.modifier - 1f) * 100f;
            string sign = percentage >= 0 ? "+" : "";
            return $"{quantityBuff.cropAffected} Quantity {sign}{percentage:F0}%";
        }
        else if (buff is ValueBuff valueBuff)
        {
            float percentage = (valueBuff.modifier - 1f) * 100f;
            string sign = percentage >= 0 ? "+" : "";
            return $"{valueBuff.cropAffected} Value {sign}{percentage:F0}%";
        }
        // Fallback for other buff types
        return "Buff Applied";
    }
}