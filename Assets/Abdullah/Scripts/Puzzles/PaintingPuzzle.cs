using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PaintingPuzzle : MonoBehaviour
{
    [SerializeField] private List<GameObject> correctSequence;
    [SerializeField] private List<GameObject> currentSequence;
    [SerializeField] private PuzzlePerspective puzzlePerspective;

    private Collider _selectedObject;
    private Vector3 _tempPosition;
    private bool _isSolved;

    private void Update()
    {
        if (_isSolved) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit) && currentSequence.Contains(hit.collider.gameObject))
            {
                if (_selectedObject == null)
                {
                    _selectedObject = hit.collider;
                }

                else
                {
                    if (hit.collider == _selectedObject) _selectedObject = null;

                    else
                    {
                        _tempPosition = hit.collider.transform.localPosition;

                        int tempIndex = currentSequence.IndexOf(hit.collider.gameObject);
                        int selectedObjectIndex = currentSequence.IndexOf(_selectedObject.gameObject);

                        currentSequence[selectedObjectIndex] = hit.collider.gameObject;
                        currentSequence[tempIndex] = _selectedObject.gameObject;

                        hit.collider.transform.localPosition = _selectedObject.transform.localPosition;
                        _selectedObject.transform.localPosition = _tempPosition;

                        _selectedObject = null;

                        if (currentSequence.SequenceEqual(correctSequence))
                        {
                            _isSolved = true;
                            puzzlePerspective.CanInteract = false;
                            puzzlePerspective.DisablePuzzle();
                            LabDoor.IsPuzzle2Solved = true;
                        }
                    }
                }
            }
        }
    }
}
