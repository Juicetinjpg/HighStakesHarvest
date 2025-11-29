using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeManager : MonoBehaviour
{
    public Slider volumeSlider;
    public TMP_Text volumeText;

    void Start()
    {
        if (volumeSlider != null)
        {
            // Load from AudioManager's saved volume
            float savedVolume = AudioManager.Instance.GetVolume();
            volumeSlider.value = savedVolume;

            UpdateVolumeText(savedVolume);

            volumeSlider.onValueChanged.AddListener(VolumeChanged);
        }
    }

    void VolumeChanged(float value)
    {
        // Update AudioManager volume
        AudioManager.Instance.SetVolume(value);

        // Update UI text
        UpdateVolumeText(value);
    }

    private void UpdateVolumeText(float value)
    {
        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(value * 100).ToString();
        }
    }
}
