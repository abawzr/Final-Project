using UnityEngine;

public class TrueCutscene : MonoBehaviour
{
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private AudioClip playerVoiceClip;

    public void PlayPlayerVoice()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play2DSFX(playerVoiceClip);
        }
    }

    public void ShowPlayerMesh()
    {
        playerCharacter.SetActive(true);
    }
}
