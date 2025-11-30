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

    private TurnManager subscribedTurnManager;
    private Coroutine activeCoroutine;

    void Start()
    {
        // Hide panel initially
        if (turnEndPanel != null)
            turnEndPanel.SetActive(false);

        // Subscribe to turn end event
        if (TurnManager.Instance != null)
        {
            subscribedTurnManager = TurnManager.Instance;
            subscribedTurnManager.OnTurnEnded += ShowTurnEndNotification;
            subscribedTurnManager.OnTurnStarted += HideTurnEndPanel;
        }
    }

    void OnDestroy()
    {
        if (subscribedTurnManager != null)
        {
            subscribedTurnManager.OnTurnEnded -= ShowTurnEndNotification;
            subscribedTurnManager.OnTurnStarted -= HideTurnEndPanel;
            subscribedTurnManager = null;
        }
    }

    private void ShowTurnEndNotification()
    {
        // Ensure any previous coroutine is stopped
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        activeCoroutine = StartCoroutine(ShowNotificationAndTransition());
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
        yield return new WaitForSecondsRealtime(displayDuration);

        // Hide panel
        if (turnEndPanel != null)
            turnEndPanel.SetActive(false);

        activeCoroutine = null;

        // Load casino
        SceneManager.LoadScene(casinoSceneName);
    }

    private void HideTurnEndPanel()
    {
        // Stop any running notification coroutine
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        if (turnEndPanel != null && turnEndPanel.activeSelf)
            turnEndPanel.SetActive(false);
    }
}
