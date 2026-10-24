// Assets/Scripts/Systems/MudPuddle.cs
using UnityEngine;

/// <summary>
/// Obstáculo del entorno: Charco de lodo y agua estancada.
/// Ralentiza al jugador al 50% mientras permanezca dentro,
/// dificultando huir de La Llorona si se cruza imprudentemente.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MudPuddle : MonoBehaviour
{
    [SerializeField] float slowFactor = 0.5f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.SpeedMultiplier = slowFactor;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.SpeedMultiplier = 1.0f;
            }
        }
    }
}
