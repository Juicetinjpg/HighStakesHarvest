using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Player Prefab")]
    public GameObject playerPrefab;

    private GameObject currentPlayer;
    private string currentSceneName;
    private string savedInventoryData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        if (currentSceneName == "FarmScene")
        {
            SpawnPlayer();
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "FarmScene" && currentPlayer != null)
        {
            SaveInventory();
            Destroy(currentPlayer);
        }
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerManager: No player prefab assigned!");
            return;
        }

        // Instantiate the player in the scene
        currentPlayer = Instantiate(playerPrefab);
        currentPlayer.name = "Player";

        // Restore saved inventory data if available
        var inventory = currentPlayer.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            if (!string.IsNullOrEmpty(savedInventoryData))
                inventory.LoadInventoryData(savedInventoryData);
        }
    }

    private void SaveInventory()
    {
        if (currentPlayer == null) return;

        var inventory = currentPlayer.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            savedInventoryData = inventory.SaveInventoryData();
        }
    }
}
