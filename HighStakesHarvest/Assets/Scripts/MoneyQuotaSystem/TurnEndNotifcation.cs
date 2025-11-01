using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Shows "Turn Ended" notification and transitions to casino
/// </summary>
public class TurnEndNotification : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject turnEndPanel;
    [SerializeField] private TextMeshProUGUI turnEndText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private string casinoSceneName = "CasinoScene";

    void Start()
    {
        // Hide panel initially
        if (turnEndPanel != null)
            turnEndPanel.SetActive(false);

        // Subscribe to turn end event
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnEnded += ShowTurnEndNotification;
        }
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnEnded -= ShowTurnEndNotification;
        }
    }

    private void ShowTurnEndNotification()
    {
        StartCoroutine(ShowNotificationAndTransition());
    }

    private IEnumerator ShowNotificationAndTransition()
    {
        // Show panel
        if (turnEndPanel != null)
        {
            turnEndPanel.SetActive(true);

            if (turnEndText != null)
            {
                // Get turns remaining
                int turnsLeft = 0;
                if (QuotaManager.Instance != null)
                    turnsLeft = QuotaManager.Instance.GetTurnsRemaining();

                // Customize message
                turnEndText.text = $"Turn Ended!\n\n" +
                                  $"Turns Remaining: {turnsLeft}\n\n" +
                                  $"Heading to Casino...";
            }
        }

        // Wait
        yield return new WaitForSeconds(displayDuration);

        // Hide panel
        if (turnEndPanel != null)
            turnEndPanel.SetActive(false);

        // Load casino
        SceneManager.LoadScene(casinoSceneName);
    }
}