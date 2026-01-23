using UnityEngine;

public class LaserSensor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Light lampLight; // Point Light

    [Header("Light Settings")]
    [SerializeField] private float lightOnIntensity = 3f;
    [SerializeField] private float lightOffIntensity = 0f;


    private Renderer objectRenderer;
    void Awake()
    {
        if (lampLight != null)
        {
             lampLight.enabled = true;
            lampLight.intensity = lightOffIntensity;
        }
    }
    public void ActivateSensor()
    {
        if (lampLight != null)
            lampLight.intensity = lightOnIntensity;
            Debug.Log("LaserSensor Activated");
    }
    public void DeactivateSensor()
    {
        if (lampLight != null)
            lampLight.intensity = lightOffIntensity;
    }
    
}
