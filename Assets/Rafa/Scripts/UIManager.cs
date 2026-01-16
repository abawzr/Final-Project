using System;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [Header("Panels")]
    public GameObject MainMenuPanel;    //1-panel
    public GameObject SettingsPanel;    //2-panel
    public GameObject InstructionsPanel;//3-panel
    public GameObject PausePanel;       //4-panel
    public GameObject FAKEEndPanel;     //5-panel
    public GameObject End1Panel;        //6-panel
    public GameObject End2Panel;        //7-panel


    [Header("Scens Loading")]
    public bool loadGamePlaySceneOnPlay=false;
    public string gameplaySceneName = "Gameplay";
    public string menuSceneName = "UI Scene";

    bool isPaused;
    bool GameStarted;

    enum SettingsOpenedFrom { Menu, Pause }
    SettingsOpenedFrom settingsFrom = SettingsOpenedFrom.Menu;
    
    void Start()
    {
        isPaused=false;
        GameStarted=false;
        MainMenu();

        //Hide all panels before starting
        /*if(MainMenuPanel!=null){MainMenuPanel.SetActive(false);}            //1-panel
        if(SettingsPanel!=null){SettingsPanel.SetActive(false);}            //2-panel
        if(InstructionsPanel!=null){InstructionsPanel.SetActive(false);}    //3-panel
        if(PausePanel!=null){PausePanel.SetActive(false);}                  //4-panel
        if(FAKEEndPanel!=null){FAKEEndPanel.SetActive(false);}              //5-panel
        if(End1Panel!=null){End1Panel.SetActive(false);}                    //6-panel
        if(End2Panel!=null){End2Panel.SetActive(false);}                    //7-panel
        */

    }

    // Update is called once per frame
    void Update()
    {
        if(!GameStarted)return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseToggel();
        }
        
    }
    //1-panel Main Menue
    public void GamePlay()
    {   
        isPaused=false;
        GameStarted=true;

        if (loadGamePlaySceneOnPlay)
        {
            Time.timeScale=1f;
            SceneManager.LoadScene(gameplaySceneName);
            return;
        }

        if (MainMenuPanel)MainMenuPanel.SetActive(false);
        if(SettingsPanel)SettingsPanel.SetActive(false);
        if(InstructionsPanel)InstructionsPanel.SetActive(false);

        ClosePause();
        CloseEnds();

        Time.timeScale=1f;
        LockCursor();
        
    }
    public void MainMenu()
    {
        isPaused=false;
        GameStarted=false;

        if(MainMenuPanel)MainMenuPanel.SetActive(true);
        if(SettingsPanel)SettingsPanel.SetActive(false);
        if(InstructionsPanel)InstructionsPanel.SetActive(false);
        if(PausePanel)PausePanel.SetActive(false);
        if(FAKEEndPanel)FAKEEndPanel.SetActive(false);
        if(End1Panel)End1Panel.SetActive(false);
        if(End2Panel)End2Panel.SetActive(false);
        
        Time.timeScale=0f;
        UnlockCursor();
    }
    //2- Settings panel
    public void SettingsUI()
    {
        
        if(MainMenuPanel)MainMenuPanel.SetActive(false);
        if(InstructionsPanel)InstructionsPanel.SetActive(false);
        if(PausePanel)PausePanel.SetActive(false);
        if(SettingsPanel)SettingsPanel.SetActive(true);

        Time.timeScale=0f;
        UnlockCursor();
    }
    public void OpenSettingeFromMenu()
    {
        settingsFrom=SettingsOpenedFrom.Menu;

        if(MainMenuPanel)MainMenuPanel.SetActive(false);
        if(PausePanel)PausePanel.SetActive(false);
        if(InstructionsPanel)InstructionsPanel.SetActive(false);
        if(SettingsPanel)SettingsPanel.SetActive(true);


        Time.timeScale=0f;
        UnlockCursor();

    }
    public void OpenSettingeFromPause()
    {
        settingsFrom=SettingsOpenedFrom.Pause;

        if(MainMenuPanel)MainMenuPanel.SetActive(false);
        if(InstructionsPanel)InstructionsPanel.SetActive(false);
        if(PausePanel)PausePanel.SetActive(false);
        if(SettingsPanel)SettingsPanel.SetActive(true);

        Time.timeScale=0f;
        UnlockCursor();
    }
    public void BackFromSettings()
    {
        if(SettingsPanel)SettingsPanel.SetActive(false);

        if (settingsFrom == SettingsOpenedFrom.Menu)
        {
            if(PausePanel)PausePanel.SetActive(false);
            if(InstructionsPanel)InstructionsPanel.SetActive(false);
            if(MainMenuPanel)MainMenuPanel.SetActive(true);
            Time.timeScale = 0f;
            UnlockCursor();
        }
        else // from Pause
        {
            if(InstructionsPanel)InstructionsPanel.SetActive(false);
            if(MainMenuPanel)MainMenuPanel.SetActive(false);
            if(PausePanel)PausePanel.SetActive(true);
            Time.timeScale = 0f;
            UnlockCursor();
            isPaused = true;
        }
    }
    //3-Instructions panel
    public void InstructionsUI()
    {

        if(MainMenuPanel)MainMenuPanel.SetActive(false);
        if(SettingsPanel)SettingsPanel.SetActive(false);
        if(InstructionsPanel)InstructionsPanel.SetActive(true);

        Time.timeScale=0f;
        UnlockCursor();
    }
    public void BackFromInstructions()
    {
        if(InstructionsPanel)InstructionsPanel.SetActive(false);
        if(MainMenuPanel)MainMenuPanel.SetActive(true);

        Time.timeScale = 0f;
        UnlockCursor();
    }
    //4-Pause panel
    public void PauseToggel()
    {

        if (isPaused)
        {
            ContinueGame();
        }
        else
        {
            PauseUI();
        }
    }
    public void PauseUI()
    {
        isPaused=true;
        //GameStarted=false;

        if(PausePanel)PausePanel.SetActive(true);
        Time.timeScale = 0f;
        UnlockCursor();

    }
    
    public void ContinueGame()
    {
        isPaused=false;
        if(PausePanel)PausePanel.SetActive(false);
        if(SettingsPanel)SettingsPanel.SetActive(false);
        if(InstructionsPanel)InstructionsPanel.SetActive(false);
        if(MainMenuPanel)MainMenuPanel.SetActive(false);

        Time.timeScale=1f;
        LockCursor();
    }
    void ClosePause()
    {
        isPaused = false;
        if (PausePanel)PausePanel.SetActive(false);

        
    }
    //5-Fake end panel
    public void EndUI()
    {
        isPaused=false;
        if(FAKEEndPanel)FAKEEndPanel.SetActive(true);

        Time.timeScale=0f;
        UnlockCursor();

    }
    //6-End 1 panel
    public void End1UI()
    {
        if(End1Panel)End1Panel.SetActive(true);

        Time.timeScale=0f;
        UnlockCursor();
    }
    //7-End 2 panel
    public void End2UI()
    {
        if(End2Panel)End2Panel.SetActive(true);

        Time.timeScale=0f;
        UnlockCursor();
    }
    public void CloseEnds()
     {
        if(End1Panel)End1Panel.SetActive(false);
        if(End2Panel)End2Panel.SetActive(false);
        if(FAKEEndPanel)FAKEEndPanel.SetActive(false);
        
    }
   
    ///////////////////////////////////////
    public void BackToMenu() => MainMenu();
    
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    //////////////////////////////////////
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}
