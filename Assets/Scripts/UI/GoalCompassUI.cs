// Assets/Scripts/UI/GoalCompassUI.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Brújula e indicador de navegación hacia la meta (salida).
/// Rota una flecha en tiempo real apuntando hacia el LevelGoal
/// y muestra la distancia en metros en el HUD, asegurando que
/// el jugador nunca se desoriente en ningún nivel.
/// </summary>
public class GoalCompassUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] RectTransform arrowRect;
    [SerializeField] Text distanceText;

    [Header("Ajustes")]
    [SerializeField] float nearDistance = 6.0f;
    [SerializeField] Color normalColor = new Color(1f, 0.90f, 0.40f);
    [SerializeField] Color nearColor = new Color(0.2f, 1.0f, 0.4f);

    Transform playerTransform;
    Transform goalTransform;

    void Start()
    {
        FindTargets();
    }

    void FindTargets()
    {
        var player = Object.FindAnyObjectByType<PlayerMovement>();
        if (player != null) playerTransform = player.transform;

        var goal = Object.FindAnyObjectByType<LevelGoal>();
        if (goal != null) goalTransform = goal.transform;
    }

    void Update()
    {
        if (playerTransform == null || goalTransform == null)
        {
            FindTargets();
            if (playerTransform == null || goalTransform == null) return;
        }

        Vector2 diff = goalTransform.position - playerTransform.position;
        float dist = diff.magnitude;

        // Rotar flecha hacia la meta
        if (arrowRect != null)
        {
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            arrowRect.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // Actualizar texto de distancia
        if (distanceText != null)
        {
            if (dist <= nearDistance)
            {
                distanceText.color = nearColor;
                distanceText.text = "¡SALIDA AQUÍ!";
            }
            else
            {
                distanceText.color = normalColor;
                distanceText.text = $"Salida: {Mathf.RoundToInt(dist)}m";
            }
        }
    }
}
