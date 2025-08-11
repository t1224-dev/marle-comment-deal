using UnityEngine;
using System;

public class CommentMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float speedVariation = 0.5f;
    [SerializeField] private bool randomizeSpeed = true;
    
    [Header("Bounds Settings")]
    [SerializeField] private float leftBoundOffset = -2f;
    [SerializeField] private float rightBoundOffset = 2f;
    
    [Header("Movement Effects")]
    [SerializeField] private bool enableFloating = true;
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatFrequency = 1f;
    
    public float CurrentSpeed { get; private set; }
    public bool IsMoving { get; private set; }
    
    public event Action<CommentMover> OnReachedLeftBound;
    public event Action<CommentMover> OnMovementStarted;
    public event Action<CommentMover> OnMovementStopped;
    
    private Vector3 startPosition;
    private float floatTimer;
    private Camera mainCamera;
    private CommentBase commentBase;
    
    private void Awake()
    {
        commentBase = GetComponent<CommentBase>();
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        InitializeSpeed();
    }
    
    private void Start()
    {
        startPosition = transform.position;
        SetupSpawnPosition();
    }
    
    private void Update()
    {
        if (IsMoving)
        {
            UpdateMovement();
            CheckBounds();
        }
    }
    
    private void InitializeSpeed()
    {
        CurrentSpeed = baseSpeed;
        
        if (randomizeSpeed)
        {
            float randomFactor = UnityEngine.Random.Range(1f - speedVariation, 1f + speedVariation);
            CurrentSpeed *= randomFactor;
        }
    }
    
    private void SetupSpawnPosition()
    {
        if (mainCamera == null) return;
        
        Vector3 rightBound = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, 0));
        Vector3 spawnPosition = new Vector3(rightBound.x + rightBoundOffset, transform.position.y, transform.position.z);
        transform.position = spawnPosition;
        startPosition = spawnPosition;
    }
    
    public void StartMovement()
    {
        IsMoving = true;
        OnMovementStarted?.Invoke(this);
        
        if (commentBase != null)
        {
            commentBase.StartMoving();
        }
    }
    
    public void StopMovement()
    {
        IsMoving = false;
        OnMovementStopped?.Invoke(this);
        
        if (commentBase != null)
        {
            commentBase.StopMoving();
        }
    }
    
    private void UpdateMovement()
    {
        Vector3 movement = Vector3.left * CurrentSpeed * Time.deltaTime;
        
        if (enableFloating)
        {
            floatTimer += Time.deltaTime * floatFrequency;
            float floatOffset = Mathf.Sin(floatTimer) * floatAmplitude;
            movement.y += floatOffset * Time.deltaTime;
        }
        
        transform.Translate(movement);
    }
    
    private void CheckBounds()
    {
        if (mainCamera == null) return;
        
        Vector3 leftBound = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        
        if (transform.position.x < leftBound.x + leftBoundOffset)
        {
            OnReachedLeftBound?.Invoke(this);
        }
    }
    
    public void SetSpeed(float speed)
    {
        CurrentSpeed = speed;
    }
    
    public void ModifySpeed(float multiplier)
    {
        CurrentSpeed *= multiplier;
    }
    
    public void ResetSpeed()
    {
        InitializeSpeed();
    }
    
    public void SetSpeedVariation(float variation)
    {
        speedVariation = variation;
    }
    
    public void SetFloatingEnabled(bool enabled)
    {
        enableFloating = enabled;
    }
    
    public void SetFloatingParameters(float amplitude, float frequency)
    {
        floatAmplitude = amplitude;
        floatFrequency = frequency;
    }
    
    public float GetProgressToLeftBound()
    {
        if (mainCamera == null) return 0f;
        
        Vector3 leftBound = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 rightBound = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0));
        
        float totalDistance = rightBound.x - leftBound.x;
        float currentDistance = transform.position.x - leftBound.x;
        
        return 1f - Mathf.Clamp01(currentDistance / totalDistance);
    }
    
    public float GetTimeToReachLeftBound()
    {
        if (mainCamera == null || CurrentSpeed <= 0) return float.MaxValue;
        
        Vector3 leftBound = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        float distanceToLeft = transform.position.x - (leftBound.x + leftBoundOffset);
        
        return distanceToLeft / CurrentSpeed;
    }
    
    public void PauseMovement()
    {
        IsMoving = false;
    }
    
    public void ResumeMovement()
    {
        IsMoving = true;
    }
    
    private void OnDestroy()
    {
        OnReachedLeftBound = null;
        OnMovementStarted = null;
        OnMovementStopped = null;
    }
}