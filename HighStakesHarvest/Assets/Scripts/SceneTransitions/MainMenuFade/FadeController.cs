using UnityEngine;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public float fadeDuration = 1.5f;
    private CanvasGroup cg;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void BeginFade()   // <-- MANUALLY triggered
    {
        cg.alpha = 1f;

        Debug.Log("Fade started");
        StopAllCoroutines();
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        Debug.Log("Fade coroutine started");

        float t = 0f;

        while (t < fadeDuration)
        {
            float dt = Time.unscaledDeltaTime;
            Debug.Log("Loop running. t = " + t + "  delta = " + dt);

            t += dt;
            cg.alpha = 1 - (t / fadeDuration);

            yield return null;
        }

        cg.alpha = 0f;
        Debug.Log("Fade coroutine finished");
    }


}
