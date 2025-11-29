using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Loads the dedicated ShopScene from the casino UI.
/// </summary>
public class CasinoShopButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnShopClicked);
        }
        else
        {
            Debug.LogError("CasinoShopButton: No Button component found on this GameObject.");
        }
    }

    public void OnShopClicked()
    {
        SceneManager.LoadScene("ShopScene");
    }
}
