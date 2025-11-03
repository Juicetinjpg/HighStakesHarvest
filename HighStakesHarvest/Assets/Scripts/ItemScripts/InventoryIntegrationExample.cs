using UnityEngine;

/// <summary>
/// Example script demonstrating how to use the integrated inventory system
/// Shows both old (string-based) and new (ItemData-based) methods
/// Attach to a test GameObject to try out the integration
/// </summary>
public class InventoryIntegrationExample : MonoBehaviour
{
    [Header("Test Items - Assign from ItemDatabase")]
    public SeedData testSeed;
    public CropData testCrop;
    public ToolData testTool;
    
    [Header("Test Settings")]
    public string currentSeason = "Spring";
    
    private void Update()
    {
        // Press keys to test different functionality
        
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TestAddItemsOldWay();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TestAddItemsNewWay();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestEquippedItemInfo();
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            TestPlanting();
        }
        
        if (Input.GetKeyDown(KeyCode.F5))
        {
            TestToolUsage();
        }
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            TestSelling();
        }
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            TestItemQueries();
        }
        
        // Display help
        if (Input.GetKeyDown(KeyCode.H))
        {
            ShowHelp();
        }
    }
    
    /// <summary>
    /// Test 1: Add items using old string-based method
    /// This still works perfectly - backward compatible!
    /// </summary>
    private void TestAddItemsOldWay()
    {
        Debug.Log("=== TEST 1: Adding Items (Old Way) ===");
        
        // String-based adding (backward compatible)
        PlayerInventory.Instance.AddItem("Tomato Seeds", 10, "Seed");
        PlayerInventory.Instance.AddItem("Carrot Seeds", 5, "Seed");
        PlayerInventory.Instance.AddItem("Basic Hoe", 1, "Tool");
        
        Debug.Log("✓ Added items using old string-based method");
        Debug.Log("These items work with both old and new systems!");
    }
    
    /// <summary>
    /// Test 2: Add items using new ItemData method
    /// This is the recommended way for new code
    /// </summary>
    private void TestAddItemsNewWay()
    {
        Debug.Log("=== TEST 2: Adding Items (New Way) ===");
        
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found! Add it to PlayerInventory GameObject");
            return;
        }
        
        // ItemData-based adding (new, recommended)
        if (testSeed != null)
        {
            InventoryManager.Instance.AddItem(testSeed, 15);
            Debug.Log($"✓ Added {testSeed.itemName} x15");
        }
        
        if (testCrop != null)
        {
            InventoryManager.Instance.AddItem(testCrop, 8);
            Debug.Log($"✓ Added {testCrop.itemName} x8");
        }
        
        if (testTool != null)
        {
            InventoryManager.Instance.AddItem(testTool, 1);
            Debug.Log($"✓ Added {testTool.itemName}");
        }
        
        Debug.Log("These items have full ItemData functionality!");
    }
    
    /// <summary>
    /// Test 3: Check what item is currently equipped
    /// </summary>
    private void TestEquippedItemInfo()
    {
        Debug.Log("=== TEST 3: Equipped Item Info ===");
        
        if (HotbarManager.Instance == null)
        {
            Debug.LogError("HotbarManager not found! Add it to HotbarSystem GameObject");
            return;
        }
        
        ItemData equippedItem = HotbarManager.Instance.GetEquippedItemData();
        
        if (equippedItem == null)
        {
            Debug.Log("No item equipped in current hotbar slot");
            return;
        }
        
        Debug.Log($"Equipped: {equippedItem.itemName}");
        Debug.Log($"Type: {equippedItem.itemType}");
        Debug.Log($"Sell Price: ${equippedItem.GetSellPrice()}");
        Debug.Log("\nFull Info:");
        Debug.Log(equippedItem.GetDisplayInfo());
        
        // Check specific types
        if (HotbarManager.Instance.IsSeedEquipped())
        {
            SeedData seed = HotbarManager.Instance.GetEquippedSeed();
            Debug.Log($"✓ This is a seed! Growth time: {seed.growthTime} turns");
        }
        else if (HotbarManager.Instance.IsToolEquipped())
        {
            ToolData tool = HotbarManager.Instance.GetEquippedTool();
            Debug.Log($"✓ This is a tool! Category: {tool.toolCategory}, Durability: {tool.currentDurability}/{tool.durability}");
        }
    }
    
    /// <summary>
    /// Test 4: Plant a seed from hotbar
    /// </summary>
    private void TestPlanting()
    {
        Debug.Log("=== TEST 4: Planting Seeds ===");
        
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found!");
            return;
        }
        
        if (!HotbarManager.Instance.IsSeedEquipped())
        {
            Debug.Log("No seed equipped! Equip a seed in hotbar and try again.");
            return;
        }
        
        SeedData equippedSeed = HotbarManager.Instance.GetEquippedSeed();
        Debug.Log($"Attempting to plant: {equippedSeed.itemName}");
        
        // Check season compatibility
        if (!equippedSeed.CanPlantInSeason(currentSeason))
        {
            Debug.LogWarning($"Cannot plant {equippedSeed.itemName} in {currentSeason}!");
            Debug.Log($"This seed prefers: {equippedSeed.seasonPreference}");
            return;
        }
        
        // Plant at player position (or slight offset)
        Vector3 plantPosition = transform.position + Vector3.forward * 2f;
        
        bool planted = InventoryManager.Instance.PlantEquippedSeed(plantPosition, currentSeason);
        
        if (planted)
        {
            Debug.Log($"✓ Successfully planted {equippedSeed.itemName} at {plantPosition}");
            Debug.Log($"It will take {equippedSeed.growthTime} turns to grow");
            Debug.Log($"Will produce: {equippedSeed.producedCrop.itemName}");
        }
    }
    
    /// <summary>
    /// Test 5: Use equipped tool
    /// </summary>
    private void TestToolUsage()
    {
        Debug.Log("=== TEST 5: Using Tools ===");
        
        if (!HotbarManager.Instance.IsToolEquipped())
        {
            Debug.Log("No tool equipped! Equip a tool in hotbar and try again.");
            return;
        }
        
        ToolData equippedTool = HotbarManager.Instance.GetEquippedTool();
        Debug.Log($"Using: {equippedTool.itemName}");
        Debug.Log($"Category: {equippedTool.toolCategory}");
        Debug.Log($"Durability before: {equippedTool.currentDurability}/{equippedTool.durability}");
        
        // Use the tool
        bool success = HotbarManager.Instance.UseEquippedItem(gameObject);
        
        if (success)
        {
            Debug.Log($"✓ Tool used successfully!");
            Debug.Log($"Durability after: {equippedTool.currentDurability}/{equippedTool.durability}");
            
            if (equippedTool.currentDurability <= 0)
            {
                Debug.LogWarning("Tool is broken! Needs repair.");
            }
        }
    }
    
    /// <summary>
    /// Test 6: Sell crops
    /// </summary>
    private void TestSelling()
    {
        Debug.Log("=== TEST 6: Selling Items ===");
        
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found!");
            return;
        }
        
        // Get all crops in inventory
        var crops = InventoryManager.Instance.GetCrops();
        
        if (crops.Count == 0)
        {
            Debug.Log("No crops to sell! Add some crops first (F2)");
            return;
        }
        
        Debug.Log($"Found {crops.Count} different crop types");
        
        foreach (var crop in crops)
        {
            int quantity = InventoryManager.Instance.GetItemQuantity(crop);
            int sellPrice = crop.GetSellPrice();
            int totalValue = sellPrice * quantity;
            
            Debug.Log($"- {crop.itemName} x{quantity} @ ${sellPrice} each = ${totalValue} total");
        }
        
        // Sell first crop type
        if (crops.Count > 0)
        {
            CropData cropToSell = crops[0];
            int quantityToSell = Mathf.Min(5, InventoryManager.Instance.GetItemQuantity(cropToSell));
            
            if (InventoryManager.Instance.SellItem(cropToSell, quantityToSell, out int totalValue))
            {
                Debug.Log($"✓ Sold {quantityToSell}x {cropToSell.itemName} for ${totalValue}!");
            }
        }
    }
    
    /// <summary>
    /// Test 7: Query inventory for specific item types
    /// </summary>
    private void TestItemQueries()
    {
        Debug.Log("=== TEST 7: Inventory Queries ===");
        
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found!");
            return;
        }
        
        // Get all seeds
        var seeds = InventoryManager.Instance.GetSeeds();
        Debug.Log($"Seeds in inventory: {seeds.Count}");
        foreach (var seed in seeds)
        {
            int qty = InventoryManager.Instance.GetItemQuantity(seed);
            Debug.Log($"  - {seed.itemName} x{qty}");
        }
        
        // Get all crops
        var crops = InventoryManager.Instance.GetCrops();
        Debug.Log($"\nCrops in inventory: {crops.Count}");
        foreach (var crop in crops)
        {
            int qty = InventoryManager.Instance.GetItemQuantity(crop);
            Debug.Log($"  - {crop.itemName} x{qty}");
        }
        
        // Get all tools
        var tools = InventoryManager.Instance.GetTools();
        Debug.Log($"\nTools in inventory: {tools.Count}");
        foreach (var tool in tools)
        {
            Debug.Log($"  - {tool.itemName} (Durability: {tool.currentDurability}/{tool.durability})");
        }
    }
    
    /// <summary>
    /// Show help text
    /// </summary>
    private void ShowHelp()
    {
        Debug.Log("=== INVENTORY INTEGRATION TEST CONTROLS ===");
        Debug.Log("F1 - Add items (old string-based way)");
        Debug.Log("F2 - Add items (new ItemData way)");
        Debug.Log("F3 - Show equipped item info");
        Debug.Log("F4 - Plant equipped seed");
        Debug.Log("F5 - Use equipped tool");
        Debug.Log("F6 - Sell crops");
        Debug.Log("F7 - Query inventory by type");
        Debug.Log("H  - Show this help");
        Debug.Log("\nUse number keys 1-9,0 to select hotbar slots");
        Debug.Log("Use Tab/E to open full inventory");
    }
    
    private void Start()
    {
        // Auto-show help on start
        ShowHelp();
        
        // Validate setup
        Debug.Log("\n=== VALIDATING SETUP ===");
        
        if (PlayerInventory.Instance == null)
            Debug.LogError("❌ PlayerInventory not found!");
        else
            Debug.Log("✓ PlayerInventory found");
        
        if (InventoryManager.Instance == null)
            Debug.LogError("❌ InventoryManager not found! Add to PlayerInventory GameObject");
        else
            Debug.Log("✓ InventoryManager found");
        
        if (HotbarSystem.Instance == null)
            Debug.LogError("❌ HotbarSystem not found!");
        else
            Debug.Log("✓ HotbarSystem found");
        
        if (HotbarManager.Instance == null)
            Debug.LogError("❌ HotbarManager not found! Add to HotbarSystem GameObject");
        else
            Debug.Log("✓ HotbarManager found");
        
        if (ItemDatabase.Instance == null)
            Debug.LogError("❌ ItemDatabase not found! Create GameObject with ItemDatabase component");
        else
            Debug.Log("✓ ItemDatabase found");
        
        Debug.Log("\nReady to test! Press H for controls");
    }
}
