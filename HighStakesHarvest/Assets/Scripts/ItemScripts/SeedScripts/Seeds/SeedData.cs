using UnityEngine;

/// <summary>
/// SeedData that integrates with the item system
/// Maintains backward compatibility with existing seed scripts
/// </summary>
[CreateAssetMenu(fileName = "NewSeed", menuName = "Farming/Seed Data")]
public class SeedData : ItemData
{
    [Header("Original Seed Properties")]
    public int price; // Kept for backward compatibility
    public int growthTime; // amount of turns til growth
    public string seasonPreference;
    public string type;
    public GameObject[] growthStages; // prefabs for each stage

    // Backward compatibility property - maps to itemName
    public string seedName
    {
        get { return itemName; }
        set { itemName = value; }
    }

    [Header("Enhanced Properties")]
    public CropData producedCrop; // The crop this seed produces when harvested
    public int harvestsPerPlant = 1; // How many times can you harvest before plant dies
    public bool isMultiHarvest = false; // Can harvest multiple times?

    [Header("Growth Modifiers")]
    public bool requiresWater = true;
    public bool requiresTilledSoil = true;
    public float droughtResistance = 0.5f; // 0 = dies without water, 1 = doesn't need water

    [Header("Yield Modifiers")]
    public int baseYieldMin = 1;
    public int baseYieldMax = 3;
    public float seasonalYieldBonus = 1.5f; // Multiplier when planted in preferred season

    private void OnValidate()
    {
        itemType = ItemType.Seed;

        // Sync basePrice with price for backward compatibility
        if (basePrice != price)
        {
            basePrice = price;
        }
    }

    private void OnEnable()
    {
        // Ensure basePrice is synced
        basePrice = price;
    }

    /// <summary>
    /// Plants the seed at the specified position
    /// </summary>
    public override bool Use(GameObject user)
    {
        // TODO: Implement planting logic
        // This should interface with your existing planting system
        Debug.Log($"Attempting to plant {itemName}");
        return true;
    }

    /// <summary>
    /// Checks if the seed can be planted in the current season
    /// </summary>
    public bool CanPlantInSeason(string currentSeason)
    {
        // "All" means any season, empty string means any season
        if (string.IsNullOrEmpty(seasonPreference) || seasonPreference == "All")
            return true;

        return seasonPreference.Equals(currentSeason, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets growth time with potential modifiers applied
    /// </summary>
    public int GetModifiedGrowthTime(string currentSeason)
    {
        // Reduce growth time by 1 turn if in preferred season (minimum 1 turn)
        if (CanPlantInSeason(currentSeason) && seasonPreference != "All")
        {
            return Mathf.Max(1, growthTime - 1);
        }

        return growthTime;
    }

    /// <summary>
    /// Gets the current growth stage prefab
    /// </summary>
    public GameObject GetGrowthStagePrefab(int stage)
    {
        if (growthStages == null || growthStages.Length == 0)
            return null;

        // Clamp stage to valid range
        stage = Mathf.Clamp(stage, 0, growthStages.Length - 1);
        return growthStages[stage];
    }

    /// <summary>
    /// Gets the total number of growth stages
    /// </summary>
    public int GetTotalGrowthStages()
    {
        return growthStages != null ? growthStages.Length : 0;
    }

    public override string GetDisplayInfo()
    {
        string info = base.GetDisplayInfo();
        info += $"\nGrowth Time: {growthTime} turns";
        info += $"\nSeason: {(string.IsNullOrEmpty(seasonPreference) ? "All" : seasonPreference)}";

        if (producedCrop != null)
        {
            info += $"\nProduces: {producedCrop.itemName}";
        }

        if (isMultiHarvest)
        {
            info += $"\nHarvests: {harvestsPerPlant}";
        }

        return info;
    }
}