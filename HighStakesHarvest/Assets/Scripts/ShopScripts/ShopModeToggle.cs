using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls a SINGLE toggle button for switching between Buy and Sell modes
/// Button text changes: "Switch to Sell" <-> "Switch to Buy"
/// Attach this to the toggle button itself
/// </summary>
public class ShopModeToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CasinoShop casinoShop;

    [Header("Button Text")]
    [SerializeField] private TextMeshProUGUI buttonTextTMP;
    [SerializeField] private Text buttonTextLegacy;

    [Header("Text Labels")]
    [SerializeField] private string buyModeText = "Switch to Sell";
    [SerializeField] private string sellModeText = "Switch to Buy";

    [Header("Optional: Shop Title")]
    [SerializeField] private TextMeshProUGUI shopTitleTMP;
    [SerializeField] private Text shopTitleLegacy;
    [SerializeField] private string buyModeTitle = "Buy Items";
    [SerializeField] private string sellModeTitle = "Sell Crops";

    [Header("Optional: Color Feedback")]
    [SerializeField] private bool changeButtonColor = true;
    [SerializeField] private Color buyModeColor = new Color(0.3f, 0.6f, 0.9f, 1f); // Blue
    [SerializeField] private Color sellModeColor = new Color(0.3f, 0.8f, 0.3f, 1f); // Green

    private Button button;
    private bool isBuyMode = true;

    void Start()
    {
        // Get button component
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("ShopModeToggle: No Button component found!");
            return;
        }

        // Find CasinoShop if not assigned
        if (casinoShop == null)
        {
            casinoShop = FindObjectOfType<CasinoShop>();
            if (casinoShop == null)
            {
                Debug.LogError("ShopModeToggle: CasinoShop not found!");
                return;
            }
        }

        // Try to find button text if not assigned
        if (buttonTextTMP == null)
        {
            buttonTextTMP = GetComponentInChildren<TextMeshProUGUI>();
        }
        if (buttonTextLegacy == null)
        {
            buttonTextLegacy = GetComponentInChildren<Text>();
        }

        // Setup button listener
        button.onClick.AddListener(ToggleMode);

        // Set initial state
        UpdateUI();
    }

    /// <summary>
    /// Toggles between buy and sell mode
    /// </summary>
    public void ToggleMode()
    {
        isBuyMode = !isBuyMode;

        if (isBuyMode)
        {
            casinoShop.SetBuyMode();
            Debug.Log("Switched to BUY mode");
        }
        else
        {
            casinoShop.SetSellMode();
            Debug.Log("Switched to SELL mode");
        }

        UpdateUI();
    }

    /// <summary>
    /// Updates button text and visuals based on current mode
    /// </summary>
    private void UpdateUI()
    {
        // Update button text
        string buttonText = isBuyMode ? buyModeText : sellModeText;

        if (buttonTextTMP != null)
        {
            buttonTextTMP.text = buttonText;
        }

        if (buttonTextLegacy != null)
        {
            buttonTextLegacy.text = buttonText;
        }

        // Update shop title if assigned
        string titleText = isBuyMode ? buyModeTitle : sellModeTitle;

        if (shopTitleTMP != null)
        {
            shopTitleTMP.text = titleText;
        }

        if (shopTitleLegacy != null)
        {
            shopTitleLegacy.text = titleText;
        }

        // Update button color if enabled
        if (changeButtonColor && button != null)
        {
            ColorBlock colors = button.colors;
            Color targetColor = isBuyMode ? buyModeColor : sellModeColor;
            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor * 1.2f;
            colors.pressedColor = targetColor * 0.8f;
            button.colors = colors;
        }
    }

    /// <summary>
    /// Force set to buy mode
    /// </summary>
    public void SetBuyMode()
    {
        if (!isBuyMode)
        {
            isBuyMode = true;
            casinoShop.SetBuyMode();
            UpdateUI();
        }
    }

    /// <summary>
    /// Force set to sell mode
    /// </summary>
    public void SetSellMode()
    {
        if (isBuyMode)
        {
            isBuyMode = false;
            casinoShop.SetSellMode();
            UpdateUI();
        }
    }
}