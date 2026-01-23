using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the record player with play/stop functionality and optional spinning disc.
/// </summary>
public class RecordPlayerController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private RecordPlayerButton playButton;
    [SerializeField] private RecordPlayerButton stopButton;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip storyNarration;

    [Header("Visual - Record Disc")]
    [SerializeField] private Transform recordDisc;
    [SerializeField] private float discRotationSpeed = 33f; // RPM
    [SerializeField] private Vector3 discRotationAxis = Vector3.up;

    [Header("Needle Arm (Optional)")]
    [SerializeField] private Transform needleArm;
    [SerializeField] private float needlePlayAngle = -15f;
    [SerializeField] private float needleStopAngle = 0f;
    [SerializeField] private float needleRotationSpeed = 2f;

    // Events
    public static event Action OnStoryStarted;
    public static event Action OnStoryStopped;
    public static event Action OnStoryFinished;

    // State
    private bool _isPlaying = false;
    private float _currentNeedleAngle;
    private Coroutine _needleCoroutine;
    private float _playStartTime; // Track when playback started to prevent false finish detection

    public bool IsPlaying => _isPlaying;

    private void Awake()
    {
        _currentNeedleAngle = needleStopAngle;

        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Configure audio source
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void OnEnable()
    {
        // Subscribe to button events
        if (playButton != null)
        {
            playButton.OnButtonClicked.AddListener(PlayStory);
        }

        if (stopButton != null)
        {
            stopButton.OnButtonClicked.AddListener(StopStory);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from button events
        if (playButton != null)
        {
            playButton.OnButtonClicked.RemoveListener(PlayStory);
        }

        if (stopButton != null)
        {
            stopButton.OnButtonClicked.RemoveListener(StopStory);
        }
    }

    private void Update()
    {
        // Spin the record disc while playing
        if (_isPlaying && recordDisc != null)
        {
            float rotationAmount = discRotationSpeed * 360f / 60f * Time.deltaTime; // Convert RPM to degrees per second
            recordDisc.Rotate(discRotationAxis, rotationAmount, Space.Self);
        }

        // Check if narration finished naturally
        // Add time check to prevent false positive during audio startup
        if (_isPlaying && audioSource != null && !audioSource.isPlaying && storyNarration != null)
        {
            // Only trigger finish if we've been playing for at least 0.5 seconds
            // This prevents false detection during audio initialization
            if (Time.time - _playStartTime > 0.5f)
            {
                OnNarrationFinished();
            }
        }
    }

    /// <summary>
    /// Starts playing the story narration from the beginning.
    /// </summary>
    public void PlayStory()
    {
        if (audioSource == null) return;

        // Always restart from beginning
        audioSource.Stop();

        if (storyNarration != null)
        {
            audioSource.clip = storyNarration;
            audioSource.Play();
        }

        _isPlaying = true;
        _playStartTime = Time.time; // Track when we started playing

        // Move needle to play position
        MoveNeedle(needlePlayAngle);

        // Fire event
        OnStoryStarted?.Invoke();

        Debug.Log("Record Player: Story started");
    }

    /// <summary>
    /// Stops the story narration.
    /// </summary>
    public void StopStory()
    {
        if (audioSource == null) return;

        audioSource.Stop();
        _isPlaying = false;

        // Move needle back to stop position
        MoveNeedle(needleStopAngle);

        // Fire event
        OnStoryStopped?.Invoke();

        Debug.Log("Record Player: Story stopped");
    }

    /// <summary>
    /// Called when the narration finishes playing naturally.
    /// </summary>
    private void OnNarrationFinished()
    {
        _isPlaying = false;

        // Move needle back to stop position
        MoveNeedle(needleStopAngle);

        // Fire event
        OnStoryFinished?.Invoke();

        Debug.Log("Record Player: Story finished");
    }

    /// <summary>
    /// Animates the needle arm to the target angle.
    /// </summary>
    private void MoveNeedle(float targetAngle)
    {
        if (needleArm == null) return;

        if (_needleCoroutine != null)
        {
            StopCoroutine(_needleCoroutine);
        }

        _needleCoroutine = StartCoroutine(MoveNeedleCoroutine(targetAngle));
    }

    /// <summary>
    /// Coroutine for smooth needle movement.
    /// </summary>
    private IEnumerator MoveNeedleCoroutine(float targetAngle)
    {
        while (Mathf.Abs(_currentNeedleAngle - targetAngle) > 0.1f)
        {
            _currentNeedleAngle = Mathf.Lerp(_currentNeedleAngle, targetAngle, Time.deltaTime * needleRotationSpeed);

            // Apply rotation - assuming Z axis rotation for needle arm
            needleArm.localRotation = Quaternion.Euler(0f, 0f, _currentNeedleAngle);

            yield return null;
        }

        _currentNeedleAngle = targetAngle;
        needleArm.localRotation = Quaternion.Euler(0f, 0f, _currentNeedleAngle);
    }

    /// <summary>
    /// Gets the current playback progress (0-1).
    /// </summary>
    public float GetPlaybackProgress()
    {
        if (audioSource == null || audioSource.clip == null) return 0f;
        return audioSource.time / audioSource.clip.length;
    }

    /// <summary>
    /// Gets the current playback time in seconds.
    /// </summary>
    public float GetCurrentTime()
    {
        if (audioSource == null) return 0f;
        return audioSource.time;
    }

    /// <summary>
    /// Gets the total duration of the story in seconds.
    /// </summary>
    public float GetTotalDuration()
    {
        if (storyNarration == null) return 0f;
        return storyNarration.length;
    }
}