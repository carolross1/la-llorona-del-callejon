// Assets/Scripts/UI/GameOverUI.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pantalla de derrota desplegada cuando Carlos pierde sus 3 vidas.
/// Permite reintentar la partida reiniciando vidas o volver al Menú Principal.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Panel Principal")]
    [SerializeField] GameObject panel;

    [Header("Botones")]
    [SerializeField] Button retryButton;
    [SerializeField] Button menuButton;

    void Start()
    {
        if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
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

    void HandleStateChanged(GameState state)
    {
        if (panel != null)
            panel.SetActive(state == GameState.GameOver);
    }

    void OnRetryClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RetryLevel();
    }

    void OnMenuClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }
}
