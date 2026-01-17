using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private List<Transform> patrolPoints;
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float viewAngle = 60f;
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private LayerMask excludeEnemyLayer;
    [SerializeField] private float triggerJumpscareDistance;
    [SerializeField] private AudioClip jumpscareClip;
    [SerializeField] private AudioClip chasingScreamClip;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float walkStepInterval;
    [SerializeField] private float runStepInterval;

    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private AudioSource _audioSource;
    private int _currentPatrolPoint;
    private Vector3 _eyePosition;
    private Vector3 _directionToPlayer;
    private float _stepTimer;
    private bool _isJumpscareOccurred;
    private float _screamTimer = 10f;

    public static bool CanMove { get; set; }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 rightLimit = Quaternion.Euler(0, viewAngle, 0) * transform.forward;
        Vector3 leftLimit = Quaternion.Euler(0, -viewAngle, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + transform.up * 0.5f, transform.position + rightLimit * viewDistance);
        Gizmos.DrawLine(transform.position + transform.up * 0.5f, transform.position + leftLimit * viewDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_eyePosition, _directionToPlayer * viewDistance);
    }

    private void Awake()
    {
        // Get Nav Mesh Agent and Animator components from same game object this script attached to
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        CanMove = true;
    }

    private void Start()
    {
        // In the start pick a random patrol point and set enemy destination to it
        int randomIndex = Random.Range(0, patrolPoints.Count);
        _navMeshAgent.SetDestination(patrolPoints[randomIndex].position);
    }

    private void Update()
    {
        if (_animator != null)
            _animator.SetFloat("Speed", _navMeshAgent.velocity.magnitude);

        if (!_isJumpscareOccurred)
        {
            _screamTimer -= Time.deltaTime;

            // Check if enemy can see player
            if (CanSeePlayer())
            {
                if (_screamTimer <= 0)
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.Play2DSFX(chasingScreamClip);
                    }
                    _screamTimer = 10f;
                }

                if (_animator != null)
                {
                    _animator.SetBool("IsChasing", true);
                }

                // Speed up the enemy and make it follow player
                _navMeshAgent.speed = runningSpeed;
                _navMeshAgent.SetDestination(playerTransform.position);

                if (Vector3.Distance(transform.position, playerTransform.position) <= triggerJumpscareDistance)
                {
                    StartCoroutine(TriggerJumpscare());
                }
            }

            else
            {
                if (_animator != null)
                {
                    _animator.SetBool("IsChasing", false);
                }

                // Slow down the enemy and make him patrolling
                _navMeshAgent.speed = walkingSpeed;
                if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance < 0.2f)
                {
                    NextPoint();
                }
            }

            PlayFootstep();
        }
    }

    private bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        // Check distance between enemy and player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance > viewDistance) return false;

        // Check field of view
        _eyePosition = transform.position + transform.up * 1.8f;
        _directionToPlayer = (playerTransform.position + playerTransform.up * 0.25f - _eyePosition).normalized;
        float angle = Vector3.Angle(transform.forward, _directionToPlayer);
        if (angle > viewAngle) return false;

        // Raycast to check if wall is blocking
        if (Physics.Raycast(_eyePosition, _directionToPlayer, out RaycastHit hitInfo, viewDistance, excludeEnemyLayer))
        {
            if (hitInfo.collider.tag == "Player")
            {
                return true;
            }

            return false; // Player is not visible
        }

        return true; // Player is visible
    }

    private void NextPoint()
    {
        if (patrolPoints.Count == 0) return;

        // Don't stop the loop till next point found
        while (true)
        {
            int tempIndex = Random.Range(0, patrolPoints.Count);

            if (tempIndex != _currentPatrolPoint)
            {
                _currentPatrolPoint = tempIndex;
                break;
            }
        }

        _navMeshAgent.SetDestination(patrolPoints[_currentPatrolPoint].position);
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

        _animator.SetBool("IsJumpscare", true);

        _audioSource.PlayOneShot(jumpscareClip);

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
}
