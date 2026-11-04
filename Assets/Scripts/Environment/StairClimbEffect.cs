using UnityEngine;

/// <summary>
/// Efecto de subir las escaleras de cantera hacia el portal de la Parroquia de Dolores:
/// - Reduce suavemente la velocidad para transmitir el esfuerzo físico de la subida.
/// - Genera un cadencia rítmica de pisadas de piedra con variación tonal ascendente.
/// - Produce un sutil vaivén/rebote vertical (bobbing) simulando el paso de escalón en escalón.
/// - Al acercarse a las puertas superiores, hace tañer las campanas sagradas de la Parroquia y mitiga a La Llorona.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class StairClimbEffect : MonoBehaviour
{
    [Header("Ajustes de Subida")]
    [SerializeField] float climbSpeedMultiplier = 0.82f;
    [SerializeField] float stepInterval = 0.26f;
    [SerializeField] float stepHeightBob = 0.07f;

    [Header("Referencias de Audio")]
    [SerializeField] AudioClip stoneStepClip;
    [SerializeField] AudioClip churchBellClip;

    PlayerMovement currentPlayer;
    Transform playerTransform;
    SpriteRenderer playerSr;
    float stepTimer;
    int currentStepIndex;
    bool bellTolled = false;
    Vector3 originalSrLocalPos;

    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();
        if (pm != null)
        {
            currentPlayer = pm;
            playerTransform = pm.transform;
            playerSr = pm.GetComponentInChildren<SpriteRenderer>();
            if (playerSr != null)
            {
                originalSrLocalPos = playerSr.transform.localPosition;
            }
            currentPlayer.SpeedMultiplier = climbSpeedMultiplier;
            stepTimer = 0f;
            currentStepIndex = 0;
            bellTolled = false;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (currentPlayer == null || playerTransform == null) return;

        Vector2 input = currentPlayer.MoveInput;
        bool isMoving = input.sqrMagnitude > 0.05f;

        if (isMoving)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                currentStepIndex++;

                // Efecto de sonido de pisada de piedra con pitch ascendente al subir
                float pitch = Mathf.Clamp(0.92f + (currentStepIndex * 0.04f), 0.9f, 1.25f);
                PlayStoneStep(pitch);

                // Elevación sutil simulando pisar el escalón superior
                if (playerSr != null)
                {
                    playerSr.transform.localPosition = originalSrLocalPos + new Vector3(0f, stepHeightBob, 0f);
                }
            }
        }

        // Recuperación suave del rebote vertical
        if (playerSr != null && playerSr.transform.localPosition != originalSrLocalPos)
        {
            playerSr.transform.localPosition = Vector3.MoveTowards(
                playerSr.transform.localPosition,
                originalSrLocalPos,
                Time.deltaTime * 0.35f
            );
        }

        // Tañido sagrado de la campana al coronar la parte alta de la escalinata
        if (!bellTolled && playerTransform.position.y >= (transform.position.y + 0.45f))
        {
            bellTolled = true;
            if (AudioManager.Instance != null)
            {
                AudioClip bell = churchBellClip != null ? churchBellClip : AudioManager.Instance.campanaLejana;
                if (bell != null)
                {
                    AudioManager.Instance.PlaySFX(bell, 0.95f);
                }
                AudioManager.Instance.DuckMusic(4.0f, 0.35f);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();
        if (pm != null && pm == currentPlayer)
        {
            pm.SpeedMultiplier = 1.0f;
            if (playerSr != null)
            {
                playerSr.transform.localPosition = originalSrLocalPos;
            }
            currentPlayer = null;
        }
    }

    void PlayStoneStep(float pitch)
    {
        if (AudioManager.Instance != null)
        {
            AudioClip clip = stoneStepClip != null 
                ? stoneStepClip 
                : (AudioManager.Instance.pasosLentos ?? AudioManager.Instance.pasos1);

            if (clip != null)
            {
                AudioManager.Instance.PlaySFX(clip, 0.65f);
            }
        }
    }
}
