using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Individual clickable 3D button for the record player.
/// Requires a Collider component for raycast detection.
/// 
/// v3 - Features:
///      - Added IsLocked property to prevent spam clicking
///      - Added IsDisabled property to completely disable the button (for puzzle solved state)
///      - Support for "stay pressed" mode (for play button that stays down while playing)
///      - Removed IPointer interfaces to avoid duplicate input
/// </summary>
[RequireComponent(typeof(Collider))]
public class RecordPlayerButton : MonoBehaviour
{
    [Header("Hover Feedback (QuickOutline)")]
    [SerializeField] private Outline outlineComponent;
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField] private float outlineWidth = 3f;

    [Header("Constant Glow (Shader-based)")]
    [Tooltip("If true, uses a separate glow mesh with GlowEffect shader for constant pulsing")]
    [SerializeField] private bool hasConstantGlow = false;
    [SerializeField] private Renderer glowRenderer;
    [SerializeField] private float hoverGlowIntensityMultiplier = 1.5f;

    [Header("Button Press Animation")]
    [SerializeField] private float pressDepth = 0.02f;
    [SerializeField] private float pressSpeed = 10f;

    [Header("Stay Pressed Mode")]
    [Tooltip("If true, button stays pressed until manually released (for play button)")]
    [SerializeField] private bool stayPressedMode = false;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    [Header("Events")]
    public UnityEvent OnButtonClicked;

    // State
    private bool _isHovered = false;
    private bool _isPressed = false;
    private bool _isLocked = false;      // Prevents clicking until unlocked (for spam prevention)
    private bool _isDisabled = false;    // Completely disables the button (for puzzle solved)
    private Vector3 _originalLocalPosition;
    private Vector3 _pressedLocalPosition;

    // Shader property IDs
    private static readonly int GlowIntensityProperty = Shader.PropertyToID("_GlowIntensity");
    private float _baseGlowIntensity;
    private MaterialPropertyBlock _propertyBlock;

    // Cached collider reference
    private Collider _collider;

    #region Public Properties

    /// <summary>
    /// Returns true if button is currently hovered.
    /// </summary>
    public bool IsHovered => _isHovered;

    /// <summary>
    /// Returns true if button is currently pressed down.
    /// </summary>
    public bool IsPressed => _isPressed;

    /// <summary>
    /// Returns true if button is locked (can't be clicked until unlocked).
    /// </summary>
    public bool IsLocked => _isLocked;

    /// <summary>
    /// Returns true if button is completely disabled.
    /// </summary>
    public bool IsDisabled => _isDisabled;

    /// <summary>
    /// Returns true if this button has constant glow.
    /// </summary>
    public bool HasConstantGlow => hasConstantGlow;

    /// <summary>
    /// Returns true if this button uses stay pressed mode.
    /// </summary>
    public bool StayPressedMode => stayPressedMode;

    #endregion

    private void Awake()
    {
        // Cache collider
        _collider = GetComponent<Collider>();

        // Store original position
        _originalLocalPosition = transform.localPosition;
        _pressedLocalPosition = _originalLocalPosition - transform.up * pressDepth;

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
            if (audioSource == null)
            {
                audioSource = GetComponentInParent<AudioSource>();
            }
        }

        // Setup outline (for hover effect, not constant glow)
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }

        // Setup shader-based glow
        if (hasConstantGlow && glowRenderer != null)
        {
            _propertyBlock = new MaterialPropertyBlock();
            glowRenderer.GetPropertyBlock(_propertyBlock);

            // Safely get the glow intensity, default to 2.0 if property doesn't exist
            if (glowRenderer.sharedMaterial.HasProperty(GlowIntensityProperty))
            {
                _baseGlowIntensity = glowRenderer.sharedMaterial.GetFloat(GlowIntensityProperty);
            }
            else
            {
                _baseGlowIntensity = 2.0f;
            }
        }
    }

    private void Update()
    {
        // Smooth button press animation
        Vector3 targetPosition = _isPressed ? _pressedLocalPosition : _originalLocalPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * pressSpeed);
    }

    #region Raycast-based interaction (called from PuzzleInputHandler)

    /// <summary>
    /// Called by PuzzleInputHandler when mouse hovers over this button.
    /// </summary>
    public void OnHoverEnter()
    {
        // Don't show hover effects if disabled
        if (_isDisabled) return;

        SetHovered(true);
    }

    /// <summary>
    /// Called by PuzzleInputHandler when mouse stops hovering over this button.
    /// </summary>
    public void OnHoverExit()
    {
        SetHovered(false);
    }

    /// <summary>
    /// Called by PuzzleInputHandler when this button is clicked via raycast.
    /// </summary>
    public void OnClickRaycast()
    {
        // Don't allow clicking if disabled or locked
        if (_isDisabled || _isLocked) return;

        OnClick();
    }

    #endregion

    /// <summary>
    /// Sets the hover state and updates visuals.
    /// </summary>
    private void SetHovered(bool hovered)
    {
        if (_isHovered == hovered) return;

        // Don't allow hover if disabled
        if (hovered && _isDisabled) return;

        _isHovered = hovered;

        // Play hover sound
        if (hovered && audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }

        // Update outline on hover
        if (outlineComponent != null)
        {
            outlineComponent.enabled = hovered;
            if (hovered)
            {
                outlineComponent.OutlineColor = outlineColor;
                outlineComponent.OutlineWidth = outlineWidth;
            }
        }

        // Intensify shader glow on hover
        if (hasConstantGlow && glowRenderer != null && _propertyBlock != null)
        {
            float targetIntensity = hovered ? _baseGlowIntensity * hoverGlowIntensityMultiplier : _baseGlowIntensity;
            _propertyBlock.SetFloat(GlowIntensityProperty, targetIntensity);
            glowRenderer.SetPropertyBlock(_propertyBlock);
        }
    }

    /// <summary>
    /// Handles the click action.
    /// </summary>
    private void OnClick()
    {
        // Play click sound
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // Handle press animation based on mode
        if (stayPressedMode)
        {
            // Stay pressed mode: button stays down, must be released manually
            _isPressed = true;
            _isLocked = true; // Lock to prevent spam
        }
        else
        {
            // Normal mode: quick press animation
            StartCoroutine(PressAnimation());
        }

        // Fire event
        OnButtonClicked?.Invoke();
    }

    /// <summary>
    /// Plays a quick press animation (for normal buttons).
    /// </summary>
    private System.Collections.IEnumerator PressAnimation()
    {
        _isPressed = true;
        yield return new WaitForSeconds(0.1f);
        _isPressed = false;
    }

    #region Public Control Methods

    /// <summary>
    /// Locks the button to prevent clicking. Used for spam prevention.
    /// </summary>
    public void Lock()
    {
        _isLocked = true;
    }

    /// <summary>
    /// Unlocks the button to allow clicking again.
    /// </summary>
    public void Unlock()
    {
        _isLocked = false;
    }

    /// <summary>
    /// Releases the button from pressed state (for stay pressed mode).
    /// Also unlocks the button.
    /// </summary>
    public void Release()
    {
        _isPressed = false;
        _isLocked = false;
    }

    /// <summary>
    /// Completely disables the button. No hover, no click, no animation.
    /// Used when puzzle is solved.
    /// </summary>
    public void Disable()
    {
        _isDisabled = true;
        _isLocked = true;
        _isPressed = false;

        // Clear hover state
        SetHovered(false);

        // Disable outline
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }

        // Optionally disable the collider to prevent any raycast hits
        if (_collider != null)
        {
            _collider.enabled = false;
        }

        // Reset glow to base intensity
        if (hasConstantGlow && glowRenderer != null && _propertyBlock != null)
        {
            _propertyBlock.SetFloat(GlowIntensityProperty, _baseGlowIntensity);
            glowRenderer.SetPropertyBlock(_propertyBlock);
        }
    }

    /// <summary>
    /// Re-enables the button after it was disabled.
    /// </summary>
    public void Enable()
    {
        _isDisabled = false;
        _isLocked = false;

        // Re-enable the collider
        if (_collider != null)
        {
            _collider.enabled = true;
        }
    }

    /// <summary>
    /// Forces the button to the pressed position (for external animation control).
    /// </summary>
    public void ForcePressed()
    {
        _isPressed = true;
    }

    /// <summary>
    /// Forces the button to the released position (for external animation control).
    /// </summary>
    public void ForceReleased()
    {
        _isPressed = false;
    }

    #endregion
}