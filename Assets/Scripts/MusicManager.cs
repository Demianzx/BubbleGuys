using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Tracks")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip normalLevelsMusic;
    [SerializeField] private AudioClip specialLevelsMusic;

    [Header("Settings")]
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private int specialLevelThreshold = 10;

    private AudioSource[] audioSources;
    private int activeSource = 0;
    private bool isFading = false;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void SetupAudioSources()
    {
        // Crear dos AudioSources para crossfading
        audioSources = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].loop = true;
            audioSources[i].playOnAwake = false;
            audioSources[i].volume = 0;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Determinar qué música reproducir basado en la escena
        if (scene.buildIndex == 0) // Menu principal
        {
            PlayMusic(menuMusic);
        }
        else // Niveles
        {
            if (scene.buildIndex >= specialLevelThreshold)
            {
                PlayMusic(specialLevelsMusic);
            }
            else
            {
                PlayMusic(normalLevelsMusic);
            }
        }
    }

    public void PlayMusic(AudioClip newClip)
    {
        // Si ya está sonando esta música, no hacer nada
        if (audioSources[activeSource].clip == newClip && audioSources[activeSource].isPlaying)
            return;

        // Si hay un fade en progreso, pararlo
        if (isFading)
        {
            StopAllCoroutines();
            isFading = false;
        }

        StartCoroutine(CrossfadeMusic(newClip));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        isFading = true;
        int newSource = 1 - activeSource; // Alternar entre 0 y 1

        // Configurar la nueva fuente de audio
        audioSources[newSource].clip = newClip;
        audioSources[newSource].volume = 0;
        audioSources[newSource].Play();

        // Realizar el crossfade
        float timer = 0;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeTime;

            audioSources[activeSource].volume = musicVolume * (1 - t);
            audioSources[newSource].volume = musicVolume * t;

            yield return null;
        }

        // Asegurarse de que los volúmenes estén correctos
        audioSources[activeSource].Stop();
        audioSources[activeSource].volume = 0;
        audioSources[newSource].volume = musicVolume;

        // Cambiar la fuente activa
        activeSource = newSource;
        isFading = false;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        audioSources[activeSource].volume = volume;
    }

    public void StopMusic()
    {
        foreach (var source in audioSources)
        {
            source.Stop();
            source.volume = 0;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}