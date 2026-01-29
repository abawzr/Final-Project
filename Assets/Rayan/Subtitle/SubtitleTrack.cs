using System.Collections.Generic;
using UnityEngine;

namespace Subtitles
{
    /// <summary>
    /// Represents one complete subtitle sequence.
    /// One SubtitleTrack corresponds to one audio clip or dialogue sequence.
    /// This ScriptableObject has no direct reference to audio - timing is managed manually.
    /// </summary>
    [CreateAssetMenu(fileName = "New Subtitle Track", menuName = "Subtitles/Subtitle Track", order = 1)]
    public class SubtitleTrack : ScriptableObject
    {
        [Tooltip("A unique identifier used to play this track (e.g., 'intro_monologue', 'guard_dialogue_01')")]
        [SerializeField]
        private string trackId;

        [Tooltip("An ordered list of subtitle entries with their timing")]
        [SerializeField]
        private List<SubtitleEntry> entries = new List<SubtitleEntry>();

        /// <summary>
        /// The unique identifier for this track.
        /// </summary>
        public string TrackId => trackId;

        /// <summary>
        /// The list of subtitle entries in this track.
        /// Returns the actual list (not a copy) for performance.
        /// </summary>
        public List<SubtitleEntry> Entries
        {
            get
            {
                // Ensure list is never null
                if (entries == null)
                {
                    entries = new List<SubtitleEntry>();
                }
                return entries;
            }
        }

        /// <summary>
        /// Returns the total duration of this track (end time of the last entry).
        /// Returns 0 if there are no entries.
        /// </summary>
        public float TotalDuration
        {
            get
            {
                if (entries == null || entries.Count == 0)
                    return 0f;

                float maxEndTime = 0f;
                foreach (var entry in entries)
                {
                    if (entry != null && entry.endTime > maxEndTime)
                        maxEndTime = entry.endTime;
                }
                return maxEndTime;
            }
        }

        /// <summary>
        /// Returns the number of subtitle entries in this track.
        /// </summary>
        public int EntryCount => entries?.Count ?? 0;

        /// <summary>
        /// Gets the subtitle entry at the specified index.
        /// Returns null if the index is out of range.
        /// </summary>
        /// <param name="index">The index of the entry to retrieve</param>
        /// <returns>The SubtitleEntry at the specified index, or null if out of range</returns>
        public SubtitleEntry GetEntry(int index)
        {
            if (entries == null || index < 0 || index >= entries.Count)
                return null;
            return entries[index];
        }

        /// <summary>
        /// Validates all entries in this track and logs any issues.
        /// Useful for debugging in the editor.
        /// </summary>
        public void ValidateEntries()
        {
            if (string.IsNullOrEmpty(trackId))
            {
                Debug.LogWarning($"SubtitleTrack '{name}' has no trackId assigned.", this);
            }

            if (entries == null || entries.Count == 0)
            {
                Debug.LogWarning($"SubtitleTrack '{trackId}' has no entries.", this);
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null)
                {
                    Debug.LogWarning($"SubtitleTrack '{trackId}': Entry at index {i} is null.", this);
                    continue;
                }

                if (entry.endTime <= entry.startTime)
                {
                    Debug.LogWarning($"SubtitleTrack '{trackId}': Entry at index {i} has invalid timing (endTime <= startTime).", this);
                }

                if (string.IsNullOrEmpty(entry.englishText) && string.IsNullOrEmpty(entry.arabicText))
                {
                    Debug.LogWarning($"SubtitleTrack '{trackId}': Entry at index {i} has no text in either language.", this);
                }

                if (entry.startTime < 0)
                {
                    Debug.LogWarning($"SubtitleTrack '{trackId}': Entry at index {i} has negative start time.", this);
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-sort entries by start time in editor when values change
            if (entries != null && entries.Count > 1)
            {
                // Use a null-safe sort
                entries.Sort((a, b) =>
                {
                    // Handle null entries safely
                    if (a == null && b == null) return 0;
                    if (a == null) return 1;  // Push nulls to end
                    if (b == null) return -1;
                    return a.startTime.CompareTo(b.startTime);
                });
            }
        }
#endif
    }
}