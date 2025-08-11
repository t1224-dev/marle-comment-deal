using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }
    
    [Header("Top Panel")]
    [SerializeField] private TextMeshProUGUI faithText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider inflammationSlider;
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Game Area")]
    [SerializeField] private RectTransform gameArea;
    [SerializeField] private Image backgroundImage;
    
    [Header("Character Area")]
    [SerializeField] private Image marleImage;
    [SerializeField] private TextMeshProUGUI reactionText;
    [SerializeField] private GameObject reactionPanel;
    
    [Header("Game Controls")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button menuButton;
    
    [Header("Debug Info")]
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private GameObject debugPanel;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }
    
    private void InitializeUI()
    {
        if (faithText != null)
        {
            faithText.text = "Faith: 0";
        }
        
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = 100;
            healthSlider.value = 100;
        }
        
        if (inflammationSlider != null)
        {
            inflammationSlider.minValue = 0;
            inflammationSlider.maxValue = 100;
            inflammationSlider.value = 0;
        }
        
        if (timeText != null)
        {
            timeText.text = "Time: 60";
        }
        
        if (reactionPanel != null)
        {
            reactionPanel.SetActive(false);
        }
        
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
        
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuButtonClicked);
        }
    }
    
    private void SubscribeToEvents()
    {
        if (FaithSystem.Instance != null)
        {
            FaithSystem.Instance.OnFaithChanged += UpdateFaithDisplay;
            FaithSystem.Instance.OnFaithGained += OnFaithGained;
            FaithSystem.Instance.OnFaithLost += OnFaithLost;
        }
        
        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.OnHealthChanged += UpdateHealthDisplay;
        }
        
        if (InflamationSystem.Instance != null)
        {
            InflamationSystem.Instance.OnInflamationChanged += UpdateInflamationDisplay;
            InflamationSystem.Instance.OnCriticalInflamation += OnInflamationCritical;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameTimeChanged += UpdateTimeDisplay;
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
    }
    
    private void UpdateFaithDisplay(int faithAmount)
    {
        if (faithText != null)
        {
            faithText.text = $"Faith: {faithAmount}";
        }
    }
    
    private void OnFaithGained(int amount)
    {
        if (faithText != null)
        {
            faithText.color = Color.green;
            Invoke(nameof(ResetFaithColor), 0.5f);
        }
        
        ShowReaction($"+{amount} Faith!");
    }
    
    private void OnFaithLost(int amount)
    {
        if (faithText != null)
        {
            faithText.color = Color.red;
            Invoke(nameof(ResetFaithColor), 0.5f);
        }
        
        ShowReaction($"-{amount} Faith...");
    }
    
    private void ResetFaithColor()
    {
        if (faithText != null)
        {
            faithText.color = Color.white;
        }
    }
    
    private void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            
            float healthPercent = (float)currentHealth / maxHealth;
            
            if (healthPercent <= 0.3f)
            {
                var colors = healthSlider.colors;
                colors.normalColor = Color.red;
                healthSlider.colors = colors;
            }
            else if (healthPercent <= 0.6f)
            {
                var colors = healthSlider.colors;
                colors.normalColor = Color.yellow;
                healthSlider.colors = colors;
            }
            else
            {
                var colors = healthSlider.colors;
                colors.normalColor = Color.green;
                healthSlider.colors = colors;
            }
        }
    }
    
    private void OnHealthCritical()
    {
        ShowReaction("Health Critical!");
        
        if (healthSlider != null)
        {
            StartCoroutine(FlashHealthBar());
        }
    }
    
    private System.Collections.IEnumerator FlashHealthBar()
    {
        for (int i = 0; i < 6; i++)
        {
            healthSlider.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            healthSlider.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private void UpdateInflamationDisplay(float currentInflamation, float maxInflamation)
    {
        if (inflammationSlider != null)
        {
            inflammationSlider.maxValue = maxInflamation;
            inflammationSlider.value = currentInflamation;
            
            float inflammationPercent = currentInflamation / maxInflamation;
            
            if (inflammationPercent >= 0.8f)
            {
                var colors = inflammationSlider.colors;
                colors.normalColor = Color.red;
                inflammationSlider.colors = colors;
            }
            else if (inflammationPercent >= 0.5f)
            {
                var colors = inflammationSlider.colors;
                colors.normalColor = Color.yellow;
                inflammationSlider.colors = colors;
            }
            else
            {
                var colors = inflammationSlider.colors;
                colors.normalColor = Color.blue;
                inflammationSlider.colors = colors;
            }
        }
    }
    
    private void OnInflammationCritical()
    {
        ShowReaction("Inflammation Critical!");
        
        if (inflammationSlider != null)
        {
            StartCoroutine(FlashInflammationBar());
        }
    }
    
    private System.Collections.IEnumerator FlashInflammationBar()
    {
        for (int i = 0; i < 6; i++)
        {
            inflammationSlider.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            inflammationSlider.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private void UpdateTimeDisplay(float currentTime)
    {
        if (timeText != null)
        {
            float remainingTime = GameManager.Instance.GetRemainingTime();
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timeText.text = $"Time: {minutes:00}:{seconds:00}";
            
            if (remainingTime <= 10f)
            {
                timeText.color = Color.red;
            }
            else if (remainingTime <= 30f)
            {
                timeText.color = Color.yellow;
            }
            else
            {
                timeText.color = Color.white;
            }
        }
    }
    
    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                SetUIActive(true);
                break;
            case GameState.Paused:
                SetUIActive(true);
                break;
            case GameState.GameOver:
            case GameState.MainMenu:
                SetUIActive(false);
                break;
        }
    }
    
    private void SetUIActive(bool active)
    {
        if (gameArea != null)
        {
            gameArea.gameObject.SetActive(active);
        }
    }
    
    private void ShowReaction(string message)
    {
        if (reactionText != null && reactionPanel != null)
        {
            reactionText.text = message;
            reactionPanel.SetActive(true);
            
            CancelInvoke(nameof(HideReaction));
            Invoke(nameof(HideReaction), 2f);
        }
    }
    
    private void HideReaction()
    {
        if (reactionPanel != null)
        {
            reactionPanel.SetActive(false);
        }
    }
    
    private void OnPauseButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                GameManager.Instance.PauseGame();
            }
            else if (GameManager.Instance.CurrentState == GameState.Paused)
            {
                GameManager.Instance.ResumeGame();
            }
        }
    }
    
    private void OnMenuButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadMainMenu();
        }
    }
    
    public void ShowDebugInfo(string info)
    {
        if (debugText != null)
        {
            debugText.text = info;
        }
        
        if (debugPanel != null)
        {
            debugPanel.SetActive(true);
        }
    }
    
    public void HideDebugInfo()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }
    
    public void SetMarleExpression(Sprite expression)
    {
        if (marleImage != null)
        {
            marleImage.sprite = expression;
        }
    }
    
    public void SetBackgroundImage(Sprite background)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = background;
        }
    }
    
    public void UpdateActiveCommentCount(int count)
    {
        if (debugText != null)
        {
            debugText.text = $"Active Comments: {count}";
        }
    }
    
    private void OnDestroy()
    {
        if (FaithSystem.Instance != null)
        {
            FaithSystem.Instance.OnFaithChanged -= UpdateFaithDisplay;
            FaithSystem.Instance.OnFaithGained -= OnFaithGained;
            FaithSystem.Instance.OnFaithLost -= OnFaithLost;
        }
        
        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.OnHealthChanged -= UpdateHealthDisplay;
        }
        
        if (InflamationSystem.Instance != null)
        {
            InflamationSystem.Instance.OnInflamationChanged -= UpdateInflamationDisplay;
            InflamationSystem.Instance.OnCriticalInflamation -= OnInflamationCritical;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameTimeChanged -= UpdateTimeDisplay;
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}