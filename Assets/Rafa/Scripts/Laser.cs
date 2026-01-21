using UnityEditor;
using UnityEngine;

public class Laser : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Transform oragin;
    private float LaserLength = 100f;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int maxReflections = 5;


    // Update is called once per frame
    void Update()
    {
        Vector3 direction = transform.forward;
        Ray ray = new Ray(oragin.position, direction);
        Vector3[]points= new Vector3[maxReflections+2];
        int pointCount=0;
        points[pointCount++]= oragin.position; //pointcount=0  ,then pointcount=1
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, LaserLength))
        {
            points[pointCount++]= hit.point;
            

            //check if hit the cube
            if (hit.transform.CompareTag("Cube"))
            {
                CubeCase(hit, ray,points, ref pointCount, 1);
            }else if (hit.transform.CompareTag("LaserSensor"))
            {
                SensorCase(hit);
        
            }
        }
        else
        {
            points[pointCount++]= oragin.position + direction * LaserLength;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, oragin.position);
            lineRenderer.SetPosition(1, oragin.position + direction * LaserLength);
        }
        //apply points to line renderer
        lineRenderer.positionCount = pointCount;
        for (int i = 0; i < pointCount; i++)
        lineRenderer.SetPosition(i, points[i]);
    }
    void CubeCase(RaycastHit hit, Ray ray,Vector3[] points, ref int pointCount, int lastIndex)
    {   
        if(pointCount>=points.Length)return; 
        
        Vector3 reflectDir = Vector3.Reflect(ray.direction, hit.normal);
        Ray reflictRay= new Ray(hit.point + reflectDir * 0.01f, reflectDir);
        RaycastHit reflectHit;

        //do a raycast from hit point as areflect ray
        if (Physics.Raycast(reflictRay, out reflectHit, LaserLength))
        {
            points[pointCount++]= reflectHit.point;
            //if hit the door sensor activate it
            if (reflectHit.transform.CompareTag("LaserSensor"))
            {
                SensorCase(reflectHit);
                return;
            }
            else if (reflectHit.transform.CompareTag("Cube"))
            {
                CubeCase(reflectHit, reflictRay, points, ref pointCount, 1);
                return;
            }
            return;
        }else
            {
                // if(pointCount<points.Length)
                // points[pointCount++]=reflictRay.origin+reflictRay.direction*LaserLength;
            }
    }
    void SensorCase(RaycastHit hit)
    {
        LaserSensor sensor=hit.transform.GetComponent<LaserSensor>();
        if(sensor!=null)
        {
            sensor.ActivateSensor();
        }
    }
        

}
