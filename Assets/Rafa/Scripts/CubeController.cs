using UnityEngine;

public class CubeController : MonoBehaviour
{   [SerializeField]float rotateAngle=10f;
     void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
           Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray,out hit))
            {
                if(hit.transform.CompareTag("Cube"))
                {   
                var rotator = hit.transform.GetComponentInParent<MirrorRotator>();
                Debug.Log("Hit mirror: " + hit.transform.name + " rotator? " + (rotator != null));
                rotator?.RotateStep();

                }
            }
        }
    }
    
}
