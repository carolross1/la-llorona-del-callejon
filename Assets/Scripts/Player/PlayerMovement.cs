using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float walkSpeed = 3.5f;
    [SerializeField] float runSpeed = 6.0f;
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDrainRate = 25f;
    [SerializeField] float staminaRegenRate = 20f;

    Rigidbody2D rb;
    Vector2 input;
    float currentStamina;

    public bool IsRunning { get; private set; }
    public bool IsHidden { get; set; }
    public float SpeedMultiplier { get; set; } = 1.0f;
    public Vector2 MoveInput => input;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaNormalized => maxStamina > 0f ? currentStamina / maxStamina : 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
        }
        transform.rotation = Quaternion.identity;
        currentStamina = maxStamina;
    }

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

        bool wantsToRun = kb.leftShiftKey.isPressed && input != Vector2.zero;

        if (wantsToRun && currentStamina > 0f)
        {
            IsRunning = true;
            currentStamina = Mathf.Max(0f, currentStamina - staminaDrainRate * Time.deltaTime);
        }
        else
        {
            IsRunning = false;
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
        }

        if (HUD.Instance != null)
        {
            HUD.Instance.UpdateStamina(StaminaNormalized);
        }
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
