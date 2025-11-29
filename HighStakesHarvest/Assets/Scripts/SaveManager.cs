using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[System.Serializable]
public class SaveData
{
    public int farmSceneTutoSeen = 0;
    public int casinoSceneTutoSeen = 0;
    public int slotsSceneTutoSeen = 0;
    public int casinoTableTutoSeen = 0;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [Header("Tutorial Prefabs (assign in Inspector)")]
    public GameObject farmSceneTutorialPrefab;
    public GameObject casinoSceneTutorialPrefab;
    public GameObject slotsTutorialPrefab;
    public GameObject casinoTableTutorialPrefab;

    private string savePath;
    public SaveData data;

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");

        // Load if file exists (do not create automatically here —
        // creation will be triggered by the Play button as you requested)
        if (File.Exists(savePath))
            LoadGame();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called by Play button before first scene load
    public void InitializeSaveFile()
    {
        if (File.Exists(savePath))
        {
            LoadGame();
        }
        else
        {
            data = new SaveData();
            SaveGame();
            Debug.Log("[SaveManager] New save file created at: " + savePath);
        }
    }

    private void LoadGame()
    {
        try
        {
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) data = new SaveData();
            Debug.Log("[SaveManager] Loaded save file.");
        }
        catch
        {
            data = new SaveData();
            Debug.LogWarning("[SaveManager] Failed to load save file, using defaults.");
        }
    }

    public void SaveGame()
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log("[SaveManager] Save file written.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[SaveManager] Failed to write save file: " + ex.Message);
        }
    }

    // Scene loaded -> try show tutorial (single place of truth)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[SaveManager] Scene loaded: " + scene.name);

        switch (scene.name)
        {
            case "FarmScene":
                TryShowTutorial(ref data.farmSceneTutoSeen, farmSceneTutorialPrefab, "FarmScene");
                break;
            case "CasinoScene":
                TryShowTutorial(ref data.casinoSceneTutoSeen, casinoSceneTutorialPrefab, "CasinoScene");
                break;
            case "Slots":
                TryShowTutorial(ref data.slotsSceneTutoSeen, slotsTutorialPrefab, "Slots");
                break;
            case "CasinoTable":
                TryShowTutorial(ref data.casinoTableTutoSeen, casinoTableTutorialPrefab, "CasinoTable");
                break;
            default:
                // nothing to do
                break;
        }
    }

    private void TryShowTutorial(ref int flag, GameObject prefab, string sceneLabel)
    {
        if (flag == 1)
        {
            Debug.Log($"[SaveManager] {sceneLabel} tutorial already seen (flag=1).");
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"[SaveManager] Tutorial prefab for {sceneLabel} is not assigned.");
            return;
        }

        Canvas targetCanvas = FindObjectOfType<Canvas>();
        GameObject instance;

        if (targetCanvas == null)
        {
            Debug.LogWarning($"[SaveManager] No Canvas found in scene {sceneLabel}. Instantiating at root.");
            instance = Instantiate(prefab);
        }
        else
        {
            instance = Instantiate(prefab, targetCanvas.transform);

            // Keep prefab’s own RectTransform settings!
            RectTransform rt = instance.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.anchoredPosition3D = Vector3.zero;
            }
        }

        Debug.Log($"[SaveManager] Spawned tutorial prefab for {sceneLabel}.");

        flag = 1;
        SaveGame();
    }

}
