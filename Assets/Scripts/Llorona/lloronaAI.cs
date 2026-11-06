using UnityEngine;
using UnityEngine.SceneManagement;

public class LloronaAI : MonoBehaviour
{
    enum State { Patrol, Wait, Chase, Distracted, InvestigateBalcony }

    [Header("Ajustes de Dificultad (ScriptableObject)")]
    [SerializeField] LloronaSettings settings;

    [Header("Patrulla")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float patrolSpeed = 1.5f;
    [SerializeField] float waitTime = 2f;

    public Transform[] Waypoints
    {
        get => waypoints;
        set => waypoints = value;
    }

    [Header("Persecución")]
    [SerializeField] float chaseSpeed = 2.8f;
    [SerializeField] float loseSightTime = 3f;

    [Header("Detección")]
    [SerializeField] float viewDistance = 5f;
    [SerializeField] float viewAngle = 100f;
    [SerializeField] float hearDistance = 1.5f;
    [SerializeField] float runNoiseMultiplier = 1.5f;
    [SerializeField] LayerMask obstacleMask;   // Layer Walls

    [Header("Audio SFX (Nivel 1)")]
    [SerializeField] AudioClip screamSfx;
    [Range(0f, 1f)] [SerializeField] float screamVolume = 0.95f;
    [SerializeField] AudioSource audioSource;
    float lastScreamTime = -99f;

    [Header("Audio y Llanto Continuo por Proximidad")]
    [SerializeField] AudioClip cryLoopClip;
    [SerializeField] AudioSource cryAudioSource;
    [SerializeField] float maxCryDistance = 22.0f;
    [SerializeField] float minCryDistance = 2.0f;
    [Range(0f, 1f)] [SerializeField] float maxCryVolume = 0.85f;

    Rigidbody2D rb;
    PlayerMovement player;
    State state = State.Patrol;
    int idx;
    float timer;
    bool caught;
    Vector2 facing = Vector2.down;
    Vector2 lastSeenPos;
    Vector2 distractionPos;

    // Ruta de escalada al balcón (Nivel 2)
    Vector2[] balconyPath;
    int balconyPathIdx = 0;
    bool isReturningFromBalcony = false;
    float balconySearchTimer = 0f;

    public Vector2 Facing => facing;
    public bool IsChasing => state == State.Chase;
    public bool IsDistracted => state == State.Distracted;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
        }
        transform.rotation = Quaternion.identity;
        EnsureAudioSource();
        player = FindAnyObjectByType<PlayerMovement>();
        ApplySettings();
        ValidateWaypoints();
    }

    void ValidateWaypoints()
    {
        var valid = new System.Collections.Generic.List<Transform>();
        if (waypoints != null)
        {
            foreach (var w in waypoints)
            {
                if (w != null) valid.Add(w);
            }
        }
        if (valid.Count == 0)
        {
            var wp1 = GameObject.Find("WP_Paseo1") ?? GameObject.Find("WP_Codo1") ?? GameObject.Find("WP_Balcones");
            var wp2 = GameObject.Find("WP_Paseo2") ?? GameObject.Find("WP_Codo2");
            if (wp1 != null) valid.Add(wp1.transform);
            if (wp2 != null) valid.Add(wp2.transform);
        }
        if (valid.Count > 0)
        {
            waypoints = valid.ToArray();
        }
    }

    bool HasValidWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            ValidateWaypoints();
            if (waypoints == null || waypoints.Length == 0) return false;
        }

        if (idx < 0 || idx >= waypoints.Length) idx = 0;
        if (waypoints[idx] == null)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    idx = i;
                    return true;
                }
            }
            return false;
        }
        return true;
    }

    void EnsureAudioSource()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length > 0)
        {
            audioSource = sources[0];
            if (sources.Length > 1) cryAudioSource = sources[1];
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.65f;
        audioSource.minDistance = 2.5f;
        audioSource.maxDistance = 28.0f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        if (cryAudioSource == null)
        {
            cryAudioSource = gameObject.AddComponent<AudioSource>();
        }
        cryAudioSource.playOnAwake = false;
        cryAudioSource.loop = true;
        cryAudioSource.spatialBlend = 0.85f;
        cryAudioSource.minDistance = minCryDistance;
        cryAudioSource.maxDistance = maxCryDistance;
        cryAudioSource.rolloffMode = AudioRolloffMode.Linear;

        InitCryClip();
    }

    void InitCryClip()
    {
        if (cryLoopClip == null)
        {
            if (AudioManager.Instance != null && AudioManager.Instance.lloronaLlanto != null)
                cryLoopClip = AudioManager.Instance.lloronaLlanto;
            else
                cryLoopClip = Resources.Load<AudioClip>("Audio/SFX/Nivel1/llorona_nivel1.mpeg");
        }

        if (cryAudioSource != null && cryLoopClip != null)
        {
            cryAudioSource.clip = cryLoopClip;
            cryAudioSource.volume = 0f;
            if (!cryAudioSource.isPlaying) cryAudioSource.Play();
        }
    }

    public void SetSettings(LloronaSettings newSettings)
    {
        settings = newSettings;
        ApplySettings();
    }

    void ApplySettings()
    {
        if (settings == null) return;
        patrolSpeed = settings.PatrolSpeed;
        chaseSpeed = settings.ChaseSpeed;
        viewDistance = settings.ViewDistance;
        viewAngle = settings.ViewAngle;
        hearDistance = settings.HearDistance;
        waitTime = settings.WaitTime;
        loseSightTime = settings.LoseSightTime;
        runNoiseMultiplier = settings.RunNoiseMultiplier;
    }

    void Update()
    {
        UpdateProximityCry();
    }

    void UpdateProximityCry()
    {
        if (cryAudioSource == null) return;

        if (cryAudioSource.clip == null)
        {
            InitCryClip();
        }

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerMovement>();
            if (player == null)
            {
                cryAudioSource.volume = Mathf.MoveTowards(cryAudioSource.volume, 0f, Time.deltaTime * 0.8f);
                return;
            }
        }

        if (!cryAudioSource.isPlaying && cryAudioSource.clip != null)
        {
            cryAudioSource.Play();
        }

        float dist = Vector2.Distance(transform.position, player.transform.position);

        float targetVol = 0f;
        if (dist <= maxCryDistance)
        {
            // El llanto incrementa progresivamente conforme se acerca a Carlos
            float t = 1.0f - Mathf.Clamp01((dist - minCryDistance) / (maxCryDistance - minCryDistance));
            targetVol = Mathf.Pow(t, 1.25f) * maxCryVolume;

            if (player.IsHidden)
            {
                // Amortiguar ligeramente si Carlos se oculta en un escondite
                targetVol *= 0.6f;
            }
            else if (state == State.Chase)
            {
                // Mayor desesperación e intensidad sonora en persecución directa
                targetVol = Mathf.Clamp01(targetVol * 1.25f);
            }
        }

        // Suavizado dinámico del volumen
        cryAudioSource.volume = Mathf.MoveTowards(cryAudioSource.volume, targetVol, Time.deltaTime * 0.85f);
    }

    void OnDisable()
    {
        if (cryAudioSource != null && cryAudioSource.isPlaying)
        {
            cryAudioSource.Stop();
        }
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerMovement>();
            if (player == null) return;
        }
        if (caught) return;

        if (Level2ChaseRunner.Instance != null && Level2ChaseRunner.Instance.IsActive)
        {
            state = State.Chase;
            facing = Vector2.right;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        bool sees = CanSeePlayer();

        switch (state)
        {
            case State.Patrol:
                if (sees) { StartChase(); break; }

                if (!HasValidWaypoint())
                {
                    rb.linearVelocity = Vector2.zero;
                    break;
                }

                MoveTo(waypoints[idx].position, patrolSpeed);
                if (Vector2.Distance(transform.position, waypoints[idx].position) < 0.45f)
                {
                    state = State.Wait;
                    timer = waitTime;
                    rb.linearVelocity = Vector2.zero;
                }
                break;

            case State.Wait:
                rb.linearVelocity = Vector2.zero;
                if (sees) { StartChase(); break; }

                // Orientarse hacia el próximo waypoint para vigilar el pasillo en vez de mirar hacia la pared
                if (waypoints != null && waypoints.Length > 1)
                {
                    int nextIdx = (idx + 1) % waypoints.Length;
                    if (waypoints[nextIdx] != null)
                    {
                        Vector2 nextDir = (waypoints[nextIdx].position - transform.position).normalized;
                        if (nextDir.sqrMagnitude > 0.01f) facing = nextDir;
                    }
                }

                timer -= Time.fixedDeltaTime;
                if (timer <= 0)
                {
                    if (waypoints != null && waypoints.Length > 0)
                        idx = (idx + 1) % waypoints.Length;
                    state = State.Patrol;
                }
                break;

            case State.Chase:
                if (sees)
                {
                    lastSeenPos = player.transform.position;
                    timer = loseSightTime;
                }
                else
                {
                    timer -= Time.fixedDeltaTime;
                }

                if (Vector2.Distance(transform.position, lastSeenPos) > 0.3f)
                    MoveTo(lastSeenPos, chaseSpeed);
                else
                    rb.linearVelocity = Vector2.zero;

                if (timer <= 0)
                {
                    state = State.Patrol;
                    if (HUD.Instance != null) HUD.Instance.SetEyeAlert(false);
                }
                break;

            case State.Distracted:
                timer -= Time.fixedDeltaTime;
                if (Vector2.Distance(transform.position, distractionPos) > 0.3f)
                    MoveTo(distractionPos, patrolSpeed * 0.8f);
                else
                    rb.linearVelocity = Vector2.zero;

                if (timer <= 0) state = State.Patrol;
                break;

            case State.InvestigateBalcony:
                if (sees) { StartChase(); break; }
                if (balconyPath == null || balconyPath.Length == 0) { state = State.Patrol; break; }

                if (balconyPathIdx < balconyPath.Length)
                {
                    Vector2 targetNode = balconyPath[balconyPathIdx];
                    MoveTo(targetNode, chaseSpeed * 1.15f);
                    if (Vector2.Distance(transform.position, targetNode) < 0.45f)
                    {
                        balconyPathIdx++;
                        if (isReturningFromBalcony && balconyPathIdx >= balconyPath.Length)
                        {
                            state = State.Patrol;
                            if (HUD.Instance != null) HUD.Instance.SetEyeAlert(false);
                        }
                    }
                }
                else if (!isReturningFromBalcony)
                {
                    balconySearchTimer -= Time.fixedDeltaTime;
                    float searchOffset = Mathf.Sin(Time.time * 2.0f) * 2.5f;
                    Vector2 searchSpot = new Vector2(balconyPath[balconyPath.Length - 1].x + searchOffset, 6.50f);
                    MoveTo(searchSpot, patrolSpeed * 1.2f);

                    if (balconySearchTimer <= 0f)
                    {
                        isReturningFromBalcony = true;
                        balconyPathIdx = 0;
                        balconyPath = new Vector2[]
                        {
                            new Vector2(14.7f, 6.50f),
                            new Vector2(10.6f, 3.25f),
                            new Vector2(4.2f, 3.25f),
                            new Vector2(0.3f, 0.40f)
                        };
                        if (HUD.Instance != null)
                        {
                            HUD.Instance.ShowEventNotice("La Llorona no encontró a nadie y regresa a la calle...", 3.0f);
                            HUD.Instance.SetEyeAlert(false);
                        }
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Distrae a La Llorona hacia una ofrenda o posición específica durante varios segundos.
    /// Durante la distracción no persigue a Carlos.
    /// </summary>
    public void Distract(Vector2 targetPos, float duration = 7.0f)
    {
        state = State.Distracted;
        timer = duration;
        distractionPos = targetPos;
        if (HUD.Instance != null)
        {
            HUD.Instance.SetEyeAlert(false);
        }
    }

    /// <summary>
    /// Distrae a La Llorona deteniéndola en su posición actual durante varios segundos.
    /// </summary>
    public void Distract(float duration = 7.0f)
    {
        Distract(transform.position, duration);
    }

    /// <summary>
    /// Alerta a La Llorona por un sonido (lata pisada, perro ladrando) para que investigue el origen.
    /// </summary>
    public void AlertNoise(Vector2 soundPos, float alertDuration = 4.0f)
    {
        if (state == State.Distracted) return;
        lastSeenPos = soundPos;
        state = State.Chase;
        timer = alertDuration;
    }

    /// <summary>
    /// Alerta a La Llorona cuando el espectro del balcón detecta al jugador en el Piso 3.
    /// La Llorona sube corriendo por las escaleras hacia la pasarela del Piso 3 para buscar a Carlos.
    /// </summary>
    public void AlertBalcony(Vector2 specterPos)
    {
        if (state == State.Distracted) return;
        if (state == State.Chase && player != null && Vector2.Distance(transform.position, player.transform.position) < 4.0f) return;

        state = State.InvestigateBalcony;
        isReturningFromBalcony = false;
        balconyPathIdx = 0;
        balconySearchTimer = 7.5f;

        float targetX = Mathf.Clamp(specterPos.x, 18.0f, 40.0f);
        balconyPath = new Vector2[]
        {
            new Vector2(0.3f, 0.40f),
            new Vector2(4.2f, 3.25f),
            new Vector2(10.6f, 3.25f),
            new Vector2(14.7f, 6.50f),
            new Vector2(targetX, 6.50f)
        };

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡El espectro te ha delatado! ¡La Llorona sube al balcón!", 3.5f);
            HUD.Instance.SetEyeAlert(true);
        }
        PlayScream();
    }

    void StartChase()
    {
        state = State.Chase;
        timer = loseSightTime;
        lastSeenPos = player.transform.position;
        if (HUD.Instance != null)
        {
            HUD.Instance.SetEyeAlert(true);
            HUD.Instance.ShowEventNotice("¡La Llorona te ha visto!", 2.0f);
        }

        PlayScream();
    }

    void PlayScream()
    {
        if (Time.time - lastScreamTime < 8.0f) return;
        lastScreamTime = Time.time;

        if (screamSfx == null && AudioManager.Instance != null)
        {
            screamSfx = AudioManager.Instance.lloronaGrito;
        }
        if (screamSfx == null)
        {
            screamSfx = Resources.Load<AudioClip>("Audio/SFX/Nivel1/llorona_nivel1.mpeg");
        }
        if (screamSfx == null)
        {
            screamSfx = Resources.Load<AudioClip>("Audio/SFX/Nivel1/llorona_nivel1.mpeg.wav");
        }

        if (audioSource != null && screamSfx != null)
        {
            audioSource.PlayOneShot(screamSfx, screamVolume);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(screamSfx ?? AudioManager.Instance.lloronaGrito, screamVolume);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.DuckMusic(5.0f, 0.25f);
        }
    }

    void MoveTo(Vector2 target, float speed)
    {
        Vector2 toTarget = target - (Vector2)transform.position;
        if (toTarget.sqrMagnitude < 0.04f)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = toTarget.normalized;

        // Desvío suave de obstáculos en caso de toparse con una esquina o barricada
        if (obstacleMask != 0)
        {
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.35f, dir, 0.7f, obstacleMask);
            if (hit.collider != null)
            {
                Vector2 leftDir = Quaternion.Euler(0, 0, 45f) * dir;
                Vector2 rightDir = Quaternion.Euler(0, 0, -45f) * dir;
                bool hitLeft = Physics2D.Raycast(transform.position, leftDir, 0.7f, obstacleMask);
                bool hitRight = Physics2D.Raycast(transform.position, rightDir, 0.7f, obstacleMask);

                if (!hitLeft) dir = leftDir;
                else if (!hitRight) dir = rightDir;
            }
        }

        if (rb != null) rb.linearVelocity = dir * speed;
        if (dir.sqrMagnitude > 0.01f) facing = dir;
    }

    bool CanSeePlayer()
    {
        if (state == State.Distracted) return false;
        if (player == null || player.IsHidden) return false;
        if (ShrineNiche.IsPositionSafe(player.transform.position)) return false;

        Vector2 to = player.transform.position - transform.position;
        float dist = to.magnitude;
        float range = player.IsRunning ? viewDistance * runNoiseMultiplier : viewDistance;
        float hearRange = player.IsRunning ? hearDistance * runNoiseMultiplier : hearDistance;

        if (dist > range) return false;
        if (dist > hearRange && Vector2.Angle(facing, to) > viewAngle * 0.5f) return false;

        return !Physics2D.Raycast(transform.position, to.normalized, dist, obstacleMask);
    }

    void OnCollisionEnter2D(Collision2D c) { CheckCatch(c.collider); }
    void OnCollisionStay2D(Collision2D c) { CheckCatch(c.collider); }
    void OnTriggerEnter2D(Collider2D c) { CheckCatch(c); }
    void OnTriggerStay2D(Collider2D c) { CheckCatch(c); }

    void CheckCatch(Collider2D col)
    {
        if (caught) return;
        if (col == null || !col.CompareTag("Player")) return;
        if (player != null && (player.IsHidden || ShrineNiche.IsPositionSafe(player.transform.position))) return;
        if (player != null && player.IsHidden) return;   // escondido: no lo atrapa

        caught = true;
        PlayScream();

        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerCaught();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Cono de visión en la ventana Scene (selecciona a la Llorona)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hearDistance);
        Vector3 l = Quaternion.Euler(0, 0, viewAngle * 0.5f) * facing;
        Vector3 r = Quaternion.Euler(0, 0, -viewAngle * 0.5f) * facing;
        Gizmos.DrawRay(transform.position, l * viewDistance);
        Gizmos.DrawRay(transform.position, r * viewDistance);
    }

    public static void OnShrineLitAggression()
    {
        var llorona = Object.FindAnyObjectByType<LloronaAI>();
        if (llorona != null)
        {
            llorona.patrolSpeed = Mathf.Min(3.0f, llorona.patrolSpeed + 0.3f);
            llorona.chaseSpeed = Mathf.Min(4.2f, llorona.chaseSpeed + 0.35f);
            if (llorona.audioSource != null && AudioManager.Instance != null && AudioManager.Instance.lloronaLlanto != null)
            {
                llorona.audioSource.PlayOneShot(AudioManager.Instance.lloronaLlanto, 0.9f);
            }
        }
    }

}
