using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    
    [Header("Game Parameters")]
    [SerializeField] private float gameTimeLimit = 60f;
    [SerializeField] private float currentGameTime = 0f;
    
    public event Action<GameState> OnGameStateChanged;
    public event Action<float> OnGameTimeChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetGameState(GameState.MainMenu);
    }
    
    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            UpdateGameTime();
        }
    }
    
    public void SetGameState(GameState newState)
    {
        if (CurrentState == newState) return;
        
        GameState previousState = CurrentState;
        CurrentState = newState;
        
        OnGameStateChanged?.Invoke(newState);
        
        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenuState();
                break;
            case GameState.Playing:
                HandlePlayingState();
                break;
            case GameState.Paused:
                HandlePausedState();
                break;
            case GameState.GameOver:
                HandleGameOverState();
                break;
        }
    }
    
    public void StartGame()
    {
        currentGameTime = 0f;
        SetGameState(GameState.Playing);
    }
    
    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
        }
    }
    
    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
        }
    }
    
    public void EndGame()
    {
        SetGameState(GameState.GameOver);
    }
    
    public void ReturnToMainMenu()
    {
        SetGameState(GameState.MainMenu);
    }
    
    private void UpdateGameTime()
    {
        currentGameTime += Time.deltaTime;
        OnGameTimeChanged?.Invoke(currentGameTime);
        
        if (currentGameTime >= gameTimeLimit)
        {
            EndGame();
        }
    }
    
    private void HandleMainMenuState()
    {
        Time.timeScale = 1f;
    }
    
    private void HandlePlayingState()
    {
        Time.timeScale = 1f;
    }
    
    private void HandlePausedState()
    {
        Time.timeScale = 0f;
    }
    
    private void HandleGameOverState()
    {
        Time.timeScale = 1f;
    }
    
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, gameTimeLimit - currentGameTime);
    }
    
    public float GetGameProgress()
    {
        return Mathf.Clamp01(currentGameTime / gameTimeLimit);
    }
}

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}