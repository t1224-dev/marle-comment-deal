using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [Header("FPS Counter Settings")]
    [SerializeField] private bool showFPS = true;
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    
    [Header("Thresholds")]
    [SerializeField] private float warningThreshold = 45f;
    [SerializeField] private float criticalThreshold = 30f;
    
    private TextMeshProUGUI fpsText;
    private float deltaTime = 0f;
    private float lastUpdateTime = 0f;
    private int frameCount = 0;
    private float currentFPS = 0f;
    
    private void Start()
    {
        CreateFPSDisplay();
    }
    
    private void CreateFPSDisplay()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("FPSCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            DontDestroyOnLoad(canvasObject);
        }
        
        GameObject fpsObject = new GameObject("FPSText");
        fpsObject.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = fpsObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(10, -10);
        rectTransform.sizeDelta = new Vector2(200, 50);
        
        fpsText = fpsObject.AddComponent<TextMeshProUGUI>();
        fpsText.text = "FPS: 0";
        fpsText.fontSize = 18;
        fpsText.color = normalColor;
        fpsText.alignment = TextAlignmentOptions.TopLeft;
        fpsText.sortingOrder = 1001;
        
        DontDestroyOnLoad(fpsObject);
    }
    
    private void Update()
    {
        if (!showFPS)
        {
            if (fpsText != null)
            {
                fpsText.gameObject.SetActive(false);
            }
            return;
        }
        
        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(true);
        }
        
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        frameCount++;
        
        if (Time.unscaledTime - lastUpdateTime >= updateInterval)
        {
            currentFPS = frameCount / (Time.unscaledTime - lastUpdateTime);
            frameCount = 0;
            lastUpdateTime = Time.unscaledTime;
            
            UpdateFPSDisplay();
        }
    }
    
    private void UpdateFPSDisplay()
    {
        if (fpsText == null) return;
        
        float fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {fps:F1}\nAvg: {currentFPS:F1}";
        
        if (fps < criticalThreshold)
        {
            fpsText.color = criticalColor;
        }
        else if (fps < warningThreshold)
        {
            fpsText.color = warningColor;
        }
        else
        {
            fpsText.color = normalColor;
        }
    }
    
    public void SetVisible(bool visible)
    {
        showFPS = visible;
        
        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(visible);
        }
    }
    
    public void SetUpdateInterval(float interval)
    {
        updateInterval = interval;
    }
    
    public void SetThresholds(float warning, float critical)
    {
        warningThreshold = warning;
        criticalThreshold = critical;
    }
    
    public float GetCurrentFPS()
    {
        return currentFPS;
    }
    
    public float GetInstantFPS()
    {
        return 1.0f / deltaTime;
    }
    
    public bool IsPerformanceGood()
    {
        return currentFPS >= warningThreshold;
    }
    
    public bool IsPerformanceCritical()
    {
        return currentFPS < criticalThreshold;
    }
}