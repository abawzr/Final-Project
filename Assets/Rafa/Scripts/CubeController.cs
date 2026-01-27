using UnityEngine;
using System.Collections;


public class CubeController : MonoBehaviour
{
    [SerializeField] private float stepAnngle = 25f;
    [SerializeField] private float rotationSpeed = 360f;
    private bool _isRotating;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Cube"))
                {
                    CubeController rotate = hit.transform.GetComponent<CubeController>();
                    rotate?.RotateStep();
                }
            }
        }
    }

    public void RotateStep()
    {
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
