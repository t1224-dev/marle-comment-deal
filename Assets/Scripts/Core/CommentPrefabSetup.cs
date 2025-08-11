using UnityEngine;

[System.Serializable]
public class CommentPrefabSetup : MonoBehaviour
{
    [Header("Prefab Setup")]
    [SerializeField] private CommentType commentType;
    [SerializeField] private bool autoSetupOnStart = true;
    
    [Header("Visual Settings")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Vector2 prefabSize = new Vector2(2f, 0.5f);
    [SerializeField] private string defaultText = "Sample Comment";
    
    [Header("Physics Settings")]
    [SerializeField] private bool useBoxCollider = true;
    [SerializeField] private bool isTrigger = false;
    
    [Header("Components")]
    [SerializeField] private bool addRigidbody = false;
    [SerializeField] private bool freezeRotation = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupPrefab();
        }
    }
    
    [ContextMenu("Setup Prefab")]
    public void SetupPrefab()
    {
        SetupSpriteRenderer();
        SetupTextMesh();
        SetupCollider();
        SetupRigidbody();
        SetupCommentComponent();
        SetupCommentMover();
        
        Debug.Log($"Prefab setup completed for {commentType}");
    }
    
    private void SetupSpriteRenderer()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        spriteRenderer.sprite = CreateDefaultSprite();
        spriteRenderer.color = GetDefaultColorForType(commentType);
        spriteRenderer.sortingOrder = 1;
    }
    
    private void SetupTextMesh()
    {
        TMPro.TextMeshPro textMesh = GetComponentInChildren<TMPro.TextMeshPro>();
        if (textMesh == null)
        {
            GameObject textObject = new GameObject("CommentText");
            textObject.transform.SetParent(transform);
            textObject.transform.localPosition = Vector3.zero;
            textObject.transform.localScale = Vector3.one;
            
            textMesh = textObject.AddComponent<TMPro.TextMeshPro>();
        }
        
        textMesh.text = defaultText;
        textMesh.fontSize = 0.5f;
        textMesh.color = Color.black;
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;
        textMesh.sortingOrder = 2;
        
        RectTransform rectTransform = textMesh.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = prefabSize;
        }
    }
    
    private void SetupCollider()
    {
        if (useBoxCollider)
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }
            
            collider.size = prefabSize;
            collider.isTrigger = isTrigger;
        }
    }
    
    private void SetupRigidbody()
    {
        if (addRigidbody)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            
            rb.gravityScale = 0f;
            rb.drag = 0f;
            rb.angularDrag = 0f;
            
            if (freezeRotation)
            {
                rb.freezeRotation = true;
            }
        }
    }
    
    private void SetupCommentComponent()
    {
        CommentBase existingComment = GetComponent<CommentBase>();
        if (existingComment != null)
        {
            DestroyImmediate(existingComment);
        }
        
        switch (commentType)
        {
            case CommentType.Holy:
                gameObject.AddComponent<HolyComment>();
                break;
            case CommentType.Ohoe:
                gameObject.AddComponent<OhoeComment>();
                break;
            case CommentType.Troll:
                gameObject.AddComponent<TrollComment>();
                break;
            case CommentType.SuperChat:
                gameObject.AddComponent<SuperChatComment>();
                break;
        }
    }
    
    private void SetupCommentMover()
    {
        CommentMover mover = GetComponent<CommentMover>();
        if (mover == null)
        {
            mover = gameObject.AddComponent<CommentMover>();
        }
        
        mover.SetSpeed(GetDefaultSpeedForType(commentType));
    }
    
    private Sprite CreateDefaultSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }
    
    private Color GetDefaultColorForType(CommentType type)
    {
        switch (type)
        {
            case CommentType.Holy:
                return Color.yellow;
            case CommentType.Ohoe:
                return Color.green;
            case CommentType.Troll:
                return Color.red;
            case CommentType.SuperChat:
                return Color.magenta;
            default:
                return defaultColor;
        }
    }
    
    private float GetDefaultSpeedForType(CommentType type)
    {
        switch (type)
        {
            case CommentType.Holy:
                return 1.5f;
            case CommentType.Ohoe:
                return 2f;
            case CommentType.Troll:
                return 2.5f;
            case CommentType.SuperChat:
                return 1f;
            default:
                return 2f;
        }
    }
    
    public void SetCommentType(CommentType type)
    {
        commentType = type;
    }
    
    public CommentType GetCommentType()
    {
        return commentType;
    }
    
    public void SetDefaultColor(Color color)
    {
        defaultColor = color;
    }
    
    public void SetPrefabSize(Vector2 size)
    {
        prefabSize = size;
    }
    
    public void SetDefaultText(string text)
    {
        defaultText = text;
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Create All Comment Prefabs")]
    public void CreateAllCommentPrefabs()
    {
        CreateCommentPrefab(CommentType.Holy);
        CreateCommentPrefab(CommentType.Ohoe);
        CreateCommentPrefab(CommentType.Troll);
        CreateCommentPrefab(CommentType.SuperChat);
    }
    
    private void CreateCommentPrefab(CommentType type)
    {
        GameObject prefab = new GameObject($"{type}Comment");
        CommentPrefabSetup setup = prefab.AddComponent<CommentPrefabSetup>();
        setup.SetCommentType(type);
        setup.SetDefaultText(GetDefaultTextForType(type));
        setup.SetupPrefab();
        
        string path = $"Assets/Prefabs/{type}Comment.prefab";
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(prefab, path);
        
        Debug.Log($"Created prefab: {path}");
        
        DestroyImmediate(prefab);
    }
    
    private string GetDefaultTextForType(CommentType type)
    {
        switch (type)
        {
            case CommentType.Holy:
                return "マールちゃん可愛い！";
            case CommentType.Ohoe:
                return "CV:おおえのたかゆき";
            case CommentType.Troll:
                return "つまらない";
            case CommentType.SuperChat:
                return "￥100 マールちゃん頑張って！";
            default:
                return "Sample Comment";
        }
    }
    #endif
}