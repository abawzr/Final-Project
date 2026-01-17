using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    void OnEnable()
    {
        ApplyAll();
    }

    // Update is called once per frame
    public void OnMasterChange(float _)
    {
        if(AudioManager.Instance)
            AudioManager.Instance.OnMasterVolumeChanged();
    }
    public void OnMusicChange(float _)
    {
        if(AudioManager.Instance)
            AudioManager.Instance.OnMusicVolumeChanged();
    }
    public void OnSFXChange(float _)
    {
        if(AudioManager.Instance)
            AudioManager.Instance.OnSFXVolumeChanged();
    }
    void ApplyAll()
    {
        OnMasterChange(0);
        OnMusicChange(0);
        OnSFXChange(0);
    }
}
