using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BadPath : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform enemyRespawnPoint;
    [SerializeField] private Padlock badPathPadlock;
    [SerializeField] private Animator endingAnimator;
    [SerializeField] private GameObject chest;
    [SerializeField] private Animator chestAnimator;

    private bool _isActive = false;
    private bool _endingTriggered = false;

    private void Awake()
    {
        _isActive = false;
        _endingTriggered = false;
    }

    private void Update()
    {
        if (badPathPadlock.IsUnlocked && !_endingTriggered)
        {
            TriggerBadEnding();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_isActive) return;

        _isActive = true;
        chest.SetActive(true);
        chestAnimator.SetTrigger("Throw");
        enemy.SetActive(true);
        enemy.GetComponent<Enemy>().Respawn(enemyRespawnPoint.position);
    }

    private void TriggerBadEnding()
    {
        _endingTriggered = true;
        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        endingAnimator.SetTrigger("End");

        yield return new WaitForSecondsRealtime(3f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.Cutscene);
        }

        yield return new WaitForSecondsRealtime(27f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.MainMenu);
            SceneManager.LoadSceneAsync("MainMenuScene");
        }
    }
}
