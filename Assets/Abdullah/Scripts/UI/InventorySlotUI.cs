using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text itemName;

    [SerializeField] private Image itemIcon;

    private ItemSO currentItem;

    public ItemSO CurrentItem => currentItem;

    public bool HasItem { get; private set; } = false;

    private void Awake()
    {
        currentItem = null;
    }

    public void SetItem(ItemSO newItem)
    {
        if (HasItem) return;

        currentItem = newItem;
        itemIcon.sprite = newItem.itemIcon;
        itemIcon.color = new Color(itemIcon.color.r, itemIcon.color.g, itemIcon.color.b, 1f);
        itemIcon.enabled = true;
        itemIcon.preserveAspect = true;
        itemName.text = newItem.itemName;
        HasItem = true;
    }

    public void RemoveItem(ItemSO item)
    {
        if (currentItem == item)
        {
            currentItem = null;
            itemIcon.sprite = null;
            itemIcon.color = new Color(itemIcon.color.r, itemIcon.color.g, itemIcon.color.b, 0f);
            itemIcon.enabled = false;
            itemName.text = string.Empty;
            HasItem = false;
        }
    }
}
