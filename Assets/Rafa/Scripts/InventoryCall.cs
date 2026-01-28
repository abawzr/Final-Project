using System.Runtime.Serialization;
using UnityEngine;
using System.Collections;

public class InventoryCall : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private Animator animator;
    private bool _isOpen;
    private Coroutine _closeCoroutine;
    [SerializeField] private float claoseAnimationTime = 10f;

    void Start()
    {
        _isOpen = false;

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
    public void Open()
    {

        if (animator != null) animator.SetBool("isOpen", true);

        _closeCoroutine = StartCoroutine(DisableAfterClose());

    }
    private void Close()
    {
        if (animator != null) animator.SetBool("isOpen", false);
        if (_closeCoroutine != null)
        {
            StopCoroutine(_closeCoroutine);
            _closeCoroutine = null;
        }
    }
    private IEnumerator DisableAfterClose()
    {
        yield return new WaitForSeconds(claoseAnimationTime);
        Close();
    }
}