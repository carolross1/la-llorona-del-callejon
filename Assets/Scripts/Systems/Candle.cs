// Assets/Scripts/Systems/Candle.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Vela coleccionable en el escenario.
/// Se recoge automáticamente al caminar sobre ella o acercarse y presionar [E].
/// Con la tecla [F] Carlos ora y coloca una ofrenda sagrada en el suelo,
/// la cual distrae y pacifica a La Llorona durante varios segundos.
/// </summary>
public class Candle : MonoBehaviour
{
    public static event Action<int> OnCandlesCountChanged;
    public static int PlayerCandlesCount { get; private set; }
    public const float OfferingDistractionDuration = 7.0f;

    [Header("Retroalimentación")]
    [SerializeField] ParticleSystem pickupParticles;

    PlayerMovement player;
    bool isCollected;

    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
        EnsureTriggerCollider();
    }

    void EnsureTriggerCollider()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.3f;
        col.offset = Vector2.zero;
    }

    void Update()
    {
        if (isCollected) return;

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerMovement>();
            if (player == null) return;
        }

        float dist = Vector2.Distance(transform.position, player.transform.position);

        // 1. Recogida automática al caminar sobre la vela (muy cómodo y sin fallas)
        if (dist <= 1.35f)
        {
            CollectCandle();
            return;
        }

        // 2. O recogida presionando [E] al estar a distancia media
        if (dist <= 2.2f && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectCandle();
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null)
        {
            CollectCandle();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null)
        {
            CollectCandle();
        }
    }

    void CollectCandle()
    {
        if (isCollected) return;
        isCollected = true;

        PlayerCandlesCount++;
        OnCandlesCountChanged?.Invoke(PlayerCandlesCount);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCandleSound();

        if (HUD.Instance != null)
            HUD.Instance.ShowEventNotice("¡Veladora recogida!\nPresiona [F] para orar y distraer a La Llorona.", 4.0f);

        if (pickupParticles != null)
        {
            pickupParticles.transform.SetParent(null);
            pickupParticles.Play();
            Destroy(pickupParticles.gameObject, 1.5f);
        }

        gameObject.SetActive(false);
    }

    public static bool TryPlaceOffering(Vector3 spawnPos)
    {
        if (PlayerCandlesCount <= 0) return false;

        PlayerCandlesCount--;
        OnCandlesCountChanged?.Invoke(PlayerCandlesCount);

        // Sonido sagrado de encendido de vela
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCandleSound();

        // 1. Instanciar objeto de ofrenda en el suelo
        GameObject offGo = new GameObject("Ofrenda_Sagrada");
        offGo.transform.position = spawnPos;

        SpriteRenderer sr = offGo.AddComponent<SpriteRenderer>();
        // Intentar usar sprite oficial de veladora roja
        sr.sprite = Resources.Load<Sprite>("Vaso_Vela_Roja_1");
        if (sr.sprite == null)
        {
            var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (var s in allSprites)
            {
                if (s.name.Contains("Vaso_Vela_Roja") || s.name.Contains("Vela"))
                {
                    sr.sprite = s;
                    break;
                }
            }
        }
        sr.sortingOrder = 3;

        // Luz cálida radiante para la ofrenda
        GameObject lGo = new GameObject("Luz_Ofrenda");
        lGo.transform.SetParent(offGo.transform, false);
        lGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        Light2D l2d = lGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.65f, 0.25f);
        l2d.intensity = 1.6f;
        l2d.pointLightOuterRadius = 5.0f;
        lGo.AddComponent<LightFlicker>();

        offGo.AddComponent<Offering>();

        // 2. Distraer inmediatamente a La Llorona en todo el atrio
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona != null)
        {
            llorona.Distract(spawnPos, OfferingDistractionDuration);
        }

        // 3. Notificación HUD en pantalla
        if (HUD.Instance != null)
        {
            HUD.Instance.SetEyeAlert(false);
            HUD.Instance.ShowEventNotice("¡Has colocado una ofrenda sagrada! La Llorona se detiene a rezar.", 4.0f);
        }

        return true;
    }

    public static bool ConsumeCandle()
    {
        if (PlayerCandlesCount <= 0) return false;
        PlayerCandlesCount--;
        OnCandlesCountChanged?.Invoke(PlayerCandlesCount);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCandleSound();
        return true;
    }

    public static void ResetInventory()
    {
        PlayerCandlesCount = 0;
        OnCandlesCountChanged?.Invoke(0);
    }
}
