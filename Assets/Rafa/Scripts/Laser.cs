using UnityEditor;
using UnityEngine;

public class Laser : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    ///_values??
    [SerializeField] private Transform oragin;  //laser start point
    private float LaserLength = 50f;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int maxReflections = 10;
    private LaserSensor _lastSensor;          //Last activated sensor



    // Update is called once per frame
    void Awake()
    {
        if (_lastSensor != null)
        {
            _lastSensor.DeactivateSensor();   //turn off previous sensor
            _lastSensor = null;
        }
    }
    void Update()
    {
        
        // Initial ray
        Vector3 direction = transform.forward;
        Ray ray = new Ray(oragin.position, direction);

        //array to store points
        Vector3[]points= new Vector3[maxReflections+2];
        int pointCount=0;
        points[pointCount++]= oragin.position; //pointcount=0  ,then pointcount=1
        RaycastHit hit;

        //Raycast and reflection logic//
        if (Physics.Raycast(ray, out hit, LaserLength))
        {
            points[pointCount++]= hit.point;
            

            //check if hit the cube
            if (hit.transform.CompareTag("Cube"))
            {   
                //handle reflection recursively
                CubeCase(hit, ray,points, ref pointCount, 1);
            }else if (hit.transform.CompareTag("LaserSensor"))
            {   
                //handle sensor activation
                SensorCase(hit);
            }
        }
        else
        {   
            //no hit draw full laser
            points[pointCount++]= oragin.position + direction * LaserLength;//??
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
    {   //check array bound
        if(pointCount>=points.Length)return; 
        
        //calculate reflection direction
        Vector3 reflectDir = Vector3.Reflect(ray.direction, hit.normal);
        Ray reflictRay= new Ray(hit.point + reflectDir * 0.01f, reflectDir);
        RaycastHit reflectHit;

        //do a raycast from hit point as areflect ray
        if (Physics.Raycast(reflictRay, out reflectHit, LaserLength))
        {
            bool valid = reflectHit.transform.CompareTag("Cube") || reflectHit.transform.CompareTag("LaserSensor");

            if (!valid)
            {
                // stop at the mirror hit point (do not draw reflection)
                return;
            }

            // save hit point
            points[pointCount++] = reflectHit.point;

            if (reflectHit.transform.CompareTag("LaserSensor"))
            {
                SensorCase(reflectHit);
                return;
            }

            // hit mirror
            CubeCase(reflectHit, reflictRay, points, ref pointCount, 1);
            return;
        }
        else
        {
            //hit nothing so no reflection line
            return;
        }
    }
    void SensorCase(RaycastHit hit)
    {
        LaserSensor sensor=hit.transform.GetComponent<LaserSensor>();
        if(sensor!=null)
        {
            sensor.ActivateSensor();
            _lastSensor = sensor;
        }
    }
        

}
