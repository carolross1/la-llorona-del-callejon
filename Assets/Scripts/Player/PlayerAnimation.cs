// Assets/Scripts/Player/PlayerAnimation.cs
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rb;
    Vector2 lastDir = Vector2.down;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude > 0.01f)
        {
            lastDir = Mathf.Abs(v.x) > Mathf.Abs(v.y)
                ? new Vector2(Mathf.Sign(v.x), 0)
                : new Vector2(0, Mathf.Sign(v.y));
        }
        anim.SetFloat("MoveX", lastDir.x);
        anim.SetFloat("MoveY", lastDir.y);
        anim.SetFloat("Speed", v.magnitude);
    }
}