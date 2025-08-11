using UnityEngine;

[CreateAssetMenu(fileName = "CommentDistribution", menuName = "Marle Game/Comment Distribution")]
public class CommentDistribution : ScriptableObject
{
    [Header("Comment Spawn Chances (%)")]
    [Range(0, 100)]
    public float holyCommentChance = 40f;
    
    [Range(0, 100)]
    public float ohoeCommentChance = 30f;
    
    [Range(0, 100)]
    public float trollCommentChance = 20f;
    
    [Range(0, 100)]
    public float superChatCommentChance = 10f;
    
    [Header("Holy Comments")]
    public string[] holyCommentTexts = {
        "マールちゃん可愛い！",
        "歌声に癒された",
        "今日もお疲れさま",
        "マールちゃん大好き",
        "癒しの時間です",
        "天使のような声",
        "今日も元気もらいました",
        "マールちゃんの笑顔最高"
    };
    
    [Header("Ohoe Comments")]
    public string[] ohoeCommentTexts = {
        "CV:おおえのたかゆき",
        "中の人バレてるよ",
        "本人降臨",
        "おおえのたかゆき",
        "声優さんお疲れ様",
        "中の人も頑張って",
        "おおえさん今日も",
        "演技うまいですね"
    };
    
    [Header("Troll Comments")]
    public string[] trollCommentTexts = {
        "つまらない",
        "やめろ",
        "下手くそ",
        "もう見ない",
        "最悪",
        "うざい",
        "才能ない",
        "時間の無駄"
    };
    
    [Header("SuperChat Comments")]
    public string[] superChatTexts = {
        "￥100 マールちゃん頑張って！",
        "￥500 いつも癒されてます",
        "￥1000 マール・アストレア最高！",
        "￥5000 応援してます！",
        "￥100 今日も可愛い",
        "￥500 歌声素敵です",
        "￥1000 いつもありがとう",
        "￥5000 マールちゃん愛してる"
    };
    
    private void OnValidate()
    {
        float total = holyCommentChance + ohoeCommentChance + trollCommentChance + superChatCommentChance;
        
        if (total != 100f)
        {
            Debug.LogWarning($"Comment distribution total is {total}%, should be 100%");
        }
    }
    
    public CommentType GetRandomCommentType()
    {
        float randomValue = Random.Range(0f, 100f);
        float cumulativeChance = 0f;
        
        cumulativeChance += holyCommentChance;
        if (randomValue <= cumulativeChance)
            return CommentType.Holy;
        
        cumulativeChance += ohoeCommentChance;
        if (randomValue <= cumulativeChance)
            return CommentType.Ohoe;
        
        cumulativeChance += trollCommentChance;
        if (randomValue <= cumulativeChance)
            return CommentType.Troll;
        
        return CommentType.SuperChat;
    }
    
    public string GetRandomCommentText(CommentType type)
    {
        switch (type)
        {
            case CommentType.Holy:
                return GetRandomFromArray(holyCommentTexts);
            case CommentType.Ohoe:
                return GetRandomFromArray(ohoeCommentTexts);
            case CommentType.Troll:
                return GetRandomFromArray(trollCommentTexts);
            case CommentType.SuperChat:
                return GetRandomFromArray(superChatTexts);
            default:
                return "デフォルトコメント";
        }
    }
    
    private string GetRandomFromArray(string[] array)
    {
        if (array == null || array.Length == 0) return "コメント";
        return array[Random.Range(0, array.Length)];
    }
}