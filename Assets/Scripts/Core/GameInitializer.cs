using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Manager Prefabs")]
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject sceneControllerPrefab;
    [SerializeField] private GameObject inputManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject commentInputHandlerPrefab;
    
    [Header("System Prefabs")]
    [SerializeField] private GameObject faithSystemPrefab;
    [SerializeField] private GameObject healthSystemPrefab;
    [SerializeField] private GameObject inflammationSystemPrefab;
    
    [Header("Game Objects")]
    [SerializeField] private GameObject commentSpawnerPrefab;
    [SerializeField] private GameObject gameUIPrefab;
    
    [Header("Settings")]
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private bool createMissingManagers = true;
    
    private void Start()
    {
        if (initializeOnStart)
        {
            InitializeGame();
        }
    }
    
    [ContextMenu("Initialize Game")]
    public void InitializeGame()
    {
        Debug.Log("Starting game initialization...");
        
        InitializeManagers();
        InitializeSystems();
        InitializeGameObjects();
        
        Debug.Log("Game initialization completed!");
    }
    
    private void InitializeManagers()
    {
        InstantiateIfNotExists<GameManager>(gameManagerPrefab, "GameManager");
        InstantiateIfNotExists<SceneController>(sceneControllerPrefab, "SceneController");
        InstantiateIfNotExists<InputManager>(inputManagerPrefab, "InputManager");
        InstantiateIfNotExists<AudioManager>(audioManagerPrefab, "AudioManager");
        InstantiateIfNotExists<CommentInputHandler>(commentInputHandlerPrefab, "CommentInputHandler");
    }
    
    private void InitializeSystems()
    {
        InstantiateIfNotExists<FaithSystem>(faithSystemPrefab, "FaithSystem");
        InstantiateIfNotExists<HealthSystem>(healthSystemPrefab, "HealthSystem");
        InstantiateIfNotExists<InflamationSystem>(inflammationSystemPrefab, "InflamationSystem");
    }
    
    private void InitializeGameObjects()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            InstantiateIfNotExists<CommentSpawner>(commentSpawnerPrefab, "CommentSpawner", false);
            InstantiateIfNotExists<GameUI>(gameUIPrefab, "GameUI", false);
        }
    }
    
    private void InstantiateIfNotExists<T>(GameObject prefab, string objectName, bool dontDestroyOnLoad = true) where T : MonoBehaviour
    {
        if (FindObjectOfType<T>() != null)
        {
            Debug.Log($"{typeof(T).Name} already exists in scene");
            return;
        }
        
        GameObject instance = null;
        
        if (prefab != null)
        {
            instance = Instantiate(prefab);
        }
        else if (createMissingManagers)
        {
            instance = new GameObject(objectName);
            instance.AddComponent<T>();
        }
        
        if (instance != null)
        {
            instance.name = objectName;
            
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(instance);
            }
            
            Debug.Log($"Created {typeof(T).Name}: {objectName}");
        }
        else
        {
            Debug.LogWarning($"Failed to create {typeof(T).Name}: {objectName}");
        }
    }
    
    public void CreateDefaultCommentDistribution()
    {
        CommentDistribution distribution = ScriptableObject.CreateInstance<CommentDistribution>();
        distribution.name = "DefaultCommentDistribution";
        
        #if UNITY_EDITOR
        string path = "Assets/ScriptableObjects/DefaultCommentDistribution.asset";
        UnityEditor.AssetDatabase.CreateAsset(distribution, path);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"Created default comment distribution at: {path}");
        #endif
    }
    
    public void SetupGameScene()
    {
        SetupCamera();
        SetupCanvas();
        SetupCommentSpawner();
    }
    
    private void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }
        
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5f;
        mainCamera.backgroundColor = Color.black;
        
        Debug.Log("Camera setup completed");
    }
    
    private void SetupCanvas()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        
        var canvasScaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }
        
        Debug.Log("Canvas setup completed");
    }
    
    private void SetupCommentSpawner()
    {
        CommentSpawner spawner = FindObjectOfType<CommentSpawner>();
        if (spawner == null && commentSpawnerPrefab != null)
        {
            GameObject spawnerObject = Instantiate(commentSpawnerPrefab);
            spawnerObject.name = "CommentSpawner";
            spawner = spawnerObject.GetComponent<CommentSpawner>();
        }
        
        if (spawner != null)
        {
            CommentDistribution distribution = Resources.Load<CommentDistribution>("DefaultCommentDistribution");
            if (distribution != null)
            {
                spawner.SetCommentDistribution(distribution);
            }
            
            Debug.Log("Comment spawner setup completed");
        }
    }
    
    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("GameManager not found! Cannot start game.");
        }
    }
    
    public void CreateTestScene()
    {
        SetupGameScene();
        InitializeGame();
        
        Debug.Log("Test scene created successfully!");
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Create Test Scene")]
    public void CreateTestSceneInEditor()
    {
        CreateTestScene();
    }
    
    [ContextMenu("Create Comment Distribution")]
    public void CreateCommentDistributionInEditor()
    {
        CreateDefaultCommentDistribution();
    }
    #endif
}