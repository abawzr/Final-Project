using UnityEngine;

/// <summary>
/// GlitchController - Glitch effect based on monster distance
/// 
/// The closer the monster gets to the player, the stronger the glitch.
/// You can limit the maximum intensity so the player can always see.
/// 
/// SETUP:
/// 1. Create empty GameObject, add this script
/// 2. Assign your GlitchMaterial (primary effect)
/// 3. Assign secondary material if you have one (optional)
/// 4. Assign Player and Monster transforms
/// </summary>
public class GlitchController : MonoBehaviour
{
    public static GlitchController Instance;

    [Header("=== Required References ===")]
    [Tooltip("The player's transform")]
    public Transform player;

    [Tooltip("The monster's transform")]
    public Transform monster;

    [Header("=== Materials ===")]
    [Tooltip("Your primary glitch material")]
    public Material glitchMaterial;

    [Tooltip("Your secondary effect material (optional)")]
    public Material secondaryMaterial;

    [Header("=== Distance Settings ===")]
    [Tooltip("Distance at which glitch starts (monster far away = no effect)")]
    public float maxDistance = 20f;

    [Tooltip("Distance at which glitch is maximum (monster very close)")]
    public float minDistance = 2f;

    [Header("=== Intensity Limits ===")]
    [Tooltip("Maximum intensity the effect can reach (0-1). Lower = player can always see.")]
    [Range(0f, 1f)]
    public float maxIntensity = 0.7f;

    [Tooltip("Minimum intensity when monster is in range (subtle constant effect)")]
    [Range(0f, 0.5f)]
    public float minIntensity = 0f;

    [Header("=== Effect Strength ===")]
    [Tooltip("Maximum chromatic aberration")]
    [Range(0.01f, 0.2f)]
    public float maxChromatic = 0.08f;

    [Tooltip("Maximum noise/static")]
    [Range(0f, 1f)]
    public float maxNoise = 0.6f;

    [Header("=== Secondary Material Settings ===")]
    [Tooltip("Property name for intensity on secondary material")]
    public string secondaryIntensityProperty = "_Intensity";

    [Tooltip("How much the secondary effect scales with glitch (0-1)")]
    [Range(0f, 1f)]
    public float secondaryEffectScale = 1f;

    [Header("=== Burst Settings ===")]
    [Tooltip("Enable random glitch bursts?")]
    public bool enableBursts = true;

    [Tooltip("Minimum time between bursts")]
    [Range(0.5f, 5f)]
    public float minBurstInterval = 1f;

    [Tooltip("Maximum time between bursts")]
    [Range(1f, 10f)]
    public float maxBurstInterval = 4f;

    [Tooltip("How long each burst lasts")]
    [Range(0.1f, 1f)]
    public float burstDuration = 0.2f;

    [Tooltip("Burst intensity multiplier")]
    [Range(1f, 3f)]
    public float burstMultiplier = 1.5f;

    [Header("=== State (Debug) ===")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private float currentDistance = 0f;
    [SerializeField] private float currentIntensity = 0f;
    [SerializeField] private bool isBursting = false;

    // Private variables
    private float burstTimer;
    private float burstTimeLeft;

    // Shader property IDs (cached for performance)
    private static readonly int PropIntensity = Shader.PropertyToID("_Intensity");
    private static readonly int PropChromatic = Shader.PropertyToID("_ChromaticSplit");
    private static readonly int PropNoise = Shader.PropertyToID("_NoiseAmount");

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        ResetEffect();
        burstTimer = Random.Range(minBurstInterval, maxBurstInterval);
    }

    void Update()
    {
        // Check if we have required references
        if (player == null || monster == null)
        {
            ResetEffect();
            return;
        }

        // Calculate distance between player and monster
        currentDistance = Vector3.Distance(player.position, monster.position);

        // Check if monster is in range
        if (currentDistance > maxDistance)
        {
            // Monster too far - no effect
            isActive = false;
            ResetEffect();
            return;
        }

        isActive = true;

        // Calculate base intensity based on distance
        // Far (maxDistance) = 0, Close (minDistance) = 1
        float distanceRatio = Mathf.InverseLerp(maxDistance, minDistance, currentDistance);

        // Apply min/max intensity limits
        float baseIntensity = Mathf.Lerp(minIntensity, maxIntensity, distanceRatio);

        // Handle bursts
        if (enableBursts)
        {
            UpdateBursts();
        }

        // Calculate final intensity with burst
        float finalIntensity = baseIntensity;
        if (isBursting)
        {
            finalIntensity = Mathf.Min(baseIntensity * burstMultiplier, maxIntensity);
        }

        currentIntensity = finalIntensity;

        // Apply effects
        ApplyEffect(finalIntensity);
    }

    void UpdateBursts()
    {
        if (isBursting)
        {
            burstTimeLeft -= Time.deltaTime;
            if (burstTimeLeft <= 0f)
            {
                isBursting = false;
                // Next burst comes faster when monster is closer
                float burstSpeed = 1f - (currentIntensity / maxIntensity) * 0.5f;
                burstTimer = Random.Range(minBurstInterval, maxBurstInterval) * burstSpeed;
            }
        }
        else
        {
            burstTimer -= Time.deltaTime;
            if (burstTimer <= 0f)
            {
                isBursting = true;
                burstTimeLeft = burstDuration * Random.Range(0.8f, 1.2f);
            }
        }
    }

    void ApplyEffect(float intensity)
    {
        // Apply to primary glitch material
        if (glitchMaterial != null)
        {
            glitchMaterial.SetFloat(PropIntensity, intensity);
            glitchMaterial.SetFloat(PropChromatic, intensity * maxChromatic);
            glitchMaterial.SetFloat(PropNoise, intensity * maxNoise);
        }

        // Apply to secondary material
        if (secondaryMaterial != null)
        {
            float secondaryIntensity = intensity * secondaryEffectScale;
            secondaryMaterial.SetFloat(secondaryIntensityProperty, secondaryIntensity);
        }
    }

    void ResetEffect()
    {
        if (glitchMaterial != null)
        {
            glitchMaterial.SetFloat(PropIntensity, 0f);
            glitchMaterial.SetFloat(PropChromatic, 0f);
            glitchMaterial.SetFloat(PropNoise, 0f);
        }

        if (secondaryMaterial != null)
        {
            secondaryMaterial.SetFloat(secondaryIntensityProperty, 0f);
        }
    }

    // =========================================================================
    // PUBLIC METHODS
    // =========================================================================

    /// <summary>
    /// Manually trigger a glitch burst
    /// </summary>
    public void TriggerBurst()
    {
        isBursting = true;
        burstTimeLeft = burstDuration;
    }

    /// <summary>
    /// Set a new monster reference (if monster changes)
    /// </summary>
    public void SetMonster(Transform newMonster)
    {
        monster = newMonster;
    }

    /// <summary>
    /// Temporarily override intensity (for cutscenes, etc.)
    /// </summary>
    public void SetIntensityOverride(float intensity)
    {
        ApplyEffect(Mathf.Clamp(intensity, 0f, maxIntensity));
    }

    void OnDisable()
    {
        ResetEffect();
    }

    void OnDestroy()
    {
        ResetEffect();
        if (Instance == this)
            Instance = null;
    }

    // Visualize distances in Scene view
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // Max distance (where effect starts)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, maxDistance);

            // Min distance (where effect is maximum)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minDistance);
        }
    }
}