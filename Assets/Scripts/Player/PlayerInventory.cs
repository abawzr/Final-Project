using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    /// <summary>
    /// Fixed-size array of items representing the player's inventory slots.
    /// </summary>
    private ItemSO[] _items = new ItemSO[4];

    /// <summary>
    /// Fired when an item is added to the inventory.
    /// </summary>
    public static event Action<ItemSO> OnItemPicked;

    /// <summary>
    /// Fired when an item is dropped from the inventory.
    /// </summary>
    public static event Action<ItemSO> OnItemDropped;

    /// <summary>
    /// Fired when an item is used from the inventory.
    /// </summary>
    public static event Action<ItemSO> OnItemUsed;

    /// <summary>
    /// Adds a new item to the first available empty slot in the inventory.
    /// </summary>
    /// <param name="newItem">The item to add.</param>
    /// <remarks>
    /// Fires <see cref="OnItemPicked"/> when an item is successfully added.
    /// If the inventory is full or the item is null, the method does nothing.
    /// </remarks>
    public void AddItem(ItemSO newItem)
    {
        if (newItem == null) return;

        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null)
            {
                _items[i] = newItem;
                OnItemPicked?.Invoke(newItem);
                return;
            }
        }
    }

    /// <summary>
    /// Uses or drops a specific item from the inventory.
    /// </summary>
    /// <param name="usedItem">The item to use or drop.</param>
    /// <param name="isUse">If true, the item is used; if false, the item is dropped.</param>
    /// <remarks>
    /// Fires <see cref="OnItemUsed"/> or <see cref="OnItemDropped"/> depending on <paramref name="isUse"/>.
    /// If the item is not found or null, the method does nothing.
    /// </remarks>
    public void UseOrDropItem(ItemSO usedItem, bool isUse)
    {
        if (usedItem == null) return;

        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == usedItem)
            {
                _items[i] = null;
                if (isUse)
                    OnItemUsed?.Invoke(usedItem);
                else
                    OnItemDropped?.Invoke(usedItem);

                return;
            }
        }
    }

    /// <summary>
    /// Checks if a specific item exists in the inventory.
    /// </summary>
    /// <param name="item">The item to check for.</param>
    /// <returns>True if the item exists in the inventory; otherwise, false.</returns>
    public bool HasItem(ItemSO item)
    {
        foreach (ItemSO inventoryItem in _items)
        {
            if (inventoryItem == item)
                return true;
        }
        return false;
    }

    public bool IsInventoryFull()
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null) return false;
        }

        return true;
    }
}
