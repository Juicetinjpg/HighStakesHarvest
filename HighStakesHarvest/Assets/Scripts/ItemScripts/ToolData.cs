using UnityEngine;

/// <summary>
/// Represents farming tools like hoes, watering cans, axes, pickaxes, etc.
/// </summary>
[CreateAssetMenu(fileName = "NewTool", menuName = "Farming/Tool Data")]
public class ToolData : ItemData
{
    [Header("Tool Properties")]
    public ToolCategory toolCategory;
    public int durability = 100; // How many uses before breaking
    public bool isUnbreakable = false;
    public int currentDurability; // Tracked at runtime
    
    [Header("Tool Effectiveness")]
    public int effectiveness = 1; // How effective the tool is (e.g., 3x3 area vs 1x1)
    public float speedMultiplier = 1.0f; // How fast the action completes
    public int range = 1; // How far the tool can reach
    
    [Header("Upgrades")]
    public ToolTier tier = ToolTier.Basic;
    public ToolData upgradeVersion; // Reference to the upgraded version of this tool
    public int upgradePrice;
    
    [Header("Special Properties")]
    public bool requiresRefill = false; // For watering cans
    public int maxCapacity = 0; // For watering cans
    public int currentCapacity = 0; // Runtime tracking
    
    [Header("Visual & Audio")]
    public GameObject toolPrefab; // The 3D/2D model when equipped
    public AudioClip useSound;
    public AnimationClip useAnimation;
    
    private void OnValidate()
    {
        itemType = ItemType.Tool;
        stackSize = 1; // Tools don't stack
        
        // Initialize durability
        if (currentDurability == 0)
        {
            currentDurability = durability;
        }
        
        // Initialize capacity for watering tools
        if (requiresRefill && currentCapacity == 0)
        {
            currentCapacity = maxCapacity;
        }
    }
    
    /// <summary>
    /// Uses the tool and decreases durability
    /// </summary>
    public override bool Use(GameObject user)
    {
        // Check if tool is broken
        if (!isUnbreakable && currentDurability <= 0)
        {
            Debug.Log($"{itemName} is broken and needs repair!");
            return false;
        }
        
        // Check if tool needs refill (watering can)
        if (requiresRefill && currentCapacity <= 0)
        {
            Debug.Log($"{itemName} needs to be refilled!");
            return false;
        }
        
        // Perform tool-specific action
        bool success = PerformToolAction(user);
        
        if (success)
        {
            // Decrease durability
            if (!isUnbreakable)
            {
                currentDurability--;
            }
            
            // Decrease capacity if applicable
            if (requiresRefill)
            {
                currentCapacity--;
            }
            
            // Play sound
            if (useSound != null)
            {
                AudioSource.PlayClipAtPoint(useSound, user.transform.position);
            }
        }
        
        return success;
    }
    
    /// <summary>
    /// Tool-specific action logic
    /// </summary>
    private bool PerformToolAction(GameObject user)
    {
        switch (toolCategory)
        {
            case ToolCategory.Hoe:
                return TillSoil(user);
            case ToolCategory.WateringCan:
                return WaterCrops(user);
            case ToolCategory.Axe:
                return ChopTree(user);
            case ToolCategory.Pickaxe:
                return MineRock(user);
            case ToolCategory.Sickle:
                return HarvestCrops(user);
            case ToolCategory.Scythe:
                return ClearArea(user);
            default:
                Debug.Log($"Tool action not implemented for {toolCategory}");
                return false;
        }
    }
    
    // Tool-specific action methods (implement these based on your game's systems)
    private bool TillSoil(GameObject user)
    {
        Debug.Log($"Tilling soil with {itemName} (Effectiveness: {effectiveness}x{effectiveness})");
        // TODO: Implement tilling logic with your soil system
        return true;
    }
    
    private bool WaterCrops(GameObject user)
    {
        Debug.Log($"Watering crops with {itemName} (Range: {range}, Capacity: {currentCapacity}/{maxCapacity})");
        // TODO: Implement watering logic
        return true;
    }
    
    private bool ChopTree(GameObject user)
    {
        Debug.Log($"Chopping tree with {itemName} (Effectiveness: {effectiveness})");
        // TODO: Implement tree chopping logic
        return true;
    }
    
    private bool MineRock(GameObject user)
    {
        Debug.Log($"Mining rock with {itemName} (Effectiveness: {effectiveness})");
        // TODO: Implement rock mining logic
        return true;
    }
    
    private bool HarvestCrops(GameObject user)
    {
        Debug.Log($"Harvesting crops with {itemName} (Range: {range})");
        // TODO: Implement harvesting logic
        return true;
    }
    
    private bool ClearArea(GameObject user)
    {
        Debug.Log($"Clearing area with {itemName} (Area: {effectiveness}x{effectiveness})");
        // TODO: Implement area clearing logic
        return true;
    }
    
    /// <summary>
    /// Repairs the tool to full durability
    /// </summary>
    public void Repair()
    {
        currentDurability = durability;
        Debug.Log($"{itemName} has been repaired!");
    }
    
    /// <summary>
    /// Refills the tool (for watering cans)
    /// </summary>
    public void Refill()
    {
        if (requiresRefill)
        {
            currentCapacity = maxCapacity;
            Debug.Log($"{itemName} has been refilled!");
        }
    }
    
    /// <summary>
    /// Checks if tool can be upgraded
    /// </summary>
    public bool CanUpgrade()
    {
        return upgradeVersion != null;
    }
    
    /// <summary>
    /// Gets durability as a percentage
    /// </summary>
    public float GetDurabilityPercent()
    {
        if (isUnbreakable) return 1f;
        return (float)currentDurability / durability;
    }
    
    public override string GetDisplayInfo()
    {
        string info = base.GetDisplayInfo();
        info += $"\nTier: {tier}";
        info += $"\nCategory: {toolCategory}";
        
        if (!isUnbreakable)
        {
            info += $"\nDurability: {currentDurability}/{durability}";
        }
        else
        {
            info += "\nDurability: Unbreakable";
        }
        
        if (requiresRefill)
        {
            info += $"\nCapacity: {currentCapacity}/{maxCapacity}";
        }
        
        if (effectiveness > 1)
        {
            info += $"\nArea: {effectiveness}x{effectiveness}";
        }
        
        if (CanUpgrade())
        {
            info += $"\nUpgrade Available: ${upgradePrice}";
        }
        
        return info;
    }
}

/// <summary>
/// Defines the different categories of tools
/// </summary>
public enum ToolCategory
{
    Hoe,          // Tills soil
    WateringCan,  // Waters crops
    Axe,          // Chops trees
    Pickaxe,      // Breaks rocks
    Sickle,       // Harvests crops
    Scythe,       // Clears large areas
    Hammer,       // Repairs/builds structures
    FishingRod    // Catches fish
}

/// <summary>
/// Defines tool quality tiers
/// </summary>
public enum ToolTier
{
    Basic,
    Copper,
    Iron,
    Gold,
    Iridium,
    Mystical
}
