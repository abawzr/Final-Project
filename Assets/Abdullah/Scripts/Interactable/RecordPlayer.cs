using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class RecordPlayer : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioClip playerReactionClip;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private bool oneTimePlay;

    public bool CanInteract { get; set; }
    public bool IsReactionFinished;

    private void Awake()
    {
        IsReactionFinished = false;
        CanInteract = true;
    }

    private IEnumerator SoundSequence()
    {
        if (oneTimePlay)
        {
            PlayerMovement.IsControlsEnabled = false;
            cinemachineCamera.Priority = 10;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSFX(audioClip, transform.position);
        }

        if (playerReactionClip == null) yield break;

        yield return new WaitForSeconds(audioClip.length);

        cinemachineCamera.Priority = 0;
        PlayerMovement.IsControlsEnabled = true;

        yield return new WaitForSeconds(1.5f);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play2DSFX(playerReactionClip);
        }

        yield return new WaitForSeconds(playerReactionClip.length + 0.5f);

        IsReactionFinished = true;
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (oneTimePlay)
        {
            CanInteract = false;
        }

        StartCoroutine(SoundSequence());
    }
}
