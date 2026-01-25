using UnityEngine;

public class FloatAndSpin : MonoBehaviour
{
    [Header("Float")]
    public bool enableFloat = true;
    public float floatAmplitude = 0.25f;
    public float floatSpeed = 1.5f;
    public Vector3 floatAxis = Vector3.up;
    public bool useLocalSpaceForFloat = false;

    [Header("Spin")]
    public bool enableSpin = true;
    public Vector3 spinAxis = Vector3.up;
    public float spinSpeed = 90f;
    public bool useLocalSpaceForSpin = true;

    [Header("Optional")]
    public bool randomizeStartPhase = true;

    private Vector3 _startPosition;
    private float _phaseOffset;

    private void Awake()
    {
        _startPosition = useLocalSpaceForFloat ? transform.localPosition : transform.position;
        _phaseOffset = randomizeStartPhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    private void Update()
    {
        if (enableFloat)
        {
            Vector3 axis = floatAxis.sqrMagnitude > 0f ? floatAxis.normalized : Vector3.up;
            float offset = Mathf.Sin((Time.unscaledTime * floatSpeed) + _phaseOffset) * floatAmplitude;

            if (useLocalSpaceForFloat)
                transform.localPosition = _startPosition + axis * offset;
            else
                transform.position = _startPosition + axis * offset;
        }

        if (enableSpin)
        {
            Vector3 axis = spinAxis.sqrMagnitude > 0f ? spinAxis.normalized : Vector3.up;
            float angle = spinSpeed * Time.unscaledDeltaTime;

            if (useLocalSpaceForSpin)
                transform.Rotate(axis, angle, Space.Self);
            else
                transform.Rotate(axis, angle, Space.World);
        }
    }

    private void OnValidate()
    {
        floatAmplitude = Mathf.Max(0f, floatAmplitude);
        floatSpeed = Mathf.Max(0f, floatSpeed);
    }
}
