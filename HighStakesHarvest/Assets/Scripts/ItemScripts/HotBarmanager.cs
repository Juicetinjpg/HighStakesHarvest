using UnityEngine;

/// <summary>
/// Extension methods and helpers for HotbarSystem to work with ItemData
/// Add this component to the same GameObject as HotbarSystem
/// </summary>
public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance { get; private set; }

    private HotbarSystem baseHotbar;

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

        baseHotbar = GetComponent<HotbarSystem>();
        if (baseHotbar == null)
        {
            baseHotbar = gameObject.AddComponent<HotbarSystem>();
        }
    }

    /// <summary>
    /// Gets the currently equipped ItemData
    /// </summary>
    public ItemData GetEquippedItemData()
    {
        if (baseHotbar == null || InventoryManager.Instance == null) return null;

        return InventoryManager.Instance.GetItemDataFromSlot(baseHotbar.CurrentSlot);
    }

    /// <summary>
    /// Gets the currently equipped item type
    /// </summary>
    public ItemType? GetEquippedItemType()
    {
        ItemData item = GetEquippedItemData();
        return item?.itemType;
    }

    /// <summary>
    /// Checks if the equipped item is a specific type
    /// </summary>
    public bool IsEquippedItemType(ItemType type)
    {
        ItemData item = GetEquippedItemData();
        return item != null && item.itemType == type;
    }

    /// <summary>
    /// Checks if a tool is equipped
    /// </summary>
    public bool IsToolEquipped()
    {
        return IsEquippedItemType(ItemType.Tool);
    }

    /// <summary>
    /// Checks if a seed is equipped
    /// </summary>
    public bool IsSeedEquipped()
    {
        return IsEquippedItemType(ItemType.Seed);
    }

    /// <summary>
    /// Gets the equipped tool (null if not a tool)
    /// </summary>
    public ToolData GetEquippedTool()
    {
        return GetEquippedItemData() as ToolData;
    }

    /// <summary>
    /// Gets the equipped seed (null if not a seed)
    /// </summary>
    public SeedData GetEquippedSeed()
    {
        return GetEquippedItemData() as SeedData;
    }

    /// <summary>
    /// Gets the equipped crop (null if not a crop)
    /// </summary>
    public CropData GetEquippedCrop()
    {
        return GetEquippedItemData() as CropData;
    }

    /// <summary>
    /// Uses the currently equipped item
    /// </summary>
    public bool UseEquippedItem(GameObject user)
    {
        ItemData item = GetEquippedItemData();

        if (item == null)
        {
            Debug.Log("No item equipped!");
            return false;
        }

        if (InventoryManager.Instance != null)
        {
            return InventoryManager.Instance.UseItem(item, user);
        }

        return false;
    }

    /// <summary>
    /// Gets display info for the currently equipped item
    /// </summary>
    public string GetEquippedItemInfo()
    {
        ItemData item = GetEquippedItemData();

        if (item == null)
        {
            return "Empty Slot";
        }

        return item.GetDisplayInfo();
    }

    /// <summary>
    /// Checks if the equipped tool matches a specific category
    /// </summary>
    public bool IsToolCategoryEquipped(ToolCategory category)
    {
        ToolData tool = GetEquippedTool();
        return tool != null && tool.toolCategory == category;
    }

    /// <summary>
    /// Gets display text for hotbar slot showing item name and quantity
    /// </summary>
    public string GetSlotDisplayText(int slotIndex)
    {
        if (PlayerInventory.Instance == null) return "";

        InventorySlot slot = PlayerInventory.Instance.GetSlot(slotIndex);

        if (slot == null || slot.IsEmpty) return "";

        string text = slot.itemName;
        if (slot.quantity > 1)
        {
            text += $" x{slot.quantity}";
        }

        return text;
    }
}