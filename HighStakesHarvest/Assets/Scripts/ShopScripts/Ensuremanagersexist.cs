using UnityEngine;

/// <summary>
/// Creates persistent managers if they don't exist yet.
/// Much simpler than prefabs - just creates GameObjects with the manager scripts!
/// 
/// Your existing managers in FarmScene have DontDestroyOnLoad and singleton pattern,
/// so this works perfectly:
/// - If you load FarmScene first: FarmScene creates them
/// - If you load CasinoScene first: This creates them
/// - Either way, only ONE instance exists (thanks to singleton pattern)
/// 
/// SETUP:
/// 1. Add this script to an empty GameObject in CasinoScene (and other casino scenes)
/// 2. Assign your ScriptableObject lists in the Inspector (for ItemDatabase)
/// 3. Done! Now you can test any scene directly.
/// </summary>
public class EnsureManagersExist : MonoBehaviour
{
    [Header("ItemDatabase Setup (required if managers don't exist)")]
    [Tooltip("Drag your seed ScriptableObjects here")]
    public SeedData[] seeds;

    [Tooltip("Drag your tool ScriptableObjects here")]
    public ToolData[] tools;

    [Tooltip("Drag your crop ScriptableObjects here")]
    public CropData[] crops;

    [Tooltip("Drag your resource ScriptableObjects here")]
    public ResourceData[] resources;

    [Header("Manager Settings")]
    [SerializeField] private int startingMoney = 100;
    [SerializeField] private int inventorySlots = 36;
    [SerializeField] private int hotbarSize = 10;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        // Check and create each manager if needed
        EnsureMoneyManager();
        EnsureItemDatabase();
        EnsurePlayerInventory();

        DebugLog("Manager initialization complete!");
    }

    private void EnsureMoneyManager()
    {
        if (MoneyManager.Instance != null)
        {
            DebugLog("MoneyManager already exists ✓");
            return;
        }

        DebugLog("Creating MoneyManager...");

        GameObject managerObj = new GameObject("MoneyManager");
        MoneyManager manager = managerObj.AddComponent<MoneyManager>();
        DontDestroyOnLoad(managerObj);

        // Use reflection to set the starting money (since it's serialized field)
        var field = typeof(MoneyManager).GetField("startingMoney",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(manager, startingMoney);
        }

        // Initialize it by calling SetMoney
        manager.SetMoney(startingMoney);

        DebugLog($"MoneyManager created with ${startingMoney}");
    }

    private void EnsureItemDatabase()
    {
        if (ItemDatabase.Instance != null)
        {
            DebugLog("ItemDatabase already exists ✓");
            return;
        }

        DebugLog("Creating ItemDatabase...");

        GameObject databaseObj = new GameObject("ItemDatabase");
        ItemDatabase database = databaseObj.AddComponent<ItemDatabase>();
        DontDestroyOnLoad(databaseObj);

        // Populate the database with items from this script's arrays
        if (seeds != null && seeds.Length > 0)
        {
            database.allSeeds.AddRange(seeds);
            DebugLog($"Added {seeds.Length} seeds to database");
        }

        if (tools != null && tools.Length > 0)
        {
            database.allTools.AddRange(tools);
            DebugLog($"Added {tools.Length} tools to database");
        }

        if (crops != null && crops.Length > 0)
        {
            database.allCrops.AddRange(crops);
            DebugLog($"Added {crops.Length} crops to database");
        }

        if (resources != null && resources.Length > 0)
        {
            database.allResources.AddRange(resources);
            DebugLog($"Added {resources.Length} resources to database");
        }

        // Force initialization (ItemDatabase does this in Awake, but we'll call it again)
        var initMethod = typeof(ItemDatabase).GetMethod("InitializeDatabase",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        if (initMethod != null)
        {
            initMethod.Invoke(database, null);
        }

        DebugLog("ItemDatabase created and populated");
    }

    private void EnsurePlayerInventory()
    {
        if (PlayerInventory.Instance != null)
        {
            DebugLog("PlayerInventory already exists ✓");
            return;
        }

        DebugLog("Creating PlayerInventory...");

        GameObject inventoryObj = new GameObject("PlayerInventory");
        PlayerInventory inventory = inventoryObj.AddComponent<PlayerInventory>();
        DontDestroyOnLoad(inventoryObj);

        // Set the inventory settings using reflection
        var totalSlotsField = typeof(PlayerInventory).GetField("totalSlots",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        if (totalSlotsField != null)
        {
            totalSlotsField.SetValue(inventory, inventorySlots);
        }

        var hotbarField = typeof(PlayerInventory).GetField("hotbarSize",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        if (hotbarField != null)
        {
            hotbarField.SetValue(inventory, hotbarSize);
        }

        DebugLog($"PlayerInventory created with {inventorySlots} slots");
    }

    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[EnsureManagers] {message}");
        }
    }

    /// <summary>
    /// For debugging: check which managers exist
    /// </summary>
    [ContextMenu("Check Manager Status")]
    public void CheckManagerStatus()
    {
        Debug.Log("=== Manager Status ===");
        Debug.Log($"MoneyManager: {(MoneyManager.Instance != null ? "EXISTS ✓" : "MISSING ✗")}");
        Debug.Log($"ItemDatabase: {(ItemDatabase.Instance != null ? "EXISTS ✓" : "MISSING ✗")}");
        Debug.Log($"PlayerInventory: {(PlayerInventory.Instance != null ? "EXISTS ✓" : "MISSING ✗")}");

        if (MoneyManager.Instance != null)
        {
            Debug.Log($"  Current Money: ${MoneyManager.Instance.GetMoney()}");
        }

        if (ItemDatabase.Instance != null)
        {
            int itemCount = ItemDatabase.Instance.allSeeds.Count +
                           ItemDatabase.Instance.allTools.Count +
                           ItemDatabase.Instance.allCrops.Count +
                           ItemDatabase.Instance.allResources.Count;
            Debug.Log($"  Total Items in Database: {itemCount}");
        }
    }
}