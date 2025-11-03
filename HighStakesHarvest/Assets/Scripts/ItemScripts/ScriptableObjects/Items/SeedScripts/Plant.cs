using UnityEngine;

public class Plant : MonoBehaviour
{
    public SeedData seedData;
    public GameObject waterIconPrefab; // drag prefab here in Inspector

    private int currentStage = 0;
    private bool needsWater = true;
    private GameObject currentVisual;
    private GameObject waterIconInstance;

    void Start()
    {
        SpawnStage(currentStage);
        ShowWaterIcon(true); // starts thirsty
    }

    public void Water()
    {
        needsWater = false;
        ShowWaterIcon(false);
        Debug.Log(seedData.seedName + " has been watered.");
    }

    public void AdvanceTurn()
    {
        if (!needsWater) // only grow if watered
        {
            if (currentStage < seedData.growthStages.Length - 1)
            {
                currentStage++;
                SpawnStage(currentStage);
                Debug.Log(seedData.seedName + " grew to stage " + currentStage);
            }
        }
        else
        {
            Debug.Log(seedData.seedName + " did not grow because it wasn’t watered.");
        }

        // Reset for next turn
        needsWater = true;
        ShowWaterIcon(true);
    }

    private void SpawnStage(int stage)
    {
        if (currentVisual != null)
            Destroy(currentVisual);

        if (seedData.growthStages[stage] != null)
            currentVisual = Instantiate(seedData.growthStages[stage], transform.position, Quaternion.identity, transform);
    }

    private void ShowWaterIcon(bool show)
    {
        if (show)
        {
            if (waterIconInstance == null && waterIconPrefab != null)
            {
                Vector3 iconPos = transform.position + Vector3.up * .5f; // float above plant
                waterIconInstance = Instantiate(waterIconPrefab, iconPos, Quaternion.identity, transform);
            }
        }
        else
        {
            if (waterIconInstance != null)
                Destroy(waterIconInstance);
        }
    }

    public bool IsFullyGrown()
    {
        return currentStage == seedData.growthStages.Length - 1;
    }
}
