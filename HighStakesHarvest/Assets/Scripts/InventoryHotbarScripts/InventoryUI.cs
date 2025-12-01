//Justin Gabon
//10/12/2025

/*
Simple inventory UI like Stardew Valley
Press Tab to open, shows all 36 slots
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
    
    [Header("Input")]
    [SerializeField] private KeyCode inventoryToggleKey = KeyCode.Tab;
    [SerializeField] private KeyCode inventoryCloseKey = KeyCode.Escape;
    
    private SimpleInventorySlot[] slotComponents;
    public bool isOpen = false;
    private int lastToggleFrame = -1; // prevents multiple toggles in the same frame
    private CanvasGroup panelCanvasGroup; // used when panel is on the same GameObject as this script
    
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

        // If the assigned panel is the same GameObject this script lives on,
        // use a CanvasGroup to hide/show instead of SetActive which would disable Update.
        if (inventoryPanel != null && inventoryPanel == gameObject)
        {
            panelCanvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = inventoryPanel.AddComponent<CanvasGroup>();
            }
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
        // Toggle inventory with Tab
        if (Input.GetKeyDown(inventoryToggleKey))
        {
            ToggleInventory();
        }
        
        // Close with Escape
        if (Input.GetKeyDown(inventoryCloseKey) && isOpen)
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
        if (lastToggleFrame == Time.frameCount)
            return; // another input already toggled this frame

        lastToggleFrame = Time.frameCount;

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
        if (inventoryPanel == null)
        {
            Debug.LogWarning("Inventory panel not assigned on InventoryUI.");
            return;
        }
        
        isOpen = true;
        SetPanelVisibility(true);
        RefreshDisplay();
        
        Debug.Log("Inventory opened (Tab/Escape to close)");
    }
    
    /// <summary>
    /// Closes inventory
    /// </summary>
    public void CloseInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogWarning("Inventory panel not assigned on InventoryUI.");
            return;
        }
        
        isOpen = false;
        SetPanelVisibility(false);
        
        Debug.Log("Inventory closed");
    }

    // Show/hide without disabling this component when the panel is the same GameObject
    private void SetPanelVisibility(bool visible)
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = visible ? 1f : 0f;
            panelCanvasGroup.blocksRaycasts = visible;
            panelCanvasGroup.interactable = visible;
        }
        else
        {
            inventoryPanel.SetActive(visible);
        }
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
