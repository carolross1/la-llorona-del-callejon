// Assets/Scripts/Systems/Stairs2D.cs
using UnityEngine;

/// <summary>
/// Sistema Natural de Escaleras de Cantera (Movimiento Fluido, Humano y con Libertad de Ruta):
/// 
/// 1. LIBERTAD DE PISO (No te atrapa si vas de paso):
///    - En la calle/piso inferior: Si caminas horizontalmente (--> o <--), pasas libremente por debajo.
///    - En el balcón/piso superior: Si caminas horizontalmente, sigues derecho por el balcón ("si el jugador
///      quisiera seguir avanzando, debe dejarlo").
/// 
/// 2. CÓMO SUBIR:
///    - Para entrar a la escalera desde abajo: Presionas ARRIBA (W / Flecha Arriba) o Diagonal hacia la escalera.
///    - En la escalera: Carlos camina hacia arriba de forma fluida y natural al avanzar en la dirección de subida.
///    - Al llegar a la cima: Se incorpora suavemente al balcón superior.
/// 
/// 3. CÓMO BAJAR:
///    - Si el balcón continúa: Solo baja si el jugador presiona activamente ABAJO (S / Flecha Abajo).
///    - Si la escalera es el final del camino: Carlos baja naturalmente hacia la escalera.
///    - En la escalera: Carlos camina hacia abajo con paso firme y fluido.
///    - Al llegar a la base: Se incorpora suavemente a la calle inferior.
/// </summary>
public class Stairs2D : MonoBehaviour
{
    [Header("Límites de Altura (Y)")]
    [Tooltip("Altura Y del piso inferior")]
    public float bottomY = 0.05f;
    [Tooltip("Altura Y del piso superior")]
    public float topY = 3.25f;

    [Header("Límites Horizontales (X)")]
    public float leftX = 0f;
    public float rightX = 4.1f;

    [Header("Orientación de la Escalera")]
    [Tooltip("True si sube hacia la derecha (+X = +Y). False si sube hacia la izquierda (-X = +Y).")]
    public bool isAscendingRight = true;

    [Header("Comportamiento en Cima")]
    [Tooltip("True si el piso superior continúa más allá de la escalera (requiere presionar Abajo para descender)")]
    public bool floorContinuesAtTop = false;

    [Header("Física de la Escalera")]
    [SerializeField] float followSpeed = 25.0f;

    // Estado de escalada activo para el jugador
    bool isClimbing = false;

    const float FloorThreshold = 0.35f;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerMovement>() ?? other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            isClimbing = false;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        var playerMovement = other.GetComponentInParent<PlayerMovement>() ?? other.GetComponent<PlayerMovement>();
        if (playerMovement == null) return;

        var rb = other.attachedRigidbody;
        if (rb == null) return;

        float playerX = rb.position.x;
        float playerY = rb.position.y;

        // Fuera del rango horizontal de la escalera
        if (playerX < leftX - 0.25f || playerX > rightX + 0.25f)
        {
            isClimbing = false;
            return;
        }

        float width = Mathf.Max(0.1f, rightX - leftX);
        float u = Mathf.Clamp01((playerX - leftX) / width);

        // Altura teórica de la escalera en la posición X de Carlos
        float stairY = isAscendingRight
            ? Mathf.Lerp(bottomY, topY, u)
            : Mathf.Lerp(topY, bottomY, u);

        Vector2 input = playerMovement.MoveInput;

        bool isMovingAscend = isAscendingRight
            ? (input.x > 0.1f || (input.y > 0.2f && rb.linearVelocity.x >= -0.05f))
            : (input.x < -0.1f || (input.y > 0.2f && rb.linearVelocity.x <= 0.05f));

        bool isMovingDescend = isAscendingRight
            ? (input.x < -0.1f || (input.y < -0.2f && rb.linearVelocity.x <= 0.05f))
            : (input.x > 0.1f || (input.y < -0.2f && rb.linearVelocity.x >= -0.05f));

        // -------------------------------------------------------------
        // 1. EVALUAR ENTRADA A LA ESCALERA
        // -------------------------------------------------------------
        if (!isClimbing)
        {
            // Caso A: Carlos está en el piso inferior (calle)
            if (playerY <= bottomY + FloorThreshold)
            {
                // Para subir: requiere presionar ARRIBA (W / Flecha Arriba) o saltar hacia la subida
                bool wantsToClimb = (input.y > 0.2f && (isAscendingRight ? input.x >= -0.1f : input.x <= 0.1f)) ||
                                    (playerMovement.IsJumping && isMovingAscend);

                if (wantsToClimb)
                {
                    isClimbing = true;
                }
                else
                {
                    // No quiere subir: pasa caminando por debajo sin ser atrapado
                    return;
                }
            }
            // Caso B: Carlos está en el piso superior (balcón)
            else if (playerY >= topY - 0.30f)
            {
                // Si el piso superior es continuo (como en Tier 2), Carlos SOLO baja si presiona ABAJO (S / Flecha Abajo).
                // Si solo camina horizontalmente (-->), sigue avanzando libremente por el balcón sin ser atrapado.
                bool wantsToDescend = false;

                if (floorContinuesAtTop)
                {
                    // Piso continuo: requiere intención explícita de bajar (tecla ABAJO)
                    wantsToDescend = input.y < -0.2f;
                }
                else
                {
                    // Fin del balcón: avanza hacia la escalera de bajada o presiona ABAJO
                    bool isNearTopEdge = isAscendingRight
                        ? (playerX >= rightX - 0.6f && playerX <= rightX + 0.3f)
                        : (playerX <= leftX + 0.6f && playerX >= leftX - 0.3f);

                    wantsToDescend = isNearTopEdge && (input.y < -0.2f || isMovingDescend || playerMovement.IsJumping);
                }

                if (wantsToDescend)
                {
                    isClimbing = true;
                }
                else
                {
                    // Sigue caminando libremente por el balcón superior
                    return;
                }
            }
            else
            {
                // Ya se encuentra en la cota intermedia de la escalera
                isClimbing = true;
            }
        }

        // -------------------------------------------------------------
        // 2. MOVIMIENTO NATURAL EN LA ESCALERA (COMO UNA PERSONA)
        // -------------------------------------------------------------
        if (isClimbing)
        {
            // Salto de emergencia para abandonar la escalera hacia el suelo
            if (input.y < -0.6f && playerMovement.IsJumping)
            {
                isClimbing = false;
                rb.position = new Vector2(rb.position.x, Mathf.MoveTowards(playerY, bottomY, followSpeed * Time.fixedDeltaTime));
                return;
            }

            // Si está en el aire por salto normal, dejar que el arco de salto actúe
            if (playerMovement.IsJumping)
            {
                return;
            }

            // Si el jugador se está moviendo:
            if (isMovingAscend || isMovingDescend || Mathf.Abs(input.x) > 0.05f || Mathf.Abs(input.y) > 0.05f)
            {
                // Guiar la altura vertical Y de Carlos suave y firmemente a lo largo de los escalones
                rb.position = new Vector2(rb.position.x, Mathf.MoveTowards(playerY, stairY, followSpeed * Time.fixedDeltaTime));

                // Al alcanzar la cima: incorporarse al piso superior
                bool reachedTop = isAscendingRight ? (playerX >= rightX - 0.15f) : (playerX <= leftX + 0.15f);
                if (reachedTop && isMovingAscend)
                {
                    rb.position = new Vector2(rb.position.x, topY);
                    isClimbing = false;
                }

                // Al alcanzar la base: incorporarse al piso inferior
                bool reachedBottom = isAscendingRight ? (playerX <= leftX + 0.15f) : (playerX >= rightX - 0.15f);
                if (reachedBottom && isMovingDescend)
                {
                    rb.position = new Vector2(rb.position.x, bottomY);
                    isClimbing = false;
                }
            }
            else
            {
                // En reposo: se mantiene firme sobre el escalón
                rb.position = new Vector2(rb.position.x, Mathf.MoveTowards(playerY, stairY, followSpeed * Time.fixedDeltaTime));
            }
        }
    }
}
