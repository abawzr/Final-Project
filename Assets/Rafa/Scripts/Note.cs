using UnityEngine;
using TMPro;

public class Note : MonoBehaviour, IInteractable
{
    public bool CanInteract { get; set; }

    [SerializeField] private string noteText; // text on the note

    [SerializeField] private GameObject noteUI; // UI to display the note
    [SerializeField] private TMP_Text noteTextTMP;
    bool _isReading = false;
    private void Awake()
    {
        CanInteract = true;


        if (noteUI)
            noteUI.SetActive(false);
    }
    public void Interact(PlayerInventory playerInventory)
    {
        // pick up note to read on E key press drop it on E key press 

        if (_isReading)
        {
            CloseNote();
        }
        else
        {
            OpenNote();

        }
    }
    private void OpenNote()
    {
        _isReading = true;

        if (noteTextTMP != null)
        {
            noteTextTMP.text = noteText;
        }

        if (noteUI != null)
        {
            noteUI.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void CloseNote()
    {
        _isReading = false;

        if (noteUI != null)
        {
            noteUI.SetActive(false);
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
