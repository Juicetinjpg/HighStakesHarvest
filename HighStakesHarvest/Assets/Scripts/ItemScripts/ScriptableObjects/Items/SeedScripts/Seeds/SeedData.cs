using UnityEngine;

/// <summary>
/// Seed data that inherits from ItemData for full inventory integration
/// Replaces your old SeedData.cs
/// Works with your existing growth stage prefabs and CropData
/// </summary>
[CreateAssetMenu(fileName = "NewSeed", menuName = "Farming/Seed Data")]
public class SeedData : ItemData
{

    [Header("Seed-Specific Properties")]
    public string cropName;
    public CropData producedCrop; // What crop this produces when harvested
    public int growthTime = 3; // Base number of turns to grow
    public string seasonPreference = "All"; // Spring, Summer, Fall, Winter, or All

    [Header("Growth Visuals")]
    public GameObject[] growthStages; // Your prefabs for each growth stage (Sprout, MidGrowth, FullGrowth)

    [Header("Planting Requirements")]
    public bool requiresWater = true;
    public bool requiresTilledSoil = false;

    [Header("Multi-Harvest")]
    public bool isMultiHarvest = false; // Can harvest multiple times?
    public int harvestsPerPlant = 1; // How many times can you harvest
    public int regrowthTime = 2; // Turns to regrow after harvest

    [Header("Seasonal Bonuses")]
    public float seasonalGrowthBonus = 0.75f; // 25% faster in preferred season
    public float seasonalYieldBonus = 1.5f; // 50% more yield in preferred season

    private void OnValidate()
    {
        itemType = ItemType.Seed;

        // Auto-link: When you assign producedCrop, it automatically sets the crop's sourceSeed
        if (producedCrop != null && producedCrop.sourceSeed != this)
        {
            producedCrop.sourceSeed = this;
        }
    }

    /// <summary>
    /// Checks if this seed can be planted in the given season
    /// </summary>
    public bool CanPlantInSeason(string season)
    {
        if (seasonPreference == "All") return true;
        return seasonPreference.Equals(season, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets modified growth time based on season
    /// </summary>
    public int GetModifiedGrowthTime(string currentSeason)
    {
        // If in preferred season, grow faster
        if (CanPlantInSeason(currentSeason) && seasonPreference != "All")
        {
            return Mathf.CeilToInt(growthTime * seasonalGrowthBonus);
        }

        return growthTime;
    }

    public int GetCurrentGrowth()
    {
        CropManager cropManager = GameObject.Find("CropManager").GetComponent<CropManager>();
        return cropManager.cropInfoDictionary[cropName].growth;
    }

    /// <summary>
    /// Gets the total number of growth stages
    /// </summary>
    public int GetTotalGrowthStages()
    {
        return growthStages != null ? growthStages.Length : 0;
    }

    /// <summary>
    /// Gets the prefab for a specific growth stage
    /// </summary>
    public GameObject GetGrowthStagePrefab(int stageIndex)
    {
        if (growthStages == null || stageIndex < 0 || stageIndex >= growthStages.Length)
        {
            return null;
        }

        return growthStages[stageIndex];
    }

    /// <summary>
    /// Uses the seed (plants it)
    /// </summary>
    public override bool Use(GameObject user)
    {
        Debug.Log($"Cannot use {itemName} directly - equip it and click on the ground to plant");
        return false;
    }

    public override string GetDisplayInfo()
    {
        string info = base.GetDisplayInfo();
        info += $"\nGrowth Time: {growthTime} turns";
        info += $"\nSeason: {seasonPreference}";
        info += $"\nProduces: {(producedCrop != null ? producedCrop.itemName : "Unknown")}";

        if (isMultiHarvest)
        {
            info += $"\nMulti-Harvest ({harvestsPerPlant}x)";
        }

        if (requiresWater)
        {
            info += "\nRequires Water";
        }

        return info;
    }
}