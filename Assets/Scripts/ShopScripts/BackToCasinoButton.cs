using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Simple back button to return to CasinoScene.
/// </summary>
public class BackToCasinoButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnBackClicked);
        }
        else
        {
            Debug.LogError("BackToCasinoButton: No Button component found.");
        }
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene("CasinoScene");
    }
}
