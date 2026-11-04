using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trigger de meta para completar el nivel.
/// </summary>
public class LevelGoal : MonoBehaviour
{
    bool isCompleted = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        TryComplete(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryComplete(other);
    }

    void TryComplete(Collider2D other)
    {
        if (isCompleted) return;

        bool isPlayer = other.CompareTag("Player") ||
                        (other.transform.parent != null && other.transform.parent.CompareTag("Player")) ||
                        other.GetComponentInParent<PlayerMovement>() != null;

        if (!isPlayer) return;

        isCompleted = true;
        Debug.Log("[LevelGoal] ¡Carlos alcanzó la meta del nivel!");

        // Asegurar que el cursor esté visible para la pantalla de victoria
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelCompleted();
        }

        var ui = Object.FindAnyObjectByType<LevelCompletedUI>(FindObjectsInactive.Include);
        if (ui != null)
        {
            ui.gameObject.SetActive(true);
            ui.Show();
        }
        else
        {
            Debug.LogWarning("[LevelGoal] LevelCompletedUI no encontrada en la escena, avanzando automáticamente al siguiente nivel.");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NextLevel();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }
}
