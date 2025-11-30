using UnityEngine;

/// <summary>
/// Consolidated plant behaviour: growth, watering, visuals, harvesting/regrow.
/// Combines the previous Plant and PlantGrowth behaviours for compatibility with TurnManager, PlantManager, and PlantPlacer.
/// </summary>
public class PlantGrowth : MonoBehaviour
{
    [Header("Plant Data")]
    public SeedData seedData;
    public CropData cropData;
    public string currentSeason = "Spring";
    public string cropName;

    [Header("Growth State")]
    public int currentStage = 0;
    public int turnsGrown = 0;
    public bool needsWater = true;
    public int timesHarvested = 0;
    public bool isOnTilledSoil = false;

    [Header("Visual References")]
    public GameObject waterIconPrefab;
    public SpriteRenderer spriteRenderer;
    private GameObject currentVisual;
    private GameObject waterIconInstance;

    /// <summary>
    /// Initialize the plant (used when spawning dynamically).
    /// </summary>
    public void Initialize(SeedData seed, GameObject waterIcon, string season)
    {
        seedData = seed;
        cropData = seedData != null ? seedData.producedCrop : null;
        cropName = seedData != null ? seedData.cropName : cropName;
        waterIconPrefab = waterIcon != null ? waterIcon : waterIconPrefab;
        currentSeason = season;

        currentStage = 0;
        turnsGrown = 0;
        needsWater = true;
        isOnTilledSoil = !seedData?.requiresTilledSoil ?? false;

        SpawnStage(currentStage);
        ShowWaterIcon(true);
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        if (seedData != null)
        {
            cropData = seedData.producedCrop;
            cropName = string.IsNullOrEmpty(cropName) ? seedData.cropName : cropName;
            SpawnStage(currentStage);
            ShowWaterIcon(true);
        }
    }

    /// <summary>
    /// Waters the plant.
    /// </summary>
    public bool Water()
    {
        if (!needsWater)
        {
            Debug.Log($"{seedData.itemName} doesn't need water right now");
            return false;
        }

        needsWater = false;
        ShowWaterIcon(false);
        Debug.Log($"{seedData.itemName} has been watered ✓");
        return true;
    }

    /// <summary>
    /// Called each turn to progress growth.
    /// </summary>
    public void AdvanceTurn()
    {
        if (IsFullyGrown())
        {
            Debug.Log($"{seedData.itemName} is already fully grown");
            return;
        }

        if (seedData.requiresWater && needsWater)
        {
            Debug.Log($"{seedData.itemName} did not grow - needs water!");
            ShowWaterIcon(true);
            return;
        }

        if (seedData.requiresTilledSoil && !isOnTilledSoil)
        {
            Debug.Log($"{seedData.itemName} did not grow - needs tilled soil!");
            return;
        }

        turnsGrown++;

        int requiredTurns = GetRequiredTurns();
        if (turnsGrown >= requiredTurns)
        {
            currentStage = Mathf.Max(0, seedData.growthStages.Length - 1);
            Debug.Log($"{seedData.itemName} is fully grown and ready to harvest!");
        }
        else
        {
            float growthProgress = (float)turnsGrown / requiredTurns;
            currentStage = Mathf.FloorToInt(growthProgress * seedData.growthStages.Length);
            currentStage = Mathf.Clamp(currentStage, 0, Mathf.Max(0, seedData.growthStages.Length - 1));
            Debug.Log($"{seedData.itemName} grew to stage {currentStage}");
        }

        SpawnStage(currentStage);

        needsWater = seedData.requiresWater;
        ShowWaterIcon(needsWater);
    }

    /// <summary>
    /// Checks if plant is fully grown.
    /// </summary>
    public bool IsFullyGrown()
    {
        return turnsGrown >= GetRequiredTurns();
    }

    /// <summary>
    /// Harvests the plant and returns the crop data.
    /// </summary>
    public CropData Harvest()
    {
        if (!IsFullyGrown())
        {
            Debug.Log($"{seedData.itemName} is not ready to harvest yet!");
            return null;
        }

        CropData crop = seedData.producedCrop;

        if (crop == null)
        {
            Debug.LogError($"{seedData.itemName} has no crop assigned!");
            return null;
        }

        timesHarvested++;
        Debug.Log($"Harvested {crop.itemName} from {seedData.itemName}");

        if (seedData.isMultiHarvest && timesHarvested < seedData.harvestsPerPlant)
        {
            turnsGrown = 0;
            needsWater = true;
            currentStage = Mathf.Max(0, seedData.growthStages.Length - 2);
            SpawnStage(currentStage);
            ShowWaterIcon(true);

            Debug.Log($"{seedData.itemName} will regrow ({timesHarvested}/{seedData.harvestsPerPlant} harvests)");
        }
        else
        {
            Debug.Log($"{seedData.itemName} has been fully harvested");

            // Free the tile immediately so a new seed can be planted this frame
            DisableColliders();
            CleanupFromManager();
            Destroy(gameObject);
        }

        return crop;
    }

    /// <summary>
    /// Spawns the visual for the current growth stage.
    /// </summary>
    private void SpawnStage(int stage)
    {
        if (seedData == null || seedData.growthStages == null) return;

        if (currentVisual != null)
        {
            Destroy(currentVisual);
        }

        if (stage < 0 || stage >= seedData.growthStages.Length)
        {
            Debug.LogWarning($"Invalid growth stage {stage} for {seedData.itemName}");
            return;
        }

        GameObject stagePrefab = seedData.growthStages[stage];
        if (stagePrefab != null)
        {
            currentVisual = Instantiate(stagePrefab, transform.position, Quaternion.identity, transform);
        }

        if (spriteRenderer != null && stagePrefab != null)
        {
            SpriteRenderer stageSpriteRenderer = stagePrefab.GetComponent<SpriteRenderer>();
            if (stageSpriteRenderer != null)
            {
                spriteRenderer.sprite = stageSpriteRenderer.sprite;
            }
        }
    }

    /// <summary>
    /// Shows or hides the water droplet icon.
    /// </summary>
    private void ShowWaterIcon(bool show)
    {
        if (show)
        {
            if (waterIconInstance == null && waterIconPrefab != null)
            {
                Vector3 iconPos = transform.position + Vector3.up * 0.5f;
                waterIconInstance = Instantiate(waterIconPrefab, iconPos, Quaternion.identity, transform);
                Debug.Log($"💧 Water droplet shown for {seedData.itemName}");
            }
        }
        else
        {
            if (waterIconInstance != null)
            {
                Destroy(waterIconInstance);
                waterIconInstance = null;
                Debug.Log($"💧 Water droplet hidden for {seedData.itemName}");
            }
        }
    }

    private int GetRequiredTurns()
    {
        if (seedData == null) return 1;

        int turns = seedData.GetModifiedGrowthTime(currentSeason);
        if (turns <= 0)
        {
            turns = seedData.GetCurrentGrowth();
        }

        return Mathf.Max(1, turns);
    }

    private void CleanupFromManager()
    {
        if (PlantManager.Instance != null)
        {
            PlantManager.Instance.RemovePlant(gameObject);
        }
    }

    private void DisableColliders()
    {
        // Prevent lingering colliders from blocking immediate replanting
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
    }

    private void OnDestroy()
    {
        CleanupFromManager();
    }
}

