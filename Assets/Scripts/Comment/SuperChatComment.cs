using UnityEngine;
using System.Text.RegularExpressions;

public class SuperChatComment : CommentBase
{
    [Header("SuperChat Settings")]
    [SerializeField] private int superChatAmount = 100;
    [SerializeField] private Color superChatColor = Color.yellow;
    [SerializeField] private Color goldColor = Color.gold;
    [SerializeField] private float autoProcessDelay = 0.3f;
    
    [Header("SuperChat Effects")]
    [SerializeField] private ParticleSystem superChatParticles;
    [SerializeField] private ParticleSystem coinParticles;
    [SerializeField] private AudioClip superChatSound;
    [SerializeField] private AudioClip coinSound;
    
    [Header("Amount-based Settings")]
    [SerializeField] private Color[] amountColors = 
    {
        Color.yellow,   // 100
        Color.cyan,     // 500
        Color.magenta,  // 1000
        Color.red       // 5000
    };
    
    [Header("Visual Effects")]
    [SerializeField] private float glowIntensity = 1f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private Vector3 scaleBoost = new Vector3(1.2f, 1.2f, 1f);
    
    private bool isAutoProcessing = false;
    private bool isProcessed = false;
    private Vector3 originalScale;
    
    protected override void Start()
    {
        base.Start();
        commentType = CommentType.SuperChat;
        
        originalScale = transform.localScale;
        
        ExtractAmountFromText();
        SetupVisualsByAmount();
        
        if (superChatParticles != null)
        {
            superChatParticles.gameObject.SetActive(false);
        }
        
        if (coinParticles != null)
        {
            coinParticles.gameObject.SetActive(false);
        }
        
        StartAutoProcessing();
    }
    
    private void ExtractAmountFromText()
    {
        if (string.IsNullOrEmpty(commentText)) return;
        
        Match match = Regex.Match(commentText, @"￥(\d+)");
        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out int amount))
            {
                superChatAmount = amount;
            }
        }
    }
    
    private void SetupVisualsByAmount()
    {
        Color targetColor = GetColorByAmount(superChatAmount);
        superChatColor = targetColor;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(Color.white, targetColor, 0.5f);
        }
        
        float scaleMultiplier = GetScaleMultiplierByAmount(superChatAmount);
        transform.localScale = originalScale * scaleMultiplier;
    }
    
    private Color GetColorByAmount(int amount)
    {
        if (amount >= 5000) return amountColors[3];
        if (amount >= 1000) return amountColors[2];
        if (amount >= 500) return amountColors[1];
        return amountColors[0];
    }
    
    private float GetScaleMultiplierByAmount(int amount)
    {
        if (amount >= 5000) return 1.5f;
        if (amount >= 1000) return 1.3f;
        if (amount >= 500) return 1.2f;
        return 1.1f;
    }
    
    private void StartAutoProcessing()
    {
        if (isAutoProcessing) return;
        
        isAutoProcessing = true;
        Invoke(nameof(AutoProcess), autoProcessDelay);
    }
    
    private void AutoProcess()
    {
        if (CurrentState == CommentState.Destroyed) return;
        
        ProcessComment();
    }
    
    private void OnMouseDown()
    {
        if (!isProcessed && CurrentState != CommentState.Destroyed)
        {
            ProcessComment();
        }
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
            FaithSystem.Instance.ProcessSuperChat(superChatAmount);
        }
        
        PlaySuperChatEffects();
        
        if (GameManager.Instance != null)
        {
            int faithGained = CalculateFaithGain(superChatAmount);
            Debug.Log($"SuperChat processed! Amount: ￥{superChatAmount}, Faith gained: {faithGained}");
        }
    }
    
    protected override void OnDamaged()
    {
        Debug.Log("SuperChat cannot be damaged - it's protected by money!");
    }
    
    protected override void OnDestroyed()
    {
        StopSuperChatEffects();
        
        if (GameManager.Instance != null)
        {
            Debug.Log($"SuperChat destroyed - Amount: ￥{superChatAmount}");
        }
    }
    
    private void PlaySuperChatEffects()
    {
        if (superChatParticles != null)
        {
            superChatParticles.gameObject.SetActive(true);
            superChatParticles.Play();
        }
        
        if (coinParticles != null)
        {
            coinParticles.gameObject.SetActive(true);
            coinParticles.Play();
        }
        
        if (superChatSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(superChatSound);
        }
        
        if (coinSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(coinSound);
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = goldColor;
        }
        
        transform.localScale = originalScale * scaleBoost;
    }
    
    private void StopSuperChatEffects()
    {
        if (superChatParticles != null)
        {
            superChatParticles.Stop();
        }
        
        if (coinParticles != null)
        {
            coinParticles.Stop();
        }
    }
    
    private int CalculateFaithGain(int amount)
    {
        if (amount >= 5000) return 120;
        if (amount >= 1000) return 60;
        if (amount >= 500) return 35;
        return 20;
    }
    
    protected override void UpdateVisualForNormal()
    {
        base.UpdateVisualForNormal();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(Color.white, superChatColor, 0.4f);
        }
    }
    
    protected override void UpdateVisualForMoving()
    {
        base.UpdateVisualForMoving();
        
        if (spriteRenderer != null)
        {
            float glowEffect = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            Color glowColor = Color.Lerp(superChatColor, goldColor, glowEffect);
            spriteRenderer.color = Color.Lerp(Color.white, glowColor, glowIntensity);
        }
    }
    
    protected override void UpdateVisualForDestroyed()
    {
        base.UpdateVisualForDestroyed();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = goldColor;
        }
    }
    
    public void SetSuperChatAmount(int amount)
    {
        superChatAmount = amount;
        SetupVisualsByAmount();
    }
    
    public int GetSuperChatAmount()
    {
        return superChatAmount;
    }
    
    public void SetAutoProcessDelay(float delay)
    {
        autoProcessDelay = delay;
    }
    
    public void SetGlowIntensity(float intensity)
    {
        glowIntensity = intensity;
    }
    
    public void SetPulseSpeed(float speed)
    {
        pulseSpeed = speed;
    }
    
    public void SetScaleBoost(Vector3 boost)
    {
        scaleBoost = boost;
    }
    
    public void SetAmountColors(Color[] colors)
    {
        if (colors != null && colors.Length == 4)
        {
            amountColors = colors;
            SetupVisualsByAmount();
        }
    }
    
    public bool IsProcessed()
    {
        return isProcessed;
    }
    
    public string GetFormattedAmount()
    {
        return $"￥{superChatAmount:N0}";
    }
    
    public SuperChatTier GetTier()
    {
        if (superChatAmount >= 5000) return SuperChatTier.Legendary;
        if (superChatAmount >= 1000) return SuperChatTier.Epic;
        if (superChatAmount >= 500) return SuperChatTier.Rare;
        return SuperChatTier.Common;
    }
    
    private void OnDisable()
    {
        CancelInvoke(nameof(AutoProcess));
        isAutoProcessing = false;
        StopSuperChatEffects();
        
        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
        }
    }
}

public enum SuperChatTier
{
    Common,    // 100
    Rare,      // 500
    Epic,      // 1000
    Legendary  // 5000
}