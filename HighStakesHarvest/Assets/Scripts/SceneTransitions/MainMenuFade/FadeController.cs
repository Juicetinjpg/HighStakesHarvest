using UnityEngine;

public class FadeController : MonoBehaviour
{
    public float fadeDuration = 1.5f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
    }

    private void Start()
    {
        StartCoroutine(FadeInRoutine());
    }

    private System.Collections.IEnumerator FadeInRoutine()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;

        // Remove panel after fade
        Destroy(gameObject);
    }
}
