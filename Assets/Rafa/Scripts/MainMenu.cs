using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("panels")]
    public GameObject MainMenuPanel;        //1-panel
    public GameObject SettingsPanel;        //2-panel
    public GameObject InstructionsPanel;    //3-panel

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button backControlsButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button backSettingsButton;
    [SerializeField] private Button quitButton;

    [Header("Audio Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        controlsButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        backControlsButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        backSettingsButton.onClick.RemoveAllListeners();

        controlsButton.onClick.AddListener(ControlsUI);
        backControlsButton.onClick.AddListener(BackFromControls);
        settingsButton.onClick.AddListener(OpenSettingeFromMenu);
        backSettingsButton.onClick.AddListener(BackFromSettings);

        if (GameManager.Instance != null)
        {
            startButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(GameManager.Instance.StartGame);
            quitButton.onClick.AddListener(GameManager.Instance.QuitGame);
        }

        if (AudioManager.Instance != null)
        {
            // Load Volumes to sliders
            if (masterSlider != null)
            {
                AudioManager.Instance.LoadVolume("MasterVolume", masterSlider, 0);
                masterSlider.onValueChanged.RemoveAllListeners();
                masterSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
            }
            if (musicSlider != null)
            {
                AudioManager.Instance.LoadVolume("MusicVolume", musicSlider, 0);
                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            }
            if (sfxSlider != null)
            {
                AudioManager.Instance.LoadVolume("SFXVolume", sfxSlider, 0);
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
            }
        }

        ShowMainMenu();
    }

    ///////////////////Panels//////////////////

    //1- Main Menu Panel
    public void ShowMainMenu()
    {
        if (MainMenuPanel) MainMenuPanel.SetActive(true);
        if (SettingsPanel) SettingsPanel.SetActive(false);
        if (InstructionsPanel) InstructionsPanel.SetActive(false);
    }
    //2- Settings Panel
    public void OpenSettingeFromMenu()
    {
        if (MainMenuPanel) MainMenuPanel.SetActive(false);
        if (InstructionsPanel) InstructionsPanel.SetActive(false);
        if (SettingsPanel) SettingsPanel.SetActive(true);
    }
    public void BackFromSettings()
    {
        ShowMainMenu();
    }
    //3- Instructions Panel
    public void ControlsUI()
    {
        if (MainMenuPanel) MainMenuPanel.SetActive(false);
        if (SettingsPanel) SettingsPanel.SetActive(false);
        if (InstructionsPanel) InstructionsPanel.SetActive(true);
    }
    public void BackFromControls()
    {
        ShowMainMenu();
    }
}
