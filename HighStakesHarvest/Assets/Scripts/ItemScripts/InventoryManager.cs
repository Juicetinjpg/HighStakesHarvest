using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Extension of PlayerInventory that adds ItemData support
/// Add this component alongside your existing PlayerInventory
/// Provides methods to add/remove items using ItemData ScriptableObjects
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    private PlayerInventory baseInventory;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        baseInventory = GetComponent<PlayerInventory>();
        if (baseInventory == null)
        {
            baseInventory = gameObject.AddComponent<PlayerInventory>();
        }
    }
    
    /// <summary>
    /// Adds an ItemData to inventory
    /// </summary>
    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;
        
        return baseInventory.AddItem(item.itemName, quantity, item.itemType.ToString());
    }
    
    /// <summary>
    /// Removes an ItemData from inventory
    /// </summary>
    public bool RemoveItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;
        
        return baseInventory.RemoveItem(item.itemName, quantity);
    }
    
    /// <summary>
    /// Checks if inventory has specific ItemData
    /// </summary>
    public bool HasItem(ItemData item, int quantity = 1)
    {
        if (item == null) return false;
        
        return baseInventory.HasItem(item.itemName, quantity);
    }
    
    /// <summary>
    /// Gets quantity of specific ItemData
    /// </summary>
    public int GetItemQuantity(ItemData item)
    {
        if (item == null) return 0;
        
        return baseInventory.GetItemQuantity(item.itemName);
    }
    
    /// <summary>
    /// Uses an item from inventory (calls item.Use() and removes if consumable)
    /// </summary>
    public bool UseItem(ItemData item, GameObject user)
    {
        if (item == null || !HasItem(item, 1)) return false;
        
        bool success = item.Use(user);
        
        if (success)
        {
            // Tools don't get consumed
            if (item.itemType != ItemType.Tool)
            {
                RemoveItem(item, 1);
            }
        }
        
        return success;
    }
    
    /// <summary>
    /// Sells item from inventory
    /// </summary>
    public bool SellItem(ItemData item, int quantity, out int totalValue)
    {
        totalValue = 0;
        
        if (item == null || !item.isTradeable || !HasItem(item, quantity))
        {
            return false;
        }
        
        totalValue = item.GetSellPrice() * quantity;
        
        if (RemoveItem(item, quantity))
        {
            // TODO: Add money to player
            Debug.Log($"Sold {quantity}x {item.itemName} for ${totalValue}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Gets the ItemData from a slot
    /// </summary>
    public ItemData GetItemDataFromSlot(int slotIndex)
    {
        InventorySlot slot = baseInventory.GetSlot(slotIndex);
        
        if (slot == null || slot.IsEmpty) return null;
        
        // Look up ItemData from database
        if (ItemDatabase.Instance != null)
        {
            return ItemDatabase.Instance.GetItem(slot.itemName);
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all ItemData objects of a specific type in inventory
    /// </summary>
    public List<ItemData> GetItemsByType(ItemType type)
    {
        List<ItemData> items = new List<ItemData>();
        
        List<InventorySlot> slots = baseInventory.GetItemsByType(type.ToString());
        
        foreach (var slot in slots)
        {
            ItemData item = ItemDatabase.Instance?.GetItem(slot.itemName);
            if (item != null && !items.Contains(item))
            {
                items.Add(item);
            }
        }
        
        return items;
    }
    
    /// <summary>
    /// Gets all seeds in inventory
    /// </summary>
    public List<SeedData> GetSeeds()
    {
        List<SeedData> seeds = new List<SeedData>();
        
        var seedItems = GetItemsByType(ItemType.Seed);
        
        foreach (var item in seedItems)
        {
            if (item is SeedData seed)
            {
                seeds.Add(seed);
            }
        }
        
        return seeds;
    }
    
    /// <summary>
    /// Gets all crops in inventory
    /// </summary>
    public List<CropData> GetCrops()
    {
        List<CropData> crops = new List<CropData>();
        
        var cropItems = GetItemsByType(ItemType.Crop);
        
        foreach (var item in cropItems)
        {
            if (item is CropData crop)
            {
                crops.Add(crop);
            }
        }
        
        return crops;
    }
    
    /// <summary>
    /// Gets all tools in inventory
    /// </summary>
    public List<ToolData> GetTools()
    {
        List<ToolData> tools = new List<ToolData>();
        
        var toolItems = GetItemsByType(ItemType.Tool);
        
        foreach (var item in toolItems)
        {
            if (item is ToolData tool)
            {
                tools.Add(tool);
            }
        }
        
        return tools;
    }
    
    /// <summary>
    /// Gets the equipped tool from hotbar
    /// </summary>
    public ToolData GetEquippedTool()
    {
        if (HotbarSystem.Instance == null) return null;
        
        ItemData equippedItem = GetItemDataFromSlot(HotbarSystem.Instance.CurrentSlot);
        
        return equippedItem as ToolData;
    }
    
    /// <summary>
    /// Gets the equipped seed from hotbar
    /// </summary>
    public SeedData GetEquippedSeed()
    {
        if (HotbarSystem.Instance == null) return null;
        
        ItemData equippedItem = GetItemDataFromSlot(HotbarSystem.Instance.CurrentSlot);
        
        return equippedItem as SeedData;
    }
    
    /// <summary>
    /// Plants a seed from hotbar (removes from inventory)
    /// </summary>
    public bool PlantEquippedSeed(Vector3 position, string currentSeason)
    {
        SeedData seed = GetEquippedSeed();
        
        if (seed == null)
        {
            Debug.Log("No seed equipped!");
            return false;
        }
        
        // Check if can plant in current season
        if (!seed.CanPlantInSeason(currentSeason))
        {
            Debug.Log($"{seed.itemName} cannot be planted in {currentSeason}!");
            return false;
        }
        
        // Remove seed from inventory
        if (!RemoveItem(seed, 1))
        {
            Debug.Log("Failed to remove seed from inventory!");
            return false;
        }
        
        // Create plant
        GameObject plantObj = new GameObject($"Plant_{seed.itemName}");
        plantObj.transform.position = position;
        
        PlantGrowth plant = plantObj.AddComponent<PlantGrowth>();
        plant.seedData = seed;
        plant.currentSeason = currentSeason;
        
        Debug.Log($"Planted {seed.itemName} at {position}");
        return true;
    }
    
    /// <summary>
    /// Uses equipped tool
    /// </summary>
    public bool UseEquippedTool(GameObject user)
    {
        ToolData tool = GetEquippedTool();
        
        if (tool == null)
        {
            Debug.Log("No tool equipped!");
            return false;
        }
        
        return tool.Use(user);
    }
    
    /// <summary>
    /// Adds crop to inventory after harvesting
    /// </summary>
    public bool AddHarvestedCrop(CropData crop, int yieldAmount)
    {
        if (crop == null || yieldAmount <= 0) return false;
        
        return AddItem(crop, yieldAmount);
    }
    
    /// <summary>
    /// Quick sell all crops of a specific type
    /// </summary>
    public int SellAllCrops(CropData crop)
    {
        if (crop == null) return 0;
        
        int quantity = GetItemQuantity(crop);
        if (quantity <= 0) return 0;
        
        if (SellItem(crop, quantity, out int totalValue))
        {
            return totalValue;
        }
        
        return 0;
    }
}
