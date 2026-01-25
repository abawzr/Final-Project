using UnityEngine;

public class CloseDoorTrigger : MonoBehaviour
{
    [SerializeField] private Transform door;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private AudioClip closeDoorClip;

    private bool _isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_isTriggered) return;

        doorAnimator.SetTrigger("Close");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSFX(closeDoorClip, door.position);
        }
    }
}
