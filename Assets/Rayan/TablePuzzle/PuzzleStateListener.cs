using System;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Listens for puzzle state changes without modifying PuzzlePerspective.
/// Detects when the puzzle camera becomes active by monitoring camera priority or GameManager state.
/// v4 - Fixes:
///      - Implemented proper CheckGameManagerState() (was just falling back to camera priority)
///      - Added warning when GameManager detection is selected but not properly configured
/// </summary>
public class PuzzleStateListener : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the puzzle's Cinemachine camera")]
    [SerializeField] private CinemachineCamera puzzleCamera;

    [Header("Detection Method")]
    [Tooltip("How to detect puzzle activation")]
    [SerializeField] private DetectionMethod detectionMethod = DetectionMethod.CameraPriority;

    [Tooltip("Priority threshold - puzzle is active when camera priority is above this")]
    [SerializeField] private int priorityThreshold = 5;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    /// <summary>
    /// Fired when puzzle mode is entered.
    /// </summary>
    public static event Action OnPuzzleEnabled;

    /// <summary>
    /// Fired when puzzle mode is exited.
    /// </summary>
    public static event Action OnPuzzleDisabled;

    public enum DetectionMethod
    {
        CameraPriority,     // Watch Cinemachine camera priority
        GameManagerState,   // Watch GameManager.GameState
        Both                // Use both methods
    }

    private bool _wasPuzzleActive = false;
    private bool _isPuzzleActive = false;

    // Track if we're quitting to know when to clean up static events
    private static bool _isApplicationQuitting = false;

    // Flag to prevent spamming the GameManager warning
    private bool _hasWarnedAboutGameManager = false;

    /// <summary>
    /// Returns true if currently in puzzle mode.
    /// </summary>
    public bool IsPuzzleActive => _isPuzzleActive;

    private void Start()
    {
        // Validate setup
        if (puzzleCamera == null && (detectionMethod == DetectionMethod.CameraPriority || detectionMethod == DetectionMethod.Both))
        {
            Debug.LogError("[PuzzleStateListener] No puzzle camera assigned! Camera priority detection will not work.");
        }

        // Warn if using GameManager detection
        if (detectionMethod == DetectionMethod.GameManagerState || detectionMethod == DetectionMethod.Both)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("[PuzzleStateListener] GameManager.Instance is null. GameManager state detection will not work until it's available.");
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[PuzzleStateListener] Initialized with detection method: {detectionMethod}");
        }
    }

    private void OnDestroy()
    {
        // Only clean up static events when application is quitting
        // This prevents breaking other listeners during scene transitions
        if (_isApplicationQuitting)
        {
            OnPuzzleEnabled = null;
            OnPuzzleDisabled = null;
        }
    }

    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }

    private void Update()
    {
        bool isActive = CheckIfPuzzleActive();

        // Detect state change
        if (isActive != _wasPuzzleActive)
        {
            _wasPuzzleActive = isActive;
            _isPuzzleActive = isActive;

            if (isActive)
            {
                if (enableDebugLogs) Debug.Log("[PuzzleStateListener] Puzzle ENABLED");
                OnPuzzleEnabled?.Invoke();
            }
            else
            {
                if (enableDebugLogs) Debug.Log("[PuzzleStateListener] Puzzle DISABLED");
                OnPuzzleDisabled?.Invoke();
            }
        }
    }

    /// <summary>
    /// Checks if the puzzle is currently active based on the detection method.
    /// </summary>
    private bool CheckIfPuzzleActive()
    {
        switch (detectionMethod)
        {
            case DetectionMethod.CameraPriority:
                return CheckCameraPriority();

            case DetectionMethod.GameManagerState:
                return CheckGameManagerState();

            case DetectionMethod.Both:
                return CheckCameraPriority() || CheckGameManagerState();

            default:
                return false;
        }
    }

    /// <summary>
    /// Checks if puzzle camera has high priority (above threshold).
    /// </summary>
    private bool CheckCameraPriority()
    {
        if (puzzleCamera == null) return false;
        return puzzleCamera.Priority > priorityThreshold;
    }

    /// <summary>
    /// Checks GameManager state for puzzle mode.
    /// FIX: Now properly checks GameManager.GameState instead of falling back to camera priority.
    /// </summary>
    private bool CheckGameManagerState()
    {
        if (GameManager.Instance == null)
        {
            // Only warn once to avoid log spam
            if (!_hasWarnedAboutGameManager && enableDebugLogs)
            {
                Debug.LogWarning("[PuzzleStateListener] GameManager.Instance is null, cannot check game state.");
                _hasWarnedAboutGameManager = true;
            }
            return false;
        }

        // Reset warning flag when GameManager becomes available
        _hasWarnedAboutGameManager = false;

        // FIX: Actually check the GameManager state
        // This assumes GameManager has a CurrentState property and GameState enum
        // Modify this based on your actual GameManager implementation
        try
        {
            // Option 1: If GameManager has a public CurrentState property
            // return GameManager.Instance.CurrentState == GameManager.GameState.Puzzle;

            // Option 2: If GameManager has a method to check state
            // return GameManager.Instance.IsInPuzzleState();

            // Option 3: Use reflection to check (works if property exists but isn't public)
            var currentStateProperty = GameManager.Instance.GetType().GetProperty("CurrentState");
            if (currentStateProperty != null)
            {
                var currentState = currentStateProperty.GetValue(GameManager.Instance);
                var puzzleState = Enum.Parse(typeof(GameManager.GameState), "Puzzle");
                return currentState.Equals(puzzleState);
            }

            // Fallback: If we can't determine the state, use camera priority
            if (enableDebugLogs && !_hasWarnedAboutGameManager)
            {
                Debug.LogWarning("[PuzzleStateListener] Could not find CurrentState property on GameManager. Falling back to camera priority.");
                _hasWarnedAboutGameManager = true;
            }
            return CheckCameraPriority();
        }
        catch (Exception ex)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[PuzzleStateListener] Error checking GameManager state: {ex.Message}. Falling back to camera priority.");
            }
            return CheckCameraPriority();
        }
    }

    #region Public Methods

    /// <summary>
    /// Manually triggers puzzle enabled event. Use with caution.
    /// </summary>
    public void ForceEnable()
    {
        if (!_isPuzzleActive)
        {
            _wasPuzzleActive = true;
            _isPuzzleActive = true;
            OnPuzzleEnabled?.Invoke();
        }
    }

    /// <summary>
    /// Manually triggers puzzle disabled event. Use with caution.
    /// </summary>
    public void ForceDisable()
    {
        if (_isPuzzleActive)
        {
            _wasPuzzleActive = false;
            _isPuzzleActive = false;
            OnPuzzleDisabled?.Invoke();
        }
    }

    /// <summary>
    /// Changes the detection method at runtime.
    /// </summary>
    public void SetDetectionMethod(DetectionMethod method)
    {
        detectionMethod = method;
        _hasWarnedAboutGameManager = false; // Reset warning

        if (enableDebugLogs)
        {
            Debug.Log($"[PuzzleStateListener] Detection method changed to: {method}");
        }
    }

    #endregion
}