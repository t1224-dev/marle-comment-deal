using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CommentSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float baseSpawnInterval = 2f;
    [SerializeField] private float spawnIntervalVariation = 0.5f;
    [SerializeField] private int maxActiveComments = 10;
    
    [Header("Comment Distribution")]
    [SerializeField] private CommentDistribution commentDistribution;
    
    [Header("Spawn Area")]
    [SerializeField] private float spawnAreaTop = 3f;
    [SerializeField] private float spawnAreaBottom = -3f;
    [SerializeField] private float spawnXOffset = 2f;
    
    [Header("Comment Prefabs")]
    [SerializeField] private GameObject holyCommentPrefab;
    [SerializeField] private GameObject ohoeCommentPrefab;
    [SerializeField] private GameObject trollCommentPrefab;
    [SerializeField] private GameObject superChatCommentPrefab;
    
    [Header("Comment Pool")]
    [SerializeField] private int poolSize = 20;
    
    public bool IsSpawning { get; private set; }
    public int ActiveCommentCount { get; private set; }
    
    public event Action<CommentBase> OnCommentSpawned;
    public event Action<CommentBase> OnCommentDestroyed;
    
    private List<CommentBase> activeComments = new List<CommentBase>();
    private Dictionary<CommentType, Queue<GameObject>> commentPools = new Dictionary<CommentType, Queue<GameObject>>();
    private Camera mainCamera;
    private Coroutine spawnCoroutine;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        InitializeCommentPools();
        
        if (commentDistribution == null)
        {
            commentDistribution = CreateDefaultDistribution();
        }
    }
    
    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }
    
    private void InitializeCommentPools()
    {
        commentPools[CommentType.Holy] = new Queue<GameObject>();
        commentPools[CommentType.Ohoe] = new Queue<GameObject>();
        commentPools[CommentType.Troll] = new Queue<GameObject>();
        commentPools[CommentType.SuperChat] = new Queue<GameObject>();
        
        CreatePooledObjects(CommentType.Holy, holyCommentPrefab);
        CreatePooledObjects(CommentType.Ohoe, ohoeCommentPrefab);
        CreatePooledObjects(CommentType.Troll, trollCommentPrefab);
        CreatePooledObjects(CommentType.SuperChat, superChatCommentPrefab);
    }
    
    private void CreatePooledObjects(CommentType type, GameObject prefab)
    {
        if (prefab == null) return;
        
        for (int i = 0; i < poolSize / 4; i++)
        {
            GameObject pooledObject = Instantiate(prefab, transform);
            pooledObject.SetActive(false);
            commentPools[type].Enqueue(pooledObject);
        }
    }
    
    private CommentDistribution CreateDefaultDistribution()
    {
        CommentDistribution distribution = ScriptableObject.CreateInstance<CommentDistribution>();
        distribution.holyCommentChance = 40f;
        distribution.ohoeCommentChance = 30f;
        distribution.trollCommentChance = 20f;
        distribution.superChatCommentChance = 10f;
        return distribution;
    }
    
    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                StartSpawning();
                break;
            case GameState.Paused:
            case GameState.GameOver:
            case GameState.MainMenu:
                StopSpawning();
                break;
        }
    }
    
    public void StartSpawning()
    {
        if (IsSpawning) return;
        
        IsSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnCoroutine());
    }
    
    public void StopSpawning()
    {
        if (!IsSpawning) return;
        
        IsSpawning = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    private IEnumerator SpawnCoroutine()
    {
        while (IsSpawning)
        {
            if (ActiveCommentCount < maxActiveComments)
            {
                SpawnComment();
            }
            
            float waitTime = GetRandomSpawnInterval();
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    private float GetRandomSpawnInterval()
    {
        float randomVariation = UnityEngine.Random.Range(-spawnIntervalVariation, spawnIntervalVariation);
        return Mathf.Max(0.1f, baseSpawnInterval + randomVariation);
    }
    
    private void SpawnComment()
    {
        CommentType commentType = DetermineCommentType();
        GameObject commentObject = GetPooledComment(commentType);
        
        if (commentObject == null) return;
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        commentObject.transform.position = spawnPosition;
        commentObject.SetActive(true);
        
        CommentBase commentBase = commentObject.GetComponent<CommentBase>();
        if (commentBase != null)
        {
            SetupComment(commentBase, commentType);
            RegisterComment(commentBase);
        }
        
        CommentMover commentMover = commentObject.GetComponent<CommentMover>();
        if (commentMover != null)
        {
            commentMover.StartMovement();
            commentMover.OnReachedLeftBound += OnCommentReachedLeftBound;
        }
        
        OnCommentSpawned?.Invoke(commentBase);
    }
    
    private CommentType DetermineCommentType()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        float cumulativeChance = 0f;
        
        cumulativeChance += commentDistribution.holyCommentChance;
        if (randomValue <= cumulativeChance)
            return CommentType.Holy;
        
        cumulativeChance += commentDistribution.ohoeCommentChance;
        if (randomValue <= cumulativeChance)
            return CommentType.Ohoe;
        
        cumulativeChance += commentDistribution.trollCommentChance;
        if (randomValue <= cumulativeChance)
            return CommentType.Troll;
        
        return CommentType.SuperChat;
    }
    
    private GameObject GetPooledComment(CommentType type)
    {
        if (commentPools[type].Count > 0)
        {
            return commentPools[type].Dequeue();
        }
        
        GameObject prefab = GetPrefabForType(type);
        if (prefab != null)
        {
            return Instantiate(prefab, transform);
        }
        
        return null;
    }
    
    private GameObject GetPrefabForType(CommentType type)
    {
        switch (type)
        {
            case CommentType.Holy: return holyCommentPrefab;
            case CommentType.Ohoe: return ohoeCommentPrefab;
            case CommentType.Troll: return trollCommentPrefab;
            case CommentType.SuperChat: return superChatCommentPrefab;
            default: return null;
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        if (mainCamera == null) return Vector3.zero;
        
        Vector3 rightBound = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, 0));
        float spawnX = rightBound.x + spawnXOffset;
        float spawnY = UnityEngine.Random.Range(spawnAreaBottom, spawnAreaTop);
        
        return new Vector3(spawnX, spawnY, 0);
    }
    
    private void SetupComment(CommentBase commentBase, CommentType type)
    {
        commentBase.OnCommentDestroyed += OnCommentDestroyed;
        commentBase.OnCommentMissed += OnCommentMissed;
        
        string commentText = GetRandomCommentText(type);
        commentBase.SetText(commentText);
        
        if (CommentInputHandler.Instance != null)
        {
            CommentInputHandler.Instance.RegisterComment(commentBase);
        }
    }
    
    private string GetRandomCommentText(CommentType type)
    {
        switch (type)
        {
            case CommentType.Holy:
                return GetRandomFromArray(commentDistribution.holyCommentTexts);
            case CommentType.Ohoe:
                return GetRandomFromArray(commentDistribution.ohoeCommentTexts);
            case CommentType.Troll:
                return GetRandomFromArray(commentDistribution.trollCommentTexts);
            case CommentType.SuperChat:
                return GetRandomFromArray(commentDistribution.superChatTexts);
            default:
                return "デフォルトコメント";
        }
    }
    
    private string GetRandomFromArray(string[] array)
    {
        if (array == null || array.Length == 0) return "コメント";
        return array[UnityEngine.Random.Range(0, array.Length)];
    }
    
    private void RegisterComment(CommentBase comment)
    {
        activeComments.Add(comment);
        ActiveCommentCount = activeComments.Count;
    }
    
    private void OnCommentReachedLeftBound(CommentMover commentMover)
    {
        CommentBase commentBase = commentMover.GetComponent<CommentBase>();
        if (commentBase != null)
        {
            commentBase.ProcessComment();
        }
    }
    
    private void OnCommentDestroyed(CommentBase comment)
    {
        UnregisterComment(comment);
        ReturnToPool(comment.gameObject);
        OnCommentDestroyed?.Invoke(comment);
    }
    
    private void OnCommentMissed(CommentBase comment)
    {
        UnregisterComment(comment);
        ReturnToPool(comment.gameObject);
    }
    
    private void UnregisterComment(CommentBase comment)
    {
        activeComments.Remove(comment);
        ActiveCommentCount = activeComments.Count;
        
        comment.OnCommentDestroyed -= OnCommentDestroyed;
        comment.OnCommentMissed -= OnCommentMissed;
        
        if (CommentInputHandler.Instance != null)
        {
            CommentInputHandler.Instance.UnregisterComment(comment);
        }
    }
    
    private void ReturnToPool(GameObject commentObject)
    {
        CommentBase commentBase = commentObject.GetComponent<CommentBase>();
        if (commentBase != null)
        {
            commentPools[commentBase.Type].Enqueue(commentObject);
        }
        
        commentObject.SetActive(false);
    }
    
    public void SetSpawnInterval(float interval)
    {
        baseSpawnInterval = interval;
    }
    
    public void SetMaxActiveComments(int max)
    {
        maxActiveComments = max;
    }
    
    public void SetCommentDistribution(CommentDistribution distribution)
    {
        commentDistribution = distribution;
    }
    
    public void ClearAllComments()
    {
        for (int i = activeComments.Count - 1; i >= 0; i--)
        {
            if (activeComments[i] != null)
            {
                Destroy(activeComments[i].gameObject);
            }
        }
        
        activeComments.Clear();
        ActiveCommentCount = 0;
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}