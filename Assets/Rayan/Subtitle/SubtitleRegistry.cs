using System.Collections.Generic;
using UnityEngine;

namespace Subtitles
{
    /// <summary>
    /// A central registry that holds references to all SubtitleTrack assets in the project.
    /// The SubtitleManager uses this to find tracks by their ID.
    /// </summary>
    [CreateAssetMenu(fileName = "Subtitle Registry", menuName = "Subtitles/Subtitle Registry", order = 0)]
    public class SubtitleRegistry : ScriptableObject
    {
        [Tooltip("All registered subtitle tracks in the project")]
        [SerializeField]
        private List<SubtitleTrack> tracks = new List<SubtitleTrack>();

        /// <summary>
        /// Gets the list of all registered tracks.
        /// </summary>
        public List<SubtitleTrack> Tracks
        {
            get
            {
                if (tracks == null)
                {
                    tracks = new List<SubtitleTrack>();
                }
                return tracks;
            }
        }

        /// <summary>
        /// Gets the number of registered tracks.
        /// </summary>
        public int TrackCount => tracks?.Count ?? 0;

        /// <summary>
        /// Searches the registry for a track with the specified ID.
        /// </summary>
        /// <param name="id">The trackId to search for</param>
        /// <returns>The matching SubtitleTrack, or null if not found</returns>
        public SubtitleTrack GetTrackById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("[SubtitleRegistry] GetTrackById called with null or empty id.");
                return null;
            }

            if (tracks == null || tracks.Count == 0)
            {
                Debug.LogWarning($"[SubtitleRegistry] No tracks registered. Cannot find track with id '{id}'.");
                return null;
            }

            foreach (var track in tracks)
            {
                if (track != null && track.TrackId == id)
                {
                    return track;
                }
            }

            Debug.LogWarning($"[SubtitleRegistry] Track with id '{id}' not found. Make sure it's added to the registry.");
            return null;
        }

        /// <summary>
        /// Checks if a track with the specified ID exists in the registry.
        /// </summary>
        /// <param name="id">The trackId to check for</param>
        /// <returns>True if the track exists, false otherwise</returns>
        public bool HasTrack(string id)
        {
            if (string.IsNullOrEmpty(id) || tracks == null)
                return false;

            foreach (var track in tracks)
            {
                if (track != null && track.TrackId == id)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all track IDs registered in this registry.
        /// Useful for debugging or creating dropdown menus.
        /// </summary>
        /// <returns>A list of all track IDs</returns>
        public List<string> GetAllTrackIds()
        {
            var ids = new List<string>();

            if (tracks == null)
                return ids;

            foreach (var track in tracks)
            {
                if (track != null && !string.IsNullOrEmpty(track.TrackId))
                {
                    ids.Add(track.TrackId);
                }
            }

            return ids;
        }

        /// <summary>
        /// Validates the registry and logs any issues.
        /// Useful for debugging in the editor.
        /// </summary>
        public void ValidateRegistry()
        {
            if (tracks == null || tracks.Count == 0)
            {
                Debug.LogWarning("[SubtitleRegistry] Registry is empty. No tracks registered.", this);
                return;
            }

            var seenIds = new HashSet<string>();

            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];

                if (track == null)
                {
                    Debug.LogWarning($"[SubtitleRegistry] Null track reference at index {i}.", this);
                    continue;
                }

                if (string.IsNullOrEmpty(track.TrackId))
                {
                    Debug.LogWarning($"[SubtitleRegistry] Track at index {i} ('{track.name}') has no trackId.", this);
                    continue;
                }

                if (seenIds.Contains(track.TrackId))
                {
                    Debug.LogWarning($"[SubtitleRegistry] Duplicate trackId '{track.TrackId}' found. Each track should have a unique ID.", this);
                }
                else
                {
                    seenIds.Add(track.TrackId);
                }
            }

            Debug.Log($"[SubtitleRegistry] Validation complete. {tracks.Count} tracks registered, {seenIds.Count} unique IDs.", this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Remove null entries in editor
            if (tracks != null)
            {
                tracks.RemoveAll(t => t == null);
            }
        }
#endif
    }
}