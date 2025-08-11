using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameTestRunner : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private float testDuration = 30f;
    [SerializeField] private bool verboseLogging = true;
    
    [Header("Test Settings")]
    [SerializeField] private int testCommentsToSpawn = 10;
    [SerializeField] private float testSpawnInterval = 2f;
    [SerializeField] private bool testAllCommentTypes = true;
    
    [Header("Expected Results")]
    [SerializeField] private int expectedFaithGain = 100;
    [SerializeField] private int maxAcceptableFaithLoss = 50;
    
    private List<string> testResults = new List<string>();
    private int commentsSpawned = 0;
    private int commentsProcessed = 0;
    private int faithAtStart = 0;
    private bool testRunning = false;
    
    private void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunGameTests());
        }
    }
    
    [ContextMenu("Run Game Tests")]
    public void RunTests()
    {
        if (!testRunning)
        {
            StartCoroutine(RunGameTests());
        }
    }
    
    private IEnumerator RunGameTests()
    {
        testRunning = true;
        testResults.Clear();
        
        LogTest("=== Starting Game Tests ===");
        
        yield return StartCoroutine(TestSystemInitialization());
        yield return StartCoroutine(TestGameFlow());
        yield return StartCoroutine(TestCommentSpawning());
        yield return StartCoroutine(TestCommentProcessing());
        yield return StartCoroutine(TestFaithSystem());
        yield return StartCoroutine(TestInputHandling());
        
        LogTest("=== Game Tests Completed ===");
        PrintTestResults();
        
        testRunning = false;
    }
    
    private IEnumerator TestSystemInitialization()
    {
        LogTest("Testing System Initialization...");
        
        bool gameManagerExists = GameManager.Instance != null;
        bool inputManagerExists = InputManager.Instance != null;
        bool faithSystemExists = FaithSystem.Instance != null;
        bool healthSystemExists = HealthSystem.Instance != null;
        bool inflammationSystemExists = InflamationSystem.Instance != null;
        bool commentInputHandlerExists = CommentInputHandler.Instance != null;
        
        testResults.Add($"GameManager: {(gameManagerExists ? "✓" : "✗")}");
        testResults.Add($"InputManager: {(inputManagerExists ? "✓" : "✗")}");
        testResults.Add($"FaithSystem: {(faithSystemExists ? "✓" : "✗")}");
        testResults.Add($"HealthSystem: {(healthSystemExists ? "✓" : "✗")}");
        testResults.Add($"InflamationSystem: {(inflammationSystemExists ? "✓" : "✗")}");
        testResults.Add($"CommentInputHandler: {(commentInputHandlerExists ? "✓" : "✗")}");
        
        yield return new WaitForSeconds(0.5f);
        
        LogTest("System Initialization Test Complete");
    }
    
    private IEnumerator TestGameFlow()
    {
        LogTest("Testing Game Flow...");
        
        if (GameManager.Instance != null)
        {
            GameState initialState = GameManager.Instance.CurrentState;
            
            GameManager.Instance.StartGame();
            yield return new WaitForSeconds(0.1f);
            
            bool gameStarted = GameManager.Instance.CurrentState == GameState.Playing;
            testResults.Add($"Game Start: {(gameStarted ? "✓" : "✗")}");
            
            GameManager.Instance.PauseGame();
            yield return new WaitForSeconds(0.1f);
            
            bool gamePaused = GameManager.Instance.CurrentState == GameState.Paused;
            testResults.Add($"Game Pause: {(gamePaused ? "✓" : "✗")}");
            
            GameManager.Instance.ResumeGame();
            yield return new WaitForSeconds(0.1f);
            
            bool gameResumed = GameManager.Instance.CurrentState == GameState.Playing;
            testResults.Add($"Game Resume: {(gameResumed ? "✓" : "✗")}");
        }
        
        yield return new WaitForSeconds(0.5f);
        
        LogTest("Game Flow Test Complete");
    }
    
    private IEnumerator TestCommentSpawning()
    {
        LogTest("Testing Comment Spawning...");
        
        CommentSpawner spawner = FindObjectOfType<CommentSpawner>();
        if (spawner == null)
        {
            GameObject spawnerObject = new GameObject("TestCommentSpawner");
            spawner = spawnerObject.AddComponent<CommentSpawner>();
        }
        
        int initialCommentCount = spawner.ActiveCommentCount;
        
        for (int i = 0; i < 3; i++)
        {
            SpawnTestComment(CommentType.Holy);
            yield return new WaitForSeconds(0.5f);
        }
        
        int finalCommentCount = spawner.ActiveCommentCount;
        bool commentsSpawned = finalCommentCount > initialCommentCount;
        
        testResults.Add($"Comment Spawning: {(commentsSpawned ? "✓" : "✗")}");
        
        LogTest("Comment Spawning Test Complete");
    }
    
    private IEnumerator TestCommentProcessing()
    {
        LogTest("Testing Comment Processing...");
        
        if (FaithSystem.Instance != null)
        {
            faithAtStart = FaithSystem.Instance.CurrentFaith;
            
            SpawnTestComment(CommentType.Holy);
            yield return new WaitForSeconds(1f);
            
            int faithAfterHoly = FaithSystem.Instance.CurrentFaith;
            bool holyProcessed = faithAfterHoly > faithAtStart;
            testResults.Add($"Holy Comment Processing: {(holyProcessed ? "✓" : "✗")}");
            
            SpawnTestComment(CommentType.SuperChat);
            yield return new WaitForSeconds(1f);
            
            int faithAfterSuperChat = FaithSystem.Instance.CurrentFaith;
            bool superChatProcessed = faithAfterSuperChat > faithAfterHoly;
            testResults.Add($"SuperChat Processing: {(superChatProcessed ? "✓" : "✗")}");
        }
        
        LogTest("Comment Processing Test Complete");
    }
    
    private IEnumerator TestFaithSystem()
    {
        LogTest("Testing Faith System...");
        
        if (FaithSystem.Instance != null)
        {
            int initialFaith = FaithSystem.Instance.CurrentFaith;
            
            FaithSystem.Instance.ProcessHolyComment();
            yield return new WaitForSeconds(0.1f);
            
            int faithAfterHoly = FaithSystem.Instance.CurrentFaith;
            bool faithIncreased = faithAfterHoly > initialFaith;
            testResults.Add($"Faith Increase: {(faithIncreased ? "✓" : "✗")}");
            
            FaithSystem.Instance.ProcessOhoeCommentFail();
            yield return new WaitForSeconds(0.1f);
            
            int faithAfterFail = FaithSystem.Instance.CurrentFaith;
            bool faithDecreased = faithAfterFail < faithAfterHoly;
            testResults.Add($"Faith Decrease: {(faithDecreased ? "✓" : "✗")}");
            
            FaithSystem.Instance.ProcessSuperChat(1000);
            yield return new WaitForSeconds(0.1f);
            
            int faithAfterSuperChat = FaithSystem.Instance.CurrentFaith;
            bool superChatWorked = faithAfterSuperChat > faithAfterFail;
            testResults.Add($"SuperChat Faith: {(superChatWorked ? "✓" : "✗")}");
        }
        
        LogTest("Faith System Test Complete");
    }
    
    private IEnumerator TestInputHandling()
    {
        LogTest("Testing Input Handling...");
        
        if (CommentInputHandler.Instance != null)
        {
            CommentInputHandler inputHandler = CommentInputHandler.Instance;
            
            inputHandler.SetDebugMode(true);
            bool debugModeSet = inputHandler.IsDebugMode();
            testResults.Add($"Debug Mode Toggle: {(debugModeSet ? "✓" : "✗")}");
            
            inputHandler.SetTapRadius(1f);
            bool tapRadiusSet = Mathf.Approximately(inputHandler.GetTapRadius(), 1f);
            testResults.Add($"Tap Radius Setting: {(tapRadiusSet ? "✓" : "✗")}");
            
            int initialActiveComments = inputHandler.GetActiveCommentCount();
            
            SpawnTestComment(CommentType.Troll);
            yield return new WaitForSeconds(0.5f);
            
            int finalActiveComments = inputHandler.GetActiveCommentCount();
            bool commentRegistered = finalActiveComments > initialActiveComments;
            testResults.Add($"Comment Registration: {(commentRegistered ? "✓" : "✗")}");
        }
        
        LogTest("Input Handling Test Complete");
    }
    
    private void SpawnTestComment(CommentType commentType)
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
        
        commentsSpawned++;
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
    
    private void LogTest(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[GameTest] {message}");
        }
    }
    
    private void PrintTestResults()
    {
        LogTest("=== Test Results ===");
        
        int passedTests = 0;
        int totalTests = testResults.Count;
        
        foreach (string result in testResults)
        {
            LogTest(result);
            if (result.Contains("✓"))
            {
                passedTests++;
            }
        }
        
        LogTest($"Tests Passed: {passedTests}/{totalTests}");
        LogTest($"Comments Spawned: {commentsSpawned}");
        LogTest($"Test Success Rate: {(float)passedTests / totalTests * 100:F1}%");
        
        if (passedTests == totalTests)
        {
            LogTest("🎉 All tests passed! Game systems are working correctly.");
        }
        else
        {
            LogTest($"⚠️ {totalTests - passedTests} tests failed. Check the logs for details.");
        }
    }
    
    public void RunQuickTest()
    {
        LogTest("Running Quick Test...");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        
        SpawnTestComment(CommentType.Holy);
        SpawnTestComment(CommentType.Ohoe);
        SpawnTestComment(CommentType.Troll);
        SpawnTestComment(CommentType.SuperChat);
        
        LogTest("Quick test complete - 4 test comments spawned");
    }
    
    public List<string> GetTestResults()
    {
        return new List<string>(testResults);
    }
    
    public bool IsTestRunning()
    {
        return testRunning;
    }
}