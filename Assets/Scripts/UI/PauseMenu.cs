// Assets/Scripts/UI/PauseMenu.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Menú modal de pausa. Se activa con la tecla Escape, P o el botón táctil de Pausa [||] y detiene el juego con Time.timeScale = 0.
/// Ofrece: Reanudar, Reiniciar Nivel, Controles, Opciones (con barras interactivas de audio y controles táctiles) y Menú Principal.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject optionsSubPanel;
    [SerializeField] GameObject controlsSubPanel;

    [Header("Botones")]
    [SerializeField] Button resumeButton;
    [SerializeField] Button restartButton;
    [SerializeField] Button controlsButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button menuButton;

    GameObject mainPauseBox;

    void Start()
    {
        SetupColonialPauseAndSubPanels();

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumeClicked);
        }
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        if (controlsButton != null)
        {
            controlsButton.onClick.RemoveAllListeners();
            controlsButton.onClick.AddListener(OnControlsClicked);
        }
        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveAllListeners();
            optionsButton.onClick.AddListener(OnOptionsClicked);
        }
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OnMenuClicked);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;

        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (optionsSubPanel != null)
            optionsSubPanel.SetActive(false);
        if (controlsSubPanel != null)
            controlsSubPanel.SetActive(false);
    }

    void SetupColonialPauseAndSubPanels()
    {
        if (pausePanel == null)
        {
            Transform ov = transform.Find("PauseOverlay");
            if (ov != null) pausePanel = ov.gameObject;
        }
        if (pausePanel == null) return;

        Transform boxTr = pausePanel.transform.Find("PausePanel");
        if (boxTr == null && resumeButton != null)
            boxTr = resumeButton.transform.parent;

        if (boxTr != null)
        {
            mainPauseBox = boxTr.gameObject;
            Font font = ColonialMenuModalHelper.GetMenuFont();
            ColonialMenuModalHelper.StyleColonialModalPanel(mainPauseBox, "PAUSA", new Vector2(540f, 520f), font);

            Vector2 btnSize = new Vector2(380f, 52f);
            resumeButton   = ColonialMenuModalHelper.EnsureColonialButton(mainPauseBox.transform, "Btn_Reanudar",  "Reanudar",        new Vector2(0f,  104f), btnSize, 22, true,  font);
            restartButton  = ColonialMenuModalHelper.EnsureColonialButton(mainPauseBox.transform, "Btn_Reiniciar", "Reiniciar Nivel", new Vector2(0f,   38f), btnSize, 20, false, font);
            controlsButton = ColonialMenuModalHelper.EnsureColonialButton(mainPauseBox.transform, "Btn_Controles", "Controles",       new Vector2(0f,  -28f), btnSize, 20, false, font);
            optionsButton  = ColonialMenuModalHelper.EnsureColonialButton(mainPauseBox.transform, "Btn_Opciones",  "Opciones",        new Vector2(0f,  -94f), btnSize, 20, false, font);
            menuButton     = ColonialMenuModalHelper.EnsureColonialButton(mainPauseBox.transform, "Btn_Menu",      "Menú Principal",  new Vector2(0f, -160f), btnSize, 20, false, font);

            optionsSubPanel = ColonialMenuModalHelper.BuildOptionsSubPanel(
                pausePanel.transform, font,
                onCloseClicked: ShowMainPauseBox,
                onOpenControlsClicked: OnControlsClicked);

            controlsSubPanel = ColonialMenuModalHelper.BuildControlsSubPanel(
                pausePanel.transform, font,
                onCloseClicked: ShowMainPauseBox);
        }
    }

    void ShowMainPauseBox()
    {
        if (optionsSubPanel != null) optionsSubPanel.SetActive(false);
        if (controlsSubPanel != null) controlsSubPanel.SetActive(false);
        if (mainPauseBox != null) mainPauseBox.SetActive(true);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    void Update()
    {
        bool pausePressed = MobileInputManager.PausePressedThisFrame;

        // Detectar tecla Escape (también mapeada al botón Atrás de Android) o tecla P
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame)
            {
                pausePressed = true;
            }
        }

        // Soporte para botón Start / Menu de Gamepad
        var pad = Gamepad.current;
        if (pad != null && pad.startButton.wasPressedThisFrame)
        {
            pausePressed = true;
        }

        if (pausePressed)
        {
            if ((optionsSubPanel != null && optionsSubPanel.activeSelf) ||
                (controlsSubPanel != null && controlsSubPanel.activeSelf))
            {
                ShowMainPauseBox();
            }
            else if (GameManager.Instance != null)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
                GameManager.Instance.TogglePause();
            }
        }
    }

    void HandleStateChanged(GameState state)
    {
        bool isPaused = state == GameState.Paused;
        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        if (isPaused)
        {
            ShowMainPauseBox();
        }
        else
        {
            if (optionsSubPanel != null) optionsSubPanel.SetActive(false);
            if (controlsSubPanel != null) controlsSubPanel.SetActive(false);
        }
    }

    void OnResumeClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    void OnRestartClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        Time.timeScale = 1f;
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.ReloadCurrentLevel();
    }

    void OnControlsClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        if (mainPauseBox != null) mainPauseBox.SetActive(false);
        if (optionsSubPanel != null) optionsSubPanel.SetActive(false);
        if (controlsSubPanel != null) controlsSubPanel.SetActive(true);
    }

    void OnOptionsClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        if (mainPauseBox != null) mainPauseBox.SetActive(false);
        if (controlsSubPanel != null) controlsSubPanel.SetActive(false);
        if (optionsSubPanel != null) optionsSubPanel.SetActive(true);
    }

    void OnMenuClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }
}
