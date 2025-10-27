//Justin Gabon
//10/12/2025

/*
Visual display for hotbar - shows first 10 slots of PlayerInventory
Updated to use TextMeshPro
*/

using UnityEngine;
using UnityEngine.UI;
using TMPro; // Added for TextMeshPro(previosly tested using legacy text)

public class HotbarUI : MonoBehaviour
{
    [Header("Hotbar Slots")]
    [SerializeField] private HotbarSlotDisplay[] slotDisplays = new HotbarSlotDisplay[10];
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    [SerializeField] private Color selectedColor = new Color(1f, 0.9f, 0.5f, 1f);
    [SerializeField] private float selectedScale = 1.1f;
    
    private int currentSelectedSlot = 0;
    
    [System.Serializable]
    public class HotbarSlotDisplay
    {
        public GameObject slotObject;
        public Image backgroundImage;
        public Image itemIcon;
        
        // TextMeshPro components
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI slotNumberText;
        public TextMeshProUGUI quantityText;
    }
    
    private void OnEnable()
    {
        HotbarSystem.OnSlotSelected += OnSlotSelected;
        PlayerInventory.OnSlotChanged += OnInventorySlotChanged;
        PlayerInventory.OnInventoryChanged += RefreshAllSlots;
    }
    
    private void OnDisable()
    {
        HotbarSystem.OnSlotSelected -= OnSlotSelected;
        PlayerInventory.OnSlotChanged -= OnInventorySlotChanged;
        PlayerInventory.OnInventoryChanged -= RefreshAllSlots;
    }
    
    private void Start()
    {
        InitializeSlots();
        RefreshAllSlots();
    }
    
    private void InitializeSlots()
    {
        for (int i = 0; i < slotDisplays.Length; i++)
        {
            // Display 1-9, 0 from left to right
            string numberText = (i == 9) ? "0" : (i + 1).ToString();
            
            if (slotDisplays[i].slotNumberText != null)
            {
                slotDisplays[i].slotNumberText.text = numberText;
            }
            
            // Set initial colors
            if (slotDisplays[i].backgroundImage != null)
            {
                slotDisplays[i].backgroundImage.color = normalColor;
            }
        }
        
        UpdateSelectedSlotVisuals(0);
    }
    
    private void OnSlotSelected(int slotIndex)
    {
        UpdateSelectedSlotVisuals(slotIndex);
    }
    
    private void UpdateSelectedSlotVisuals(int newSlot)
    {
        // Reset previous slot
        if (slotDisplays[currentSelectedSlot].backgroundImage != null)
        {
            slotDisplays[currentSelectedSlot].backgroundImage.color = normalColor;
        }
        if (slotDisplays[currentSelectedSlot].slotObject != null)
        {
            slotDisplays[currentSelectedSlot].slotObject.transform.localScale = Vector3.one;
        }
        
        // Highlight new slot
        currentSelectedSlot = newSlot;
        
        if (slotDisplays[currentSelectedSlot].backgroundImage != null)
        {
            slotDisplays[currentSelectedSlot].backgroundImage.color = selectedColor;
        }
        if (slotDisplays[currentSelectedSlot].slotObject != null)
        {
            slotDisplays[currentSelectedSlot].slotObject.transform.localScale = Vector3.one * selectedScale;
        }
    }
    
    private void OnInventorySlotChanged(int slotIndex, InventorySlot slotData)
    {
        if (slotIndex < 10)
        {
            UpdateSlotDisplay(slotIndex, slotData);
        }
    }
    
    private void UpdateSlotDisplay(int slotIndex, InventorySlot slotData)
    {
        if (slotIndex < 0 || slotIndex >= slotDisplays.Length) return;
        
        HotbarSlotDisplay display = slotDisplays[slotIndex];
        
        if (slotData == null || slotData.IsEmpty)
        {
            // Empty slot
            if (display.itemIcon != null)
            {
                display.itemIcon.enabled = false;
            }
            if (display.itemNameText != null)
            {
                display.itemNameText.text = "";
            }
            if (display.quantityText != null)
            {
                display.quantityText.text = "";
            }
        }
        else
        {
            // Has item
            if (display.itemIcon != null)
            {
                display.itemIcon.enabled = true;
                
                // Load icon sprite if available
                if (SimpleItemIcons.Instance != null)
                {
                    Sprite icon = SimpleItemIcons.Instance.GetIcon(slotData.itemName);
                    if (icon != null)
                    {
                        display.itemIcon.sprite = icon;
                    }
                }
            }
            
            if (display.itemNameText != null)
            {
                display.itemNameText.text = slotData.itemName;
            }
            
            if (display.quantityText != null)
            {
                display.quantityText.text = slotData.quantity > 1 ? slotData.quantity.ToString() : "";
            }
        }
    }
    
    public void RefreshAllSlots()
    {
        if (PlayerInventory.Instance == null) return;
        
        InventorySlot[] hotbarSlots = PlayerInventory.Instance.GetHotbarSlots();
        
        for (int i = 0; i < hotbarSlots.Length && i < slotDisplays.Length; i++)
        {
            UpdateSlotDisplay(i, hotbarSlots[i]);
        }
    }
}