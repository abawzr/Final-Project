using System;

namespace Subtitles
{
    /// <summary>
    /// A simple data container for a single subtitle line.
    /// Contains timing information and text in both English and Arabic.
    /// </summary>
    [Serializable]
    public class SubtitleEntry
    {
        /// <summary>
        /// The time in seconds when this subtitle should appear, relative to when the track starts playing.
        /// </summary>
        public float startTime;

        /// <summary>
        /// The time in seconds when this subtitle should disappear.
        /// </summary>
        public float endTime;

        /// <summary>
        /// The subtitle text in English.
        /// </summary>
        public string englishText;

        /// <summary>
        /// The subtitle text in Arabic.
        /// </summary>
        public string arabicText;

        /// <summary>
        /// Default constructor for serialization.
        /// </summary>
        public SubtitleEntry()
        {
            startTime = 0f;
            endTime = 1f;
            englishText = string.Empty;
            arabicText = string.Empty;
        }

        /// <summary>
        /// Creates a new SubtitleEntry with the specified values.
        /// </summary>
        /// <param name="startTime">When the subtitle should appear (in seconds)</param>
        /// <param name="endTime">When the subtitle should disappear (in seconds)</param>
        /// <param name="englishText">The English text to display</param>
        /// <param name="arabicText">The Arabic text to display</param>
        public SubtitleEntry(float startTime, float endTime, string englishText, string arabicText)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            this.englishText = englishText;
            this.arabicText = arabicText;
        }

        /// <summary>
        /// Returns the duration of this subtitle entry in seconds.
        /// </summary>
        public float Duration => endTime - startTime;

        /// <summary>
        /// Checks if this entry is valid (end time is after start time and has text content).
        /// </summary>
        public bool IsValid => endTime > startTime && (!string.IsNullOrEmpty(englishText) || !string.IsNullOrEmpty(arabicText));
    }
}