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
        }

        // Carlos cuenta con sprites nativos para todas las direcciones (fila 2: izquierda, fila 3: derecha).
        // flipX debe permanecer siempre en false para no invertir ni quebrar la animación hacia la izquierda.
        if (sr != null && sr.flipX)
        {
            sr.flipX = false;
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
