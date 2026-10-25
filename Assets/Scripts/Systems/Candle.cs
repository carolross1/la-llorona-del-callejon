// Assets/Scripts/Systems/Candle.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Vela coleccionable en el escenario.
/// Al acercarse y presionar E:
/// - Modo GDD literal: distrae inmediatamente a La Llorona por 3 segundos.
/// - Modo táctico (por defecto): se añade al inventario del jugador y se coloca con la tecla F.
/// </summary>
public class Candle : MonoBehaviour
{
    public static event Action<int> OnCandlesCountChanged;
    public static int PlayerCandlesCount { get; private set; }

    [Header("Comportamiento")]
    [Tooltip("Si está activo, distrae a La Llorona inmediatamente al recogerse (comportamiento literal del GDD). Si está apagado, se guarda en inventario y se coloca con F.")]
    [SerializeField] bool distractImmediatelyOnPickup = false;
    [SerializeField] float distractionDuration = 3.0f;

    [Header("Prefab de Ofrenda")]
    [SerializeField] GameObject offeringPrefab;

    [Header("Retroalimentación")]
    [SerializeField] ParticleSystem pickupParticles;

    bool playerNearby;

    void Update()
    {
        if (playerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectCandle();
        }

        // Si el jugador tiene velas y presiona F, coloca una ofrenda en su posición
        if (PlayerCandlesCount > 0 && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            PlaceOffering();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    void CollectCandle()
    {
        if (distractImmediatelyOnPickup)
        {
            // Comportamiento literal GDD
            LloronaAI llorona = FindAnyObjectByType<LloronaAI>();
            if (llorona != null)
                llorona.Distract(transform.position, distractionDuration);

            if (HUD.Instance != null)
                HUD.Instance.ShowTutorial("¡Has recogido una ofrenda! La Llorona se ha detenido por 3 segundos.", 3.5f);
        }
        else
        {
            // Comportamiento táctico
            PlayerCandlesCount++;
            OnCandlesCountChanged?.Invoke(PlayerCandlesCount);

            if (HUD.Instance != null)
                HUD.Instance.ShowTutorial("Vela recogida. Presiona F para colocarla y distraer a La Llorona.", 4f);
        }

        if (pickupParticles != null)
        {
            pickupParticles.transform.SetParent(null);
            pickupParticles.Play();
            Destroy(pickupParticles.gameObject, 1.5f);
        }

        gameObject.SetActive(false);
    }

    void PlaceOffering()
    {
        PlayerCandlesCount = Mathf.Max(0, PlayerCandlesCount - 1);
        OnCandlesCountChanged?.Invoke(PlayerCandlesCount);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 spawnPos = player != null ? player.transform.position : transform.position;

        if (offeringPrefab != null)
        {
            Instantiate(offeringPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Generar ofrenda procedural si no hay prefab asignado
            GameObject offGo = new GameObject("Ofrenda_Colocada");
            offGo.transform.position = spawnPos;

            SpriteRenderer sr = offGo.AddComponent<SpriteRenderer>();
            SpriteRenderer mySr = GetComponentInChildren<SpriteRenderer>();
            if (mySr != null) sr.sprite = mySr.sprite;
            sr.sortingOrder = 2;

            Light2D l2d = offGo.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Point;
            l2d.color = new Color(1.0f, 0.75f, 0.35f);
            l2d.intensity = 1.4f;
            l2d.pointLightOuterRadius = 3.5f;

            offGo.AddComponent<Offering>();
        }
    }

    public static void ResetInventory()
    {
        PlayerCandlesCount = 0;
        OnCandlesCountChanged?.Invoke(0);
    }
}
