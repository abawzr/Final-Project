using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject canvas;
    public GameObject SettingsPanel;    //1-panel
    public GameObject PausePanel;       //2-panel
    [SerializeField] private GameObject gameplayCanvas;
    [SerializeField] private GameObject screenEffectsCanvas;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Audio Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

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
        CanPause = true;
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
        if (CanPause)
        {
            PauseUI();
        }
        else
        {
            ContinueGame();
        }
    }

    public void PauseUI()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.Pause);

        CanPause = false;
        CloseAllGameplayPanels();

        if (canvas != null) canvas.SetActive(true);
        if (PausePanel != null) PausePanel.SetActive(true);
        if (gameplayCanvas != null) gameplayCanvas.SetActive(false);
        if (screenEffectsCanvas != null) gameplayCanvas.SetActive(false);
    }

    //Continue Game after Pause
    public void ContinueGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.Gameplay);

        CanPause = true;
        CloseAllGameplayPanels();

        if (PausePanel != null) PausePanel.SetActive(false);
        if (canvas != null) canvas.SetActive(false);
        if (gameplayCanvas != null) gameplayCanvas.SetActive(true);
        if (screenEffectsCanvas != null) gameplayCanvas.SetActive(true);
    }

    //2- Settings Panel
    //Open Settinge From Pause NOT menu
    public void OpenSettingeFromPause()
    {
        if (PausePanel != null) PausePanel.SetActive(false);
        if (SettingsPanel != null) SettingsPanel.SetActive(true);
    }

    public void BackFromSettings()
    {
        if (SettingsPanel != null) SettingsPanel.SetActive(false);
        if (PausePanel != null) PausePanel.SetActive(true);
    }
}

