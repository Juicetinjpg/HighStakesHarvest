using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
            Debug.LogError("QuitGame: No Button component found on this GameObject.");
        }
    }

    void OnButtonClick()
    {
        Debug.Log("Quit button pressed.");

        // If running in the Unity Editor, stop Play Mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running as a built application, quit the game
        Application.Quit();
#endif
    }
}
