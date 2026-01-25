using UnityEngine;

public class CubeController : MonoBehaviour
{
    [SerializeField] float rotateAngle = 10f;

    public static bool CanInteract = true;

    private void Awake()
    {
        CanInteract = true;
    }

    void Update()
    {
        if (!CanInteract) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Cube"))
                {
                    var rotator = hit.transform.GetComponentInParent<MirrorRotator>();
                    rotator?.RotateStep();

                }
            }
        }
    }

}
