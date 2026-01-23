using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class Padlock : MonoBehaviour
{
    [Header("Wheels")]
    [SerializeField] private List<GameObject> wheels;
    [SerializeField] private List<int> correctNumbers;
    [SerializeField] private List<int> currentNumbers;

    [Header("Rotation")]
    [SerializeField] private float stepAngle = 75f;
    [SerializeField] private float rotateDuration = 0.1f;

    [Header("Puzzle")]
    [SerializeField] private Animator animator;
    [SerializeField] private PuzzlePerspective puzzle;
    [SerializeField] private Collider chestCollider;
    [SerializeField] private float timeBeforeDisablingPuzzle = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip chestOpenClip;

    private bool _isUnlocked;
    private bool _isRotating;

    public bool IsUnlocked => _isUnlocked;

    private void Awake()
    {
        if (currentNumbers.Count != wheels.Count)
        {
            currentNumbers = new List<int>(new int[wheels.Count]);
        }
    }

    private void Update()
    {
        if (_isUnlocked || _isRotating) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                int index = wheels.IndexOf(hit.collider.gameObject);
                if (index == -1) return;

                RotateWheel(index);
            }
        }
    }

    private void RotateWheel(int index)
    {
        _isRotating = true;

        currentNumbers[index] = (currentNumbers[index] + 1) % 5;

        Quaternion targetRotation = Quaternion.Euler(currentNumbers[index] * stepAngle, 0f, 0f);

        StartCoroutine(RotateSmooth(wheels[index].transform, targetRotation));

        if (currentNumbers.SequenceEqual(correctNumbers))
        {
            Unlock();
        }
    }

    private IEnumerator RotateSmooth(Transform wheel, Quaternion targetRotation)
    {
        Quaternion startRotation = wheel.localRotation;
        float elapsed = 0f;

        while (elapsed < rotateDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float time = elapsed / rotateDuration;

            wheel.localRotation = Quaternion.Slerp(startRotation, targetRotation, time);
            yield return null;
        }

        wheel.localRotation = targetRotation;
        _isRotating = false;
    }

    private void Unlock()
    {
        puzzle.CanInteract = false;

        if (animator != null)
        {
            animator.SetTrigger("Open");
        }

        StartCoroutine(PuzzleDone());
    }

    private IEnumerator PuzzleDone()
    {
        if (AudioManager.Instance != null && chestOpenClip != null)
        {
            AudioManager.Instance.Play3DSFX(chestOpenClip, transform.position);
        }

        if (chestCollider != null)
        {
            chestCollider.enabled = false;
        }

        yield return new WaitForSecondsRealtime(timeBeforeDisablingPuzzle);

        _isUnlocked = true;

        puzzle.DestoryPadlock();
    }
}
