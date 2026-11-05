// Assets/Scripts/Systems/CandleStand.cs
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mesa o dispensador de veladoras benditas en el Atrio de Dolores.
/// Permite al jugador reponer veladoras si se le terminan al distraer a La Llorona,
/// evitando por completo que quede bloqueado (soft-lock) sin poder abrir la Parroquia.
/// Utiliza TextMesh nativo de Unity para cero dependencias externas.
/// </summary>
public class CandleStand : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] float interactionRadius = 2.2f;
    [SerializeField] int maxCandlesAllowed = 2; // Si tiene menos de 2, puede tomar más

    [Header("Referencias Visuales")]
    [SerializeField] GameObject promptContainer;
    [SerializeField] TextMesh promptTextMesh;

    [Header("Efectos de Audio")]
    [SerializeField] AudioClip takeCandleSound;

    Transform playerTransform;

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (promptContainer != null)
            promptContainer.SetActive(false);
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
        bool inRange = dist <= interactionRadius;

        if (promptContainer != null)
        {
            if (inRange != promptContainer.activeSelf)
                promptContainer.SetActive(inRange);

            if (inRange && promptTextMesh != null)
            {
                if (Candle.PlayerCandlesCount < maxCandlesAllowed)
                {
                    promptTextMesh.text = "[E] Tomar Veladora Bendita";
                    promptTextMesh.color = new Color(1.0f, 0.92f, 0.4f);
                }
                else
                {
                    promptTextMesh.text = "Mesa de Cirios\n(Llevas suficientes)";
                    promptTextMesh.color = new Color(0.8f, 0.8f, 0.8f);
                }
            }
        }

        if (inRange)
        {
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame)
            {
                TryTakeCandle();
            }
        }
    }

    void TryTakeCandle()
    {
        if (Candle.PlayerCandlesCount >= maxCandlesAllowed)
        {
            if (HUD.Instance != null)
            {
                HUD.Instance.ShowEventNotice("Ya llevas suficientes veladoras para tus ofrendas.", 2.5f);
            }
            return;
        }

        Candle.AddCandle(1);

        if (takeCandleSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(takeCandleSound, 1.0f);
        }

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡Tomaste una veladora bendita del atrio!", 3.0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
