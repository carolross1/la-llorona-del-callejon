// Assets/Scripts/UI/HUD.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la visualización de datos en pantalla durante el juego:
/// 3 corazones de vida, cronómetro, puntaje, objetivo y mensajes de tutorial contextual.
/// </summary>
public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }

    [Header("Corazones de Vida")]
    [SerializeField] Image[] heartImages;
    [SerializeField] Color activeHeartColor = Color.red;
    [SerializeField] Color lostHeartColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Header("Indicadores de Texto")]
    [SerializeField] Text timerText;
    [SerializeField] Text scoreText;
    [SerializeField] Text objectiveText;

    [Header("Inventario de Velas / Ofrendas")]
    [SerializeField] GameObject candleInventoryRoot;
    [SerializeField] Text candleCountText;

    [Header("Tutorial Contextual")]
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] Text tutorialText;
    [SerializeField] CanvasGroup tutorialCanvasGroup;

    Coroutine tutorialCoroutine;

    void Awake()
    {
        Instance = this;
        if (tutorialPanel != null && tutorialCanvasGroup == null)
            tutorialCanvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
    }

    void Start()
    {
        Candle.OnCandlesCountChanged += UpdateCandleCount;
        UpdateCandleCount(Candle.PlayerCandlesCount);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnTimeChanged += UpdateTime;
            GameManager.Instance.OnScoreChanged += UpdateScore;

            // Inicializar valores
            int currentLives = GameSession.Instance != null ? GameSession.Instance.CurrentLives : 3;
            UpdateLives(currentLives);
            UpdateTime(GameManager.Instance.LevelTimer);
            UpdateScore(GameManager.Instance.LevelScore);
        }

        if (objectiveText != null)
            objectiveText.text = "Objetivo: Cruza el callejón sin que La Llorona te vea";
    }

    void OnDestroy()
    {
        Candle.OnCandlesCountChanged -= UpdateCandleCount;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnTimeChanged -= UpdateTime;
            GameManager.Instance.OnScoreChanged -= UpdateScore;
        }
    }

    public void UpdateCandleCount(int count)
    {
        if (candleInventoryRoot != null)
            candleInventoryRoot.SetActive(count > 0);
        if (candleCountText != null)
            candleCountText.text = $"Velas: {count} [F: Colocar]";
    }

    public void UpdateLives(int lives)
    {
        if (heartImages == null) return;
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
                heartImages[i].color = (i < lives) ? activeHeartColor : lostHeartColor;
        }
    }

    public void UpdateTime(float seconds)
    {
        if (timerText == null) return;
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);
        timerText.text = $"Tiempo: {min:00}:{sec:00}";
    }

    public void UpdateScore(int score)
    {
        if (scoreText == null) return;
        scoreText.text = $"Puntos: {score}";
    }

    public void SetObjective(string text)
    {
        if (objectiveText != null)
            objectiveText.text = $"Objetivo: {text}";
    }

    /// <summary>
    /// Muestra un mensaje de tutorial contextual durante un número de segundos.
    /// </summary>
    public void ShowTutorial(string message, float duration = 4.5f)
    {
        if (tutorialCoroutine != null)
            StopCoroutine(tutorialCoroutine);

        tutorialCoroutine = StartCoroutine(TutorialRoutine(message, duration));
    }

    IEnumerator TutorialRoutine(string message, float duration)
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        if (tutorialText != null) tutorialText.text = message;

        if (tutorialCanvasGroup != null)
        {
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                tutorialCanvasGroup.alpha = Mathf.Clamp01(t / 0.3f);
                yield return null;
            }
            tutorialCanvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(duration);

            t = 0.3f;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                tutorialCanvasGroup.alpha = Mathf.Clamp01(t / 0.3f);
                yield return null;
            }
            tutorialCanvasGroup.alpha = 0f;
        }
        else
        {
            yield return new WaitForSecondsRealtime(duration);
        }

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }
}
