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
    public GameObject SettingsPanel;    //1-panel
    public GameObject PausePanel;       //2-panel
    public GameObject FAKEEndPanel;     //3-panel
    public GameObject End1Panel;        //4-panel
    public GameObject End2Panel;        //5-panel


    [Header("Scens Loading")]
    public bool loadGamePlaySceneOnPlay=false;
    public string menuSceneName = "MainMenu Scene";

    bool isPaused;
    
    void Start()
    {
        isPaused=false;
        CloseAllGameplayPanels();
        if(GameManager.Instance!=null)
            GameManager.Instance.SetGameState(GameManager.GameState.Gameplay);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseToggel();
        }
        
    }
    ///////////////////Panels//////////////////
    /// 1-Pause panel
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
        CloseAllGameplayPanels();
        if(PausePanel)PausePanel.SetActive(true);

        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.Pause);

    }
    //Continue Game after Pause
     public void ContinueGame()
    {
        isPaused=false;
        CloseAllGameplayPanels();

        
        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.Gameplay);          if(PausePanel)PausePanel.SetActive(false);
    }
    //2- Settings Panel
    //Open Settinge From Pause NOT menu
    public void OpenSettingeFromPause()
    {
        isPaused=true;
        
        if(PausePanel)PausePanel.SetActive(false);
        if(SettingsPanel)SettingsPanel.SetActive(true);

        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.Pause); 

    }
    public void BackFromSettings()
    {
        if(SettingsPanel)SettingsPanel.SetActive(false);
        if(PausePanel)PausePanel.SetActive(true);
        isPaused=true;
        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.Pause); 

    }
    //3-Fake end panel
    public void EndUI()
    {
        isPaused=true;
        CloseAllGameplayPanels();
        if(FAKEEndPanel)FAKEEndPanel.SetActive(true);

        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.Pause);

    }
    //4-End 1 panel
    public void End1UI()
    {
        isPaused=true;
        CloseAllGameplayPanels();
        if(End1Panel)End1Panel.SetActive(true);

        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.Pause);
    }
    //5-End 2 panel
    public void End2UI()
    {
        isPaused=true;
        CloseAllGameplayPanels();
        if(End2Panel)End2Panel.SetActive(true);

        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.Pause);
    }
   
    ///////////////////////////////////////
    public void Restart()
    {
        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.Gameplay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void BackToMenu() {
        SceneManager.LoadScene(menuSceneName);

        if(GameManager.Instance)
            GameManager.Instance.SetGameState(GameManager.GameState.MainMenu);
    }
    
    //////////////HIDE ALL////////////////
    void CloseAllGameplayPanels()
    {
        //Hide all panels before starting
        if(SettingsPanel!=null){SettingsPanel.SetActive(false);}            //1-panel
        if(PausePanel!=null){PausePanel.SetActive(false);}                  //2-panel
        if(FAKEEndPanel!=null){FAKEEndPanel.SetActive(false);}              //3-panel
        if(End1Panel!=null){End1Panel.SetActive(false);}                    //4-panel
        if(End2Panel!=null){End2Panel.SetActive(false);}                    //5-panel
        
    }

}

