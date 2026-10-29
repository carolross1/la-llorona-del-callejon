// Assets/Scripts/Systems/NoiseObstacle.cs
using UnityEngine;

/// <summary>
/// Obstáculo del entorno: Botellas y vidrios en el suelo.
/// Al pisar botellas, emite sonido de vidrio roto, cambia el sprite a Vidrios_rotos
/// y alerta a La Llorona hacia esa posición exacta.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class NoiseObstacle : MonoBehaviour
{
    [SerializeField] float alertRadius = 18f;
    [SerializeField] float cooldown = 3f;
    [SerializeField] Sprite brokenSprite;
    [SerializeField] AudioClip glassBreakSound;

    SpriteRenderer spriteRenderer;
    float lastTriggerTime = -99f;
    bool isBroken = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

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

        var player = other.GetComponent<PlayerMovement>() ?? other.GetComponentInParent<PlayerMovement>();
        if (player == null || player.IsHidden) return;

        // Protección contra activación lejana: solo se activa si los pies de Carlos pisan directamente sobre las botellas (< 0.40 u)
        Vector2 feetPos = (Vector2)player.transform.position + new Vector2(0f, -0.35f);
        float distToFeet = Vector2.Distance((Vector2)transform.position, feetPos);
        if (distToFeet > 0.40f) return;

        lastTriggerTime = Time.time;

        // Cambio visual a vidrios rotos al pisarse
        if (!isBroken && brokenSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = brokenSprite;
            isBroken = true;
        }

        // Sonido de vidrio
        if (glassBreakSound == null && AudioManager.Instance != null)
        {
            glassBreakSound = AudioManager.Instance.sonidoVidrio;
        }
        if (glassBreakSound == null)
        {
            glassBreakSound = Resources.Load<AudioClip>("Audio/SFX/Nivel1/vidrio");
        }

        if (AudioManager.Instance != null)
        {
            if (glassBreakSound != null)
                AudioManager.Instance.PlaySFX(glassBreakSound, 1.0f);
            else
                AudioManager.Instance.PlayFootstep(); // Respaldo acústico
        }
        else if (glassBreakSound != null)
        {
            AudioSource.PlayClipAtPoint(glassBreakSound, transform.position, 1.0f);
        }

        // Aviso oficial en la franja superior del HUD
        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡Cristales! La Llorona te oyó", 2.5f);
        }

        // Alertar a La Llorona hacia esta posición exacta
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona != null)
        {
            float dist = Vector2.Distance(transform.position, llorona.transform.position);
            if (dist <= alertRadius)
            {
                llorona.AlertNoise(transform.position, 5.0f);
            }
        }
    }
}

