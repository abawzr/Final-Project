using UnityEngine;

public class TriggerFirstRoom : MonoBehaviour
{
    [SerializeField] private Transform door;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private AudioClip doorClip;

    public static bool IsTriggered = false;

    private void Awake()
    {
        IsTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (IsTriggered) return;

        IsTriggered = true;
        ForcedDeathState.UseGlitchDeath = true;

        doorAnimator.SetTrigger("Close");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSFX(doorClip, door.position);
        }
    }
}
