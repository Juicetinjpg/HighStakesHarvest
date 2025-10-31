using UnityEngine;

/// <summary>
/// ScriptableObject defining a quota requirement for a specific creditor.
/// Based on the game's story progression: Grandma -> Friends -> Goon -> Mafia Loan Shark
/// </summary>
[CreateAssetMenu(fileName = "NewQuota", menuName = "High Stakes Harvest/Quota Data")]
public class QuotaData : ScriptableObject
{
    [Header("Creditor Information")]
    public string creditorName;
    [TextArea(3, 5)]
    public string creditorDescription;
    public Sprite creditorPortrait;

    [Header("Quota Requirements")]
    public int quotaAmount;
    public int turnsAllowed = 10; // Default 10 turns per season
    public Season season;

    [Header("Story")]
    [TextArea(3, 10)]
    public string introDialogue;
    [TextArea(3, 10)]
    public string successDialogue;
    [TextArea(3, 10)]
    public string failureDialogue;

    [Header("Rewards")]
    public int completionBonus = 0; // Bonus money for completing quota
    public string[] unlockedSeeds; // Seeds unlocked after completion
    public string[] unlockedBuffs; // Buffs unlocked after completion
}

public enum Season
{
    Spring,
    Summer,
    Fall,
    Winter
}
