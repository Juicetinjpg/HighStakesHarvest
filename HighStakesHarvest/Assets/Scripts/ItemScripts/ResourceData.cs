using UnityEngine;

/// <summary>
/// Represents raw resources gathered from the environment (wood, stone, ore, etc.)
/// Can be used for crafting, building, or selling
/// </summary>
[CreateAssetMenu(fileName = "NewResource", menuName = "Farming/Resource Data")]
public class ResourceData : ItemData
{
    [Header("Resource Properties")]
    public ResourceCategory category;
    public int harvestAmount = 1; // How much you get per harvest
    
    [Header("Source")]
    public GameObject sourceObject; // Tree, rock, etc. that drops this
    public ToolCategory requiredTool; // What tool is needed to gather this
    public ToolTier minimumToolTier = ToolTier.Basic;
    
    [Header("Usage")]
    public bool canBeProcessed = false; // Can be refined into something else?
    public ResourceData processedInto; // What it becomes after processing
    public int processingTime = 0; // Turns needed to process
    
    private void OnValidate()
    {
        itemType = ItemType.Resource;
    }
    
    /// <summary>
    /// Use/process the resource
    /// </summary>
    public override bool Use(GameObject user)
    {
        if (canBeProcessed && processedInto != null)
        {
            Debug.Log($"Processing {itemName} into {processedInto.itemName}...");
            // TODO: Implement processing logic
            return true;
        }
        
        Debug.Log($"{itemName} cannot be used directly.");
        return false;
    }
    
    /// <summary>
    /// Checks if the player has the right tool to harvest this resource
    /// </summary>
    public bool CanHarvestWith(ToolData tool)
    {
        if (tool == null) return false;
        
        // Check tool category matches
        if (tool.toolCategory != requiredTool)
            return false;
        
        // Check tool tier is sufficient
        if (tool.tier < minimumToolTier)
            return false;
        
        return true;
    }
    
    public override string GetDisplayInfo()
    {
        string info = base.GetDisplayInfo();
        info += $"\nCategory: {category}";
        info += $"\nRequired Tool: {requiredTool} ({minimumToolTier}+)";
        
        if (canBeProcessed && processedInto != null)
        {
            info += $"\nProcesses into: {processedInto.itemName}";
            info += $"\nProcessing Time: {processingTime} turns";
        }
        
        return info;
    }
}

/// <summary>
/// Categories of resources
/// </summary>
public enum ResourceCategory
{
    Wood,
    Stone,
    Ore,
    Fiber,
    Clay,
    Sand,
    Coal,
    Crystal
}
