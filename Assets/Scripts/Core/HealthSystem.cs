using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    public static HealthSystem Instance { get; private set; }
    
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int initialHealth = 5;
    [SerializeField] private bool canRecoverHealth = true;
    
    [Header("Health Recovery")]
    [SerializeField] private int recoveryAmount = 1;
    [SerializeField] private float recoveryInterval = 30f;
    [SerializeField] private bool autoRecovery = false;
    
    [Header("Damage Settings")]
    [SerializeField] private int trollCommentDamage = 1;
    [SerializeField] private float invulnerabilityTime = 1f;
    
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get => maxHealth; }
    public bool IsInvulnerable { get; private set; }
    
    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnHealthLost;
    public event Action<int> OnHealthRecovered;
    public event Action OnHealthZero;
    public event Action OnMaxHealthReached;
    
    private float lastRecoveryTime;
    private float invulnerabilityTimer;
    
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
        ResetHealth();
        lastRecoveryTime = Time.time;
    }
    
    private void Update()
    {
        UpdateInvulnerability();
        UpdateAutoRecovery();
    }
    
    private void UpdateInvulnerability()
    {
        if (IsInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                IsInvulnerable = false;
            }
        }
    }
    
    private void UpdateAutoRecovery()
    {
        if (autoRecovery && canRecoverHealth && CurrentHealth < maxHealth)
        {
            if (Time.time - lastRecoveryTime >= recoveryInterval)
            {
                RecoverHealth(recoveryAmount);
                lastRecoveryTime = Time.time;
            }
        }
    }
    
    public void ResetHealth()
    {
        CurrentHealth = initialHealth;
        IsInvulnerable = false;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
    
    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsInvulnerable || CurrentHealth <= 0) return;
        
        int previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnHealthLost?.Invoke(damage);
        
        if (CurrentHealth <= 0)
        {
            OnHealthZero?.Invoke();
        }
        
        SetInvulnerable();
    }
    
    public void RecoverHealth(int amount)
    {
        if (amount <= 0 || CurrentHealth >= maxHealth || !canRecoverHealth) return;
        
        int previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnHealthRecovered?.Invoke(amount);
        
        if (CurrentHealth >= maxHealth)
        {
            OnMaxHealthReached?.Invoke();
        }
    }
    
    public void ProcessTrollCommentDamage()
    {
        TakeDamage(trollCommentDamage);
    }
    
    private void SetInvulnerable()
    {
        IsInvulnerable = true;
        invulnerabilityTimer = invulnerabilityTime;
    }
    
    public float GetHealthPercentage()
    {
        if (maxHealth <= 0) return 0f;
        return (float)CurrentHealth / maxHealth;
    }
    
    public bool IsHealthCritical()
    {
        return CurrentHealth <= 1;
    }
    
    public bool IsHealthFull()
    {
        return CurrentHealth >= maxHealth;
    }
    
    public bool IsAlive()
    {
        return CurrentHealth > 0;
    }
    
    public void SetMaxHealth(int newMaxHealth)
    {
        if (newMaxHealth <= 0) return;
        
        int oldMaxHealth = maxHealth;
        maxHealth = newMaxHealth;
        
        if (CurrentHealth > maxHealth)
        {
            CurrentHealth = maxHealth;
        }
        
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
    
    public void SetCanRecoverHealth(bool canRecover)
    {
        canRecoverHealth = canRecover;
    }
    
    public void SetAutoRecovery(bool autoRecover)
    {
        autoRecovery = autoRecover;
    }
    
    public void SetRecoveryParameters(int amount, float interval)
    {
        recoveryAmount = amount;
        recoveryInterval = interval;
    }
    
    public void SetDamageAmount(int damage)
    {
        trollCommentDamage = damage;
    }
    
    public void SetInvulnerabilityTime(float time)
    {
        invulnerabilityTime = time;
    }
    
    public void FullHeal()
    {
        RecoverHealth(maxHealth);
    }
    
    public int GetMissingHealth()
    {
        return maxHealth - CurrentHealth;
    }
    
    public HealthSystemData GetCurrentData()
    {
        return new HealthSystemData
        {
            currentHealth = CurrentHealth,
            maxHealth = maxHealth,
            canRecoverHealth = canRecoverHealth,
            autoRecovery = autoRecovery,
            recoveryAmount = recoveryAmount,
            recoveryInterval = recoveryInterval
        };
    }
    
    public void LoadData(HealthSystemData data)
    {
        CurrentHealth = data.currentHealth;
        maxHealth = data.maxHealth;
        canRecoverHealth = data.canRecoverHealth;
        autoRecovery = data.autoRecovery;
        recoveryAmount = data.recoveryAmount;
        recoveryInterval = data.recoveryInterval;
        
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}

[System.Serializable]
public class HealthSystemData
{
    public int currentHealth;
    public int maxHealth;
    public bool canRecoverHealth;
    public bool autoRecovery;
    public int recoveryAmount;
    public float recoveryInterval;
}