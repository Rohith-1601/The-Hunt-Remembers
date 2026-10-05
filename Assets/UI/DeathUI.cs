using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject deathPanel;

    private void Awake()
    {
        if (deathPanel == null)
        {
            Debug.LogError(
                "DeathUI: Death Panel is not assigned.",
                this
            );

            enabled = false;
            return;
        }

        deathPanel.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDied += ShowDeathPanel;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDied -= ShowDeathPanel;
    }

    private void ShowDeathPanel()
    {
        deathPanel.SetActive(true);

        // Freeze the game while the death screen is open.
        Time.timeScale = 0f;

        // Release the mouse so the button can be clicked.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Death Panel displayed.");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }
}