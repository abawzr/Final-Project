using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int framerate = 120;
    [SerializeField] private Material glitchMaterial;
    [SerializeField] private Material glitchMaterial2;

    private GameState _currentState;

    public event Action<GameState> OnGameStateChanged;

    public static GameManager Instance;

    public enum GameState
    {
        MainMenu,
        Pause,
        Gameplay,
        Puzzle,
        FirstDeath,
        Choice,
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
    }

    private void Start()
    {
        SetGameState(GameState.MainMenu);
    }

    private void ResetGlitchMaterial(Scene scene, LoadSceneMode sceneMode)
    {
        glitchMaterial.SetFloat("_Intensity", 0);
        glitchMaterial.SetFloat("_ChromaticSplit", 0);
        glitchMaterial.SetFloat("_NoiseAmount", 0);

        glitchMaterial2.SetFloat("_Intensity", 0);
    }

    public void SetGameState(GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
        Debug.Log(_currentState);
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Confined;
                PlayerMovement.IsControlsEnabled = false;
                break;

            case GameState.Pause:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.Confined;
                PlayerMovement.IsControlsEnabled = false;
                break;

            case GameState.Gameplay:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                PlayerMovement.IsControlsEnabled = true;
                break;

            case GameState.FirstDeath:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Confined;
                PlayerMovement.IsControlsEnabled = false;
                break;

            case GameState.Choice:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.Confined;
                PlayerMovement.IsControlsEnabled = false;
                break;

            case GameState.Puzzle:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Confined;
                PlayerMovement.IsControlsEnabled = false;
                break;

            case GameState.Cutscene:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.Locked;
                PlayerMovement.IsControlsEnabled = false;
                break;

            case GameState.Credits:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.Locked;
                PlayerMovement.IsControlsEnabled = false;
                break;

            default:
                break;
        }
    }

    public void StartGame()
    {
        SetGameState(GameState.Gameplay);
        SceneManager.LoadSceneAsync("StartScene");
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
