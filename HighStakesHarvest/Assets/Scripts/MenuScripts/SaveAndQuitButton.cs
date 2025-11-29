using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveAndQuitButton : MonoBehaviour
{
    public void SaveAndQuit()
    {
        // Ensure SaveManager exists
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[SaveAndQuitButton] No SaveManager found! Cannot save.");
            return;
        }

        SavePlayerState();

        // Write the save file to disk
        SaveManager.Instance.SaveGame();

        Debug.Log("[SaveAndQuitButton] Game saved. Returning to Main Menu...");

        // Load Main Menu
        SceneManager.LoadScene("MainMenu");
    }

    private void SavePlayerState()
    {
        SaveData data = SaveManager.Instance.data;

        // ---------------------------
        // SAVE INVENTORY
        // ---------------------------
        if (PlayerInventory.Instance != null)
        {
            data.inventoryJson = PlayerInventory.Instance.SaveInventoryData();
            Debug.Log("[SaveAndQuitButton] Saved player inventory.");
        }
        else
        {
            Debug.LogWarning("[SaveAndQuitButton] PlayerInventory not found. Inventory not saved.");
        }

        // ---------------------------
        // SAVE QUOTA / MONEY / TURNS
        // ---------------------------
        if (QuotaManager.Instance != null)
        {
            data.savedQuotaIndex = QuotaManager.Instance.GetCurrentQuotaIndex();
            data.savedTurnsRemaining = QuotaManager.Instance.GetTurnsRemaining();
            Debug.Log("[SaveAndQuitButton] Saved quotaIndex=" + data.savedQuotaIndex +
                      " turns=" + data.savedTurnsRemaining);
        }
        else
        {
            Debug.LogWarning("[SaveAndQuitButton] QuotaManager not found. Quota not saved.");
        }

        if (MoneyManager.Instance != null)
        {
            data.savedMoney = MoneyManager.Instance.GetMoney();
            Debug.Log("[SaveAndQuitButton] Saved money=" + data.savedMoney);
        }
        else
        {
            Debug.LogWarning("[SaveAndQuitButton] MoneyManager not found. Money not saved.");
        }
    }
}
