using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestSceneSetup : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createTestUI = true;
    [SerializeField] private bool startGameAutomatically = true;
    
    [Header("Comment Test Settings")]
    [SerializeField] private float testSpawnInterval = 2f;
    [SerializeField] private int maxTestComments = 5;
    [SerializeField] private bool enableDebugMode = true;
    
    private Canvas testCanvas;
    private GameObject testUI;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupTestScene();
        }
    }
    
    [ContextMenu("Setup Test Scene")]
    public void SetupTestScene()
    {
        Debug.Log("Setting up test scene...");
        
        SetupEventSystem();
        SetupMainCamera();
        SetupGameManagers();
        SetupTestCanvas();
        
        if (createTestUI)
        {
            SetupTestUI();
        }
        
        if (startGameAutomatically)
        {
            Invoke(nameof(StartTestGame), 1f);
        }
        
        Debug.Log("Test scene setup completed!");
    }
    
    private void SetupEventSystem()
    {
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            Debug.Log("EventSystem created");
        }
    }
    
    private void SetupMainCamera()
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
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.2f, 1f);
        mainCamera.transform.position = new Vector3(0, 0, -10);
        
        Debug.Log("Main camera configured");
    }
    
    private void SetupGameManagers()
    {
        if (GameManager.Instance == null)
        {
            GameObject gameManagerObject = new GameObject("GameManager");
            gameManagerObject.AddComponent<GameManager>();
            DontDestroyOnLoad(gameManagerObject);
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
        
        Debug.Log("Game managers initialized");
    }
    
    private void SetupTestCanvas()
    {
        testCanvas = FindObjectOfType<Canvas>();
        if (testCanvas == null)
        {
            GameObject canvasObject = new GameObject("TestCanvas");
            testCanvas = canvasObject.AddComponent<Canvas>();
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }
        
        testCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        testCanvas.sortingOrder = 0;
        
        CanvasScaler canvasScaler = testCanvas.GetComponent<CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }
        
        Debug.Log("Test canvas configured");
    }
    
    private void SetupTestUI()
    {
        testUI = new GameObject("TestUI");
        testUI.transform.SetParent(testCanvas.transform, false);
        
        RectTransform testUIRect = testUI.AddComponent<RectTransform>();
        testUIRect.anchorMin = Vector2.zero;
        testUIRect.anchorMax = Vector2.one;
        testUIRect.sizeDelta = Vector2.zero;
        testUIRect.anchoredPosition = Vector2.zero;
        
        CreateTestInfoPanel();
        CreateTestControlPanel();
        
        Debug.Log("Test UI created");
    }
    
    private void CreateTestInfoPanel()
    {
        GameObject infoPanel = new GameObject("InfoPanel");
        infoPanel.transform.SetParent(testUI.transform, false);
        
        RectTransform infoPanelRect = infoPanel.AddComponent<RectTransform>();
        infoPanelRect.anchorMin = new Vector2(0, 0.8f);
        infoPanelRect.anchorMax = new Vector2(1, 1);
        infoPanelRect.sizeDelta = Vector2.zero;
        infoPanelRect.anchoredPosition = Vector2.zero;
        
        Image infoPanelImage = infoPanel.AddComponent<Image>();
        infoPanelImage.color = new Color(0, 0, 0, 0.5f);
        
        GameObject faithText = new GameObject("FaithText");
        faithText.transform.SetParent(infoPanel.transform, false);
        
        RectTransform faithTextRect = faithText.AddComponent<RectTransform>();
        faithTextRect.anchorMin = new Vector2(0, 0.5f);
        faithTextRect.anchorMax = new Vector2(0.3f, 1);
        faithTextRect.sizeDelta = Vector2.zero;
        faithTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI faithTextMesh = faithText.AddComponent<TextMeshProUGUI>();
        faithTextMesh.text = "Faith: 0";
        faithTextMesh.fontSize = 24;
        faithTextMesh.color = Color.white;
        faithTextMesh.alignment = TextAlignmentOptions.Center;
        
        GameObject healthText = new GameObject("HealthText");
        healthText.transform.SetParent(infoPanel.transform, false);
        
        RectTransform healthTextRect = healthText.AddComponent<RectTransform>();
        healthTextRect.anchorMin = new Vector2(0.3f, 0.5f);
        healthTextRect.anchorMax = new Vector2(0.6f, 1);
        healthTextRect.sizeDelta = Vector2.zero;
        healthTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI healthTextMesh = healthText.AddComponent<TextMeshProUGUI>();
        healthTextMesh.text = "Health: 100%";
        healthTextMesh.fontSize = 24;
        healthTextMesh.color = Color.white;
        healthTextMesh.alignment = TextAlignmentOptions.Center;
        
        GameObject timeText = new GameObject("TimeText");
        timeText.transform.SetParent(infoPanel.transform, false);
        
        RectTransform timeTextRect = timeText.AddComponent<RectTransform>();
        timeTextRect.anchorMin = new Vector2(0.6f, 0.5f);
        timeTextRect.anchorMax = new Vector2(1, 1);
        timeTextRect.sizeDelta = Vector2.zero;
        timeTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI timeTextMesh = timeText.AddComponent<TextMeshProUGUI>();
        timeTextMesh.text = "Time: 60";
        timeTextMesh.fontSize = 24;
        timeTextMesh.color = Color.white;
        timeTextMesh.alignment = TextAlignmentOptions.Center;
        
        GameUI gameUI = infoPanel.AddComponent<GameUI>();
        
        Debug.Log("Test info panel created");
    }
    
    private void CreateTestControlPanel()
    {
        GameObject controlPanel = new GameObject("ControlPanel");
        controlPanel.transform.SetParent(testUI.transform, false);
        
        RectTransform controlPanelRect = controlPanel.AddComponent<RectTransform>();
        controlPanelRect.anchorMin = new Vector2(0, 0);
        controlPanelRect.anchorMax = new Vector2(1, 0.2f);
        controlPanelRect.sizeDelta = Vector2.zero;
        controlPanelRect.anchoredPosition = Vector2.zero;
        
        Image controlPanelImage = controlPanel.AddComponent<Image>();
        controlPanelImage.color = new Color(0, 0, 0, 0.3f);
        
        CreateTestButton("Start Game", controlPanel, new Vector2(0, 0.5f), new Vector2(0.2f, 1), () => StartTestGame());
        CreateTestButton("Spawn Comment", controlPanel, new Vector2(0.2f, 0.5f), new Vector2(0.4f, 1), () => SpawnTestComment());
        CreateTestButton("Reset Faith", controlPanel, new Vector2(0.4f, 0.5f), new Vector2(0.6f, 1), () => ResetFaith());
        CreateTestButton("Toggle Debug", controlPanel, new Vector2(0.6f, 0.5f), new Vector2(0.8f, 1), () => ToggleDebug());
        CreateTestButton("Exit", controlPanel, new Vector2(0.8f, 0.5f), new Vector2(1, 1), () => ExitGame());
        
        Debug.Log("Test control panel created");
    }
    
    private void CreateTestButton(string text, GameObject parent, Vector2 anchorMin, Vector2 anchorMax, System.Action onClick)
    {
        GameObject button = new GameObject(text + "Button");
        button.transform.SetParent(parent.transform, false);
        
        RectTransform buttonRect = button.AddComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.sizeDelta = Vector2.zero;
        buttonRect.anchoredPosition = Vector2.zero;
        
        Image buttonImage = button.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        Button buttonComponent = button.AddComponent<Button>();
        buttonComponent.targetGraphic = buttonImage;
        buttonComponent.onClick.AddListener(() => onClick());
        
        GameObject buttonText = new GameObject("Text");
        buttonText.transform.SetParent(button.transform, false);
        
        RectTransform buttonTextRect = buttonText.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonTextMesh = buttonText.AddComponent<TextMeshProUGUI>();
        buttonTextMesh.text = text;
        buttonTextMesh.fontSize = 18;
        buttonTextMesh.color = Color.white;
        buttonTextMesh.alignment = TextAlignmentOptions.Center;
    }
    
    private void StartTestGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
            Debug.Log("Test game started");
        }
        
        SetupCommentSpawner();
    }
    
    private void SetupCommentSpawner()
    {
        CommentSpawner spawner = FindObjectOfType<CommentSpawner>();
        if (spawner == null)
        {
            GameObject spawnerObject = new GameObject("CommentSpawner");
            spawner = spawnerObject.AddComponent<CommentSpawner>();
        }
        
        spawner.SetSpawnInterval(testSpawnInterval);
        spawner.SetMaxActiveComments(maxTestComments);
        
        CommentInputHandler inputHandler = CommentInputHandler.Instance;
        if (inputHandler != null)
        {
            inputHandler.SetDebugMode(enableDebugMode);
        }
        
        Debug.Log("Comment spawner configured for testing");
    }
    
    private void SpawnTestComment()
    {
        CreateTestCommentObject(CommentType.Holy);
        Debug.Log("Test comment spawned");
    }
    
    private void CreateTestCommentObject(CommentType commentType)
    {
        GameObject commentObject = new GameObject($"Test{commentType}Comment");
        commentObject.transform.position = new Vector3(8, Random.Range(-3f, 3f), 0);
        
        SpriteRenderer spriteRenderer = commentObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateTestSprite();
        spriteRenderer.color = GetColorForCommentType(commentType);
        
        BoxCollider2D collider = commentObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(2, 0.5f);
        
        CommentMover mover = commentObject.AddComponent<CommentMover>();
        
        switch (commentType)
        {
            case CommentType.Holy:
                commentObject.AddComponent<HolyComment>();
                break;
            case CommentType.Ohoe:
                commentObject.AddComponent<OhoeComment>();
                break;
            case CommentType.Troll:
                commentObject.AddComponent<TrollComment>();
                break;
            case CommentType.SuperChat:
                commentObject.AddComponent<SuperChatComment>();
                break;
        }
        
        mover.StartMovement();
        
        if (CommentInputHandler.Instance != null)
        {
            CommentBase commentBase = commentObject.GetComponent<CommentBase>();
            if (commentBase != null)
            {
                CommentInputHandler.Instance.RegisterComment(commentBase);
            }
        }
    }
    
    private Sprite CreateTestSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }
    
    private Color GetColorForCommentType(CommentType commentType)
    {
        switch (commentType)
        {
            case CommentType.Holy: return Color.yellow;
            case CommentType.Ohoe: return Color.green;
            case CommentType.Troll: return Color.red;
            case CommentType.SuperChat: return Color.magenta;
            default: return Color.white;
        }
    }
    
    private void ResetFaith()
    {
        if (FaithSystem.Instance != null)
        {
            FaithSystem.Instance.ResetFaith();
            Debug.Log("Faith reset");
        }
    }
    
    private void ToggleDebug()
    {
        if (CommentInputHandler.Instance != null)
        {
            bool currentDebug = CommentInputHandler.Instance.IsDebugMode();
            CommentInputHandler.Instance.SetDebugMode(!currentDebug);
            Debug.Log($"Debug mode: {!currentDebug}");
        }
    }
    
    private void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}