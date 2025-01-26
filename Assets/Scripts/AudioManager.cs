using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // Singleton instance
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
    }

    [Header("Sound Settings")]
    [SerializeField] private Sound[] sounds;
    [SerializeField] private int audioSourcePoolSize = 10;

    private Queue<AudioSource> audioSourcePool;
    private Dictionary<string, Sound> soundDictionary;

    void Awake()
    {
        // Singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioPool();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeAudioPool()
    {
        // Initialize the sound dictionary
        soundDictionary = new Dictionary<string, Sound>();
        foreach (Sound sound in sounds)
        {
            soundDictionary[sound.name] = sound;
        }

        // Create audio source pool
        audioSourcePool = new Queue<AudioSource>();
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSourcePool.Enqueue(audioSource);
        }
    }

    public void PlaySound(string soundName)
    {
        if (!soundDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound {soundName} not found in AudioManager!");
            return;
        }

        Sound sound = soundDictionary[soundName];
        AudioSource audioSource = GetAvailableAudioSource();

        if (audioSource != null)
        {
            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume;
            audioSource.pitch = sound.pitch;
            audioSource.Play();
            StartCoroutine(ReturnToPoolWhenFinished(audioSource));
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        // First, try to get from pool
        if (audioSourcePool.Count > 0)
        {
            return audioSourcePool.Dequeue();
        }

        // If pool is empty, find a non-playing source
        AudioSource[] allSources = GetComponents<AudioSource>();
        foreach (AudioSource source in allSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        Debug.LogWarning("No available audio sources! Consider increasing pool size.");
        return null;
    }

    private System.Collections.IEnumerator ReturnToPoolWhenFinished(AudioSource audioSource)
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        audioSourcePool.Enqueue(audioSource);
    }

    public void StopAllSounds()
    {
        AudioSource[] allSources = GetComponents<AudioSource>();
        foreach (AudioSource source in allSources)
        {
            source.Stop();
        }
    }

    public void SetGlobalVolume(float volume)
    {
        AudioSource[] allSources = GetComponents<AudioSource>();
        foreach (AudioSource source in allSources)
        {
            source.volume = volume;
        }
    }
}