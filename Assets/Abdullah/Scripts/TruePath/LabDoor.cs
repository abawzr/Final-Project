using UnityEngine;
using UnityEngine.Playables;

public class LabDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private AudioClip puzzleSolvedClip;
    [SerializeField] private MeshRenderer puzzleRedLight1;
    [SerializeField] private MeshRenderer puzzleRedLight2;
    [SerializeField] private MeshRenderer puzzleRedLight3;
    [SerializeField] private Material puzzleGreenLight;

    public bool CanInteract { get; set; }

    public static bool IsPuzzle1Solved = false;
    public static bool IsPuzzle2Solved = false;
    public static bool IsPuzzle3Solved = false;

    private void Awake()
    {
        CanInteract = true;

        IsPuzzle1Solved = false;
        IsPuzzle2Solved = false;
        IsPuzzle3Solved = false;
    }

    private void Update()
    {
        if (IsPuzzle1Solved && puzzleRedLight1.material != puzzleGreenLight)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play3DSFX(puzzleSolvedClip, transform.position);
            }

            puzzleRedLight1.material = puzzleGreenLight;
        }

        if (IsPuzzle2Solved && puzzleRedLight2.material != puzzleGreenLight)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play3DSFX(puzzleSolvedClip, transform.position);
            }

            puzzleRedLight2.material = puzzleGreenLight;
        }

        if (IsPuzzle3Solved && puzzleRedLight3.material != puzzleGreenLight)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play3DSFX(puzzleSolvedClip, transform.position);
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
