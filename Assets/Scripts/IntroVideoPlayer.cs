using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class IntroVideoPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoClip introVideo; // Video local para Windows
    [SerializeField] private float skipTime = 7f;

    [Header("References")]
    [SerializeField] private RawImage videoScreen;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private VideoPlayer videoPlayer;

    private float currentTime = 0f;
    private bool hasFinished = false;

    void Start()
    {
        // Si estamos en WebGL, saltamos directamente al menú
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            SkipToMainMenu();
            return;
        }

        // Ocultar el menú principal inicialmente
        if (mainMenuUI != null)
        {
            mainMenuUI.SetActive(false);
        }

        // Configurar el VideoPlayer para Windows
        SetupVideoPlayer();
    }

    void SetupVideoPlayer()
    {
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        // Configurar para reproducir el clip local
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = introVideo;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        if (videoScreen != null)
        {
            RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            videoPlayer.targetTexture = renderTexture;
            videoScreen.texture = renderTexture;
        }

        videoPlayer.errorReceived += HandleVideoError;
        videoPlayer.loopPointReached += HandleVideoComplete;
        videoPlayer.Prepare();
    }

    void Update()
    {
        if (!hasFinished && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            currentTime += Time.deltaTime;

            // Permitir saltar el video con cualquier tecla
            if (currentTime >= skipTime || Input.anyKeyDown)
            {
                FinishVideo();
            }
        }
    }

    void HandleVideoError(VideoPlayer vp, string errorMessage)
    {
        Debug.LogWarning($"Error en la reproducción del video: {errorMessage}");
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

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            if (videoPlayer.targetTexture != null)
            {
                videoPlayer.targetTexture.Release();
            }
        }

        if (videoScreen != null)
        {
            videoScreen.gameObject.SetActive(false);
        }

        ShowMainMenu();
    }

    void SkipToMainMenu()
    {
        // Deshabilitar componentes innecesarios en WebGL
        if (videoScreen != null)
        {
            videoScreen.gameObject.SetActive(false);
        }
        if (videoPlayer != null)
        {
            videoPlayer.enabled = false;
        }

        ShowMainMenu();
    }

    void ShowMainMenu()
    {
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