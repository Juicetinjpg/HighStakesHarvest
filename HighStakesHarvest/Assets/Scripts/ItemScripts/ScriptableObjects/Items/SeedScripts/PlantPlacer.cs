using UnityEngine;
using UnityEngine.Tilemaps;

public class PlantPlacer : MonoBehaviour
{
    public Tilemap soilTilemap;        // assign your Tilemap in the Inspector
    public GameObject Potato;      // assign your crop prefabs here
    public GameObject Blueberry;
    public GameObject Pumpkin;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = soilTilemap.WorldToCell(worldPos);
            Vector3 placePos = soilTilemap.GetCellCenterWorld(cellPos);

            switch (ToolsManager.Instance.currentAction)
            {
                case PlayerAction.Plant1:
                    TryPlantPotato(placePos);
                    break;

                case PlayerAction.Plant2:
                    TryPlantBlueberry(placePos);
                    break;

                case PlayerAction.Plant3:
                    TryPlantPumpkin(placePos);
                    break;

                case PlayerAction.Water:
                    TryWater(placePos);
                    break;

                case PlayerAction.Harvest:
                    TryHarvest(placePos);
                    break;
            }
        }
    }

    void TryPlantPotato(Vector3 placePos)
    {
        Collider2D hit = Physics2D.OverlapPoint(placePos);
        if (hit == null)
        {
            GameObject go = Instantiate(Potato, placePos, Quaternion.identity);
            if (PlantManager.Instance != null)
            {
                PlantManager.Instance.AddPlant(go);
            }
            else
            {
                DontDestroyOnLoad(go);
            }

            Debug.Log("Planted Potato seed at " + placePos);
        }
    }

    void TryPlantBlueberry(Vector3 placePos)
    {
        Collider2D hit = Physics2D.OverlapPoint(placePos);
        if (hit == null)
        {
            GameObject go = Instantiate(Blueberry, placePos, Quaternion.identity);
            if (PlantManager.Instance != null)
            {
                PlantManager.Instance.AddPlant(go);
            }
            else
            {
                DontDestroyOnLoad(go);
            }

            Debug.Log("Planted Blueberry seed at " + placePos);
        }
    }

    void TryPlantPumpkin(Vector3 placePos)
    {
        Collider2D hit = Physics2D.OverlapPoint(placePos);
        if (hit == null)
        {
            GameObject go = Instantiate(Pumpkin, placePos, Quaternion.identity);
            if (PlantManager.Instance != null)
            {
                PlantManager.Instance.AddPlant(go);
            }
            else
            {
                DontDestroyOnLoad(go);
            }

            Debug.Log("Planted Pumkin seed at " + placePos);
        }
    }

    void TryWater(Vector3 placePos)
    {
        Collider2D hit = Physics2D.OverlapPoint(placePos);
        if (hit != null)
        {
            Plant plant = hit.GetComponent<Plant>();
            if (plant != null)
            {
                plant.Water();
                Debug.Log("Watered plant at " + placePos);
            }
        }
    }

    void TryHarvest(Vector3 placePos)
    {
        Collider2D hit = Physics2D.OverlapPoint(placePos);
        if (hit != null)
        {
            Plant plant = hit.GetComponent<Plant>();
            if (plant != null)
            {
                if (plant.IsFullyGrown())
                {
                    if (PlantManager.Instance != null)
                        PlantManager.Instance.RemovePlant(plant.gameObject);

                    Destroy(plant.gameObject);
                    Debug.Log("Harvested plant at " + placePos);
                }
                else
                {
                    Debug.Log("Plant is not fully grown at " + placePos);
                }
            }
        }
    }
}
