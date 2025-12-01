using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
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
    [SerializeField] private ScrollRect buyScrollRect;
    [SerializeField] private ScrollRect sellScrollRect;

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

    [Header("Styling")]
    [SerializeField] private TMP_FontAsset pixelFont;
    [SerializeField] private int noItemsFontSize = 36;
    [SerializeField] private int contentTopPadding = 24;
    [SerializeField] private int emptyStateTopPadding = 80;
    private Coroutine buyScrollRoutine;
    private Coroutine sellScrollRoutine;

    private readonly List<ShopItemRuntime> buyRuntimeItems = new();
    private readonly List<ShopItemRuntime> sellRuntimeItems = new();
    private bool loggedBuySlotInfo;
    private bool loggedSellSlotInfo;
    
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
        Debug.Log("=== PopulateBuyPanel START ===");
        ClearPanel(buyRuntimeItems, buySlotContainer);
        List<ItemData> itemsToSell = new();

        if (sellSeeds)
        {
            Debug.Log($"Adding seeds. Total seeds in database: {ItemDatabase.Instance.allSeeds.Count}");
            itemsToSell.AddRange(ItemDatabase.Instance.allSeeds.Where(s => s != null));
        }
        if (sellTools)
        {
            Debug.Log($"Adding tools. Total tools in database: {ItemDatabase.Instance.allTools.Count}");
            itemsToSell.AddRange(ItemDatabase.Instance.allTools.Where(t => t != null));
        }
        if (sellCrops)
        {
            Debug.Log($"Adding crops. Total crops in database: {ItemDatabase.Instance.allCrops.Count}");
            itemsToSell.AddRange(ItemDatabase.Instance.allCrops.Where(c => c != null));
        }
        if (sellResources)
        {
            Debug.Log($"Adding resources. Total resources in database: {ItemDatabase.Instance.allResources.Count}");
            itemsToSell.AddRange(ItemDatabase.Instance.allResources.Where(r => r != null));
        }

        Debug.Log($"Total items to sell: {itemsToSell.Count}");
        Debug.Log($"Buy Slot Container null? {buySlotContainer == null}");
        Debug.Log($"Item Slot Prefab null? {itemSlotPrefab == null}");

        if (itemsToSell.Count == 0)
        {
            Debug.LogWarning("No items to sell - creating no items message");
            CreateNoItemsMessage("No items available for purchase!", buySlotContainer);
        }
        else
        {
            Debug.Log($"Creating {itemsToSell.Count} buy slots...");
            foreach (var itemData in itemsToSell)
            {
                Debug.Log($"Creating slot for: {itemData.itemName}");
                CreateBuySlot(itemData);
            }
        }

        UpdateButtonStates();
        int padding = itemsToSell.Count == 0 ? emptyStateTopPadding : contentTopPadding;
        EnsureTopPadding(buySlotContainer, padding);
        RebuildLayout(buySlotContainer);
        ResetScroll(buyScrollRect, ref buyScrollRoutine);
        LogContainerState(buySlotContainer, "BUY");
        Debug.Log("=== PopulateBuyPanel END ===");
    }

    private void ConfigureSlotLayout(GameObject slotObj)
    {
        const float cardWidth = 300f;
        const float cardHeight = 350f;

        // Stretch slot to top-center and set a predictable size so GridLayoutGroup can arrange it
        RectTransform rt = slotObj.transform as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
            rt.sizeDelta = new Vector2(cardWidth, cardHeight);
        }

        LayoutElement le = slotObj.GetComponent<LayoutElement>();
        if (le != null)
        {
            le.minWidth = cardWidth;
            le.minHeight = cardHeight;
            le.preferredWidth = cardWidth;
            le.preferredHeight = cardHeight;
            le.flexibleWidth = 0f;
            le.flexibleHeight = 0f;
        }
    }

    private void CreateBuySlot(ItemData itemData)
    {
        GameObject slotObj = Instantiate(itemSlotPrefab, buySlotContainer);
        slotObj.transform.SetParent(buySlotContainer, false);
        slotObj.name = $"BuySlot_{itemData.itemName}";
        ConfigureSlotLayout(slotObj);
        LogSlotDebugOnce(slotObj, "BUY");

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
        Sprite icon = itemData.GetIcon();
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
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
        Debug.Log("=== PopulateSellPanel START ===");
        ClearPanel(sellRuntimeItems, sellSlotContainer);

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found for sell panel!");
            return;
        }

        List<ItemData> itemsToShow = new();

        if (shopBuysCrops)
        {
            Debug.Log("Shop buys crops - checking inventory...");
            var crops = InventoryManager.Instance.GetCrops();
            Debug.Log($"Found {crops.Count} crops in inventory");
            foreach (var crop in crops)
            {
                int quantity = InventoryManager.Instance.GetItemQuantity(crop);
                Debug.Log($"Crop: {crop.itemName}, Quantity: {quantity}, Tradeable: {crop.isTradeable}");
                if (quantity > 0 && crop.isTradeable)
                    itemsToShow.Add(crop);
            }
        }

        if (shopBuysResources)
        {
            Debug.Log("Shop buys resources - checking inventory...");
            var resources = InventoryManager.Instance.GetItemsByType(ItemType.Resource);
            Debug.Log($"Found {resources.Count} resources in inventory");
            foreach (var resource in resources)
            {
                int quantity = InventoryManager.Instance.GetItemQuantity(resource);
                Debug.Log($"Resource: {resource.itemName}, Quantity: {quantity}, Tradeable: {resource.isTradeable}");
                if (quantity > 0 && resource.isTradeable)
                    itemsToShow.Add(resource);
            }
        }

        Debug.Log($"Total items to show in sell panel: {itemsToShow.Count}");

        if (itemsToShow.Count == 0)
        {
            string message = shopBuysCrops ? "No crops to sell!\nGo farm some crops first!" : "No items to sell!";
            Debug.LogWarning($"No items to sell - showing message: {message}");
            CreateNoItemsMessage(message, sellSlotContainer);
        }
        else
        {
            Debug.Log($"Creating {itemsToShow.Count} sell slots...");
            foreach (var itemData in itemsToShow)
            {
                int quantity = InventoryManager.Instance.GetItemQuantity(itemData);
                Debug.Log($"Creating sell slot for: {itemData.itemName} x{quantity}");
                CreateSellSlot(itemData, quantity);
            }
        }
        Debug.Log("=== PopulateSellPanel END ===");
        int padding = itemsToShow.Count == 0 ? emptyStateTopPadding : contentTopPadding;
        EnsureTopPadding(sellSlotContainer, padding);
        RebuildLayout(sellSlotContainer);
        ResetScroll(sellScrollRect, ref sellScrollRoutine);
        LogContainerState(sellSlotContainer, "SELL");
    }

    private void CreateSellSlot(ItemData itemData, int quantity)
    {
        GameObject slotObj = Instantiate(itemSlotPrefab, sellSlotContainer);
        slotObj.transform.SetParent(sellSlotContainer, false);
        slotObj.name = $"SellSlot_{itemData.itemName}";
        ConfigureSlotLayout(slotObj);
        LogSlotDebugOnce(slotObj, "SELL");

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
        Sprite icon = itemData.GetIcon();
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
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
        text.fontSize = noItemsFontSize > 0 ? noItemsFontSize : 36f;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        ApplyPixelFont(text);

        // Give it a reasonable size so the scroll viewport doesn't clip the top
        LayoutElement le = msgObj.AddComponent<LayoutElement>();
        le.minHeight = 200f;
        le.preferredHeight = 220f;
        le.flexibleHeight = 1f;
        
        RectTransform rt = msgObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, 250f);
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
        if (tmpText != null)
        {
            tmpText.text = value;
            ApplyPixelFont(tmpText);
        }
    }

    private void ApplyPixelFont(TextMeshProUGUI tmp)
    {
        if (tmp == null || pixelFont == null) return;

        tmp.font = pixelFont;
        var tex = pixelFont.material != null ? pixelFont.material.mainTexture : null;
        if (tex != null)
        {
            tex.filterMode = FilterMode.Point; // keep pixelated look
        }
    }

    private void EnsureTopPadding(Transform container, int topPadding)
    {
        if (container == null) return;

        var grid = container.GetComponent<GridLayoutGroup>();
        if (grid != null && grid.padding.top != topPadding)
        {
            grid.padding = new RectOffset(
                grid.padding.left,
                grid.padding.right,
                topPadding,
                grid.padding.bottom);
        }
    }

    private void ClearPanel(List<ShopItemRuntime> list, Transform container)
    {
        foreach (var item in list)
        {
            if (item.buyButton != null)
                item.buyButton.onClick.RemoveAllListeners();
        }
        list.Clear();

        if (container != null)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }
    }

    private void RebuildLayout(Transform container)
    {
        if (container == null) return;
        RectTransform rt = container as RectTransform;
        if (rt != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            if (rt.parent != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt.parent as RectTransform);
            Canvas.ForceUpdateCanvases();
        }
    }

    private void ResetScroll(ScrollRect scrollRect, ref Coroutine routine)
    {
        if (scrollRect == null) return;

        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(ResetScrollNextFrame(scrollRect));
    }

    private IEnumerator ResetScrollNextFrame(ScrollRect scrollRect)
    {
        // Wait a frame so Content Size Fitter/GridLayout have applied sizes
        yield return null;
        scrollRect.verticalNormalizedPosition = 1f;
        scrollRect.horizontalNormalizedPosition = 0f;
    }

    private void LogSlotDebugOnce(GameObject slotObj, string panel)
    {
        if (slotObj == null) return;
        if (panel == "BUY" && loggedBuySlotInfo) return;
        if (panel == "SELL" && loggedSellSlotInfo) return;

        RectTransform rt = slotObj.transform as RectTransform;
        if (rt != null)
        {
            Vector2 size = rt.rect.size;
            Vector2 anchored = rt.anchoredPosition;
            var parentRt = rt.parent as RectTransform;
            string parentInfo = parentRt == null ? "none" : $"parent size {parentRt.rect.size}, pos {parentRt.anchoredPosition}";
            Debug.Log($"[{panel}] First slot debug -> anchored: {anchored}, size: {size}, world pos: {rt.position}, parent: {rt.parent?.name}, {parentInfo}");
        }

        if (panel == "BUY") loggedBuySlotInfo = true;
        if (panel == "SELL") loggedSellSlotInfo = true;
    }

    private void LogContainerState(Transform container, string panel)
    {
        if (container == null) return;
        RectTransform rt = container as RectTransform;
        string header = rt == null
            ? $"[{panel}] Container info: no RectTransform, childCount {container.childCount}"
            : $"[{panel}] Container info: size {rt.rect.size}, anchored {rt.anchoredPosition}, scale {rt.lossyScale}, childCount {container.childCount}, active {container.gameObject.activeInHierarchy}";
        Debug.Log(header);

        RectTransform viewport = container.parent as RectTransform;
        int i = 0;
        foreach (Transform child in container)
        {
            if (child == null) continue;
            RectTransform crt = child as RectTransform;
            Vector2 cSize = crt != null ? crt.rect.size : Vector2.zero;
            Vector2 cPos = crt != null ? crt.anchoredPosition : Vector2.zero;
            Vector3 cScale = crt != null ? crt.lossyScale : Vector3.one;
            bool overlapsViewport = false;
            if (crt != null && viewport != null)
            {
                Rect childRect = new Rect(crt.anchoredPosition - Vector2.Scale(crt.rect.size, new Vector2(crt.pivot.x, 1f - crt.pivot.y)), crt.rect.size);
                Rect viewRect = new Rect(Vector2.zero, viewport.rect.size);
                overlapsViewport = childRect.Overlaps(viewRect);
            }
            Debug.Log($"[{panel}] child {i}: name {child.name}, activeSelf {child.gameObject.activeSelf}, anchored {cPos}, size {cSize}, scale {cScale}, overlapsViewport {overlapsViewport}");
            i++;
            if (i >= 5) break; // avoid spamming
        }
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
