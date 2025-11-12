using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Button script to transition from CasinoScene to SlotScene.
/// Follows the same pattern as your existing GoToCasinoScene script.
/// </summary>
public class GoToSlotScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string slotSceneName = "SlotScene"; // Change this to match your exact scene name

    void Start()
    {
        // Get the Button component attached to this GameObject
        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("GoToSlotScene: No Button component found on this GameObject.");
        }
    }

    void OnButtonClick()
    {
        // Load the Slot Scene
        SceneManager.LoadScene(slotSceneName);
        Debug.Log($"Loading {slotSceneName}...");
    }
}