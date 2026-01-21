using UnityEngine;

public class CubeController : MonoBehaviour
{   [SerializeField]float rotateAngle=15f;
     void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
           Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray,out hit))
            {
                if(hit.transform.CompareTag("Cube"))
                {   //rotate the cube 15 degrees around y axis
                    Vector3 currentAngle = hit.transform.eulerAngles;
                    float nextY = currentAngle.y + rotateAngle;
                    hit.transform.eulerAngles = new Vector3(currentAngle.x, nextY, currentAngle.z);
                }
            }
        }
    }
    
}
