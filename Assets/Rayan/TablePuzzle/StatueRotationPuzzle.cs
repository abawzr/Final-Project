using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main controller for the statue rotation puzzle.
/// Manages statue spawning, win condition checking, and puzzle completion.
/// </summary>
public class StatueRotationPuzzle : MonoBehaviour
{
    [Header("Statue Configuration")]
    [SerializeField] private List<StatueConfig> statueConfigs = new List<StatueConfig>();

    [Header("Inventory Reference")]
    //[SerializeField] private PlayerInventory playerInventory;

    [Header("Puzzle Completion")]
    [SerializeField] private GameObject objectToHide;
    [SerializeField] private GameObject objectToShow;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip puzzleSolvedSound;

    [Header("Spawn Settings")]
    [SerializeField] private float delayBetweenSpawns = 0.5f;

    // Events for external systems
    public static event Action OnPuzzleSolved;

    // Track spawned statues
    private List<RotatableStatue> _spawnedStatues = new List<RotatableStatue>();
    private bool _isPuzzleSolved = false;
    private bool _isActive = false;
    private bool _isSpawning = false; // Guard against multiple spawn coroutines

    private void OnEnable()
    {
        // Subscribe to PuzzleStateListener (doesn't require modifying PuzzlePerspective)
        PuzzleStateListener.OnPuzzleEnabled += OnPuzzleActivated;
        PuzzleStateListener.OnPuzzleDisabled += OnPuzzleDeactivated;
        RotatableStatue.OnAnyStatueRotated += CheckWinCondition;
    }

    private void OnDisable()
    {
        PuzzleStateListener.OnPuzzleEnabled -= OnPuzzleActivated;
        PuzzleStateListener.OnPuzzleDisabled -= OnPuzzleDeactivated;
        RotatableStatue.OnAnyStatueRotated -= CheckWinCondition;
    }

    /// <summary>
    /// Called when the puzzle camera becomes active.
    /// Spawns all collected statues onto the table.
    /// </summary>
    private void OnPuzzleActivated()
    {
        if (_isPuzzleSolved) return;
        if (_isSpawning) return; // Don't start another spawn coroutine

        _isActive = true;
        StartCoroutine(SpawnCollectedStatues());
    }

    /// <summary>
    /// Called when exiting the puzzle view.
    /// </summary>
    private void OnPuzzleDeactivated()
    {
        _isActive = false;
    }

    /// <summary>
    /// Spawns all statues that the player has collected.
    /// </summary>
    private IEnumerator SpawnCollectedStatues()
    {
        _isSpawning = true;

        // Small delay to let camera transition complete
        yield return new WaitForSeconds(0.3f);

        foreach (StatueConfig config in statueConfigs)
        {
            // Skip if already spawned
            if (IsStatueAlreadySpawned(config)) continue;

            // Check if player has this statue in inventory
            //if (playerInventory != null && playerInventory.HasItem(config.statueItem))
            //{
            //    SpawnStatue(config);

            //    // Remove from inventory (use the item)
            //    playerInventory.UseOrDropItem(config.statueItem, true);

            //    yield return new WaitForSeconds(delayBetweenSpawns);
            //}
        }

        // Check win condition after all statues are placed
        yield return new WaitForSeconds(0.5f);
        CheckWinCondition();

        _isSpawning = false;
    }

    /// <summary>
    /// Checks if a statue has already been spawned on the table.
    /// </summary>
    private bool IsStatueAlreadySpawned(StatueConfig config)
    {
        foreach (RotatableStatue statue in _spawnedStatues)
        {
            if (statue != null && statue.ConfigName == config.statueName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Spawns a single statue at its designated table position.
    /// </summary>
    private void SpawnStatue(StatueConfig config)
    {
        if (config.statuePrefab == null || config.tablePosition == null)
        {
            Debug.LogWarning($"StatueRotationPuzzle: Missing prefab or table position for statue {config.statueName}");
            return;
        }

        // Instantiate at table position (animation will handle the drop)
        GameObject statueObj = Instantiate(
            config.statuePrefab,
            config.tablePosition.position,
            config.tablePosition.rotation,
            transform
        );

        // Setup the rotatable statue component
        RotatableStatue rotatableStatue = statueObj.GetComponent<RotatableStatue>();
        if (rotatableStatue == null)
        {
            rotatableStatue = statueObj.AddComponent<RotatableStatue>();
        }

        // Initialize the statue
        rotatableStatue.Initialize(config.statueName, config.correctRotation);
        _spawnedStatues.Add(rotatableStatue);

        // Trigger the placement animation
        Animator animator = statueObj.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Place");
            // Start a fallback coroutine in case Animation Event doesn't call OnPlacementComplete
            StartCoroutine(FallbackPlacementComplete(rotatableStatue, 2f));
        }
        else
        {
            // No animator - just mark as placed immediately
            Debug.LogWarning($"StatueRotationPuzzle: No Animator for {config.statueName}. Marking as placed immediately.");
            rotatableStatue.OnPlacementComplete();
        }
    }

    /// <summary>
    /// Fallback to mark statue as placed if Animation Event doesn't fire.
    /// </summary>
    private IEnumerator FallbackPlacementComplete(RotatableStatue statue, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Only call if not already placed
        if (statue != null && !statue.IsPlaced)
        {
            Debug.LogWarning($"StatueRotationPuzzle: Animation Event 'OnPlacementComplete' not called for {statue.ConfigName}. Using fallback.");
            statue.OnPlacementComplete();
        }
    }

    /// <summary>
    /// Checks if all statues are placed and at correct rotations.
    /// </summary>
    private void CheckWinCondition()
    {
        if (_isPuzzleSolved) return;

        // Must have all 4 statues placed
        if (_spawnedStatues.Count < statueConfigs.Count)
        {
            return;
        }

        // Check if all statues are at correct rotation
        foreach (RotatableStatue statue in _spawnedStatues)
        {
            if (statue == null || !statue.IsAtCorrectRotation())
            {
                return;
            }
        }

        // All conditions met - puzzle solved!
        OnPuzzleSolved_Internal();
    }

    /// <summary>
    /// Handles puzzle completion.
    /// </summary>
    private void OnPuzzleSolved_Internal()
    {
        _isPuzzleSolved = true;

        // Hide/show objects
        if (objectToHide != null)
        {
            objectToHide.SetActive(false);
        }

        if (objectToShow != null)
        {
            objectToShow.SetActive(true);
        }

        // Play sound
        if (audioSource != null && puzzleSolvedSound != null)
        {
            audioSource.PlayOneShot(puzzleSolvedSound);
        }

        // Disable interaction on all statues
        foreach (RotatableStatue statue in _spawnedStatues)
        {
            if (statue != null)
            {
                statue.SetInteractable(false);
            }
        }

        // Fire event
        OnPuzzleSolved?.Invoke();

        Debug.Log("Puzzle Solved!");
    }

    /// <summary>
    /// Returns true if the puzzle is currently active (in puzzle view).
    /// </summary>
    public bool IsActive => _isActive;

    /// <summary>
    /// Returns true if the puzzle has been solved.
    /// </summary>
    public bool IsSolved => _isPuzzleSolved;

    /// <summary>
    /// Gets the number of statues currently placed on the table.
    /// </summary>
    public int PlacedStatueCount => _spawnedStatues.Count;
}

/// <summary>
/// Configuration for a single statue in the puzzle.
/// </summary>
[System.Serializable]
public class StatueConfig
{
    [Tooltip("Name for identification")]
    public string statueName;

    [Tooltip("Reference to the inventory item")]
    //public ItemSO statueItem;

    //[Tooltip("The statue prefab to spawn")]
    public GameObject statuePrefab;

    [Tooltip("Fixed position on the table where this statue spawns")]
    public Transform tablePosition;

    [Tooltip("Target Y rotation (0, 45, 90, 135, 180, 225, 270, 315)")]
    [Range(0f, 315f)]
    public float correctRotation;
}