using UnityEngine;

public class ResumeButton : MonoBehaviour
{
    public void OnResumeButtonClicked()
    {
        // Find the active PauseManager in the scene
        PauseManager manager = FindObjectOfType<PauseManager>();
        if (manager != null)
        {
            manager.Resume();
        }
        else
        {
            Debug.LogWarning("PauseManager not found in scene!");
        }
    }
}
