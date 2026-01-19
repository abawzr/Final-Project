using UnityEngine;

/// <summary>
/// GlitchController - Glitch effect with chase, jumpscare, and death UI modes
/// 
/// MODES:
/// 1. CHASE MODE: Effect based on monster distance (automatic)
/// 2. PAUSED: No effect (for jumpscares/cutscenes)
/// 3. DEATH UI MODE: Strong effect during game over UI
/// 
/// FLOW:
/// Chase (auto) => StopChaseEffect() => Jumpscare => StartDeathEffect() => UI => StopDeathEffect() => Resume
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

    [Header("=== Distance Settings (Chase Mode) ===")]
    [Tooltip("Distance at which glitch starts")]
    public float maxDistance = 20f;

    [Tooltip("Distance at which glitch is maximum")]
    public float minDistance = 2f;

    [Header("=== Intensity Limits (Chase Mode) ===")]
    [Tooltip("Maximum intensity during chase (0-1)")]
    [Range(0f, 1f)]
    public float maxChaseIntensity = 0.7f;

    [Tooltip("Minimum intensity when monster is in range")]
    [Range(0f, 0.5f)]
    public float minChaseIntensity = 0f;

    [Header("=== Death UI Settings ===")]
    [Tooltip("Intensity during death UI screen")]
    [Range(0f, 1f)]
    public float deathIntensity = 0.9f;

    [Tooltip("How fast the death effect fades in (seconds)")]
    [Range(0.1f, 2f)]
    public float deathFadeInTime = 0.3f;

    [Tooltip("Chromatic aberration during death UI")]
    [Range(0f, 0.3f)]
    public float deathChromatic = 0.15f;

    [Tooltip("Noise amount during death UI")]
    [Range(0f, 1f)]
    public float deathNoise = 0.8f;

    [Header("=== Effect Strength (Chase Mode) ===")]
    [Tooltip("Maximum chromatic aberration during chase")]
    [Range(0.01f, 0.2f)]
    public float maxChromatic = 0.08f;

    [Tooltip("Maximum noise during chase")]
    [Range(0f, 1f)]
    public float maxNoise = 0.6f;

    [Header("=== Secondary Material Settings ===")]
    [Tooltip("Property name for intensity on secondary material")]
    public string secondaryIntensityProperty = "_Intensity";

    [Tooltip("How much the secondary effect scales (0-1)")]
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
    [SerializeField] private EffectMode currentMode = EffectMode.Chase;
    [SerializeField] private float currentDistance = 0f;
    [SerializeField] private float currentIntensity = 0f;
    [SerializeField] private bool isBursting = false;

    public enum EffectMode
    {
        Chase,      // Normal gameplay - effect based on monster distance
        Paused,     // No effect - for jumpscares/cutscenes
        DeathUI     // Death screen - strong glitch effect
    }

    // Private variables
    private float burstTimer;
    private float burstTimeLeft;
    private float deathFadeProgress = 0f;

    // Shader property IDs
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
        switch (currentMode)
        {
            case EffectMode.Chase:
                UpdateChaseEffect();
                break;

            case EffectMode.Paused:
                // Do nothing - effect stays off
                break;

            case EffectMode.DeathUI:
                UpdateDeathEffect();
                break;
        }
    }

    void UpdateChaseEffect()
    {
        // Check if we have required references
        if (player == null || monster == null)
        {
            ResetEffect();
            return;
        }

        // Calculate distance
        currentDistance = Vector3.Distance(player.position, monster.position);

        // Check if monster is in range
        if (currentDistance > maxDistance)
        {
            ResetEffect();
            return;
        }

        // Calculate intensity based on distance
        float distanceRatio = Mathf.InverseLerp(maxDistance, minDistance, currentDistance);
        float baseIntensity = Mathf.Lerp(minChaseIntensity, maxChaseIntensity, distanceRatio);

        // Handle bursts
        if (enableBursts)
        {
            UpdateBursts();
        }

        // Apply burst multiplier
        float finalIntensity = baseIntensity;
        if (isBursting)
        {
            finalIntensity = Mathf.Min(baseIntensity * burstMultiplier, maxChaseIntensity);
        }

        currentIntensity = finalIntensity;

        // Apply effect
        float chromatic = finalIntensity * maxChromatic;
        float noise = finalIntensity * maxNoise;
        ApplyEffect(finalIntensity, chromatic, noise);
    }

    void UpdateDeathEffect()
    {
        // Fade in the death effect
        if (deathFadeProgress < 1f)
        {
            deathFadeProgress += Time.unscaledDeltaTime / deathFadeInTime;
            deathFadeProgress = Mathf.Clamp01(deathFadeProgress);
        }

        float intensity = Mathf.Lerp(0f, deathIntensity, deathFadeProgress);
        currentIntensity = intensity;

        ApplyEffect(intensity, deathChromatic, deathNoise);
    }

    void UpdateBursts()
    {
        if (isBursting)
        {
            burstTimeLeft -= Time.deltaTime;
            if (burstTimeLeft <= 0f)
            {
                isBursting = false;
                float burstSpeed = 1f - (currentIntensity / maxChaseIntensity) * 0.5f;
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

    void ApplyEffect(float intensity, float chromatic, float noise)
    {
        if (glitchMaterial != null)
        {
            glitchMaterial.SetFloat(PropIntensity, intensity);
            glitchMaterial.SetFloat(PropChromatic, chromatic);
            glitchMaterial.SetFloat(PropNoise, noise);
        }

        if (secondaryMaterial != null)
        {
            float secondaryIntensity = intensity * secondaryEffectScale;
            secondaryMaterial.SetFloat(secondaryIntensityProperty, secondaryIntensity);
        }
    }

    void ResetEffect()
    {
        currentIntensity = 0f;

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
    // PUBLIC METHODS - Call these from your scripts!
    // =========================================================================

    /// <summary>
    /// STEP 1: Call this when monster catches player.
    /// Stops the glitch so jumpscare/cutscene is visible.
    /// </summary>
    public void StopChaseEffect()
    {
        currentMode = EffectMode.Paused;
        ResetEffect();
        Debug.Log("Glitch STOPPED for jumpscare/cutscene");
    }

    /// <summary>
    /// STEP 2: Call this after jumpscare/cutscene, when death UI appears.
    /// Starts the glitch effect on the UI screen.
    /// </summary>
    public void StartDeathEffect()
    {
        currentMode = EffectMode.DeathUI;
        deathFadeProgress = 0f;
        Debug.Log("Glitch STARTED for death UI");
    }

    /// <summary>
    /// STEP 2 (Alternative): Start death effect with custom values.
    /// </summary>
    public void StartDeathEffect(float intensity, float chromatic, float noise)
    {
        deathIntensity = intensity;
        deathChromatic = chromatic;
        deathNoise = noise;
        currentMode = EffectMode.DeathUI;
        deathFadeProgress = 0f;
        Debug.Log("Glitch STARTED for death UI (custom values)");
    }

    /// <summary>
    /// STEP 3: Call this when player clicks Restart or Resume.
    /// Stops death effect and returns to normal chase mode.
    /// </summary>
    public void StopDeathEffect()
    {
        currentMode = EffectMode.Chase;
        deathFadeProgress = 0f;
        ResetEffect();
        Debug.Log("Glitch STOPPED, returning to chase mode");
    }

    /// <summary>
    /// Manually trigger a glitch burst.
    /// </summary>
    public void TriggerBurst()
    {
        isBursting = true;
        burstTimeLeft = burstDuration;
    }

    /// <summary>
    /// Set a new monster reference.
    /// </summary>
    public void SetMonster(Transform newMonster)
    {
        monster = newMonster;
    }

    /// <summary>
    /// Get current mode.
    /// </summary>
    public EffectMode GetCurrentMode()
    {
        return currentMode;
    }

    /// <summary>
    /// Get current intensity.
    /// </summary>
    public float GetCurrentIntensity()
    {
        return currentIntensity;
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

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, maxDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minDistance);
        }
    }
}