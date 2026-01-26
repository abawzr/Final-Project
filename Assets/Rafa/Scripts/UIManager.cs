using System;
using TMPro;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Panels")]
    public GameObject SettingsPanel;    //1-panel
    public GameObject PausePanel;       //2-panel


    [Header("Scens Loading")]
    public bool loadGamePlaySceneOnPlay = false;

    bool isPaused;

    public static UIManager Instance { get; private set; }

    void Start()
    {
        isPaused = false;
        CloseAllGameplayPanels();

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
        isPaused = true;
        CloseAllGameplayPanels();
        if (PausePanel) PausePanel.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.Pause);

    }
    //Continue Game after Pause
    public void ContinueGame()
    {
        isPaused = false;
        CloseAllGameplayPanels();

        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.Gameplay);
        if (PausePanel) PausePanel.SetActive(false);
    }
    //2- Settings Panel
    //Open Settinge From Pause NOT menu
    public void OpenSettingeFromPause()
    {
        isPaused = true;

        if (PausePanel) PausePanel.SetActive(false);
        if (SettingsPanel) SettingsPanel.SetActive(true);


    }
    public void BackFromSettings()
    {
        if (SettingsPanel) SettingsPanel.SetActive(false);
        if (PausePanel) PausePanel.SetActive(true);
        isPaused = true;
    }

    ///////////////////////////////////////
    public void BackToMenu()
    {
        if (GameManager.Instance != null)
        {
            //GameManager.Instance.SetGameState(GameManager.GameState.Quit);



        }
    }

    //////////////HIDE ALL////////////////
    void CloseAllGameplayPanels()
    {
        //Hide all panels before starting
        if (SettingsPanel != null) { SettingsPanel.SetActive(false); }            //1-panel
        if (PausePanel != null) { PausePanel.SetActive(false); }                  //2-panel

    }

}

