using UnityEngine;
using System;

public class InflamationSystem : MonoBehaviour
{
    public static InflamationSystem Instance { get; private set; }
    
    [Header("Inflamation Settings")]
    [SerializeField] private float maxInflamation = 100f;
    [SerializeField] private float initialInflamation = 0f;
    [SerializeField] private float criticalThreshold = 100f;
    
    [Header("Inflamation Increase")]
    [SerializeField] private float trollCommentInflamation = 20f;
    [SerializeField] private float missedTrollInflamation = 25f;
    
    [Header("Inflamation Decay")]
    [SerializeField] private float decayRate = 5f;
    [SerializeField] private float decayDelay = 2f;
    [SerializeField] private bool enableAutoDecay = true;
    
    [Header("Inflamation Effects")]
    [SerializeField] private float trollSpawnMultiplier = 1.5f;
    [SerializeField] private float commentSpeedMultiplier = 1.2f;
    [SerializeField] private float inflammationDuration = 5f;
    
    public float CurrentInflamation { get; private set; }
    public float MaxInflamation { get => maxInflamation; }
    public bool IsInflamed { get; private set; }
    public bool IsCritical { get; private set; }
    
    public event Action<float, float> OnInflamationChanged;
    public event Action<float> OnInflamationIncreased;
    public event Action<float> OnInflamationDecreased;
    public event Action OnInflamationStarted;
    public event Action OnInflamationEnded;
    public event Action OnCriticalInflamation;
    
    private float lastInflamationTime;
    private float inflammationTimer;
    
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
        ResetInflamation();
    }
    
    private void Update()
    {
        UpdateInflamationDecay();
        UpdateInflamationTimer();
    }
    
    private void UpdateInflamationDecay()
    {
        if (enableAutoDecay && CurrentInflamation > 0f)
        {
            if (Time.time - lastInflamationTime >= decayDelay)
            {
                float decay = decayRate * Time.deltaTime;
                DecreaseInflamation(decay);
            }
        }
    }
    
    private void UpdateInflamationTimer()
    {
        if (IsInflamed)
        {
            inflammationTimer -= Time.deltaTime;
            if (inflammationTimer <= 0f)
            {
                EndInflamation();
            }
        }
    }
    
    public void ResetInflamation()
    {
        CurrentInflamation = initialInflamation;
        IsInflamed = false;
        IsCritical = false;
        inflammationTimer = 0f;
        OnInflamationChanged?.Invoke(CurrentInflamation, maxInflamation);
    }
    
    public void IncreaseInflamation(float amount)
    {
        if (amount <= 0f) return;
        
        float previousInflamation = CurrentInflamation;
        CurrentInflamation = Mathf.Min(maxInflamation, CurrentInflamation + amount);
        lastInflamationTime = Time.time;
        
        OnInflamationChanged?.Invoke(CurrentInflamation, maxInflamation);
        OnInflamationIncreased?.Invoke(amount);
        
        CheckInflamationState(previousInflamation);
    }
    
    public void DecreaseInflamation(float amount)
    {
        if (amount <= 0f) return;
        
        float previousInflamation = CurrentInflamation;
        CurrentInflamation = Mathf.Max(0f, CurrentInflamation - amount);
        
        OnInflamationChanged?.Invoke(CurrentInflamation, maxInflamation);
        OnInflamationDecreased?.Invoke(amount);
        
        CheckInflamationState(previousInflamation);
    }
    
    private void CheckInflamationState(float previousInflamation)
    {
        bool wasCritical = IsCritical;
        bool wasInflamed = IsInflamed;
        
        IsCritical = CurrentInflamation >= criticalThreshold;
        
        if (!wasCritical && IsCritical)
        {
            OnCriticalInflamation?.Invoke();
        }
        
        if (CurrentInflamation > 0f && !wasInflamed)
        {
            StartInflamation();
        }
        else if (CurrentInflamation <= 0f && wasInflamed)
        {
            EndInflamation();
        }
    }
    
    private void StartInflamation()
    {
        IsInflamed = true;
        inflammationTimer = inflammationDuration;
        OnInflamationStarted?.Invoke();
    }
    
    private void EndInflamation()
    {
        IsInflamed = false;
        IsCritical = false;
        inflammationTimer = 0f;
        OnInflamationEnded?.Invoke();
    }
    
    public void ProcessTrollCommentInflamation()
    {
        IncreaseInflamation(missedTrollInflamation);
    }
    
    public void ProcessMissedTrollComment()
    {
        IncreaseInflamation(missedTrollInflamation);
    }
    
    public float GetInflamationPercentage()
    {
        if (maxInflamation <= 0f) return 0f;
        return CurrentInflamation / maxInflamation;
    }
    
    public float GetTrollSpawnMultiplier()
    {
        return IsInflamed ? trollSpawnMultiplier : 1f;
    }
    
    public float GetCommentSpeedMultiplier()
    {
        return IsInflamed ? commentSpeedMultiplier : 1f;
    }
    
    public bool IsInflamationCritical()
    {
        return IsCritical;
    }
    
    public float GetRemainingInflamationTime()
    {
        return IsInflamed ? inflammationTimer : 0f;
    }
    
    public void SetMaxInflamation(float max)
    {
        if (max <= 0f) return;
        
        maxInflamation = max;
        if (CurrentInflamation > maxInflamation)
        {
            CurrentInflamation = maxInflamation;
        }
        
        OnInflamationChanged?.Invoke(CurrentInflamation, maxInflamation);
    }
    
    public void SetDecayRate(float rate)
    {
        decayRate = rate;
    }
    
    public void SetDecayDelay(float delay)
    {
        decayDelay = delay;
    }
    
    public void SetAutoDecay(bool enabled)
    {
        enableAutoDecay = enabled;
    }
    
    public void SetInflamationDuration(float duration)
    {
        inflammationDuration = duration;
    }
    
    public void SetInflamationEffects(float trollMultiplier, float speedMultiplier)
    {
        trollSpawnMultiplier = trollMultiplier;
        commentSpeedMultiplier = speedMultiplier;
    }
    
    public void SetCriticalThreshold(float threshold)
    {
        criticalThreshold = threshold;
    }
    
    public void ClearInflamation()
    {
        DecreaseInflamation(CurrentInflamation);
    }
    
    public void SetInflamationAmounts(float trollAmount, float missedTrollAmount)
    {
        trollCommentInflamation = trollAmount;
        missedTrollInflamation = missedTrollAmount;
    }
    
    public InflamationSystemData GetCurrentData()
    {
        return new InflamationSystemData
        {
            currentInflamation = CurrentInflamation,
            maxInflamation = maxInflamation,
            isInflamed = IsInflamed,
            isCritical = IsCritical,
            inflammationTimer = inflammationTimer,
            lastInflamationTime = lastInflamationTime
        };
    }
    
    public void LoadData(InflamationSystemData data)
    {
        CurrentInflamation = data.currentInflamation;
        maxInflamation = data.maxInflamation;
        IsInflamed = data.isInflamed;
        IsCritical = data.isCritical;
        inflammationTimer = data.inflammationTimer;
        lastInflamationTime = data.lastInflamationTime;
        
        OnInflamationChanged?.Invoke(CurrentInflamation, maxInflamation);
    }
}

[System.Serializable]
public class InflamationSystemData
{
    public float currentInflamation;
    public float maxInflamation;
    public bool isInflamed;
    public bool isCritical;
    public float inflammationTimer;
    public float lastInflamationTime;
}