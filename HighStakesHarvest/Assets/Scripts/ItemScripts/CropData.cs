using UnityEngine;

/// <summary>
/// Represents harvested crops that can be sold or used
/// Links to the seed that produced it for integrated farming system
/// </summary>
[CreateAssetMenu(fileName = "NewCrop", menuName = "Farming/Crop Data")]
public class CropData : ItemData
{
    [Header("Crop Properties")]
    public SeedData sourceSeed; // Reference to the seed that produces this crop
    public CropQuality quality = CropQuality.Normal;
    
    [Header("Yield Information")]
    public int minYield = 1;
    public int maxYield = 1;
    public float qualityMultiplier = 1.0f; // Affects sell price based on quality
    
    [Header("Seasonal Bonuses")]
    public bool hasSeasonalBonus = false;
    public string bonusSeason; // Should match the season the crop thrives in
    public float seasonalPriceMultiplier = 1.5f;
    
    [Header("Usage")]
    public bool canBeUsedAsAnimalFeed = true;
    public int feedValue = 1;
    
    private void OnValidate()
    {
        itemType = ItemType.Crop;
        
        // Auto-link seasonal bonus to source seed if available
        if (sourceSeed != null && !hasSeasonalBonus)
        {
            bonusSeason = sourceSeed.seasonPreference;
        }
    }
    
    /// <summary>
    /// Gets the sell price with quality and seasonal modifiers applied
    /// </summary>
    public override int GetSellPrice()
    {
        float price = basePrice * qualityMultiplier;
        
        // Apply seasonal bonus if applicable
        if (hasSeasonalBonus && IsInBonusSeason())
        {
            price *= seasonalPriceMultiplier;
        }
        
        return Mathf.FloorToInt(price * sellPriceMultiplier);
    }
    
    /// <summary>
    /// Calculates the actual yield amount based on min/max range
    /// </summary>
    public int GetYieldAmount()
    {
        return Random.Range(minYield, maxYield + 1);
    }
    
    /// <summary>
    /// Checks if the crop is in its bonus season
    /// </summary>
    private bool IsInBonusSeason()
    {
        // TODO: Implement season manager check
        // For now, returning false. Hook this up to your season system
        // Example: return SeasonManager.Instance.CurrentSeason == bonusSeason;
        return false;
    }
    
    /// <summary>
    /// Use crop as animal feed
    /// </summary>
    public override bool Use(GameObject user)
    {
        if (canBeUsedAsAnimalFeed)
        {
            // TODO: Implement animal feeding logic
            Debug.Log($"Used {itemName} as animal feed (Feed Value: {feedValue})");
            return true;
        }
        
        Debug.Log($"{itemName} cannot be used this way.");
        return false;
    }
    
    /// <summary>
    /// Creates a crop with a specific quality
    /// </summary>
    public static CropData CreateWithQuality(CropData baseCrop, CropQuality quality)
    {
        CropData qualityCrop = Instantiate(baseCrop);
        qualityCrop.quality = quality;
        qualityCrop.qualityMultiplier = GetQualityMultiplier(quality);
        return qualityCrop;
    }
    
    /// <summary>
    /// Gets the price multiplier for a given quality
    /// </summary>
    private static float GetQualityMultiplier(CropQuality quality)
    {
        switch (quality)
        {
            case CropQuality.Poor: return 0.75f;
            case CropQuality.Normal: return 1.0f;
            case CropQuality.Good: return 1.25f;
            case CropQuality.Excellent: return 1.5f;
            case CropQuality.Perfect: return 2.0f;
            default: return 1.0f;
        }
    }
    
    public override string GetDisplayInfo()
    {
        string info = base.GetDisplayInfo();
        info += $"\nQuality: {quality}";
        if (canBeUsedAsAnimalFeed)
        {
            info += $"\nFeed Value: {feedValue}";
        }
        return info;
    }
}

/// <summary>
/// Defines crop quality levels that affect price
/// </summary>
public enum CropQuality
{
    Poor,
    Normal,
    Good,
    Excellent,
    Perfect
}
