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
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
