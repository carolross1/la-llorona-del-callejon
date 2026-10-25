// Assets/Scripts/Systems/MudPuddle.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// Obstáculo del entorno: Charco de lodo y agua estancada.
/// Reduce la velocidad de Carlos al 50% durante 2.5 segundos con aviso visual en el HUD.
/// Requiere que los pies de Carlos pisen directamente sobre el lodo para activarse.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MudPuddle : MonoBehaviour
{
    [SerializeField] float slowFactor = 0.5f;
    [SerializeField] float slowDuration = 2.5f;
    float lastAlertTime = -99f;

    void OnTriggerEnter2D(Collider2D other)
    {
        ApplySlow(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        ApplySlow(other);
    }

    void ApplySlow(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var movement = other.GetComponent<PlayerMovement>() ?? other.GetComponentInParent<PlayerMovement>();
        if (movement == null) return;

        // Verificar que los pies de Carlos estén realmente sobre la zona visible del charco
        Vector2 feetPos = (Vector2)movement.transform.position + new Vector2(0f, -0.35f);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.bounds.Contains(feetPos))
        {
            float dist = Vector2.Distance((Vector2)transform.position, feetPos);
            if (dist > 0.60f) return;
        }

        movement.SpeedMultiplier = slowFactor;

        if (Time.time > lastAlertTime + 3.0f)
        {
            lastAlertTime = Time.time;
            if (HUD.Instance != null)
            {
                HUD.Instance.ShowEventNotice("¡Pisaste lodo! Movimiento reducido", 2.5f);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var movement = other.GetComponent<PlayerMovement>() ?? other.GetComponentInParent<PlayerMovement>();
        if (movement != null && gameObject.activeInHierarchy)
        {
            StartCoroutine(RestoreSpeedRoutine(movement));
        }
    }

    IEnumerator RestoreSpeedRoutine(PlayerMovement movement)
    {
        yield return new WaitForSeconds(slowDuration);
        if (movement != null)
        {
            movement.SpeedMultiplier = 1.0f;
        }
    }
}
