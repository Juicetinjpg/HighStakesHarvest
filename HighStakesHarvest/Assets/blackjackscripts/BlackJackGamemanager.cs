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

    [Header("Buff System (optional)")]
    [SerializeField]
    private BuffManager buffManager;
    [Tooltip("Optional UI text to show which buff (if any) was awarded this round.")]
    public Text buffRewardText;

    private IEnumerator FindBuffManagerNextFrame()
    {
        yield return null; // wait one frame

        BuffManager bm = FindFirstObjectByType<BuffManager>();
        if (bm != null)
        {
            buffManager = bm;
            Debug.Log("BuffManager auto-found: " + buffManager.name);
        }
        else
        {
            Debug.LogError("No BuffManager found in scene!");
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
        buffManager = null;

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
            StartCoroutine(FindBuffManagerNextFrame());
        }

        // Sort tiers by minScore descending for predictable checking
        buffTiers.Sort((a, b) => b.minScore.CompareTo(a.minScore));

        ResetUI();
        UpdateUI();
    }

    public void DealClicked()
    {
        ResetUI();

        if (deal != null) deal.gameObject.SetActive(false);
        if (hit != null) hit.gameObject.SetActive(true);
        if (stand != null) stand.gameObject.SetActive(true);

        roundEnded = false;
        playerHasStood = false;

        if (DealerText != null) DealerText.gameObject.SetActive(false);

        var deck = GameObject.Find("Deck");
        if (deck != null)
        {
            var deckScript = deck.GetComponent<DeckScript>();
            if (deckScript != null) deckScript.Shuffle();
        }

        playerscript.DealInitialHand();
        dealerscript.DealInitialHand();

        if (hideCard != null) hideCard.SetActive(true);

        pot = 40;
        playerscript.AdjustMoney(-20);

        UpdateUI();

        if (dealerEmotion != null)
        {
            dealerEmotion.EvaluateInitialHand(dealerscript.handValue, 0);

            if (DealerEmotionText != null)
            {
                DealerEmotionText.text = dealerEmotion.GetDealerStatement();
                DealerEmotionText.gameObject.SetActive(true);
            }
        }

        CheckBlackjack();
    }

    public void HitClicked()
    {
        if (roundEnded) return;

        playerscript.HitOneCard();
        UpdateUI();

        if (playerscript.handValue >= 21)
        {
            if (hit != null) hit.gameObject.SetActive(false);
            if (stand != null) stand.gameObject.SetActive(false);

            playerHasStood = true;
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

    private IEnumerator DealerTurn()
    {
        if (hideCard != null)
            hideCard.SetActive(false);

        if (DealerText != null)
            DealerText.gameObject.SetActive(true);

        yield return new WaitForSeconds(.5f);

        if (playerscript.handValue > 21)
        {
            DetermineRoundOutcome();
            yield break;
        }

        while (dealerscript.handValue < playerscript.handValue && dealerscript.handValue < 17 && dealerscript.cardIndex < dealerscript.hand.Length)
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
        if (dealerscript.handValue == 21 && IsBlackjack(dealerscript))
        {
            playerHasStood = true;
            if (hit != null) hit.gameObject.SetActive(false);
            if (stand != null) stand.gameObject.SetActive(false);
            StartCoroutine(DealerTurnAfterBlackjack());
        }
        else if (playerscript.handValue == 21 && IsBlackjack(playerscript))
        {
            if (hit != null) hit.gameObject.SetActive(false);
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
        yield return new WaitForSeconds(1f);

        bool playerBust = playerscript.handValue > 21;
        bool dealerBust = dealerscript.handValue > 21;

        if (buffRewardText != null) buffRewardText.text = "";

        if ((playerBust && dealerBust) || (playerscript.handValue == dealerscript.handValue))
        {
            if (buffRewardText != null)
                buffRewardText.text = "The Player and Dealer Are Even Resulting In A Draw";
            playerscript.AdjustMoney(pot / 2);
        }
        else if (playerBust || (!dealerBust && dealerscript.handValue > playerscript.handValue))
        {
            if (buffRewardText != null)
                buffRewardText.text = "The Player Has Lost";
        }
        else
        {
            if (buffRewardText != null)
                buffRewardText.text = "The Player Has Won";
            yield return new WaitForSeconds(1.5f);
            playerscript.AdjustMoney(pot);

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
        if (hideCard != null) hideCard.SetActive(false);
        if (buffRewardText != null) buffRewardText.text = "";
    }

    // ---------------------
    // Buff awarding helpers
    // ---------------------
    private void AwardBuffForBlackjackWin(int finalScore)
    {
        if (buffManager == null)
        {
            Debug.LogWarning("BuffManager not assigned — cannot award buffs.");
            if (buffRewardText != null) buffRewardText.text = "Buff: None";
            return;
        }

        // Find the tier that matches the final score
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
            if (buffRewardText != null) buffRewardText.text = "Buff: None";
            return;
        }

        // Drop chance
        float roll = Random.Range(0f, 1f);
        if (roll > matchedTier.dropChance)
        {
            if (buffRewardText != null) buffRewardText.text = "Buff: None";
            return;
        }

        // --- Fallback tier logic ---
        BuffTier currentTier = matchedTier;
        List<ScriptableBuff> available = new List<ScriptableBuff>();

        while (currentTier != null)
        {
            available.Clear();
            foreach (var buff in currentTier.possibleBuffs)
            {
                if (buff != null && !buffManager.HasBuff(buff))
                    available.Add(buff);
            }

            if (available.Count > 0)
                break;

            int currentIndex = buffTiers.IndexOf(currentTier);
            if (currentIndex < buffTiers.Count - 1)
                currentTier = buffTiers[currentIndex + 1]; // go to lower tier
            else
                currentTier = null;
        }

        if (available.Count == 0)
        {
            if (buffRewardText != null) buffRewardText.text = "Buff: None";
            return;
        }

        int index = Random.Range(0, available.Count);
        ScriptableBuff selected = available[index];
        buffManager.AddBuff(selected);

        if (buffRewardText != null)
        {
            string desc = GetBuffDescription(selected);
            buffRewardText.text = $"Buff Won: {selected.BuffName}\n{desc}";
        }
    }

    private string GetBuffDescription(ScriptableBuff buff)
    {
        if (buff is QuantityBuff qBuff)
        {
            float percentage = (qBuff.modifier - 1f) * 100f;
            string sign = percentage >= 0 ? "+" : "";
            return $"{qBuff.cropAffected} Quantity {sign}{percentage:F0}%";
        }
        else if (buff is ValueBuff vBuff)
        {
            float percentage = (vBuff.modifier - 1f) * 100f;
            string sign = percentage >= 0 ? "+" : "";
            return $"{vBuff.cropAffected} Value {sign}{percentage:F0}%";
        }
        return "Buff Applied";
    }
}
