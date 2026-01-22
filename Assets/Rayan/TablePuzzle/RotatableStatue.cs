using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles individual statue rotation, hover detection, and visual feedback.
/// Attach to each statue prefab.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RotatableStatue : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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

    // Static event for puzzle controller to subscribe to
    public static event Action OnAnyStatueRotated;

    // Instance event
    public event Action<float> OnRotationChanged;

    // Configuration
    private string _configName;
    private float _correctRotation;

    // State
    private float _currentRotation = 0f;
    private bool _isRotating = false;
    private bool _isHovered = false;
    private bool _isInteractable = true;
    private bool _isPlaced = false;

    /// <summary>
    /// Gets the configuration name of this statue.
    /// </summary>
    public string ConfigName => _configName;

    /// <summary>
    /// Gets the current Y rotation of the statue.
    /// </summary>
    public float CurrentRotation => _currentRotation;

    /// <summary>
    /// Returns true if the statue has been placed on the table.
    /// </summary>
    public bool IsPlaced => _isPlaced;

    private void Awake()
    {
        // Try to find outline component if not assigned
        if (outlineComponent == null)
        {
            outlineComponent = GetComponent<Outline>();
            if (outlineComponent == null)
            {
                outlineComponent = GetComponentInChildren<Outline>();
            }
        }

        // Try to find audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Try to find arrows UI (include inactive in case it's hidden)
        if (arrowsUI == null)
        {
            arrowsUI = GetComponentInChildren<StatueArrowsUI>(true);
        }

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
    }

    private void OnEnable()
    {
        if (arrowsUI != null)
        {
            arrowsUI.OnLeftArrowClicked += RotateCounterClockwise;
            arrowsUI.OnRightArrowClicked += RotateClockwise;
        }
    }

    private void OnDisable()
    {
        if (arrowsUI != null)
        {
            arrowsUI.OnLeftArrowClicked -= RotateCounterClockwise;
            arrowsUI.OnRightArrowClicked -= RotateClockwise;
        }
    }

    /// <summary>
    /// Initializes the statue with its configuration.
    /// </summary>
    public void Initialize(string configName, float correctRotation)
    {
        _configName = configName;
        _correctRotation = NormalizeRotation(correctRotation);
        _isPlaced = false;

        // Get initial rotation from current transform
        _currentRotation = NormalizeRotation(transform.eulerAngles.y);
    }

    /// <summary>
    /// Called when placement animation completes (call from Animation Event or externally).
    /// </summary>
    public void OnPlacementComplete()
    {
        _isPlaced = true;
        _currentRotation = NormalizeRotation(transform.eulerAngles.y);
    }

    /// <summary>
    /// Sets whether this statue can be interacted with.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        _isInteractable = interactable;

        if (!interactable)
        {
            // Hide UI elements
            if (outlineComponent != null)
            {
                outlineComponent.enabled = false;
            }
            if (arrowsUI != null)
            {
                arrowsUI.Hide();
            }
        }
    }

    /// <summary>
    /// Rotates the statue clockwise by one step (45 degrees).
    /// </summary>
    public void RotateClockwise()
    {
        if (!_isInteractable || _isRotating || !_isPlaced) return;
        Rotate(rotationStep);
    }

    /// <summary>
    /// Rotates the statue counter-clockwise by one step (-45 degrees).
    /// </summary>
    public void RotateCounterClockwise()
    {
        if (!_isInteractable || _isRotating || !_isPlaced) return;
        Rotate(-rotationStep);
    }

    /// <summary>
    /// Rotates the statue by the specified angle.
    /// </summary>
    private void Rotate(float angle)
    {
        if (_isRotating) return;

        float targetRotation = NormalizeRotation(_currentRotation + angle);
        StartCoroutine(RotateCoroutine(targetRotation));
    }

    /// <summary>
    /// Smooth rotation coroutine.
    /// </summary>
    private IEnumerator RotateCoroutine(float targetRotation)
    {
        _isRotating = true;

        // Play sound
        if (audioSource != null && rotateSound != null)
        {
            audioSource.PlayOneShot(rotateSound);
        }

        float startRotation = _currentRotation;
        float elapsed = 0f;

        // Store original X and Z rotation to preserve them
        float originalX = transform.eulerAngles.x;
        float originalZ = transform.eulerAngles.z;

        // Calculate shortest rotation path
        float delta = Mathf.DeltaAngle(startRotation, targetRotation);

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;

            // Smooth step interpolation
            t = t * t * (3f - 2f * t);

            float newRotation = startRotation + delta * t;
            transform.rotation = Quaternion.Euler(originalX, newRotation, originalZ);

            yield return null;
        }

        // Snap to final rotation
        _currentRotation = targetRotation;
        transform.rotation = Quaternion.Euler(originalX, _currentRotation, originalZ);

        _isRotating = false;

        // Fire events
        OnRotationChanged?.Invoke(_currentRotation);
        OnAnyStatueRotated?.Invoke();
    }

    /// <summary>
    /// Normalizes rotation to 0-360 range.
    /// </summary>
    private float NormalizeRotation(float rotation)
    {
        rotation = rotation % 360f;
        if (rotation < 0f)
        {
            rotation += 360f;
        }
        return rotation;
    }

    /// <summary>
    /// Checks if the statue is at its correct rotation.
    /// </summary>
    public bool IsAtCorrectRotation()
    {
        float diff = Mathf.Abs(Mathf.DeltaAngle(_currentRotation, _correctRotation));
        return diff < 1f; // Allow small tolerance
    }

    #region Pointer Events (for EventSystem-based detection)

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInteractable || !_isPlaced) return;
        SetHovered(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHovered(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Click handling is done through arrows UI
    }

    #endregion

    #region Raycast-based hover (called from PuzzleInputHandler)

    /// <summary>
    /// Called by PuzzleInputHandler when mouse hovers over this statue.
    /// </summary>
    public void OnHoverEnter()
    {
        if (!_isInteractable || !_isPlaced) return;
        SetHovered(true);
    }

    /// <summary>
    /// Called by PuzzleInputHandler when mouse stops hovering over this statue.
    /// </summary>
    public void OnHoverExit()
    {
        SetHovered(false);
    }

    #endregion

    /// <summary>
    /// Sets the hover state and updates visuals.
    /// </summary>
    private void SetHovered(bool hovered)
    {
        if (_isHovered == hovered) return;
        _isHovered = hovered;

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
    }
}