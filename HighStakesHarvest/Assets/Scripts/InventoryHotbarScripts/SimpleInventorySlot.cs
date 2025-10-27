//Justin Gabon
//10/12/2025

/*
Individual inventory slot component
Handles display and click interactions
Supports drag-and-drop to reorganize
Updated to use TextMeshPro
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Added for TextMeshPro

public class SimpleInventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Components")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText; // Changed from Text to TextMeshProUGUI
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    [SerializeField] private Color hotbarColor = new Color(0.3f, 0.25f, 0.2f, 0.9f);
    [SerializeField] private Color emptyColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    
    private int slotIndex;
    private GameObject dragIcon;
    private Canvas canvas;
    
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        
        // Auto-find components if not assigned
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
        if (iconImage == null) iconImage = transform.Find("Icon")?.GetComponent<Image>();
        if (quantityText == null) quantityText = transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
    }
    
    /// <summary>
    /// Initializes the slot with its index
    /// </summary>
    public void Initialize(int index)
    {
        slotIndex = index;
        
        // Color hotbar slots differently
        if (backgroundImage != null && PlayerInventory.Instance != null)
        {
            if (slotIndex < PlayerInventory.Instance.HotbarSize)
            {
                backgroundImage.color = hotbarColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }
    }
    
    /// <summary>
    /// Updates the slot display
    /// </summary>
    public void UpdateDisplay(InventorySlot slotData)
    {
        if (slotData == null || slotData.IsEmpty)
        {
            // Empty slot
            if (iconImage != null)
            {
                iconImage.enabled = false;
            }
            if (quantityText != null)
            {
                quantityText.text = "";
            }
            
            if (backgroundImage != null)
            {
                backgroundImage.color = emptyColor;
            }
        }
        else
        {
            // Has item
            if (iconImage != null)
            {
                iconImage.enabled = true;
                
                // Load icon sprite if available
                if (SimpleItemIcons.Instance != null)
                {
                    Sprite icon = SimpleItemIcons.Instance.GetIcon(slotData.itemName);
                    if (icon != null)
                    {
                        iconImage.sprite = icon;
                    }
                }
            }
            
            if (quantityText != null)
            {
                quantityText.text = slotData.quantity > 1 ? slotData.quantity.ToString() : "";
            }
            
            // Restore normal color
            if (backgroundImage != null)
            {
                if (slotIndex < PlayerInventory.Instance.HotbarSize)
                {
                    backgroundImage.color = hotbarColor;
                }
                else
                {
                    backgroundImage.color = normalColor;
                }
            }
        }
    }
    
    /// <summary>
    /// Called when slot is clicked
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (PlayerInventory.Instance == null) return;
        
        InventorySlot slotData = PlayerInventory.Instance.GetSlot(slotIndex);
        
        if (slotData != null && !slotData.IsEmpty)
        {
            Debug.Log($"Clicked slot {slotIndex}: {slotData.itemName} x{slotData.quantity}");
            
            // Right-click to drop/use
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                DropItem();
            }
        }
    }
    
    /// <summary>
    /// Drops one item from this slot
    /// </summary>
    private void DropItem()
    {
        if (PlayerInventory.Instance == null) return;
        
        InventorySlot slotData = PlayerInventory.Instance.GetSlot(slotIndex);
        if (slotData != null && !slotData.IsEmpty)
        {
            // Remove one from inventory
            PlayerInventory.Instance.RemoveItem(slotData.itemName, 1);
            Debug.Log($"Dropped {slotData.itemName}");
            
            // TODO: Spawn item in world at player position
        }
    }
    
    /// <summary>
    /// Begin dragging
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (PlayerInventory.Instance == null) return;
        
        InventorySlot slotData = PlayerInventory.Instance.GetSlot(slotIndex);
        if (slotData == null || slotData.IsEmpty) return;
        
        // Create drag icon
        if (iconImage != null && canvas != null)
        {
            dragIcon = new GameObject("DragIcon");
            dragIcon.transform.SetParent(canvas.transform);
            dragIcon.transform.SetAsLastSibling();
            
            Image dragImage = dragIcon.AddComponent<Image>();
            dragImage.sprite = iconImage.sprite;
            dragImage.raycastTarget = false;
            dragImage.color = new Color(1, 1, 1, 0.6f);
            
            RectTransform rectTransform = dragIcon.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(50, 50);
        }
        
        // Make original semi-transparent
        if (backgroundImage != null)
        {
            Color color = backgroundImage.color;
            color.a = 0.5f;
            backgroundImage.color = color;
        }
        if (iconImage != null)
        {
            Color color = iconImage.color;
            color.a = 0.5f;
            iconImage.color = color;
        }
    }
    
    /// <summary>
    /// While dragging
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }
    }
    
    /// <summary>
    /// End dragging
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        // Destroy drag icon
        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }
        
        // Restore original alpha
        if (backgroundImage != null)
        {
            Color color = backgroundImage.color;
            color.a = 0.9f;
            backgroundImage.color = color;
        }
        if (iconImage != null)
        {
            Color color = iconImage.color;
            color.a = 1f;
            iconImage.color = color;
        }
        
        // Check if dropped on another slot
        if (eventData.pointerEnter != null)
        {
            SimpleInventorySlot targetSlot = eventData.pointerEnter.GetComponent<SimpleInventorySlot>();
            if (targetSlot != null && targetSlot != this)
            {
                // Swap slots
                SwapSlots(targetSlot.slotIndex);
            }
        }
    }
    
    /// <summary>
    /// Swaps this slot with another
    /// </summary>
    private void SwapSlots(int targetIndex)
    {
        if (PlayerInventory.Instance == null) return;
        
        PlayerInventory.Instance.SwapSlots(slotIndex, targetIndex);
        Debug.Log($"Swapped slot {slotIndex} with slot {targetIndex}");
    }
    
    public int GetSlotIndex()
    {
        return slotIndex;
    }
}