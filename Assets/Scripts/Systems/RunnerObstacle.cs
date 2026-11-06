// Assets/Scripts/Systems/RunnerObstacle.cs
using UnityEngine;

/// <summary>
/// Componente para obstáculos dinámicos en el modo Runner / Subway Surfers del Nivel 2.
/// Si Carlos salta con [Espacio], esquiva limpiamente los obstáculos bajos (huacales, charcos, botellas).
/// Si choca con ellos sin saltar, tropieza, reduce velocidad momentáneamente y La Llorona se acerca (1 Strike).
/// </summary>
public class RunnerObstacle : MonoBehaviour
{
    [Header("Propiedades del Obstáculo")]
    [Tooltip("True si Carlos puede brincarlo con la barra espaciadora")]
    [SerializeField] bool canBeJumpedOver = true;

    [Tooltip("Cantidad de fallos que suma al chocar (1 a 3)")]
    [SerializeField] int strikeCount = 1;

    [Tooltip("Duración de la ralentización del tropiezo")]
    [SerializeField] float stumbleDuration = 0.45f;

    [Header("Audio")]
    [SerializeField] AudioClip hitSound;
    [Range(0f, 1f)] [SerializeField] float soundVolume = 0.9f;

    bool hasTriggered = false;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        HandleCollision(other);
    }

    void HandleCollision(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponent<PlayerMovement>() ?? other.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        // Si Carlos está saltando en el aire y el obstáculo es bajo, pasa limpio
        if (canBeJumpedOver && player.IsJumping)
        {
            return;
        }

        hasTriggered = true;

        if (hitSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hitSound, soundVolume);
        }

        if (Level2ChaseRunner.Instance != null)
        {
            Level2ChaseRunner.Instance.OnObstacleHit(gameObject.name, strikeCount, stumbleDuration);
        }
    }
}
