using UnityEngine;

public class HolyComment : CommentBase
{
    [Header("Holy Comment Settings")]
    [SerializeField] private float autoProcessDelay = 0.5f;
    [SerializeField] private int faithReward = 8;
    [SerializeField] private Color holyGlowColor = Color.yellow;
    
    [Header("Holy Comment Effects")]
    [SerializeField] private ParticleSystem holyParticles;
    [SerializeField] private AudioClip holySound;
    
    private bool isAutoProcessing = false;
    
    protected override void Start()
    {
        base.Start();
        commentType = CommentType.Holy;
        
        if (holyParticles != null)
        {
            holyParticles.gameObject.SetActive(false);
        }
        
        StartAutoProcessing();
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
    
    protected override void OnProcessed()
    {
        if (FaithSystem.Instance != null)
        {
            FaithSystem.Instance.ProcessHolyComment();
        }
        
        PlayHolyEffects();
        
        if (GameManager.Instance != null)
        {
            Debug.Log($"Holy comment processed! Faith gained: {faithReward}");
        }
    }
    
    protected override void OnDamaged()
    {
        Debug.Log("Holy comment cannot be damaged - it's blessed!");
    }
    
    protected override void OnDestroyed()
    {
        StopHolyEffects();
        
        if (GameManager.Instance != null)
        {
            Debug.Log("Holy comment destroyed");
        }
    }
    
    private void PlayHolyEffects()
    {
        if (holyParticles != null)
        {
            holyParticles.gameObject.SetActive(true);
            holyParticles.Play();
        }
        
        if (holySound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(holySound);
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = holyGlowColor;
        }
    }
    
    private void StopHolyEffects()
    {
        if (holyParticles != null)
        {
            holyParticles.Stop();
        }
    }
    
    protected override void UpdateVisualForNormal()
    {
        base.UpdateVisualForNormal();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(Color.white, holyGlowColor, 0.3f);
        }
    }
    
    protected override void UpdateVisualForMoving()
    {
        base.UpdateVisualForMoving();
        
        if (spriteRenderer != null)
        {
            float glowIntensity = Mathf.PingPong(Time.time * 2f, 1f);
            spriteRenderer.color = Color.Lerp(Color.white, holyGlowColor, glowIntensity * 0.5f);
        }
    }
    
    public void SetFaithReward(int reward)
    {
        faithReward = reward;
    }
    
    public void SetAutoProcessDelay(float delay)
    {
        autoProcessDelay = delay;
    }
    
    public void SetHolyGlowColor(Color color)
    {
        holyGlowColor = color;
    }
    
    private void OnDisable()
    {
        CancelInvoke(nameof(AutoProcess));
        isAutoProcessing = false;
        StopHolyEffects();
    }
}