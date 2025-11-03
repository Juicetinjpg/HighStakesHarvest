using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the growth of a planted seed into a harvestable crop
/// Integrates SeedData and CropData for complete farming cycle
/// </summary>
public class PlantGrowth : MonoBehaviour
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
    private GameObject currentStagePrefab;
    
    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (seedData != null)
        {
            cropData = seedData.producedCrop;
            UpdateGrowthVisual();
        }
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
        isWatered = false; // Reset watered state for next turn
        
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
    }
    
    /// <summary>
    /// Waters the plant
    /// </summary>
    public bool Water()
    {
        if (isWatered)
        {
            Debug.Log($"{seedData.itemName} is already watered!");
            return false;
        }
        
        isWatered = true;
        Debug.Log($"Watered {seedData.itemName}");
        
        // Visual feedback for watering could go here
        return true;
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
        
        // Check if plant can be harvested again
        if (seedData.isMultiHarvest && timesHarvested < seedData.harvestsPerPlant)
        {
            // Reset for next harvest
            isFullyGrown = false;
            turnsGrown = 0;
            currentGrowthStage = Mathf.Max(0, seedData.GetTotalGrowthStages() - 2);
            UpdateGrowthVisual();
            
            Debug.Log($"Harvested {yieldAmount}x {cropData.itemName}. Plant will regrow!");
        }
        else
        {
            // Plant dies after harvest
            Debug.Log($"Harvested {yieldAmount}x {cropData.itemName}. Plant is spent.");
            Destroy(gameObject);
        }
        
        return cropData;
    }
    
    /// <summary>
    /// Updates the visual representation based on growth stage
    /// </summary>
    private void UpdateGrowthVisual()
    {
        if (seedData == null) return;
        
        GameObject stagePrefab = seedData.GetGrowthStagePrefab(currentGrowthStage);
        
        if (stagePrefab == null) return;
        
        // Destroy old visual
        if (currentStagePrefab != null)
        {
            Destroy(currentStagePrefab);
        }
        
        // Instantiate new visual
        currentStagePrefab = Instantiate(stagePrefab, transform.position, Quaternion.identity, transform);
        
        // If using sprites instead of prefabs
        if (spriteRenderer != null)
        {
            SpriteRenderer stageSpriteRenderer = stagePrefab.GetComponent<SpriteRenderer>();
            if (stageSpriteRenderer != null)
            {
                spriteRenderer.sprite = stageSpriteRenderer.sprite;
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
