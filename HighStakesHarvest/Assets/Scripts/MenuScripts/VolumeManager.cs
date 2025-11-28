using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeManager : MonoBehaviour
{
    public Slider volumeSlider;   // Drag your slider here
    public TMP_Text volumeText;   // Drag the TMP text here

    void Start()
    {
        if (volumeSlider != null)
        {
            // Initialize the slider to current volume
            volumeSlider.value = AudioListener.volume;

            // Update the text immediately
            UpdateVolumeText(volumeSlider.value);

            // Add a listener to update volume and text when slider changes
            volumeSlider.onValueChanged.AddListener(VolumeChanged);
        }
    }

    void VolumeChanged(float value)
    {
        AudioListener.volume = value;
        UpdateVolumeText(value);
    }

    private void UpdateVolumeText(float value)
    {
        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(value * 100).ToString(); // 0-100
        }
    }
}
