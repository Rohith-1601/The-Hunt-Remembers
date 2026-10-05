using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject winPanel;

    private void Awake()
    {
        if (winPanel == null)
        {
            Debug.LogError(
                "WinUI: Win Panel is not assigned.",
                this
            );

            enabled = false;
            return;
        }

        winPanel.SetActive(false);
    }

    private void OnEnable()
    {
        ObjectiveManager.OnGameWon += ShowWinPanel;
    }

    private void OnDisable()
    {
        ObjectiveManager.OnGameWon -= ShowWinPanel;
    }

    private void ShowWinPanel()
    {
        winPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Win Panel displayed.");
    }

    // =========================================================
    // RESTART GAME
    // =========================================================

    public void RestartGame()
    {
        // Make sure the game is running normally.
        Time.timeScale = 1f;

        // Reload the current scene.
        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }
}