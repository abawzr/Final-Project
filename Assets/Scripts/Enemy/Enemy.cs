using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float normalSpeed;
    [SerializeField] private float stunSpeed;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float triggerJumpscareDistance;
    [SerializeField] private AudioClip jumpscareClip;
    [SerializeField] private AudioClip chasingScreamClip;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float walkStepInterval;
    [SerializeField] private float runStepInterval;
    [SerializeField] private Material glitchMaterial;

    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private Material originalObstacleMaterial;
    private float _stepTimer;
    private bool _isJumpscareOccurred;
    private float _screamTimer = 10f;

    private void Awake()
    {
        // Get Nav Mesh Agent and Animator components from same game object this script attached to
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        _navMeshAgent.speed = normalSpeed;
    }

    private void Update()
    {
        if (_animator != null)
            _animator.SetFloat("Speed", _navMeshAgent.velocity.magnitude);

        if (!_isJumpscareOccurred)
        {
            _navMeshAgent.SetDestination(playerTransform.position);

            _screamTimer -= Time.deltaTime;

            if (_screamTimer <= 0)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play2DSFX(chasingScreamClip);
                }
                _screamTimer = 10f;
            }

            if (Vector3.Distance(transform.position, playerTransform.position) <= triggerJumpscareDistance)
            {
                StartCoroutine(TriggerJumpscare());
            }

            PlayFootstep();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obstacle")) return;

        _navMeshAgent.speed = stunSpeed;

        MeshRenderer meshRenderer;

        if (other.TryGetComponent(out meshRenderer))
        {
            originalObstacleMaterial = meshRenderer.material;
            meshRenderer.material = glitchMaterial;
        }
        else
        {
            meshRenderer = other.GetComponentInChildren<MeshRenderer>(includeInactive: true);

            originalObstacleMaterial = meshRenderer.material;
            meshRenderer.material = glitchMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Obstacle")) return;

        _navMeshAgent.speed = normalSpeed;

        MeshRenderer meshRenderer;

        if (other.TryGetComponent(out meshRenderer))
        {
            meshRenderer.material = originalObstacleMaterial;
        }
        else
        {
            meshRenderer = other.GetComponentInChildren<MeshRenderer>(includeInactive: true);

            meshRenderer.material = originalObstacleMaterial;
        }
    }

    private IEnumerator TriggerJumpscare()
    {
        // PlayerMovement.IsMovementInputOn = false;
        // PlayerCamera.IsCameraInputOn = false;

        _navMeshAgent.speed = 0f;
        _navMeshAgent.acceleration = 0f;
        _navMeshAgent.velocity = Vector3.zero;

        // Jumpscare here
        // Get horizontal direction from camera (no vertical component)
        Vector3 horizontalForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;

        // Position enemy close to camera but on ground level
        Vector3 closePosition = cameraTransform.position + horizontalForward * 0.4f; // Adjust distance
        closePosition.y = transform.position.y; // Keep enemy at its current ground level

        _navMeshAgent.enabled = false; // Disable to allow teleport
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

        Cursor.lockState = CursorLockMode.Confined;
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

        float speed = _navMeshAgent.velocity.magnitude;

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
}
