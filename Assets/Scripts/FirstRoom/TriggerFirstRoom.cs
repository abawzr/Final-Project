using UnityEngine;

public class TriggerFirstRoom : MonoBehaviour
{
    [SerializeField] private Animator doorAnimator;

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
    }
}
