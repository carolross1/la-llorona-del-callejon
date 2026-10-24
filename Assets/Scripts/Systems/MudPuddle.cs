// Assets/Scripts/Systems/MudPuddle.cs
using UnityEngine;

/// <summary>
/// Obstáculo del entorno: Charco de lodo y agua estancada.
/// Ralentiza al jugador intensamente (al 35% de su velocidad) mientras permanezca dentro,
/// transmitiendo la sensación de quedar atascado en el fango y chapotear.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MudPuddle : MonoBehaviour
{
    [SerializeField] float slowFactor = 0.35f;
    float lastAlertTime = -99f;

    void OnTriggerEnter2D(Collider2D other)
    {
        ApplySlow(other, true);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        ApplySlow(other, false);
    }

    void ApplySlow(Collider2D other, bool notifyHud)
    {
        var movement = other.GetComponent<PlayerMovement>() ?? other.GetComponentInParent<PlayerMovement>();
        if (movement != null)
        {
            movement.SpeedMultiplier = slowFactor;

            if (notifyHud && Time.time > lastAlertTime + 4.0f)
            {
                lastAlertTime = Time.time;
                if (HUD.Instance != null)
                {
                    HUD.Instance.ShowTutorial("¡Chapoteaste en lodo! Te cuesta avanzar...", 2.5f);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var movement = other.GetComponent<PlayerMovement>() ?? other.GetComponentInParent<PlayerMovement>();
        if (movement != null)
        {
            movement.SpeedMultiplier = 1.0f;
        }
    }
}
