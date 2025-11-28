using UnityEngine;

public class SceneFadeSpawner : MonoBehaviour
{
    public GameObject blackFadePrefab;

    private void Start()
    {
        // Spawn the fade panel into the scene Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null && blackFadePrefab != null)
        {
            Instantiate(blackFadePrefab, canvas.transform);
        }
    }
}
