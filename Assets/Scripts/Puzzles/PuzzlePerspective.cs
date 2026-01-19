using UnityEngine;
using Unity.Cinemachine;

public class PuzzlePerspective : MonoBehaviour, IInteractable
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineCamera puzzleCamera;

    private bool _isPuzzleEnabled;

    private void Awake()
    {
        _isPuzzleEnabled = false;
    }

    private void Update()
    {
        if (_isPuzzleEnabled && Input.GetButtonDown("QuitPuzzle"))
        {
            DisablePuzzle();
        }
    }

    private void DisablePuzzle()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.Gameplay);
        }

        _isPuzzleEnabled = false;
        PlayerMovement.IsControlsEnabled = true;
        PlayerInteraction.CanInteract = true;
        puzzleCamera.Priority = 0;
        mainCamera.cullingMask = ~0;
    }

    public void Interact()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.Puzzle);
        }

        _isPuzzleEnabled = true;
        PlayerMovement.IsControlsEnabled = false;
        PlayerInteraction.CanInteract = false;
        puzzleCamera.Priority = 10;
        mainCamera.cullingMask = ~LayerMask.GetMask("Player");
    }
}
