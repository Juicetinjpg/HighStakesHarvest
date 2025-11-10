using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI component to display player money.
/// Updates automatically when money changes.
/// Can be placed in any scene (Casino, Blackjack, Slots, etc.)
/// </summary>
public class MoneyDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Text moneyText;

    [Header("Display Format")]
    [SerializeField] private string prefix = "$";
    [SerializeField] private string suffix = "";
    [SerializeField] private bool showLabel = true;
    [SerializeField] private string label = "Money: ";

    private void Start()
    {
        if (moneyText == null)
        {
            moneyText = GetComponent<Text>();
        }

        if (moneyText == null)
        {
            Debug.LogError("MoneyDisplay: No Text component found!");
            return;
        }

        // Subscribe to money changes
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateDisplay;
            UpdateDisplay(MoneyManager.Instance.GetMoney());
        }
        else
        {
            Debug.LogError("MoneyDisplay: MoneyManager instance not found!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from money changes
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateDisplay;
        }
    }

    private void UpdateDisplay(int currentMoney)
    {
        if (moneyText == null) return;

        string displayText = "";

        if (showLabel)
            displayText += label;

        displayText += prefix + currentMoney.ToString() + suffix;

        moneyText.text = displayText;
    }

    /// <summary>
    /// Manually refresh the display (useful if MoneyManager loads after this)
    /// </summary>
    public void RefreshDisplay()
    {
        if (MoneyManager.Instance != null)
        {
            UpdateDisplay(MoneyManager.Instance.GetMoney());
        }
    }
}