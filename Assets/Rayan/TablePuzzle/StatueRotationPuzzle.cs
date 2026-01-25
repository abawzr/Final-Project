using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main controller for the statue rotation puzzle.
/// v6 - Fixes:
///      - Improved win condition check timing (waits for ALL placements to complete)
///      - Added null safety in win condition check
///      - Better debug logging with consistent format
/// </summary>
public class StatueRotationPuzzle : MonoBehaviour
{
    [Header("Statue Configuration")]
    [SerializeField] private List<StatueConfig> statueConfigs = new List<StatueConfig>();

    [Header("Inventory Reference")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Puzzle Completion")]
    [SerializeField] private GameObject objectToHide;
    [SerializeField] private GameObject objectToShow;

    [Header("Table Interaction")]
    [Tooltip("Reference to PuzzlePerspective to disable interaction after puzzle is solved")]
    [SerializeField] private PuzzlePerspective puzzlePerspective;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip puzzleSolvedSound;

    [Header("Spawn Settings")]
    [SerializeField] private float delayBetweenSpawns = 0.5f;
    [SerializeField] private float initialSpawnDelay = 0.3f;
    [SerializeField] private float postSpawnCheckDelay = 0.5f;

    [Header("Drop Animation")]
    [Tooltip("Enable code-based drop animation")]
    [SerializeField] private bool useDropAnimation = true;

    [Tooltip("Height above table to start the drop")]
    [SerializeField] private float dropHeight = 0.5f;

    [Tooltip("Duration of drop animation in seconds")]
    [SerializeField] private float dropDuration = 0.3f;

    // Events
    public static event Action OnPuzzleSolved;

    // Track spawned statues
    private List<RotatableStatue> _spawnedStatues = new List<RotatableStatue>();
    private bool _isPuzzleSolved = false;
    private bool _isActive = false;
    private bool _isSpawning = false;
    private Coroutine _spawnCoroutine;

    // Track pending drop animations
    private int _pendingDropAnimations = 0;

    // Track if we're quitting to know when to clean up static events
    private static bool _isApplicationQuitting = false;

    #region Unity Lifecycle

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }
    }

    private void OnEnable()
    {
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

    private void OnDestroy()
    {
        // Stop any running coroutines
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        // Only clean up static event when application is quitting
        // This prevents breaking other listeners during scene transitions
        if (_isApplicationQuitting)
        {
            OnPuzzleSolved = null;
        }
    }

    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }

    #endregion


    #region Puzzle State

    private void OnPuzzleActivated()
    {

        if (_isPuzzleSolved)
        {
            return;
        }

        if (_isSpawning)
        {
            return;
        }

        _isActive = true;
        _spawnCoroutine = StartCoroutine(SpawnCollectedStatues());
    }

    private void OnPuzzleDeactivated()
    {
        _isActive = false;
    }

    #endregion

    #region Statue Spawning

    /// <summary>
    /// Spawns statues for items in the player's inventory.
    /// </summary>
    private IEnumerator SpawnCollectedStatues()
    {
        _isSpawning = true;
        _pendingDropAnimations = 0;

        // Wait for puzzle to fully activate
        yield return new WaitForSeconds(initialSpawnDelay);

        if (playerInventory == null)
        {
            _isSpawning = false;
            yield break;
        }


        int spawnedCount = 0;

        foreach (StatueConfig config in statueConfigs)
        {
            // Skip if already spawned
            if (IsStatueAlreadySpawned(config))
            {
                continue;
            }

            // Skip invalid configs
            if (config.statueItem == null || config.statuePrefab == null || config.tablePosition == null)
            {
                continue;
            }

            // Check if player has the item
            bool hasItem = playerInventory.HasItem(config.statueItem);

            if (hasItem)
            {
                SpawnStatue(config);
                playerInventory.UseOrDropItem(config.statueItem, true);
                spawnedCount++;

                yield return new WaitForSeconds(delayBetweenSpawns);
            }
        }

        // FIX: Wait for ALL drop animations to complete before checking win condition
        // This ensures IsAtCorrectRotation() returns accurate results
        if (_pendingDropAnimations > 0)
        {
            // Wait until all animations are done
            while (_pendingDropAnimations > 0)
            {
                yield return null;
            }

            // Small additional delay for safety
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            // If no drop animations, still wait the configured delay
            yield return new WaitForSeconds(postSpawnCheckDelay);
        }

        CheckWinCondition();

        _isSpawning = false;
        _spawnCoroutine = null;
    }

    /// <summary>
    /// Checks if a statue with the given config name is already spawned.
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
    /// Spawns a single statue at the configured position.
    /// </summary>
    private void SpawnStatue(StatueConfig config)
    {
        // Get the final position on the table
        Vector3 finalPosition = config.tablePosition.position;
        Quaternion finalRotation = config.tablePosition.rotation;

        // Calculate start position (above the table for drop animation)
        Vector3 startPosition = useDropAnimation
            ? finalPosition + Vector3.up * dropHeight
            : finalPosition;

        // Instantiate WITHOUT parent to avoid scale/position inheritance issues
        GameObject statueObj = Instantiate(config.statuePrefab, startPosition, finalRotation);
        statueObj.name = $"{config.statueName}_Table";

        // Ensure the statue has the correct scale from the prefab
        statueObj.transform.localScale = config.statuePrefab.transform.localScale;

        // Disable any Animator to prevent it from overriding position
        Animator animator = statueObj.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        // Setup RotatableStatue component
        RotatableStatue rotatableStatue = statueObj.GetComponent<RotatableStatue>();
        if (rotatableStatue == null)
        {
            rotatableStatue = statueObj.AddComponent<RotatableStatue>();
        }

        rotatableStatue.Initialize(config.statueName, config.correctRotation);
        _spawnedStatues.Add(rotatableStatue);

        // Start drop animation
        if (useDropAnimation)
        {
            _pendingDropAnimations++;
            StartCoroutine(DropAnimation(statueObj.transform, startPosition, finalPosition, rotatableStatue));
        }
        else
        {
            rotatableStatue.OnPlacementComplete();
        }
    }

    /// <summary>
    /// Animates a statue dropping from start to end position.
    /// </summary>
    private IEnumerator DropAnimation(Transform statueTransform, Vector3 startPos, Vector3 endPos, RotatableStatue statue)
    {
        float elapsed = 0f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);

            // Ease out cubic (decelerate at the end for natural landing)
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            statueTransform.position = Vector3.Lerp(startPos, endPos, easedT);

            yield return null;
        }

        // Snap to final position
        statueTransform.position = endPos;

        // Mark placement complete
        if (statue != null)
        {
            statue.OnPlacementComplete();
        }

        // Decrement pending counter
        _pendingDropAnimations--;
    }

    #endregion

    #region Win Condition

    /// <summary>
    /// Checks if all statues are placed and at the correct rotation.
    /// </summary>
    private void CheckWinCondition()
    {
        if (_isPuzzleSolved) return;

        // Clean up null references from destroyed statues
        _spawnedStatues.RemoveAll(s => s == null);

        // Check if all required statues are placed
        if (_spawnedStatues.Count < statueConfigs.Count)
        {
            return;
        }

        // Check if all statues are at correct rotation
        // Note: IsAtCorrectRotation() now checks _isPlaced internally
        foreach (RotatableStatue statue in _spawnedStatues)
        {
            // Extra null safety
            if (statue == null) continue;

            if (!statue.IsAtCorrectRotation())
            {
                return;
            }
        }

        // All conditions met - puzzle solved!
        OnPuzzleSolvedInternal();
    }

    /// <summary>
    /// Handles puzzle completion.
    /// </summary>
    private void OnPuzzleSolvedInternal()
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

        // Disable table interaction (player can't press E on the table anymore)
        if (puzzlePerspective != null)
        {
            puzzlePerspective.CanInteract = false;
            puzzlePerspective.DisablePuzzle();
            LabDoor.IsPuzzle3Solved = true;
        }

        // Fire event (RecordPlayerController listens to this to disable record player)
        OnPuzzleSolved?.Invoke();
    }

    #endregion

    #region Public Properties

    /// <summary>Returns true if the puzzle is currently active.</summary>
    public bool IsActive => _isActive;

    /// <summary>Returns true if the puzzle has been solved.</summary>
    public bool IsSolved => _isPuzzleSolved;

    /// <summary>Returns the number of statues currently placed on the table.</summary>
    public int PlacedStatueCount => _spawnedStatues.Count;

    /// <summary>Returns the total number of statues required to solve the puzzle.</summary>
    public int RequiredStatueCount => statueConfigs.Count;

    /// <summary>Returns a read-only list of spawned statues.</summary>
    public IReadOnlyList<RotatableStatue> SpawnedStatues => _spawnedStatues.AsReadOnly();

    /// <summary>Returns true if currently spawning statues.</summary>
    public bool IsSpawning => _isSpawning;

    #endregion

    #region Public Methods

    /// <summary>
    /// Manually resets the puzzle. Destroys all spawned statues.
    /// </summary>
    public void ResetPuzzle()
    {

        // Stop any spawning in progress
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        // Destroy all spawned statues
        foreach (RotatableStatue statue in _spawnedStatues)
        {
            if (statue != null)
            {
                Destroy(statue.gameObject);
            }
        }
        _spawnedStatues.Clear();

        // Reset state
        _isPuzzleSolved = false;
        _isSpawning = false;
        _pendingDropAnimations = 0;

        // Reset visuals
        if (objectToHide != null) objectToHide.SetActive(true);
        if (objectToShow != null) objectToShow.SetActive(false);

    }

    #endregion
}

/// <summary>
/// Configuration for a single statue in the puzzle.
/// </summary>
[System.Serializable]
public class StatueConfig
{
    [Tooltip("Name for identification (must be unique)")]
    public string statueName;

    [Tooltip("Reference to the inventory item that unlocks this statue")]
    public ItemSO statueItem;

    [Tooltip("The statue prefab to spawn (should have RotatableStatue and StatueArrowsUI)")]
    public GameObject statuePrefab;

    [Tooltip("Position on the table where this statue will be placed")]
    public Transform tablePosition;

    [Tooltip("Target Y rotation for puzzle solution (0, 45, 90, 135, 180, 225, 270, 315)")]
    [Range(0f, 315f)]
    public float correctRotation;
}