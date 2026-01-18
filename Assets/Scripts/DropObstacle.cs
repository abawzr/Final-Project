using TMPro;
using UnityEngine;

public class DropObstacle : MonoBehaviour
{
    [SerializeField] private GameObject obstacle;
    [SerializeField] private Animator obstacleAnimator;
    [SerializeField] private AudioClip obstacleAudio;
    [SerializeField] private TMP_Text interactionTMP;
    [SerializeField] private string interactionText;

    private bool _isPlayerInRange = false;
    private bool _isTriggered = false;

    private void Awake()
    {
        _isTriggered = false;
        _isPlayerInRange = false;
    }

    private void Update()
    {
        if (_isPlayerInRange && !_isTriggered && Input.GetButtonDown("Interact"))
        {
            Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_isTriggered) return;

        _isPlayerInRange = true;
        interactionTMP.text = interactionText;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _isPlayerInRange = false;
        interactionTMP.text = string.Empty;
    }

    private void Interact()
    {
        _isTriggered = true;
        interactionTMP.text = string.Empty;

        if (obstacleAnimator != null)
        {
            obstacleAnimator.SetTrigger("Drop");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSFX(obstacleAudio, obstacle.transform.position);
        }
    }
}
