using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventorySlotUI[] _inventorySlots = new InventorySlotUI[4];

    private void OnEnable()
    {
        PlayerInventory.OnItemPicked += AddItem;
        PlayerInventory.OnItemDropped += RemoveItem;
        PlayerInventory.OnItemUsed += RemoveItem;
    }

    private void OnDisable()
    {
        PlayerInventory.OnItemPicked -= AddItem;
        PlayerInventory.OnItemDropped -= RemoveItem;
        PlayerInventory.OnItemUsed -= RemoveItem;
    }

    private void AddItem(ItemSO newItem)
    {
        if (newItem == null) return;

        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            if (_inventorySlots[i].HasItem) continue;

            _inventorySlots[i].SetItem(newItem);
            break;
        }
    }

    private void RemoveItem(ItemSO item)
    {
        if (item == null) return;

        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            if (_inventorySlots[i].HasItem && _inventorySlots[i].CurrentItem == item)
            {
                _inventorySlots[i].RemoveItem(item);
                break;
            }
        }
    }
}
