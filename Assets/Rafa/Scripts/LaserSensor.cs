using UnityEngine;

public class LaserSensor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]private Material redMaterial;
    [SerializeField]private Material greenMaterial;
    private Renderer objectRenderer;
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        objectRenderer.material = redMaterial;
    }

    public void ActivateSensor()
    {
        objectRenderer.material = greenMaterial;
    }
    public void UNActivateSensor()
    {
        objectRenderer.material = redMaterial;
    }
}
