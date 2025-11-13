using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GoToSlotsScene : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private GameObject fadePrefab;   // Reference to fade overlay prefab
    [SerializeField] private float fadeDuration = 1f; // Duration of fade in seconds

    private CanvasGroup fadeCanvasGroup;

    void Start()
    {
        // Get the Button component attached to this GameObject
        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("GoToSlotsScene: No Button component found on this GameObject.");
        }
    }

    void OnButtonClick()
    {
        // Disable button to prevent double clicks
        GetComponent<Button>().interactable = false;

        // Create fade overlay if prefab is assigned
        if (fadePrefab != null)
        {
            // Try to find an existing Canvas
            Canvas existingCanvas = FindObjectOfType<Canvas>();

            GameObject fadeInstance;
            if (existingCanvas != null)
            {
                // Instantiate as a child of the existing canvas
                fadeInstance = Instantiate(fadePrefab, existingCanvas.transform);
            }
            else
            {
                // Create a new Canvas if none exists
                GameObject canvasObj = new GameObject("FadeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Canvas newCanvas = canvasObj.GetComponent<Canvas>();
                newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                newCanvas.sortingOrder = 999; // Make sure it's on top
                fadeInstance = Instantiate(fadePrefab, newCanvas.transform);
            }

            // Make sure the fade overlay renders on top
            fadeInstance.transform.SetAsLastSibling();

            fadeCanvasGroup = fadeInstance.GetComponent<CanvasGroup>();

            if (fadeCanvasGroup != null)
                StartCoroutine(FadeOutAndLoadScene());
            else
                SceneManager.LoadScene("Slots");
        }
        else
        {
            Debug.LogWarning("Fade prefab not assigned — loading scene directly.");
            SceneManager.LoadScene("Slots");
        }
    }

    private IEnumerator FadeOutAndLoadScene()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene("Slots");
    }
}
