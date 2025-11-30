using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;   // ← IMPORTANT

public class CutscenePlayer : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public float[] pauseTimes; // Time (in seconds) where video should pause

    [Header("Dialogue")]
    public TextMeshProUGUI dialogueText;
    [TextArea(3, 5)]
    public string[] dialogueLines;

    [Header("UI")]
    public GameObject clickCatcher; // panel that detects click to continue

    private int index = 0;
    private bool waitingForClick = false;

    void Start()
    {
        dialogueText.text = "";
        clickCatcher.SetActive(false);

        videoPlayer.Play();
        StartCoroutine(CutsceneRoutine());
    }

    IEnumerator CutsceneRoutine()
    {
        while (index < pauseTimes.Length && index < dialogueLines.Length)
        {
            // Wait until video reaches pause time
            yield return new WaitUntil(() => videoPlayer.time >= pauseTimes[index]);

            // Pause video
            videoPlayer.Pause();
            dialogueText.text = dialogueLines[index];
            clickCatcher.SetActive(true);

            // Wait for user click
            waitingForClick = true;
            yield return new WaitUntil(() => waitingForClick == false);

            // Hide dialogue UI
            dialogueText.text = "";
            clickCatcher.SetActive(false);

            // Resume video
            videoPlayer.Play();

            index++;
        }

        SceneManager.LoadScene("FarmScene");

    }

    public void OnClickContinue()
    {
        waitingForClick = false;
    }
}
