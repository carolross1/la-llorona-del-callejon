// Assets/Scripts/Systems/StrayDogAI.cs
using UnityEngine;

/// <summary>
/// Enemigo secundario: Perro callejero territorial.
/// Duerme o vigila su territorio en callejones y esquinas.
/// Si el jugador corre cerca o se acerca sin sigilo:
///   1. Ladra furiosamente ("¡GUAU! ¡GUAU!"), alertando a La Llorona.
///   2. Si el jugador lo toca o choca con él, arremete y le quita una vida.
/// Se puede evitar caminando despacio (sin SHIFT) o usando escondites.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class StrayDogAI : MonoBehaviour
{
    [Header("Rangos de Detección")]
    [SerializeField] float sleepWakeRadius = 4.5f;   // Distancia para despertar si el jugador corre
    [SerializeField] float runWakeRadius = 6.0f;     // Si el jugador corre
    [SerializeField] float alertLloronaRadius = 20.0f;
    [SerializeField] float barkCooldown = 3.5f;

    [Header("Visual")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Color sleepColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] Color alertColor = Color.white;

    PlayerMovement player;
    float lastBarkTime = -99f;
    bool isAwake = false;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        player = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>();
        if (spriteRenderer != null) spriteRenderer.color = sleepColor;
    }

    void Update()
    {
        if (player == null || player.IsHidden)
        {
            if (isAwake && Time.time > lastBarkTime + 4f)
            {
                isAwake = false;
                if (spriteRenderer != null) spriteRenderer.color = sleepColor;
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, player.transform.position);
        bool playerRunning = player.IsRunning;

        // Despertar si corre cerca o camina muy pegado a él
        bool shouldBark = (playerRunning && dist <= runWakeRadius) || (dist <= sleepWakeRadius * 0.6f);

        if (shouldBark && Time.time > lastBarkTime + barkCooldown)
        {
            BarkAndAlert();
        }
    }

    void BarkAndAlert()
    {
        lastBarkTime = Time.time;
        isAwake = true;
        if (spriteRenderer != null) spriteRenderer.color = alertColor;

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowTutorial("¡GUAU! ¡GUAU! ¡El perro callejero alertó a La Llorona!", 3.5f);
        }

        // Alertar a La Llorona
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona != null)
        {
            float dist = Vector2.Distance(transform.position, llorona.transform.position);
            if (dist <= alertLloronaRadius)
            {
                llorona.AlertNoise(transform.position, 5.0f);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckBite(collision.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        CheckBite(other);
    }

    void CheckBite(Collider2D collider)
    {
        if (!collider.CompareTag("Player")) return;
        if (player != null && player.IsHidden) return;

        // Muerde al jugador
        if (HUD.Instance != null)
        {
            HUD.Instance.ShowTutorial("¡El perro callejero te mordió!", 2.5f);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerCaught();
        }
    }
}
