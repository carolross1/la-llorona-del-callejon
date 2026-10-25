// Assets/Scripts/UI/MainMenuUI.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador del Menú Principal.
/// Gestiona navegación entre Jugar, Continuar, Selección de Nivel, Opciones, Créditos y Salir.
/// Oculta el botón Salir automáticamente en compilaciones WebGL.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Botones Principales")]
    [SerializeField] Button playButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button levelSelectButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button exitButton;

    [Header("Panel de Opciones")]
    [SerializeField] GameObject optionsPanel;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Dropdown qualityDropdown;
    [SerializeField] Button closeOptionsButton;

    void Start()
    {
        // Enlazar botones principales
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
        if (creditsButton != null) creditsButton.onClick.AddListener(OnCreditsClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        // Desactivar Salir en WebGL
#if UNITY_WEBGL
        if (exitButton != null)
            exitButton.gameObject.SetActive(false);
#endif

        // Configuración del panel de opciones
        if (closeOptionsButton != null) closeOptionsButton.onClick.AddListener(OnCloseOptionsClicked);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        // Cargar valores guardados en controles de opciones
        if (musicSlider != null)
        {
            musicSlider.value = SaveSystem.GetMusicVolume();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = SaveSystem.GetSfxVolume();
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            var names = new System.Collections.Generic.List<string>(QualitySettings.names);
            qualityDropdown.AddOptions(names);
            qualityDropdown.value = SaveSystem.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }
    }

    void OnPlayClicked()
    {
        // Iniciar partida nueva: nivel 1 con 3 vidas
        if (FindAnyObjectByType<GameSession>() != null)
            FindAnyObjectByType<GameSession>().ResetSession();

        LoadSceneNamed("Level1_Mercado");
    }

    void OnContinueClicked()
    {
        int unlocked = SaveSystem.GetUnlockedLevel();
        string sceneName = GetSceneNameForLevel(unlocked);
        LoadSceneNamed(sceneName);
    }

    void OnLevelSelectClicked()
    {
        LoadSceneNamed("LevelSelect");
    }

    void OnOptionsClicked()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    void OnCloseOptionsClicked()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    void OnCreditsClicked()
    {
        LoadSceneNamed("Credits");
    }

    void OnExitClicked()
    {
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
