using UnityEngine;

public class TrollComment : CommentBase
{
    [Header("Troll Comment Settings")]
    [SerializeField] private int faithReward = 15;
    [SerializeField] private int faithPenalty = 25;
    [SerializeField] private Color trollColor = Color.red;
    [SerializeField] private Color crackedColor = Color.gray;
    
    [Header("Troll Comment Effects")]
    [SerializeField] private ParticleSystem trollParticles;
    [SerializeField] private ParticleSystem crackParticles;
    [SerializeField] private AudioClip trollSuccessSound;
    [SerializeField] private AudioClip trollCrackSound;
    [SerializeField] private AudioClip trollFailSound;
    
    [Header("Double Tap Settings")]
    [SerializeField] private float doubleTapTimeWindow = 0.5f;
    [SerializeField] private float doubleTapDistanceThreshold = 1f;
    
    [Header("Inflammation Settings")]
    [SerializeField] private float inflammationAmount = 25f;
    [SerializeField] private float inflammationDuration = 3f;
    
    private bool isProcessed = false;
    private bool waitingForSecondTap = false;
    private float firstTapTime = 0f;
    private Vector2 firstTapPosition;
    private int tapCount = 0;
    
    protected override void Start()
    {
        base.Start();
        commentType = CommentType.Troll;
        maxHealth = 2;
        CurrentHealth = maxHealth;
        
        if (trollParticles != null)
        {
            trollParticles.gameObject.SetActive(false);
        }
        
        if (crackParticles != null)
        {
            crackParticles.gameObject.SetActive(false);
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (waitingForSecondTap)
        {
            if (Time.time - firstTapTime > doubleTapTimeWindow)
            {
                OnDoubleTapFailed();
            }
        }
    }
    
    private void OnMouseDown()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        HandleTap(mousePosition);
    }
    
    public void HandleInputTap(Vector2 tapPosition)
    {
        if (isProcessed || CurrentState == CommentState.Destroyed) return;
        
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(tapPosition);
        Collider2D collider = GetComponent<Collider2D>();
        
        if (collider != null && collider.bounds.Contains(worldPosition))
        {
            HandleTap(worldPosition);
        }
    }
    
    private void HandleTap(Vector2 tapPosition)
    {
        if (isProcessed || CurrentState == CommentState.Destroyed) return;
        
        if (!waitingForSecondTap)
        {
            HandleFirstTap(tapPosition);
        }
        else
        {
            HandleSecondTap(tapPosition);
        }
    }
    
    private void HandleFirstTap(Vector2 tapPosition)
    {
        tapCount = 1;
        firstTapTime = Time.time;
        firstTapPosition = tapPosition;
        waitingForSecondTap = true;
        
        TakeDamage(1);
        
        if (CurrentHealth > 0)
        {
            Debug.Log("Troll comment cracked! Tap again quickly to destroy it!");
        }
    }
    
    private void HandleSecondTap(Vector2 tapPosition)
    {
        float timeDelta = Time.time - firstTapTime;
        float distance = Vector2.Distance(tapPosition, firstTapPosition);
        
        if (timeDelta <= doubleTapTimeWindow && distance <= doubleTapDistanceThreshold)
        {
            tapCount = 2;
            waitingForSecondTap = false;
            
            TakeDamage(1);
            
            if (CurrentHealth <= 0)
            {
                ProcessComment();
            }
        }
        else
        {
            OnDoubleTapFailed();
        }
    }
    
    private void OnDoubleTapFailed()
    {
        waitingForSecondTap = false;
        tapCount = 0;
        
        if (CurrentHealth <= 0)
        {
            OnMissed();
        }
        else
        {
            CurrentHealth = maxHealth;
            SetState(CommentState.Normal);
        }
    }
    
    protected override void OnProcessed()
    {
        if (isProcessed) return;
        isProcessed = true;
        
        if (FaithSystem.Instance != null)
        {
            FaithSystem.Instance.ProcessTrollCommentSuccess();
        }
        
        PlayTrollSuccessEffects();
        
        if (GameManager.Instance != null)
        {
            Debug.Log($"Troll comment successfully destroyed! Faith gained: {faithReward}");
        }
    }
    
    protected override void OnDamaged()
    {
        PlayTrollCrackEffects();
        
        if (CurrentHealth <= 0)
        {
            ApplyInflammationEffect();
        }
        
        if (GameManager.Instance != null)
        {
            Debug.Log($"Troll comment damaged! Health: {CurrentHealth}");
        }
    }
    
    protected override void OnDestroyed()
    {
        StopTrollEffects();
        
        if (GameManager.Instance != null)
        {
            Debug.Log("Troll comment destroyed");
        }
    }
    
    protected override void OnMissed()
    {
        if (FaithSystem.Instance != null)
        {
            FaithSystem.Instance.ProcessTrollCommentFail();
        }
        
        ApplyInflammationEffect();
        PlayTrollFailEffects();
        
        base.OnMissed();
        
        if (GameManager.Instance != null)
        {
            Debug.Log($"Troll comment missed! Faith lost: {faithPenalty}");
        }
    }
    
    private void PlayTrollSuccessEffects()
    {
        if (trollParticles != null)
        {
            trollParticles.gameObject.SetActive(true);
            trollParticles.Play();
        }
        
        if (trollSuccessSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(trollSuccessSound);
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green;
        }
    }
    
    private void PlayTrollCrackEffects()
    {
        if (crackParticles != null)
        {
            crackParticles.gameObject.SetActive(true);
            crackParticles.Play();
        }
        
        if (trollCrackSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(trollCrackSound);
        }
    }
    
    private void PlayTrollFailEffects()
    {
        if (trollFailSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(trollFailSound);
        }
    }
    
    private void StopTrollEffects()
    {
        if (trollParticles != null)
        {
            trollParticles.Stop();
        }
        
        if (crackParticles != null)
        {
            crackParticles.Stop();
        }
    }
    
    private void ApplyInflammationEffect()
    {
        if (InflamationSystem.Instance != null)
        {
            InflamationSystem.Instance.IncreaseInflamation(inflammationAmount);
        }
    }
    
    protected override void UpdateVisualForNormal()
    {
        base.UpdateVisualForNormal();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(Color.white, trollColor, 0.3f);
        }
    }
    
    protected override void UpdateVisualForMoving()
    {
        base.UpdateVisualForMoving();
        
        if (spriteRenderer != null)
        {
            float pulseIntensity = Mathf.PingPong(Time.time * 3f, 1f);
            spriteRenderer.color = Color.Lerp(Color.white, trollColor, pulseIntensity * 0.5f);
        }
    }
    
    protected override void UpdateVisualForCracked()
    {
        base.UpdateVisualForCracked();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = crackedColor;
        }
    }
    
    public void SetFaithReward(int reward)
    {
        faithReward = reward;
    }
    
    public void SetFaithPenalty(int penalty)
    {
        faithPenalty = penalty;
    }
    
    public void SetDoubleTapTimeWindow(float timeWindow)
    {
        doubleTapTimeWindow = timeWindow;
    }
    
    public void SetDoubleTapDistanceThreshold(float threshold)
    {
        doubleTapDistanceThreshold = threshold;
    }
    
    public void SetInflammationAmount(float amount)
    {
        inflammationAmount = amount;
    }
    
    public void SetInflammationDuration(float duration)
    {
        inflammationDuration = duration;
    }
    
    public int GetTapCount()
    {
        return tapCount;
    }
    
    public bool IsWaitingForSecondTap()
    {
        return waitingForSecondTap;
    }
    
    public float GetRemainingDoubleTapTime()
    {
        if (!waitingForSecondTap) return 0f;
        return Mathf.Max(0f, doubleTapTimeWindow - (Time.time - firstTapTime));
    }
    
    private void OnDisable()
    {
        StopTrollEffects();
        waitingForSecondTap = false;
        tapCount = 0;
    }
}