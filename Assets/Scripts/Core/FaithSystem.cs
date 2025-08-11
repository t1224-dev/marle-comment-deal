using UnityEngine;
using System;

public class FaithSystem : MonoBehaviour
{
    public static FaithSystem Instance { get; private set; }
    
    [Header("Faith Settings")]
    [SerializeField] private int initialFaith = 0;
    [SerializeField] private int maxFaith = 9999;
    [SerializeField] private int minFaith = -100;
    
    [Header("Faith Rewards")]
    [SerializeField] private int holyCommentReward = 8;
    [SerializeField] private int ohoeCommentReward = 12;
    [SerializeField] private int trollCommentReward = 15;
    [SerializeField] private int superChatReward100 = 20;
    [SerializeField] private int superChatReward500 = 35;
    [SerializeField] private int superChatReward1000 = 60;
    [SerializeField] private int superChatReward5000 = 120;
    
    [Header("Faith Penalties")]
    [SerializeField] private int ohoeCommentPenalty = -15;
    [SerializeField] private int trollCommentPenalty = -25;
    [SerializeField] private int gameOverThreshold = -100;
    
    public int CurrentFaith { get; private set; }
    public int TargetFaith { get; private set; } = 200;
    
    public event Action<int> OnFaithChanged;
    public event Action<int> OnFaithGained;
    public event Action<int> OnFaithLost;
    public event Action OnTargetFaithReached;
    public event Action OnFaithGameOver;
    
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
        ResetFaith();
    }
    
    public void ResetFaith()
    {
        CurrentFaith = initialFaith;
        OnFaithChanged?.Invoke(CurrentFaith);
    }
    
    public void SetTargetFaith(int target)
    {
        TargetFaith = target;
    }
    
    public void ProcessHolyComment()
    {
        AddFaith(holyCommentReward);
    }
    
    public void ProcessOhoeCommentSuccess()
    {
        AddFaith(ohoeCommentReward);
    }
    
    public void ProcessOhoeCommentFail()
    {
        SubtractFaith(Mathf.Abs(ohoeCommentPenalty));
    }
    
    public void ProcessTrollCommentSuccess()
    {
        AddFaith(trollCommentReward);
    }
    
    public void ProcessTrollCommentFail()
    {
        SubtractFaith(Mathf.Abs(trollCommentPenalty));
    }
    
    public void ProcessSuperChat(int amount)
    {
        int reward = GetSuperChatReward(amount);
        AddFaith(reward);
    }
    
    private int GetSuperChatReward(int amount)
    {
        if (amount >= 5000)
            return superChatReward5000;
        else if (amount >= 1000)
            return superChatReward1000;
        else if (amount >= 500)
            return superChatReward500;
        else
            return superChatReward100;
    }
    
    public void AddFaith(int amount)
    {
        if (amount <= 0) return;
        
        int previousFaith = CurrentFaith;
        CurrentFaith = Mathf.Clamp(CurrentFaith + amount, minFaith, maxFaith);
        
        OnFaithChanged?.Invoke(CurrentFaith);
        OnFaithGained?.Invoke(amount);
        
        CheckTargetFaithReached(previousFaith);
    }
    
    public void SubtractFaith(int amount)
    {
        if (amount <= 0) return;
        
        int previousFaith = CurrentFaith;
        CurrentFaith = Mathf.Clamp(CurrentFaith - amount, minFaith, maxFaith);
        
        OnFaithChanged?.Invoke(CurrentFaith);
        OnFaithLost?.Invoke(amount);
        
        CheckGameOverCondition();
    }
    
    private void CheckTargetFaithReached(int previousFaith)
    {
        if (previousFaith < TargetFaith && CurrentFaith >= TargetFaith)
        {
            OnTargetFaithReached?.Invoke();
        }
    }
    
    private void CheckGameOverCondition()
    {
        if (CurrentFaith <= gameOverThreshold)
        {
            OnFaithGameOver?.Invoke();
        }
    }
    
    public float GetFaithProgress()
    {
        if (TargetFaith <= 0) return 0f;
        return Mathf.Clamp01((float)CurrentFaith / TargetFaith);
    }
    
    public int GetFaithDeficit()
    {
        return Mathf.Max(0, TargetFaith - CurrentFaith);
    }
    
    public bool HasReachedTarget()
    {
        return CurrentFaith >= TargetFaith;
    }
    
    public bool IsGameOverFaith()
    {
        return CurrentFaith <= gameOverThreshold;
    }
    
    public void SetFaithRewards(int holy, int ohoe, int troll)
    {
        holyCommentReward = holy;
        ohoeCommentReward = ohoe;
        trollCommentReward = troll;
    }
    
    public void SetFaithPenalties(int ohoe, int troll)
    {
        ohoeCommentPenalty = -Mathf.Abs(ohoe);
        trollCommentPenalty = -Mathf.Abs(troll);
    }
    
    public void SetSuperChatRewards(int reward100, int reward500, int reward1000, int reward5000)
    {
        superChatReward100 = reward100;
        superChatReward500 = reward500;
        superChatReward1000 = reward1000;
        superChatReward5000 = reward5000;
    }
    
    public FaithSystemData GetCurrentData()
    {
        return new FaithSystemData
        {
            currentFaith = CurrentFaith,
            targetFaith = TargetFaith,
            maxFaith = maxFaith,
            minFaith = minFaith,
            gameOverThreshold = gameOverThreshold
        };
    }
    
    public void LoadData(FaithSystemData data)
    {
        CurrentFaith = data.currentFaith;
        TargetFaith = data.targetFaith;
        maxFaith = data.maxFaith;
        minFaith = data.minFaith;
        gameOverThreshold = data.gameOverThreshold;
        
        OnFaithChanged?.Invoke(CurrentFaith);
    }
}

[System.Serializable]
public class FaithSystemData
{
    public int currentFaith;
    public int targetFaith;
    public int maxFaith;
    public int minFaith;
    public int gameOverThreshold;
}