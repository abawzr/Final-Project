using UnityEngine;
using TMPro;

public class Note : MonoBehaviour, IInteractable
{
    public bool CanInteract { get; set; }

    [Header("Note Text")]
    [TextArea(3, 10)]
    [SerializeField] private string noteTextEnglish;

    [TextArea(3, 10)]
    [SerializeField] private string noteTextArabic;

    [SerializeField] private GameObject noteUI; // UI to display the note
    [SerializeField] private TMP_Text englishTextTMP;
    [SerializeField] private TMP_Text arabicTextTMP;

    bool _isReading = false;
    [SerializeField] private bool useArabic = false;
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

        if (englishTextTMP != null)
        {
            englishTextTMP.text = noteTextEnglish;
            englishTextTMP.gameObject.SetActive(!useArabic);
        }

        if (arabicTextTMP != null)
        {
            arabicTextTMP.text = noteTextArabic;
            arabicTextTMP.gameObject.SetActive(useArabic);
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
