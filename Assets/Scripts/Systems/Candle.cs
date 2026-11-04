// Assets/Scripts/Systems/Candle.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Vela coleccionable en el escenario.
/// Al acercarse y presionar [E] se recoge al inventario.
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

    bool playerNearby;

    void Update()
    {
        if (playerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectCandle();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null)
        {
            playerNearby = true;
            if (HUD.Instance != null)
                HUD.Instance.ShowEventNotice("[E] Recoger veladora", 2.0f);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null)
        {
            playerNearby = false;
        }
    }

    void CollectCandle()
    {
        playerNearby = false;
        PlayerCandlesCount++;
        OnCandlesCountChanged?.Invoke(PlayerCandlesCount);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCandleSound();

        if (HUD.Instance != null)
            HUD.Instance.ShowTutorial("¡Veladora recogida!\nPresiona [F] para orar, colocar la ofrenda y distraer a La Llorona.", 4.5f);

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

        // Sonido de encendido de vela
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

    public static void ResetInventory()
    {
        PlayerCandlesCount = 0;
        OnCandlesCountChanged?.Invoke(0);
    }
}
