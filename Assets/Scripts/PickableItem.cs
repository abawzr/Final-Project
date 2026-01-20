using UnityEngine;

public class PickableItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemSO item;

    public void Interact(PlayerInventory playerInventory)
    {
        if (playerInventory != null && !playerInventory.IsInventoryFull())
        {
            playerInventory.AddItem(item);
            Destroy(gameObject);
        }
    }
}
