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

    [Tooltip("True si este obstáculo es un espectro (aplica frío espectral severo)")]
    [SerializeField] bool isSpecter = false;

    [Tooltip("Multiplicador de velocidad al tocar el espectro (ej. 0.45 = 45% de velocidad)")]
    [SerializeField] float slowMultiplier = 0.45f;

    [Tooltip("Duración de la ralentización del tropiezo o frío")]
    [SerializeField] float stumbleDuration = 2.0f;

    [Header("Audio")]
    [SerializeField] AudioClip hitSound;
    [Range(0f, 1f)] [SerializeField] float soundVolume = 0.9f;

    bool hasTriggered = false;
    PlayerMovement cachedPlayer;

    void Awake()
    {
        if (gameObject.name.ToLower().Contains("espectro"))
        {
            isSpecter = true;
            canBeJumpedOver = false;
            slowMultiplier = 0.45f;
            stumbleDuration = 2.0f;

            // BoxCollider amplio que abarca desde la altura del espectro hasta el suelo
            var bc = GetComponent<BoxCollider2D>();
            if (bc == null) bc = gameObject.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(3.5f, 4.5f);
            bc.offset = new Vector2(0f, -1.0f);
            bc.isTrigger = true;
        }
        else
        {
            var col = GetComponent<Collider2D>();
            if (col == null)
            {
                var cc = gameObject.AddComponent<CircleCollider2D>();
                cc.radius = 0.6f;
                cc.isTrigger = true;
            }
            else
            {
                col.isTrigger = true;
            }
        }
    }

    void Update()
    {
        if (hasTriggered) return;

        if (cachedPlayer == null)
        {
            cachedPlayer = FindAnyObjectByType<PlayerMovement>();
            if (cachedPlayer == null) return;
        }

        // Verificación directa por distancia para garantizar activación aunque fallen capas físicas
        float dx = Mathf.Abs(cachedPlayer.transform.position.x - transform.position.x);
        float dy = Mathf.Abs(cachedPlayer.transform.position.y - transform.position.y);

        // Si Carlos cruza a menos de 1.4m horizontalmente y a menos de 2.0m verticalmente (mismo piso)
        if (dx <= 1.4f && dy <= 2.0f)
        {
            if (canBeJumpedOver && cachedPlayer.IsJumping) return;
            TriggerHit(cachedPlayer);
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

        if (canBeJumpedOver && player.IsJumping) return;

        TriggerHit(player);
    }

    void TriggerHit(PlayerMovement player)
    {
        if (hasTriggered) return;
        hasTriggered = true;

        if (hitSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hitSound, soundVolume);
        }

        if (Level2ChaseRunner.Instance != null)
        {
            if (isSpecter)
            {
                Level2ChaseRunner.Instance.OnSpecterHit(gameObject.name, strikeCount, slowMultiplier, stumbleDuration);
            }
            else
            {
                Level2ChaseRunner.Instance.OnObstacleHit(gameObject.name, strikeCount, stumbleDuration);
            }
        }
    }
}
