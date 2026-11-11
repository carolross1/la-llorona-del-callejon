// Assets/Scripts/Llorona/LloronaBossAI.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Inteligencia Artificial del Enfrentamiento Final (Boss Fight - Nivel 4):
/// - 3 Fases de combate con barra de vida de 9 impactos (3 por fase).
/// - Fase 1: Acecho directo y embestidas telegrafiadas.
/// - Fase 2: Danza de sombras, teletransporte e invocación de espectros.
/// - Fase 3: Furia espectral y persecución a máxima velocidad.
/// - Derrota: Exorcismo y purificación sagrada en destello dorado, abriendo la Gran Reja.
/// </summary>
public class LloronaBossAI : MonoBehaviour
{
    public static LloronaBossAI Instance { get; private set; }

    public static event Action<int, int> OnHealthChanged;
    public static event Action<int> OnPhaseChanged;
    public static event Action OnDefeated;

    enum BossState { NormalChase, TelegraphLunge, Lunging, Staggered, Teleporting, Purified }

    [Header("Salud y Fases")]
    [SerializeField] int maxHealth = 9;
    [SerializeField] int currentHealth = 9;
    [SerializeField] int currentPhase = 1;

    [Header("Velocidades por Fase")]
    [SerializeField] float phase1Speed = 2.85f;
    [SerializeField] float phase2Speed = 3.40f;
    [SerializeField] float phase3Speed = 3.85f;
    [SerializeField] float lungeSpeed = 6.5f;

    [Header("Audio SFX")]
    [SerializeField] AudioClip screamSfx;
    [SerializeField] AudioClip hurtSfx;
    [SerializeField] AudioClip lungeSfx;
    [SerializeField] AudioClip purifiedSfx;

    [Header("Espectros Menores (Fase 2)")]
    [SerializeField] GameObject specter1;
    [SerializeField] GameObject specter2;

    [Header("Efectos Visuales")]
    [SerializeField] Light2D bossLight;
    [SerializeField] Light2D globalLight;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    PlayerMovement player;
    BossState state = BossState.NormalChase;

    float lungeTimer = 5.0f;
    float invulnerableTimer = 0f;
    Vector2 lungeTargetDir;
    Vector2 facing = Vector2.down;
    Color normalColor = Color.white;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentPhase => currentPhase;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) normalColor = spriteRenderer.color;
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentPhase = 1;
        player = FindAnyObjectByType<PlayerMovement>();

        if (specter1 != null) specter1.SetActive(false);
        if (specter2 != null) specter2.SetActive(false);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnPhaseChanged?.Invoke(currentPhase);
    }

    void Update()
    {
        if (player == null) player = FindAnyObjectByType<PlayerMovement>();
        if (state == BossState.Purified) return;

        // Temporizador de invulnerabilidad tras ser impactada
        if (invulnerableTimer > 0f)
        {
            invulnerableTimer -= Time.deltaTime;
            if (spriteRenderer != null)
            {
                // Parpadeo espectral
                spriteRenderer.enabled = (Mathf.Sin(Time.time * 25f) > 0);
            }
            if (invulnerableTimer <= 0f && spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = normalColor;
            }
        }

        switch (state)
        {
            case BossState.NormalChase:
                HandleChase();
                HandleLungeCooldown();
                break;

            case BossState.Lunging:
                // Embestida en línea recta
                if (rb != null) rb.linearVelocity = lungeTargetDir * lungeSpeed;
                break;
        }
    }

    void HandleChase()
    {
        if (player == null || player.IsHidden)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        float speed = (currentPhase == 1) ? phase1Speed : (currentPhase == 2 ? phase2Speed : phase3Speed);
        Vector2 toPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
        Vector2 dir = toPlayer.normalized;

        if (rb != null) rb.linearVelocity = dir * speed;
        if (dir.sqrMagnitude > 0.01f)
        {
            facing = dir;
            if (spriteRenderer != null) spriteRenderer.flipX = (dir.x < 0);
        }
    }

    void HandleLungeCooldown()
    {
        if (player == null || player.IsHidden) return;

        lungeTimer -= Time.deltaTime;
        if (lungeTimer <= 0f)
        {
            StartCoroutine(PerformLungeSequence());
        }
    }

    IEnumerator PerformLungeSequence()
    {
        state = BossState.TelegraphLunge;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 1. Telegrafeado: Alarido y destello rojo de aviso (0.6s)
        PlaySound(lungeSfx ?? screamSfx, 1.0f);
        if (bossLight != null) bossLight.color = new Color(1.0f, 0.2f, 0.2f);
        if (spriteRenderer != null) spriteRenderer.color = new Color(1.0f, 0.4f, 0.4f);

        yield return new WaitForSeconds(0.6f);

        // 2. Ejecutar embestida veloz hacia donde estaba Carlos
        if (player != null)
        {
            lungeTargetDir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        }
        else
        {
            lungeTargetDir = facing;
        }

        state = BossState.Lunging;
        yield return new WaitForSeconds(0.45f);

        // 3. Recuperación tras la embestida
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (bossLight != null) UpdateBossLightColor();
        if (spriteRenderer != null) spriteRenderer.color = normalColor;

        lungeTimer = (currentPhase == 1) ? UnityEngine.Random.Range(5.5f, 7.5f) : UnityEngine.Random.Range(4.0f, 6.0f);
        state = BossState.NormalChase;
    }

    public void TakeDamage(int damage = 1)
    {
        if (invulnerableTimer > 0f || state == BossState.Purified) return;

        currentHealth -= damage;
        invulnerableTimer = 0.85f;

        PlaySound(hurtSfx ?? screamSfx, 1.0f);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Aviso visual en el HUD
        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice($"¡Impacto Sagrado! Vida de La Llorona: {currentHealth} / {maxHealth}", 1.4f);
        }

        // Efecto visual de destello de daño
        StartCoroutine(HitFlashEffect());

        // Retroceso por el impacto sagrado (Knockback)
        if (player != null && rb != null)
        {
            Vector2 awayDir = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
            transform.position += (Vector3)(awayDir * 1.5f);
        }

        // Revisar cambio de fase o derrota
        if (currentHealth <= 0)
        {
            StartCoroutine(DefeatAndPurifySequence());
        }
        else if (currentHealth == 6 && currentPhase == 1)
        {
            StartCoroutine(TransitionToPhase(2));
        }
        else if (currentHealth == 3 && currentPhase == 2)
        {
            StartCoroutine(TransitionToPhase(3));
        }
        else if (currentPhase >= 2)
        {
            // En fase 2 y 3, tiene chance de teletransportarse tras ser golpeada
            StartCoroutine(TeleportReposition());
        }
    }

    IEnumerator HitFlashEffect()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = normalColor;
        Vector3 originalScale = transform.localScale;

        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = new Color(1.0f, 0.25f, 0.25f, 0.95f); // Rojo dolor
            transform.localScale = originalScale * 1.15f;
            yield return new WaitForSeconds(0.08f);

            spriteRenderer.color = new Color(0.4f, 0.95f, 1.0f, 1.0f); // Celeste sacro
            transform.localScale = originalScale;
            yield return new WaitForSeconds(0.08f);
        }

        spriteRenderer.color = originalColor;
        transform.localScale = originalScale;
    }

    IEnumerator TransitionToPhase(int newPhase)
    {
        state = BossState.Teleporting;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        currentPhase = newPhase;
        OnPhaseChanged?.Invoke(currentPhase);

        PlaySound(screamSfx, 1.0f);

        if (newPhase == 2)
        {
            if (HUD.Instance != null)
                HUD.Instance.ShowEventNotice("¡Fase 2: La Furia de las Ánimas!\nLa Llorona invoca espíritus y se teletransporta.", 4.5f);

            // Despertar a los dos espectros en la arena
            if (specter1 != null) specter1.SetActive(true);
            if (specter2 != null) specter2.SetActive(true);

            // Reubicarse en el centro
            transform.position = new Vector3(23.5f, 14.0f, 0f);
        }
        else if (newPhase == 3)
        {
            if (HUD.Instance != null)
                HUD.Instance.ShowEventNotice("¡Fase 3: El Alarido Final!\n¡La Llorona está desesperada, acéptala con el último golpe!", 4.5f);

            // Niebla y luz roja de tensión
            if (globalLight != null) globalLight.color = new Color(0.45f, 0.18f, 0.22f);
        }

        UpdateBossLightColor();
        yield return new WaitForSeconds(1.2f);
        state = BossState.NormalChase;
    }

    IEnumerator TeleportReposition()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        yield return new WaitForSeconds(0.4f);

        // Reaparece a 6 metros de Carlos en una posición válida
        if (player != null)
        {
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 5.5f;
            Vector3 newPos = player.transform.position + (Vector3)offset;
            newPos.x = Mathf.Clamp(newPos.x, 4.0f, 42.0f);
            newPos.y = Mathf.Clamp(newPos.y, 4.0f, 24.0f);
            transform.position = newPos;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    IEnumerator DefeatAndPurifySequence()
    {
        state = BossState.Purified;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        OnDefeated?.Invoke();

        // 1. Sonido celestial de purificación
        PlaySound(purifiedSfx, 1.0f);

        // 2. Destello dorado sagrado
        if (bossLight != null)
        {
            bossLight.color = new Color(1.0f, 0.90f, 0.40f);
            bossLight.intensity = 4.0f;
            bossLight.pointLightOuterRadius = 8.0f;
        }

        // 3. Notificación triunfal en el HUD
        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡¡¡EL ESPÍRITU DE LA LLORONA HA SIDO PURIFICADO!!!\n¡¡El tormento terminó! Cruza la Gran Reja hacia tu hogar!!", 8.0f);
        }

        // 4. Abrir la Gran Reja Gótica y activar LevelGoal
        var ritualCtrl = FindAnyObjectByType<Level4RitualController>();
        var goal = FindAnyObjectByType<LevelGoal>(FindObjectsInactive.Include);
        if (goal != null)
        {
            goal.gameObject.SetActive(true);
            var col = goal.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }
        var barrier = GameObject.Find("Barrera_Solida_Sello");
        if (barrier != null) barrier.SetActive(false);

        var gateLight = GameObject.Find("LuzSalvacion")?.GetComponent<Light2D>();
        if (gateLight != null)
        {
            gateLight.color = new Color(0.15f, 1.0f, 0.45f);
            gateLight.intensity = 3.5f;
        }

        var label = GameObject.Find("LabelMeta")?.GetComponent<TextMesh>();
        if (label != null)
        {
            label.text = "¡¡¡LIBERTAD (A TU HOGAR)!!! ➔";
            label.color = new Color(0.2f, 1.0f, 0.5f);
        }

        // 5. Desvanecimiento suave del espíritu
        for (float t = 0; t < 2.5f; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(1.0f, 0f, t / 2.5f);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1.0f, 1.0f, 0.8f, alpha);
                transform.position += Vector3.up * (Time.deltaTime * 0.4f); // Ascenso celestial
            }
            yield return null;
        }

        gameObject.SetActive(false);
    }

    void UpdateBossLightColor()
    {
        if (bossLight == null) return;
        if (currentPhase == 1) bossLight.color = new Color(0.35f, 0.80f, 1.0f); // Azul etéreo
        else if (currentPhase == 2) bossLight.color = new Color(0.70f, 0.25f, 0.95f); // Púrpura de sombras
        else bossLight.color = new Color(1.0f, 0.25f, 0.25f); // Rojo de furia
    }

    void PlaySound(AudioClip clip, float vol)
    {
        if (clip == null) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(clip, vol);
        else AudioSource.PlayClipAtPoint(clip, transform.position, vol);
    }

    void OnCollisionEnter2D(Collision2D c) { CheckCatch(c.collider); }
    void OnTriggerEnter2D(Collider2D c) { CheckCatch(c); }

    void CheckCatch(Collider2D col)
    {
        if (state == BossState.Purified || invulnerableTimer > 0f) return;
        if (col == null || !col.CompareTag("Player")) return;
        if (player != null && player.IsHidden) return;

        // Si atrapa a Carlos
        PlaySound(screamSfx, 1.0f);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerCaught();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
