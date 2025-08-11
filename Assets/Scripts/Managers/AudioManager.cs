using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioSource voiceSource;
    
    [Header("Audio Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float bgmVolume = 0.7f;
    [SerializeField] private float seVolume = 1f;
    [SerializeField] private float voiceVolume = 1f;
    
    [Header("Audio Pool")]
    [SerializeField] private int audioSourcePoolSize = 10;
    
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAudioManager()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }
        
        if (seSource == null)
        {
            seSource = gameObject.AddComponent<AudioSource>();
            seSource.loop = false;
            seSource.playOnAwake = false;
        }
        
        if (voiceSource == null)
        {
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.loop = false;
            voiceSource.playOnAwake = false;
        }
        
        CreateAudioSourcePool();
        UpdateVolumes();
    }
    
    private void CreateAudioSourcePool()
    {
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            AudioSource pooledSource = gameObject.AddComponent<AudioSource>();
            pooledSource.playOnAwake = false;
            pooledSource.loop = false;
            audioSourcePool.Enqueue(pooledSource);
        }
    }
    
    private void UpdateVolumes()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = masterVolume * bgmVolume;
        }
        
        if (seSource != null)
        {
            seSource.volume = masterVolume * seVolume;
        }
        
        if (voiceSource != null)
        {
            voiceSource.volume = masterVolume * voiceVolume;
        }
    }
    
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;
        
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        
        bgmSource.clip = clip;
        bgmSource.Play();
    }
    
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }
    
    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }
    
    public void ResumeBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying && bgmSource.clip != null)
        {
            bgmSource.UnPause();
        }
    }
    
    public void PlaySE(AudioClip clip)
    {
        if (seSource == null || clip == null) return;
        
        seSource.PlayOneShot(clip);
    }
    
    public void PlaySE(AudioClip clip, float volume)
    {
        if (seSource == null || clip == null) return;
        
        seSource.PlayOneShot(clip, volume * masterVolume * seVolume);
    }
    
    public void PlayVoice(AudioClip clip)
    {
        if (voiceSource == null || clip == null) return;
        
        voiceSource.clip = clip;
        voiceSource.Play();
    }
    
    public void StopVoice()
    {
        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Stop();
        }
    }
    
    public AudioSource PlaySEWithReturn(AudioClip clip)
    {
        if (clip == null) return null;
        
        AudioSource source = GetPooledAudioSource();
        if (source == null) return null;
        
        source.clip = clip;
        source.volume = masterVolume * seVolume;
        source.Play();
        
        activeAudioSources.Add(source);
        StartCoroutine(ReturnToPoolWhenFinished(source));
        
        return source;
    }
    
    private AudioSource GetPooledAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            return audioSourcePool.Dequeue();
        }
        
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.loop = false;
        return newSource;
    }
    
    private System.Collections.IEnumerator ReturnToPoolWhenFinished(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        
        activeAudioSources.Remove(source);
        source.clip = null;
        audioSourcePool.Enqueue(source);
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public float GetMasterVolume()
    {
        return masterVolume;
    }
    
    public float GetBGMVolume()
    {
        return bgmVolume;
    }
    
    public float GetSEVolume()
    {
        return seVolume;
    }
    
    public float GetVoiceVolume()
    {
        return voiceVolume;
    }
    
    public bool IsBGMPlaying()
    {
        return bgmSource != null && bgmSource.isPlaying;
    }
    
    public bool IsVoicePlaying()
    {
        return voiceSource != null && voiceSource.isPlaying;
    }
    
    public void StopAllSE()
    {
        if (seSource != null)
        {
            seSource.Stop();
        }
        
        foreach (AudioSource source in activeAudioSources)
        {
            if (source != null)
            {
                source.Stop();
            }
        }
    }
    
    public void StopAllAudio()
    {
        StopBGM();
        StopAllSE();
        StopVoice();
    }
    
    public void PauseAllAudio()
    {
        PauseBGM();
        
        foreach (AudioSource source in activeAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Pause();
            }
        }
    }
    
    public void ResumeAllAudio()
    {
        ResumeBGM();
        
        foreach (AudioSource source in activeAudioSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }
    }
    
    private void OnDestroy()
    {
        StopAllAudio();
    }
}