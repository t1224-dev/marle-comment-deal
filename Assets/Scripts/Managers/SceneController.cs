using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    
    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "00_MainMenu";
    [SerializeField] private string gameLevelScene = "01_GameLevel";
    [SerializeField] private string resultScene = "02_Result";
    [SerializeField] private string settingsScene = "03_Settings";
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeTransitionTime = 0.5f;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    
    public event Action<string> OnSceneLoadStarted;
    public event Action<string> OnSceneLoadCompleted;
    
    private bool isTransitioning = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeCanvas();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void CreateFadeCanvas()
    {
        if (fadeCanvasGroup == null)
        {
            GameObject fadeCanvas = new GameObject("FadeCanvas");
            DontDestroyOnLoad(fadeCanvas);
            
            Canvas canvas = fadeCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            fadeCanvasGroup = fadeCanvas.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
            
            GameObject fadePanel = new GameObject("FadePanel");
            fadePanel.transform.SetParent(fadeCanvas.transform, false);
            
            UnityEngine.UI.Image fadeImage = fadePanel.AddComponent<UnityEngine.UI.Image>();
            fadeImage.color = Color.black;
            
            RectTransform rectTransform = fadePanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
    
    public void LoadMainMenu()
    {
        LoadScene(mainMenuScene);
    }
    
    public void LoadGameLevel()
    {
        LoadScene(gameLevelScene);
    }
    
    public void LoadResult()
    {
        LoadScene(resultScene);
    }
    
    public void LoadSettings()
    {
        LoadScene(settingsScene);
    }
    
    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;
        
        StartCoroutine(LoadSceneWithFade(sceneName));
    }
    
    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        isTransitioning = true;
        OnSceneLoadStarted?.Invoke(sceneName);
        
        yield return StartCoroutine(FadeOut());
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
        
        yield return StartCoroutine(FadeIn());
        
        OnSceneLoadCompleted?.Invoke(sceneName);
        isTransitioning = false;
    }
    
    private IEnumerator FadeOut()
    {
        fadeCanvasGroup.blocksRaycasts = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTransitionTime)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeTransitionTime);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTransitionTime)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeTransitionTime));
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
    
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
}