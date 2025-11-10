using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Dynamic CasinoShop that auto-populates items from ItemDatabase
/// and generates UI slots automatically!
/// </summary>
public class CasinoShop : MonoBehaviour
{
    [Header("What to Sell")]
    [SerializeField] private bool sellSeeds = true;
    [SerializeField] private bool sellTools = true;
    [SerializeField] private bool sellCrops = false;
    [SerializeField] private bool sellResources = false;

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
        if (!ValidateManagers()) return;

        MoneyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
        UpdateMoneyDisplay(MoneyManager.Instance.GetMoney());

        PopulateShopFromDatabase();

        HideAllPanels();
    }

    private void OnDestroy()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
        }

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
    /// Automatically populates the shop from ItemDatabase based on settings
    /// </summary>
    private void PopulateShopFromDatabase()
    {
        List<ItemData> itemsToSell = new List<ItemData>();

        // Gather items based on what we want to sell
        if (sellSeeds)
        {
            itemsToSell.AddRange(ItemDatabase.Instance.allSeeds.Where(s => s != null));
        }

        if (sellTools)
        {
            itemsToSell.AddRange(ItemDatabase.Instance.allTools.Where(t => t != null));
        }

        if (sellCrops)
        {
            itemsToSell.AddRange(ItemDatabase.Instance.allCrops.Where(c => c != null));
        }

        if (sellResources)
        {
            itemsToSell.AddRange(ItemDatabase.Instance.allResources.Where(r => r != null));
        }

        Debug.Log($"CasinoShop: Found {itemsToSell.Count} items to sell");

        // Create UI slot for each item
        foreach (var itemData in itemsToSell)
        {
            CreateItemSlot(itemData);
        }

        UpdateButtonStates();
    }

    /// <summary>
    /// Creates a UI slot for an item
    /// </summary>
    private void CreateItemSlot(ItemData itemData)
    {
        // Instantiate the prefab
        GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotContainer);
        slotObj.name = $"ItemSlot_{itemData.itemName}";

        // Calculate cost
        int cost = useItemBasePrices ? itemData.GetBuyPrice() : itemData.basePrice;
        cost = Mathf.CeilToInt(cost * priceMultiplier);

        // Find UI components in the instantiated prefab
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
            ItemData localItemData = itemData; // Capture for lambda
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

        Debug.Log($"CasinoShop: Created slot for {itemData.itemName} - ${cost}");
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
                ShowPurchaseSuccess(itemData.itemName);
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

    private void UpdateMoneyDisplay(int currentMoney)
    {
        string moneyText = $"${currentMoney}";

        if (moneyDisplayText != null)
            moneyDisplayText.text = moneyText;

        if (moneyDisplayTextTMP != null)
            moneyDisplayTextTMP.text = moneyText;

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (MoneyManager.Instance == null) return;

        int currentMoney = MoneyManager.Instance.GetMoney();

        foreach (var item in runtimeItems)
        {
            if (item.buyButton != null)
            {
                item.buyButton.interactable = currentMoney >= item.cost;
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

    private void ShowPurchaseSuccess(string itemName)
    {
        if (purchaseSuccessPanel != null)
        {
            purchaseSuccessPanel.SetActive(true);

            string successText = $"Purchased: {itemName}!";
            SetText(purchaseSuccessText, purchaseSuccessTextTMP, successText);

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
    /// Manually refresh the shop (useful if ItemDatabase changes)
    /// </summary>
    [ContextMenu("Refresh Shop")]
    public void RefreshShop()
    {
        // Clear existing items
        foreach (var item in runtimeItems)
        {
            if (item.slotObject != null)
            {
                Destroy(item.slotObject);
            }
        }
        runtimeItems.Clear();

        // Repopulate
        PopulateShopFromDatabase();
    }
}