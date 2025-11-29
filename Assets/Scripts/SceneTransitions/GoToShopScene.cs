using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Loads the ShopScene when the attached button is clicked.
/// </summary>
public class GoToShopScene : MonoBehaviour
{
    [SerializeField] private string shopSceneName = "ShopScene";

    private void Awake()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => SceneManager.LoadScene(shopSceneName));
        }
        else
        {
            Debug.LogError("GoToShopScene: No Button component found on this GameObject.");
        }
    }

    // Optional public hook for UnityEvents
    public void LoadShopScene()
    {
        SceneManager.LoadScene(shopSceneName);
    }
}
