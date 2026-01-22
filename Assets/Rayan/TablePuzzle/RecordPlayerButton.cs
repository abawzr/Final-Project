using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Individual clickable 3D button for the record player.
/// Requires a Collider component for raycast detection.
/// For constant glow, use the GlowEffect.shader material instead of Outline.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RecordPlayerButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    [Header("Events")]
    public UnityEvent OnButtonClicked;

    // State
    private bool _isHovered = false;
    private bool _isPressed = false;
    private Vector3 _originalLocalPosition;
    private Vector3 _pressedLocalPosition;

    // Shader property IDs
    private static readonly int GlowIntensityProperty = Shader.PropertyToID("_GlowIntensity");
    private float _baseGlowIntensity;
    private MaterialPropertyBlock _propertyBlock;

    private void Awake()
    {
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
                Debug.LogWarning($"RecordPlayerButton: Material on {glowRenderer.name} doesn't have _GlowIntensity property. Using default value.");
            }
        }
    }

    private void Update()
    {
        // Smooth button press animation
        Vector3 targetPosition = _isPressed ? _pressedLocalPosition : _originalLocalPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * pressSpeed);
    }

    #region Pointer Events (for EventSystem-based detection)

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHovered(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHovered(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick();
    }

    #endregion

    #region Raycast-based interaction (called from PuzzleInputHandler)

    /// <summary>
    /// Called by PuzzleInputHandler when mouse hovers over this button.
    /// </summary>
    public void OnHoverEnter()
    {
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
        OnClick();
    }

    #endregion

    /// <summary>
    /// Sets the hover state and updates visuals.
    /// </summary>
    private void SetHovered(bool hovered)
    {
        if (_isHovered == hovered) return;
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

        // Button press animation
        StartCoroutine(PressAnimation());

        // Fire event
        OnButtonClicked?.Invoke();
    }

    /// <summary>
    /// Plays a quick press animation.
    /// </summary>
    private System.Collections.IEnumerator PressAnimation()
    {
        _isPressed = true;
        yield return new WaitForSeconds(0.1f);
        _isPressed = false;
    }

    /// <summary>
    /// Returns true if this button has constant glow.
    /// </summary>
    public bool HasConstantGlow => hasConstantGlow;
}