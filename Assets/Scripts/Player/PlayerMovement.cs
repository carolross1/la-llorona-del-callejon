using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float walkSpeed = 3f;
    [SerializeField] float runSpeed = 6f;
    Rigidbody2D rb;
    Vector2 input;

    public bool IsRunning { get; private set; }
    public bool IsHidden { get; set; }
    public float SpeedMultiplier { get; set; } = 1.0f;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            input = Vector2.zero;
            IsRunning = false;
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        input = new Vector2(
            (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1 : 0) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1 : 0),
            (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1 : 0) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1 : 0)
        ).normalized;
        IsRunning = kb.leftShiftKey.isPressed && input != Vector2.zero;
    }

    void FixedUpdate()
    {
        if (IsHidden || (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.linearVelocity = input * (IsRunning ? runSpeed : walkSpeed) * SpeedMultiplier;
    }
}
