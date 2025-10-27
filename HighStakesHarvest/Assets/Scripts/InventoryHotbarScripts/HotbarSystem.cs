//Justin Gabon
//10/12/2025

/*
Hotbar system that directly uses first 10 slots of PlayerInventory
Like Stardew Valley: Hotbar IS the first row of your inventory
Keys 1-9, 0 select slots, mouse scroll to cycle
*/

using UnityEngine;
using System;

public class HotbarSystem : MonoBehaviour
{
    public static HotbarSystem Instance { get; private set; }
    
    private int currentSlot = 0; // Currently selected slot (0-9)
    private int hotbarSize = 10;
    
    // Events
    public static event Action<int> OnSlotSelected; // slot index
    
    // Properties
    public int CurrentSlot => currentSlot;
    public InventorySlot CurrentSlotData => PlayerInventory.Instance?.GetSlot(currentSlot);
    public string CurrentItem => CurrentSlotData?.itemName;
    public bool HasItemEquipped => CurrentSlotData != null && !CurrentSlotData.IsEmpty;
    
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
    }
    
    private void Update()
    {
        HandleHotbarInput();
    }
    
    /// <summary>
    /// Handles number key input for hotbar selection (1-9, 0)
    /// </summary>
    private void HandleHotbarInput()
    {
        // Number keys 1-9, 0
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectSlot(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SelectSlot(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) SelectSlot(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) SelectSlot(8);
        if (Input.GetKeyDown(KeyCode.Alpha0)) SelectSlot(9);
        
        // Mouse scroll wheel to cycle through slots
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SelectSlot((currentSlot - 1 + hotbarSize) % hotbarSize); // Scroll up
        }
        else if (scroll < 0f)
        {
            SelectSlot((currentSlot + 1) % hotbarSize); // Scroll down
        }
    }
    
    /// <summary>
    /// Selects a specific hotbar slot
    /// </summary>
    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotbarSize) return;
        
        currentSlot = slotIndex;
        OnSlotSelected?.Invoke(currentSlot);
        
        string itemName = HasItemEquipped ? CurrentItem : "Empty";
        Debug.Log($"Selected Slot {currentSlot + 1}: {itemName}");
    }
    
    /// <summary>
    /// Uses one of the currently selected item
    /// </summary>
    public bool UseCurrentItem()
    {
        if (!HasItemEquipped) return false;
        
        InventorySlot slot = CurrentSlotData;
        
        // Remove one from inventory
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.RemoveItem(slot.itemName, 1);
            Debug.Log($"Used {slot.itemName}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Gets the currently equipped item type
    /// </summary>
    public string GetCurrentItemType()
    {
        return CurrentSlotData?.itemType;
    }
    
    /// <summary>
    /// Gets quantity of currently equipped item
    /// </summary>
    public int GetCurrentItemQuantity()
    {
        return CurrentSlotData?.quantity ?? 0;
    }
}