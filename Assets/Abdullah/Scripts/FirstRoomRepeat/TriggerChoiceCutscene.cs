using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class TriggerChoiceCutscene : MonoBehaviour
{
    [SerializeField] private RecordPlayer recordPlayer;
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private GameObject badEndingButton;
    [SerializeField] private GameObject trueEndingButton;
    [SerializeField] private GameObject badEndingPath;
    [SerializeField] private GameObject trueEndingPath;
    [SerializeField] private Transform badPathDoor;
    [SerializeField] private Transform truePathDoor;
    [SerializeField] private Animator badPathDoorAnimator;
    [SerializeField] private Animator truePathDoorAnimator;
    [SerializeField] private AudioClip openDoorClip;

    private int _counterToTriggerCutscene = 0;
    private bool _isTriggered = false;

    private void Awake()
    {
        _counterToTriggerCutscene = 0;
        _isTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_isTriggered) return;

        _counterToTriggerCutscene++;

        if (_counterToTriggerCutscene > 1 && recordPlayer.IsReactionFinished)
        {
            _isTriggered = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameState(GameManager.GameState.Cutscene);
            }

            playableDirector.Play();
        }
    }

    private IEnumerator BadEndingSequence()
    {
        badPathDoorAnimator.SetTrigger("Open");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSFX(openDoorClip, badPathDoor.position);
        }

        badEndingPath.SetActive(true);
        badEndingButton.SetActive(false);
        trueEndingButton.SetActive(false);

        yield return new WaitForSecondsRealtime(1.1f);

        playableDirector.Resume();
    }

    private IEnumerator TrueEndingSequence()
    {
        truePathDoorAnimator.SetTrigger("Open");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSFX(openDoorClip, truePathDoor.position);
        }

        trueEndingPath.SetActive(true);
        trueEndingButton.SetActive(false);
        badEndingButton.SetActive(false);

        yield return new WaitForSecondsRealtime(1.1f);

        playableDirector.Resume();
    }

    public void ShowButtons()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.Choice);
        }

        badEndingButton.SetActive(true);
        trueEndingButton.SetActive(true);

        playableDirector.Pause();
    }

    public void BadEnding()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.Cutscene);
        }

        StartCoroutine(BadEndingSequence());
    }

    public void TrueEnding()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.Cutscene);
        }

        StartCoroutine(TrueEndingSequence());
    }

    public void FinishCutscene()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.Gameplay);
        }
    }
}
