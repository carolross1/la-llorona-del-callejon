// Assets/Scripts/Player/PlayerAnimation.cs
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rb;
    SpriteRenderer sr;
    PlayerMovement movement;
    Vector2 lastDir = Vector2.down;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        Vector2 inputDir = (movement != null) ? movement.MoveInput : Vector2.zero;
        Vector2 vel = (rb != null) ? rb.linearVelocity : Vector2.zero;

        bool isMoving = inputDir.sqrMagnitude > 0.01f || vel.sqrMagnitude > 0.01f;
        Vector2 activeDir = (inputDir.sqrMagnitude > 0.01f) ? inputDir : vel;

        if (isMoving)
        {
            lastDir = Mathf.Abs(activeDir.x) > Mathf.Abs(activeDir.y)
                ? new Vector2(Mathf.Sign(activeDir.x), 0)
                : new Vector2(0, Mathf.Sign(activeDir.y));

            if (sr != null)
            {
                if (Mathf.Abs(activeDir.x) > 0.1f)
                    sr.flipX = (activeDir.x < 0);
                else
                    sr.flipX = false;
            }
        }

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            float speed = isMoving ? Mathf.Max(vel.magnitude, 1.0f) : 0f;

            anim.SetFloat("MoveX", lastDir.x);
            anim.SetFloat("MoveY", lastDir.y);
            anim.SetFloat("Speed", speed);
        }
    }
}
