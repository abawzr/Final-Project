using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemSO item;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private AudioClip openDoor;
    [SerializeField] private AudioClip lockedDoor;

    public bool CanInteract { get; set; }

    private void Awake()
    {
        CanInteract = true;
    }

    private void UnlockDoor()
    {
        CanInteract = false;

        doorAnimator.SetTrigger("Open");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSFX(openDoor, transform.position);
        }
    }

    private void DoorLocked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSFX(lockedDoor, transform.position);
        }
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (item != null && playerInventory.HasItem(item))
        {
            playerInventory.UseOrDropItem(item, isUse: true);
            UnlockDoor();
        }

        else
        {
            DoorLocked();
        }
    }
}
