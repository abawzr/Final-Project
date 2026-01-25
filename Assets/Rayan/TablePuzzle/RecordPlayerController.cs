using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the record player with play/stop functionality and optional spinning disc.
/// v4 - Features:
///      - Play button spam prevention (locked while playing, unlocked when stopped)
///      - Play button stays pressed while sound plays, releases when sound ends
///      - Stop button releases play button and allows it to be pressed again
///      - DisableRecordPlayer() method to disable after puzzle is solved
///      - Subscribes to StatueRotationPuzzle.OnPuzzleSolved to auto-disable
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

    [Header("Puzzle Integration")]
    [Tooltip("If true, automatically disables record player when puzzle is solved")]
    [SerializeField] private bool disableOnPuzzleSolved = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    // Events
    public static event Action OnStoryStarted;
    public static event Action OnStoryStopped;
    public static event Action OnStoryFinished;

    // State
    private bool _isPlaying = false;
    private bool _isDisabled = false;
    private float _currentNeedleAngle;
    private Coroutine _needleCoroutine;
    private Coroutine _finishCheckCoroutine;

    // Track if we're quitting to know when to clean up static events
    private static bool _isApplicationQuitting = false;

    public bool IsPlaying => _isPlaying;
    public bool IsDisabled => _isDisabled;

    private void Awake()
    {
        _currentNeedleAngle = needleStopAngle;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void OnEnable()
    {
        if (playButton != null)
        {
            playButton.OnButtonClicked.AddListener(PlayStory);
        }

        if (stopButton != null)
        {
            stopButton.OnButtonClicked.AddListener(StopStory);
        }

        // Subscribe to puzzle solved event
        if (disableOnPuzzleSolved)
        {
            StatueRotationPuzzle.OnPuzzleSolved += OnPuzzleSolved;
        }
    }

    private void OnDisable()
    {
        if (playButton != null)
        {
            playButton.OnButtonClicked.RemoveListener(PlayStory);
        }

        if (stopButton != null)
        {
            stopButton.OnButtonClicked.RemoveListener(StopStory);
        }

        // Unsubscribe from puzzle solved event
        if (disableOnPuzzleSolved)
        {
            StatueRotationPuzzle.OnPuzzleSolved -= OnPuzzleSolved;
        }
    }

    private void OnDestroy()
    {
        // Stop coroutines
        if (_needleCoroutine != null)
        {
            StopCoroutine(_needleCoroutine);
            _needleCoroutine = null;
        }

        if (_finishCheckCoroutine != null)
        {
            StopCoroutine(_finishCheckCoroutine);
            _finishCheckCoroutine = null;
        }

        // Only clean up static events when application is quitting
        // This prevents breaking other listeners during scene transitions
        if (_isApplicationQuitting)
        {
            OnStoryStarted = null;
            OnStoryStopped = null;
            OnStoryFinished = null;
        }
    }

    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }

    private void Update()
    {
        // Spin the record disc while playing
        if (_isPlaying && recordDisc != null)
        {
            float rotationAmount = discRotationSpeed * 360f / 60f * Time.deltaTime;
            recordDisc.Rotate(discRotationAxis, rotationAmount, Space.Self);
        }
    }

    /// <summary>
    /// Called when the puzzle is solved. Disables the record player.
    /// </summary>
    private void OnPuzzleSolved()
    {
        if (enableDebugLogs) Debug.Log("[RecordPlayerController] Puzzle solved - disabling record player");
        DisableRecordPlayer();
    }

    /// <summary>
    /// Starts playing the story narration from the beginning.
    /// </summary>
    public void PlayStory()
    {
        // Don't allow playing if disabled or already playing
        if (_isDisabled || _isPlaying)
        {
            if (enableDebugLogs) Debug.Log($"[RecordPlayerController] PlayStory ignored - disabled={_isDisabled}, playing={_isPlaying}");
            return;
        }

        if (audioSource == null) return;

        audioSource.Stop();

        if (storyNarration != null)
        {
            audioSource.clip = storyNarration;
            audioSource.Play();
        }

        _isPlaying = true;

        // Lock the play button to prevent spam (it's already pressed and locked via stayPressedMode)
        // The button handles this internally when stayPressedMode is true

        // Move needle to play position
        MoveNeedle(needlePlayAngle);

        // Start coroutine to check for finish
        if (_finishCheckCoroutine != null)
        {
            StopCoroutine(_finishCheckCoroutine);
        }
        _finishCheckCoroutine = StartCoroutine(CheckForAudioFinish());

        OnStoryStarted?.Invoke();

        if (enableDebugLogs) Debug.Log("[RecordPlayerController] Story started");
    }

    /// <summary>
    /// Stops the story narration.
    /// </summary>
    public void StopStory()
    {
        // Don't allow stopping if disabled
        if (_isDisabled)
        {
            if (enableDebugLogs) Debug.Log("[RecordPlayerController] StopStory ignored - disabled");
            return;
        }

        if (audioSource == null) return;

        audioSource.Stop();
        _isPlaying = false;

        if (_finishCheckCoroutine != null)
        {
            StopCoroutine(_finishCheckCoroutine);
            _finishCheckCoroutine = null;
        }

        // Release the play button so it can be pressed again
        if (playButton != null)
        {
            playButton.Release();
        }

        MoveNeedle(needleStopAngle);

        OnStoryStopped?.Invoke();

        if (enableDebugLogs) Debug.Log("[RecordPlayerController] Story stopped");
    }

    /// <summary>
    /// Coroutine that checks when audio finishes naturally.
    /// </summary>
    private IEnumerator CheckForAudioFinish()
    {
        // Wait a moment for audio to actually start
        yield return new WaitForSeconds(0.2f);

        // Wait until audio stops playing
        while (audioSource != null && audioSource.isPlaying)
        {
            yield return null;
        }

        // Only trigger if we're still supposed to be playing
        if (_isPlaying)
        {
            OnNarrationFinished();
        }
    }

    /// <summary>
    /// Called when the narration finishes playing naturally.
    /// </summary>
    private void OnNarrationFinished()
    {
        _isPlaying = false;
        _finishCheckCoroutine = null;

        // Release the play button so it goes back up and can be pressed again
        if (playButton != null)
        {
            playButton.Release();
        }

        MoveNeedle(needleStopAngle);

        OnStoryFinished?.Invoke();

        if (enableDebugLogs) Debug.Log("[RecordPlayerController] Story finished");
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
            needleArm.localRotation = Quaternion.Euler(0f, 0f, _currentNeedleAngle);
            yield return null;
        }

        _currentNeedleAngle = targetAngle;
        needleArm.localRotation = Quaternion.Euler(0f, 0f, _currentNeedleAngle);
        _needleCoroutine = null;
    }

    #region Public Methods

    /// <summary>
    /// Completely disables the record player. No buttons can be clicked.
    /// Used when puzzle is solved.
    /// </summary>
    public void DisableRecordPlayer()
    {
        _isDisabled = true;

        // Stop any playing audio
        if (_isPlaying)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            _isPlaying = false;

            if (_finishCheckCoroutine != null)
            {
                StopCoroutine(_finishCheckCoroutine);
                _finishCheckCoroutine = null;
            }
        }

        // Disable both buttons
        if (playButton != null)
        {
            playButton.Disable();
        }

        if (stopButton != null)
        {
            stopButton.Disable();
        }

        // Move needle to stop position
        MoveNeedle(needleStopAngle);

        if (enableDebugLogs) Debug.Log("[RecordPlayerController] Record player disabled");
    }

    /// <summary>
    /// Re-enables the record player after it was disabled.
    /// </summary>
    public void EnableRecordPlayer()
    {
        _isDisabled = false;

        // Enable both buttons
        if (playButton != null)
        {
            playButton.Enable();
        }

        if (stopButton != null)
        {
            stopButton.Enable();
        }

        if (enableDebugLogs) Debug.Log("[RecordPlayerController] Record player enabled");
    }

    #endregion

    #region Public Properties

    public float GetPlaybackProgress()
    {
        if (audioSource == null || audioSource.clip == null) return 0f;
        return audioSource.time / audioSource.clip.length;
    }

    public float GetCurrentTime()
    {
        if (audioSource == null) return 0f;
        return audioSource.time;
    }

    public float GetTotalDuration()
    {
        if (storyNarration == null) return 0f;
        return storyNarration.length;
    }

    #endregion
}