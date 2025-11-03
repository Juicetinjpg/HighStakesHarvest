using UnityEngine;

/// <summary>
/// Enhanced SeedData that extends ItemData
/// Compatible with your old Plant.cs visual system
/// Works with the new inventory system
/// </summary>
[CreateAssetMenu(fileName = "NewSeed", menuName = "Farming/Seed Data")]
public class SeedData : ItemData
{
    [Header("Old System Compatibility")]
    public string seedName; // Kept for backward compatibility
    public int price; // Kept for backward compatibility
    public GameObject[] growthStages; // Your existing prefabs for each stage

    [Header("Growth Properties")]
    public int growthTime = 3; // Turns until fully grown
    public string seasonPreference = "All"; // Spring, Summer, Fall, Winter, or All
    public string type = "Vegetable"; // Vegetable, Fruit, Flower, etc.

    [Header("Crop Output")]
    public CropData producedCrop; // What crop this seed produces when harvested

    [Header("Requirements")]
    public bool requiresWater = true;
    public bool requiresTilledSoil = false;

    [Header("Multi-Harvest")]
    public bool isMultiHarvest = false; // Can be harvested multiple times?
    public int harvestsPerPlant = 1;
    public int regrowthTime = 1; // Turns between harvests if multi-harvest

    [Header("Seasonal Bonuses")]
    public float seasonalYieldBonus = 1.2f; // Yield bonus when planted in preferred season
    public float seasonalGrowthSpeed = 0.8f; // Grows faster in preferred season (0.8 = 20% faster)

    private void OnValidate()
    {
        // Auto-sync with ItemData fields
        itemType = ItemType.Seed;

        // Sync old fields with new fields for compatibility
        if (string.IsNullOrEmpty(seedName) && !string.IsNullOrEmpty(itemName))
        {
            seedName = itemName;
        }
        else if (!string.IsNullOrEmpty(seedName) && string.IsNullOrEmpty(itemName))
        {
            itemName = seedName;
        }

        if (price != 0 && basePrice == 0)
        {
            basePrice = price;
        }
        else if (basePrice != 0 && price == 0)
        {
            price = basePrice;
        }
    }

    /// <summary>
    /// Checks if this seed can be planted in the current season
    /// </summary>
    public bool CanPlantInSeason(string currentSeason)
    {
        if (seasonPreference == "All") return true;
        return seasonPreference.Equals(currentSeason, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets modified growth time based on season
    /// </summary>
    public int GetModifiedGrowthTime(string currentSeason)
    {
        if (CanPlantInSeason(currentSeason) && seasonPreference != "All")
        {
            // Grows faster in preferred season
            return Mathf.CeilToInt(growthTime * seasonalGrowthSpeed);
        }
        return growthTime;
    }

    /// <summary>
    /// Gets total number of growth stages
    /// </summary>
    public int GetTotalGrowthStages()
    {
        return growthStages != null ? growthStages.Length : 0;
    }

    /// <summary>
    /// Gets the prefab for a specific growth stage
    /// </summary>
    public GameObject GetGrowthStagePrefab(int stage)
    {
        if (growthStages == null || stage < 0 || stage >= growthStages.Length)
            return null;

        return growthStages[stage];
    }

    /// <summary>
    /// Use the seed (plants it)
    /// </summary>
    public override bool Use(GameObject user)
    {
        Debug.Log($"Use {itemName} - right-click on tilled soil to plant!");
        return false; // Don't consume here, ImprovedPlantPlacer handles it
    }

    public override string GetDisplayInfo()
    {
        string info = base.GetDisplayInfo();
        info += $"\nGrowth Time: {growthTime} turns";
        info += $"\nSeason: {seasonPreference}";

        if (producedCrop != null)
        {
            info += $"\nProduces: {producedCrop.itemName}";
        }

        if (isMultiHarvest)
        {
            info += $"\nMulti-Harvest: {harvestsPerPlant}x";
        }

        return info;
    }
}