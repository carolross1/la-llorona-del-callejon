using UnityEngine;
using UnityEngine.SceneManagement;

public class LloronaAI : MonoBehaviour
{
    enum State { Patrol, Wait, Chase, Distracted }

    [Header("Ajustes de Dificultad (ScriptableObject)")]
    [SerializeField] LloronaSettings settings;

    [Header("Patrulla")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float patrolSpeed = 1.5f;
    [SerializeField] float waitTime = 2f;

    [Header("Persecución")]
    [SerializeField] float chaseSpeed = 2.8f;
    [SerializeField] float loseSightTime = 3f;

    [Header("Detección")]
    [SerializeField] float viewDistance = 5f;
    [SerializeField] float viewAngle = 100f;
    [SerializeField] float hearDistance = 1.5f;
    [SerializeField] float runNoiseMultiplier = 1.5f;
    [SerializeField] LayerMask obstacleMask;   // Layer Walls

    Rigidbody2D rb;
    PlayerMovement player;
    State state = State.Patrol;
    int idx;
    float timer;
    bool caught;
    Vector2 facing = Vector2.down;
    Vector2 lastSeenPos;
    Vector2 distractionPos;

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
        player = FindAnyObjectByType<PlayerMovement>();
        ApplySettings();
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

    void FixedUpdate()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerMovement>();
            if (player == null) return;
        }
        if (caught) return;

        bool sees = CanSeePlayer();

        switch (state)
        {
            case State.Patrol:
                if (sees) { StartChase(); break; }

                if (waypoints == null || waypoints.Length == 0)
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
                    Vector2 nextDir = (waypoints[nextIdx].position - transform.position).normalized;
                    if (nextDir.sqrMagnitude > 0.01f) facing = nextDir;
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
        }
    }

    /// <summary>
    /// Distrae a La Llorona hacia una ofrenda o posición específica durante varios segundos.
    /// Durante la distracción no persigue a Carlos.
    /// </summary>
    public void Distract(Vector2 targetPos, float duration = 3.0f)
    {
        state = State.Distracted;
        timer = duration;
        distractionPos = targetPos;
    }

    /// <summary>
    /// Distrae a La Llorona deteniéndola en su posición actual durante varios segundos.
    /// </summary>
    public void Distract(float duration = 3.0f)
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
        if (player != null && player.IsHidden) return;   // escondido: no lo atrapa

        caught = true;
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
}
