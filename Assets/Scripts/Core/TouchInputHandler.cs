using UnityEngine;
using System;

public class TouchInputHandler : MonoBehaviour
{
    [Header("Touch Settings")]
    [SerializeField] private float doubleTapTimeWindow = 0.3f;
    [SerializeField] private float doubleTapDistanceThreshold = 50f;
    [SerializeField] private LayerMask commentLayerMask = -1;
    
    [Header("Feedback Settings")]
    [SerializeField] private bool enableHapticFeedback = true;
    [SerializeField] private bool enableVisualFeedback = true;
    
    public event Action<Vector2, CommentBase> OnCommentTapped;
    public event Action<Vector2, CommentBase> OnCommentDoubleTapped;
    public event Action<Vector2> OnEmptyAreaTapped;
    public event Action<Vector2> OnTouchStarted;
    public event Action<Vector2> OnTouchEnded;
    
    private Vector2 lastTapPosition;
    private float lastTapTime;
    private CommentBase lastTappedComment;
    private bool waitingForDoubleTap;
    private Camera mainCamera;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }
    
    private void Start()
    {
        InputManager.Instance.OnSingleTap += HandleSingleTap;
        InputManager.Instance.OnDoubleTap += HandleDoubleTap;
        InputManager.Instance.OnTouchStart += HandleTouchStart;
        InputManager.Instance.OnTouchEnd += HandleTouchEnd;
    }
    
    private void HandleTouchStart(Vector2 screenPosition)
    {
        OnTouchStarted?.Invoke(screenPosition);
    }
    
    private void HandleTouchEnd(Vector2 screenPosition)
    {
        OnTouchEnded?.Invoke(screenPosition);
    }
    
    private void HandleSingleTap(Vector2 screenPosition)
    {
        Vector2 worldPosition = ScreenToWorldPosition(screenPosition);
        CommentBase tappedComment = GetCommentAtPosition(worldPosition);
        
        if (tappedComment != null)
        {
            ProcessSingleTapOnComment(screenPosition, tappedComment);
        }
        else
        {
            OnEmptyAreaTapped?.Invoke(screenPosition);
        }
    }
    
    private void HandleDoubleTap(Vector2 screenPosition)
    {
        Vector2 worldPosition = ScreenToWorldPosition(screenPosition);
        CommentBase tappedComment = GetCommentAtPosition(worldPosition);
        
        if (tappedComment != null)
        {
            ProcessDoubleTapOnComment(screenPosition, tappedComment);
        }
    }
    
    private void ProcessSingleTapOnComment(Vector2 screenPosition, CommentBase comment)
    {
        switch (comment.Type)
        {
            case CommentType.Holy:
                break;
                
            case CommentType.Ohoe:
                ProcessOhoeComment(comment);
                break;
                
            case CommentType.SuperChat:
                ProcessSuperChatComment(comment);
                break;
                
            case CommentType.Troll:
                break;
        }
        
        OnCommentTapped?.Invoke(screenPosition, comment);
        
        if (enableHapticFeedback)
        {
            TriggerHapticFeedback();
        }
        
        if (enableVisualFeedback)
        {
            ShowTapFeedback(screenPosition);
        }
    }
    
    private void ProcessDoubleTapOnComment(Vector2 screenPosition, CommentBase comment)
    {
        switch (comment.Type)
        {
            case CommentType.Troll:
                ProcessTrollComment(comment);
                break;
                
            case CommentType.Holy:
            case CommentType.Ohoe:
            case CommentType.SuperChat:
                break;
        }
        
        OnCommentDoubleTapped?.Invoke(screenPosition, comment);
        
        if (enableHapticFeedback)
        {
            TriggerHapticFeedback();
        }
        
        if (enableVisualFeedback)
        {
            ShowDoubleTapFeedback(screenPosition);
        }
    }
    
    private void ProcessOhoeComment(CommentBase comment)
    {
        comment.ProcessComment();
        FaithSystem.Instance.ProcessOhoeCommentSuccess();
    }
    
    private void ProcessSuperChatComment(CommentBase comment)
    {
        int amount = ExtractSuperChatAmount(comment.Text);
        comment.ProcessComment();
        FaithSystem.Instance.ProcessSuperChat(amount);
    }
    
    private void ProcessTrollComment(CommentBase comment)
    {
        if (comment.CurrentState == CommentState.Normal)
        {
            comment.TakeDamage(1);
        }
        else if (comment.CurrentState == CommentState.Cracked)
        {
            comment.ProcessComment();
            FaithSystem.Instance.ProcessTrollCommentSuccess();
        }
    }
    
    private int ExtractSuperChatAmount(string text)
    {
        if (string.IsNullOrEmpty(text)) return 100;
        
        if (text.Contains("￥5000")) return 5000;
        if (text.Contains("￥1000")) return 1000;
        if (text.Contains("￥500")) return 500;
        if (text.Contains("￥100")) return 100;
        
        return 100;
    }
    
    private Vector2 ScreenToWorldPosition(Vector2 screenPosition)
    {
        if (mainCamera == null) return Vector2.zero;
        
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));
        return new Vector2(worldPos.x, worldPos.y);
    }
    
    private CommentBase GetCommentAtPosition(Vector2 worldPosition)
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition, commentLayerMask);
        
        if (hitCollider != null)
        {
            return hitCollider.GetComponent<CommentBase>();
        }
        
        return null;
    }
    
    private void TriggerHapticFeedback()
    {
        if (Application.isMobilePlatform)
        {
            Handheld.Vibrate();
        }
    }
    
    private void ShowTapFeedback(Vector2 screenPosition)
    {
    }
    
    private void ShowDoubleTapFeedback(Vector2 screenPosition)
    {
    }
    
    public void SetDoubleTapSettings(float timeWindow, float distanceThreshold)
    {
        doubleTapTimeWindow = timeWindow;
        doubleTapDistanceThreshold = distanceThreshold;
    }
    
    public void SetCommentLayerMask(LayerMask layerMask)
    {
        commentLayerMask = layerMask;
    }
    
    public void SetHapticFeedback(bool enabled)
    {
        enableHapticFeedback = enabled;
    }
    
    public void SetVisualFeedback(bool enabled)
    {
        enableVisualFeedback = enabled;
    }
    
    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSingleTap -= HandleSingleTap;
            InputManager.Instance.OnDoubleTap -= HandleDoubleTap;
            InputManager.Instance.OnTouchStart -= HandleTouchStart;
            InputManager.Instance.OnTouchEnd -= HandleTouchEnd;
        }
    }
}