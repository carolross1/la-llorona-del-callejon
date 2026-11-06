using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float walkSpeed = 3.5f;
    [SerializeField] float runSpeed = 6.0f;
    [Header("Estamina")]
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDrainRate = 25f;
    [SerializeField] float staminaRegenRate = 20f;

    [Header("Salto / Brinco")]
    [SerializeField] float jumpDuration = 0.45f;
    [SerializeField] float jumpHeight = 1.2f;
    [SerializeField] float jumpStaminaCost = 5f;

    Rigidbody2D rb;
    Collider2D playerCollider;
    Vector2 input;
    float currentStamina;
    float jumpTimer;

    public static bool IsAutoRunnerMode = false;

    public bool IsRunning { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsHidden { get; set; }
    public bool CanRun { get; set; } = true;
    public float SpeedMultiplier { get; set; } = 1.0f;
    public Vector2 MoveInput => input;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaNormalized => maxStamina > 0f ? currentStamina / maxStamina : 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Material fisico sin friccion para permitir deslizamiento suave en paredes y esquinas
            PhysicsMaterial2D frictionless = new PhysicsMaterial2D("PlayerFrictionless")
            {
                friction = 0f,
                bounciness = 0f
            };
            rb.sharedMaterial = frictionless;
        }

        // Colisionador ajustado a los pies para permitir movimiento libre en perspectiva 2D
        if (playerCollider is CircleCollider2D circle)
        {
            circle.radius = 0.28f;
            circle.offset = new Vector2(0f, -0.45f);
        }

        transform.rotation = Quaternion.identity;
        currentStamina = maxStamina;
    }

    void OnDisable()
    {
        IsAutoRunnerMode = false;
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

        bool wantsToRun = CanRun && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed) && input != Vector2.zero;

        // Salto con Barra Espaciadora [Espacio]
        if (kb.spaceKey.wasPressedThisFrame && !IsJumping && !IsHidden && currentStamina >= jumpStaminaCost)
        {
            currentStamina = Mathf.Max(0f, currentStamina - jumpStaminaCost);
            StartJump();
        }

        // Ofrenda / Orar con [F] (Prioriza consagrar nicho sagrado si está cerca, o colocar ofrenda en el suelo)
        if (kb.fKey.wasPressedThisFrame && Candle.PlayerCandlesCount > 0 && !IsHidden)
        {
            if (!ShrineNiche.TryLightNearestShrine(transform.position))
            {
                Candle.TryPlaceOffering(transform.position);
            }
        }

        if (IsJumping)
        {
            jumpTimer += Time.deltaTime;
            if (jumpTimer >= jumpDuration)
            {
                EndJump();
            }
        }

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

    public void DrainStamina(float amount)
    {
        currentStamina = Mathf.Max(0f, currentStamina - amount);
        if (HUD.Instance != null)
        {
            HUD.Instance.UpdateStamina(StaminaNormalized);
        }
    }

    void StartJump()
    {
        IsJumping = true;
        jumpTimer = 0f;
        if (playerCollider != null)
        {
            playerCollider.isTrigger = true;
        }
    }

    void EndJump()
    {
        IsJumping = false;
        if (playerCollider != null)
        {
            playerCollider.isTrigger = false;
        }
    }

    void FixedUpdate()
    {
        if (IsHidden || (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float speed = IsJumping ? runSpeed * 1.15f : (IsRunning ? runSpeed : walkSpeed);
        Vector2 vel = input * speed * SpeedMultiplier;

        // Impulso y arco vertical durante el salto
        if (IsJumping)
        {
            float arc = Mathf.Sin((jumpTimer / jumpDuration) * Mathf.PI);
            vel += Vector2.up * (arc * jumpHeight * 2.2f);
        }

        rb.linearVelocity = vel;
    }
}
