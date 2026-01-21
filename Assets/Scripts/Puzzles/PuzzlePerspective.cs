using UnityEngine;
using Unity.Cinemachine;
using TMPro;

public class PuzzlePerspective : MonoBehaviour, IInteractable
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineCamera puzzleCamera;
    [SerializeField] private TMP_Text interactionTMP;

    [Header("Padlock")]
    [SerializeField] private GameObject padlock;

    public bool CanInteract = true;

    private bool _isPuzzleEnabled;

    private void Awake()
    {
        _isPuzzleEnabled = false;
        CanInteract = true;
    }

    private void Update()
    {
        if (_isPuzzleEnabled && Input.GetButtonDown("QuitPuzzle"))
        {
            DisablePuzzle();
        }
    }

    public void DisablePuzzle()
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
        interactionTMP.text = string.Empty;
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (!CanInteract) return;

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

    public void DestoryPadlock()
    {
        DisablePuzzle();
        Destroy(padlock);
    }
}
