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
    // Brighter defaults so slots don't look disabled/greyed out
    [SerializeField] private Color normalColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color hotbarColor = new Color(0.65f, 0.6f, 0.5f, 1f);
    [SerializeField] private Color emptyColor = new Color(0.35f, 0.35f, 0.35f, 0.9f);
    
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

        StyleText(quantityText);
        UpgradeLegacyColorsIfNeeded();
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
                Sprite icon = null;

                // Prefer ItemData icons (SeedData uses seedSprite, etc.)
                if (ItemDatabase.Instance != null)
                {
                    ItemData itemData = ItemDatabase.Instance.GetItem(slotData.itemName);
                    if (itemData != null)
                    {
                        icon = itemData.GetIcon();
                    }
                }

                // Fallback to legacy SimpleItemIcons mapping
                if (icon == null && SimpleItemIcons.Instance != null)
                {
                    icon = SimpleItemIcons.Instance.GetIcon(slotData.itemName);
                }

                iconImage.enabled = icon != null;
                if (icon != null)
                {
                    iconImage.sprite = icon;
                    iconImage.color = Color.white; // ensure no dark tint on icons
                    FitIconToSlot(iconImage, icon);
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

    /// <summary>
    /// Scales the icon to fit inside the slot while preserving aspect ratio.
    /// </summary>
    private void FitIconToSlot(Image image, Sprite sprite)
    {
        if (image == null || sprite == null) return;

        image.preserveAspect = true;
        RectTransform rt = image.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        image.SetNativeSize();
        rt.anchoredPosition = Vector2.zero;

        RectTransform parentRect = backgroundImage != null
            ? backgroundImage.rectTransform
            : image.rectTransform.parent as RectTransform;

        if (parentRect == null) return;

        float parentSize = Mathf.Min(parentRect.rect.width, parentRect.rect.height);
        if (parentSize <= 0f)
        {
            // Fallback to the current image size if layout hasn't resolved yet
            parentSize = Mathf.Min(image.rectTransform.rect.width, image.rectTransform.rect.height);
        }
        if (parentSize <= 0f)
        {
            // Reasonable default if both are zero
            parentSize = 64f;
        }

        float maxSize = parentSize * 0.85f;
        float spriteWidth = sprite.rect.width;
        float spriteHeight = sprite.rect.height;

        if (spriteWidth <= 0 || spriteHeight <= 0) return;

        float scale = Mathf.Min(maxSize / spriteWidth, maxSize / spriteHeight);
        image.rectTransform.sizeDelta = new Vector2(spriteWidth * scale, spriteHeight * scale);
    }

    /// <summary>
    /// Improve text legibility with bright color and subtle outline.
    /// </summary>
    private void StyleText(TextMeshProUGUI text)
    {
        if (text == null) return;

        text.color = new Color(1f, 1f, 1f, 0.95f);
        Outline outline = text.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(0f, 0f, 0f, 0.6f);
        outline.effectDistance = new Vector2(1f, -1f);
    }

    /// <summary>
    /// If the serialized colors are still the old dark defaults, upgrade them to the brighter palette.
    /// This ensures existing prefab/scene instances get the new look without manual inspector edits.
    /// </summary>
    private void UpgradeLegacyColorsIfNeeded()
    {
        Color oldNormal = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        Color oldHotbar = new Color(0.3f, 0.25f, 0.2f, 0.9f);
        Color oldEmpty = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        if (Approximately(backgroundImage != null ? backgroundImage.color : normalColor, oldNormal))
        {
            normalColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        }
        if (Approximately(hotbarColor, oldHotbar))
        {
            hotbarColor = new Color(0.65f, 0.6f, 0.5f, 1f);
        }
        if (Approximately(emptyColor, oldEmpty))
        {
            emptyColor = new Color(0.35f, 0.35f, 0.35f, 0.9f);
        }
    }

    private bool Approximately(Color a, Color b, float tolerance = 0.02f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               Mathf.Abs(a.a - b.a) < tolerance;
    }
}
