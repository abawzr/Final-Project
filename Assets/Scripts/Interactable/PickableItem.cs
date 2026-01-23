using UnityEngine;

public class PickableItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemSO item;

    public bool CanInteract { get; set; }

    private void Awake()
    {
        CanInteract = true;
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (playerInventory != null && !playerInventory.IsInventoryFull())
        {
            CanInteract = false;
            playerInventory.AddItem(item);
            Destroy(gameObject);
        }
    }
}
