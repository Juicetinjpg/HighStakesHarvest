using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central database manager for all items in the game
/// Provides easy access to seeds, crops, tools, and resources
/// </summary>
public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }
    
    [Header("Item Collections")]
    public List<SeedData> allSeeds = new List<SeedData>();
    public List<CropData> allCrops = new List<CropData>();
    public List<ToolData> allTools = new List<ToolData>();
    public List<ResourceData> allResources = new List<ResourceData>();
    public List<ItemData> allOtherItems = new List<ItemData>();
    
    private Dictionary<string, ItemData> itemLookup = new Dictionary<string, ItemData>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Loads all items and creates lookup dictionary
    /// </summary>
    private void InitializeDatabase()
    {
        itemLookup.Clear();
        
        // Add all items to lookup
        foreach (var seed in allSeeds)
        {
            if (seed != null && !itemLookup.ContainsKey(seed.itemName))
                itemLookup.Add(seed.itemName, seed);
        }
        
        foreach (var crop in allCrops)
        {
            if (crop != null && !itemLookup.ContainsKey(crop.itemName))
                itemLookup.Add(crop.itemName, crop);
        }
        
        foreach (var tool in allTools)
        {
            if (tool != null && !itemLookup.ContainsKey(tool.itemName))
                itemLookup.Add(tool.itemName, tool);
        }
        
        foreach (var resource in allResources)
        {
            if (resource != null && !itemLookup.ContainsKey(resource.itemName))
                itemLookup.Add(resource.itemName, resource);
        }
        
        foreach (var item in allOtherItems)
        {
            if (item != null && !itemLookup.ContainsKey(item.itemName))
                itemLookup.Add(item.itemName, item);
        }
        
        Debug.Log($"Item Database initialized with {itemLookup.Count} items");
    }
    
    /// <summary>
    /// Gets an item by name
    /// </summary>
    public ItemData GetItem(string itemName)
    {
        if (itemLookup.TryGetValue(itemName, out ItemData item))
        {
            return item;
        }
        
        Debug.LogWarning($"Item '{itemName}' not found in database!");
        return null;
    }
    
    /// <summary>
    /// Gets a seed by name
    /// </summary>
    public SeedData GetSeed(string seedName)
    {
        return allSeeds.FirstOrDefault(s => s != null && s.itemName == seedName);
    }
    
    /// <summary>
    /// Gets a crop by name
    /// </summary>
    public CropData GetCrop(string cropName)
    {
        return allCrops.FirstOrDefault(c => c != null && c.itemName == cropName);
    }
    
    /// <summary>
    /// Gets a tool by name
    /// </summary>
    public ToolData GetTool(string toolName)
    {
        return allTools.FirstOrDefault(t => t != null && t.itemName == toolName);
    }
    
    /// <summary>
    /// Gets all seeds available in a specific season
    /// </summary>
    public List<SeedData> GetSeedsBySeason(string season)
    {
        return allSeeds.Where(s => s != null && s.CanPlantInSeason(season)).ToList();
    }
    
    /// <summary>
    /// Gets all tools of a specific category
    /// </summary>
    public List<ToolData> GetToolsByCategory(ToolCategory category)
    {
        return allTools.Where(t => t != null && t.toolCategory == category).ToList();
    }
    
    /// <summary>
    /// Gets all tools of a specific tier
    /// </summary>
    public List<ToolData> GetToolsByTier(ToolTier tier)
    {
        return allTools.Where(t => t != null && t.tier == tier).ToList();
    }
    
    /// <summary>
    /// Gets all items of a specific type
    /// </summary>
    public List<ItemData> GetItemsByType(ItemType type)
    {
        return itemLookup.Values.Where(i => i != null && i.itemType == type).ToList();
    }
    
    /// <summary>
    /// Gets all tradeable items
    /// </summary>
    public List<ItemData> GetTradeableItems()
    {
        return itemLookup.Values.Where(i => i != null && i.isTradeable).ToList();
    }
    
    /// <summary>
    /// Gets the crop produced by a seed
    /// </summary>
    public CropData GetCropFromSeed(SeedData seed)
    {
        return seed != null ? seed.producedCrop : null;
    }
    
    /// <summary>
    /// Finds which seed produces a specific crop
    /// </summary>
    public SeedData GetSeedForCrop(CropData crop)
    {
        return allSeeds.FirstOrDefault(s => s != null && s.producedCrop == crop);
    }
    
    /// <summary>
    /// Gets all multi-harvest crops
    /// </summary>
    public List<SeedData> GetMultiHarvestSeeds()
    {
        return allSeeds.Where(s => s != null && s.isMultiHarvest).ToList();
    }
    
    /// <summary>
    /// Gets crops that can be used as animal feed
    /// </summary>
    public List<CropData> GetAnimalFeedCrops()
    {
        return allCrops.Where(c => c != null && c.canBeUsedAsAnimalFeed).ToList();
    }
    
    /// <summary>
    /// Validates all item references
    /// </summary>
    [ContextMenu("Validate Database")]
    public void ValidateDatabase()
    {
        Debug.Log("=== Validating Item Database ===");
        
        // Check for null references
        int nullSeeds = allSeeds.Count(s => s == null);
        int nullCrops = allCrops.Count(c => c == null);
        int nullTools = allTools.Count(t => t == null);
        int nullResources = allResources.Count(r => r == null);
        
        if (nullSeeds > 0) Debug.LogWarning($"Found {nullSeeds} null seed references!");
        if (nullCrops > 0) Debug.LogWarning($"Found {nullCrops} null crop references!");
        if (nullTools > 0) Debug.LogWarning($"Found {nullTools} null tool references!");
        if (nullResources > 0) Debug.LogWarning($"Found {nullResources} null resource references!");
        
        // Check for seeds without crops
        foreach (var seed in allSeeds.Where(s => s != null))
        {
            if (seed.producedCrop == null)
            {
                Debug.LogWarning($"Seed '{seed.itemName}' has no crop assigned!");
            }
        }
        
        // Check for crops without source seeds
        foreach (var crop in allCrops.Where(c => c != null))
        {
            if (crop.sourceSeed == null)
            {
                Debug.LogWarning($"Crop '{crop.itemName}' has no source seed assigned!");
            }
        }
        
        // Check for duplicate names
        var duplicates = itemLookup.GroupBy(x => x.Key)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
        
        foreach (var duplicate in duplicates)
        {
            Debug.LogError($"Duplicate item name found: '{duplicate}'");
        }
        
        Debug.Log($"Validation complete. Total items: {itemLookup.Count}");
    }
    
    /// <summary>
    /// Reloads the database (useful for editor changes)
    /// </summary>
    [ContextMenu("Reload Database")]
    public void ReloadDatabase()
    {
        InitializeDatabase();
        Debug.Log("Database reloaded!");
    }
}
