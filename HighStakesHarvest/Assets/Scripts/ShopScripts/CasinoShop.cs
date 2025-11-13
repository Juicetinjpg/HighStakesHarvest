using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Dynamic CasinoShop that auto-populates items from ItemDatabase
/// Now supports BOTH buying items AND selling crops!
/// Toggle between modes with the buyMode boolean
/// </summary>
public class CasinoShop : MonoBehaviour
{
    [Header("Shop Mode")]
    [SerializeField] private bool buyMode = true; // true = buy items, false = sell crops
    [SerializeField] private Button toggleModeButton; // Optional button to switch modes

    [Header("What to Sell (Buy Mode)")]
    [SerializeField] private bool sellSeeds = true;
    [SerializeField] private bool sellTools = true;
    [SerializeField] private bool sellCrops = false;
    [SerializeField] private bool sellResources = false;

    [Header("What Shop Buys (Sell Mode)")]
    [SerializeField] private bool shopBuysCrops = true;
    [SerializeField] private bool shopBuysResources = false;

    [Header("Item Slot Prefab")]
    [SerializeField] private GameObject itemSlotPrefab; // Your ItemSlot UI prefab
    [SerializeField] private Transform itemSlotContainer; // Parent (e.g., ScrollView Content)

    [Header("Prefab References (assign in prefab)")]
    [Tooltip("These should be set in the ItemSlot prefab itself")]
    public string iconImageName = "IconImage";
    public string nameTextName = "NameText";
    public string costTextName = "CostText";
    public string descriptionTextName = "DescriptionText";
    public string buyButtonName = "BuyButton";

    [Header("UI References")]
    [SerializeField] private Text moneyDisplayText;
    [SerializeField] private TextMeshProUGUI moneyDisplayTextTMP;
    [SerializeField] private GameObject insufficientFundsPanel;
    [SerializeField] private GameObject insufficientSpacePanel;
    [SerializeField] private GameObject purchaseSuccessPanel;
    [SerializeField] private Text purchaseSuccessText;
    [SerializeField] private TextMeshProUGUI purchaseSuccessTextTMP;
    [SerializeField] private float notificationDuration = 2f;

    [Header("Pricing")]
    [SerializeField] private bool useItemBasePrices = true;
    [SerializeField] private float priceMultiplier = 1.0f; // Adjust prices (1.0 = normal, 1.5 = 50% more expensive)

    private List<ShopItemRuntime> runtimeItems = new List<ShopItemRuntime>();

    private class ShopItemRuntime
    {
        public ItemData itemData;
        public int cost;
        public Button buyButton;
        public GameObject slotObject;
    }

    private void Start()
    {
        Debug.Log("=== CasinoShop Start ===");
        Debug.Log($"Buy Mode: {buyMode}");
        Debug.Log($"MoneyManager exists: {MoneyManager.Instance != null}");
        Debug.Log($"ItemDatabase exists: {ItemDatabase.Instance != null}");
        Debug.Log($"PlayerInventory exists: {PlayerInventory.Instance != null}");
        Debug.Log($"InventoryManager exists: {InventoryManager.Instance != null}");

        if (MoneyManager.Instance != null)
        {
            Debug.Log($"Current Money: ${MoneyManager.Instance.GetMoney()}");
        }

        if (ItemDatabase.Instance != null)
        {
            Debug.Log($"Seeds in database: {ItemDatabase.Instance.allSeeds.Count}");
            Debug.Log($"Tools in database: {ItemDatabase.Instance.allTools.Count}");
            Debug.Log($"Crops in database: {ItemDatabase.Instance.allCrops.Count}");
        }

        if (!ValidateManagers())
        {
            Debug.LogError("ValidateManagers FAILED!");
            return;
        }

        Debug.Log("Subscribing to MoneyChanged event...");
        MoneyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;

        // Subscribe to inventory changes for sell mode (static event)
        PlayerInventory.OnInventoryChanged += OnInventoryChanged;

        Debug.Log("Initial money display update...");
        UpdateMoneyDisplay(MoneyManager.Instance.GetMoney());

        Debug.Log("Populating shop...");
        PopulateShop();

        Debug.Log("Hiding panels...");
        HideAllPanels();

        // Setup toggle button if assigned
        if (toggleModeButton != null)
        {
            toggleModeButton.onClick.AddListener(ToggleMode);
        }

        Debug.Log("=== CasinoShop Start Complete ===");
    }

    private void OnDestroy()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
        }

        // Unsubscribe from static inventory event
        PlayerInventory.OnInventoryChanged -= OnInventoryChanged;

        // Clean up button listeners
        foreach (var item in runtimeItems)
        {
            if (item.buyButton != null)
            {
                item.buyButton.onClick.RemoveAllListeners();
            }
        }
    }

    private bool ValidateManagers()
    {
        bool valid = true;

        if (MoneyManager.Instance == null)
        {
            Debug.LogError("CasinoShop: MoneyManager not found!");
            valid = false;
        }

        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("CasinoShop: PlayerInventory not found!");
            valid = false;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("CasinoShop: InventoryManager not found!");
            valid = false;
        }

        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("CasinoShop: ItemDatabase not found!");
            valid = false;
        }

        if (itemSlotPrefab == null)
        {
            Debug.LogError("CasinoShop: ItemSlot prefab not assigned!");
            valid = false;
        }

        if (itemSlotContainer == null)
        {
            Debug.LogError("CasinoShop: Item slot container not assigned!");
            valid = false;
        }

        return valid;
    }

    /// <summary>
    /// Toggles between buy and sell mode
    /// </summary>
    public void ToggleMode()
    {
        buyMode = !buyMode;
        Debug.Log($"Shop mode switched to: {(buyMode ? "BUY" : "SELL")}");
        PopulateShop();
    }

    /// <summary>
    /// Sets the shop to buy mode
    /// </summary>
    public void SetBuyMode()
    {
        if (!buyMode)
        {
            buyMode = true;
            PopulateShop();
        }
    }

    /// <summary>
    /// Sets the shop to sell mode
    /// </summary>
    public void SetSellMode()
    {
        if (buyMode)
        {
            buyMode = false;
            PopulateShop();
        }
    }

    /// <summary>
    /// Main populate method - calls appropriate method based on mode
    /// </summary>
    private void PopulateShop()
    {
        if (buyMode)
        {
            PopulateShopFromDatabase();
        }
        else
        {
            PopulateSellMode();
        }
    }

    /// <summary>
    /// Called when inventory changes - refresh sell mode display
    /// </summary>
    private void OnInventoryChanged()
    {
        if (!buyMode)
        {
            PopulateSellMode();
        }
    }

    /// <summary>
    /// Populates shop with items FROM database (for player to BUY)
    /// </summary>
    private void PopulateShopFromDatabase()
    {
        Debug.Log("=== PopulateShopFromDatabase (BUY MODE) ===");
        ClearShop();

        List<ItemData> itemsToSell = new List<ItemData>();

        // Gather items based on what we want to sell
        if (sellSeeds)
        {
            Debug.Log($"Checking seeds... Database has {ItemDatabase.Instance.allSeeds.Count} seeds");
            var seeds = ItemDatabase.Instance.allSeeds.Where(s => s != null).ToList();
            Debug.Log($"Found {seeds.Count} non-null seeds");
            itemsToSell.AddRange(seeds);
        }

        if (sellTools)
        {
            Debug.Log($"Checking tools... Database has {ItemDatabase.Instance.allTools.Count} tools");
            var tools = ItemDatabase.Instance.allTools.Where(t => t != null).ToList();
            Debug.Log($"Found {tools.Count} non-null tools");
            itemsToSell.AddRange(tools);
        }

        if (sellCrops)
        {
            itemsToSell.AddRange(ItemDatabase.Instance.allCrops.Where(c => c != null));
        }

        if (sellResources)
        {
            itemsToSell.AddRange(ItemDatabase.Instance.allResources.Where(r => r != null));
        }

        Debug.Log($"CasinoShop: Total items to sell: {itemsToSell.Count}");

        if (itemsToSell.Count == 0)
        {
            Debug.LogWarning("NO ITEMS TO SELL! Check your ItemDatabase!");
        }

        // Create UI slot for each item (BUY MODE)
        foreach (var itemData in itemsToSell)
        {
            Debug.Log($"Creating BUY slot for: {itemData.itemName}");
            CreateBuySlot(itemData);
        }

        Debug.Log($"Created {runtimeItems.Count} shop items");
        UpdateButtonStates();
    }

    /// <summary>
    /// Populates shop with items FROM player inventory (for player to SELL)
    /// </summary>
    private void PopulateSellMode()
    {
        Debug.Log("=== PopulateSellMode (SELL MODE) ===");
        ClearShop();

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found for sell mode!");
            return;
        }

        List<ItemData> itemsToShow = new List<ItemData>();

        // Get items from player's inventory
        if (shopBuysCrops)
        {
            var crops = InventoryManager.Instance.GetCrops();
            foreach (var crop in crops)
            {
                int quantity = InventoryManager.Instance.GetItemQuantity(crop);
                if (quantity > 0 && crop.isTradeable)
                {
                    itemsToShow.Add(crop);
                }
            }
            Debug.Log($"Found {crops.Count} sellable crops in inventory");
        }

        if (shopBuysResources)
        {
            var resources = InventoryManager.Instance.GetItemsByType(ItemType.Resource);
            foreach (var resource in resources)
            {
                int quantity = InventoryManager.Instance.GetItemQuantity(resource);
                if (quantity > 0 && resource.isTradeable)
                {
                    itemsToShow.Add(resource);
                }
            }
        }

        Debug.Log($"Total items player can sell: {itemsToShow.Count}");

        if (itemsToShow.Count == 0)
        {
            Debug.Log("Player has no items to sell!");
            CreateNoItemsMessage("No crops to sell!\nGo farm some crops first!");
            return;
        }

        // Create UI slot for each item (SELL MODE)
        foreach (var itemData in itemsToShow)
        {
            int quantity = InventoryManager.Instance.GetItemQuantity(itemData);
            Debug.Log($"Creating SELL slot for: {itemData.itemName} (x{quantity})");
            CreateSellSlot(itemData, quantity);
        }

        Debug.Log($"Created {runtimeItems.Count} sell slots");
    }

    /// <summary>
    /// Creates a UI slot for buying an item
    /// </summary>
    private void CreateBuySlot(ItemData itemData)
    {
        // Instantiate the prefab
        GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotContainer);
        slotObj.name = $"BuySlot_{itemData.itemName}";

        // Calculate cost
        int cost = useItemBasePrices ? itemData.GetBuyPrice() : itemData.basePrice;
        cost = Mathf.CeilToInt(cost * priceMultiplier);

        // Find UI components
        Image iconImage = FindInChildren<Image>(slotObj, iconImageName);
        Text nameText = FindInChildren<Text>(slotObj, nameTextName);
        Text costText = FindInChildren<Text>(slotObj, costTextName);
        Text descText = FindInChildren<Text>(slotObj, descriptionTextName);
        TextMeshProUGUI nameTextTMP = FindInChildren<TextMeshProUGUI>(slotObj, nameTextName);
        TextMeshProUGUI costTextTMP = FindInChildren<TextMeshProUGUI>(slotObj, costTextName);
        TextMeshProUGUI descTextTMP = FindInChildren<TextMeshProUGUI>(slotObj, descriptionTextName);
        Button buyButton = FindInChildren<Button>(slotObj, buyButtonName);

        // Setup icon
        if (iconImage != null && itemData.icon != null)
        {
            iconImage.sprite = itemData.icon;
            iconImage.enabled = true;
        }

        // Setup text
        SetText(nameText, nameTextTMP, itemData.itemName);
        SetText(costText, costTextTMP, $"${cost}");
        SetText(descText, descTextTMP, itemData.description);

        // Setup button
        if (buyButton != null)
        {
            // Change button text to BUY
            Text buttonText = buyButton.GetComponentInChildren<Text>();
            TextMeshProUGUI buttonTextTMP = buyButton.GetComponentInChildren<TextMeshProUGUI>();
            SetText(buttonText, buttonTextTMP, "BUY");

            ItemData localItemData = itemData;
            int localCost = cost;
            buyButton.onClick.AddListener(() => PurchaseItem(localItemData, localCost));
        }

        // Store runtime data
        ShopItemRuntime runtimeItem = new ShopItemRuntime
        {
            itemData = itemData,
            cost = cost,
            buyButton = buyButton,
            slotObject = slotObj
        };
        runtimeItems.Add(runtimeItem);

        Debug.Log($"CasinoShop: Created BUY slot for {itemData.itemName} - ${cost}");
    }

    /// <summary>
    /// Creates a UI slot for selling an item
    /// </summary>
    private void CreateSellSlot(ItemData itemData, int quantity)
    {
        // Instantiate the prefab
        GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotContainer);
        slotObj.name = $"SellSlot_{itemData.itemName}";

        // Calculate sell price
        int sellPrice = itemData.GetSellPrice();
        int totalValue = sellPrice * quantity;

        // Find UI components
        Image iconImage = FindInChildren<Image>(slotObj, iconImageName);
        Text nameText = FindInChildren<Text>(slotObj, nameTextName);
        Text costText = FindInChildren<Text>(slotObj, costTextName);
        Text descText = FindInChildren<Text>(slotObj, descriptionTextName);
        TextMeshProUGUI nameTextTMP = FindInChildren<TextMeshProUGUI>(slotObj, nameTextName);
        TextMeshProUGUI costTextTMP = FindInChildren<TextMeshProUGUI>(slotObj, costTextName);
        TextMeshProUGUI descTextTMP = FindInChildren<TextMeshProUGUI>(slotObj, descriptionTextName);
        Button sellButton = FindInChildren<Button>(slotObj, buyButtonName);

        // Setup icon
        if (iconImage != null && itemData.icon != null)
        {
            iconImage.sprite = itemData.icon;
            iconImage.enabled = true;
        }

        // Setup text - show quantity
        SetText(nameText, nameTextTMP, $"{itemData.itemName} (x{quantity})");
        SetText(costText, costTextTMP, $"${sellPrice} each\n${totalValue} total");

        // Show seasonal bonus if applicable
        string desc = itemData.description;
        if (itemData is CropData cropData && cropData.hasSeasonalBonus)
        {
            if (TurnManager.Instance != null && TurnManager.Instance.GetCurrentSeason() == cropData.bonusSeason)
            {
                desc += "\n⭐ SEASONAL BONUS!";
            }
        }
        SetText(descText, descTextTMP, desc);

        // Setup button
        if (sellButton != null)
        {
            // Change button text to SELL
            Text buttonText = sellButton.GetComponentInChildren<Text>();
            TextMeshProUGUI buttonTextTMP = sellButton.GetComponentInChildren<TextMeshProUGUI>();
            SetText(buttonText, buttonTextTMP, "SELL");

            ItemData localItemData = itemData;
            int localQuantity = quantity;
            sellButton.onClick.AddListener(() => SellItemToShop(localItemData, localQuantity));
        }

        // Store runtime data
        ShopItemRuntime runtimeItem = new ShopItemRuntime
        {
            itemData = itemData,
            cost = totalValue,
            buyButton = sellButton,
            slotObject = slotObj
        };
        runtimeItems.Add(runtimeItem);

        Debug.Log($"CasinoShop: Created SELL slot for {itemData.itemName} x{quantity} - ${totalValue}");
    }

    /// <summary>
    /// Creates a message when no items available
    /// </summary>
    private void CreateNoItemsMessage(string message)
    {
        GameObject msgObj = new GameObject("NoItemsMessage");
        msgObj.transform.SetParent(itemSlotContainer);

        TextMeshProUGUI text = msgObj.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24;
        text.color = Color.white;

        RectTransform rt = msgObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 100);
    }

    /// <summary>
    /// Finds a component in children by GameObject name
    /// </summary>
    private T FindInChildren<T>(GameObject parent, string childName) where T : Component
    {
        Transform child = parent.transform.Find(childName);
        if (child != null)
        {
            return child.GetComponent<T>();
        }

        // Try recursive search if direct child not found
        T[] components = parent.GetComponentsInChildren<T>(true);
        foreach (T comp in components)
        {
            if (comp.gameObject.name == childName)
            {
                return comp;
            }
        }

        return null;
    }

    /// <summary>
    /// Handles purchasing an item (player buying FROM shop)
    /// </summary>
    private void PurchaseItem(ItemData itemData, int cost)
    {
        if (itemData == null)
        {
            Debug.LogError("CasinoShop: Cannot purchase - item data is null!");
            return;
        }

        if (!MoneyManager.Instance.HasEnoughMoney(cost))
        {
            ShowInsufficientFunds();
            return;
        }

        bool canAdd = PlayerInventory.Instance.HasEmptySlot() ||
                      PlayerInventory.Instance.HasItem(itemData.itemName);

        if (!canAdd)
        {
            ShowInsufficientSpace();
            return;
        }

        if (MoneyManager.Instance.RemoveMoney(cost))
        {
            string itemType = itemData.itemType.ToString();
            bool added = PlayerInventory.Instance.AddItem(itemData.itemName, 1, itemType);

            if (added)
            {
                Debug.Log($"CasinoShop: Purchased {itemData.itemName} for ${cost}");
                ShowPurchaseSuccess($"Purchased: {itemData.itemName}!");
            }
            else
            {
                MoneyManager.Instance.AddMoney(cost);
                ShowInsufficientSpace();
                Debug.LogWarning($"CasinoShop: Failed to add {itemData.itemName}, refunded ${cost}");
            }

            UpdateButtonStates();
        }
    }

    /// <summary>
    /// Handles selling an item (player selling TO shop)
    /// </summary>
    private void SellItemToShop(ItemData itemData, int quantity)
    {
        if (itemData == null)
        {
            Debug.LogError("CasinoShop: Cannot sell - item data is null!");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("CasinoShop: InventoryManager not found!");
            return;
        }

        // Use InventoryManager's SellItem which handles money
        bool success = InventoryManager.Instance.SellItem(itemData, quantity, out int totalValue);

        if (success)
        {
            Debug.Log($"✅ Sold {quantity}x {itemData.itemName} for ${totalValue}!");
            ShowPurchaseSuccess($"Sold {quantity}x {itemData.itemName} for ${totalValue}!");

            // Refresh sell mode display
            PopulateSellMode();
        }
        else
        {
            Debug.LogWarning($"Failed to sell {itemData.itemName}");
        }
    }

    private void UpdateMoneyDisplay(int currentMoney)
    {
        string moneyText = $"Money:${currentMoney}";

        if (moneyDisplayText != null)
        {
            moneyDisplayText.text = moneyText;
        }

        if (moneyDisplayTextTMP != null)
        {
            moneyDisplayTextTMP.text = moneyText;
        }

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (MoneyManager.Instance == null) return;

        int currentMoney = MoneyManager.Instance.GetMoney();

        // Only disable buttons in buy mode if not enough money
        if (buyMode)
        {
            foreach (var item in runtimeItems)
            {
                if (item.buyButton != null)
                {
                    item.buyButton.interactable = currentMoney >= item.cost;
                }
            }
        }
        else
        {
            // In sell mode, all buttons should be enabled
            foreach (var item in runtimeItems)
            {
                if (item.buyButton != null)
                {
                    item.buyButton.interactable = true;
                }
            }
        }
    }

    private void ShowInsufficientFunds()
    {
        if (insufficientFundsPanel != null)
        {
            insufficientFundsPanel.SetActive(true);
            CancelInvoke(nameof(HideInsufficientFunds));
            Invoke(nameof(HideInsufficientFunds), notificationDuration);
        }
    }

    private void HideInsufficientFunds()
    {
        if (insufficientFundsPanel != null)
            insufficientFundsPanel.SetActive(false);
    }

    private void ShowInsufficientSpace()
    {
        if (insufficientSpacePanel != null)
        {
            insufficientSpacePanel.SetActive(true);
            CancelInvoke(nameof(HideInsufficientSpace));
            Invoke(nameof(HideInsufficientSpace), notificationDuration);
        }
    }

    private void HideInsufficientSpace()
    {
        if (insufficientSpacePanel != null)
            insufficientSpacePanel.SetActive(false);
    }

    private void ShowPurchaseSuccess(string message)
    {
        if (purchaseSuccessPanel != null)
        {
            purchaseSuccessPanel.SetActive(true);
            SetText(purchaseSuccessText, purchaseSuccessTextTMP, message);
            CancelInvoke(nameof(HidePurchaseSuccess));
            Invoke(nameof(HidePurchaseSuccess), notificationDuration);
        }
    }

    private void HidePurchaseSuccess()
    {
        if (purchaseSuccessPanel != null)
            purchaseSuccessPanel.SetActive(false);
    }

    private void HideAllPanels()
    {
        if (insufficientFundsPanel != null)
            insufficientFundsPanel.SetActive(false);
        if (insufficientSpacePanel != null)
            insufficientSpacePanel.SetActive(false);
        if (purchaseSuccessPanel != null)
            purchaseSuccessPanel.SetActive(false);
    }

    private void SetText(Text legacyText, TextMeshProUGUI tmpText, string value)
    {
        if (legacyText != null)
            legacyText.text = value;

        if (tmpText != null)
            tmpText.text = value;
    }

    /// <summary>
    /// Clears all spawned items
    /// </summary>
    private void ClearShop()
    {
        foreach (var item in runtimeItems)
        {
            if (item.slotObject != null)
            {
                Destroy(item.slotObject);
            }
        }
        runtimeItems.Clear();
    }

    /// <summary>
    /// Manually refresh the shop
    /// </summary>
    [ContextMenu("Refresh Shop")]
    public void RefreshShop()
    {
        PopulateShop();
    }
}