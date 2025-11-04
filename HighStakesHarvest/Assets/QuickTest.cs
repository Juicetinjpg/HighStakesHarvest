using UnityEngine;

public class QuickTest : MonoBehaviour
{
    public SeedData seed;
    public ToolData wateringCan;

    void Start()
    {
        // Add 10 seeds and a watering can
        InventoryManager.Instance.AddItem(seed, 10);
        InventoryManager.Instance.AddItem(wateringCan, 1);
        Debug.Log("Added test items!");
    }
}