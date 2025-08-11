using UnityEngine;

public class OhoeComment : CommentBase
{
    [Header("Ohoe Comment Settings")]
    [SerializeField] private int faithReward = 12;
    [SerializeField] private int faithPenalty = 15;
    [SerializeField] private Color ohoeColor = Color.green;
    [SerializeField] private Color confusedColor = Color.magenta;
    
    [Header("Ohoe Comment Effects")]
    [SerializeField] private ParticleSystem ohoeParticles;
    [SerializeField] private AudioClip ohoeSuccessSound;
    [SerializeField] private AudioClip ohoeFailSound;
    
    [Header("Confusion Settings")]
    [SerializeField] private float confusionDuration = 2f;
    [SerializeField] private float confusionShakeIntensity = 0.1f;
    
    private bool isProcessed = false;
    private bool isConfused = false;
    private Vector3 originalPosition;
    private float confusionTimer = 0f;
    
    protected override void Start()
    {
        base.Start();
        commentType = CommentType.Ohoe;
        maxHealth = 1;
        CurrentHealth = maxHealth;
        
        if (ohoeParticles != null)
        {
            ohoeParticles.gameObject.SetActive(false);
        }
        
        originalPosition = transform.position;
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (isConfused)
        {
            UpdateConfusionEffect();
        }
    }
    
    private void UpdateConfusionEffect()
    {
        confusionTimer += Time.deltaTime;
        
        if (confusionTimer >= confusionDuration)
        {
            EndConfusion();
            return;
        }
        
        Vector3 shakeOffset = new Vector3(
            Random.Range(-confusionShakeIntensity, confusionShakeIntensity),
            Random.Range(-confusionShakeIntensity, confusionShakeIntensity),
            0
        );
        
        transform.position = originalPosition + shakeOffset;
    }
    
    private void OnMouseDown()
    {
        HandleSingleTap();
    }
    
    private void HandleSingleTap()
    {
        if (isProcessed || CurrentState == CommentState.Destroyed) return;
        
        ProcessComment();
    }
    
    public void HandleInputTap(Vector2 tapPosition)
    {
        if (isProcessed || CurrentState == CommentState.Destroyed) return;
        
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(tapPosition);
        Collider2D collider = GetComponent<Collider2D>();
        
        if (collider != null && collider.bounds.Contains(worldPosition))
        {
            ProcessComment();
        }
    }
    
    protected override void OnProcessed()
    {
        if (isProcessed) return;
        isProcessed = true;
        
        if (FaithSystem.Instance != null)
        {
            FaithSystem.Instance.ProcessOhoeCommentSuccess();
        }
        
        PlayOhoeSuccessEffects();
        ApplyConfusionEffect();
        
        if (GameManager.Instance != null)
        {
            Debug.Log($"Ohoe comment processed successfully! Faith gained: {faithReward}");
        }
    }
    
    protected override void OnDamaged()
    {
        if (FaithSystem.Instance != null)
        {
            FaithSystem.Instance.ProcessOhoeCommentFail();
        }
        
        PlayOhoeFailEffects();
        
        if (GameManager.Instance != null)
        {
            Debug.Log($"Ohoe comment damaged! Faith lost: {faithPenalty}");
        }
    }
    
    protected override void OnDestroyed()
    {
        StopOhoeEffects();
        EndConfusion();
        
        if (GameManager.Instance != null)
        {
            Debug.Log("Ohoe comment destroyed");
        }
    }
    
    protected override void OnMissed()
    {
        if (FaithSystem.Instance != null)
        {
            FaithSystem.Instance.ProcessOhoeCommentFail();
        }
        
        base.OnMissed();
        
        if (GameManager.Instance != null)
        {
            Debug.Log($"Ohoe comment missed! Faith lost: {faithPenalty}");
        }
    }
    
    private void PlayOhoeSuccessEffects()
    {
        if (ohoeParticles != null)
        {
            ohoeParticles.gameObject.SetActive(true);
            ohoeParticles.Play();
        }
        
        if (ohoeSuccessSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(ohoeSuccessSound);
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = ohoeColor;
        }
    }
    
    private void PlayOhoeFailEffects()
    {
        if (ohoeFailSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(ohoeFailSound);
        }
    }
    
    private void StopOhoeEffects()
    {
        if (ohoeParticles != null)
        {
            ohoeParticles.Stop();
        }
    }
    
    private void ApplyConfusionEffect()
    {
        if (isConfused) return;
        
        isConfused = true;
        confusionTimer = 0f;
        originalPosition = transform.position;
        
        if (InflamationSystem.Instance != null)
        {
            InflamationSystem.Instance.ApplyConfusion(confusionDuration);
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = confusedColor;
        }
    }
    
    private void EndConfusion()
    {
        if (!isConfused) return;
        
        isConfused = false;
        confusionTimer = 0f;
        transform.position = originalPosition;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }
    
    protected override void UpdateVisualForNormal()
    {
        base.UpdateVisualForNormal();
        
        if (spriteRenderer != null && !isConfused)
        {
            spriteRenderer.color = Color.Lerp(Color.white, ohoeColor, 0.2f);
        }
    }
    
    protected override void UpdateVisualForMoving()
    {
        base.UpdateVisualForMoving();
        
        if (spriteRenderer != null && !isConfused)
        {
            spriteRenderer.color = Color.Lerp(Color.white, ohoeColor, 0.3f);
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
    
    public void SetConfusionDuration(float duration)
    {
        confusionDuration = duration;
    }
    
    public void SetConfusionShakeIntensity(float intensity)
    {
        confusionShakeIntensity = intensity;
    }
    
    private void OnDisable()
    {
        StopOhoeEffects();
        EndConfusion();
    }
}