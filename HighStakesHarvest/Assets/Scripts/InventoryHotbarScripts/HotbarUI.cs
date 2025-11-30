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
                StyleText(slotDisplays[i].slotNumberText);
            }
            
            // Set initial colors
            if (slotDisplays[i].backgroundImage != null)
            {
                slotDisplays[i].backgroundImage.color = normalColor;
            }

            StyleText(slotDisplays[i].itemNameText);
            StyleText(slotDisplays[i].quantityText);
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
                Sprite icon = null;

                // Prefer ItemData icons (Seeds use seedSprite, etc.)
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

                display.itemIcon.enabled = icon != null;
                if (icon != null)
                {
                    display.itemIcon.sprite = icon;
                    FitIconToSlot(display.itemIcon, icon, display.backgroundImage);
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

    /// <summary>
    /// Scales the icon to fit inside the slot while preserving aspect ratio.
    /// </summary>
    private void FitIconToSlot(Image image, Sprite sprite, Image background)
    {
        if (image == null || sprite == null) return;

        image.preserveAspect = true;
        RectTransform rt = image.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        image.SetNativeSize();
        rt.anchoredPosition = Vector2.zero;

        RectTransform parentRect = background != null
            ? background.rectTransform
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
}
