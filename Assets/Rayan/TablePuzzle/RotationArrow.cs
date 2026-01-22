using UnityEngine;

/// <summary>
/// Simple component to identify rotation arrows.
/// Attach to each arrow object (left and right) in the statue prefab.
/// No tags or layers required.
/// </summary>
public class RotationArrow : MonoBehaviour
{
    [SerializeField] private bool isLeftArrow = false;

    /// <summary>
    /// Returns true if this is the left (counter-clockwise) arrow.
    /// </summary>
    public bool IsLeftArrow => isLeftArrow;

    /// <summary>
    /// Returns true if this is the right (clockwise) arrow.
    /// </summary>
    public bool IsRightArrow => !isLeftArrow;

    /// <summary>
    /// Gets the parent StatueArrowsUI component.
    /// Uses includeInactive to find it even when hidden.
    /// </summary>
    public StatueArrowsUI GetArrowsUI()
    {
        // First try normal GetComponentInParent
        StatueArrowsUI ui = GetComponentInParent<StatueArrowsUI>();

        // If not found, search upward manually (in case parent is inactive)
        if (ui == null)
        {
            Transform current = transform.parent;
            while (current != null)
            {
                ui = current.GetComponent<StatueArrowsUI>();
                if (ui != null) break;
                current = current.parent;
            }
        }

        return ui;
    }
}