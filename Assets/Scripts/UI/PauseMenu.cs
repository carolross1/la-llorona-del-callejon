// Assets/Scripts/UI/PauseMenu.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Menú modal de pausa. Se activa con la tecla Escape y detiene el juego con Time.timeScale = 0.
/// Ofrece: Reanudar, Reiniciar Nivel, Opciones y Salir al Menú Principal.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject optionsSubPanel;

    [Header("Botones")]
    [SerializeField] Button resumeButton;
    [SerializeField] Button restartButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button menuButton;

    void Start()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;

        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (optionsSubPanel != null)
            optionsSubPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    void Update()
    {
        // Detectar tecla Escape con protección ante Keyboard nulo (móvil)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (optionsSubPanel != null && optionsSubPanel.activeSelf)
            {
                optionsSubPanel.SetActive(false);
                if (pausePanel != null) pausePanel.SetActive(true);
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
            }
        }
    }

    void HandleStateChanged(GameState state)
    {
        if (pausePanel != null)
            pausePanel.SetActive(state == GameState.Paused);

        if (state != GameState.Paused && optionsSubPanel != null)
            optionsSubPanel.SetActive(false);
    }

    void OnResumeClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    void OnRestartClicked()
    {
        Time.timeScale = 1f;
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.ReloadCurrentLevel();
    }

    void OnOptionsClicked()
    {
        if (optionsSubPanel != null)
            optionsSubPanel.SetActive(!optionsSubPanel.activeSelf);
    }

    void OnMenuClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }
}
