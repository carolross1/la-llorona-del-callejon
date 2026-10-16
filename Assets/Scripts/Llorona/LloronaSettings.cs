// Assets/Scripts/Llorona/LloronaSettings.cs
using UnityEngine;

/// <summary>
/// ScriptableObject que define la curva de dificultad progresiva de La Llorona para cada nivel.
/// Configura velocidades, distancias de visión, ángulos de detección y tiempos de espera.
/// </summary>
[CreateAssetMenu(fileName = "LloronaSettings_Nivel", menuName = "La Llorona/Llorona Settings", order = 1)]
public class LloronaSettings : ScriptableObject
{
    [Header("Patrulla")]
    [Tooltip("Velocidad de movimiento en estado de patrulla (u/s)")]
    [SerializeField] float patrolSpeed = 1.2f;

    [Tooltip("Tiempo de espera en cada waypoint antes de avanzar (segundos)")]
    [SerializeField] float waitTime = 2.0f;

    [Header("Persecución")]
    [Tooltip("Velocidad de movimiento al perseguir a Carlos (u/s)")]
    [SerializeField] float chaseSpeed = 2.0f;

    [Tooltip("Tiempo de búsqueda antes de regresar a patrulla al perder contacto visual (segundos)")]
    [SerializeField] float loseSightTime = 3.0f;

    [Header("Detección")]
    [Tooltip("Distancia máxima de visión frontal en línea recta")]
    [SerializeField] float viewDistance = 4.0f;

    [Tooltip("Ángulo total del cono de visión en grados")]
    [SerializeField] float viewAngle = 90.0f;

    [Tooltip("Distancia de audición omnidireccional (detecta pasos incluso a la espalda)")]
    [SerializeField] float hearDistance = 1.5f;

    [Tooltip("Multiplicador del rango de detección cuando Carlos corre (Shift)")]
    [SerializeField] float runNoiseMultiplier = 1.5f;

    public float PatrolSpeed => patrolSpeed;
    public float WaitTime => waitTime;
    public float ChaseSpeed => chaseSpeed;
    public float LoseSightTime => loseSightTime;
    public float ViewDistance => viewDistance;
    public float ViewAngle => viewAngle;
    public float HearDistance => hearDistance;
    public float RunNoiseMultiplier => runNoiseMultiplier;
}
