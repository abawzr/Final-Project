using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float normalSpeed;
    [SerializeField] private float stunSpeed;

    [Header("Player References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform cameraTransform;

    [Header("Jumpscare")]
    [SerializeField] private float triggerJumpscareDistance;
    [SerializeField] private AudioClip jumpscareClip;

    [Header("Footstep")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float walkStepInterval;
    [SerializeField] private float runStepInterval;

    [Header("Obstacle Glitch")]
    [SerializeField] private Material obstacleGlitchMaterial;

    [Header("Glitch Death (SPECIAL ONLY)")]
    [SerializeField] private RawImage glitchImage;
    [SerializeField] private CanvasGroup deathCanvas;
    [SerializeField] private Button restartButton;
    [SerializeField] private string afterFirstDeathSceneName;

    private NavMeshAgent _agent;
    private Animator _animator;
    private Material _originalObstacleMaterial;
    private float _stepTimer;
    private bool _isJumpscareOccurred;

    // first death only
    private int _restartPressCount;
    private float _glitchIntensity = 0.2f;

    private void Awake()
    {
        // Get Nav Mesh Agent and Animator components from same game object this script attached to
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        _agent.speed = normalSpeed;

        if (glitchImage != null)
            glitchImage.enabled = false;

        if (deathCanvas != null)
        {
            deathCanvas.alpha = 0;
            deathCanvas.interactable = false;
            deathCanvas.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (_animator != null)
            _animator.SetFloat("Speed", _agent.velocity.magnitude);

        if (_isJumpscareOccurred) return;

        _agent.SetDestination(playerTransform.position);

        if (Vector3.Distance(transform.position, playerTransform.position) <= triggerJumpscareDistance)
        {
            StartCoroutine(TriggerJumpscare());
        }

        PlayFootstep();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obstacle")) return;

        _agent.speed = stunSpeed;

        MeshRenderer meshRenderer;

        if (other.TryGetComponent(out meshRenderer))
        {
            _originalObstacleMaterial = meshRenderer.material;
            meshRenderer.material = obstacleGlitchMaterial;
        }
        else
        {
            meshRenderer = other.GetComponentInChildren<MeshRenderer>(includeInactive: true);

            _originalObstacleMaterial = meshRenderer.material;
            meshRenderer.material = obstacleGlitchMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Obstacle")) return;

        _agent.speed = normalSpeed;

        MeshRenderer meshRenderer;

        if (other.TryGetComponent(out meshRenderer))
        {
            meshRenderer.material = _originalObstacleMaterial;
        }
        else
        {
            meshRenderer = other.GetComponentInChildren<MeshRenderer>(includeInactive: true);

            meshRenderer.material = _originalObstacleMaterial;
        }
    }

    private IEnumerator TriggerJumpscare()
    {
        PlayerMovement.IsControlsEnabled = false;

        _agent.speed = 0f;
        _agent.acceleration = 0f;
        _agent.velocity = Vector3.zero;

        // Jumpscare here
        // Get horizontal direction from camera (no vertical component)
        Vector3 horizontalForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;

        // Position enemy close to camera but on ground level
        Vector3 closePosition = cameraTransform.position + horizontalForward * 0.4f; // Adjust distance
        closePosition.y = transform.position.y; // Keep enemy at its current ground level

        _agent.enabled = false; // Disable to allow teleport
        transform.position = closePosition;
        transform.LookAt(new Vector3(cameraTransform.position.x, transform.position.y, cameraTransform.position.z)); // Face camera

        // Calculate enemy's face position
        Vector3 enemyFacePosition = transform.position + transform.up * 1.8f;

        // Snap camera to look at enemy face
        StartCoroutine(SnapCameraToEnemy(enemyFacePosition));

        if (_animator != null)
        {
            _animator.SetBool("IsJumpscare", true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play2DSFX(jumpscareClip);
        }

        _isJumpscareOccurred = true;

        yield return new WaitForSeconds(2.5f);

        if (ForcedDeathState.UseGlitchDeath)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameState(GameManager.GameState.FirstDeath);
            }

            StartCoroutine(GlitchDeathSequence());
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator SnapCameraToEnemy(Vector3 targetPosition)
    {
        float duration = 0.2f; // How fast camera snaps to enemy
        float elapsed = 0f;

        Quaternion startRotation = cameraTransform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Calculate direction to enemy face
            Vector3 directionToFace = targetPosition - cameraTransform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToFace);

            // Smoothly rotate camera
            cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        // Keep camera locked on enemy face
        while (true)
        {
            Vector3 directionToFace = targetPosition - cameraTransform.position;
            cameraTransform.rotation = Quaternion.LookRotation(directionToFace);
            yield return null;
        }
    }

    private void PlayFootstep()
    {
        if (footstepClip == null) return;

        float speed = _agent.velocity.magnitude;

        // Enemy is not moving
        if (speed < 0.01f)
        {
            _stepTimer = 0;
            return;
        }

        float stepInterval = speed < 5 ? walkStepInterval : runStepInterval;

        _stepTimer += Time.deltaTime;

        if (_stepTimer >= stepInterval)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play3DSFX(footstepClip, transform.position);
            }
            _stepTimer = 0;
        }
    }

    public void Respawn(Vector3 respawnPoint)
    {
        transform.position = respawnPoint;
        enabled = true;
    }

    // ===== SPECIAL GLITCH DEATH ONLY =====

    private IEnumerator GlitchDeathSequence()
    {
        ForcedDeathState.UseGlitchDeath = false;

        glitchImage.enabled = true;
        glitchImage.material.SetFloat("_Intensity", _glitchIntensity);
        glitchImage.material.SetFloat("_ChromaticSplit", 0.3f);
        glitchImage.material.SetFloat("_NoiseAmount", 0.3f);

        deathCanvas.alpha = 1;
        deathCanvas.interactable = true;
        deathCanvas.blocksRaycasts = true;

        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(OnGlitchRestartPressed);

        yield return null;
    }

    private void OnGlitchRestartPressed()
    {
        _restartPressCount++;

        _glitchIntensity += 0.25f;

        glitchImage.material.SetFloat("_Intensity", _glitchIntensity);
        glitchImage.material.SetFloat("_ChromaticSplit", 0.3f + 0.1f * _restartPressCount);
        glitchImage.material.SetFloat("_NoiseAmount", 0.3f + 0.1f * _restartPressCount);

        if (_restartPressCount >= 3)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameState(GameManager.GameState.Gameplay);
            }

            SceneManager.LoadSceneAsync(afterFirstDeathSceneName);
        }
    }
}
