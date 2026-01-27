using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("3D Audio Settings")]
    [Range(1, 30)][SerializeField] private int sfx3dPoolSize = 10; // Number of 3D audio sources to pool

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainmenuMusic;
    [SerializeField] private AudioClip ambientMusic;

    private List<AudioSource> _sfx3dPool; // Pool of 3D audio sources for spatial sounds
    private AudioSource _sfx2dSource; // Single audio source for 2D sound effects
    private AudioSource _musicSource; // Single audio source for background music
    private bool _isMusicPaused;

    /// <summary>
    /// Singleton instance of AudioManager
    /// </summary>
    public static AudioManager Instance { get; private set; }

    /// <summary>
    /// Initialize the AudioManager singleton and create audio sources
    /// </summary>
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Detach from parent if it exists
        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);

        // Initialize 3D audio pool
        _sfx3dPool = new List<AudioSource>();
        for (int i = 0; i < sfx3dPoolSize; i++)
        {
            GameObject sfxObj = new GameObject($"3D_AudioSource_{i}");
            sfxObj.transform.parent = transform;
            AudioSource source = sfxObj.AddComponent<AudioSource>();

            source.spatialBlend = 1; // 3D Sound
            source.playOnAwake = false;
            source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];

            _sfx3dPool.Add(source);
        }

        // Create single audio source for music
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.clip = mainmenuMusic;
        _musicSource.loop = true;
        _musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
        _musicSource.spatialBlend = 0; // 2D Sound
        _musicSource.playOnAwake = true;

        // Create single audio source for 2D sound effects
        _sfx2dSource = gameObject.AddComponent<AudioSource>();
        _sfx2dSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
        _sfx2dSource.spatialBlend = 0; // 2D SFX Sound
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += UpdateMusic;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= UpdateMusic;
    }

    private void UpdateMusic(GameManager.GameState gameState)
    {
        switch (gameState)
        {
            case GameManager.GameState.Gameplay:
                if (_musicSource.clip == ambientMusic && _musicSource.isPlaying) break;
                _musicSource.clip = ambientMusic;
                _musicSource.volume = 0.1f;
                if (_isMusicPaused)
                {
                    _musicSource.UnPause();
                    _isMusicPaused = false;
                }
                else
                    _musicSource.Play();
                break;

            case GameManager.GameState.MainMenu:
                _musicSource.clip = mainmenuMusic;
                _musicSource.volume = 1f;
                _musicSource.Play();
                break;

            case GameManager.GameState.FirstDeath:
            case GameManager.GameState.Choice:
            case GameManager.GameState.Cutscene:
                _musicSource.Pause();
                _isMusicPaused = true;
                break;
        }
    }

    private void SetVolume(string parameter, float dBValue)
    {
        audioMixer.SetFloat(parameter, dBValue);
        PlayerPrefs.SetFloat(parameter, dBValue);
    }

    public void LoadVolume(string parameter, Slider slider, float defaultValue)
    {
        float saved = PlayerPrefs.GetFloat(parameter, defaultValue);
        slider.SetValueWithoutNotify(saved);
        audioMixer.SetFloat(parameter, saved);
    }

    public void SetMasterVolume(float value)
    {
        SetVolume("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        SetVolume("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        SetVolume("SFXVolume", value);
    }

    /// <summary>
    /// Plays a 3D sound effect at the given world position using a pooled AudioSource.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="position">The world position where the sound originates.</param>
    /// <param name="volume">Optional volume multiplier (default = 1).</param>
    public void Play3DSFX(AudioClip clip, Vector3 position, float minDistance = 1f, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource source = null;
        for (int i = 0; i < _sfx3dPool.Count; i++)
        {
            // Find a free 3D audio source in the pool
            if (!_sfx3dPool[i].isPlaying)
            {
                source = _sfx3dPool[i];
                break;
            }
        }

        // If all sources are busy, use the first one and stop it
        if (source == null)
        {
            source = _sfx3dPool[0];
            source.Stop();
        }

        // Set clip, position, volume, and play
        source.clip = clip;
        source.transform.position = position;
        source.volume = Mathf.Clamp01(volume);
        source.minDistance = minDistance;
        source.Play();
    }

    /// <summary>
    /// Plays a 2D sound effect using the single 2D AudioSource.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="volume">Optional volume multiplier (default = 1).</param>
    public void Play2DSFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        _sfx2dSource.volume = Mathf.Clamp01(volume);
        _sfx2dSource.PlayOneShot(clip);
    }
}
