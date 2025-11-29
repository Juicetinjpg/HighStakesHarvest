using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainToFarm : MonoBehaviour
{
    void Start()
    {
        Button button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnButtonClick);
        else
            Debug.LogError("No Button component found on this GameObject.");
    }

    void OnButtonClick()
    {
        // Initialize save file before starting the game
        SaveManager.Instance.InitializeSaveFile();

        // Load the first scene
        SceneManager.LoadScene("FarmScene");
    }
}
