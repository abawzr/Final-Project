using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Padlock : MonoBehaviour
{
    [SerializeField] private List<GameObject> interactableObjects;
    [SerializeField] private List<float> correctNumbers;
    [SerializeField] private List<float> currentNumbers;

    private bool _isUnlocked;

    public bool IsUnlocked => _isUnlocked;

    public event Action OnPadlockUnlocked;

    private void Update()
    {
        if (_isUnlocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit) && interactableObjects.Contains(hit.collider.gameObject))
            {
                int index = interactableObjects.IndexOf(hit.collider.gameObject);

                hit.collider.transform.Rotate(new Vector3(0, 0, 36f));

                currentNumbers[index] = Mathf.Round(hit.collider.transform.localEulerAngles.y);

                if (currentNumbers.SequenceEqual(correctNumbers))
                {
                    _isUnlocked = true;
                    OnPadlockUnlocked?.Invoke();
                }
            }
        }
    }
}
