// Assets/Scripts/Player/PlayerHolyWaterLauncher.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Sistema de combate de Carlos para el Boss Fight final (Nivel 4):
/// - Permite a Carlos apuntar y lanzar Jarros de Agua Bendita contra La Llorona.
/// - Controles: Clic Izquierdo (apunta con el cursor hacia La Llorona) o Tecla F (hacia donde mira/camina Carlos).
/// - Administra la munición de jarros y notifica a la interfaz de usuario.
/// </summary>
public class PlayerHolyWaterLauncher : MonoBehaviour
{
    public static event Action<int, int> OnJarsChanged;

    [Header("Munición")]
    [SerializeField] int maxJars = 3;
    [SerializeField] int currentJars = 3;
    [SerializeField] float fireCooldown = 0.5f;

    [Header("Referencias Visuales y Audio")]
    [SerializeField] Sprite jarSprite;
    [SerializeField] AudioClip throwSfx;
    [SerializeField] AudioClip impactSfx;
    [SerializeField] AudioClip emptySfx;

    float lastFireTime = -99f;
    PlayerMovement playerMovement;
    Collider2D playerCollider;

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
        if (playerMovement != null && playerMovement.IsHidden) return;

        // Disparo con Clic Izquierdo (Apunta con el cursor del mouse)
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPos = mouse.position.ReadValue();
            Vector3 mouseWorld = transform.position + (transform.localScale.x < 0 ? Vector3.left : Vector3.right);

            if (Camera.main != null)
            {
                float camDist = Mathf.Abs(Camera.main.transform.position.z);
                if (camDist < 0.1f) camDist = 10f;
                mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, camDist));
            }

            Vector2 fireDir = new Vector2(mouseWorld.x - transform.position.x, mouseWorld.y - transform.position.y);
            if (fireDir.sqrMagnitude < 0.001f)
            {
                fireDir = (transform.localScale.x < 0) ? Vector2.left : Vector2.right;
            }
            TryThrowJar(fireDir.normalized);
        }
        // Disparo con tecla F (En la dirección de movimiento o vista de Carlos)
        else
        {
            var kb = Keyboard.current;
            if (kb != null && kb.fKey.wasPressedThisFrame)
            {
                Vector2 fireDir = (playerMovement != null && playerMovement.MoveInput.sqrMagnitude > 0.05f)
                    ? playerMovement.MoveInput.normalized 
                    : (transform.localScale.x < 0 ? Vector2.left : Vector2.right);
                TryThrowJar(fireDir);
            }
        }
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
