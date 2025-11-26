using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioSource musicSource;

    private void Start()
    {
        // Load previous volume or default to 0.5
        float volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        musicSource.volume = volume;
        volumeSlider.value = volume;

        // Update text if you show numeric value
        UpdateVolumeText(volume);
    }

    public void OnVolumeChanged(float value)
    {
        musicSource.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
        UpdateVolumeText(value);
    }

    void UpdateVolumeText(float value)
    {
        var text = volumeSlider.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
            text.text = Mathf.RoundToInt(value * 100).ToString();
    }
}
