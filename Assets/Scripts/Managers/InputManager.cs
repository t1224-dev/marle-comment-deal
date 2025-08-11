using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    [Header("Touch/Click Settings")]
    [SerializeField] private float doubleTapTimeThreshold = 0.3f;
    [SerializeField] private float doubleTapDistanceThreshold = 50f;
    
    public event Action<Vector2> OnSingleTap;
    public event Action<Vector2> OnDoubleTap;
    public event Action<Vector2> OnTouchStart;
    public event Action<Vector2> OnTouchEnd;
    
    private Vector2 lastTapPosition;
    private float lastTapTime;
    private bool waitingForDoubleTap;
    
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
    
    private void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        if (Application.isMobilePlatform)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }
    
    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = touch.position;
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchStart(touchPosition);
                    break;
                    
                case TouchPhase.Ended:
                    HandleTouchEnd(touchPosition);
                    break;
            }
        }
    }
    
    private void HandleMouseInput()
    {
        Vector2 mousePosition = Input.mousePosition;
        
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouchStart(mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            HandleTouchEnd(mousePosition);
        }
    }
    
    private void HandleTouchStart(Vector2 position)
    {
        OnTouchStart?.Invoke(position);
    }
    
    private void HandleTouchEnd(Vector2 position)
    {
        OnTouchEnd?.Invoke(position);
        
        float currentTime = Time.time;
        float timeSinceLastTap = currentTime - lastTapTime;
        float distanceFromLastTap = Vector2.Distance(position, lastTapPosition);
        
        if (waitingForDoubleTap && 
            timeSinceLastTap <= doubleTapTimeThreshold && 
            distanceFromLastTap <= doubleTapDistanceThreshold)
        {
            waitingForDoubleTap = false;
            OnDoubleTap?.Invoke(position);
        }
        else
        {
            if (waitingForDoubleTap)
            {
                OnSingleTap?.Invoke(lastTapPosition);
            }
            
            waitingForDoubleTap = true;
            lastTapPosition = position;
            lastTapTime = currentTime;
            
            Invoke(nameof(ProcessSingleTap), doubleTapTimeThreshold);
        }
    }
    
    private void ProcessSingleTap()
    {
        if (waitingForDoubleTap)
        {
            waitingForDoubleTap = false;
            OnSingleTap?.Invoke(lastTapPosition);
        }
    }
    
    public Vector2 GetWorldPositionFromScreenPosition(Vector2 screenPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera != null)
        {
            return mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));
        }
        
        return Vector2.zero;
    }
    
    public bool IsPointerOverGameObject()
    {
        if (Application.isMobilePlatform)
        {
            return Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        else
        {
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
    }
}