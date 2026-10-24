// Assets/Scripts/Systems/NoiseObstacle.cs
using UnityEngine;

/// <summary>
/// Obstáculo del entorno: Montón de latas y botellas rotas.
/// Si el jugador lo pisa o pasa corriendo encima, genera un estruendo metálico
/// que alerta inmediatamente a La Llorona hacia esa posición.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class NoiseObstacle : MonoBehaviour
{
    [SerializeField] float alertRadius = 16f;
    [SerializeField] float cooldown = 3f;
    float lastTriggerTime = -99f;

    void OnTriggerEnter2D(Collider2D other)
    {
        CheckNoise(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        CheckNoise(other);
    }

    void CheckNoise(Collider2D other)
    {
        if (Time.time < lastTriggerTime + cooldown) return;
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponent<PlayerMovement>();
        if (player == null || player.IsHidden) return;

        lastTriggerTime = Time.time;

        // Mostrar aviso en HUD si está disponible
        if (HUD.Instance != null)
        {
            HUD.Instance.ShowTutorial("¡CLANG! ¡Pisaste latas! La Llorona escuchó el ruido...", 3.0f);
        }

        // Buscar a La Llorona y alertarla
        LloronaAI llorona = Object.FindAnyObjectByType<LloronaAI>();
        if (llorona != null)
        {
            float dist = Vector2.Distance(transform.position, llorona.transform.position);
            if (dist <= alertRadius)
            {
                llorona.AlertNoise(transform.position, 4.5f);
            }
        }
    }
}
