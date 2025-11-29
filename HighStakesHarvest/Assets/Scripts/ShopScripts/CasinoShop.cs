using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// CasinoShop - Standalone scene with two separate panels: Buy (from ItemDatabase) and Sell (from player inventory)
/// </summary>
public class CasinoShop : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private string returnSceneName = "MainGame"; // Scene to return to when closing shop
    [SerializeField] private Button closeShopButton; // Button to exit shop scene
    
    [Header("Panel Management")]
    [SerializeField] private Button buyTabButton;
    [SerializeField] private Button sellTabButton;
    [SerializeField] private GameObject buyPanelRoot;
    [SerializeField] private GameObject sellPanelRoot;
    
    [Header("What to Sell (Buy Panel)")]
    [SerializeField] private bool sellSeeds = true;
    [SerializeField] private bool sellTools = true;
    [SerializeField] private bool sellCrops = false;
    [SerializeField] private bool sellResources = false;

    [Header("What Shop Buys (Sell Panel)")]
    [SerializeField] private bool shopBuysCrops = true;
    [SerializeField] private bool shopBuysResources = false;

    [Header("Item Slot Prefab")]
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("UI Containers")]
    [SerializeField] private Transform buySlotContainer;
    [SerializeField] private Transform sellSlotContainer;

    [Header("Prefab References")]
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
    [SerializeField] private float priceMultiplier = 1.0f;

    private readonly List<ShopItemRuntime> buyRuntimeItems = new();
    private readonly List<ShopItemRuntime> sellRuntimeItems = new();
    
    private enum ShopPanel { Buy, Sell }
    private ShopPanel currentPanel = ShopPanel.Buy;

    private class ShopItemRuntime
    {
        public ItemData itemData;
        public int cost;
        public Button buyButton;
        public GameObject slotObject;
    }

    private void Start()
    {
        if (!ValidateManagers()) return;

        // Subscribe to events
        MoneyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
        PlayerInventory.OnInventoryChanged += OnInventoryChanged;

        // Setup UI
        UpdateMoneyDisplay(MoneyManager.Instance.GetMoney());
        HideAllPanels();
        
        // Setup panel switching buttons
        SetupPanelButtons();
        
        // Setup close button
        if (closeShopButton != null)
            closeShopButton.onClick.AddListener(CloseShop);

        // Populate both panels
        PopulateBuyPanel();
        PopulateSellPanel();
        
        // Show buy panel by default
        ShowPanel(ShopPanel.Buy);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;

        PlayerInventory.OnInventoryChanged -= OnInventoryChanged;

        // Cleanup button listeners
        CleanupButtons(buyRuntimeItems);
        CleanupButtons(sellRuntimeItems);
        
        if (closeShopButton != null)
            closeShopButton.onClick.RemoveAllListeners();
        if (buyTabButton != null)
            buyTabButton.onClick.RemoveAllListeners();
        if (sellTabButton != null)
            sellTabButton.onClick.RemoveAllListeners();
    }

    #region Panel Management
    
    private void SetupPanelButtons()
    {
        if (buyTabButton != null)
        {
            buyTabButton.onClick.AddListener(() => ShowPanel(ShopPanel.Buy));
        }
        
        if (sellTabButton != null)
        {
            sellTabButton.onClick.AddListener(() => ShowPanel(ShopPanel.Sell));
        }
    }
    
    private void ShowPanel(ShopPanel panel)
    {
        currentPanel = panel;
        
        if (buyPanelRoot != null)
            buyPanelRoot.SetActive(panel == ShopPanel.Buy);
        
        if (sellPanelRoot != null)
            sellPanelRoot.SetActive(panel == ShopPanel.Sell);
        
        // Update tab button visuals (optional - add highlighting)
        UpdateTabButtonVisuals();
        
        // Refresh the active panel
        if (panel == ShopPanel.Sell)
            PopulateSellPanel();
    }
    
    private void UpdateTabButtonVisuals()
    {
        // Optional: Update button colors/sprites to show active tab
        if (buyTabButton != null)
        {
            ColorBlock colors = buyTabButton.colors;
            colors.normalColor = currentPanel == ShopPanel.Buy ? new Color(0.8f, 0.8f, 0.8f) : Color.white;
            buyTabButton.colors = colors;
        }
        
        if (sellTabButton != null)
        {
            ColorBlock colors = sellTabButton.colors;
            colors.normalColor = currentPanel == ShopPanel.Sell ? new Color(0.8f, 0.8f, 0.8f) : Color.white;
            sellTabButton.colors = colors;
        }
    }
    
    private void CloseShop()
    {
        Debug.Log($"Closing shop, returning to {returnSceneName}");
        
        // Optional: Save any shop state before leaving
        
        SceneManager.LoadScene(returnSceneName);
    }
    
    #endregion

    #region Validation
    
    private bool ValidateManagers()
    {
        bool valid = true;
        if (MoneyManager.Instance == null) { Debug.LogError("CasinoShop: MoneyManager not found!"); valid = false; }
        if (PlayerInventory.Instance == null) { Debug.LogError("CasinoShop: PlayerInventory not found!"); valid = false; }
        if (InventoryManager.Instance == null) { Debug.LogError("CasinoShop: InventoryManager not found!"); valid = false; }
        if (ItemDatabase.Instance == null) { Debug.LogError("CasinoShop: ItemDatabase not found!"); valid = false; }
        if (itemSlotPrefab == null) { Debug.LogError("CasinoShop: ItemSlot prefab not assigned!"); valid = false; }
        if (buySlotContainer == null) { Debug.LogError("CasinoShop: Buy slot container not assigned!"); valid = false; }
        if (sellSlotContainer == null) { Debug.LogError("CasinoShop: Sell slot container not assigned!"); valid = false; }
        if (buyPanelRoot == null) { Debug.LogWarning("CasinoShop: Buy panel root not assigned - panel switching may not work!"); }
        if (sellPanelRoot == null) { Debug.LogWarning("CasinoShop: Sell panel root not assigned - panel switching may not work!"); }
        return valid;
    }
    
    #endregion

    #region Buy Panel (Shop Sells to Player)
    
    private void PopulateBuyPanel()
    {
        ClearPanel(buyRuntimeItems);
        List<ItemData> itemsToSell = new();

        if (sellSeeds)
            itemsToSell.AddRange(ItemDatabase.Instance.allSeeds.Where(s => s != null));
        if (sellTools)
            itemsToSell.AddRange(ItemDatabase.Instance.allTools.Where(t => t != null));
        if (sellCrops)
            itemsToSell.AddRange(ItemDatabase.Instance.allCrops.Where(c => c != null));
        if (sellResources)
            itemsToSell.AddRange(ItemDatabase.Instance.allResources.Where(r => r != null));

        if (itemsToSell.Count == 0)
        {
            CreateNoItemsMessage("No items available for purchase!", buySlotContainer);
        }
        else
        {
            foreach (var itemData in itemsToSell)
                CreateBuySlot(itemData);
        }

        UpdateButtonStates();
    }

    private void CreateBuySlot(ItemData itemData)
    {
        GameObject slotObj = Instantiate(itemSlotPrefab, buySlotContainer);
        slotObj.name = $"BuySlot_{itemData.itemName}";

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

        // Set icon
        if (iconImage != null && itemData.icon != null)
        {
            iconImage.sprite = itemData.icon;
            iconImage.enabled = true;
        }

        // Set text
        SetText(nameText, nameTextTMP, itemData.itemName);
        SetText(costText, costTextTMP, $"${cost}");
        SetText(descText, descTextTMP, itemData.description);

        // Setup buy button
        if (buyButton != null)
        {
            SetText(buyButton.GetComponentInChildren<Text>(), buyButton.GetComponentInChildren<TextMeshProUGUI>(), "BUY");
            ItemData localItemData = itemData;
            int localCost = cost;
            buyButton.onClick.AddListener(() => PurchaseItem(localItemData, localCost));
        }

        buyRuntimeItems.Add(new ShopItemRuntime { itemData = itemData, cost = cost, buyButton = buyButton, slotObject = slotObj });
    }

    private void PurchaseItem(ItemData itemData, int cost)
    {
        if (itemData == null) { Debug.LogError("CasinoShop: Cannot purchase - item data is null!"); return; }
        if (!MoneyManager.Instance.HasEnoughMoney(cost)) { ShowInsufficientFunds(); return; }

        bool canAdd = PlayerInventory.Instance.HasEmptySlot() || PlayerInventory.Instance.HasItem(itemData.itemName);
        if (!canAdd) { ShowInsufficientSpace(); return; }

        if (MoneyManager.Instance.RemoveMoney(cost))
        {
            string itemType = itemData.itemType.ToString();
            bool added = PlayerInventory.Instance.AddItem(itemData.itemName, 1, itemType);
            if (added)
            {
                ShowPurchaseSuccess($"Purchased: {itemData.itemName}!");
            }
            else
            {
                MoneyManager.Instance.AddMoney(cost);
                ShowInsufficientSpace();
            }
            UpdateButtonStates();
        }
    }
    
    #endregion

    #region Sell Panel (Shop Buys from Player)
    
    private void PopulateSellPanel()
    {
        ClearPanel(sellRuntimeItems);

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found for sell panel!");
            return;
        }

        List<ItemData> itemsToShow = new();

        if (shopBuysCrops)
        {
            var crops = InventoryManager.Instance.GetCrops();
            foreach (var crop in crops)
            {
                int quantity = InventoryManager.Instance.GetItemQuantity(crop);
                if (quantity > 0 && crop.isTradeable)
                    itemsToShow.Add(crop);
            }
        }

        if (shopBuysResources)
        {
            var resources = InventoryManager.Instance.GetItemsByType(ItemType.Resource);
            foreach (var resource in resources)
            {
                int quantity = InventoryManager.Instance.GetItemQuantity(resource);
                if (quantity > 0 && resource.isTradeable)
                    itemsToShow.Add(resource);
            }
        }

        if (itemsToShow.Count == 0)
        {
            string message = shopBuysCrops ? "No crops to sell!\nGo farm some crops first!" : "No items to sell!";
            CreateNoItemsMessage(message, sellSlotContainer);
        }
        else
        {
            foreach (var itemData in itemsToShow)
            {
                int quantity = InventoryManager.Instance.GetItemQuantity(itemData);
                CreateSellSlot(itemData, quantity);
            }
        }
    }

    private void CreateSellSlot(ItemData itemData, int quantity)
    {
        GameObject slotObj = Instantiate(itemSlotPrefab, sellSlotContainer);
        slotObj.name = $"SellSlot_{itemData.itemName}";

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

        // Set icon
        if (iconImage != null && itemData.icon != null)
        {
            iconImage.sprite = itemData.icon;
            iconImage.enabled = true;
        }

        // Set text
        SetText(nameText, nameTextTMP, $"{itemData.itemName} (x{quantity})");
        SetText(costText, costTextTMP, $"${sellPrice} each\n${totalValue} total");

        // Set description with seasonal bonus
        string desc = itemData.description;
        if (itemData is CropData cropData && cropData.hasSeasonalBonus)
        {
            if (TurnManager.Instance != null && TurnManager.Instance.GetCurrentSeason() == cropData.bonusSeason)
                desc += "\n★ SEASONAL BONUS!";
        }
        SetText(descText, descTextTMP, desc);

        // Setup sell button
        if (sellButton != null)
        {
            SetText(sellButton.GetComponentInChildren<Text>(), sellButton.GetComponentInChildren<TextMeshProUGUI>(), "SELL");
            ItemData localItemData = itemData;
            int localQuantity = quantity;
            sellButton.onClick.AddListener(() => SellItemToShop(localItemData, localQuantity));
        }

        sellRuntimeItems.Add(new ShopItemRuntime { itemData = itemData, cost = totalValue, buyButton = sellButton, slotObject = slotObj });
    }

    private void SellItemToShop(ItemData itemData, int quantity)
    {
        if (itemData == null) { Debug.LogError("CasinoShop: Cannot sell - item data is null!"); return; }
        if (InventoryManager.Instance == null) { Debug.LogError("CasinoShop: InventoryManager not found!"); return; }

        bool success = InventoryManager.Instance.SellItem(itemData, quantity, out int totalValue);
        if (success)
        {
            ShowPurchaseSuccess($"Sold {quantity}x {itemData.itemName} for ${totalValue}!");
            PopulateSellPanel();
        }
    }

    private void OnInventoryChanged()
    {
        // Only refresh sell panel if it's currently active
        if (currentPanel == ShopPanel.Sell)
        {
            PopulateSellPanel();
        }
    }
    
    #endregion

    #region UI Updates
    
    private void UpdateMoneyDisplay(int currentMoney)
    {
        string moneyText = $"Money: ${currentMoney}";
        if (moneyDisplayText != null) moneyDisplayText.text = moneyText;
        if (moneyDisplayTextTMP != null) moneyDisplayTextTMP.text = moneyText;
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (MoneyManager.Instance == null) return;
        int currentMoney = MoneyManager.Instance.GetMoney();
        
        // Update buy button states
        foreach (var item in buyRuntimeItems)
        {
            if (item.buyButton != null)
                item.buyButton.interactable = currentMoney >= item.cost;
        }
        
        // Sell buttons are always interactable
        foreach (var item in sellRuntimeItems)
        {
            if (item.buyButton != null)
                item.buyButton.interactable = true;
        }
    }
    
    #endregion

    #region Notifications
    
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
        if (insufficientFundsPanel != null) insufficientFundsPanel.SetActive(false);
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
        if (insufficientSpacePanel != null) insufficientSpacePanel.SetActive(false);
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
        if (purchaseSuccessPanel != null) purchaseSuccessPanel.SetActive(false);
    }

    private void HideAllPanels()
    {
        if (insufficientFundsPanel != null) insufficientFundsPanel.SetActive(false);
        if (insufficientSpacePanel != null) insufficientSpacePanel.SetActive(false);
        if (purchaseSuccessPanel != null) purchaseSuccessPanel.SetActive(false);
    }
    
    #endregion

    #region Helper Methods
    
    private void CreateNoItemsMessage(string message, Transform container)
    {
        GameObject msgObj = new GameObject("NoItemsMessage");
        msgObj.transform.SetParent(container, false);
        
        TextMeshProUGUI text = msgObj.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24;
        text.color = Color.white;
        
        RectTransform rt = msgObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.sizeDelta = Vector2.zero;
    }

    private T FindInChildren<T>(GameObject parent, string childName) where T : Component
    {
        Transform child = parent.transform.Find(childName);
        if (child != null) return child.GetComponent<T>();
        
        T[] components = parent.GetComponentsInChildren<T>(true);
        foreach (T comp in components)
        {
            if (comp.gameObject.name == childName)
                return comp;
        }
        return null;
    }

    private void SetText(Text legacyText, TextMeshProUGUI tmpText, string value)
    {
        if (legacyText != null) legacyText.text = value;
        if (tmpText != null) tmpText.text = value;
    }

    private void ClearPanel(List<ShopItemRuntime> list)
    {
        foreach (var item in list)
        {
            if (item.slotObject != null)
                Destroy(item.slotObject);
        }
        list.Clear();
    }

    private void CleanupButtons(List<ShopItemRuntime> list)
    {
        foreach (var item in list)
        {
            if (item.buyButton != null)
                item.buyButton.onClick.RemoveAllListeners();
        }
    }
    
    #endregion

    #region Public Methods
    
    [ContextMenu("Refresh Shop")]
    public void RefreshShop()
    {
        PopulateBuyPanel();
        PopulateSellPanel();
    }
    
    public void SwitchToBuyPanel()
    {
        ShowPanel(ShopPanel.Buy);
    }
    
    public void SwitchToSellPanel()
    {
        ShowPanel(ShopPanel.Sell);
    }
    
    #endregion
}