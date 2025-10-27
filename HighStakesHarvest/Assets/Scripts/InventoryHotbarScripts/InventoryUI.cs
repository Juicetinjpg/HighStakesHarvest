//Justin Gabon
//10/12/2025

/*
Simple inventory UI like Stardew Valley
Press Tab/E to open, shows all 36 slots
First row (slots 0-9) = hotbar (always visible at bottom)
Can drag items to reorganize
*/

using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }
    
    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryPanel;
    
    [Header("Slot Grid")]
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;
    
    private SimpleInventorySlot[] slotComponents;
    public bool isOpen = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        CreateInventorySlots();
        CloseInventory();
        
        // Subscribe to inventory changes
        PlayerInventory.OnInventoryChanged += RefreshDisplay;
        PlayerInventory.OnSlotChanged += OnSlotChanged;
    }
    
    private void OnDestroy()
    {
        PlayerInventory.OnInventoryChanged -= RefreshDisplay;
        PlayerInventory.OnSlotChanged -= OnSlotChanged;
    }
    
    private void Update()
    {
        // Toggle inventory with Tab or E
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
        
        // Close with Escape
        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            CloseInventory();
        }
    }
    
    /// <summary>
    /// Creates all inventory slot UI elements
    /// </summary>
    private void CreateInventorySlots()
    {
        if (PlayerInventory.Instance == null || slotsContainer == null) return;
        
        int totalSlots = PlayerInventory.Instance.TotalSlots;
        slotComponents = new SimpleInventorySlot[totalSlots];
        
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            slotObj.name = $"InventorySlot_{i}";
            
            SimpleInventorySlot slotComponent = slotObj.GetComponent<SimpleInventorySlot>();
            if (slotComponent != null)
            {
                slotComponent.Initialize(i);
                slotComponents[i] = slotComponent;
            }
            
            // Visual separator after first row (hotbar)
            if (i == PlayerInventory.Instance.HotbarSize - 1)
            {
                // You can add a visual divider here if you want
            }
        }
        
        RefreshDisplay();
    }
    
    /// <summary>
    /// Toggles inventory open/closed
    /// </summary>
    public void ToggleInventory()
    {
        if (isOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }
    
    /// <summary>
    /// Opens inventory
    /// </summary>
    public void OpenInventory()
    {
        if (inventoryPanel == null) return;
        
        isOpen = true;
        inventoryPanel.SetActive(true);
        RefreshDisplay();
        
        Debug.Log("Inventory opened (Tab/E to close)");
    }
    
    /// <summary>
    /// Closes inventory
    /// </summary>
    public void CloseInventory()
    {
        if (inventoryPanel == null) return;
        
        isOpen = false;
        inventoryPanel.SetActive(false);
        
        Debug.Log("Inventory closed");
    }
    
    /// <summary>
    /// Refreshes entire inventory display
    /// </summary>
    private void RefreshDisplay()
    {
        if (PlayerInventory.Instance == null || slotComponents == null) return;
        
        for (int i = 0; i < slotComponents.Length; i++)
        {
            if (slotComponents[i] != null)
            {
                InventorySlot slotData = PlayerInventory.Instance.GetSlot(i);
                slotComponents[i].UpdateDisplay(slotData);
            }
        }
    }
    
    /// <summary>
    /// Called when a specific slot changes
    /// </summary>
    private void OnSlotChanged(int slotIndex, InventorySlot slotData)
    {
        if (slotIndex >= 0 && slotIndex < slotComponents.Length && slotComponents[slotIndex] != null)
        {
            slotComponents[slotIndex].UpdateDisplay(slotData);
        }
    }
}