using UnityEngine;
using System.Collections.Generic;

public class CommentInputHandler : MonoBehaviour
{
    public static CommentInputHandler Instance { get; private set; }
    
    [Header("Input Settings")]
    [SerializeField] private LayerMask commentLayerMask = -1;
    [SerializeField] private float tapRadius = 0.5f;
    [SerializeField] private bool debugMode = false;
    
    private Camera mainCamera;
    private List<CommentBase> activeComments = new List<CommentBase>();
    
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
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSingleTap += HandleSingleTap;
            InputManager.Instance.OnDoubleTap += HandleDoubleTap;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
    }
    
    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                enabled = true;
                break;
            case GameState.Paused:
            case GameState.GameOver:
            case GameState.MainMenu:
                enabled = false;
                break;
        }
    }
    
    private void HandleSingleTap(Vector2 screenPosition)
    {
        if (mainCamera == null) return;
        
        Vector2 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        CommentBase targetComment = GetCommentAtPosition(worldPosition);
        
        if (targetComment != null)
        {
            ProcessSingleTapOnComment(targetComment, worldPosition);
        }
        
        if (debugMode)
        {
            Debug.Log($"Single tap at screen: {screenPosition}, world: {worldPosition}, comment: {targetComment?.name}");
        }
    }
    
    private void HandleDoubleTap(Vector2 screenPosition)
    {
        if (mainCamera == null) return;
        
        Vector2 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        CommentBase targetComment = GetCommentAtPosition(worldPosition);
        
        if (targetComment != null)
        {
            ProcessDoubleTapOnComment(targetComment, worldPosition);
        }
        
        if (debugMode)
        {
            Debug.Log($"Double tap at screen: {screenPosition}, world: {worldPosition}, comment: {targetComment?.name}");
        }
    }
    
    private CommentBase GetCommentAtPosition(Vector2 worldPosition)
    {
        Collider2D hit = Physics2D.OverlapCircle(worldPosition, tapRadius, commentLayerMask);
        
        if (hit != null)
        {
            CommentBase comment = hit.GetComponent<CommentBase>();
            if (comment != null && comment.CurrentState != CommentState.Destroyed)
            {
                return comment;
            }
        }
        
        CommentBase closestComment = null;
        float closestDistance = float.MaxValue;
        
        foreach (CommentBase comment in activeComments)
        {
            if (comment == null || comment.CurrentState == CommentState.Destroyed) continue;
            
            float distance = Vector2.Distance(worldPosition, comment.transform.position);
            if (distance <= tapRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestComment = comment;
            }
        }
        
        return closestComment;
    }
    
    private void ProcessSingleTapOnComment(CommentBase comment, Vector2 worldPosition)
    {
        switch (comment.Type)
        {
            case CommentType.Holy:
                ProcessHolyComment(comment as HolyComment, worldPosition);
                break;
            case CommentType.Ohoe:
                ProcessOhoeComment(comment as OhoeComment, worldPosition);
                break;
            case CommentType.Troll:
                ProcessTrollComment(comment as TrollComment, worldPosition);
                break;
            case CommentType.SuperChat:
                ProcessSuperChatComment(comment as SuperChatComment, worldPosition);
                break;
        }
    }
    
    private void ProcessDoubleTapOnComment(CommentBase comment, Vector2 worldPosition)
    {
        switch (comment.Type)
        {
            case CommentType.Troll:
                ProcessTrollComment(comment as TrollComment, worldPosition);
                break;
            default:
                ProcessSingleTapOnComment(comment, worldPosition);
                break;
        }
    }
    
    private void ProcessHolyComment(HolyComment holyComment, Vector2 worldPosition)
    {
        if (holyComment == null) return;
        
        holyComment.ProcessComment();
        
        if (debugMode)
        {
            Debug.Log("Holy comment processed via tap");
        }
    }
    
    private void ProcessOhoeComment(OhoeComment ohoeComment, Vector2 worldPosition)
    {
        if (ohoeComment == null) return;
        
        ohoeComment.HandleInputTap(mainCamera.WorldToScreenPoint(worldPosition));
        
        if (debugMode)
        {
            Debug.Log("Ohoe comment processed via single tap");
        }
    }
    
    private void ProcessTrollComment(TrollComment trollComment, Vector2 worldPosition)
    {
        if (trollComment == null) return;
        
        trollComment.HandleInputTap(mainCamera.WorldToScreenPoint(worldPosition));
        
        if (debugMode)
        {
            Debug.Log($"Troll comment processed via tap - Tap count: {trollComment.GetTapCount()}");
        }
    }
    
    private void ProcessSuperChatComment(SuperChatComment superChatComment, Vector2 worldPosition)
    {
        if (superChatComment == null) return;
        
        superChatComment.HandleInputTap(mainCamera.WorldToScreenPoint(worldPosition));
        
        if (debugMode)
        {
            Debug.Log($"SuperChat comment processed via tap - Amount: {superChatComment.GetFormattedAmount()}");
        }
    }
    
    public void RegisterComment(CommentBase comment)
    {
        if (comment != null && !activeComments.Contains(comment))
        {
            activeComments.Add(comment);
            comment.OnCommentDestroyed += UnregisterComment;
        }
    }
    
    public void UnregisterComment(CommentBase comment)
    {
        if (comment != null && activeComments.Contains(comment))
        {
            activeComments.Remove(comment);
            comment.OnCommentDestroyed -= UnregisterComment;
        }
    }
    
    public void ClearAllComments()
    {
        foreach (CommentBase comment in activeComments)
        {
            if (comment != null)
            {
                comment.OnCommentDestroyed -= UnregisterComment;
            }
        }
        
        activeComments.Clear();
    }
    
    public int GetActiveCommentCount()
    {
        return activeComments.Count;
    }
    
    public List<CommentBase> GetActiveComments()
    {
        return new List<CommentBase>(activeComments);
    }
    
    public void SetTapRadius(float radius)
    {
        tapRadius = radius;
    }
    
    public float GetTapRadius()
    {
        return tapRadius;
    }
    
    public void SetDebugMode(bool enabled)
    {
        debugMode = enabled;
    }
    
    public bool IsDebugMode()
    {
        return debugMode;
    }
    
    public void SetCommentLayerMask(LayerMask layerMask)
    {
        commentLayerMask = layerMask;
    }
    
    public LayerMask GetCommentLayerMask()
    {
        return commentLayerMask;
    }
    
    private void OnDrawGizmos()
    {
        if (!debugMode) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tapRadius);
        
        foreach (CommentBase comment in activeComments)
        {
            if (comment != null)
            {
                Gizmos.color = GetGizmoColorForCommentType(comment.Type);
                Gizmos.DrawWireSphere(comment.transform.position, tapRadius);
            }
        }
    }
    
    private Color GetGizmoColorForCommentType(CommentType type)
    {
        switch (type)
        {
            case CommentType.Holy: return Color.yellow;
            case CommentType.Ohoe: return Color.green;
            case CommentType.Troll: return Color.red;
            case CommentType.SuperChat: return Color.magenta;
            default: return Color.white;
        }
    }
    
    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSingleTap -= HandleSingleTap;
            InputManager.Instance.OnDoubleTap -= HandleDoubleTap;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
        
        ClearAllComments();
    }
}