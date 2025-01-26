using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class IntroVideoPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private string videoUrl = ""; // URL del video
    [SerializeField] private float skipTime = 7f; // Duraci�n del video

    [Header("References")]
    [SerializeField] private RawImage videoScreen;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private VideoPlayer videoPlayer;

    private float currentTime = 0f;
    private bool hasFinished = false;

    void Start()
    {
        // Ocultar el men� principal inicialmente
        if (mainMenuUI != null)
        {
            mainMenuUI.SetActive(false);
        }

        // Configurar el VideoPlayer
        SetupVideoPlayer();
    }

    void SetupVideoPlayer()
    {
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        // Configurar para reproducir desde URL
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoUrl;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // Si tenemos una RawImage, configurar para mostrar el video ah�
        if (videoScreen != null)
        {
            // Crear una RenderTexture del tama�o de la pantalla
            RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            videoPlayer.targetTexture = renderTexture;
            videoScreen.texture = renderTexture;
        }

        // Eventos para manejar errores y finalizaci�n
        videoPlayer.errorReceived += HandleVideoError;
        videoPlayer.loopPointReached += HandleVideoComplete;

        // Comenzar reproducci�n
        videoPlayer.Prepare();
    }

    void Update()
    {
        if (!hasFinished)
        {
            currentTime += Time.deltaTime;

            // Si se alcanza el tiempo de skip o hay alg�n input
            if (currentTime >= skipTime || Input.anyKeyDown)
            {
                FinishVideo();
            }
        }
    }

    void HandleVideoError(VideoPlayer vp, string errorMessage)
    {
        Debug.LogWarning($"Error en la reproducci�n del video: {errorMessage}");
        FinishVideo();
    }

    void HandleVideoComplete(VideoPlayer vp)
    {
        FinishVideo();
    }

    void FinishVideo()
    {
        if (hasFinished) return;

        hasFinished = true;

        // Detener y limpiar el video
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            if (videoPlayer.targetTexture != null)
            {
                videoPlayer.targetTexture.Release();
            }
        }

        // Ocultar la pantalla de video
        if (videoScreen != null)
        {
            videoScreen.gameObject.SetActive(false);
        }

        // Mostrar el men� principal
        if (mainMenuUI != null)
        {
            mainMenuUI.SetActive(true);
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.errorReceived -= HandleVideoError;
            videoPlayer.loopPointReached -= HandleVideoComplete;
        }
    }
}