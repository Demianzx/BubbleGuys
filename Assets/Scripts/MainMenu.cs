using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Navigation")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Level Selection")]
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform levelButtonContainer;

    void Start()
    {
        ShowMainMenu();

        if (levelButtonContainer != null && levelButtonPrefab != null)
        {
            GenerateLevelButtons();
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
    }

    private void GenerateLevelButtons()
    {
        // Limpiar botones existentes
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 1; i < sceneCount; i++)
        {
            int levelIndex = i;

            GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            Button button = buttonObj.GetComponent<Button>();

            // Configurar el texto del botón
            TMP_Text levelText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (levelText != null)
            {
                levelText.text = $"Lvl {i}";
            }

            // Configurar el callback del botón
            if (button != null)
            {
                button.onClick.AddListener(() => LoadLevel(levelIndex));
            }
        }
    }

    private void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}