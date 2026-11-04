// Assets/Scripts/Systems/Stairs2D.cs
using UnityEngine;

/// <summary>
/// Permite al jugador subir y bajar escaleras coloniales con total naturalidad entre múltiples niveles.
/// Bloquea el eje vertical al ángulo exacto de la rampa/escalón para que el jugador NO pueda
/// traspasar la escalera ni flotar en el aire: al caminar izquierda/derecha (o WASD), Carlos
/// asciende o desciende suavemente a lo largo de la pendiente hasta desembocar en el siguiente piso.
/// </summary>
public class Stairs2D : MonoBehaviour
{
    [Header("Límites de Altura (Y)")]
    [Tooltip("Altura Y en la base de la escalera (piso inferior)")]
    public float bottomY = 0.6f;
    [Tooltip("Altura Y en la cima de la escalera (piso superior)")]
    public float topY = 4.4f;

    [Header("Límites Horizontales (X)")]
    [Tooltip("Posición X izquierda de la escalera")]
    public float leftX = 0f;
    [Tooltip("Posición X derecha de la escalera")]
    public float rightX = 4f;

    [Header("Orientación de la Escalera")]
    [Tooltip("True si sube hacia la derecha (+X = +Y). False si sube hacia la izquierda (-X = +Y).")]
    public bool isAscendingRight = true;

    [Header("Suavizado de Pendiente")]
    [SerializeField] float followSpeed = 25.0f;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null)
        {
            var rb = other.attachedRigidbody;
            if (rb == null) return;

            float playerX = rb.position.x;

            // Verificar si el jugador está dentro del rango horizontal de la escalera
            if (playerX >= leftX - 0.25f && playerX <= rightX + 0.25f)
            {
                float t = Mathf.Clamp01((playerX - leftX) / Mathf.Max(0.01f, rightX - leftX));
                float targetY = isAscendingRight
                    ? Mathf.Lerp(bottomY, topY, t)
                    : Mathf.Lerp(topY, bottomY, t);

                // Ajustar suavemente la posición Y de Carlos a la cota del escalón
                float newY = Mathf.MoveTowards(rb.position.y, targetY, followSpeed * Time.fixedDeltaTime);
                rb.position = new Vector2(rb.position.x, newY);

                // Evitar que la velocidad vertical en FixedUpdate anule la subida
                if (Mathf.Abs(rb.linearVelocity.x) > 0.05f)
                {
                    float slopeSign = isAscendingRight ? 1f : -1f;
                    float slope = Mathf.Abs(topY - bottomY) / Mathf.Max(0.01f, rightX - leftX);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.x * slope * slopeSign);
                }
            }
        }
    }
}
