using UnityEngine;

/// <summary>
/// Handles mouse input for the puzzle, including raycasting for hover and click detection.
/// Should be placed in the puzzle scene and only active during puzzle mode.
/// </summary>
public class PuzzleInputHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Camera puzzleCamera;
    [SerializeField] private LayerMask interactableLayers = ~0; // Default to Everything
    [SerializeField] private float raycastDistance = 100f;

    [Header("Debug")]
    [SerializeField] private bool showDebugRay = false;

    // Currently hovered objects
    private RotatableStatue _hoveredStatue;
    private RecordPlayerButton _hoveredButton;
    private StatueArrowsUI _hoveredArrowsUI;
    private bool _isLeftArrowHovered;
    private bool _isRightArrowHovered;

    // Cached references
    private GameObject _lastHitObject;

    // State
    private bool _isActive = false;

    private void Start()
    {
        // Warn if no camera assigned
        if (puzzleCamera == null)
        {
            Debug.LogWarning("PuzzleInputHandler: No puzzle camera assigned. Will try to use Camera.main as fallback, but this may not work correctly with Cinemachine.");
        }
    }

    private void OnEnable()
    {
        // Subscribe to PuzzleStateListener (doesn't require modifying PuzzlePerspective)
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

        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnPuzzleDisabled()
    {
        _isActive = false;

        // Clear all hovers
        ClearAllHovers();

        // Hide cursor (let other systems handle this)
    }

    private void Update()
    {
        if (!_isActive) return;

        // Get the camera to use
        Camera cam = puzzleCamera != null ? puzzleCamera : Camera.main;
        if (cam == null) return;

        // Perform raycast
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.yellow);
        }

        RaycastHit hit;
        bool didHit = Physics.Raycast(ray, out hit, raycastDistance, interactableLayers);

        if (didHit)
        {
            HandleHit(hit);
        }
        else
        {
            // Nothing hit - clear all hovers
            ClearAllHovers();
        }

        // Handle click input
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    /// <summary>
    /// Handles a raycast hit.
    /// </summary>
    private void HandleHit(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;

        // Check if we're still hovering the same object
        if (hitObject == _lastHitObject) return;

        // Clear previous hovers
        ClearAllHovers();

        _lastHitObject = hitObject;

        // Check for rotation arrow (using RotationArrow component - no tags needed)
        RotationArrow arrow = hitObject.GetComponent<RotationArrow>();
        if (arrow == null)
        {
            arrow = hitObject.GetComponentInParent<RotationArrow>();
        }

        if (arrow != null)
        {
            _hoveredArrowsUI = arrow.GetArrowsUI();
            if (_hoveredArrowsUI != null)
            {
                if (arrow.IsLeftArrow)
                {
                    _isLeftArrowHovered = true;
                    _hoveredArrowsUI.OnLeftArrowHoverEnter();
                }
                else
                {
                    _isRightArrowHovered = true;
                    _hoveredArrowsUI.OnRightArrowHoverEnter();
                }
                return;
            }
        }

        // Check for statue
        RotatableStatue statue = hitObject.GetComponent<RotatableStatue>();
        if (statue == null)
        {
            statue = hitObject.GetComponentInParent<RotatableStatue>();
        }

        if (statue != null)
        {
            _hoveredStatue = statue;
            _hoveredStatue.OnHoverEnter();
            return;
        }

        // Check for record player button
        RecordPlayerButton button = hitObject.GetComponent<RecordPlayerButton>();
        if (button == null)
        {
            button = hitObject.GetComponentInParent<RecordPlayerButton>();
        }

        if (button != null)
        {
            _hoveredButton = button;
            _hoveredButton.OnHoverEnter();
            return;
        }
    }

    /// <summary>
    /// Clears all current hover states.
    /// </summary>
    private void ClearAllHovers()
    {
        if (_hoveredStatue != null)
        {
            _hoveredStatue.OnHoverExit();
            _hoveredStatue = null;
        }

        if (_hoveredButton != null)
        {
            _hoveredButton.OnHoverExit();
            _hoveredButton = null;
        }

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

        _isLeftArrowHovered = false;
        _isRightArrowHovered = false;
        _lastHitObject = null;
    }

    /// <summary>
    /// Handles mouse click.
    /// </summary>
    private void HandleClick()
    {
        // Click on button
        if (_hoveredButton != null)
        {
            _hoveredButton.OnClickRaycast();
            return;
        }

        // Click on arrow
        if (_hoveredArrowsUI != null)
        {
            if (_isLeftArrowHovered)
            {
                _hoveredArrowsUI.OnLeftArrowClickRaycast();
            }
            else if (_isRightArrowHovered)
            {
                _hoveredArrowsUI.OnRightArrowClickRaycast();
            }
        }
    }

    /// <summary>
    /// Sets the puzzle camera reference.
    /// </summary>
    public void SetPuzzleCamera(Camera camera)
    {
        puzzleCamera = camera;
    }

    /// <summary>
    /// Manually activates the input handler.
    /// </summary>
    public void Activate()
    {
        _isActive = true;
    }

    /// <summary>
    /// Manually deactivates the input handler.
    /// </summary>
    public void Deactivate()
    {
        _isActive = false;
        ClearAllHovers();
    }
}