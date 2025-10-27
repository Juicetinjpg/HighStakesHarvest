//Justin Gabon
//10/12/2025

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public string itemName;
    public int quantity;
    public string itemType;
    
    public InventorySlot(string name = null, int qty = 0, string type = null)
    {
        itemName = name;
        quantity = qty;
        itemType = type;
    }
    
    public bool IsEmpty => string.IsNullOrEmpty(itemName) || quantity <= 0;
    
    public void Clear()
    {
        itemName = null;
        quantity = 0;
        itemType = null;
    }
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Inventory Settings")]
    [SerializeField] private int totalSlots = 36; // Like Stardew: 12 wide x 3 rows
    [SerializeField] private int hotbarSize = 10; // First row (keys 1-9, 0)
    [SerializeField] private int maxStackSize = 999;

    private InventorySlot[] inventorySlots;
    
    // Events
    public static event Action<int, InventorySlot> OnSlotChanged; // slot index, slot data
    public static event Action OnInventoryChanged;

    // Properties
    public int TotalSlots => totalSlots;
    public int HotbarSize => hotbarSize;
    
    void Awake()
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
        
        InitializeInventory();
    }
    
    private void InitializeInventory()
    {
        inventorySlots = new InventorySlot[totalSlots];
        for (int i = 0; i < totalSlots; i++)
        {
            inventorySlots[i] = new InventorySlot();
        }
    }

    /// <summary>
    /// Adds an item to inventory (finds first available slot or stacks)
    /// </summary>
    public bool AddItem(string itemName, int quantity, string itemType)
    {
        if (string.IsNullOrEmpty(itemName) || quantity <= 0)
        {
            Debug.LogWarning("Invalid item to add");
            return false;
        }
        
        int remainingQuantity = quantity;
        
        // First, try to stack with existing items
        for (int i = 0; i < totalSlots && remainingQuantity > 0; i++)
        {
            InventorySlot slot = inventorySlots[i];
            
            if (!slot.IsEmpty && slot.itemName == itemName)
            {
                int spaceInSlot = maxStackSize - slot.quantity;
                int amountToAdd = Mathf.Min(spaceInSlot, remainingQuantity);
                
                if (amountToAdd > 0)
                {
                    slot.quantity += amountToAdd;
                    remainingQuantity -= amountToAdd;
                    OnSlotChanged?.Invoke(i, slot);
                }
            }
        }
        
        // Then, add to empty slots
        for (int i = 0; i < totalSlots && remainingQuantity > 0; i++)
        {
            InventorySlot slot = inventorySlots[i];
            
            if (slot.IsEmpty)
            {
                int amountToAdd = Mathf.Min(maxStackSize, remainingQuantity);
                slot.itemName = itemName;
                slot.itemType = itemType;
                slot.quantity = amountToAdd;
                remainingQuantity -= amountToAdd;
                OnSlotChanged?.Invoke(i, slot);
            }
        }
        
        if (remainingQuantity > 0)
        {
            Debug.LogWarning($"Inventory full! Could not add {remainingQuantity} {itemName}");
        }
        
        OnInventoryChanged?.Invoke();
        return remainingQuantity == 0;
    }

    /// <summary>
    /// Removes specified quantity of an item from inventory
    /// </summary>
    public bool RemoveItem(string itemName, int quantity)
    {
        int remainingToRemove = quantity;
        
        for (int i = 0; i < totalSlots && remainingToRemove > 0; i++)
        {
            InventorySlot slot = inventorySlots[i];
            
            if (!slot.IsEmpty && slot.itemName == itemName)
            {
                int amountToRemove = Mathf.Min(slot.quantity, remainingToRemove);
                slot.quantity -= amountToRemove;
                remainingToRemove -= amountToRemove;
                
                if (slot.quantity <= 0)
                {
                    slot.Clear();
                }
                
                OnSlotChanged?.Invoke(i, slot);
            }
        }
        
        OnInventoryChanged?.Invoke();
        return remainingToRemove == 0;
    }
    
    /// <summary>
    /// Checks if inventory has item with sufficient quantity
    /// </summary>
    public bool HasItem(string itemName, int quantity = 1)
    {
        return GetItemQuantity(itemName) >= quantity;
    }
    
    /// <summary>
    /// Gets total quantity of an item across all slots
    /// </summary>
    public int GetItemQuantity(string itemName)
    {
        int total = 0;
        for (int i = 0; i < totalSlots; i++)
        {
            if (!inventorySlots[i].IsEmpty && inventorySlots[i].itemName == itemName)
            {
                total += inventorySlots[i].quantity;
            }
        }
        return total;
    }
    
    /// <summary>
    /// Gets a specific inventory slot
    /// </summary>
    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= totalSlots) return null;
        return inventorySlots[index];
    }
    
    /// <summary>
    /// Gets all hotbar slots (first 10 slots)
    /// </summary>
    public InventorySlot[] GetHotbarSlots()
    {
        InventorySlot[] hotbar = new InventorySlot[hotbarSize];
        for (int i = 0; i < hotbarSize; i++)
        {
            hotbar[i] = inventorySlots[i];
        }
        return hotbar;
    }
    
    /// <summary>
    /// Sets a specific slot's contents
    /// </summary>
    public void SetSlot(int index, string itemName, int quantity, string itemType)
    {
        if (index < 0 || index >= totalSlots) return;
        
        inventorySlots[index].itemName = itemName;
        inventorySlots[index].quantity = quantity;
        inventorySlots[index].itemType = itemType;
        
        OnSlotChanged?.Invoke(index, inventorySlots[index]);
        OnInventoryChanged?.Invoke();
    }
    
    /// <summary>
    /// Swaps two inventory slots (for dragging/reorganizing)
    /// </summary>
    public void SwapSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= totalSlots || indexB < 0 || indexB >= totalSlots) return;
        
        InventorySlot temp = new InventorySlot(
            inventorySlots[indexA].itemName,
            inventorySlots[indexA].quantity,
            inventorySlots[indexA].itemType
        );
        
        inventorySlots[indexA].itemName = inventorySlots[indexB].itemName;
        inventorySlots[indexA].quantity = inventorySlots[indexB].quantity;
        inventorySlots[indexA].itemType = inventorySlots[indexB].itemType;
        
        inventorySlots[indexB].itemName = temp.itemName;
        inventorySlots[indexB].quantity = temp.quantity;
        inventorySlots[indexB].itemType = temp.itemType;
        
        OnSlotChanged?.Invoke(indexA, inventorySlots[indexA]);
        OnSlotChanged?.Invoke(indexB, inventorySlots[indexB]);
        OnInventoryChanged?.Invoke();
    }
    
    /// <summary>
    /// Clears a specific slot
    /// </summary>
    public void ClearSlot(int index)
    {
        if (index < 0 || index >= totalSlots) return;
        
        inventorySlots[index].Clear();
        OnSlotChanged?.Invoke(index, inventorySlots[index]);
        OnInventoryChanged?.Invoke();
    }
    
    /// <summary>
    /// Gets all items by type
    /// </summary>
    public List<InventorySlot> GetItemsByType(string itemType)
    {
        List<InventorySlot> items = new List<InventorySlot>();
        
        for (int i = 0; i < totalSlots; i++)
        {
            if (!inventorySlots[i].IsEmpty && inventorySlots[i].itemType == itemType)
            {
                items.Add(inventorySlots[i]);
            }
        }
        
        return items;
    }
    
    /// <summary>
    /// Clears entire inventory
    /// </summary>
    public void ClearInventory()
    {
        for (int i = 0; i < totalSlots; i++)
        {
            inventorySlots[i].Clear();
        }
        OnInventoryChanged?.Invoke();
        Debug.Log("Inventory cleared!");
    }
    
    /// <summary>
    /// Checks if inventory has any empty slots
    /// </summary>
    public bool HasEmptySlot()
    {
        for (int i = 0; i < totalSlots; i++)
        {
            if (inventorySlots[i].IsEmpty) return true;
        }
        return false;
    }
    
    /// <summary>
    /// Gets first empty slot index, or -1 if full
    /// </summary>
    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < totalSlots; i++)
        {
            if (inventorySlots[i].IsEmpty) return i;
        }
        return -1;
    }
    
    /// <summary>
    /// Debug: Print all inventory contents
    /// </summary>
    public void PrintInventory()
    {
        Debug.Log("=== INVENTORY ===");
        Debug.Log("HOTBAR (Slots 1-10):");
        for (int i = 0; i < hotbarSize; i++)
        {
            if (!inventorySlots[i].IsEmpty)
            {
                Debug.Log($"  [{i + 1}] {inventorySlots[i].itemName} x{inventorySlots[i].quantity}");
            }
        }
        
        Debug.Log("REST OF INVENTORY:");
        for (int i = hotbarSize; i < totalSlots; i++)
        {
            if (!inventorySlots[i].IsEmpty)
            {
                Debug.Log($"  [{i}] {inventorySlots[i].itemName} x{inventorySlots[i].quantity}");
            }
        }
    }
    
    // SAVE/LOAD
    [Serializable]
    public class InventoryData
    {
        public InventorySlot[] slots;
    }
    
    public string SaveInventoryData()
    {
        InventoryData data = new InventoryData { slots = inventorySlots };
        return JsonUtility.ToJson(data);
    }
    
    public void LoadInventoryData(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData)) return;
        
        InventoryData data = JsonUtility.FromJson<InventoryData>(jsonData);
        inventorySlots = data.slots ?? new InventorySlot[totalSlots];
        
        OnInventoryChanged?.Invoke();
        Debug.Log("Inventory loaded!");
    }
}