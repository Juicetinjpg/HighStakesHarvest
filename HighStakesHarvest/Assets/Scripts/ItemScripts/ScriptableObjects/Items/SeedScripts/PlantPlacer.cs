using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Inventory-based planting system that works with your tilemap grid
/// Replaces the old numbered plant system (Plant1, Plant2, Plant3)
/// Now uses items from hotbar - quantity decreases as you use them
/// Keeps all visual feedback (water droplets, growth stages)
/// </summary>
public class PlantPlacer : MonoBehaviour
{
    [Header("References")]
    public Tilemap soilTilemap;
    public GameObject waterIconPrefab; // Your water droplet prefab

    [Header("Visual Feedback")]
    public Color validPlacementColor = new Color(0, 1, 0, 0.3f);
    public Color invalidPlacementColor = new Color(1, 0, 0, 0.3f);

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        // Get world position from mouse and convert to grid position
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        Vector3Int cellPos = soilTilemap.WorldToCell(worldPos);
        Vector3 placePos = soilTilemap.GetCellCenterWorld(cellPos);

        // Get what's currently equipped in the hotbar
        if (HotbarManager.Instance == null)
        {
            Debug.LogWarning("HotbarManager not found! Make sure it's attached to your HotbarSystem GameObject");
            return;
        }

        ItemData equippedItem = HotbarManager.Instance.GetEquippedItemData();

        if (equippedItem == null)
        {
            Debug.Log("No item equipped. Select an item from your hotbar (keys 1-9, 0)");
            return;
        }

        // Handle different item types
        switch (equippedItem.itemType)
        {
            case ItemType.Seed:
                TryPlantSeed(placePos, equippedItem as SeedData);
                break;

            case ItemType.Tool:
                TryUseTool(placePos, equippedItem as ToolData);
                break;

            default:
                Debug.Log($"{equippedItem.itemName} cannot be used here");
                break;
        }
    }

    /// <summary>
    /// Plants a seed at the specified position
    /// </summary>
    private void TryPlantSeed(Vector3 placePos, SeedData seed)
    {
        if (seed == null) return;

        // Check if spot is already occupied
        Collider2D hit = Physics2D.OverlapPoint(placePos);
        if (hit != null)
        {
            Debug.Log("Cannot plant here - space already occupied");
            return;
        }

        // Check if can plant in current season
        string currentSeason = TurnManager.Instance != null ? TurnManager.Instance.GetCurrentSeason() : "Spring";

        if (!seed.CanPlantInSeason(currentSeason))
        {
            Debug.Log($"Cannot plant {seed.itemName} in {currentSeason}! Prefers: {seed.seasonPreference}");
            return;
        }

        // Check if we have the seed in inventory
        if (!InventoryManager.Instance.HasItem(seed, 1))
        {
            Debug.Log($"No {seed.itemName} in inventory!");
            return;
        }

        // Remove seed from inventory (this decreases quantity)
        if (!InventoryManager.Instance.RemoveItem(seed, 1))
        {
            Debug.Log("Failed to remove seed from inventory");
            return;
        }

        // Create the plant GameObject
        GameObject plantObj = new GameObject($"Plant_{seed.itemName}");
        plantObj.transform.position = placePos;
        plantObj.layer = LayerMask.NameToLayer("Default"); // Or your plant layer

        // Add collider so we can detect it's there
        CircleCollider2D collider = plantObj.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;

        // Add the Plant component (uses the currentSeason from above)
        Plant plant = plantObj.AddComponent<Plant>();
        plant.Initialize(seed, waterIconPrefab, currentSeason);

        // Register with PlantManager for persistence and turn management
        if (PlantManager.Instance != null)
        {
            PlantManager.Instance.AddPlant(plantObj);
        }
        else
        {
            DontDestroyOnLoad(plantObj);
        }

        Debug.Log($"✓ Planted {seed.itemName} at {placePos}");
    }

    /// <summary>
    /// Uses a tool at the specified position
    /// </summary>
    private void TryUseTool(Vector3 placePos, ToolData tool)
    {
        if (tool == null) return;

        // Check what tool it is
        switch (tool.toolCategory)
        {
            case ToolCategory.WateringCan:
                TryWaterPlant(placePos, tool);
                break;

            case ToolCategory.Sickle:
            case ToolCategory.Scythe:
                TryHarvestPlant(placePos, tool);
                break;

            case ToolCategory.Hoe:
                TryTillSoil(placePos, tool);
                break;

            default:
                Debug.Log($"{tool.itemName} cannot be used here");
                break;
        }
    }

    /// <summary>
    /// Waters a plant at the position
    /// </summary>
    private void TryWaterPlant(Vector3 placePos, ToolData tool)
    {
        Collider2D hit = Physics2D.OverlapPoint(placePos);
        if (hit == null)
        {
            Debug.Log("No plant here to water");
            return;
        }

        Plant plant = hit.GetComponent<Plant>();
        if (plant == null)
        {
            Debug.Log("Not a plant");
            return;
        }

        // Use the tool (decreases durability/capacity)
        if (InventoryManager.Instance.UseEquippedTool(gameObject))
        {
            plant.Water();
            Debug.Log($"✓ Watered {plant.seedData.itemName}");
        }
    }

    /// <summary>
    /// Harvests a plant at the position
    /// </summary>
    private void TryHarvestPlant(Vector3 placePos, ToolData tool)
    {
        Collider2D hit = Physics2D.OverlapPoint(placePos);
        if (hit == null)
        {
            Debug.Log("No plant here to harvest");
            return;
        }

        Plant plant = hit.GetComponent<Plant>();
        if (plant == null)
        {
            Debug.Log("Not a plant");
            return;
        }

        if (!plant.IsFullyGrown())
        {
            Debug.Log($"{plant.seedData.itemName} is not ready to harvest yet!");
            return;
        }

        // Use the tool
        InventoryManager.Instance.UseEquippedTool(gameObject);

        // Harvest the plant
        CropData harvestedCrop = plant.Harvest();

        if (harvestedCrop != null)
        {
            int yieldAmount = harvestedCrop.GetYieldAmount();

            // Add to inventory
            if (InventoryManager.Instance.AddHarvestedCrop(harvestedCrop, yieldAmount))
            {
                Debug.Log($"✓ Harvested {yieldAmount}x {harvestedCrop.itemName}!");
            }

            // If plant doesn't regrow, remove it from PlantManager
            if (!plant.seedData.isMultiHarvest || plant.timesHarvested >= plant.seedData.harvestsPerPlant)
            {
                if (PlantManager.Instance != null)
                {
                    PlantManager.Instance.RemovePlant(plant.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Tills soil at the position (optional feature)
    /// </summary>
    private void TryTillSoil(Vector3 placePos, ToolData tool)
    {
        // Use the tool
        InventoryManager.Instance.UseEquippedTool(gameObject);

        // You can implement tilling logic here if needed
        Debug.Log($"Tilled soil at {placePos}");

        // Could change the tile to a tilled version:
        // Vector3Int cellPos = soilTilemap.WorldToCell(placePos);
        // soilTilemap.SetTile(cellPos, tilledSoilTile);
    }
}