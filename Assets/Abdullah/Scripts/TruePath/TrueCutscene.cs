using System.Collections;
using Subtitles;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrueCutscene : MonoBehaviour
{
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private AudioClip playerVoiceClip;
    [SerializeField] private Animator credits;
    [SerializeField] private string subtitle;

    private IEnumerator CreditsSequence()
    {
        credits.SetTrigger("End");

        yield return new WaitForSecondsRealtime(16f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.MainMenu);
        }

        SceneManager.LoadSceneAsync("MainMenuScene");
    }

    public void PlayPlayerVoice()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play2DSFX(playerVoiceClip);
            SubtitleManager.Instance.Play(subtitle);
        }
    }

    public void ShowPlayerMesh()
    {
        playerCharacter.SetActive(true);
    }

    public void ShowCredits()
    {
        StartCoroutine(CreditsSequence());
    }
}
