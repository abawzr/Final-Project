using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles individual statue rotation, hover detection, and visual feedback.
/// v10 - Fixes:
///       - IsAtCorrectRotation() now checks _isPlaced (CRITICAL: prevents premature win detection)
///       - Added OnApplicationQuit cleanup for OnAnyStatueRotated static event
///       - OnHoverEnter() now returns bool to indicate success
///       - Improved debug logging consistency
/// </summary>
[RequireComponent(typeof(Collider))]
public class RotatableStatue : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationStep = 45f;
    [SerializeField] private float rotationDuration = 0.2f;

    [Header("Visual Feedback")]
    [SerializeField] private Outline outlineComponent;
    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float glowWidth = 5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rotateSound;

    [Header("Arrow UI")]
    [SerializeField] private StatueArrowsUI arrowsUI;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    // Static event for puzzle controller
    public static event Action OnAnyStatueRotated;

    // Instance event
    public event Action<float> OnRotationChanged;

    // Configuration
    private string _configName = "Uninitialized";
    private float _correctRotation;

    // State
    private float _currentRotation = 0f;
    private bool _isRotating = false;
    private bool _isHovered = false;
    private bool _isInteractable = true;
    private bool _isPlaced = false;
    private bool _isSubscribedToArrows = false;
    private bool _isInitialized = false;

    // Arrow management - store the arrows' initial world rotation
    private Quaternion _arrowsInitialWorldRotation;
    private bool _arrowsRotationInitialized = false;

    // Track if we're quitting to know when to clean up static events
    private static bool _isApplicationQuitting = false;

    // Public properties
    public string ConfigName => _configName;
    public float CurrentRotation => _currentRotation;
    public bool IsPlaced => _isPlaced;
    public bool IsHovered => _isHovered;
    public bool IsInteractable => _isInteractable;
    public StatueArrowsUI ArrowsUI => arrowsUI;

    #region Unity Lifecycle

    private void Awake()
    {
        if (enableDebugLogs) Debug.Log($"[RotatableStatue] {name}: Awake");
        InitializeComponents();
    }

    private void Start()
    {
        if (enableDebugLogs) Debug.Log($"[RotatableStatue] {name}: Start - ArrowsUI: {arrowsUI != null}");

        // Ensure initialization is complete
        if (!_isInitialized)
        {
            InitializeComponents();
        }

        // Store the initial world rotation of arrows
        CaptureArrowsInitialRotation();
    }

    private void OnEnable()
    {
        SubscribeToArrowEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromArrowEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromArrowEvents();
        OnRotationChanged = null;

        // FIX: Clean up static event when application is quitting
        if (_isApplicationQuitting)
        {
            OnAnyStatueRotated = null;
        }
    }

    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }

    private void LateUpdate()
    {
        // Counter-rotate arrows to keep them facing the original direction
        // This runs every frame to ensure arrows stay stationary even as statue rotates
        if (_arrowsRotationInitialized && arrowsUI != null)
        {
            arrowsUI.transform.rotation = _arrowsInitialWorldRotation;
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Finds and initializes all required components.
    /// </summary>
    private void InitializeComponents()
    {
        if (_isInitialized) return;

        // Find outline component
        if (outlineComponent == null)
        {
            outlineComponent = GetComponent<Outline>();
            if (outlineComponent == null)
            {
                outlineComponent = GetComponentInChildren<Outline>(true);
            }
        }

        // Find audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = GetComponentInChildren<AudioSource>(true);
            }
        }

        // Find arrows UI
        FindArrowsUI();

        // Disable outline initially
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }

        // Hide arrows initially
        if (arrowsUI != null)
        {
            arrowsUI.Hide();
        }

        _isInitialized = true;
    }

    /// <summary>
    /// Finds the StatueArrowsUI component in children.
    /// </summary>
    private void FindArrowsUI()
    {
        if (arrowsUI != null) return;

        // Try direct children first
        arrowsUI = GetComponentInChildren<StatueArrowsUI>(true);

        // Try by name if not found
        if (arrowsUI == null)
        {
            Transform arrowsCanvas = transform.Find("ArrowsCanvas");
            if (arrowsCanvas != null)
            {
                arrowsUI = arrowsCanvas.GetComponent<StatueArrowsUI>();
            }
        }

        if (arrowsUI == null && enableDebugLogs)
        {
            Debug.LogWarning($"[RotatableStatue] {name}: Could not find ArrowsUI!");
        }
    }

    /// <summary>
    /// Captures the initial world rotation of the arrows canvas.
    /// </summary>
    private void CaptureArrowsInitialRotation()
    {
        if (arrowsUI == null || _arrowsRotationInitialized) return;

        _arrowsInitialWorldRotation = arrowsUI.transform.rotation;
        _arrowsRotationInitialized = true;

        if (enableDebugLogs)
        {
            Debug.Log($"[RotatableStatue] {_configName}: Arrows initial rotation: {_arrowsInitialWorldRotation.eulerAngles}");
        }
    }

    #endregion

    #region Arrow Event Subscription

    /// <summary>
    /// Subscribes to arrow click events. Safe to call multiple times.
    /// </summary>
    private void SubscribeToArrowEvents()
    {
        if (arrowsUI == null || _isSubscribedToArrows) return;

        arrowsUI.OnLeftArrowClicked += RotateCounterClockwise;
        arrowsUI.OnRightArrowClicked += RotateClockwise;
        _isSubscribedToArrows = true;

        if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: Subscribed to arrow events");
    }

    /// <summary>
    /// Unsubscribes from arrow click events. Safe to call multiple times.
    /// </summary>
    private void UnsubscribeFromArrowEvents()
    {
        if (arrowsUI == null || !_isSubscribedToArrows) return;

        arrowsUI.OnLeftArrowClicked -= RotateCounterClockwise;
        arrowsUI.OnRightArrowClicked -= RotateClockwise;
        _isSubscribedToArrows = false;

        if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: Unsubscribed from arrow events");
    }

    #endregion

    #region Public Initialization Methods

    /// <summary>
    /// Initializes the statue with configuration data. Called by StatueRotationPuzzle.
    /// </summary>
    public void Initialize(string configName, float correctRotation)
    {
        if (enableDebugLogs) Debug.Log($"[RotatableStatue] {name}: Initialize({configName}, {correctRotation})");

        _configName = configName;
        _correctRotation = NormalizeRotation(correctRotation);
        _isPlaced = false;
        _currentRotation = NormalizeRotation(transform.eulerAngles.y);

        // Ensure components are found
        if (!_isInitialized)
        {
            InitializeComponents();
        }
        else if (arrowsUI == null)
        {
            FindArrowsUI();
        }

        // Ensure we're subscribed
        SubscribeToArrowEvents();

        // Capture arrow rotation if not already done
        CaptureArrowsInitialRotation();

        // Hide arrows until placed
        if (arrowsUI != null)
        {
            arrowsUI.Hide();
        }
    }

    /// <summary>
    /// Called when the statue has finished its placement animation.
    /// </summary>
    public void OnPlacementComplete()
    {
        _isPlaced = true;
        _currentRotation = NormalizeRotation(transform.eulerAngles.y);

        if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: Placement complete at rotation {_currentRotation}");

        // Final initialization check
        if (arrowsUI == null)
        {
            FindArrowsUI();
            SubscribeToArrowEvents();
        }

        CaptureArrowsInitialRotation();
    }

    /// <summary>
    /// Enables or disables interaction with this statue.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        _isInteractable = interactable;

        if (!interactable)
        {
            SetHovered(false);
        }

        if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: Interactable = {interactable}");
    }

    #endregion

    #region Rotation

    /// <summary>
    /// Rotates the statue clockwise by the rotation step.
    /// </summary>
    public void RotateClockwise()
    {
        if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: RotateClockwise called");

        if (!CanRotate()) return;

        Rotate(rotationStep);
    }

    /// <summary>
    /// Rotates the statue counter-clockwise by the rotation step.
    /// </summary>
    public void RotateCounterClockwise()
    {
        if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: RotateCounterClockwise called");

        if (!CanRotate()) return;

        Rotate(-rotationStep);
    }

    /// <summary>
    /// Returns true if the statue can currently be rotated.
    /// </summary>
    private bool CanRotate()
    {
        if (!_isInteractable)
        {
            if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: Cannot rotate - not interactable");
            return false;
        }

        if (_isRotating)
        {
            if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: Cannot rotate - already rotating");
            return false;
        }

        if (!_isPlaced)
        {
            if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: Cannot rotate - not placed yet");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Initiates rotation by the specified angle.
    /// </summary>
    private void Rotate(float angle)
    {
        float targetRotation = NormalizeRotation(_currentRotation + angle);
        StartCoroutine(RotateCoroutine(targetRotation));
    }

    /// <summary>
    /// Smoothly rotates the statue to the target rotation.
    /// </summary>
    private IEnumerator RotateCoroutine(float targetRotation)
    {
        _isRotating = true;

        // Play rotation sound
        if (audioSource != null && rotateSound != null)
        {
            audioSource.PlayOneShot(rotateSound);
        }

        float startRotation = _currentRotation;
        float elapsed = 0f;
        float delta = Mathf.DeltaAngle(startRotation, targetRotation);

        // Preserve X and Z rotation
        Vector3 currentEuler = transform.eulerAngles;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);

            // Smoothstep easing
            t = t * t * (3f - 2f * t);

            float newY = startRotation + delta * t;
            transform.rotation = Quaternion.Euler(currentEuler.x, newY, currentEuler.z);

            // LateUpdate handles keeping arrows stationary

            yield return null;
        }

        // Snap to exact target
        _currentRotation = targetRotation;
        transform.rotation = Quaternion.Euler(currentEuler.x, _currentRotation, currentEuler.z);

        _isRotating = false;

        // Fire events
        OnRotationChanged?.Invoke(_currentRotation);
        OnAnyStatueRotated?.Invoke();

        if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: Rotation complete = {_currentRotation}");
    }

    /// <summary>
    /// Normalizes an angle to the range [0, 360).
    /// </summary>
    private float NormalizeRotation(float rotation)
    {
        rotation = rotation % 360f;
        if (rotation < 0f) rotation += 360f;
        return rotation;
    }

    /// <summary>
    /// Returns true if the statue is at the correct rotation (within tolerance).
    /// CRITICAL FIX: Now checks _isPlaced to prevent premature win detection.
    /// </summary>
    public bool IsAtCorrectRotation()
    {
        // FIX: Must be placed before we can consider it "at correct rotation"
        if (!_isPlaced)
        {
            return false;
        }

        float diff = Mathf.Abs(Mathf.DeltaAngle(_currentRotation, _correctRotation));
        return diff < 1f;
    }

    #endregion

    #region Hover Handling (Raycast-based only)

    /// <summary>
    /// Called by PuzzleInputHandler when raycast enters this statue.
    /// FIX: Now returns bool to indicate if hover was successfully activated.
    /// </summary>
    /// <returns>True if hover was activated, false if it was rejected.</returns>
    public bool OnHoverEnter()
    {
        if (!_isInteractable || !_isPlaced)
        {
            if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: OnHoverEnter rejected (interactable={_isInteractable}, placed={_isPlaced})");
            return false;
        }

        SetHovered(true);
        return true;
    }

    /// <summary>
    /// Called by PuzzleInputHandler when raycast exits this statue.
    /// </summary>
    public void OnHoverExit()
    {
        SetHovered(false);
    }

    /// <summary>
    /// Sets the hover state and updates visuals accordingly.
    /// </summary>
    private void SetHovered(bool hovered)
    {
        if (_isHovered == hovered) return;

        _isHovered = hovered;

        if (enableDebugLogs) Debug.Log($"[RotatableStatue] {_configName}: SetHovered({hovered})");

        // Update outline
        if (outlineComponent != null)
        {
            outlineComponent.enabled = hovered;
            if (hovered)
            {
                outlineComponent.OutlineColor = glowColor;
                outlineComponent.OutlineWidth = glowWidth;
            }
        }

        // Update arrows UI
        if (arrowsUI != null)
        {
            if (hovered)
            {
                arrowsUI.Show();
            }
            else
            {
                arrowsUI.Hide();
            }
        }
        else if (hovered && enableDebugLogs)
        {
            Debug.LogWarning($"[RotatableStatue] {_configName}: ArrowsUI is null, cannot show arrows!");
        }
    }

    #endregion
}