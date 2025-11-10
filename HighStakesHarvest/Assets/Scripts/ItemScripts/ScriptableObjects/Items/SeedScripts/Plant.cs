using UnityEngine;

/// <summary>
/// Updated Plant component that works with the new ItemData-based SeedData
/// Keeps all your visual feedback (water droplets, growth stages)
/// Now has Initialize() method for dynamic creation
/// </summary>
public class Plant : MonoBehaviour
{
    [Header("Plant Data")]
    public SeedData seedData;
    public string currentSeason = "Spring";
    public string cropName;

    [Header("Growth State")]
    public int currentStage = 0;
    public int turnsGrown = 0;
    public bool needsWater = true;
    public int timesHarvested = 0;

    [Header("Visual References")]
    public GameObject waterIconPrefab;
    private GameObject currentVisual;
    private GameObject waterIconInstance;

    /// <summary>
    /// Initialize the plant (called when created dynamically)
    /// </summary>
    public void Initialize(SeedData seed, GameObject waterIcon, string season)
    {
        seedData = seed;
        waterIconPrefab = waterIcon;
        currentSeason = season;

        currentStage = 0;
        turnsGrown = 0;
        needsWater = true;

        // Spawn initial stage and show water droplet
        SpawnStage(currentStage);
        ShowWaterIcon(true);

        Debug.Log($"Initialized {seedData.itemName} - needs water to grow");
    }

    void Start()
    {
        // If not initialized (old plants), set up defaults
        if (seedData != null && waterIconPrefab != null)
        {
            SpawnStage(currentStage);
            ShowWaterIcon(true);
        }
    }

    /// <summary>
    /// Waters the plant
    /// </summary>
    public void Water()
    {
        if (!needsWater)
        {
            Debug.Log($"{seedData.itemName} doesn't need water right now");
            return;
        }

        needsWater = false;
        ShowWaterIcon(false);
        Debug.Log($"{seedData.itemName} has been watered ✓");
    }

    /// <summary>
    /// Called each turn to progress growth
    /// </summary>
    public void AdvanceTurn()
    {
        if (IsFullyGrown())
        {
            Debug.Log($"{seedData.itemName} is already fully grown");
            return;
        }

        // Only grow if watered
        if (needsWater)
        {
            Debug.Log($"{seedData.itemName} did not grow - needs water!");
            ShowWaterIcon(true); // Make sure water icon is showing
            return;
        }

        // Progress growth
        turnsGrown++;

        // Check if reached full growth
        int requiredTurns = seedData.GetCurrentGrowth();
        if (turnsGrown >= requiredTurns)
        {
            currentStage = seedData.growthStages.Length - 1;
            Debug.Log($"{seedData.itemName} is fully grown and ready to harvest! 🌾");
        }
        else
        {
            // Calculate current stage based on progress
            float growthProgress = (float)turnsGrown / requiredTurns;
            currentStage = Mathf.FloorToInt(growthProgress * seedData.growthStages.Length);
            currentStage = Mathf.Clamp(currentStage, 0, seedData.growthStages.Length - 1);
            Debug.Log($"{seedData.itemName} grew to stage {currentStage}");
        }

        SpawnStage(currentStage);

        // Reset water for next turn
        needsWater = true;
        ShowWaterIcon(true);
    }

    /// <summary>
    /// Spawns the visual for the current growth stage
    /// </summary>
    private void SpawnStage(int stage)
    {
        if (seedData == null || seedData.growthStages == null) return;

        // Destroy old visual
        if (currentVisual != null)
        {
            Destroy(currentVisual);
        }

        // Check if stage is valid
        if (stage < 0 || stage >= seedData.growthStages.Length)
        {
            Debug.LogWarning($"Invalid growth stage {stage} for {seedData.itemName}");
            return;
        }

        // Spawn new stage prefab
        if (seedData.growthStages[stage] != null)
        {
            currentVisual = Instantiate(seedData.growthStages[stage], transform.position, Quaternion.identity, transform);
        }
    }

    /// <summary>
    /// Shows or hides the water droplet icon
    /// </summary>
    private void ShowWaterIcon(bool show)
    {
        if (show)
        {
            // Show water icon if not already showing
            if (waterIconInstance == null && waterIconPrefab != null)
            {
                Vector3 iconPos = transform.position + Vector3.up * 0.5f; // Float above plant
                waterIconInstance = Instantiate(waterIconPrefab, iconPos, Quaternion.identity, transform);
                Debug.Log($"💧 Water droplet shown for {seedData.itemName}");
            }
        }
        else
        {
            // Hide water icon
            if (waterIconInstance != null)
            {
                Destroy(waterIconInstance);
                waterIconInstance = null;
                Debug.Log($"💧 Water droplet hidden for {seedData.itemName}");
            }
        }
    }

    /// <summary>
    /// Checks if plant is fully grown
    /// </summary>
    public bool IsFullyGrown()
    {
        return turnsGrown >= seedData.GetCurrentGrowth();
    }

    /// <summary>
    /// Harvests the plant and returns the crop
    /// </summary>
    public CropData Harvest()
    {
        if (!IsFullyGrown())
        {
            Debug.Log($"{seedData.itemName} is not ready to harvest yet!");
            return null;
        }

        // Get the crop produced by this seed
        CropData crop = seedData.producedCrop;

        if (crop == null)
        {
            Debug.LogError($"{seedData.itemName} has no crop assigned!");
            return null;
        }

        timesHarvested++;
        Debug.Log($"Harvested {crop.itemName} from {seedData.itemName}");

        // Check if plant can be harvested again (multi-harvest crops)
        if (seedData.isMultiHarvest && timesHarvested < seedData.harvestsPerPlant)
        {
            // Reset for next harvest
            turnsGrown = 0;
            needsWater = true;
            currentStage = Mathf.Max(0, seedData.growthStages.Length - 2);

            SpawnStage(currentStage);
            ShowWaterIcon(true);

            Debug.Log($"{seedData.itemName} will regrow ({timesHarvested}/{seedData.harvestsPerPlant} harvests)");
        }
        else
        {
            // Plant is done
            Debug.Log($"{seedData.itemName} has been fully harvested");
            Destroy(gameObject);
        }

        return crop;
    }
}