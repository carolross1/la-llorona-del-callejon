// Assets/Scripts/AI/SpecterAI.cs
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Inteligencia Artificial para el Espectro Menor / Sombra del Balcón (Nivel 2):
/// 1. Patrulla flotando suavemente de ida y vuelta a lo largo del balcón del Piso 3.
/// 2. Tiene levitación sinusoidal continua para dar la sensación de espíritu incorpóreo.
/// 3. Cono de visión frontal: si Carlos se esconde en huacales/sacos, el espectro NO lo detecta.
/// 4. Si descubre a Carlos:
///    - Suelta un chillido espectral (screamSfx).
///    - Drena un porcentaje de estamina a Carlos por el sobresalto.
///    - Alerta a La Llorona en la calle baja (lloronaAI.AlertNoise), haciendo que corra a investigar
///      el área de la calle justo debajo del balcón.
///    - Entra en enfriamiento temporal para evitar gritos continuos.
/// </summary>
public class SpecterAI : MonoBehaviour
{
    [Header("Patrulla Horizontal")]
    [SerializeField] float minX = 18.0f;
    [SerializeField] float maxX = 39.0f;
    [SerializeField] float patrolSpeed = 1.35f;
    [SerializeField] float waitTimeAtTurn = 1.5f;

    [Header("Levitación Flotante")]
    [SerializeField] float floatAmplitude = 0.18f;
    [SerializeField] float floatFrequency = 2.2f;

    [Header("Detección de Carlos")]
    [SerializeField] float visionDistance = 5.5f;
    [SerializeField] float visionAngle = 70.0f;
    [SerializeField] float verticalRange = 1.6f;
    [SerializeField] LayerMask obstacleMask = 0;

    [Header("Efecto de Alerta / Susto")]
    [SerializeField] float staminaDrain = 35.0f;
    [SerializeField] float alertCooldown = 5.0f;
    [SerializeField] AudioClip screamSfx;
    [SerializeField] AudioClip ambientSfx;

    SpriteRenderer spriteRenderer;
    AudioSource audioSource;
    Light2D specterLight;
    PlayerMovement player;
    LloronaAI llorona;

    float baseY;
    float currentWaitTimer = 0f;
    float cooldownTimer = 0f;
    bool movingRight = true;
    Color normalColor = new Color(0.35f, 0.85f, 1.0f, 0.72f);
    Color alertColor  = new Color(1.0f, 0.35f, 0.35f, 0.90f);

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.75f;
        audioSource.minDistance = 2.0f;
        audioSource.maxDistance = 16.0f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        baseY = transform.position.y;
    }

    void Start()
    {
        baseY = transform.position.y;
        player = FindAnyObjectByType<PlayerMovement>();
        llorona = FindAnyObjectByType<LloronaAI>();
        specterLight = GetComponentInChildren<Light2D>();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
            spriteRenderer.sortingLayerName = "Characters";
            spriteRenderer.sortingOrder = 11;
        }

        // Sonido ambiental de susurro espectral en loop
        if (ambientSfx != null && audioSource != null)
        {
            audioSource.clip = ambientSfx;
            audioSource.loop = true;
            audioSource.volume = 0.45f;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (player == null) player = FindAnyObjectByType<PlayerMovement>();
        if (llorona == null) llorona = FindAnyObjectByType<LloronaAI>();

        // Enfriamiento de alerta
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f && spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }

        // Patrulla y flotación
        HandlePatrol();

        // Chequeo de visión
        if (cooldownTimer <= 0f && CanSeePlayer())
        {
            TriggerAlert();
        }
    }

    void HandlePatrol()
    {
        // 1. Efecto de levitación sinusoidal
        float bobbing = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        float targetY = baseY + bobbing;

        // 2. Movimiento horizontal
        if (currentWaitTimer > 0f)
        {
            currentWaitTimer -= Time.deltaTime;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            return;
        }

        float currentX = transform.position.x;
        float dir = movingRight ? 1f : -1f;
        float newX = currentX + dir * patrolSpeed * Time.deltaTime;

        if (movingRight && newX >= maxX)
        {
            newX = maxX;
            movingRight = false;
            currentWaitTimer = waitTimeAtTurn;
            FlipSprite(false);
        }
        else if (!movingRight && newX <= minX)
        {
            newX = minX;
            movingRight = true;
            currentWaitTimer = waitTimeAtTurn;
            FlipSprite(true);
        }

        transform.position = new Vector3(newX, targetY, transform.position.z);
    }

    void FlipSprite(bool faceRight)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !faceRight;
        }
    }

    bool CanSeePlayer()
    {
        if (player == null || player.IsHidden) return false;

        Vector2 playerPos = player.transform.position;
        Vector2 myPos = transform.position;

        // Rango vertical: debe estar en el mismo balcón (Piso 3)
        if (Mathf.Abs(playerPos.y - myPos.y) > verticalRange) return false;

        Vector2 toPlayer = playerPos - myPos;
        float dist = toPlayer.magnitude;

        if (dist > visionDistance) return false;

        // Cono de visión hacia la dirección en que flota
        Vector2 facingDir = movingRight ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(facingDir, toPlayer);

        if (angle > visionAngle * 0.5f) return false;

        // Chequeo de línea de visión limpia sin obstáculos intermedios
        if (obstacleMask != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(myPos, toPlayer.normalized, dist, obstacleMask);
            if (hit.collider != null) return false;
        }

        return true;
    }

    void TriggerAlert()
    {
        cooldownTimer = alertCooldown;

        // 1. Cambio visual a rojo espectral alertado
        if (spriteRenderer != null)
        {
            spriteRenderer.color = alertColor;
        }

        // 2. Chillido espectral
        if (screamSfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(screamSfx, 0.9f);
        }

        // 3. Drenar estamina a Carlos por el sobresalto
        if (player != null)
        {
            player.DrainStamina(staminaDrain);
        }

        // 4. Activar ojo en el HUD si está presente
        if (HUD.Instance != null)
        {
            HUD.Instance.SetEyeAlert(true);
        }

        // Si estamos en persecución Runner, aplicar la parálisis y frío espectral
        if (Level2ChaseRunner.Instance != null && Level2ChaseRunner.Instance.IsActive)
        {
            Level2ChaseRunner.Instance.OnSpecterHit("Espectro_Piso3", 1, 0.35f, 1.75f);
        }

        // 5. ALERTAR A LA LLORONA PARA QUE SUBA AL BALCÓN:
        // Sube por las escaleras hacia la pasarela del Piso 3 para cazar a Carlos
        if (llorona != null)
        {
            llorona.AlertBalcony(transform.position);
            Debug.Log($"[SpecterAI] ¡Espectro alertado en X={transform.position.x:F1}! La Llorona sube al balcón del Piso 3.");
        }
    }

    public void ConfigurePatrol(float min, float max, float speed, AudioClip scream, AudioClip ambient)
    {
        minX = min;
        maxX = max;
        patrolSpeed = speed;
        screamSfx = scream;
        ambientSfx = ambient;
        baseY = transform.position.y;
    }
}
