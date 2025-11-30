using UnityEngine;
using UnityEngine.UI;

public class FullscreenToggle : MonoBehaviour
{
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        if (fullscreenToggle == null)
            fullscreenToggle = GetComponent<Toggle>();

        // Load saved data
        bool saved = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenToggle.isOn = saved;
        Screen.fullScreen = saved;

        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

}
