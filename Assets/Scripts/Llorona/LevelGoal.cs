using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Nivel completado!");
            // Temporal: cuando tengas Nivel 2, cambia esto por LoadScene del siguiente
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}