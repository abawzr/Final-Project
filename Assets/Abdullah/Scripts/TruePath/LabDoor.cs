using UnityEngine;
using UnityEngine.Playables;

public class LabDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private Transform soundPosition;
    [SerializeField] private AudioClip puzzleSolvedClip;
    [SerializeField] private MeshRenderer puzzleRedLight1;
    [SerializeField] private MeshRenderer puzzleRedLight2;
    [SerializeField] private MeshRenderer puzzleRedLight3;
    [SerializeField] private Material puzzleGreenLight;

    private bool _puzzle1Triggered;
    private bool _puzzle2Triggered;
    private bool _puzzle3Triggered;

    public bool CanInteract { get; set; }

    public static bool IsPuzzle1Solved = false;
    public static bool IsPuzzle2Solved = false;
    public static bool IsPuzzle3Solved = false;

    private void Awake()
    {
        CanInteract = true;

        IsPuzzle1Solved = true;
        IsPuzzle2Solved = true;
        IsPuzzle3Solved = true;

        _puzzle1Triggered = false;
        _puzzle2Triggered = false;
        _puzzle3Triggered = false;
    }

    private void Update()
    {
        if (IsPuzzle1Solved && puzzleRedLight1.material != puzzleGreenLight && !_puzzle1Triggered)
        {
            _puzzle1Triggered = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play3DSFX(puzzleSolvedClip, soundPosition.position, 5f);
            }

            puzzleRedLight1.material = puzzleGreenLight;
        }

        if (IsPuzzle2Solved && puzzleRedLight2.material != puzzleGreenLight && !_puzzle2Triggered)
        {
            _puzzle2Triggered = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play3DSFX(puzzleSolvedClip, soundPosition.position, 5f);
            }

            puzzleRedLight2.material = puzzleGreenLight;
        }

        if (IsPuzzle3Solved && puzzleRedLight3.material != puzzleGreenLight && !_puzzle3Triggered)
        {
            _puzzle3Triggered = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play3DSFX(puzzleSolvedClip, soundPosition.position, 5f);
            }

            puzzleRedLight3.material = puzzleGreenLight;
        }
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (!IsPuzzle1Solved || !IsPuzzle2Solved || !IsPuzzle3Solved) return;
        if (!CanInteract) return;

        CanInteract = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.Cutscene);
        }

        playableDirector.Play();
    }
}
