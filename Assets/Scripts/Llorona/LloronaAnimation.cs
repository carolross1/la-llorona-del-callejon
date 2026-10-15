using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class LloronaAnimation : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rb;
    LloronaAI ai;
    Vector2 lastDir = Vector2.down;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        ai = GetComponent<LloronaAI>();

        if (ai == null)
            Debug.LogError("LloronaAnimation: falta el script LloronaAI en este objeto.", this);
        if (anim.runtimeAnimatorController == null)
            Debug.LogError("LloronaAnimation: el Animator no tiene Controller asignado.", this);
    }

    void Update()
    {
        if (ai == null || anim.runtimeAnimatorController == null) return;

        Vector2 f = ai.Facing;
        if (f.sqrMagnitude > 0.01f)
        {
            lastDir = Mathf.Abs(f.x) > Mathf.Abs(f.y)
                ? new Vector2(Mathf.Sign(f.x), 0)
                : new Vector2(0, Mathf.Sign(f.y));
        }

        anim.SetFloat("MoveX", lastDir.x);
        anim.SetFloat("MoveY", lastDir.y);
        anim.SetFloat("Speed", rb.linearVelocity.magnitude);
        anim.SetBool("IsChasing", ai.IsChasing);
    }
}