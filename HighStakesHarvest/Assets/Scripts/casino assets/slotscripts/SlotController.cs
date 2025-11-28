using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlotController : MonoBehaviour
{
    public static event Action HandlePulled = delegate { };

    [SerializeField]
    private TMP_Text prizeText;

    [SerializeField]
    private Row[] rows;

    [SerializeField]
    private Transform handle;

    [Header("Buff System")]
    public BuffManager buffManager;

    private int prizeValue;
    private bool resultsChecked = false;
    private bool canPull = true;

    // Original prize dictionaries
    private Dictionary<string, int> threeMatchPrizes = new Dictionary<string, int>()
    {
        { "Cherry", 200 },
        { "Bell", 400 },
        { "Bar", 600 },
        { "Seven", 1000 }
    };

    private Dictionary<string, int> twoMatchPrizes = new Dictionary<string, int>()
    {
        { "Cherry", 100 },
        { "Bell", 200 },
        { "Bar", 300 },
        { "Seven", 500 }
    };

    // Buff tier system based on prize score
    [System.Serializable]
    public class BuffTier
    {
        public string tierName;
        public int minScore; // Minimum score needed to unlock this tier
        public List<ScriptableBuff> possibleBuffs = new List<ScriptableBuff>();
        [Range(0f, 1f)]
        public float dropChance = 1f; // Chance to get a buff from this tier (0-1)
    }

    [Header("Buff Tier Configuration")]
    [SerializeField]
    private List<BuffTier> buffTiers = new List<BuffTier>();

    private void Start()
    {
        if (buffManager == null)
        {
            Debug.LogWarning("SlotController: BuffManager not assigned. Finding automatically...");
            StartCoroutine(FindBuffManagerNextFrame());
        }

        // Validate rows
        if (rows == null || rows.Length < 3)
        {
            Debug.LogError("SlotController: rows array is not properly assigned! Need 3 rows.");
            return;
        }

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null)
            {
                Debug.LogError($"SlotController: Row {i} is null!");
            }
        }

        // Sort buff tiers by minScore (highest first) for easier checking
        buffTiers.Sort((a, b) => b.minScore.CompareTo(a.minScore));
    }

    private IEnumerator FindBuffManagerNextFrame()
    {
        yield return null;

        BuffManager found = FindFirstObjectByType<BuffManager>();
        if (found != null)
        {
            buffManager = found;
            Debug.Log("SlotController found BuffManager successfully.");
        }
        else
        {
            Debug.LogError("SlotController could NOT find BuffManager in scene!");
        }
    }

    private void Update()
    {
        // Add null checks
        if (rows == null || rows.Length < 3) return;
        if (rows[0] == null || rows[1] == null || rows[2] == null) return;

        if (!rows[0].rowStopped || !rows[1].rowStopped || !rows[2].rowStopped)
        {
            prizeValue = 0;
            if (prizeText != null)
                prizeText.enabled = false;
            resultsChecked = false;
        }

        if (rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped && !resultsChecked)
        {
            CheckResults();
            if (prizeText != null)
            {
                prizeText.enabled = true;
            }
        }
    }

    private void OnMouseDown()
    {
        if (!canPull) return;
        if (rows == null || rows.Length < 3) return;
        if (rows[0] == null || rows[1] == null || rows[2] == null) return;

        if (rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped)
        {
            StartCoroutine("PullHandle");

        }
    }

    private IEnumerator PullHandle()
    {
        canPull = false;

        for (int i = 0; i < 30; i += 5)
        {
            if (handle != null)
                handle.Rotate(i, 0f, 0f);
            yield return new WaitForSeconds(.1f);
        }

        HandlePulled();

        for (int i = 0; i < 30; i += 5)
        {
            if (handle != null)
                handle.Rotate(-i, 0f, 0f);
            yield return new WaitForSeconds(.1f);
        }

        yield return new WaitUntil(() =>
            rows[0].rowStopped &&
            rows[1].rowStopped &&
            rows[2].rowStopped
        );

        canPull = true;
    }

    private void CheckResults()
    {
        if (rows == null || rows.Length < 3) return;
        if (rows[0] == null || rows[1] == null || rows[2] == null) return;

        string slot0 = rows[0].stoppedSlot;
        string slot1 = rows[1].stoppedSlot;
        string slot2 = rows[2].stoppedSlot;

        // Debug to see what stopped
        Debug.Log($"Results: {slot0}, {slot1}, {slot2}");

        prizeValue = 0;
        string matchType = "";

        // Check for three matching
        if (slot0 == slot1 && slot1 == slot2 && !string.IsNullOrEmpty(slot0))
        {
            if (threeMatchPrizes.ContainsKey(slot0))
            {
                prizeValue = threeMatchPrizes[slot0];
                matchType = $"3x {slot0}";
            }
        }
        // Check for two matching
        else
        {
            string matchedSymbol = null;
            if (slot0 == slot1 && !string.IsNullOrEmpty(slot0))
                matchedSymbol = slot0;
            else if (slot0 == slot2 && !string.IsNullOrEmpty(slot0))
                matchedSymbol = slot0;
            else if (slot1 == slot2 && !string.IsNullOrEmpty(slot1))
                matchedSymbol = slot1;

            if (matchedSymbol != null && twoMatchPrizes.ContainsKey(matchedSymbol))
            {
                prizeValue = twoMatchPrizes[matchedSymbol];
                matchType = $"2x {matchedSymbol}";
            }
        }

        // Award buff based on prize score
        if (prizeValue > 0)
        {
            if (buffManager == null)
            {
                Debug.LogWarning("BuffManager is null! Cannot award buffs.");
                // Display "No Prize" since we can't award a buff
                if (prizeText != null)
                {
                    prizeText.text = "Prize: None";
                }
            }
            else
            {
                string tierName;
                ScriptableBuff awardedBuff = GetBuffForScore(prizeValue, out tierName);
                if (awardedBuff != null)
                {
                    buffManager.AddBuff(awardedBuff);
                    Debug.Log($"Awarded buff: {awardedBuff.BuffName} from {tierName} tier (Score: {prizeValue})");

                    // Update prize text to show buff name and description
                    Debug.Log($"About to set prize text. prizeText is null? {prizeText == null}");
                    if (prizeText != null)
                    {
                        string buffDescription = GetBuffDescription(awardedBuff);
                        string displayText = $"Prize: {awardedBuff.BuffName}\n{buffDescription}";
                        prizeText.text = displayText;
                        Debug.Log($"Prize text set to: {displayText}");
                    }
                    else
                    {
                        Debug.LogError("prizeText is NULL! Cannot display buff info.");
                        Debug.LogError("prizeText is NULL! Cannot display buff info.");
                    }
                }
                else
                {
                    Debug.Log($"No buff awarded for score: {prizeValue}");
                    // No buff was awarded (already have all buffs or drop chance failed)
                    if (prizeText != null)
                    {
                        prizeText.text = "Prize: None";
                    }
                }
            }
        }
        else
        {
            // No match at all
            if (prizeText != null)
            {
                prizeText.text = "Prize: None";
            }
        }

        resultsChecked = true;
    }

    private ScriptableBuff GetBuffForScore(int score, out string tierName)
    {
        tierName = "";

        Debug.Log($"Checking buff tiers for score: {score}. Total tiers configured: {buffTiers.Count}");

        // Find the highest tier that the score qualifies for
        foreach (BuffTier tier in buffTiers)
        {
            Debug.Log($"Checking tier '{tier.tierName}' (minScore: {tier.minScore}, buffs: {tier.possibleBuffs.Count})");

            if (score >= tier.minScore)
            {
                // Check drop chance
                float roll = UnityEngine.Random.Range(0f, 1f);
                Debug.Log($"Score qualifies for {tier.tierName}! Rolling for drop chance... (rolled {roll:F2} vs {tier.dropChance:F2})");

                if (roll <= tier.dropChance)
                {
                    // Filter out buffs that are already active (only if buffManager exists)
                    List<ScriptableBuff> availableBuffs = new List<ScriptableBuff>();

                    if (buffManager != null)
                    {
                        foreach (ScriptableBuff buff in tier.possibleBuffs)
                        {
                            if (buff != null && !buffManager.HasBuff(buff))
                            {
                                availableBuffs.Add(buff);
                            }
                        }
                    }
                    else
                    {
                        // If no buff manager, add all non-null buffs
                        foreach (ScriptableBuff buff in tier.possibleBuffs)
                        {
                            if (buff != null)
                            {
                                availableBuffs.Add(buff);
                            }
                        }
                    }

                    // Select random buff from available buffs
                    if (availableBuffs.Count > 0)
                    {
                        foreach (var buff in tier.possibleBuffs)
                        {
                            bool has = buffManager.HasBuff(buff);
                            Debug.Log($"Buff {buff.BuffName} — HasBuff? {has}");
                        }
                        int randomIndex = UnityEngine.Random.Range(0, availableBuffs.Count);
                        ScriptableBuff selectedBuff = availableBuffs[randomIndex];

                        tierName = tier.tierName;
                        Debug.Log($"SUCCESS! Awarded '{selectedBuff.BuffName}' from {tier.tierName} tier (selected from {availableBuffs.Count} available buffs)");
                        return selectedBuff;
                    }
                    else
                    {
                        Debug.LogWarning($"{tier.tierName} tier - all buffs already active! Checking lower tiers...");
                    }
                }
                else
                {
                    Debug.Log($"Drop chance failed for {tier.tierName} tier. Checking lower tiers...");
                }

                // If we didn't get a buff from this tier, check lower tiers
                continue;
            }
        }

        Debug.LogWarning($"No buff awarded! Score {score} did not qualify for any tier or all buffs already obtained.");
        return null;
    }

    // Helper method to add buff tiers in code if needed
    public void AddBuffTier(string tierName, int minScore, float dropChance, params ScriptableBuff[] buffs)
    {
        BuffTier newTier = new BuffTier
        {
            tierName = tierName,
            minScore = minScore,
            dropChance = dropChance,
            possibleBuffs = new List<ScriptableBuff>(buffs)
        };
        buffTiers.Add(newTier);
        buffTiers.Sort((a, b) => b.minScore.CompareTo(a.minScore));
    }

    // Public getter for current score (useful for other systems)
    public int GetCurrentPrizeValue()
    {
        return prizeValue;
    }

    // Helper method to generate buff description from buff data
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