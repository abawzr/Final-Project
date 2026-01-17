using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("panels")]
    public GameObject MainMenuPanel;        //1-panel
    public GameObject SettingsPanel;        //2-panel
    public GameObject InstructionsPanel;    //3-panel

    [Header("Scene Loading")]
    public string gameplaySceneName="Gameplay Scene";
    void Start()
    {
        ShowMainMenu();
        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.MainMenu);
    }

    public void Gameplay()
    {
        SceneManager.LoadScene(gameplaySceneName);
        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.Gameplay);
    }
    public void QuitGame()//////////
    {
        if(GameManager.Instance)
            GameManager.Instance.QuitGame();
        else
            Application.Quit();
    }
    
    ///////////////////Panels//////////////////
    
    //1- Main Menu Panel
    public void ShowMainMenu()
    {
        if (MainMenuPanel) MainMenuPanel.SetActive(true);
        if (SettingsPanel) SettingsPanel.SetActive(false);
        if (InstructionsPanel) InstructionsPanel.SetActive(false);

        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.MainMenu);
        
    }
    //2- Settings Panel
    public void OpenSettingeFromMenu()
    {
        if (MainMenuPanel) MainMenuPanel.SetActive(false);
        if (InstructionsPanel) InstructionsPanel.SetActive(false);
        if (SettingsPanel) SettingsPanel.SetActive(true);

        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.MainMenu);
    }
    public void BackFromSettings()
    {
        ShowMainMenu();
    }
    //3- Instructions Panel
    public void InstructionsUI()
    {
        if (MainMenuPanel) MainMenuPanel.SetActive(false);
        if (SettingsPanel) SettingsPanel.SetActive(false);
        if (InstructionsPanel) InstructionsPanel.SetActive(true);

        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.MainMenu);

    }
    public void BackFromInstructions()
    {
        ShowMainMenu();
    }


}
