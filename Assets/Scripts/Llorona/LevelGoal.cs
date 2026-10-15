using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Nivel completado!");
            if (GameManager.Instance != null)
                GameManager.Instance.OnLevelCompleted();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}