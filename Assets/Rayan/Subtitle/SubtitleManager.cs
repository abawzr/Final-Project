using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Subtitles
{
    /// <summary>
    /// The runtime controller that handles displaying subtitles.
    /// This is the only component that needs to exist in the scene.
    /// Implements a singleton pattern and persists across scene loads.
    /// </summary>
    public class SubtitleManager : MonoBehaviour
    {
        #region Singleton

        private static SubtitleManager _instance;
        private static bool _isQuitting = false;

        /// <summary>
        /// The singleton instance of the SubtitleManager.
        /// </summary>
        public static SubtitleManager Instance
        {
            get
            {
                // Reset quitting flag if we're in a new play session (editor only)
#if UNITY_EDITOR
                if (_isQuitting && !Application.isPlaying)
                {
                    _isQuitting = false;
                }
#endif

                if (_isQuitting)
                {
                    Debug.LogWarning("[SubtitleManager] Instance requested after application quit. Returning null.");
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SubtitleManager>();

                    if (_instance == null)
                    {
                        Debug.LogWarning("[SubtitleManager] No SubtitleManager found in scene. Please add one to your scene.");
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Inspector References

        [Header("Registry")]
        [Tooltip("Reference to the SubtitleRegistry asset containing all subtitle tracks")]
        [SerializeField]
        private SubtitleRegistry subtitleRegistry;

        [Header("UI References")]
        [Tooltip("The parent panel that contains everything and gets shown/hidden")]
        [SerializeField]
        private GameObject subtitlePanel;

        [Tooltip("Reference to the English TextMeshPro component")]
        [SerializeField]
        private TMP_Text englishText;

        [Tooltip("Reference to the Arabic TextMeshPro component")]
        [SerializeField]
        private TMP_Text arabicText;

        #endregion

        #region Private Fields

        private Coroutine _playbackCoroutine;
        private string _currentTrackId;
        private bool _isPlaying;

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if subtitles are currently being displayed.
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Returns the ID of the currently playing track, or null if nothing is playing.
        /// </summary>
        public string CurrentTrackId => _currentTrackId;

        /// <summary>
        /// Returns true if the UI references are valid and subtitles can be displayed.
        /// </summary>
        public bool HasValidUIReferences => subtitlePanel != null && englishText != null && arabicText != null;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Reset quitting flag when a new instance is created
            _isQuitting = false;

            // Singleton setup
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[SubtitleManager] Duplicate SubtitleManager found. Destroying this instance.");
                Destroy(gameObject);
                return;
            }

            _instance = this;

            // Validate references
            ValidateReferences();

            // Ensure panel starts hidden
            HidePanel();

            // Subscribe to scene loaded event to check for broken references
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            // Unsubscribe from scene events
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Called when a new scene is loaded. Checks if UI references are still valid.
        /// </summary>
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // Check if our UI references were destroyed (happens if UI Canvas was in the old scene)
            if (subtitlePanel == null || englishText == null || arabicText == null)
            {
                // Stop any playing subtitles since we can't display them
                if (_isPlaying)
                {
                    Debug.LogWarning("[SubtitleManager] UI references lost after scene load. Stopping subtitles.");
                    StopInternal();
                }

                Debug.LogError("[SubtitleManager] UI references are missing after scene load. " +
                    "Either make sure the subtitle UI is part of a DontDestroyOnLoad object, " +
                    "or reassign the references after scene loads.", this);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Plays the subtitle track with the specified ID.
        /// If a track is already playing, it will be stopped first.
        /// </summary>
        /// <param name="trackId">The unique identifier of the track to play</param>
        public void Play(string trackId)
        {
            // Validate trackId
            if (string.IsNullOrEmpty(trackId))
            {
                Debug.LogWarning("[SubtitleManager] Play called with null or empty trackId.");
                return;
            }

            // Validate registry
            if (subtitleRegistry == null)
            {
                Debug.LogError("[SubtitleManager] SubtitleRegistry is not assigned. Cannot play subtitles.");
                return;
            }

            // Find the track
            SubtitleTrack track = subtitleRegistry.GetTrackById(trackId);
            if (track == null)
            {
                Debug.LogWarning($"[SubtitleManager] Track '{trackId}' not found in registry. Subtitles will not play.");
                return;
            }

            // Stop any currently playing track
            if (_isPlaying)
            {
                Stop();
            }

            // Validate UI references
            if (!ValidateUIReferences())
            {
                Debug.LogError("[SubtitleManager] UI references are not properly assigned. Cannot play subtitles.");
                return;
            }

            // Start playback
            _currentTrackId = trackId;
            _isPlaying = true;
            ShowPanel();

            _playbackCoroutine = StartCoroutine(PlaybackCoroutine(track));
        }

        /// <summary>
        /// Stops the currently playing subtitle track.
        /// Can be called safely even if nothing is playing.
        /// </summary>
        public void Stop()
        {
            StopInternal();
        }

        /// <summary>
        /// Reassigns the UI references at runtime.
        /// Useful after scene loads if the UI is not marked as DontDestroyOnLoad.
        /// </summary>
        /// <param name="panel">The subtitle panel GameObject</param>
        /// <param name="english">The English TMP_Text component</param>
        /// <param name="arabic">The Arabic TMP_Text component</param>
        public void SetUIReferences(GameObject panel, TMP_Text english, TMP_Text arabic)
        {
            // Stop any current playback before changing references
            if (_isPlaying)
            {
                StopInternal();
            }

            subtitlePanel = panel;
            englishText = english;
            arabicText = arabic;

            // Validate the new references
            if (!ValidateUIReferences())
            {
                Debug.LogWarning("[SubtitleManager] SetUIReferences called with one or more null references.");
            }

            // Ensure panel starts hidden
            HidePanel();
        }

        /// <summary>
        /// Internal stop implementation. Stops playback without additional logging.
        /// </summary>
        private void StopInternal()
        {
            // Stop the coroutine if running
            if (_playbackCoroutine != null)
            {
                StopCoroutine(_playbackCoroutine);
                _playbackCoroutine = null;
            }

            // Clear text (safe even if references are null)
            ClearText();

            // Hide panel (safe even if reference is null)
            HidePanel();

            // Reset state
            _currentTrackId = null;
            _isPlaying = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Coroutine that handles the playback logic for a subtitle track.
        /// </summary>
        private IEnumerator PlaybackCoroutine(SubtitleTrack track)
        {
            List<SubtitleEntry> entries = track.Entries;

            // Handle empty track
            if (entries == null || entries.Count == 0)
            {
                Debug.LogWarning($"[SubtitleManager] Track '{track.TrackId}' has no entries.");
                StopInternal();
                yield break;
            }

            float timer = 0f;
            float trackDuration = track.TotalDuration;

            // Track which entries are currently active
            HashSet<int> activeEntries = new HashSet<int>();

            // Process the initial state at time 0 before showing panel
            // This ensures any entries starting at 0 are displayed immediately
            for (int i = 0; i < entries.Count; i++)
            {
                SubtitleEntry entry = entries[i];
                if (entry == null) continue;

                if (timer >= entry.startTime && timer < entry.endTime)
                {
                    activeEntries.Add(i);
                }
            }

            // Update text before showing panel (prevents empty panel flash)
            if (activeEntries.Count > 0)
            {
                UpdateDisplayedText(entries, activeEntries);
            }

            // Main playback loop
            while (timer <= trackDuration)
            {
                // Check each entry for state changes
                for (int i = 0; i < entries.Count; i++)
                {
                    SubtitleEntry entry = entries[i];
                    if (entry == null) continue;

                    bool shouldBeActive = timer >= entry.startTime && timer < entry.endTime;
                    bool isActive = activeEntries.Contains(i);

                    // Entry should start
                    if (shouldBeActive && !isActive)
                    {
                        activeEntries.Add(i);
                        UpdateDisplayedText(entries, activeEntries);
                    }
                    // Entry should end
                    else if (!shouldBeActive && isActive)
                    {
                        activeEntries.Remove(i);
                        UpdateDisplayedText(entries, activeEntries);
                    }
                }

                yield return null;
                timer += Time.unscaledDeltaTime;
            }

            // Playback complete
            StopInternal();
        }

        /// <summary>
        /// Updates the displayed text based on currently active entries.
        /// Handles multiple overlapping entries by concatenating their text.
        /// </summary>
        private void UpdateDisplayedText(List<SubtitleEntry> entries, HashSet<int> activeEntries)
        {
            if (activeEntries.Count == 0)
            {
                ClearText();
                return;
            }

            // For single entry, display directly
            if (activeEntries.Count == 1)
            {
                foreach (int index in activeEntries)
                {
                    SubtitleEntry entry = entries[index];
                    SetText(entry.englishText, entry.arabicText);
                    return;
                }
            }

            // For multiple overlapping entries, concatenate with line breaks
            var englishLines = new List<string>();
            var arabicLines = new List<string>();

            // Sort by index to maintain order
            var sortedIndices = new List<int>(activeEntries);
            sortedIndices.Sort();

            foreach (int index in sortedIndices)
            {
                SubtitleEntry entry = entries[index];
                if (!string.IsNullOrEmpty(entry.englishText))
                    englishLines.Add(entry.englishText);
                if (!string.IsNullOrEmpty(entry.arabicText))
                    arabicLines.Add(entry.arabicText);
            }

            string englishCombined = string.Join("\n", englishLines);
            string arabicCombined = string.Join("\n", arabicLines);

            SetText(englishCombined, arabicCombined);
        }

        /// <summary>
        /// Sets the text on both text components and forces layout rebuild.
        /// </summary>
        private void SetText(string english, string arabic)
        {
            if (englishText != null)
                englishText.text = english ?? string.Empty;

            if (arabicText != null)
                arabicText.text = arabic ?? string.Empty;

            // Force layout rebuild so ContentSizeFitter recalculates panel size
            ForceLayoutRebuild();
        }

        /// <summary>
        /// Clears the text on both text components.
        /// </summary>
        private void ClearText()
        {
            if (englishText != null)
                englishText.text = string.Empty;

            if (arabicText != null)
                arabicText.text = string.Empty;

            // Force layout rebuild
            ForceLayoutRebuild();
        }

        /// <summary>
        /// Forces the layout system to recalculate the panel size.
        /// Call this after changing text content.
        /// </summary>
        private void ForceLayoutRebuild()
        {
            if (subtitlePanel != null)
            {
                RectTransform panelRect = subtitlePanel.GetComponent<RectTransform>();
                if (panelRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
                }
            }
        }

        /// <summary>
        /// Shows the subtitle panel.
        /// </summary>
        private void ShowPanel()
        {
            if (subtitlePanel != null)
                subtitlePanel.SetActive(true);
        }

        /// <summary>
        /// Hides the subtitle panel.
        /// </summary>
        private void HidePanel()
        {
            if (subtitlePanel != null)
                subtitlePanel.SetActive(false);
        }

        /// <summary>
        /// Validates that all required references are assigned.
        /// </summary>
        private void ValidateReferences()
        {
            if (subtitleRegistry == null)
                Debug.LogError("[SubtitleManager] SubtitleRegistry is not assigned!", this);

            if (subtitlePanel == null)
                Debug.LogError("[SubtitleManager] SubtitlePanel is not assigned!", this);

            if (englishText == null)
                Debug.LogError("[SubtitleManager] EnglishText is not assigned!", this);

            if (arabicText == null)
                Debug.LogError("[SubtitleManager] ArabicText is not assigned!", this);
        }

        /// <summary>
        /// Validates UI references before playback.
        /// </summary>
        /// <returns>True if all UI references are valid</returns>
        private bool ValidateUIReferences()
        {
            return subtitlePanel != null && englishText != null && arabicText != null;
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only method to test subtitle playback.
        /// </summary>
        [ContextMenu("Validate Setup")]
        private void ValidateSetup()
        {
            ValidateReferences();

            if (subtitleRegistry != null)
            {
                subtitleRegistry.ValidateRegistry();
            }

            Debug.Log("[SubtitleManager] Setup validation complete. Check console for any warnings or errors.");
        }
#endif

        #endregion
    }
}