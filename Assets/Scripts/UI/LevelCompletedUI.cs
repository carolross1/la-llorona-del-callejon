// Assets/Scripts/UI/LevelCompletedUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Pantalla de victoria que se despliega al alcanzar la meta del nivel.
/// Muestra tiempo transcurrido, puntaje obtenido y botones para avanzar o volver.
/// </summary>
public class LevelCompletedUI : MonoBehaviour
{
    [Header("Panel Principal")]
    [SerializeField] GameObject panel;

    [Header("Textos de Resultados")]
    [SerializeField] Text timeResultText;
    [SerializeField] Text scoreResultText;
    [SerializeField] Text highScoreText;

    [Header("Botones")]
    [SerializeField] Button nextLevelButton;
    [SerializeField] Button menuButton;

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
            if (panel != null) panel.SetActive(true);

            // Asegurar que el cursor esté visible e interactuable
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

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
        else
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    void OnNextLevelClicked()
    {
        Debug.Log("[LevelCompletedUI] Avanzando al siguiente nivel...");
        if (GameManager.Instance != null)
            GameManager.Instance.NextLevel();
    }

    void OnMenuClicked()
    {
        Debug.Log("[LevelCompletedUI] Regresando al menú principal...");
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }
}
