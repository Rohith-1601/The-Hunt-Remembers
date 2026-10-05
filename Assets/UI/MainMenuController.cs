using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject guidePanel;

    [Header("Game Scene")]
    [SerializeField] private string gameSceneName = "MainGame";

    private void Start()
    {
        Time.timeScale = 1f;

        ShowMainMenu();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        Debug.Log("START GAME");

        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError(
                "MainMenuController: Game Scene Name is empty.",
                this
            );

            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenGuide()
    {
        Debug.Log("OPEN GUIDE");

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError(
                "MainMenuController: Main Menu Panel is not assigned.",
                this
            );
        }

        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "MainMenuController: Guide Panel is not assigned.",
                this
            );
        }
    }

    public void CloseGuide()
    {
        Debug.Log("CLOSE GUIDE");

        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Debug.Log("QUIT GAME");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        Debug.Log(
            "Quit requested. Close the browser tab to exit."
        );
#else
        Application.Quit();
#endif
    }

    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }
    }
}