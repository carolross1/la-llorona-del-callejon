// Assets/Scripts/Managers/GameManager.cs
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Paused,
    LevelCompleted,
    GameOver
}

/// <summary>
/// Controlador principal del ciclo de juego en cada nivel.
/// Administra estados (Jugar, Pausa, Fin de Nivel, Game Over),
/// cronómetro, cálculo de puntaje y comunicación con la UI.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración del Nivel")]
    [SerializeField] int currentLevelNumber = 1;
    [SerializeField] int baseScore = 1000;
    [SerializeField] float timeBonusFactor = 5f;

    [Header("Estado Actual")]
    [SerializeField] GameState currentState = GameState.Playing;
    [SerializeField] float levelTimer = 0f;
    [SerializeField] int levelScore = 0;

    public GameState CurrentState => currentState;
    public float LevelTimer => levelTimer;
    public int LevelScore => levelScore;
    public int CurrentLevelNumber { get => currentLevelNumber; set => currentLevelNumber = value; }
    public void SetLevelNumber(int level) => currentLevelNumber = level;

    // Eventos para actualización reactiva del HUD y menús
    public event Action<int> OnLivesChanged;
    public event Action<float> OnTimeChanged;
    public event Action<int> OnScoreChanged;
    public event Action<GameState> OnStateChanged;

    bool isLevelEnded;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Asegurar que existan los servicios persistentes
        EnsureServices();
    }

    void Start()
    {
        Time.timeScale = 1f;
        SetState(GameState.Playing);

        // Notificar vidas iniciales
        int lives = GameSession.Instance != null ? GameSession.Instance.CurrentLives : 3;
        OnLivesChanged?.Invoke(lives);
        OnScoreChanged?.Invoke(levelScore);

        // Iniciar música ambiental del nivel sin empalmarse con los SFX
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelMusic();
        }
    }

    void Update()
    {
        if (currentState == GameState.Playing)
        {
            levelTimer += Time.deltaTime;
            OnTimeChanged?.Invoke(levelTimer);
        }
    }

    void EnsureServices()
    {
        if (FindAnyObjectByType<GameSession>() == null)
        {
            GameObject sessionGo = new GameObject("GameSession");
            sessionGo.AddComponent<GameSession>();
        }

        if (FindAnyObjectByType<SceneLoader>() == null)
        {
            GameObject loaderGo = new GameObject("SceneLoader");
            loaderGo.AddComponent<SceneLoader>();
        }

        if (AudioManager.Instance == null && FindAnyObjectByType<AudioManager>() == null)
        {
            GameObject audioGo = new GameObject("AudioManager");
            audioGo.AddComponent<AudioManager>();
        }
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(currentState);
    }

    /// <summary>
    /// Alterna entre juego y pausa.
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
            PauseGame();
        else if (currentState == GameState.Paused)
            ResumeGame();
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        Time.timeScale = 1f;
        SetState(GameState.Playing);
    }

    /// <summary>
    /// Invocado cuando La Llorona atrapa a Carlos.
    /// </summary>
    public void OnPlayerCaught()
    {
        if (isLevelEnded) return;

        int livesRemaining = GameSession.Instance != null
            ? GameSession.Instance.LoseLife()
            : 0;

        OnLivesChanged?.Invoke(livesRemaining);

        if (livesRemaining > 0)
        {
            // Aún tiene vidas: reiniciar el nivel actual
            isLevelEnded = true;
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.ReloadCurrentLevel();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            // Derrota definitiva: Game Over
            isLevelEnded = true;
            Time.timeScale = 0f;
            SetState(GameState.GameOver);
        }
    }

    /// <summary>
    /// Invocado cuando Carlos llega a la meta del nivel.
    /// </summary>
    public void OnLevelCompleted()
    {
        if (isLevelEnded) return;
        isLevelEnded = true;

        // Cálculo de puntaje (base + bonificación por rapidez)
        int timePenalty = Mathf.RoundToInt(levelTimer * timeBonusFactor);
        levelScore = Mathf.Max(200, baseScore - timePenalty);

        if (GameSession.Instance != null)
            GameSession.Instance.AddScore(levelScore);

        // Desbloquear el siguiente nivel en SaveSystem
        SaveSystem.UnlockLevel(currentLevelNumber + 1);
        SaveSystem.TrySaveHighScore(currentLevelNumber, levelScore);

        OnScoreChanged?.Invoke(levelScore);
        Time.timeScale = 0f;
        SetState(GameState.LevelCompleted);
    }

    /// <summary>
    /// Reinicia la partida tras un Game Over restableciendo vidas.
    /// </summary>
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        if (GameSession.Instance != null)
            GameSession.Instance.ResetSession();

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.ReloadCurrentLevel();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Regresa al Menú Principal.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("MainMenu");
        else
            SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Carga el siguiente nivel en la progresión.
    /// </summary>
    public void NextLevel()
    {
        Time.timeScale = 1f;
        int nextIndex = currentLevelNumber + 1;
        string nextSceneName = $"Level{nextIndex}_" + (nextIndex == 2 ? "Callejon" : nextIndex == 3 ? "Atrio" : "Cementerio");

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene(nextSceneName);
    }
}
