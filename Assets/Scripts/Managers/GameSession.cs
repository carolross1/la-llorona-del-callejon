// Assets/Scripts/Managers/GameSession.cs
using UnityEngine;

/// <summary>
/// Mantiene los datos persistentes del jugador (vidas, puntaje acumulado)
/// a lo largo de reinicios de escena y transiciones entre niveles.
/// </summary>
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Configuración de Vidas")]
    [SerializeField] int maxLives = 3;
    [SerializeField] int currentLives = 3;

    [Header("Puntaje")]
    [SerializeField] int totalScore = 0;

    public int MaxLives => maxLives;
    public int CurrentLives => currentLives;
    public int TotalScore => totalScore;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Restablece las vidas y el puntaje para una partida nueva.
    /// </summary>
    public void ResetSession()
    {
        currentLives = maxLives;
        totalScore = 0;
    }

    /// <summary>
    /// Resta una vida y devuelve las vidas restantes.
    /// </summary>
    public int LoseLife()
    {
        currentLives = Mathf.Max(0, currentLives - 1);
        return currentLives;
    }

    /// <summary>
    /// Suma puntos al total de la sesión.
    /// </summary>
    public void AddScore(int points)
    {
        if (points > 0)
            totalScore += points;
    }

    /// <summary>
    /// Permite asignar las vidas restantes (por ejemplo al reiniciar o reintentar).
    /// </summary>
    public void SetLives(int lives)
    {
        currentLives = Mathf.Clamp(lives, 0, maxLives);
    }
}
