using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FarmhandManager : MonoBehaviour
{
    [Header("Farmhand Settings")]
    [Tooltip("Prefab to instantiate as the farmhand AI")]
    public GameObject farmhandPrefab;

    [Tooltip("Optional spawn point for the farmhand. If null, spawns at Vector3.zero.")]
    public Transform spawnPoint;

    [Tooltip("If you prefer automatic lookup: the GameObject name used to locate the spawn point when joining the FarmScene.")]
    public string spawnPointObjectName = "FarmhandSpawnPoint";

    [Tooltip("Optional: lookup the spawn point by tag if name lookup fails. Leave blank to disable.")]
    public string spawnPointTag = "";

    [Header("AI Settings")]
    [Tooltip("Movement speed of the farmhand in units per second")]
    public float moveSpeed = 2f;

    [Tooltip("Distance at which the farmhand is considered close enough to water the plant")]
    public float engageDistance = 0f;

    [Tooltip("How long the farmhand waits at the plant before watering (seconds)")]
    public float waitBeforeWater = 1f;

    // Whether the farmhand should be present on the farm
    [SerializeField]
    private bool farmhandActive = false;

    // Reference to the currently spawned farmhand instance
    private GameObject currentFarmhand;

    private Coroutine farmhandRoutine;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Try to resolve spawnPoint for the starting scene
        ResolveSpawnPoint();

        // If the game started on the farm scene, ensure the farmhand state is applied
        if (SceneManager.GetActiveScene().name == "FarmScene")
        {
            if (farmhandActive)
                SpawnFarmhand();
            else
                DespawnFarmhand();
        }
        else
        {
            // Not on farm scene, make sure farmhand isn't present
            DespawnFarmhand();
        }
    }

    void Update()
    {
        // Intentionally left empty. Farmhand AI behaviour runs in coroutine started on spawn.
    }

    public void ToggleFarmhand()
    {
        farmhandActive = !farmhandActive;
        Debug.Log("Farmhand Active: " + farmhandActive);

        // Apply immediately if we're on the farm scene
        if (SceneManager.GetActiveScene().name == "FarmScene")
        {
            if (farmhandActive)
                SpawnFarmhand();
            else
                DespawnFarmhand();
        }
    }

    public bool IsFarmhandActive() => farmhandActive;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Resolve spawn point after the scene has been loaded so a scene-local transform can be found
        ResolveSpawnPoint();

        if (scene.name == "FarmScene")
        {
            if (farmhandActive)
                SpawnFarmhand();
            else
                DespawnFarmhand();
        }
        else
        {
            // Ensure farmhand is not present in other scenes
            DespawnFarmhand();
        }
    }

    /// <summary>
    /// Ensure spawnPoint references an object in the active scene (or try to find one by name/tag).
    /// Call this after scene loads and before spawning.
    /// </summary>
    private void ResolveSpawnPoint()
    {
        // If we already have a spawnPoint, ensure it belongs to the active scene.
        if (spawnPoint != null)
        {
            // If the referenced object was from a different/unloaded scene it may no longer be valid.
            // Compare the Scene structs to ensure it's in the currently active scene.
            if (spawnPoint.gameObject != null && spawnPoint.gameObject.scene == SceneManager.GetActiveScene())
                return;

            // Otherwise clear it so we attempt to find a new one
            spawnPoint = null;
        }

        // Try to find by explicit name first
        if (!string.IsNullOrEmpty(spawnPointObjectName))
        {
            var found = GameObject.Find(spawnPointObjectName);
            if (found != null && found.scene == SceneManager.GetActiveScene())
            {
                spawnPoint = found.transform;
                Debug.Log($"FarmhandManager: resolved spawnPoint by name '{spawnPointObjectName}'.");
                return;
            }
        }

        // Next try to find by tag (if provided)
        if (!string.IsNullOrEmpty(spawnPointTag))
        {
            try
            {
                var tagged = GameObject.FindWithTag(spawnPointTag);
                if (tagged != null && tagged.scene == SceneManager.GetActiveScene())
                {
                    spawnPoint = tagged.transform;
                    Debug.Log($"FarmhandManager: resolved spawnPoint by tag '{spawnPointTag}'.");
                    return;
                }
            }
            catch
            {
                // FindWithTag throws if tag doesn't exist; ignore silently.
            }
        }

        // No spawn point found for this scene — leaving spawnPoint null will cause spawn to use Vector3.zero.
        Debug.Log("FarmhandManager: spawnPoint not found in scene; using Vector3.zero when spawning.");
    }

    public void SpawnFarmhand()
    {
        if (currentFarmhand != null)
            return; // already spawned

        if (farmhandPrefab == null)
        {
            Debug.LogWarning("Farmhand prefab not assigned in FarmhandManager.");
            return;
        }

        // Re-resolve spawn point before spawning (useful if spawnPoint was a scene object that was just created)
        ResolveSpawnPoint();

        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        currentFarmhand = Instantiate(farmhandPrefab, pos, rot, transform);
        currentFarmhand.name = farmhandPrefab.name; // avoid Inst (Clone) name if desired

        Debug.Log("Spawned farmhand.");

        // Start AI routine
        if (farmhandRoutine != null)
            StopCoroutine(farmhandRoutine);
        farmhandRoutine = StartCoroutine(FarmhandRoutine());
    }

    public void DespawnFarmhand()
    {
        // Stop AI routine first
        if (farmhandRoutine != null)
        {
            StopCoroutine(farmhandRoutine);
            farmhandRoutine = null;
        }

        if (currentFarmhand != null)
        {
            Destroy(currentFarmhand);
            currentFarmhand = null;
            Debug.Log("Despawned farmhand.");
        }
    }

    /// <summary>
    /// Returns the world position the farmhand should move to in order to water the plant.
    /// Prefers a child Transform named "WaterPoint", otherwise falls back to the plant root position.
    /// </summary>
    private Vector3 GetPlantWaterPosition(GameObject plantObj)
    {
        if (plantObj == null) return Vector3.zero;

        // Look for a child marker named "WaterPoint"
        var marker = plantObj.transform.Find("WaterPoint");
        if (marker != null)
            return marker.position;

        // Optionally, if your Plant component exposes a specific watering point you could use that here.
        // Fallback to the plant object's position.
        return plantObj.transform.position;
    }

    private IEnumerator FarmhandRoutine()
    {
        // Loop while farmhand should be active and the instance exists and we're on the farm
        while (farmhandActive && currentFarmhand != null && SceneManager.GetActiveScene().name == "FarmScene")
        {
            // Find nearest plant that needs water
            if (PlantManager.Instance == null)
            {
                Debug.LogWarning("PlantManager not found for Farmhand AI.");
                yield return new WaitForSeconds(2f);
                continue;
            }

            GameObject targetObj = null;
            Plant targetPlant = null;
            float bestDist = float.MaxValue;

            var plants = PlantManager.Instance.Plants;
            for (int i = 0; i < plants.Count; i++)
            {
                var pObj = plants[i];
                if (pObj == null) continue;

                // Only consider active plants
                if (!pObj.activeInHierarchy) continue;

                var plantComp = pObj.GetComponent<Plant>();
                if (plantComp == null) continue;

                // Check if needs water
                if (!plantComp.needsWater) continue;

                // Use the resolved watering position when evaluating distance
                Vector3 waterPos = GetPlantWaterPosition(pObj);
                float d = Vector3.Distance(currentFarmhand.transform.position, waterPos);
                if (d < bestDist)
                {
                    bestDist = d;
                    targetObj = pObj;
                    targetPlant = plantComp;
                }
            }

            if (targetObj == null || targetPlant == null)
            {
                // Nothing to water right now
                yield return new WaitForSeconds(2f);
                continue;
            }

            // Determine target position (water anchor or plant position)
            Vector3 targetPos = GetPlantWaterPosition(targetObj);

            // Move toward the target plant until within engageDistance
            while (currentFarmhand != null && targetObj != null && Vector3.Distance(currentFarmhand.transform.position, targetPos) > engageDistance)
            {
                // Re-check conditions
                if (!farmhandActive || SceneManager.GetActiveScene().name != "FarmScene")
                    yield break;

                currentFarmhand.transform.position = Vector3.MoveTowards(currentFarmhand.transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;

                // If plant no longer needs water, break out and look for another
                if (targetPlant == null || !targetPlant.needsWater)
                    break;
            }

            // Ensure target still valid and needs water
            if (targetPlant != null && targetObj != null && targetPlant.needsWater)
            {
                // Wait a bit (simulate watering action)
                yield return new WaitForSeconds(waitBeforeWater);

                // Final check then water
                if (targetPlant != null && targetPlant.needsWater)
                {
                    targetPlant.Water();
                    Debug.Log($"Farmhand watered {targetPlant.seedData?.itemName ?? targetObj.name}");
                }
            }

            // Short pause before next search
            yield return new WaitForSeconds(0.5f);
        }

        farmhandRoutine = null;
    }
}
