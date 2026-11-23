// Assets/Scripts/UI/MainMenuUI.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador del Menú Principal.
/// Gestiona navegación entre Jugar, Continuar, Selección de Nivel, Opciones (con controles de volumen y pantalla),
/// Controles del Juego, Créditos y Salir. Oculta el botón Salir automáticamente en compilaciones WebGL.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Botones Principales")]
    [SerializeField] Button playButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button levelSelectButton;
    [SerializeField] Button controlsButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button exitButton;

    [Header("Panel de Opciones y Controles")]
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject controlsPanel;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Dropdown qualityDropdown;
    [SerializeField] Button closeOptionsButton;

    void Start()
    {
        // Iniciar música del menú (Intro.mp3)
        EnsureAudioManager();

        // Construir o actualizar modales de Opciones y Controles con barras funcionales
        SetupOptionsAndControlsModals();

        // Enlazar botones principales
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
        if (controlsButton != null) controlsButton.onClick.AddListener(OnControlsClicked);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
        if (creditsButton != null) creditsButton.onClick.AddListener(OnCreditsClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        // Desactivar Salir en WebGL
#if UNITY_WEBGL
        if (exitButton != null)
            exitButton.gameObject.SetActive(false);
#endif
    }

    void SetupOptionsAndControlsModals()
    {
        Font font = ColonialMenuModalHelper.GetMenuFont();

        // Si había un PanelOpciones antiguo en la escena, ocultarlo y renombrarlo antes de destruir
        if (optionsPanel != null)
        {
            optionsPanel.name = "PanelOpciones_Old";
            optionsPanel.SetActive(false);
            Destroy(optionsPanel);
            optionsPanel = null;
        }

        optionsPanel = ColonialMenuModalHelper.BuildOptionsSubPanel(
            transform, font,
            onCloseClicked: OnCloseOptionsClicked,
            onOpenControlsClicked: OnControlsClicked);

        controlsPanel = ColonialMenuModalHelper.BuildControlsSubPanel(
            transform, font,
            onCloseClicked: OnCloseControlsClicked);

        // Si no existe botón directo de Controles en la botonera principal, agregarlo debajo de Opciones
        if (controlsButton == null && optionsButton != null && optionsButton.transform.parent != null)
        {
            Transform btnParent = optionsButton.transform.parent;
            Transform existingCtrl = btnParent.Find("Btn_Controles");
            if (existingCtrl != null)
            {
                controlsButton = existingCtrl.GetComponent<Button>();
            }
            else
            {
                GameObject ctrlBtnGo = Instantiate(optionsButton.gameObject, btnParent);
                ctrlBtnGo.name = "Btn_Controles";
                controlsButton = ctrlBtnGo.GetComponent<Button>();
                if (controlsButton != null)
                {
                    controlsButton.onClick = new Button.ButtonClickedEvent();
                }

                Text txt = ctrlBtnGo.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.text = "CONTROLES";
                }

                // Acomodar limpiamente la columna de botones para incluir CONTROLES sin empalmes
                ArrangeMainMenuButtons();
            }
        }
    }

    void ArrangeMainMenuButtons()
    {
        Button[] ordered = new Button[]
        {
            playButton,
            levelSelectButton,
            optionsButton,
            controlsButton,
            creditsButton,
            exitButton
        };

        // Detectar si los botones están centrados alrededor de Y=0 (como en MainMenu.unity actual)
        if (playButton == null || exitButton == null) return;
        RectTransform rtPlay = playButton.GetComponent<RectTransform>();
        RectTransform rtExit = exitButton.GetComponent<RectTransform>();
        if (rtPlay == null || rtExit == null) return;

        float topY = rtPlay.anchoredPosition.y;
        float bottomY = rtExit.anchoredPosition.y;

        // Expandir ligeramente el rango vertical para que quepan 6 botones con buen aire
        float center = (topY + bottomY) * 0.5f;
        float step = 50f;
        float startY = center + 2.5f * step;

        for (int i = 0; i < ordered.Length; i++)
        {
            if (ordered[i] == null) continue;
            RectTransform rt = ordered[i].GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startY - i * step);
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 44f);
            }
        }
    }

    void OnPlayClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        // Iniciar partida nueva: nivel 1 con 3 vidas
        if (FindAnyObjectByType<GameSession>() != null)
            FindAnyObjectByType<GameSession>().ResetSession();

        LoadSceneNamed("Level1_Mercado");
    }

    void OnContinueClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        int unlocked = SaveSystem.GetUnlockedLevel();
        string sceneName = GetSceneNameForLevel(unlocked);
        LoadSceneNamed(sceneName);
    }

    void OnLevelSelectClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        LoadSceneNamed("LevelSelect");
    }

    void OnControlsClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    void OnCloseControlsClicked()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    void OnOptionsClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    void OnCloseOptionsClicked()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    void OnCreditsClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        LoadSceneNamed("Credits");
    }

    void OnExitClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
#if !UNITY_WEBGL
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
#endif
    }

    void OnMusicVolumeChanged(float val)
    {
        SaveSystem.SetMusicVolume(val);
    }

    void OnSfxVolumeChanged(float val)
    {
        SaveSystem.SetSfxVolume(val);
    }

    void OnQualityChanged(int index)
    {
        SaveSystem.SetQualityLevel(index);
    }

    void LoadSceneNamed(string sName)
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(sName);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(sName);
    }

    void EnsureAudioManager()
    {
        if (AudioManager.Instance == null && FindAnyObjectByType<AudioManager>() == null)
        {
            GameObject audioGo = new GameObject("AudioManager");
            audioGo.AddComponent<AudioManager>();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
    }

    string GetSceneNameForLevel(int level)
    {
        switch (level)
        {
            case 1: return "Level1_Mercado";
            case 2: return "Level2_Callejon";
            case 3: return "Level3_Atrio";
            case 4: return "Level4_Cementerio";
            default: return "Level1_Mercado";
        }
    }
}
