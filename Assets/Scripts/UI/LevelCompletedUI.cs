// Assets/Scripts/UI/LevelCompletedUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Pantalla de victoria que se despliega al alcanzar la meta del nivel.
/// Muestra tiempo transcurrido, puntaje obtenido y botones para avanzar o volver.
/// Si es el Nivel 4 (Final del juego), adapta el diseño para anunciar el clímax final y dirigir a Créditos.
/// </summary>
public class LevelCompletedUI : MonoBehaviour
{
    [Header("Panel Principal")]
    [SerializeField] GameObject panel;

    [Header("Textos de Resultados")]
    [SerializeField] Text titleText;
    [SerializeField] Text timeResultText;
    [SerializeField] Text scoreResultText;
    [SerializeField] Text highScoreText;

    [Header("Botones")]
    [SerializeField] Button nextLevelButton;
    [SerializeField] Button menuButton;

    Text narrativeFinalText;

    void Start()
    {
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;

        if (panel != null)
            panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    void Update()
    {
        if (panel != null && panel.activeInHierarchy)
        {
            // Atajos de teclado para continuar cómodamente usando exclusivamente el nuevo Input System
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame ||
                    kb.numpadEnterKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
                {
                    OnNextLevelClicked();
                    return;
                }
                if (kb.escapeKey.wasPressedThisFrame)
                {
                    OnMenuClicked();
                    return;
                }
            }
        }
    }

    void HandleStateChanged(GameState state)
    {
        if (state == GameState.LevelCompleted)
        {
            Show();
        }
        else
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    /// <summary>
    /// Determina si el nivel actual corresponde al enfrentamiento final (Nivel 4).
    /// </summary>
    bool IsFinalLevel()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentLevelNumber >= 4)
            return true;

        string currentScene = SceneManager.GetActiveScene().name;
        return currentScene.Contains("Level4") || currentScene.Contains("Cementerio");
    }

    /// <summary>
    /// Despliega el panel de victoria con estadísticas actualizadas y ambientación especial si es el final del juego.
    /// </summary>
    public void Show()
    {
        if (panel != null) panel.SetActive(true);

        // Asegurar que el cursor esté visible e interactuable
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        bool isFinal = IsFinalLevel();

        // 1. Configurar título dinámico
        ResolveTitleText(isFinal);

        // 2. Configurar narrativa y botones para el final del juego
        if (isFinal)
        {
            SetupGameCompletionTheme();
        }

        // 3. Estadísticas de la partida
        if (GameManager.Instance != null)
        {
            float t = GameManager.Instance.LevelTimer;
            int min = Mathf.FloorToInt(t / 60f);
            int sec = Mathf.FloorToInt(t % 60f);
            if (timeResultText != null)
                timeResultText.text = $"Tiempo: {min:00}:{sec:00}";

            int score = GameManager.Instance.LevelScore;
            if (scoreResultText != null)
                scoreResultText.text = $"Puntos Obtenidos: {score}";

            int high = SaveSystem.GetHighScore(GameManager.Instance.CurrentLevelNumber);
            if (highScoreText != null)
                highScoreText.text = $"Récord: {high}";
        }
    }

    void ResolveTitleText(bool isFinal)
    {
        if (titleText == null && panel != null)
        {
            Transform t = panel.transform.Find("VictoryPanel/TituloModal");
            if (t == null) t = panel.transform.Find("Card/TituloModal");
            if (t == null) t = panel.transform.Find("TituloModal");
            if (t == null)
            {
                foreach (var txt in panel.GetComponentsInChildren<Text>(true))
                {
                    if (txt.gameObject.name.Contains("Titulo") || txt.text.Contains("NIVEL") || txt.text.Contains("FINAL"))
                    {
                        titleText = txt;
                        break;
                    }
                }
            }
            else
            {
                titleText = t.GetComponent<Text>();
            }
        }

        if (titleText != null)
        {
            if (isFinal)
            {
                titleText.text = "✦ ¡HAS LLEGADO AL FINAL DEL JUEGO! ✦";
                titleText.color = new Color(1.0f, 0.85f, 0.25f, 1f);
                titleText.fontSize = Mathf.Min(titleText.fontSize, 26);
            }
            else
            {
                titleText.text = "¡NIVEL COMPLETADO!";
            }
        }
    }

    void SetupGameCompletionTheme()
    {
        // Actualizar botón de avanzar para que indique ir a Créditos
        if (nextLevelButton != null)
        {
            Text btnTxt = nextLevelButton.GetComponentInChildren<Text>();
            if (btnTxt != null)
            {
                btnTxt.text = "Ver Créditos ➔";
            }
        }

        // Crear o actualizar texto de narrativa final en la tarjeta
        if (panel != null && narrativeFinalText == null)
        {
            Transform card = panel.transform.Find("VictoryPanel") ?? panel.transform.Find("Card") ?? panel.transform;
            Transform existing = card.Find("TextoNarrativaFinal");
            if (existing != null)
            {
                narrativeFinalText = existing.GetComponent<Text>();
            }
            else
            {
                GameObject storyObj = new GameObject("TextoNarrativaFinal");
                storyObj.transform.SetParent(card, false);

                RectTransform rt = storyObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -80f);
                rt.sizeDelta = new Vector2(500f, 65f);

                narrativeFinalText = storyObj.AddComponent<Text>();
                narrativeFinalText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                narrativeFinalText.fontSize = 15;
                narrativeFinalText.alignment = TextAnchor.MiddleCenter;
                narrativeFinalText.color = new Color(0.95f, 0.90f, 0.75f, 1.0f);
                narrativeFinalText.raycastTarget = false;

                Shadow shadow = storyObj.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
                shadow.effectDistance = new Vector2(1f, -1f);
            }
        }

        if (narrativeFinalText != null)
        {
            narrativeFinalText.text = "¡El tormento de La Llorona ha terminado!\nCarlos ha colocado la ofrenda sagrada en su tumba, liberando su alma en pena para siempre. Guanajuato descansa en paz.";
        }

        // Tocar campana solemne de victoria si existe en AudioManager
        if (AudioManager.Instance != null && AudioManager.Instance.campanaLejana != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.campanaLejana, 1.0f);
        }
    }

    void OnNextLevelClicked()
    {
        if (IsFinalLevel())
        {
            Debug.Log("[LevelCompletedUI] ¡Fin del juego! Cargando escena de Créditos...");
            Time.timeScale = 1f;

            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadScene("Credits");
            else
                SceneManager.LoadScene("Credits");
        }
        else
        {
            Debug.Log("[LevelCompletedUI] Avanzando al siguiente nivel...");
            if (GameManager.Instance != null)
                GameManager.Instance.NextLevel();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    void OnMenuClicked()
    {
        Debug.Log("[LevelCompletedUI] Regresando al menú principal...");
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
        else
            SceneManager.LoadScene("MainMenu");
    }
}
