// Assets/Scripts/Player/PlayerHolyWaterLauncher.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Sistema de combate de Carlos para el Boss Fight final (Nivel 4):
/// - Permite a Carlos apuntar y lanzar Jarros de Agua Bendita contra La Llorona.
/// - Controles PC: Clic Izquierdo (apunta con el cursor hacia La Llorona) o Tecla F.
/// - Controles Móvil: Tocar directamente en el escenario hacia La Llorona (ignorando el joystick/botones UI) o botón [AGUA BENDITA].
/// - Administra la munición de jarros y notifica a la interfaz de usuario.
/// </summary>
public class PlayerHolyWaterLauncher : MonoBehaviour
{
    public static event Action<int, int> OnJarsChanged;

    [Header("Munición")]
    [SerializeField] int maxJars = 5;
    [SerializeField] int currentJars = 5;
    [SerializeField] float fireCooldown = 0.45f;

    [Header("Referencias Visuales y Audio")]
    [SerializeField] Sprite jarSprite;
    [SerializeField] AudioClip throwSfx;
    [SerializeField] AudioClip impactSfx;
    [SerializeField] AudioClip emptySfx;

    float lastFireTime = -99f;
    PlayerMovement playerMovement;
    Collider2D playerCollider;
    Vector2 lastAimDir = Vector2.right;

    public int CurrentJars => currentJars;
    public int MaxJars => maxJars;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCollider = GetComponent<Collider2D>();
        currentJars = maxJars;
    }

    void Start()
    {
        OnJarsChanged?.Invoke(currentJars, maxJars);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (playerMovement != null && playerMovement.IsHidden) return;

        if (playerMovement != null && playerMovement.MoveInput.sqrMagnitude > 0.05f)
        {
            lastAimDir = playerMovement.MoveInput.normalized;
        }

        // 1. Disparo táctil multitouch en pantalla (tocar el escenario fuera del joystick y botones UI)
        bool handledTouchAim = false;
        var ts = Touchscreen.current;
        if (ts != null)
        {
            foreach (var touch in ts.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    Vector2 touchPos = touch.position.ReadValue();
                    int touchId = touch.touchId.ReadValue();
                    if (!MobileInputManager.IsScreenPointOverUI(touchPos, touchId))
                    {
                        ThrowTowardsScreenPoint(touchPos);
                        handledTouchAim = true;
                        break;
                    }
                }
            }
        }

        if (handledTouchAim) return;

        // 2. Disparo con Clic Izquierdo en PC (ignorando clics sobre botones de pausa o controles UI)
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && (ts == null || !ts.primaryTouch.press.isPressed))
        {
            Vector2 mouseScreenPos = mouse.position.ReadValue();
            if (!MobileInputManager.IsScreenPointOverUI(mouseScreenPos))
            {
                ThrowTowardsScreenPoint(mouseScreenPos);
                return;
            }
        }

        // 3. Disparo con tecla [F] o botón táctil [AGUA BENDITA]
        var kb = Keyboard.current;
        bool fPressed = (kb != null && kb.fKey.wasPressedThisFrame) || MobileInputManager.ActionFPressedThisFrame;
        if (fPressed)
        {
            Vector2 fireDir = GetSmartAutoAimDirection();
            TryThrowJar(fireDir);
        }
    }

    void ThrowTowardsScreenPoint(Vector2 screenPos)
    {
        Vector3 worldPoint = transform.position + (Vector3)lastAimDir;
        if (Camera.main != null)
        {
            float camDist = Mathf.Abs(Camera.main.transform.position.z);
            if (camDist < 0.1f) camDist = 10f;
            worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camDist));
        }

        Vector2 fireDir = new Vector2(worldPoint.x - transform.position.x, worldPoint.y - transform.position.y);
        if (fireDir.sqrMagnitude < 0.001f)
        {
            fireDir = lastAimDir;
        }
        TryThrowJar(fireDir.normalized);
    }

    Vector2 GetSmartAutoAimDirection()
    {
        // En móvil o con botón rápido, si La Llorona Boss está cerca y visible, auto-apuntar hacia ella facilita el combate táctil
        if (MobileInputManager.ShouldShowTouchControls && LloronaBossAI.Instance != null && LloronaBossAI.Instance.gameObject.activeInHierarchy)
        {
            Vector2 toBoss = LloronaBossAI.Instance.transform.position - transform.position;
            if (toBoss.sqrMagnitude > 0.01f && toBoss.magnitude <= 14f)
            {
                return toBoss.normalized;
            }
        }

        if (playerMovement != null && playerMovement.MoveInput.sqrMagnitude > 0.05f)
            return playerMovement.MoveInput.normalized;

        return lastAimDir.sqrMagnitude > 0.01f ? lastAimDir.normalized : Vector2.right;
    }

    public bool TryThrowJar(Vector2 direction)
    {
        if (Time.time - lastFireTime < fireCooldown) return false;

        if (currentJars <= 0)
        {
            if (emptySfx != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(emptySfx, 0.7f);

            if (HUD.Instance != null)
                HUD.Instance.ShowEventNotice("¡Sin Agua Bendita! Acércate a una Pila Bendita y presiona [E].", 2.2f);

            return false;
        }

        currentJars--;
        lastFireTime = Time.time;
        OnJarsChanged?.Invoke(currentJars, maxJars);

        // Sonido de lanzamiento
        if (throwSfx != null)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(throwSfx, 0.9f);
            else AudioSource.PlayClipAtPoint(throwSfx, transform.position, 0.9f);
        }

        // Crear proyectil
        SpawnProjectile(direction);

        return true;
    }

    void SpawnProjectile(Vector2 direction)
    {
        Vector3 spawnPos = transform.position + (Vector3)(direction.normalized * 0.55f);
        GameObject projGo = new GameObject("HolyWater_Projectile");
        projGo.transform.position = spawnPos;
        projGo.transform.localScale = new Vector3(0.75f, 0.75f, 1f);

        // Sprite del Jarro / Botella
        SpriteRenderer sr = projGo.AddComponent<SpriteRenderer>();
        sr.sprite = jarSprite;
        sr.sortingLayerName = "Characters";
        sr.sortingOrder = 15;

        // Luz de agua bendita
        Light2D l2d = projGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.25f, 0.88f, 1.0f);
        l2d.intensity = 2.2f;
        l2d.pointLightOuterRadius = 3.5f;

        // Collider Trigger
        CircleCollider2D cc = projGo.AddComponent<CircleCollider2D>();
        cc.radius = 0.45f;
        cc.isTrigger = true;

        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(cc, playerCollider, true);
        }

        // Rigidbody2D
        Rigidbody2D rb = projGo.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Componente Proyectil
        HolyWaterProjectile proj = projGo.AddComponent<HolyWaterProjectile>();
        proj.Launch(direction, impactSfx, impactSfx);
    }

    public void RefillJars()
    {
        currentJars = maxJars;
        OnJarsChanged?.Invoke(currentJars, maxJars);
    }

    public void Configure(Sprite sprite, AudioClip throwClip, AudioClip impactClip, AudioClip emptyClip)
    {
        jarSprite = sprite;
        throwSfx = throwClip;
        impactSfx = impactClip;
        emptySfx = emptyClip;
    }
}
