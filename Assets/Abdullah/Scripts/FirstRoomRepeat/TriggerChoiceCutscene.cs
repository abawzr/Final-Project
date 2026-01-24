using UnityEngine;
using UnityEngine.Playables;

public class TriggerChoiceCutscene : MonoBehaviour
{
    [SerializeField] private RecordPlayer recordPlayer;
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private GameObject badEndingButton;
    [SerializeField] private GameObject trueEndingButton;

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

    public void ShowButtons()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.Choice);
        }

        badEndingButton.SetActive(true);
        trueEndingButton.SetActive(true);
    }
}
