using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Shows "Turn Ended" notification using a prefab and transitions to casino
/// </summary>
public class TurnEndNotification : MonoBehaviour
{
    [Header("Prefab Reference")]
    [SerializeField] private GameObject turnEndPanelPrefab;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private string casinoSceneName = "CasinoScene";

    private GameObject turnEndPanelInstance;
    private TextMeshProUGUI turnEndText;
    private TurnManager subscribedTurnManager;
    private Coroutine activeCoroutine;

    void Start()
    {
        // Subscribe to events
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
        }
    }

    private void InstantiatePanelIfNeeded()
    {
        if (turnEndPanelInstance != null)
            return;

        // Find a canvas in the scene
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("TurnEndNotification: No Canvas found in the scene!");
            return;
        }

        // Instantiate panel under the canvas
        turnEndPanelInstance = Instantiate(turnEndPanelPrefab, canvas.transform);
        turnEndPanelInstance.SetActive(false);

        // Grab the text component automatically
        turnEndText = turnEndPanelInstance.GetComponentInChildren<TextMeshProUGUI>(true);
        if (turnEndText == null)
            Debug.LogError("TurnEndNotification: No TextMeshProUGUI found inside the TurnEndPanel prefab!");
    }

    private void ShowTurnEndNotification()
    {
        InstantiatePanelIfNeeded();

        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(ShowNotificationAndTransition());
    }

    private IEnumerator ShowNotificationAndTransition()
    {
        if (turnEndPanelInstance == null)
            yield break;

        turnEndPanelInstance.SetActive(true);

        if (turnEndText != null)
        {
            int turnsLeft = 0;
            if (QuotaManager.Instance != null)
                turnsLeft = QuotaManager.Instance.GetTurnsRemaining();

            turnEndText.text =
                $"Turn Ended!\n\n" +
                $"Turns Remaining: {turnsLeft}\n\n" +
                $"Heading to Casino...";
        }

        yield return new WaitForSeconds(displayDuration);

        turnEndPanelInstance.SetActive(false);
        activeCoroutine = null;

        //SceneManager.LoadScene(casinoSceneName);
    }

    private void HideTurnEndPanel()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        if (turnEndPanelInstance != null)
            turnEndPanelInstance.SetActive(false);
    }
}
