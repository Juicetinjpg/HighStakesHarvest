using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

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
    public Tilemap interactableTilemap;
    public CropManager cropManager;
    public GameObject waterIconPrefab; // Your water droplet prefab
    public GameObject Player;

    // list of seed prefabs
    [SerializeField] public List<GameObject> seedList = new List<GameObject>();
    public Dictionary<string, GameObject> seedDict = new Dictionary<string, GameObject>();
    [SerializeField] private List<ToolCategory> harvestToolCategories = new List<ToolCategory> { ToolCategory.Sickle, ToolCategory.Scythe };

    [Header("Visual Feedback")]
    public Color validPlacementColor = new Color(0, 1, 0, 0.3f);
    public Color invalidPlacementColor = new Color(1, 0, 0, 0.3f);

    private void Start()
    {
        Player = GameObject.Find("Player");
        cropManager = GameObject.Find("CropManager").GetComponent<CropManager>();  

        Debug.Log("Starting dict");
        foreach (GameObject i in seedList)
        {
            PlantGrowth plant = i.GetComponent<PlantGrowth>();
            if (plant == null)
            {
                Debug.LogWarning($"Seed prefab {i.name} is missing PlantGrowth");
                continue;
            }

            string key = !string.IsNullOrEmpty(plant.cropName) ? plant.cropName : plant.seedData?.cropName;
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"Seed prefab {i.name} has no cropName; skipping dictionary add");
                continue;
            }

            seedDict.Add(key, i);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Get world position from mouse and convert to grid position
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            Vector3Int cellPos = soilTilemap.WorldToCell(worldPos);
            Vector3 placePos = soilTilemap.GetCellCenterWorld(cellPos);

            Vector3 playerWorldPosition = Player.transform.position;

            // Convert player world position to cell position on the tilemap
            Vector3Int playerCellPosition = soilTilemap.WorldToCell(playerWorldPosition);

            int thresholdX = Mathf.Abs((int)(placePos.x - playerCellPosition.x));
            int thresholdY = Mathf.Abs((int)(placePos.y - playerCellPosition.y));

            // magic number : change to playerRange
            if (thresholdX < 2 && thresholdY < 2)
            {
                HandleMouseClick(placePos);
            }
            else
            {
                Debug.Log("Tile not in range");
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            // get player position - to reuse in mouseclick as well 

            Vector3 playerWorldPosition = Player.transform.position;

            // Convert world position to cell position on the tilemap
            Vector3Int playerCellPosition = soilTilemap.WorldToCell(playerWorldPosition);

            Vector3 placePos = soilTilemap.GetCellCenterWorld(playerCellPosition);

            HandleMouseClick(placePos);

        }
    }

    private void HandleMouseClick(Vector3 pos)
    {
        

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
            //TryHarvestPlant(pos, null); // hacky solution to test harvest without tool
            return;
        }

        // Handle different item types
        switch (equippedItem.itemType)
        {
            case ItemType.Seed:
                TryPlantSeed(pos, equippedItem as SeedData);
                break;

            case ItemType.Tool:
                TryUseTool(pos, equippedItem as ToolData);
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
        /** Convert interacted position to a vector position for object referencing **/

        // Convert world position to cell position on the tilemap
        Vector3Int cellPos = soilTilemap.WorldToCell(placePos);

        // Convert cell position back to vector position in relation to center of tile
        Vector3 objectPos = new Vector3(cellPos.x + (float)0.5, cellPos.y + (float)0.5);

        // get layer mask with plant objects to ensure collision issues aren't caused
        int defaultLayerMask = LayerMask.GetMask("Default");

        // get object at position of interaction on the layer mask
        Collider2D hit = Physics2D.OverlapPoint(objectPos, defaultLayerMask);

        // above lines are reused in other functions

        // get interactable tile for checking if is interactable (if tile on interactable map exists)
        TileBase interactable = interactableTilemap.GetTile(cellPos);


        if (seed == null) return;

        if (hit != null || interactable == null)
        {
            Debug.Log("Cannot plant here - space already occupied");
            return;
        }

        // Check if can plant in current season
        bool seasonsEnabled = TurnManager.Instance != null && TurnManager.Instance.enableSeasons;
        string currentSeason = TurnManager.Instance != null ? TurnManager.Instance.GetCurrentSeason() : "Spring";

        if (seasonsEnabled && !seed.CanPlantInSeason(currentSeason))
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

       

        if (!seedDict.TryGetValue(seed.cropName, out GameObject seedPrefab))
        {
            Debug.LogWarning($"No prefab registered for seed '{seed.cropName}'. Add it to PlantPlacer.seedList.");
            InventoryManager.Instance.AddItem(seed, 1);
            return;
        }

        GameObject go = Instantiate(seedPrefab, objectPos, Quaternion.identity);

        // set the growth time according to whatever is in seed data..?
        //go.GetComponent<Plant>().seedData.growthTime = cropManager.cropInfoDictionary[seed.cropName].growth;
        
        // change growthrate to be whats stored in cropinfo

        // Register with PlantManager for persistence and turn management
        if (PlantManager.Instance != null)
        {
            PlantManager.Instance.AddPlant(go);
        }
        else
        {
            DontDestroyOnLoad(go);
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
        // Convert world position to cell position on the tilemap
        Vector3Int cellPos = soilTilemap.WorldToCell(placePos);

        // Convert cell position back to vector position in relation to center of tile
        Vector3 objectPos = new Vector3(cellPos.x + (float)0.5, cellPos.y + (float)0.5);

        // get layer mask with plant objects to ensure collision issues aren't caused
        int defaultLayerMask = LayerMask.GetMask("Default");

        // get object at position of interaction on the layer mask
        Collider2D hit = Physics2D.OverlapPoint(objectPos, defaultLayerMask);

        if (hit == null)
        {
            Debug.Log("No plant here to water");
            return;
        }

        PlantGrowth plant = hit.GetComponent<PlantGrowth>();
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
        if (!IsHarvestTool(tool))
        {
            Debug.Log($"{tool?.itemName ?? "This item"} cannot harvest crops");
            return;
        }

        Vector3Int cellPos = soilTilemap.WorldToCell(placePos);
        Vector3 objectPos = new Vector3(cellPos.x + (float)0.5, cellPos.y + (float)0.5);
        int defaultLayerMask = LayerMask.GetMask("Default");
        Collider2D hit = Physics2D.OverlapPoint(objectPos, defaultLayerMask);

        if (hit == null)
        {
            Debug.Log("No plant here to harvest");
            return;
        }

        PlantGrowth plant = hit.GetComponent<PlantGrowth>();
        if (plant == null)
        {
            Debug.Log("Not a plant");
            return;
        }

        if (!plant.IsFullyGrown())
        {
            Debug.Log($"{plant.seedData.itemName} is not ready to harvest yet! Growth time is {plant.seedData.GetCurrentGrowth()}");
            return;
        }

        if (!InventoryManager.Instance.UseEquippedTool(gameObject))
        {
            Debug.Log($"Could not use {tool.itemName} to harvest");
            return;
        }

        CropData harvestedCrop = plant.Harvest();

        if (harvestedCrop != null)
        {
            int yieldAmount = 1;
            if (cropManager != null && cropManager.cropInfoDictionary.TryGetValue(harvestedCrop.cropName, out CropInfo info))
            {
                yieldAmount = Mathf.Max(1, info.quantity);
            }
            else
            {
                yieldAmount = harvestedCrop.GetYieldAmount();
                Debug.LogWarning($"No crop info found for {harvestedCrop.cropName}; using data-defined yield {yieldAmount}");
            }

            if (InventoryManager.Instance.AddHarvestedCrop(harvestedCrop, yieldAmount))
            {
                Debug.Log($"✓ Harvested {yieldAmount}x {harvestedCrop.itemName}!");
            }
            else
            {
                Debug.LogWarning($"Failed to add {harvestedCrop.itemName} to inventory");
            }

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

    private bool IsHarvestTool(ToolData tool)
    {
        return tool != null && harvestToolCategories != null && harvestToolCategories.Contains(tool.toolCategory);
    }
}
