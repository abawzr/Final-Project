using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles mouse input for the puzzle, including raycasting for hover and click detection.
/// v5 - Fixes:
///      - Now checks return value of OnHoverEnter() before storing hover reference
///      - Improved null safety
///      - Better state management when hover is rejected
/// 
/// KEY FEATURES:
/// - Uses RaycastAll to detect overlapping colliders (arrows + statue)
/// - Prioritizes arrows over statues even when statue is hit first
/// - Hovering an arrow also triggers its parent statue's hover effect
/// - Supports optional separate layer for arrows (recommended)
/// </summary>
public class PuzzleInputHandler : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera puzzleCamera;

    [Header("Layer Settings")]
    [Tooltip("Layer mask for all interactable objects (statues, buttons, arrows)")]
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Tooltip("Optional: Separate layer for arrows. If set, arrows on this layer get priority.")]
    [SerializeField] private LayerMask arrowLayer;

    [Tooltip("If true, raycast arrows layer separately for guaranteed priority")]
    [SerializeField] private bool useSeparateArrowLayer = false;

    [Header("Raycast Settings")]
    [SerializeField] private float raycastDistance = 100f;
    [SerializeField] private int maxRaycastHits = 10;

    // Currently hovered objects
    private RotatableStatue _hoveredStatue;
    private RecordPlayerButton _hoveredButton;
    private StatueArrowsUI _hoveredArrowsUI;
    private RotationArrow _hoveredArrow;
    private bool _isLeftArrowHovered;
    private bool _isRightArrowHovered;

    // Raycast hit buffer (reused to avoid allocation)
    private RaycastHit[] _hitBuffer;

    // State
    private bool _isActive = false;

    private void Awake()
    {
        _hitBuffer = new RaycastHit[maxRaycastHits];
    }

    private void Start()
    {
        if (puzzleCamera == null)
        {
            GameObject puzzleCamObj = GameObject.Find("PuzzleCamera");
            if (puzzleCamObj != null)
            {
                puzzleCamera = puzzleCamObj.GetComponent<Camera>();
            }
        }
    }

    private void OnEnable()
    {
        PuzzleStateListener.OnPuzzleEnabled += OnPuzzleEnabled;
        PuzzleStateListener.OnPuzzleDisabled += OnPuzzleDisabled;
    }

    private void OnDisable()
    {
        PuzzleStateListener.OnPuzzleEnabled -= OnPuzzleEnabled;
        PuzzleStateListener.OnPuzzleDisabled -= OnPuzzleDisabled;
    }

    private void OnPuzzleEnabled()
    {
        _isActive = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnPuzzleDisabled()
    {
        _isActive = false;
        ClearAllHovers();
    }

    private void Update()
    {
        if (!_isActive) return;

        Camera cam = puzzleCamera != null ? puzzleCamera : Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Perform raycast and process results
        ProcessRaycast(ray);

        // Handle click input
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    /// <summary>
    /// Performs raycast using RaycastAll to detect all overlapping objects.
    /// Prioritizes: Arrows > Buttons > Statues
    /// </summary>
    private void ProcessRaycast(Ray ray)
    {
        RotationArrow foundArrow = null;
        RecordPlayerButton foundButton = null;
        RotatableStatue foundStatue = null;

        // Option 1: Use separate arrow layer for guaranteed priority
        if (useSeparateArrowLayer && arrowLayer != 0)
        {
            int arrowHitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, raycastDistance, arrowLayer);

            for (int i = 0; i < arrowHitCount; i++)
            {
                RotationArrow arrow = GetArrowFromHit(_hitBuffer[i]);
                if (arrow != null)
                {
                    foundArrow = arrow;
                    break;
                }
            }
        }

        // Then check all interactable layers
        int hitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, raycastDistance, interactableLayers);

        // Sort hits by distance (closest first)
        SortHitsByDistance(hitCount);

        // Process all hits to find the best match for each type
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _hitBuffer[i];
            GameObject hitObject = hit.collider.gameObject;

            // Check for arrow (highest priority)
            if (foundArrow == null)
            {
                RotationArrow arrow = GetArrowFromHit(hit);
                if (arrow != null)
                {
                    foundArrow = arrow;
                    continue;
                }
            }

            // Check for button (second priority)
            if (foundButton == null)
            {
                RecordPlayerButton button = hitObject.GetComponent<RecordPlayerButton>();
                if (button == null)
                {
                    button = hitObject.GetComponentInParent<RecordPlayerButton>();
                }
                if (button != null)
                {
                    foundButton = button;
                    continue;
                }
            }

            // Check for statue (lowest priority)
            if (foundStatue == null)
            {
                RotatableStatue statue = hitObject.GetComponent<RotatableStatue>();
                if (statue == null)
                {
                    statue = hitObject.GetComponentInParent<RotatableStatue>();
                }
                if (statue != null)
                {
                    foundStatue = statue;
                    continue;
                }
            }
        }

        // Apply hover states based on priority
        ApplyHoverStates(foundArrow, foundButton, foundStatue);
    }

    /// <summary>
    /// Gets a RotationArrow component from a raycast hit.
    /// </summary>
    private RotationArrow GetArrowFromHit(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;

        RotationArrow arrow = hitObject.GetComponent<RotationArrow>();
        if (arrow == null)
        {
            arrow = hitObject.GetComponentInParent<RotationArrow>();
        }

        return arrow;
    }

    /// <summary>
    /// Simple bubble sort for raycast hits by distance.
    /// </summary>
    private void SortHitsByDistance(int count)
    {
        for (int i = 0; i < count - 1; i++)
        {
            for (int j = 0; j < count - i - 1; j++)
            {
                if (_hitBuffer[j].distance > _hitBuffer[j + 1].distance)
                {
                    RaycastHit temp = _hitBuffer[j];
                    _hitBuffer[j] = _hitBuffer[j + 1];
                    _hitBuffer[j + 1] = temp;
                }
            }
        }
    }

    /// <summary>
    /// Applies hover states based on what was found.
    /// KEY FIX: Now checks return value of OnHoverEnter() before storing reference.
    /// </summary>
    private void ApplyHoverStates(RotationArrow foundArrow, RecordPlayerButton foundButton, RotatableStatue foundStatue)
    {
        // PRIORITY 1: Arrow hover (also triggers statue hover)
        if (foundArrow != null)
        {
            StatueArrowsUI arrowsUI = foundArrow.GetArrowsUI();

            if (arrowsUI != null)
            {
                // KEY FIX: Get the statue that owns this arrow and trigger its hover
                RotatableStatue ownerStatue = foundArrow.GetOwnerStatue();

                // Clear other hovers
                ClearButtonHover();

                // Handle statue hover (this shows the outline and keeps arrows visible)
                if (ownerStatue != null)
                {
                    if (_hoveredStatue != ownerStatue)
                    {
                        ClearStatueHover();

                        // FIX: Only store reference if hover was successfully activated
                        if (ownerStatue.OnHoverEnter())
                        {
                            _hoveredStatue = ownerStatue;
                        }
                    }
                }

                // Handle arrow-specific hover (highlight the specific arrow)
                HandleArrowHover(foundArrow, arrowsUI);

                return;
            }
        }

        // PRIORITY 2: Button hover
        if (foundButton != null)
        {
            ClearArrowHover();
            ClearStatueHover();

            if (_hoveredButton != foundButton)
            {
                ClearButtonHover();
                _hoveredButton = foundButton;
                _hoveredButton.OnHoverEnter();
            }
            return;
        }

        // PRIORITY 3: Statue hover (direct statue hover, not via arrow)
        if (foundStatue != null)
        {
            ClearArrowHover();
            ClearButtonHover();

            if (_hoveredStatue != foundStatue)
            {
                ClearStatueHover();

                // FIX: Only store reference if hover was successfully activated
                if (foundStatue.OnHoverEnter())
                {
                    _hoveredStatue = foundStatue;
                }
            }
            return;
        }

        // Nothing hit - clear all hovers
        ClearAllHovers();
    }

    /// <summary>
    /// Handles arrow-specific hover with proper left/right tracking.
    /// </summary>
    private void HandleArrowHover(RotationArrow arrow, StatueArrowsUI arrowsUI)
    {
        // Check if arrows are actually visible/interactable
        if (!arrowsUI.IsVisible)
        {
            return;
        }

        bool isLeft = arrow.IsLeftArrow;

        // Check if we're already hovering this specific arrow
        if (_hoveredArrow == arrow)
        {
            return;
        }

        // Clear previous arrow hover if it's a different UI
        if (_hoveredArrowsUI != null && _hoveredArrowsUI != arrowsUI)
        {
            ClearArrowHoverOnly();
        }
        else if (_hoveredArrowsUI == arrowsUI)
        {
            // Same UI, different arrow - just update the hover state
            if (_isLeftArrowHovered && !isLeft)
            {
                _hoveredArrowsUI.OnLeftArrowHoverExit();
                _isLeftArrowHovered = false;
            }
            else if (_isRightArrowHovered && isLeft)
            {
                _hoveredArrowsUI.OnRightArrowHoverExit();
                _isRightArrowHovered = false;
            }
        }

        _hoveredArrowsUI = arrowsUI;
        _hoveredArrow = arrow;

        if (isLeft)
        {
            _isLeftArrowHovered = true;
            _isRightArrowHovered = false;
            _hoveredArrowsUI.OnLeftArrowHoverEnter();
        }
        else
        {
            _isRightArrowHovered = true;
            _isLeftArrowHovered = false;
            _hoveredArrowsUI.OnRightArrowHoverEnter();
        }
    }

    /// <summary>
    /// Clears statue hover state.
    /// </summary>
    private void ClearStatueHover()
    {
        if (_hoveredStatue != null)
        {
            _hoveredStatue.OnHoverExit();
            _hoveredStatue = null;
        }
    }

    /// <summary>
    /// Clears button hover state.
    /// </summary>
    private void ClearButtonHover()
    {
        if (_hoveredButton != null)
        {
            _hoveredButton.OnHoverExit();
            _hoveredButton = null;
        }
    }

    /// <summary>
    /// Clears arrow hover state only (not statue).
    /// </summary>
    private void ClearArrowHoverOnly()
    {
        if (_hoveredArrowsUI != null)
        {
            if (_isLeftArrowHovered)
            {
                _hoveredArrowsUI.OnLeftArrowHoverExit();
            }
            if (_isRightArrowHovered)
            {
                _hoveredArrowsUI.OnRightArrowHoverExit();
            }
            _hoveredArrowsUI = null;
        }

        _hoveredArrow = null;
        _isLeftArrowHovered = false;
        _isRightArrowHovered = false;
    }

    /// <summary>
    /// Clears arrow hover state (and statue if not hovering arrow).
    /// </summary>
    private void ClearArrowHover()
    {
        ClearArrowHoverOnly();
    }

    /// <summary>
    /// Clears all hover states.
    /// </summary>
    private void ClearAllHovers()
    {
        ClearArrowHoverOnly();
        ClearButtonHover();
        ClearStatueHover();
    }

    /// <summary>
    /// Handles mouse click based on current hover state.
    /// </summary>
    private void HandleClick()
    {
        // Click on arrow (highest priority)
        if (_hoveredArrowsUI != null)
        {
            // Extra safety check - ensure arrows are still visible
            if (!_hoveredArrowsUI.IsVisible)
            {
                ClearArrowHoverOnly();
                return;
            }

            if (_isLeftArrowHovered)
            {
                _hoveredArrowsUI.OnLeftArrowClickRaycast();
            }
            else if (_isRightArrowHovered)
            {
                _hoveredArrowsUI.OnRightArrowClickRaycast();
            }
            return;
        }

        // Click on button
        if (_hoveredButton != null)
        {
            _hoveredButton.OnClickRaycast();
            return;
        }
    }

    #region Public Methods

    public void SetPuzzleCamera(Camera camera)
    {
        puzzleCamera = camera;
    }

    public void Activate()
    {
        _isActive = true;
    }

    public void Deactivate()
    {
        _isActive = false;
        ClearAllHovers();
    }

    /// <summary>
    /// Returns true if currently hovering any arrow.
    /// </summary>
    public bool IsHoveringArrow => _hoveredArrowsUI != null && _hoveredArrowsUI.IsVisible;

    /// <summary>
    /// Returns true if currently hovering any statue.
    /// </summary>
    public bool IsHoveringStatue => _hoveredStatue != null;

    /// <summary>
    /// Returns the currently hovered statue (if any).
    /// </summary>
    public RotatableStatue HoveredStatue => _hoveredStatue;

    #endregion
}