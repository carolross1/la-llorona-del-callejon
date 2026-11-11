// Assets/Scripts/Systems/HolyWaterProjectile.cs
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Proyectil de Agua Bendita / Fuego Sagrado lanzado por Carlos en el enfrentamiento final:
/// - Vuela en la dirección indicada girando suavemente con detección física continua.
/// - Chequeo activo por esfera de impacto (garantiza detección infalible contra La Llorona).
/// - Vuela por encima de tumbas bajas y solo se rompe al impactar muros o a La Llorona.
/// </summary>
public class HolyWaterProjectile : MonoBehaviour
{
    [SerializeField] float speed = 11.5f;
    [SerializeField] float lifeTime = 1.8f;
    [SerializeField] AudioClip impactSfx;
    [SerializeField] AudioClip breakSfx;

    Vector2 direction = Vector2.right;
    Rigidbody2D rb;
    CircleCollider2D col;
    bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    public void Launch(Vector2 dir, AudioClip impactClip, AudioClip breakClip)
    {
        direction = (dir.sqrMagnitude > 0.001f) ? dir.normalized : Vector2.right;
        impactSfx = impactClip;
        breakSfx = breakClip;

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Rotación visual al volar
        transform.Rotate(0f, 0f, -540f * Time.deltaTime);

        // Si por alguna razón la velocidad física no estuviera activa, asegurar avance:
        if (rb != null && rb.linearVelocity.sqrMagnitude < 1f)
        {
            rb.linearVelocity = direction * speed;
        }

        // Chequeo activo de proximidad (Esfera sagrada de 0.85m contra La Llorona)
        CheckActiveProximityHit();
    }

    void CheckActiveProximityHit()
    {
        if (hasHit) return;

        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 0.85f);
        for (int i = 0; i < nearby.Length; i++)
        {
            Collider2D other = nearby[i];
            if (other == null || other.gameObject == gameObject) continue;
            if (other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null) continue;

            LloronaBossAI boss = other.GetComponent<LloronaBossAI>() ?? other.GetComponentInParent<LloronaBossAI>();
            if (boss == null)
            {
                boss = FindAnyObjectByType<LloronaBossAI>();
                if (boss != null)
                {
                    float dist = Vector2.Distance(transform.position, boss.transform.position);
                    if (dist > 1.1f) boss = null;
                }
            }

            if (boss != null)
            {
                HitBoss(boss);
                return;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (other == null || other.gameObject == gameObject) return;
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null) return;

        // 1. Impacto directo con La Llorona (Boss)
        LloronaBossAI boss = other.GetComponent<LloronaBossAI>() ?? other.GetComponentInParent<LloronaBossAI>();
        if (boss != null)
        {
            HitBoss(boss);
            return;
        }

        // 2. Muros perimetrales
        int wallsLayer = LayerMask.NameToLayer("Walls");
        if (wallsLayer >= 0 && other.gameObject.layer == wallsLayer)
        {
            hasHit = true;
            PlaySound(breakSfx, 0.8f);
            SpawnHolySplashVisual();
            Destroy(gameObject);
        }
    }

    void HitBoss(LloronaBossAI boss)
    {
        hasHit = true;
        boss.TakeDamage(1);
        PlaySound(impactSfx, 1.0f);
        SpawnHolySplashVisual();
        Destroy(gameObject);
    }

    void SpawnHolySplashVisual()
    {
        GameObject splashObj = new GameObject("HolySplash_FX");
        splashObj.transform.position = transform.position;

        Light2D l2d = splashObj.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.35f, 0.90f, 1.0f);
        l2d.intensity = 3.5f;
        l2d.pointLightOuterRadius = 3.5f;

        Destroy(splashObj, 0.45f);
    }

    void PlaySound(AudioClip clip, float vol)
    {
        if (clip == null) return;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip, vol);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, vol);
        }
    }
}
