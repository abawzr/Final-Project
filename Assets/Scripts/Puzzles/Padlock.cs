using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class Padlock : MonoBehaviour
{
    [SerializeField] private List<GameObject> interactableObjects;
    [SerializeField] private List<float> correctNumbers;
    [SerializeField] private List<float> currentNumbers;
    [SerializeField] private Vector3 rotationAround;
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private PuzzlePerspective puzzle;
    [SerializeField] private float timeBeforeDisablingPuzzle;

    private bool _isUnlocked;

    public bool IsUnlocked => _isUnlocked;

    private void Update()
    {
        if (_isUnlocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit) && interactableObjects.Contains(hit.collider.gameObject))
            {
                int index = interactableObjects.IndexOf(hit.collider.gameObject);

                hit.collider.transform.Rotate(rotationAround);

                currentNumbers[index] = Mathf.Round(hit.collider.transform.localEulerAngles.y);

                if (currentNumbers.SequenceEqual(correctNumbers))
                {
                    _isUnlocked = true;

                    puzzle.CanInteract = false;

                    if (chestAnimator != null)
                        chestAnimator.SetTrigger("Open");

                    StartCoroutine(PuzzleDone());
                }
            }
        }
    }

    private IEnumerator PuzzleDone()
    {
        yield return new WaitForSecondsRealtime(timeBeforeDisablingPuzzle);

        puzzle.DestoryPadlock();
    }
}
