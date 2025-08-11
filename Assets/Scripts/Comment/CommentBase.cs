using UnityEngine;
using System;

public abstract class CommentBase : MonoBehaviour
{
    [Header("Comment Properties")]
    [SerializeField] protected string commentText;
    [SerializeField] protected CommentType commentType;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int maxHealth = 1;
    
    [Header("Visual Components")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected TMPro.TextMeshPro textMeshPro;
    
    public CommentState CurrentState { get; protected set; } = CommentState.Normal;
    public CommentType Type { get => commentType; }
    public string Text { get => commentText; set => commentText = value; }
    public int CurrentHealth { get; protected set; }
    
    public event Action<CommentBase> OnCommentProcessed;
    public event Action<CommentBase> OnCommentDestroyed;
    public event Action<CommentBase> OnCommentMissed;
    
    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
        InitializeComment();
    }
    
    protected virtual void Start()
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = commentText;
        }
    }
    
    protected virtual void Update()
    {
        UpdateMovement();
        CheckBounds();
    }
    
    protected virtual void InitializeComment()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (textMeshPro == null)
        {
            textMeshPro = GetComponentInChildren<TMPro.TextMeshPro>();
        }
    }
    
    protected virtual void UpdateMovement()
    {
        if (CurrentState == CommentState.Moving)
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        }
    }
    
    protected virtual void CheckBounds()
    {
        float leftBound = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x - 2f;
        
        if (transform.position.x < leftBound)
        {
            OnMissed();
        }
    }
    
    public virtual void StartMoving()
    {
        SetState(CommentState.Moving);
    }
    
    public virtual void StopMoving()
    {
        SetState(CommentState.Normal);
    }
    
    public virtual void ProcessComment()
    {
        if (CurrentState == CommentState.Destroyed) return;
        
        OnProcessed();
        OnCommentProcessed?.Invoke(this);
    }
    
    public virtual void TakeDamage(int damage = 1)
    {
        if (CurrentState == CommentState.Destroyed) return;
        
        CurrentHealth -= damage;
        
        if (CurrentHealth <= 0)
        {
            DestroyComment();
        }
        else
        {
            SetState(CommentState.Cracked);
            OnDamaged();
        }
    }
    
    protected virtual void DestroyComment()
    {
        SetState(CommentState.Destroyed);
        OnDestroyed();
        OnCommentDestroyed?.Invoke(this);
        
        Destroy(gameObject, 0.1f);
    }
    
    protected virtual void OnMissed()
    {
        OnCommentMissed?.Invoke(this);
        Destroy(gameObject);
    }
    
    protected virtual void SetState(CommentState newState)
    {
        if (CurrentState == newState) return;
        
        CurrentState = newState;
        OnStateChanged(newState);
    }
    
    protected virtual void OnStateChanged(CommentState newState)
    {
        switch (newState)
        {
            case CommentState.Normal:
                UpdateVisualForNormal();
                break;
            case CommentState.Moving:
                UpdateVisualForMoving();
                break;
            case CommentState.Cracked:
                UpdateVisualForCracked();
                break;
            case CommentState.Destroyed:
                UpdateVisualForDestroyed();
                break;
        }
    }
    
    protected virtual void UpdateVisualForNormal()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }
    
    protected virtual void UpdateVisualForMoving()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }
    
    protected virtual void UpdateVisualForCracked()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }
    }
    
    protected virtual void UpdateVisualForDestroyed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
    }
    
    protected abstract void OnProcessed();
    protected abstract void OnDamaged();
    protected abstract void OnDestroyed();
    
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    public void SetText(string text)
    {
        commentText = text;
        if (textMeshPro != null)
        {
            textMeshPro.text = text;
        }
    }
}

public enum CommentState
{
    Normal,
    Moving,
    Cracked,
    Destroyed
}

public enum CommentType
{
    Holy,
    Ohoe,
    Troll,
    SuperChat
}