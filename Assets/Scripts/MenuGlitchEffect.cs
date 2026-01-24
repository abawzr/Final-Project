using UnityEngine;

public class MenuGlitchEffect : MonoBehaviour
{
    [SerializeField] private Material glitchEffect;
    [SerializeField] private float intensity;
    [SerializeField] private float chromaticSplit;
    [SerializeField] private float noiseAmount;

    private void Start()
    {
        glitchEffect.SetFloat("_Intensity", intensity);
        glitchEffect.SetFloat("_ChromaticSplit", chromaticSplit);
        glitchEffect.SetFloat("_NoiseAmount", noiseAmount);
    }
}
