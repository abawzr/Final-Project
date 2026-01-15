using System;
using TMPro;
using UnityEngine;

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



    bool isPaused;
    bool GameStarted;
    enum SettingsOpenedFrom { Menu, Pause }
    SettingsOpenedFrom settingsFrom = SettingsOpenedFrom.Menu;
    
    void Start()
    {
        isPaused=false;
        GameStarted=false;
        //Hide all panels before starting
        if(MainMenuPanel!=null){MainMenuPanel.SetActive(false);}            //1-panel
        if(SettingsPanel!=null){SettingsPanel.SetActive(false);}            //2-panel
        if(InstructionsPanel!=null){InstructionsPanel.SetActive(false);}    //3-panel
        if(PausePanel!=null){PausePanel.SetActive(false);}                  //4-panel
        if(FAKEEndPanel!=null){FAKEEndPanel.SetActive(false);}              //5-panel
        if(End1Panel!=null){End1Panel.SetActive(false);}                    //6-panel
        if(End2Panel!=null){End2Panel.SetActive(false);}                    //7-panel


        
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
    //1-panel Main Menue
    public void GamePlay()
    {   
        isPaused=false;
        GameStarted=true;
        MainMenuPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        InstructionsPanel.SetActive(false);
        Time.timeScale=1;
        
    }
    public void MainMenu()
    {
        isPaused=false;
        GameStarted=false;

        MainMenuPanel.SetActive(true);
        SettingsPanel.SetActive(false);
        InstructionsPanel.SetActive(false);
        PausePanel.SetActive(false);
        FAKEEndPanel.SetActive(false);
        End1Panel.SetActive(false);
        End2Panel.SetActive(false);
        
    }
    //2- Settings panel
    public void SettingsUI()
    {
        
        MainMenuPanel.SetActive(false);
        SettingsPanel.SetActive(true);
        InstructionsPanel.SetActive(false);
    }

    //3-Instructions panel
    public void InstructionsUI()
    {

        MainMenuPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        InstructionsPanel.SetActive(true);
    }
    public void OpenSettingeFromMenu()
    {
        settingsFrom=SettingsOpenedFrom.Menu;

        MainMenuPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        InstructionsPanel.SetActive(true);
    }
    public void OpenSettingeFromPause()
    {
        settingsFrom=SettingsOpenedFrom.Pause;

        MainMenuPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        InstructionsPanel.SetActive(true);
    }
    //4-Pause panel
    public void PauseUI()
    {
        isPaused=true;
        GameStarted=false;

        
    }
    //5-Fake end panel
    public void FAKEEndUI()
    {
        
    }
    //6-End 1 panel
    public void End1UI()
    {
        
    }
    //7-End 2 panel
    public void End2UI()
    {
        
    }
}
