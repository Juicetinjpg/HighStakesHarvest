using UnityEngine;

public class SceneFadeSpawner : MonoBehaviour
{
    private void Start()
    {
        FadeController fade = FindObjectOfType<FadeController>(true);

        if (fade != null)
        {
            Debug.Log("Triggering fade");
            fade.BeginFade();   // <--- now we call it manually
        }
        else
        {
            Debug.LogError("FadeController not found!");
        }
    }
}
