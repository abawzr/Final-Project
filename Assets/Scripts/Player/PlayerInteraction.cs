using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float interactionRayDistance;
    [SerializeField] private LayerMask interacitonLayer;
    [SerializeField] private TMP_Text interactionTMP;
    [SerializeField] private string interactionText;
    [SerializeField] private string pickupText;
    [SerializeField] private string exitText;

    private IInteractable _currentTarget;
    private PlayerInventory _playerInventory;

    public static bool CanInteract { get; set; }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(playerCamera.position, playerCamera.forward * interactionRayDistance);
    }

    private void Awake()
    {
        _playerInventory = GetComponent<PlayerInventory>();

        _currentTarget = null;
        CanInteract = true;
    }

    private void Update()
    {
        if (!CanInteract) return;

        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hitInfo, interactionRayDistance, interacitonLayer))
        {
            if (hitInfo.collider.TryGetComponent(out IInteractable interactableObject))
            {
                if (_currentTarget != interactableObject)
                {
                    _currentTarget = interactableObject;

                    if ((interactableObject is PuzzlePerspective || interactableObject is Door) && interactableObject.CanInteract)
                        interactionTMP.text = interactionText;

                    else if (interactableObject is PickableItem)
                        interactionTMP.text = pickupText;
                }

                if (Input.GetButtonDown("Interact") && interactableObject.CanInteract)
                {
                    SetCurrentTargetNull();

                    if (interactableObject is PuzzlePerspective)
                        interactionTMP.text = exitText;

                    interactableObject.Interact(_playerInventory);
                }
            }
            else
            {
                SetCurrentTargetNull();
            }
        }
        else
        {
            SetCurrentTargetNull();
        }
    }

    private void SetCurrentTargetNull()
    {
        _currentTarget = null;

        interactionTMP.text = string.Empty;
    }
}
