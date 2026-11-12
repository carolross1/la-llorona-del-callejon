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
    [SerializeField] float phase1Speed = 2.35f;
    [SerializeField] float phase2Speed = 2.75f;
    [SerializeField] float phase3Speed = 3.15f;
    [SerializeField] float lungeSpeed = 5.2f;

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
    float stunTimer = 0f;
    float shockwaveTimer = 7.0f;
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

        // Si está aturdida por impacto de agua bendita:
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

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

        if (stunTimer > 0f) return;

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

        // Alarido en Onda Expansiva periódico (Fase 2 y 3 si Carlos está cerca)
        if (currentPhase >= 2 && state == BossState.NormalChase)
        {
            shockwaveTimer -= Time.deltaTime;
            if (shockwaveTimer <= 0f && toPlayer.magnitude < 4.8f)
            {
                shockwaveTimer = (currentPhase == 2) ? 10.5f : 8.0f;
                StartCoroutine(PerformShockwaveScream(shockwaveRadius: 5.2f, knockbackForce: 3.8f));
                return;
            }
        }

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
        invulnerableTimer = 1.0f;
        stunTimer = 1.2f;

        PlaySound(hurtSfx ?? screamSfx, 1.0f);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Aviso visual en el HUD
        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice($"¡Impacto Sagrado! Vida de La Llorona: {currentHealth} / {maxHealth}", 1.4f);
        }

        // Efecto visual de destello de daño
        StartCoroutine(HitFlashEffect());

        // Retroceso contundente por el impacto sagrado (Knockback de 3.2m)
        if (player != null && rb != null)
        {
            Vector2 awayDir = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
            if (awayDir.sqrMagnitude < 0.01f) awayDir = Vector2.up;
            transform.position += (Vector3)(awayDir * 3.2f);
            rb.linearVelocity = Vector2.zero;
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

            // Reubicarse en el centro de la arena expandida (X: 45)
            transform.position = new Vector3(45.0f, 14.0f, 0f);
        }
        else if (newPhase == 3)
        {
            if (HUD.Instance != null)
                HUD.Instance.ShowEventNotice("¡Fase 3: El Alarido Final!\n¡La Llorona está desesperada, acéptala con el último golpe!", 4.5f);

            // Niebla y luz roja de tensión
            if (globalLight != null) globalLight.color = new Color(0.45f, 0.18f, 0.22f);
        }

        UpdateBossLightColor();
        yield return new WaitForSeconds(0.4f);

        // Alarido con onda de choque masiva al estallar la nueva fase
        yield return StartCoroutine(PerformShockwaveScream(shockwaveRadius: 6.2f, knockbackForce: 4.5f));

        yield return new WaitForSeconds(0.4f);
        state = BossState.NormalChase;
    }

    public IEnumerator PerformShockwaveScream(float shockwaveRadius = 5.2f, float knockbackForce = 4.0f)
    {
        if (state == BossState.Purified) yield break;

        // Breve pausa para telegrafiar el alarido (0.4s)
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Color origColor = normalColor;
        if (bossLight != null)
        {
            bossLight.color = (currentPhase == 3) ? new Color(1.0f, 0.15f, 0.2f) : new Color(0.85f, 0.25f, 1.0f);
            bossLight.intensity = 3.8f;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1.0f, 0.4f, 0.4f);
        }

        yield return new WaitForSeconds(0.40f);

        // 1. Chillido ensordecedor
        PlaySound(screamSfx, 1.0f);

        // 2. Destello visual de onda expansiva
        StartCoroutine(SpawnShockwaveVisualRing(transform.position, shockwaveRadius));

        // 3. Empuje y susto a Carlos si está dentro del radio
        if (player != null && !player.IsHidden)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist <= shockwaveRadius)
            {
                Vector2 pushDir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
                if (pushDir.sqrMagnitude < 0.01f) pushDir = (transform.localScale.x < 0) ? Vector2.left : Vector2.right;

                // Empuje físico
                player.transform.position += (Vector3)(pushDir * knockbackForce);

                // Drenado de estamina por el impacto del grito (25 puntos)
                player.DrainStamina(25f);

                if (HUD.Instance != null)
                {
                    HUD.Instance.ShowEventNotice("¡¡EL ALARIDO DE LA LLORONA TE EMPUJA HACIA ATRÁS!!", 1.8f);
                }
            }
        }

        yield return new WaitForSeconds(0.45f);

        if (spriteRenderer != null) spriteRenderer.color = origColor;
        UpdateBossLightColor();
    }

    IEnumerator SpawnShockwaveVisualRing(Vector3 center, float maxRadius)
    {
        GameObject shockObj = new GameObject("Shockwave_FX");
        shockObj.transform.position = center;

        Light2D l2d = shockObj.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = (currentPhase == 3) ? new Color(1.0f, 0.25f, 0.25f) : new Color(0.85f, 0.35f, 1.0f);
        l2d.intensity = 4.5f;
        l2d.pointLightInnerRadius = 0.5f;
        l2d.pointLightOuterRadius = 1.0f;

        float dur = 0.5f;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float p = t / dur;
            if (l2d != null)
            {
                l2d.pointLightOuterRadius = Mathf.Lerp(1.0f, maxRadius * 1.35f, p);
                l2d.intensity = Mathf.Lerp(4.5f, 0f, p);
            }
            yield return null;
        }

        Destroy(shockObj);
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
            newPos.x = Mathf.Clamp(newPos.x, 6.0f, 66.0f);
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

        // 2. Luz celeste y pacífica
        if (bossLight != null)
        {
            bossLight.color = new Color(0.4f, 0.85f, 1.0f);
            bossLight.intensity = 3.0f;
            bossLight.pointLightOuterRadius = 6.0f;
        }

        if (globalLight != null)
        {
            globalLight.color = new Color(0.35f, 0.45f, 0.65f); // Luz pacífica de luna
            globalLight.intensity = 1.0f;
        }

        // 3. Purificar y desvanecer a todos los espectros menores invocados
        PurifyAllSpecters();

        // 4. Conectar con la Tumba de La Llorona para la ofrenda final
        if (LloronaGraveOffering.Instance != null)
        {
            LloronaGraveOffering.Instance.EnableOffering();

            // Flotar suavemente hacia su tumba y posarse en vigilia en paz
            Vector3 targetGravePos = LloronaGraveOffering.Instance.transform.position + new Vector3(1.1f, 0.4f, 0f);
            Vector3 startPos = transform.position;
            float travelTime = 2.4f;

            for (float t = 0; t < travelTime; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(startPos, targetGravePos, t / travelTime);
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(0.7f, 0.95f, 1.0f, 0.85f);
                }
                yield return null;
            }
            transform.position = targetGravePos;
        }
        else
        {
            // Respaldo por si no estuviera la tumba
            AscendToHeaven();
        }
    }

    public void AscendToHeaven()
    {
        StartCoroutine(AscensionSequence());
    }

    IEnumerator AscensionSequence()
    {
        if (bossLight != null)
        {
            bossLight.color = new Color(1.0f, 0.95f, 0.45f);
            bossLight.intensity = 4.5f;
            bossLight.pointLightOuterRadius = 8.5f;
        }

        PlaySound(purifiedSfx, 1.0f);

        // Desvanecimiento suave y ascenso al cielo
        for (float t = 0; t < 2.5f; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(0.85f, 0f, t / 2.5f);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1.0f, 1.0f, 0.85f, alpha);
                transform.position += Vector3.up * (Time.deltaTime * 0.55f);
            }
            yield return null;
        }

        gameObject.SetActive(false);
    }

    void PurifyAllSpecters()
    {
        if (specter1 != null) StartCoroutine(PurifyMinorSpecter(specter1));
        if (specter2 != null) StartCoroutine(PurifyMinorSpecter(specter2));

        // Por seguridad, purificar cualquier otro espectro activo en el cementerio
        SpecterAI[] allSpecters = FindObjectsByType<SpecterAI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var sp in allSpecters)
        {
            if (sp != null && sp.gameObject != specter1 && sp.gameObject != specter2)
            {
                StartCoroutine(PurifyMinorSpecter(sp.gameObject));
            }
        }
    }

    IEnumerator PurifyMinorSpecter(GameObject spObj)
    {
        if (spObj == null) yield break;

        // Desactivar colisiones para que no puedan asustar ni tocar a Carlos
        var cols = spObj.GetComponentsInChildren<Collider2D>();
        foreach (var c in cols) c.enabled = false;

        var spAI = spObj.GetComponent<SpecterAI>();
        if (spAI != null) spAI.enabled = false;

        var sr = spObj.GetComponentInChildren<SpriteRenderer>();
        var l2d = spObj.GetComponentInChildren<Light2D>();
        if (l2d != null)
        {
            l2d.color = new Color(1.0f, 0.90f, 0.45f); // Luz sagrada dorada
            l2d.intensity = 2.0f;
        }

        // Ascenso y desvanecimiento
        for (float t = 0; t < 2.0f; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(1.0f, 0f, t / 2.0f);
            if (sr != null) sr.color = new Color(1.0f, 1.0f, 0.85f, alpha);
            if (spObj != null) spObj.transform.position += Vector3.up * (Time.deltaTime * 0.45f);
            yield return null;
        }

        if (spObj != null) spObj.SetActive(false);
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
        if (state == BossState.Purified || invulnerableTimer > 0f || stunTimer > 0f) return;
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
