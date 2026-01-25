using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Manages the arrow UI for statue rotation.
/// v9 - Fixes:
///      - Added visibility check to click handlers (CRITICAL: prevents clicking invisible arrows)
///      - Colliders are now disabled when hidden (fixes raycast hitting invisible arrows)
///      - Added IsInteractable property for external state checking
///      - Improved null safety throughout
/// </summary>
public class StatueArrowsUI : MonoBehaviour
{
    [Header("Arrow References (UI Buttons)")]
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Image leftArrowImage;
    [SerializeField] private Image rightArrowImage;

    [Header("Arrow GameObjects (for 3D arrows with colliders)")]
    [Tooltip("3D arrow object with collider and RotationArrow component")]
    [SerializeField] private GameObject leftArrowObject;
    [Tooltip("3D arrow object with collider and RotationArrow component")]
    [SerializeField] private GameObject rightArrowObject;

    [Header("Visibility Settings")]
    [Tooltip("If true, uses Renderer enable/disable instead of SetActive for 3D arrows")]
    [SerializeField] private bool useRendererForVisibility = true;
    [Tooltip("Alpha value when arrows are hidden (0 = fully hidden)")]
    [SerializeField] private float hiddenAlpha = 0f;
    [Tooltip("Alpha value when arrows are visible (1 = fully visible)")]
    [SerializeField] private float visibleAlpha = 1f;

    [Header("Hover Effects")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.9f, 0.5f);
    [SerializeField] private float transitionSpeed = 10f;

    [Header("Outline Effects (for 3D arrows)")]
    [SerializeField] private Outline leftArrowOutline;
    [SerializeField] private Outline rightArrowOutline;
    [SerializeField] private Color arrowGlowColor = Color.white;
    [SerializeField] private float arrowGlowWidth = 3f;

    [Header("Canvas Settings")]
    [SerializeField] private Canvas arrowCanvas;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    // Events - fired when arrows are clicked
    public event Action OnLeftArrowClicked;
    public event Action OnRightArrowClicked;

    // State
    private bool _isVisible = false;
    private bool _leftHovered = false;
    private bool _rightHovered = false;
    private bool _isInitialized = false;

    // Target states for smooth transitions
    private float _leftTargetScale = 1f;
    private float _rightTargetScale = 1f;
    private Color _leftTargetColor;
    private Color _rightTargetColor;

    // Original scales (stored from prefab)
    private Vector3 _leftArrowOriginalScale = Vector3.one;
    private Vector3 _rightArrowOriginalScale = Vector3.one;
    private Vector3 _leftObjectOriginalScale = Vector3.one;
    private Vector3 _rightObjectOriginalScale = Vector3.one;

    // Cached renderers for 3D arrows
    private Renderer _leftArrowRenderer;
    private Renderer _rightArrowRenderer;

    // Cached colliders for 3D arrows (NEW: for disabling when hidden)
    private Collider _leftArrowCollider;
    private Collider _rightArrowCollider;

    // Public properties
    public bool IsVisible => _isVisible;
    public bool IsInteractable => _isVisible && _isInitialized;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponent();
    }

    private void Start()
    {
        if (!_isInitialized)
        {
            InitializeComponent();
        }
    }

    private void Update()
    {
        if (!_isVisible) return;
        UpdateArrowVisuals();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes all components and caches references.
    /// </summary>
    private void InitializeComponent()
    {
        if (_isInitialized) return;

        _leftTargetColor = normalColor;
        _rightTargetColor = normalColor;

        // Store original LOCAL scales for UI arrows
        CacheOriginalScales();

        // Cache renderers for 3D arrows
        CacheRenderers();

        // Cache colliders for 3D arrows (NEW)
        CacheColliders();

        // Setup canvas and canvas group
        SetupCanvas();

        // Setup button click listeners (for direct UI clicks)
        SetupButtons();

        // Disable outlines initially
        if (leftArrowOutline != null) leftArrowOutline.enabled = false;
        if (rightArrowOutline != null) rightArrowOutline.enabled = false;

        _isInitialized = true;
    }

    /// <summary>
    /// Caches the original scales of arrow objects.
    /// </summary>
    private void CacheOriginalScales()
    {
        if (leftArrowImage != null)
        {
            _leftArrowOriginalScale = leftArrowImage.transform.localScale;
            if (_leftArrowOriginalScale.magnitude < 0.01f)
                _leftArrowOriginalScale = Vector3.one;
        }

        if (rightArrowImage != null)
        {
            _rightArrowOriginalScale = rightArrowImage.transform.localScale;
            if (_rightArrowOriginalScale.magnitude < 0.01f)
                _rightArrowOriginalScale = Vector3.one;
        }

        if (leftArrowObject != null)
        {
            _leftObjectOriginalScale = leftArrowObject.transform.localScale;
            if (_leftObjectOriginalScale.magnitude < 0.01f)
                _leftObjectOriginalScale = Vector3.one;
        }

        if (rightArrowObject != null)
        {
            _rightObjectOriginalScale = rightArrowObject.transform.localScale;
            if (_rightObjectOriginalScale.magnitude < 0.01f)
                _rightObjectOriginalScale = Vector3.one;
        }
    }

    /// <summary>
    /// Caches renderer references for 3D arrows.
    /// </summary>
    private void CacheRenderers()
    {
        if (leftArrowObject != null)
        {
            _leftArrowRenderer = leftArrowObject.GetComponent<Renderer>();
            if (_leftArrowRenderer == null)
            {
                _leftArrowRenderer = leftArrowObject.GetComponentInChildren<Renderer>();
            }
        }

        if (rightArrowObject != null)
        {
            _rightArrowRenderer = rightArrowObject.GetComponent<Renderer>();
            if (_rightArrowRenderer == null)
            {
                _rightArrowRenderer = rightArrowObject.GetComponentInChildren<Renderer>();
            }
        }
    }

    /// <summary>
    /// Caches collider references for 3D arrows.
    /// NEW: Required for properly disabling raycasts when arrows are hidden.
    /// </summary>
    private void CacheColliders()
    {
        if (leftArrowObject != null)
        {
            _leftArrowCollider = leftArrowObject.GetComponent<Collider>();
            if (_leftArrowCollider == null)
            {
                _leftArrowCollider = leftArrowObject.GetComponentInChildren<Collider>();
            }
        }

        if (rightArrowObject != null)
        {
            _rightArrowCollider = rightArrowObject.GetComponent<Collider>();
            if (_rightArrowCollider == null)
            {
                _rightArrowCollider = rightArrowObject.GetComponentInChildren<Collider>();
            }
        }
    }

    /// <summary>
    /// Sets up canvas and canvas group components.
    /// </summary>
    private void SetupCanvas()
    {
        if (arrowCanvas == null)
        {
            arrowCanvas = GetComponent<Canvas>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    /// <summary>
    /// Sets up button click listeners.
    /// </summary>
    private void SetupButtons()
    {
        if (leftArrowButton != null)
        {
            leftArrowButton.onClick.RemoveAllListeners();
            leftArrowButton.onClick.AddListener(OnLeftArrowClick);
        }

        if (rightArrowButton != null)
        {
            rightArrowButton.onClick.RemoveAllListeners();
            rightArrowButton.onClick.AddListener(OnRightArrowClick);
        }
    }

    #endregion

    #region Visibility

    /// <summary>
    /// Shows the arrows.
    /// </summary>
    public void Show()
    {
        if (!_isInitialized)
        {
            InitializeComponent();
        }

        SetVisible(true);
    }

    /// <summary>
    /// Hides the arrows.
    /// </summary>
    public void Hide()
    {
        SetVisible(false);
        ResetHoverStates();
    }

    /// <summary>
    /// Sets the visibility of the arrows.
    /// FIX: Now also disables colliders when hidden to prevent raycast hits on invisible arrows.
    /// </summary>
    private void SetVisible(bool visible)
    {
        _isVisible = visible;

        // Handle UI Canvas visibility via CanvasGroup
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? visibleAlpha : hiddenAlpha;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        // Handle 3D arrow visibility
        if (useRendererForVisibility)
        {
            // Use renderer enable/disable for visual
            if (_leftArrowRenderer != null)
            {
                _leftArrowRenderer.enabled = visible;
            }
            if (_rightArrowRenderer != null)
            {
                _rightArrowRenderer.enabled = visible;
            }

            // FIX: Also disable colliders to prevent raycast hits on invisible arrows
            if (_leftArrowCollider != null)
            {
                _leftArrowCollider.enabled = visible;
            }
            if (_rightArrowCollider != null)
            {
                _rightArrowCollider.enabled = visible;
            }
        }
        else
        {
            // Fallback: Use SetActive (colliders will be disabled when hidden)
            if (leftArrowObject != null)
            {
                leftArrowObject.SetActive(visible);
            }
            if (rightArrowObject != null)
            {
                rightArrowObject.SetActive(visible);
            }
        }
    }

    /// <summary>
    /// Resets all hover states to default.
    /// </summary>
    private void ResetHoverStates()
    {
        _leftHovered = false;
        _rightHovered = false;
        _leftTargetScale = normalScale;
        _rightTargetScale = normalScale;
        _leftTargetColor = normalColor;
        _rightTargetColor = normalColor;

        // Disable outlines
        if (leftArrowOutline != null) leftArrowOutline.enabled = false;
        if (rightArrowOutline != null) rightArrowOutline.enabled = false;

        // Reset scales and colors immediately
        if (leftArrowImage != null)
        {
            leftArrowImage.transform.localScale = _leftArrowOriginalScale * normalScale;
            leftArrowImage.color = normalColor;
        }
        if (rightArrowImage != null)
        {
            rightArrowImage.transform.localScale = _rightArrowOriginalScale * normalScale;
            rightArrowImage.color = normalColor;
        }
        if (leftArrowObject != null)
        {
            leftArrowObject.transform.localScale = _leftObjectOriginalScale * normalScale;
        }
        if (rightArrowObject != null)
        {
            rightArrowObject.transform.localScale = _rightObjectOriginalScale * normalScale;
        }
    }

    #endregion

    #region Hover Effects

    /// <summary>
    /// Called when mouse enters an arrow.
    /// </summary>
    private void OnArrowHoverEnter(bool isLeft)
    {
        if (!_isVisible) return;

        if (isLeft)
        {
            _leftHovered = true;
            _leftTargetScale = hoverScale;
            _leftTargetColor = hoverColor;

            if (leftArrowOutline != null)
            {
                leftArrowOutline.enabled = true;
                leftArrowOutline.OutlineColor = arrowGlowColor;
                leftArrowOutline.OutlineWidth = arrowGlowWidth;
            }
        }
        else
        {
            _rightHovered = true;
            _rightTargetScale = hoverScale;
            _rightTargetColor = hoverColor;

            if (rightArrowOutline != null)
            {
                rightArrowOutline.enabled = true;
                rightArrowOutline.OutlineColor = arrowGlowColor;
                rightArrowOutline.OutlineWidth = arrowGlowWidth;
            }
        }

        // Play hover sound
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    /// <summary>
    /// Called when mouse exits an arrow.
    /// </summary>
    private void OnArrowHoverExit(bool isLeft)
    {
        if (isLeft)
        {
            _leftHovered = false;
            _leftTargetScale = normalScale;
            _leftTargetColor = normalColor;

            if (leftArrowOutline != null)
            {
                leftArrowOutline.enabled = false;
            }
        }
        else
        {
            _rightHovered = false;
            _rightTargetScale = normalScale;
            _rightTargetColor = normalColor;

            if (rightArrowOutline != null)
            {
                rightArrowOutline.enabled = false;
            }
        }
    }

    /// <summary>
    /// Updates arrow visuals smoothly based on hover state.
    /// </summary>
    private void UpdateArrowVisuals()
    {
        float dt = Time.deltaTime * transitionSpeed;

        // Left arrow UI
        if (leftArrowImage != null)
        {
            Vector3 targetScale = _leftArrowOriginalScale * _leftTargetScale;
            leftArrowImage.transform.localScale = Vector3.Lerp(leftArrowImage.transform.localScale, targetScale, dt);
            leftArrowImage.color = Color.Lerp(leftArrowImage.color, _leftTargetColor, dt);
        }

        // Left arrow 3D object
        if (leftArrowObject != null)
        {
            Vector3 targetScale = _leftObjectOriginalScale * _leftTargetScale;
            leftArrowObject.transform.localScale = Vector3.Lerp(leftArrowObject.transform.localScale, targetScale, dt);
        }

        // Right arrow UI
        if (rightArrowImage != null)
        {
            Vector3 targetScale = _rightArrowOriginalScale * _rightTargetScale;
            rightArrowImage.transform.localScale = Vector3.Lerp(rightArrowImage.transform.localScale, targetScale, dt);
            rightArrowImage.color = Color.Lerp(rightArrowImage.color, _rightTargetColor, dt);
        }

        // Right arrow 3D object
        if (rightArrowObject != null)
        {
            Vector3 targetScale = _rightObjectOriginalScale * _rightTargetScale;
            rightArrowObject.transform.localScale = Vector3.Lerp(rightArrowObject.transform.localScale, targetScale, dt);
        }
    }

    #endregion

    #region Click Handling

    /// <summary>
    /// Called when left arrow is clicked (via UI Button or raycast).
    /// FIX: Now checks visibility before processing click.
    /// </summary>
    private void OnLeftArrowClick()
    {
        // CRITICAL FIX: Prevent clicking invisible arrows
        if (!_isVisible)
        {
            return;
        }

        PlayClickSound();
        OnLeftArrowClicked?.Invoke();
    }

    /// <summary>
    /// Called when right arrow is clicked (via UI Button or raycast).
    /// FIX: Now checks visibility before processing click.
    /// </summary>
    private void OnRightArrowClick()
    {
        // CRITICAL FIX: Prevent clicking invisible arrows
        if (!_isVisible)
        {
            return;
        }

        PlayClickSound();
        OnRightArrowClicked?.Invoke();
    }

    /// <summary>
    /// Plays the click sound effect.
    /// </summary>
    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    #endregion

    #region Public API for PuzzleInputHandler (Raycast-based)

    /// <summary>Called by PuzzleInputHandler when raycast enters left arrow.</summary>
    public void OnLeftArrowHoverEnter() => OnArrowHoverEnter(true);

    /// <summary>Called by PuzzleInputHandler when raycast exits left arrow.</summary>
    public void OnLeftArrowHoverExit() => OnArrowHoverExit(true);

    /// <summary>Called by PuzzleInputHandler when raycast enters right arrow.</summary>
    public void OnRightArrowHoverEnter() => OnArrowHoverEnter(false);

    /// <summary>Called by PuzzleInputHandler when raycast exits right arrow.</summary>
    public void OnRightArrowHoverExit() => OnArrowHoverExit(false);

    /// <summary>Called by PuzzleInputHandler when left arrow is clicked via raycast.</summary>
    public void OnLeftArrowClickRaycast() => OnLeftArrowClick();

    /// <summary>Called by PuzzleInputHandler when right arrow is clicked via raycast.</summary>
    public void OnRightArrowClickRaycast() => OnRightArrowClick();

    #endregion
}