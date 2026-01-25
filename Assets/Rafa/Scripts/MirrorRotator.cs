using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class MirrorRotator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [SerializeField] private float stepAnngle = 15f;
    [SerializeField] private float rotationSpeed = 360f;

    private bool _isRotating;

    public void RotateStep()
    {
        Debug.Log("RotateStep called on " + gameObject.name);

        if (_isRotating) return; 

        Quaternion target =
            transform.rotation * Quaternion.Euler(0f, stepAnngle, 0f);

        StartCoroutine(RotateTo(target));
    }

    private IEnumerator RotateTo(Quaternion target)
    {
        _isRotating = true;

        while (Quaternion.Angle(transform.rotation, target) > 0.05f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.rotation = target;
        _isRotating = false;
    }
}
