using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Manages the circular arrow UI that appears around statues for rotation control.
/// Should be attached to a World Space Canvas that is a child of the statue.
/// </summary>
public class StatueArrowsUI : MonoBehaviour
{
    [Header("Arrow References")]
    [SerializeField] private Button leftArrowButton;    // Counter-clockwise
    [SerializeField] private Button rightArrowButton;   // Clockwise
    [SerializeField] private Image leftArrowImage;
    [SerializeField] private Image rightArrowImage;

    [Header("Arrow GameObjects (for 3D arrows)")]
    [SerializeField] private GameObject leftArrowObject;
    [SerializeField] private GameObject rightArrowObject;

    [Header("Hover Effects")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.9f, 0.5f);
    [SerializeField] private float transitionSpeed = 10f;

    [Header("Outline Effects")]
    [SerializeField] private Outline leftArrowOutline;
    [SerializeField] private Outline rightArrowOutline;
    [SerializeField] private Color arrowGlowColor = Color.white;
    [SerializeField] private float arrowGlowWidth = 3f;

    [Header("Canvas Settings")]
    [SerializeField] private Canvas arrowCanvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool billboardToCamera = true;
    [SerializeField] private Camera targetCamera;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    // Events
    public event Action OnLeftArrowClicked;
    public event Action OnRightArrowClicked;

    // State
    private bool _isVisible = false;
    private bool _leftHovered = false;
    private bool _rightHovered = false;

    // Target states for smooth transitions
    private float _leftTargetScale = 1f;
    private float _rightTargetScale = 1f;
    private Color _leftTargetColor;
    private Color _rightTargetColor;

    // Store original scales
    private Vector3 _leftArrowOriginalScale;
    private Vector3 _rightArrowOriginalScale;
    private Vector3 _leftObjectOriginalScale;
    private Vector3 _rightObjectOriginalScale;

    private void Awake()
    {
        // Initialize target colors
        _leftTargetColor = normalColor;
        _rightTargetColor = normalColor;

        // Store original scales
        if (leftArrowImage != null) _leftArrowOriginalScale = leftArrowImage.transform.localScale;
        if (rightArrowImage != null) _rightArrowOriginalScale = rightArrowImage.transform.localScale;
        if (leftArrowObject != null) _leftObjectOriginalScale = leftArrowObject.transform.localScale;
        if (rightArrowObject != null) _rightObjectOriginalScale = rightArrowObject.transform.localScale;

        // Setup canvas if not assigned
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

        // Setup button listeners
        if (leftArrowButton != null)
        {
            leftArrowButton.onClick.AddListener(OnLeftArrowClick);
            AddHoverEvents(leftArrowButton.gameObject, true);
        }

        if (rightArrowButton != null)
        {
            rightArrowButton.onClick.AddListener(OnRightArrowClick);
            AddHoverEvents(rightArrowButton.gameObject, false);
        }

        // Disable outlines initially
        if (leftArrowOutline != null) leftArrowOutline.enabled = false;
        if (rightArrowOutline != null) rightArrowOutline.enabled = false;

        // Hide by default
        SetVisible(false);
    }

    private void Update()
    {
        // Skip if not visible
        if (!_isVisible) return;

        // Billboard to camera
        if (billboardToCamera)
        {
            Camera cam = targetCamera;
            if (cam == null)
            {
                cam = Camera.main;
            }

            if (cam != null)
            {
                transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                                 cam.transform.rotation * Vector3.up);
            }
        }

        // Smooth scale and color transitions
        UpdateArrowVisuals();
    }

    /// <summary>
    /// Adds hover event triggers to an arrow button.
    /// </summary>
    private void AddHoverEvents(GameObject target, bool isLeft)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = target.AddComponent<EventTrigger>();
        }

        // Pointer Enter
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnArrowHoverEnter(isLeft); });
        trigger.triggers.Add(enterEntry);

        // Pointer Exit
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnArrowHoverExit(isLeft); });
        trigger.triggers.Add(exitEntry);
    }

    /// <summary>
    /// Called when pointer enters an arrow.
    /// </summary>
    private void OnArrowHoverEnter(bool isLeft)
    {
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
    /// Called when pointer exits an arrow.
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
    /// Updates arrow visuals with smooth transitions.
    /// </summary>
    private void UpdateArrowVisuals()
    {
        float dt = Time.deltaTime * transitionSpeed;

        // Left arrow - use original scale as base
        if (leftArrowImage != null)
        {
            Vector3 targetScale = _leftArrowOriginalScale * _leftTargetScale;
            leftArrowImage.transform.localScale = Vector3.Lerp(
                leftArrowImage.transform.localScale,
                targetScale,
                dt
            );
            leftArrowImage.color = Color.Lerp(leftArrowImage.color, _leftTargetColor, dt);
        }

        if (leftArrowObject != null)
        {
            Vector3 targetScale = _leftObjectOriginalScale * _leftTargetScale;
            leftArrowObject.transform.localScale = Vector3.Lerp(
                leftArrowObject.transform.localScale,
                targetScale,
                dt
            );
        }

        // Right arrow - use original scale as base
        if (rightArrowImage != null)
        {
            Vector3 targetScale = _rightArrowOriginalScale * _rightTargetScale;
            rightArrowImage.transform.localScale = Vector3.Lerp(
                rightArrowImage.transform.localScale,
                targetScale,
                dt
            );
            rightArrowImage.color = Color.Lerp(rightArrowImage.color, _rightTargetColor, dt);
        }

        if (rightArrowObject != null)
        {
            Vector3 targetScale = _rightObjectOriginalScale * _rightTargetScale;
            rightArrowObject.transform.localScale = Vector3.Lerp(
                rightArrowObject.transform.localScale,
                targetScale,
                dt
            );
        }
    }

    /// <summary>
    /// Called when left arrow is clicked.
    /// </summary>
    private void OnLeftArrowClick()
    {
        PlayClickSound();
        OnLeftArrowClicked?.Invoke();
    }

    /// <summary>
    /// Called when right arrow is clicked.
    /// </summary>
    private void OnRightArrowClick()
    {
        PlayClickSound();
        OnRightArrowClicked?.Invoke();
    }

    /// <summary>
    /// Plays the click sound.
    /// </summary>
    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    /// <summary>
    /// Shows the arrow UI.
    /// </summary>
    public void Show()
    {
        SetVisible(true);
    }

    /// <summary>
    /// Hides the arrow UI.
    /// </summary>
    public void Hide()
    {
        SetVisible(false);

        // Reset hover states
        _leftHovered = false;
        _rightHovered = false;
        _leftTargetScale = normalScale;
        _rightTargetScale = normalScale;
        _leftTargetColor = normalColor;
        _rightTargetColor = normalColor;

        if (leftArrowOutline != null) leftArrowOutline.enabled = false;
        if (rightArrowOutline != null) rightArrowOutline.enabled = false;

        // Immediately reset scales and colors so they're correct when shown again
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

    /// <summary>
    /// Sets the visibility of the arrow UI.
    /// Uses CanvasGroup to hide/show without deactivating the GameObject.
    /// This ensures GetComponentInChildren can still find this component.
    /// </summary>
    private void SetVisible(bool visible)
    {
        _isVisible = visible;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        // Don't deactivate the GameObject - this would prevent GetComponentInChildren from finding it
        // and would break the RotatableStatue's ability to find the arrowsUI
        // gameObject.SetActive(visible); // REMOVED
    }

    /// <summary>
    /// Sets the camera for billboarding.
    /// </summary>
    public void SetTargetCamera(Camera camera)
    {
        targetCamera = camera;

        if (arrowCanvas != null)
        {
            arrowCanvas.worldCamera = camera;
        }
    }

    #region Raycast-based hover (called from PuzzleInputHandler)

    /// <summary>
    /// Called by PuzzleInputHandler when mouse hovers over left arrow.
    /// </summary>
    public void OnLeftArrowHoverEnter()
    {
        OnArrowHoverEnter(true);
    }

    /// <summary>
    /// Called by PuzzleInputHandler when mouse stops hovering over left arrow.
    /// </summary>
    public void OnLeftArrowHoverExit()
    {
        OnArrowHoverExit(true);
    }

    /// <summary>
    /// Called by PuzzleInputHandler when mouse hovers over right arrow.
    /// </summary>
    public void OnRightArrowHoverEnter()
    {
        OnArrowHoverEnter(false);
    }

    /// <summary>
    /// Called by PuzzleInputHandler when mouse stops hovering over right arrow.
    /// </summary>
    public void OnRightArrowHoverExit()
    {
        OnArrowHoverExit(false);
    }

    /// <summary>
    /// Called by PuzzleInputHandler when left arrow is clicked via raycast.
    /// </summary>
    public void OnLeftArrowClickRaycast()
    {
        OnLeftArrowClick();
    }

    /// <summary>
    /// Called by PuzzleInputHandler when right arrow is clicked via raycast.
    /// </summary>
    public void OnRightArrowClickRaycast()
    {
        OnRightArrowClick();
    }

    #endregion
}