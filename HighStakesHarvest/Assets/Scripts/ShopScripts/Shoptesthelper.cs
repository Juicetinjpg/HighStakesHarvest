using UnityEngine;

/// <summary>
/// Test helper to add crops to inventory for testing the shop system
/// Attach this to any GameObject in CasinoScene
/// Press T to add test crops
/// </summary>
public class ShopTestHelper : MonoBehaviour
{
    [Header("Test Crops - Drag ScriptableObjects here")]
    [SerializeField] private CropData[] testCrops;

    [Header("Test Settings")]
    [SerializeField] private int quantityPerCrop = 5;
    [SerializeField] private KeyCode addCropsKey = KeyCode.T;
    [SerializeField] private KeyCode clearInventoryKey = KeyCode.C;
    [SerializeField] private KeyCode addMoneyKey = KeyCode.M;
    [SerializeField] private int moneyToAdd = 100;

    void Update()
    {
        // Press T to add test crops
        if (Input.GetKeyDown(addCropsKey))
        {
            AddTestCrops();
        }

        // Press C to clear inventory
        if (Input.GetKeyDown(clearInventoryKey))
        {
            ClearInventory();
        }

        // Press M to add money
        if (Input.GetKeyDown(addMoneyKey))
        {
            AddTestMoney();
        }
    }

    /// <summary>
    /// Adds test crops to inventory
    /// </summary>
    public void AddTestCrops()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found!");
            return;
        }

        if (testCrops == null || testCrops.Length == 0)
        {
            Debug.LogWarning("No test crops assigned! Drag some CropData ScriptableObjects to this component.");

            // Try to get crops from ItemDatabase
            if (ItemDatabase.Instance != null)
            {
                var allCrops = ItemDatabase.Instance.allCrops;
                if (allCrops != null && allCrops.Count > 0)
                {
                    Debug.Log("Using crops from ItemDatabase instead...");
                    foreach (var crop in allCrops)
                    {
                        if (crop != null)
                        {
                            InventoryManager.Instance.AddItem(crop, quantityPerCrop);
                            Debug.Log($"✓ Added {quantityPerCrop}x {crop.itemName} to inventory");
                        }
                    }
                    return;
                }
            }

            Debug.LogError("Could not find any crops to add!");
            return;
        }

        // Add assigned test crops
        foreach (CropData crop in testCrops)
        {
            if (crop != null)
            {
                InventoryManager.Instance.AddItem(crop, quantityPerCrop);
                Debug.Log($"✓ Added {quantityPerCrop}x {crop.itemName} to inventory");
            }
        }

        Debug.Log($"🎁 Added {testCrops.Length} crop types to inventory!");
    }

    /// <summary>
    /// Clears inventory for testing
    /// </summary>
    public void ClearInventory()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.ClearInventory();
            Debug.Log("🗑️ Inventory cleared!");
        }
        else
        {
            Debug.LogError("PlayerInventory not found!");
        }
    }

    /// <summary>
    /// Adds test money
    /// </summary>
    public void AddTestMoney()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(moneyToAdd);
            Debug.Log($"💰 Added ${moneyToAdd}! Total: ${MoneyManager.Instance.GetMoney()}");
        }
        else
        {
            Debug.LogError("MoneyManager not found!");
        }
    }

    /// <summary>
    /// Creates a quick test crop at runtime if no crops assigned
    /// </summary>
    private CropData CreateTestCrop(string name, int price)
    {
        CropData crop = ScriptableObject.CreateInstance<CropData>();
        crop.itemName = name;
        crop.basePrice = price;
        crop.sellPriceMultiplier = 0.5f;
        crop.itemType = ItemType.Crop;
        crop.isTradeable = true;
        return crop;
    }
}