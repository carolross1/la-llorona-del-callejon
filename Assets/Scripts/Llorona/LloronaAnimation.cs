// Assets/Scripts/Llorona/LloronaAnimation.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LloronaAnimation : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rb;
    SpriteRenderer sr;
    LloronaAI ai;
    Vector2 lastDir = Vector2.down;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ai = GetComponent<LloronaAI>();
    }

    void Update()
    {
        if (ai == null || rb == null) return;

        Vector2 f = ai.Facing;
        if (f.sqrMagnitude > 0.01f)
        {
            lastDir = Mathf.Abs(f.x) > Mathf.Abs(f.y)
                ? new Vector2(Mathf.Sign(f.x), 0)
                : new Vector2(0, Mathf.Sign(f.y));

            if (sr != null && Mathf.Abs(f.x) > 0.1f)
            {
                sr.flipX = (f.x < 0);
            }
        }

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.SetFloat("MoveX", lastDir.x);
            anim.SetFloat("MoveY", lastDir.y);
            anim.SetFloat("Speed", rb.linearVelocity.magnitude);
            anim.SetBool("IsChasing", ai.IsChasing);
        }
    }
}
