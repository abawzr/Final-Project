using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns collectible statues at random positions in the world when the game starts.
/// This is separate from the puzzle table system.
/// </summary>
public class StatueWorldSpawner : MonoBehaviour
{
    [Header("Statues to Spawn")]
    [Tooltip("List of statue prefabs to spawn in the world")]
    [SerializeField] private List<GameObject> statuePrefabs = new List<GameObject>();

    [Header("Spawn Points Pool")]
    [Tooltip("Possible spawn locations - statues will randomly pick from these")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Settings")]
    [Tooltip("Spawn statues when the game starts")]
    [SerializeField] private bool spawnOnStart = true;

    [Tooltip("Parent object for spawned statues (optional, for organization)")]
    [SerializeField] private Transform statuesParent;

    // Track which spawn points have been used
    private List<Transform> _availableSpawnPoints = new List<Transform>();

    // Track spawned statues
    private List<GameObject> _spawnedStatues = new List<GameObject>();

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnAllStatues();
        }
    }

    /// <summary>
    /// Spawns all statues at random positions.
    /// </summary>
    public void SpawnAllStatues()
    {
        // Reset available spawn points
        _availableSpawnPoints = new List<Transform>(spawnPoints);

        // Check if we have enough spawn points
        if (spawnPoints.Count < statuePrefabs.Count)
        {
            Debug.LogWarning($"StatueWorldSpawner: Not enough spawn points ({spawnPoints.Count}) for all statues ({statuePrefabs.Count})");
        }

        foreach (GameObject statuePrefab in statuePrefabs)
        {
            if (statuePrefab == null) continue;

            Transform spawnPoint = GetRandomSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning($"StatueWorldSpawner: No available spawn points left for {statuePrefab.name}");
                continue;
            }

            SpawnStatue(statuePrefab, spawnPoint);
        }

        Debug.Log($"StatueWorldSpawner: Spawned {_spawnedStatues.Count} statues");
    }

    /// <summary>
    /// Gets a random spawn point and removes it from the available pool.
    /// </summary>
    private Transform GetRandomSpawnPoint()
    {
        if (_availableSpawnPoints == null || _availableSpawnPoints.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, _availableSpawnPoints.Count);
        Transform spawnPoint = _availableSpawnPoints[randomIndex];
        _availableSpawnPoints.RemoveAt(randomIndex);

        return spawnPoint;
    }

    /// <summary>
    /// Spawns a single statue at the given position.
    /// </summary>
    private void SpawnStatue(GameObject prefab, Transform spawnPoint)
    {
        Transform parent = statuesParent != null ? statuesParent : transform;

        GameObject statue = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation,
            parent
        );

        _spawnedStatues.Add(statue);
    }

    /// <summary>
    /// Clears all spawned statues (useful for resetting).
    /// </summary>
    public void ClearAllStatues()
    {
        foreach (GameObject statue in _spawnedStatues)
        {
            if (statue != null)
            {
                Destroy(statue);
            }
        }
        _spawnedStatues.Clear();
    }

    /// <summary>
    /// Respawns all statues at new random positions.
    /// </summary>
    public void RespawnAllStatues()
    {
        ClearAllStatues();
        SpawnAllStatues();
    }
}