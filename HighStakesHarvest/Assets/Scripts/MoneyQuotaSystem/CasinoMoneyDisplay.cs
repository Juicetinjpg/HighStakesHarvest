using UnityEngine;
using TMPro;

/// <summary>
/// Displays player money in casino scenes
/// Automatically updates when money changes
/// </summary>
public class CasinoMoneyDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("Display Settings")]
    [SerializeField] private string prefix = "Money: $";
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;

    private int lastMoney = 0;

    void Start()
    {
        if (moneyText == null)
        {
            Debug.LogError("MoneyText not assigned!");
            return;
        }

        // Subscribe to money changes
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
            // Initial display
            UpdateMoneyDisplay(MoneyManager.Instance.GetMoney());
        }
        else
        {
            Debug.LogError("MoneyManager not found!");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
        }
    }

    private void UpdateMoneyDisplay(int newMoney)
    {
        if (moneyText == null) return;

        // Update text
        moneyText.text = $"{prefix}{newMoney}";

        // Flash color if money changed (optional visual feedback)
        if (lastMoney != 0) // Skip first update
        {
            if (newMoney > lastMoney)
            {
                // Money increased - flash green
                FlashColor(positiveColor);
            }
            else if (newMoney < lastMoney)
            {
                // Money decreased - flash red
                FlashColor(negativeColor);
            }
        }

        lastMoney = newMoney;
    }

    private void FlashColor(Color flashColor)
    {
        if (moneyText == null) return;

        StopAllCoroutines();
        StartCoroutine(FlashColorCoroutine(flashColor));
    }

    private System.Collections.IEnumerator FlashColorCoroutine(Color flashColor)
    {
        Color originalColor = moneyText.color;

        // Flash to new color
        moneyText.color = flashColor;

        // Wait
        yield return new WaitForSeconds(flashDuration);

        // Fade back to original
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            moneyText.color = Color.Lerp(flashColor, originalColor, elapsed / flashDuration);
            yield return null;
        }

        moneyText.color = originalColor;
    }
}