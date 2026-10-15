using UnityEngine;
using UnityEngine.SceneManagement;

public class LloronaAI : MonoBehaviour
{
    enum State { Patrol, Wait, Chase }

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

    public Vector2 Facing => facing;
    public bool IsChasing => state == State.Chase;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = FindFirstObjectByType<PlayerMovement>();
    }

    void FixedUpdate()
    {
        if (player == null || caught) return;

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
                if (Vector2.Distance(transform.position, waypoints[idx].position) < 0.2f)
                {
                    state = State.Wait;
                    timer = waitTime;
                    rb.linearVelocity = Vector2.zero;
                }
                break;

            case State.Wait:
                rb.linearVelocity = Vector2.zero;
                if (sees) { StartChase(); break; }

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

                if (Vector2.Distance(transform.position, lastSeenPos) > 0.2f)
                    MoveTo(lastSeenPos, chaseSpeed);
                else
                    rb.linearVelocity = Vector2.zero;

                if (timer <= 0) state = State.Patrol;
                break;
        }
    }

    void StartChase()
    {
        state = State.Chase;
        timer = loseSightTime;
        lastSeenPos = player.transform.position;
    }

    void MoveTo(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * speed;
        if (dir.sqrMagnitude > 0.01f) facing = dir;
    }

    bool CanSeePlayer()
    {
        if (player.IsHidden) return false;

        Vector2 to = player.transform.position - transform.position;
        float dist = to.magnitude;
        float range = player.IsRunning ? viewDistance * runNoiseMultiplier : viewDistance;

        if (dist > range) return false;
        if (dist > hearDistance && Vector2.Angle(facing, to) > viewAngle * 0.5f) return false;

        return !Physics2D.Raycast(transform.position, to.normalized, dist, obstacleMask);
    }

    void OnCollisionEnter2D(Collision2D c) { CheckCatch(c); }
    void OnCollisionStay2D(Collision2D c) { CheckCatch(c); }

    void CheckCatch(Collision2D c)
    {
        if (caught) return;
        if (!c.collider.CompareTag("Player")) return;
        if (player.IsHidden) return;   // escondido: no lo atrapa

        caught = true;
        // Temporal: cuando exista el GameManager aquí se pierde una vida
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