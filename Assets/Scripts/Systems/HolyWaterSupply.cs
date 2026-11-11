// Assets/Scripts/Systems/HolyWaterSupply.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Pila / Fuente de Agua Bendita en el panteón:
/// - Carlos se acerca y presiona [E] para recargar sus jarros de agua bendita al máximo.
/// - Cuenta con luz sacra parpadeante y aviso contextual en pantalla.
/// </summary>
public class HolyWaterSupply : MonoBehaviour
{
    [SerializeField] string fontName = "Pila de Agua Bendita";
    [SerializeField] float interactRadius = 2.2f;
    [SerializeField] AudioClip refillSfx;
    [SerializeField] GameObject promptObject;
    [SerializeField] TextMesh promptTextMesh;
    [SerializeField] Light2D fontLight;

    Transform playerTransform;
    PlayerHolyWaterLauncher launcher;
    float cooldownTimer = 0f;

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            launcher = player.GetComponent<PlayerHolyWaterLauncher>();
        }

        if (promptTextMesh != null)
        {
            promptTextMesh.text = $"[E] {fontName}\n(Recargar Agua Bendita)";
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                playerTransform = p.transform;
                launcher = p.GetComponent<PlayerHolyWaterLauncher>();
            }
            return;
        }

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRadius;

        if (promptObject != null)
        {
            if (promptObject.activeSelf != inRange)
                promptObject.SetActive(inRange);

            if (inRange && promptTextMesh != null && launcher != null)
            {
                if (launcher.CurrentJars < launcher.MaxJars)
                {
                    promptTextMesh.text = $"[E] Tomar Agua Bendita\n({launcher.CurrentJars}/{launcher.MaxJars})";
                    promptTextMesh.color = new Color(0.35f, 0.95f, 1.0f);
                }
                else
                {
                    promptTextMesh.text = $"✓ Jarros Llenos\n({launcher.CurrentJars}/{launcher.MaxJars})";
                    promptTextMesh.color = new Color(0.6f, 1.0f, 0.7f);
                }
            }
        }

        // Interacción con tecla [E] o [Espacio]
        var kb = Keyboard.current;
        if (inRange && kb != null && (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
        {
            TryRefill();
        }
    }

    void TryRefill()
    {
        if (cooldownTimer > 0f || launcher == null) return;

        if (launcher.CurrentJars >= launcher.MaxJars)
        {
            if (HUD.Instance != null)
            {
                HUD.Instance.ShowEventNotice("Tus jarros de agua bendita ya están llenos.", 2.0f);
            }
            return;
        }

        launcher.RefillJars();
        cooldownTimer = 1.0f;

        if (refillSfx != null)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(refillSfx, 1.0f);
            else AudioSource.PlayClipAtPoint(refillSfx, transform.position);
        }

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡Jarros de Agua Bendita recargados! Apunta y ataca a La Llorona.", 3.0f);
        }

        // Pulso visual en la luz de la pila
        if (fontLight != null)
        {
            fontLight.intensity = 2.8f;
        }
    }
}
