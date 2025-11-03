using UnityEngine;
using System.Collections;

/// <summary>
/// Improved PlantGrowth that integrates SeedData/CropData with water droplet icons
/// Combines the best of both Plant.cs and PlantGrowth.cs
/// </summary>
public class PlantGrowth: MonoBehaviour
{
    [Header("Plant Data")]
    public SeedData seedData;
    public CropData cropData;

    [Header("Growth State")]
    public int currentGrowthStage = 0;
    public int turnsGrown = 0;
    public bool isFullyGrown = false;
    public bool isWatered = false;
    public int timesHarvested = 0;

    [Header("Soil State")]
    public bool isOnTilledSoil = false;
    public string currentSeason = "Spring";

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public GameObject waterIconPrefab; // Assign the Waterdrop prefab 

    private GameObject currentStagePrefab;
    private GameObject waterIconInstance;

    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // If still null, add one
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        if (seedData != null)
        {
            cropData = seedData.producedCrop;
            UpdateGrowthVisual();

            // Show water icon if plant needs water
            if (seedData.requiresWater)
            {
                ShowWaterIcon(true);
            }
        }
    }

    /// <summary>
    /// Waters the plant - PUBLIC method for PlantPlacer to call
    /// </summary>
    public void Water()
    {
        if (isWatered)
        {
            Debug.Log($"{seedData.itemName} is already watered!");
            return;
        }

        isWatered = true;
        ShowWaterIcon(false);
        Debug.Log($"Watered {seedData.itemName}");
    }

    /// <summary>
    /// Called each turn to progress growth
    /// </summary>
    public void ProgressGrowth()
    {
        if (isFullyGrown)
        {
            Debug.Log($"{seedData.itemName} is already fully grown!");
            return;
        }

        // Check if plant has required conditions
        if (seedData.requiresWater && !isWatered)
        {
            Debug.Log($"{seedData.itemName} needs water to grow!");
            return;
        }

        if (seedData.requiresTilledSoil && !isOnTilledSoil)
        {
            Debug.Log($"{seedData.itemName} needs tilled soil to grow!");
            return;
        }

        // Progress growth
        turnsGrown++;

        // Check if reached full growth
        int requiredTurns = seedData.GetModifiedGrowthTime(currentSeason);
        if (turnsGrown >= requiredTurns)
        {
            isFullyGrown = true;
            currentGrowthStage = seedData.GetTotalGrowthStages() - 1;
            Debug.Log($"{seedData.itemName} is fully grown and ready to harvest!");
        }
        else
        {
            // Calculate current stage based on progress
            float growthProgress = (float)turnsGrown / requiredTurns;
            currentGrowthStage = Mathf.FloorToInt(growthProgress * seedData.GetTotalGrowthStages());
        }

        UpdateGrowthVisual();

        // Reset watered state and show icon for next turn
        isWatered = false;
        if (seedData.requiresWater && !isFullyGrown)
        {
            ShowWaterIcon(true);
        }
    }

    /// <summary>
    /// Checks if plant is fully grown - PUBLIC for PlantPlacer
    /// </summary>
    public bool IsFullyGrown()
    {
        return isFullyGrown;
    }

    /// <summary>
    /// Harvests the plant and returns the crop
    /// </summary>
    public CropData Harvest()
    {
        if (!isFullyGrown)
        {
            Debug.Log($"{seedData.itemName} is not ready to harvest yet!");
            return null;
        }

        // Calculate yield
        int yieldAmount = cropData.GetYieldAmount();

        // Apply seasonal bonus if applicable
        bool inBonusSeason = seedData.CanPlantInSeason(currentSeason) &&
                            seedData.seasonPreference != "All";

        if (inBonusSeason)
        {
            yieldAmount = Mathf.CeilToInt(yieldAmount * seedData.seasonalYieldBonus);
            Debug.Log($"Seasonal bonus applied! Yield increased.");
        }

        timesHarvested++;

        // Add crop to inventory if InventoryManager exists
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddHarvestedCrop(cropData, yieldAmount);
            Debug.Log($"Added {yieldAmount}x {cropData.itemName} to inventory!");
        }

        // Check if plant can be harvested again
        if (seedData.isMultiHarvest && timesHarvested < seedData.harvestsPerPlant)
        {
            // Reset for next harvest
            isFullyGrown = false;
            turnsGrown = 0;
            currentGrowthStage = Mathf.Max(0, seedData.GetTotalGrowthStages() - 2);
            UpdateGrowthVisual();

            // Show water icon again if needed
            if (seedData.requiresWater)
            {
                ShowWaterIcon(true);
            }

            Debug.Log($"Harvested {yieldAmount}x {cropData.itemName}. Plant will regrow!");
        }
        else
        {
            // Plant dies after harvest
            Debug.Log($"Harvested {yieldAmount}x {cropData.itemName}. Plant is spent.");

            // Clean up before destroying
            if (waterIconInstance != null)
            {
                Destroy(waterIconInstance);
            }

            Destroy(gameObject);
        }

        return cropData;
    }

    /// <summary>
    /// Shows or hides the water droplet icon
    /// </summary>
    private void ShowWaterIcon(bool show)
    {
        if (show)
        {
            // Only create if we don't have one and we have the prefab
            if (waterIconInstance == null && waterIconPrefab != null)
            {
                // Position icon above the plant (adjust Y offset as needed)
                Vector3 iconPos = transform.position + Vector3.up * 0.5f;
                waterIconInstance = Instantiate(waterIconPrefab, iconPos, Quaternion.identity, transform);

                Debug.Log($"Showing water icon for {seedData.itemName}");
            }
        }
        else
        {
            // Destroy the icon if it exists
            if (waterIconInstance != null)
            {
                Destroy(waterIconInstance);
                waterIconInstance = null;
            }
        }
    }

    /// <summary>
    /// Updates the visual representation based on growth stage
    /// </summary>
    private void UpdateGrowthVisual()
    {
        if (seedData == null || seedData.growthStages == null || seedData.growthStages.Length == 0)
        {
            return;
        }

        // Clamp stage to valid range
        int stage = Mathf.Clamp(currentGrowthStage, 0, seedData.growthStages.Length - 1);
        GameObject stagePrefab = seedData.growthStages[stage];

        if (stagePrefab == null)
        {
            return;
        }

        // Destroy old visual if it exists
        if (currentStagePrefab != null)
        {
            Destroy(currentStagePrefab);
        }

        // Instantiate new visual as a child of this plant
        currentStagePrefab = Instantiate(stagePrefab, transform.position, Quaternion.identity, transform);

        // If the stage has a sprite renderer, copy its sprite to our renderer
        if (spriteRenderer != null)
        {
            SpriteRenderer stageSpriteRenderer = stagePrefab.GetComponent<SpriteRenderer>();
            if (stageSpriteRenderer != null)
            {
                spriteRenderer.sprite = stageSpriteRenderer.sprite;
                spriteRenderer.sortingLayerID = stageSpriteRenderer.sortingLayerID;
                spriteRenderer.sortingOrder = stageSpriteRenderer.sortingOrder;
            }
        }
    }

    /// <summary>
    /// Gets growth progress as percentage
    /// </summary>
    public float GetGrowthProgress()
    {
        if (isFullyGrown) return 1f;

        int requiredTurns = seedData.GetModifiedGrowthTime(currentSeason);
        return Mathf.Clamp01((float)turnsGrown / requiredTurns);
    }

    /// <summary>
    /// Gets display info for UI
    /// </summary>
    public string GetPlantInfo()
    {
        string info = $"{seedData.itemName}\n";

        if (isFullyGrown)
        {
            info += "Ready to Harvest!\n";
            info += $"Produces: {cropData.itemName}";
        }
        else
        {
            int requiredTurns = seedData.GetModifiedGrowthTime(currentSeason);
            int turnsLeft = requiredTurns - turnsGrown;
            info += $"Growth: {turnsGrown}/{requiredTurns} turns\n";
            info += $"Turns until harvest: {turnsLeft}";
        }

        if (seedData.requiresWater)
        {
            info += $"\nWatered: {(isWatered ? "Yes" : "No")}";
        }

        if (seedData.isMultiHarvest)
        {
            info += $"\nHarvests: {timesHarvested}/{seedData.harvestsPerPlant}";
        }

        return info;
    }
}