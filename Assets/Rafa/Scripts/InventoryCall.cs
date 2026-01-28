using System.Runtime.Serialization;
using UnityEngine;
using System.Collections;

public class InventoryCall : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private Animator animator;
    [SerializeField] private float claoseAnimationTime = 0.2f;
    private bool _isOpen;
    void Start()
    {
        _isOpen = false;
        if (inventoryUI != null)
            inventoryUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        _isOpen = !_isOpen;
        if (_isOpen)
            Open();
        else
            Close();
    }
    private void Open()
    {
        if (!inventoryUI) return;
        inventoryUI.SetActive(true);
        if (animator != null) animator.SetBool("isOpen", true);
        if (animator != null) animator.SetBool("isClose", false);

    }
    private void Close()
    {
        if (!inventoryUI) return;
        inventoryUI.SetActive(false);
        if (animator != null) animator.SetBool("isClose", true);
        if (animator != null) animator.SetBool("isOpen", false);

        StartCoroutine(DisableAfterClose());


    }
    private IEnumerator DisableAfterClose()
    {
        yield return new WaitForSeconds(claoseAnimationTime);
        if (!_isOpen && inventoryUI)
            inventoryUI.SetActive(false);
    }
}