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

    [Header("Indicador de Interacción")]
    [SerializeField] GameObject promptObject;

    bool isLit = false;
    public bool IsLit => isLit;
    public string ShrineName => shrineName;
    public float SanctuaryRadius => sanctuaryRadius;

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
        UpdateVisuals();
    }

    void Update()
    {
        if (isLit) return;

        // Mostrar prompt si Carlos está cerca
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && promptObject != null)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            bool inRange = dist <= 2.2f;
            if (promptObject.activeSelf != inRange)
                promptObject.SetActive(inRange);
        }
    }

    public bool LightShrine()
    {
        if (isLit) return false;
        if (!Candle.ConsumeCandle()) return false;

        isLit = true;
        if (promptObject != null) promptObject.SetActive(false);

        UpdateVisuals();

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
    }

    /// <summary>
    /// Intenta encender el nicho más cercano a la posición del jugador.
    /// </summary>
    public static bool TryLightNearestShrine(Vector3 playerPos, float maxDist = 2.4f)
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
