using UnityEngine;

/// <summary>
/// Component to identify rotation arrows and provide easy access to parent UI.
/// v3 - Fixes:
///      - Fixed caching logic: only sets _hasCached flag when valid reference is found
///      - Added ability to retry caching on null result
///      - Added public IsCached properties for debugging
/// 
/// SETUP:
/// 1. Attach this component to each arrow GameObject (left and right)
/// 2. Set isLeftArrow to true for the left/counter-clockwise arrow
/// 3. Ensure the arrow has a Collider component for raycast detection
/// 4. (Optional) Put arrows on a separate layer for better raycast priority
/// </summary>
[RequireComponent(typeof(Collider))]
public class RotationArrow : MonoBehaviour
{
    [Header("Arrow Configuration")]
    [Tooltip("Set to true for the left (counter-clockwise) arrow, false for right (clockwise)")]
    [SerializeField] private bool isLeftArrow = false;

    [Header("Optional Layer Setup")]
    [Tooltip("If set, the arrow will be moved to this layer on Start")]
    [SerializeField] private string arrowLayerName = "";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    // Cached reference to parent StatueArrowsUI
    private StatueArrowsUI _cachedArrowsUI;
    private bool _hasCachedUI = false;

    // Cached reference to parent RotatableStatue
    private RotatableStatue _cachedStatue;
    private bool _hasCachedStatue = false;

    /// <summary>
    /// Returns true if this is the left (counter-clockwise) arrow.
    /// </summary>
    public bool IsLeftArrow => isLeftArrow;

    /// <summary>
    /// Returns true if this is the right (clockwise) arrow.
    /// </summary>
    public bool IsRightArrow => !isLeftArrow;

    /// <summary>
    /// Returns true if ArrowsUI reference has been successfully cached.
    /// </summary>
    public bool HasCachedArrowsUI => _hasCachedUI && _cachedArrowsUI != null;

    /// <summary>
    /// Returns true if Statue reference has been successfully cached.
    /// </summary>
    public bool HasCachedStatue => _hasCachedStatue && _cachedStatue != null;

    private void Start()
    {
        // Pre-cache references
        GetArrowsUI();
        GetOwnerStatue();

        // Optional layer setup
        SetupLayer();

        // Validate setup
        ValidateSetup();
    }

    /// <summary>
    /// Gets the parent StatueArrowsUI component.
    /// FIX: Only caches when a valid reference is found; retries on null.
    /// </summary>
    public StatueArrowsUI GetArrowsUI()
    {
        // FIX: Only return cached value if it's valid
        // If we cached null previously, try again
        if (_hasCachedUI && _cachedArrowsUI != null)
        {
            return _cachedArrowsUI;
        }

        // Try GetComponentInParent first (works for active hierarchies)
        _cachedArrowsUI = GetComponentInParent<StatueArrowsUI>();

        // If not found, search upward manually (handles inactive parents)
        if (_cachedArrowsUI == null)
        {
            Transform current = transform.parent;
            while (current != null)
            {
                _cachedArrowsUI = current.GetComponent<StatueArrowsUI>();
                if (_cachedArrowsUI != null) break;
                current = current.parent;
            }
        }

        // FIX: Only set cached flag if we found a valid reference
        if (_cachedArrowsUI != null)
        {
            _hasCachedUI = true;
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"[RotationArrow] {name}: Could not find StatueArrowsUI in parent hierarchy!");
        }

        return _cachedArrowsUI;
    }

    /// <summary>
    /// Gets the RotatableStatue that owns this arrow.
    /// FIX: Only caches when a valid reference is found; retries on null.
    /// </summary>
    public RotatableStatue GetOwnerStatue()
    {
        // FIX: Only return cached value if it's valid
        // If we cached null previously, try again
        if (_hasCachedStatue && _cachedStatue != null)
        {
            return _cachedStatue;
        }

        _cachedStatue = GetComponentInParent<RotatableStatue>();

        // If not found, search upward manually
        if (_cachedStatue == null)
        {
            Transform current = transform.parent;
            while (current != null)
            {
                _cachedStatue = current.GetComponent<RotatableStatue>();
                if (_cachedStatue != null) break;
                current = current.parent;
            }
        }

        // FIX: Only set cached flag if we found a valid reference
        if (_cachedStatue != null)
        {
            _hasCachedStatue = true;
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"[RotationArrow] {name}: Could not find RotatableStatue in parent hierarchy!");
        }

        return _cachedStatue;
    }

    /// <summary>
    /// Sets up the layer if specified.
    /// </summary>
    private void SetupLayer()
    {
        if (string.IsNullOrEmpty(arrowLayerName)) return;

        int layer = LayerMask.NameToLayer(arrowLayerName);

        if (layer == -1)
        {
            Debug.LogWarning($"[RotationArrow] {name}: Layer '{arrowLayerName}' does not exist. Please create it in Tags & Layers.");
            return;
        }

        gameObject.layer = layer;

        if (enableDebugLogs)
        {
            Debug.Log($"[RotationArrow] {name}: Set to layer '{arrowLayerName}' ({layer})");
        }
    }

    /// <summary>
    /// Validates the arrow setup and logs warnings for common issues.
    /// </summary>
    private void ValidateSetup()
    {
        // Check for collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[RotationArrow] {name}: Missing Collider component! Arrow won't be clickable.");
        }
        else if (!col.enabled)
        {
            Debug.LogWarning($"[RotationArrow] {name}: Collider is disabled. Arrow won't be clickable.");
        }

        // Check for StatueArrowsUI
        if (_cachedArrowsUI == null)
        {
            Debug.LogError($"[RotationArrow] {name}: No StatueArrowsUI found in parent hierarchy!");
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[RotationArrow] {name}: Setup complete. IsLeft={isLeftArrow}, ArrowsUI={_cachedArrowsUI != null}, Statue={_cachedStatue?.ConfigName ?? "none"}");
        }
    }

    /// <summary>
    /// Clears cached references. Useful if hierarchy changes at runtime.
    /// </summary>
    public void ClearCache()
    {
        _cachedArrowsUI = null;
        _hasCachedUI = false;
        _cachedStatue = null;
        _hasCachedStatue = false;
    }

    /// <summary>
    /// Forces a refresh of all cached references.
    /// </summary>
    public void RefreshCache()
    {
        ClearCache();
        GetArrowsUI();
        GetOwnerStatue();
    }

    #region Editor Helpers

#if UNITY_EDITOR
    /// <summary>
    /// Draws a gizmo to help identify arrows in the scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Draw arrow direction indicator
        Gizmos.color = isLeftArrow ? Color.yellow : Color.cyan;

        Vector3 center = transform.position;
        float size = 0.1f;

        // Draw a small sphere at the arrow position
        Gizmos.DrawWireSphere(center, size);

        // Draw direction arrow
        Vector3 direction = isLeftArrow ? -transform.right : transform.right;
        Gizmos.DrawRay(center, direction * size * 2);
    }

    /// <summary>
    /// Draws a label in scene view showing arrow type.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.2f,
            isLeftArrow ? "? LEFT (CCW)" : "? RIGHT (CW)");
    }
#endif

    #endregion
}