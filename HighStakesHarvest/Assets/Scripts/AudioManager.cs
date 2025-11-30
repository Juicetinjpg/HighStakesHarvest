using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[System.Serializable]
public class AudioSettingsData
{
    public float musicVolume = 0.5f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Assign AudioSource (required)")]
    [Tooltip("Drag the AudioSource component here (on this GameObject).")]
    public AudioSource musicSource;

    [Header("Background Music Clips (assign your .wav/.ogg assets)")]
    public AudioClip mainMenuMusic;
    public AudioClip farmSceneMusic;
    public AudioClip casinoSceneMusic;
    public AudioClip slotsMusic;
    public AudioClip casinoTableMusic;
    public AudioClip cutsceneMusic;

    [Header("Options")]
    [Tooltip("Fade time when switching tracks. Set to 0 for instant swap.")]
    public float crossfadeTime = 0.5f;

    private string settingsPath;
    private Coroutine fadeCoroutine;
    private float savedVolume = 0.5f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // If AudioSource wasn't assigned in inspector, try to get one from this GameObject
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        if (musicSource == null)
            Debug.LogWarning("AudioManager: No AudioSource found. Add one to the GameObject or assign in inspector.");

        musicSource.loop = true;

        settingsPath = Path.Combine(Application.persistentDataPath, "audioSettings.json");
        LoadAudioSettings();

        // Ensure initial scene's music plays when the game starts
        PlayMusicForScene(SceneManager.GetActiveScene().name);

        // React to subsequent scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip clip = null;

        switch (sceneName)
        {
            case "MainMenu":
                clip = mainMenuMusic;
                break;
            case "FarmScene":
                clip = farmSceneMusic;
                break;
            case "CasinoScene":
                clip = casinoSceneMusic;
                break;
            case "Slots":
                clip = slotsMusic;
                break;
            case "CasinoTable":
                clip = casinoTableMusic;
                break;
            case "Cutscene":
                clip = cutsceneMusic;
                break;
            default:
                return;
        }

        if (clip == null || musicSource == null) return;

        if (musicSource.clip == clip) return; // already playing

        if (crossfadeTime <= 0f)
        {
            musicSource.clip = clip;
            musicSource.Play();
            musicSource.volume = savedVolume;
        }
        else
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(CrossfadeCoroutine(clip, crossfadeTime));
        }
    }

    private System.Collections.IEnumerator CrossfadeCoroutine(AudioClip newClip, float fadeTime)
    {
        float startVol = musicSource.volume;
        float t = 0f;

        // Fade out
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, startVol, t / fadeTime);
            yield return null;
        }

        musicSource.volume = startVol;
        fadeCoroutine = null;
    }

    // -------- Volume settings (JSON) --------
    private void LoadAudioSettings()
    {
        if (File.Exists(settingsPath))
        {
            string json = File.ReadAllText(settingsPath);
            try
            {
                AudioSettingsData data = JsonUtility.FromJson<AudioSettingsData>(json);
                savedVolume = Mathf.Clamp01(data.musicVolume);
            }
            catch
            {
                savedVolume = 0.5f;
            }
        }
        else
        {
            savedVolume = 0.5f;
            SaveAudioSettings();
        }

        if (musicSource != null)
            musicSource.volume = savedVolume;
    }

    public void SetVolume(float value)
    {
        savedVolume = Mathf.Clamp01(value);
        if (musicSource != null) musicSource.volume = savedVolume;
        SaveAudioSettings();
    }

    public float GetVolume()
    {
        return savedVolume;
    }

    private void SaveAudioSettings()
    {
        AudioSettingsData data = new AudioSettingsData();
        data.musicVolume = savedVolume;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(settingsPath, json);
    }
}
