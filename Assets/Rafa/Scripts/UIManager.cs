using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject canvas;
    public GameObject SettingsPanel;    //1-panel
    public GameObject PausePanel;       //2-panel

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Audio Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private bool _isPaused = false;

    public static bool CanPause;

    private void Awake()
    {
        // Add listener to restart and quit buttons
        restartButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();

        if (GameManager.Instance != null)
        {
            restartButton.onClick.AddListener(GameManager.Instance.Restart);
            quitButton.onClick.AddListener(GameManager.Instance.BackToMenu);
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
    }

    private void Start()
    {
        _isPaused = false;
        CloseAllGameplayPanels();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseToggel();
        }
    }

    //////////////HIDE ALL////////////////
    private void CloseAllGameplayPanels()
    {
        //Hide all panels before starting
        if (canvas != null) { canvas.SetActive(false); }
        if (SettingsPanel != null) { SettingsPanel.SetActive(false); }            //1-panel
        if (PausePanel != null) { PausePanel.SetActive(false); }                  //2-panel
    }

    ///////////////////Panels//////////////////
    /// 1-Pause panel
    public void PauseToggel()
    {
        if (_isPaused)
        {
            ContinueGame();
        }
        else if (CanPause)
        {
            PauseUI();
        }
    }

    public void PauseUI()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.Pause);

        _isPaused = true;
        CloseAllGameplayPanels();
        if (canvas) canvas.SetActive(true);
        if (PausePanel) PausePanel.SetActive(true);
    }

    //Continue Game after Pause
    public void ContinueGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.Gameplay);

        _isPaused = false;
        CloseAllGameplayPanels();

        if (PausePanel) PausePanel.SetActive(false);
        if (canvas) canvas.SetActive(false);
    }

    //2- Settings Panel
    //Open Settinge From Pause NOT menu
    public void OpenSettingeFromPause()
    {
        if (PausePanel) PausePanel.SetActive(false);
        if (SettingsPanel) SettingsPanel.SetActive(true);
    }

    public void BackFromSettings()
    {
        if (SettingsPanel) SettingsPanel.SetActive(false);
        if (PausePanel) PausePanel.SetActive(true);
    }
}

