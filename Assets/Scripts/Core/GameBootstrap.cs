using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [Header("Bootstrap Settings")]
    [SerializeField] private bool autoInitialize = true;
    [SerializeField] private bool createTestEnvironment = false;
    [SerializeField] private bool runInitialTests = false;
    
    [Header("Game Configuration")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private CommentDistribution defaultCommentDistribution;
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugMode = false;
    [SerializeField] private bool showFPSCounter = false;
    [SerializeField] private bool verboseLogging = false;
    
    public static GameBootstrap Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (autoInitialize)
            {
                InitializeGame();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (createTestEnvironment)
        {
            CreateTestEnvironment();
        }
        
        if (runInitialTests)
        {
            RunInitialTests();
        }
    }
    
    [ContextMenu("Initialize Game")]
    public void InitializeGame()
    {
        Debug.Log("=== Game Bootstrap: Initializing Game ===");
        
        SetupApplicationSettings();
        InitializeCoreSystems();
        InitializeGameSystems();
        LoadConfiguration();
        
        Debug.Log("=== Game Bootstrap: Initialization Complete ===");
    }
    
    private void SetupApplicationSettings()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        
        if (enableDebugMode)
        {
            Debug.unityLogger.logEnabled = true;
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
        }
        
        Debug.Log("Application settings configured");
    }
    
    private void InitializeCoreSystems()
    {
        if (GameManager.Instance == null)
        {
            GameObject gameManagerObject = new GameObject("GameManager");
            gameManagerObject.AddComponent<GameManager>();
            DontDestroyOnLoad(gameManagerObject);
        }
        
        if (SceneController.Instance == null)
        {
            GameObject sceneControllerObject = new GameObject("SceneController");
            sceneControllerObject.AddComponent<SceneController>();
            DontDestroyOnLoad(sceneControllerObject);
        }
        
        if (InputManager.Instance == null)
        {
            GameObject inputManagerObject = new GameObject("InputManager");
            inputManagerObject.AddComponent<InputManager>();
            DontDestroyOnLoad(inputManagerObject);
        }
        
        if (AudioManager.Instance == null)
        {
            GameObject audioManagerObject = new GameObject("AudioManager");
            audioManagerObject.AddComponent<AudioManager>();
            DontDestroyOnLoad(audioManagerObject);
        }
        
        if (CommentInputHandler.Instance == null)
        {
            GameObject commentInputHandlerObject = new GameObject("CommentInputHandler");
            commentInputHandlerObject.AddComponent<CommentInputHandler>();
            DontDestroyOnLoad(commentInputHandlerObject);
        }
        
        Debug.Log("Core systems initialized");
    }
    
    private void InitializeGameSystems()
    {
        if (FaithSystem.Instance == null)
        {
            GameObject faithSystemObject = new GameObject("FaithSystem");
            faithSystemObject.AddComponent<FaithSystem>();
            DontDestroyOnLoad(faithSystemObject);
        }
        
        if (HealthSystem.Instance == null)
        {
            GameObject healthSystemObject = new GameObject("HealthSystem");
            healthSystemObject.AddComponent<HealthSystem>();
            DontDestroyOnLoad(healthSystemObject);
        }
        
        if (InflamationSystem.Instance == null)
        {
            GameObject inflammationSystemObject = new GameObject("InflamationSystem");
            inflammationSystemObject.AddComponent<InflamationSystem>();
            DontDestroyOnLoad(inflammationSystemObject);
        }
        
        Debug.Log("Game systems initialized");
    }
    
    private void LoadConfiguration()
    {
        if (gameConfig != null)
        {
            ApplyGameConfiguration(gameConfig);
        }
        else
        {
            Debug.LogWarning("Game configuration not found, using default settings");
        }
        
        if (defaultCommentDistribution != null)
        {
            Debug.Log("Default comment distribution loaded");
        }
        else
        {
            Debug.LogWarning("Default comment distribution not found");
        }
    }
    
    private void ApplyGameConfiguration(GameConfig config)
    {
        if (FaithSystem.Instance != null)
        {
            FaithSystem.Instance.SetTargetFaith(config.targetFaith);
        }
        
        if (GameManager.Instance != null)
        {
            // Apply game manager configuration
        }
        
        Debug.Log("Game configuration applied");
    }
    
    private void CreateTestEnvironment()
    {
        Debug.Log("Creating test environment...");
        
        GameObject testSceneSetup = new GameObject("TestSceneSetup");
        testSceneSetup.AddComponent<TestSceneSetup>();
        
        GameObject gameTestRunner = new GameObject("GameTestRunner");
        gameTestRunner.AddComponent<GameTestRunner>();
        
        Debug.Log("Test environment created");
    }
    
    private void RunInitialTests()
    {
        Debug.Log("Running initial tests...");
        
        GameTestRunner testRunner = FindObjectOfType<GameTestRunner>();
        if (testRunner != null)
        {
            testRunner.RunQuickTest();
        }
        else
        {
            Debug.LogWarning("GameTestRunner not found for initial tests");
        }
    }
    
    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadGameLevel();
        }
    }
    
    public void RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameState.MainMenu);
        }
        
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadMainMenu();
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void EnableDebugMode(bool enabled)
    {
        enableDebugMode = enabled;
        
        if (CommentInputHandler.Instance != null)
        {
            CommentInputHandler.Instance.SetDebugMode(enabled);
        }
        
        Debug.Log($"Debug mode: {enabled}");
    }
    
    public void ShowFPS(bool show)
    {
        showFPSCounter = show;
        
        if (show)
        {
            CreateFPSCounter();
        }
        else
        {
            DestroyFPSCounter();
        }
    }
    
    private void CreateFPSCounter()
    {
        GameObject fpsCounter = GameObject.Find("FPSCounter");
        if (fpsCounter == null)
        {
            fpsCounter = new GameObject("FPSCounter");
            fpsCounter.AddComponent<FPSCounter>();
            DontDestroyOnLoad(fpsCounter);
        }
    }
    
    private void DestroyFPSCounter()
    {
        GameObject fpsCounter = GameObject.Find("FPSCounter");
        if (fpsCounter != null)
        {
            Destroy(fpsCounter);
        }
    }
    
    public bool IsGameInitialized()
    {
        return GameManager.Instance != null &&
               InputManager.Instance != null &&
               FaithSystem.Instance != null &&
               HealthSystem.Instance != null &&
               InflamationSystem.Instance != null;
    }
    
    public void ResetAllSystems()
    {
        Debug.Log("Resetting all systems...");
        
        if (FaithSystem.Instance != null)
        {
            FaithSystem.Instance.ResetFaith();
        }
        
        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.ResetHealth();
        }
        
        if (InflamationSystem.Instance != null)
        {
            InflamationSystem.Instance.ResetInflammation();
        }
        
        CommentSpawner spawner = FindObjectOfType<CommentSpawner>();
        if (spawner != null)
        {
            spawner.ClearAllComments();
        }
        
        Debug.Log("All systems reset");
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (GameManager.Instance != null)
        {
            if (pauseStatus)
            {
                GameManager.Instance.PauseGame();
            }
            else
            {
                GameManager.Instance.ResumeGame();
            }
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (GameManager.Instance != null)
        {
            if (!hasFocus)
            {
                GameManager.Instance.PauseGame();
            }
            else
            {
                GameManager.Instance.ResumeGame();
            }
        }
    }
    
    public GameBootstrapInfo GetBootstrapInfo()
    {
        return new GameBootstrapInfo
        {
            isInitialized = IsGameInitialized(),
            debugMode = enableDebugMode,
            fpsCounter = showFPSCounter,
            verboseLogging = verboseLogging,
            gameConfigLoaded = gameConfig != null,
            commentDistributionLoaded = defaultCommentDistribution != null
        };
    }
}

[System.Serializable]
public class GameBootstrapInfo
{
    public bool isInitialized;
    public bool debugMode;
    public bool fpsCounter;
    public bool verboseLogging;
    public bool gameConfigLoaded;
    public bool commentDistributionLoaded;
}

[System.Serializable]
public class GameConfig
{
    [Header("Faith System")]
    public int targetFaith = 200;
    public int initialFaith = 0;
    
    [Header("Game Settings")]
    public float gameTimeLimit = 60f;
    public int maxActiveComments = 10;
    public float baseSpawnInterval = 2f;
    
    [Header("Difficulty")]
    public float difficultyMultiplier = 1f;
    public bool enableDynamicDifficulty = true;
}