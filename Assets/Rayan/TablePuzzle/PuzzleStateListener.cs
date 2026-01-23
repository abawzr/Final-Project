using System;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Listens for puzzle state changes without modifying PuzzlePerspective.
/// Detects when the puzzle camera becomes active by monitoring camera priority or GameManager state.
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

    /// <summary>
    /// Returns true if currently in puzzle mode.
    /// </summary>
    public bool IsPuzzleActive => _isPuzzleActive;

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
                OnPuzzleEnabled?.Invoke();
                Debug.Log("PuzzleStateListener: Puzzle enabled");
            }
            else
            {
                OnPuzzleDisabled?.Invoke();
                Debug.Log("PuzzleStateListener: Puzzle disabled");
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
    /// Checks if puzzle camera has high priority.
    /// </summary>
    private bool CheckCameraPriority()
    {
        if (puzzleCamera == null) return false;
        return puzzleCamera.Priority > priorityThreshold;
    }

    /// <summary>
    /// Checks GameManager state.
    /// NOTE: Uncomment and modify this if your GameManager has a CurrentState property.
    /// By default, this returns false to avoid compile errors.
    /// </summary>
    private bool CheckGameManagerState()
    {
        if (GameManager.Instance == null) return false;

        // UNCOMMENT AND MODIFY this line based on your GameManager implementation:
        // return GameManager.Instance.CurrentState == GameManager.GameState.Puzzle;

        // Default: Use CameraPriority method instead
        Debug.LogWarning("PuzzleStateListener: GameManagerState detection not implemented. Use CameraPriority method instead.");
        return false;
    }
}