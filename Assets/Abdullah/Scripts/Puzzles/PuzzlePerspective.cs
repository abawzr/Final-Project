using UnityEngine;
using Unity.Cinemachine;
using TMPro;
using Subtitles;

public class PuzzlePerspective : MonoBehaviour, IInteractable
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineCamera puzzleCamera;
    [SerializeField] private TMP_Text interactionTMP;

    [Header("Padlock")]
    [SerializeField] private GameObject padlock;

    [Header("Paintings")]
    [SerializeField] private bool isPaintingPuzzle;
    [SerializeField] private AudioClip noPaintingsClip;
    [SerializeField] private ItemSO painting1;
    [SerializeField] private ItemSO painting2;
    [SerializeField] private ItemSO painting3;
    [SerializeField] private GameObject paintingSpawn1;
    [SerializeField] private GameObject paintingSpawn2;
    [SerializeField] private GameObject paintingSpawn3;
    [SerializeField] private string subtitle;

    public bool CanInteract { get; set; }

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

    public CinemachineCamera GetPuzzleCamera()
    {
        return puzzleCamera;
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

        if (isPaintingPuzzle)
        {
            if (!playerInventory.HasItem(painting1) || !playerInventory.HasItem(painting2) || !playerInventory.HasItem(painting3))
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play2DSFX(noPaintingsClip);
                    SubtitleManager.Instance.Play(subtitle);
                }

                return;
            }

            else if (playerInventory.HasItem(painting1) && playerInventory.HasItem(painting2) && playerInventory.HasItem(painting3))
            {
                paintingSpawn1.SetActive(true);
                paintingSpawn2.SetActive(true);
                paintingSpawn3.SetActive(true);

                playerInventory.UseOrDropItem(painting1, isUse: true);
                playerInventory.UseOrDropItem(painting2, isUse: true);
                playerInventory.UseOrDropItem(painting3, isUse: true);
            }

            isPaintingPuzzle = false;
        }

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
