// Assets/Scripts/Systems/ShrineNiche.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Nicho u ofrenda sagrada en el Atrio de Dolores.
/// El jugador debe encender los 3 nichos usando una veladora [F] para romper el sello
/// y abrir las puertas de la Parroquia.
/// Cada nicho encendido purifica el área, creando una zona segura donde La Llorona no puede entrar.
/// Utiliza TextMesh nativo de Unity para garantizar cero dependencias de paquetes externos.
/// </summary>
public class ShrineNiche : MonoBehaviour
{
    public static readonly List<ShrineNiche> AllShrines = new List<ShrineNiche>();

    [Header("Identificación")]
    [SerializeField] string shrineName = "Nicho Sagrado";
    [SerializeField] float sanctuaryRadius = 3.2f;

    [Header("Efectos Visuales")]
    [SerializeField] GameObject flameVisual;
    [SerializeField] Light2D shrineLight;
    [SerializeField] Light2D beaconLight; // Luz guía etérea visible cuando aún no está consagrado
    [SerializeField] GameObject sanctuaryDome; // Círculo/aura dorada de santuario

    [Header("Indicador de Interacción")]
    [SerializeField] GameObject promptObject;
    [SerializeField] TextMesh promptTextMesh;

    [Header("Efectos de Audio")]
    [SerializeField] AudioClip lightSound;
    [SerializeField] AudioClip blessingSound;

    bool isLit = false;
    public bool IsLit => isLit;
    public string ShrineName => shrineName;
    public float SanctuaryRadius => sanctuaryRadius;

    Transform playerTransform;

    void OnEnable()
    {
        if (!AllShrines.Contains(this))
            AllShrines.Add(this);
    }

    void OnDisable()
    {
        AllShrines.Remove(this);
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        UpdateVisuals();
    }

    void Update()
    {
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (!isLit)
        {
            // Pulso suave en el faro guía cuando no está encendido
            if (beaconLight != null)
            {
                beaconLight.intensity = 1.2f + Mathf.PingPong(Time.time * 1.5f, 0.8f);
            }

            if (promptObject != null)
            {
                // Visible hasta 6.5 metros para guiar claramente al jugador
                bool inRange = dist <= 6.5f;
                if (promptObject.activeSelf != inRange)
                    promptObject.SetActive(inRange);

                if (inRange && promptTextMesh != null)
                {
                    if (dist <= 2.4f)
                    {
                        if (Candle.PlayerCandlesCount > 0)
                        {
                            promptTextMesh.text = $"[F] Consagrar {shrineName}\n(Ofrendar veladora)";
                            promptTextMesh.color = new Color(1.0f, 0.95f, 0.5f);
                        }
                        else
                        {
                            promptTextMesh.text = $"{shrineName}\n(¡Necesitas 1 veladora!)";
                            promptTextMesh.color = new Color(1.0f, 0.6f, 0.6f);
                        }
                    }
                    else
                    {
                        promptTextMesh.text = $"✦ {shrineName} ✦";
                        promptTextMesh.color = new Color(0.85f, 0.92f, 1.0f);
                    }
                }
            }
        }
        else
        {
            // Ya encendido: mostrar zona segura si el jugador está dentro
            if (promptObject != null)
            {
                bool insideSanctuary = dist <= sanctuaryRadius;
                if (promptObject.activeSelf != insideSanctuary)
                    promptObject.SetActive(insideSanctuary);

                if (insideSanctuary && promptTextMesh != null)
                {
                    promptTextMesh.text = $"✓ {shrineName}\nSantuario (Zona Segura)";
                    promptTextMesh.color = new Color(0.9f, 1.0f, 0.8f);
                }
            }
        }
    }

    public bool LightShrine()
    {
        if (isLit) return false;
        if (!Candle.ConsumeCandle()) return false;

        isLit = true;

        UpdateVisuals();

        // Reproducir sonido sagrado de encendido y consagración
        if (AudioManager.Instance != null)
        {
            if (lightSound != null) AudioManager.Instance.PlaySFX(lightSound, 1.0f);
            if (blessingSound != null) AudioManager.Instance.PlaySFX(blessingSound, 1.0f);
        }

        // Notificar al controlador de puertas de la Parroquia
        if (ParroquiaDoorController.Instance != null)
        {
            ParroquiaDoorController.Instance.OnShrineLit(this);
        }

        // Aumentar la agresión de La Llorona
        LloronaAI.OnShrineLitAggression();

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice($"¡{shrineName} consagrado!\nSe ha purificado una zona segura del atrio.", 4.5f);
        }

        return true;
    }

    void UpdateVisuals()
    {
        if (flameVisual != null) flameVisual.SetActive(isLit);
        if (shrineLight != null) shrineLight.enabled = isLit;
        if (beaconLight != null) beaconLight.enabled = !isLit;
        if (sanctuaryDome != null) sanctuaryDome.SetActive(isLit);
    }

    /// <summary>
    /// Intenta encender el nicho más cercano a la posición del jugador.
    /// </summary>
    public static bool TryLightNearestShrine(Vector3 playerPos, float maxDist = 2.6f)
    {
        ShrineNiche nearest = null;
        float minDist = maxDist;

        foreach (var shrine in AllShrines)
        {
            if (shrine == null || shrine.IsLit) continue;
            float dist = Vector2.Distance(playerPos, shrine.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = shrine;
            }
        }

        if (nearest != null)
        {
            return nearest.LightShrine();
        }

        return false;
    }

    /// <summary>
    /// Determina si el jugador está a corta distancia de algún nicho sagrado que aún no esté consagrado.
    /// </summary>
    public static bool IsPlayerNearAnyUnlitShrine(Vector3 playerPos, float maxDist = 3.2f)
    {
        foreach (var s in AllShrines)
        {
            if (s != null && !s.IsLit)
            {
                if (Vector2.Distance(playerPos, s.transform.position) <= maxDist)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Verifica si una posición se encuentra dentro de la zona sagrada de algún nicho encendido.
    /// </summary>
    public static bool IsPositionSafe(Vector3 position)
    {
        foreach (var shrine in AllShrines)
        {
            if (shrine != null && shrine.IsLit)
            {
                if (Vector2.Distance(position, shrine.transform.position) <= shrine.SanctuaryRadius)
                    return true;
            }
        }
        return false;
    }

    public static int GetLitCount()
    {
        int count = 0;
        foreach (var s in AllShrines)
        {
            if (s != null && s.IsLit) count++;
        }
        return count;
    }

    public static void ResetAll()
    {
        AllShrines.Clear();
    }
}
