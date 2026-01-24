using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int framerate = 120;
    [SerializeField] private Material glitchMaterial;

    public static GameManager Instance;

    public enum GameState
    {
        MainMenu,
        Pause,
        Gameplay,
        Puzzle,
        FirstDeath,
        Cutscene,
        Credits
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += ResetGlitchMaterial;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= ResetGlitchMaterial;
    }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Detach from parent if it exists
        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = framerate;

        // NOTE: Remove this comment after creating main menu scene
        // SetGameState(GameState.MainMenu);
    }

    private void ResetGlitchMaterial(Scene scene, LoadSceneMode sceneMode)
    {
        glitchMaterial.SetFloat("_Intensity", 0);
        glitchMaterial.SetFloat("_ChromaticSplit", 0);
        glitchMaterial.SetFloat("_NoiseAmount", 0);
    }

    public void SetGameState(GameState newState)
    {
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.Confined;
                break;

            case GameState.Pause:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.Confined;
                break;

            case GameState.Gameplay:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case GameState.FirstDeath:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Confined;
                break;

            case GameState.Puzzle:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Confined;
                break;

            case GameState.Cutscene:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case GameState.Credits:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.Locked;
                break;

            default:
                break;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync("GameScene");
        SetGameState(GameState.Gameplay);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // Stops play mode
#else
        Application.Quit(); // Quits build
#endif
    }
}
