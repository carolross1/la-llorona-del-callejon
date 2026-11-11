// Assets/Scripts/Systems/LloronaGraveOffering.cs
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// La Tumba Olvidada de La Llorona (Nivel 4 Clímax):
/// - Punto sagrado donde reposan los restos del alma en pena.
/// - Fase 1 (Durante el combate): La tumba está sellada por el rencor.
/// - Fase 2 (Tras derrotar a La Llorona): El espíritu apaciguado se reúne aquí.
///   Carlos debe BUSCAR PRIMERO la Ofrenda Sagrada en el panteón y traerla aquí con [E].
/// - Fase 3 (Ofrenda completada): La tumba estalla en flores y luz dorada,
///   el alma de La Llorona asciende en paz al cielo y la Gran Reja hacia casa se abre.
/// </summary>
public class LloronaGraveOffering : MonoBehaviour
{
    public static LloronaGraveOffering Instance { get; private set; }

    [Header("Referencias Visuales")]
    [SerializeField] GameObject offeringDecorations; // Tapete de flores y veladoras
    [SerializeField] Light2D graveLight;             // Luz mística de la tumba
    [SerializeField] GameObject promptObject;
    [SerializeField] TextMesh promptTextMesh;

    [Header("Referencias de Audio")]
    [SerializeField] AudioClip offeringSfx;          // Sonido de colocar ofrenda / consagración
    [SerializeField] AudioClip bellTollSfx;          // Campanadas de catedral
    [SerializeField] AudioClip gateOpenSfx;          // Apertura del gran portón

    [Header("Interacción")]
    [SerializeField] float interactRadius = 3.5f;

    public enum GraveState { Locked, ReadyForOffering, Consecrated }
    [SerializeField] GraveState state = GraveState.Locked;

    Transform playerTransform;
    PlayerMovement playerMovement;
    bool isConsecrated = false;
    bool hasOffering = false;

    public GraveState CurrentState => state;
    public bool IsConsecrated => isConsecrated;
    public bool HasOffering => hasOffering;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (offeringDecorations != null) offeringDecorations.SetActive(false);
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        UpdatePromptVisuals(false);
    }

    void Update()
    {
        if (isConsecrated) return;

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                playerTransform = p.transform;
                playerMovement = p.GetComponent<PlayerMovement>();
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRadius;

        UpdatePromptVisuals(inRange);

        // Interacción con tecla [E] o [Espacio]
        if (inRange && state == GraveState.ReadyForOffering)
        {
            var kb = Keyboard.current;
            if (kb != null && (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
            {
                PlaceSacredOffering();
            }
        }
    }

    void UpdatePromptVisuals(bool inRange)
    {
        if (promptObject == null) return;

        if (promptObject.activeSelf != inRange)
            promptObject.SetActive(inRange);

        if (!inRange || promptTextMesh == null) return;

        switch (state)
        {
            case GraveState.Locked:
                promptTextMesh.text = "✦ Tumba Olvidada de La Llorona ✦\n[Sellada por el dolor] Apacigua primero su espíritu";
                promptTextMesh.color = new Color(0.7f, 0.4f, 0.9f);
                break;

            case GraveState.ReadyForOffering:
                if (hasOffering)
                {
                    promptTextMesh.text = "✦ Tumba de La Llorona ✦\n[E] Colocar Ofrenda Sagrada (Cempasúchil y Veladoras)";
                    promptTextMesh.color = new Color(1.0f, 0.88f, 0.25f);
                }
                else
                {
                    promptTextMesh.text = "✦ Tumba de La Llorona ✦\n[Falta la Ofrenda] Busca las Flores y Veladoras en el panteón";
                    promptTextMesh.color = new Color(1.0f, 0.45f, 0.35f);
                }
                break;

            case GraveState.Consecrated:
                promptTextMesh.text = "✓ Ofrenda Consagrada\nEl alma descansa en paz";
                promptTextMesh.color = new Color(0.4f, 1.0f, 0.6f);
                break;
        }
    }

    /// <summary>
    /// Llamado desde SacredOfferingPickup al recoger la ofrenda.
    /// </summary>
    public void PickUpOffering()
    {
        hasOffering = true;
    }

    /// <summary>
    /// Llamado desde LloronaBossAI al llegar a 0 de vida.
    /// </summary>
    public void EnableOffering()
    {
        state = GraveState.ReadyForOffering;

        // Luz dorada brillante guiando al jugador
        if (graveLight != null)
        {
            graveLight.color = new Color(1.0f, 0.85f, 0.35f);
            graveLight.intensity = 3.5f;
            graveLight.pointLightOuterRadius = 6.0f;
        }

        if (HUD.Instance != null)
        {
            if (hasOffering)
            {
                HUD.Instance.ShowEventNotice("✦ ¡Has apaciguado la furia de La Llorona! ✦\nTienes la Ofrenda: acércate a su tumba y colócala con [E].", 6.0f);
            }
            else
            {
                HUD.Instance.ShowEventNotice("✦ ¡Has apaciguado la furia de La Llorona! ✦\n¡Busca la Ofrenda Sagrada en el Panteón y tráela a su tumba!", 6.5f);
            }
        }
    }

    public void PlaceSacredOffering()
    {
        if (isConsecrated || state != GraveState.ReadyForOffering) return;

        if (!hasOffering)
        {
            if (HUD.Instance != null)
            {
                HUD.Instance.ShowEventNotice("No tienes la Ofrenda Sagrada.\nBúscala en el Altar de las Ánimas (sigue el brillo dorado).", 3.5f);
            }
            return;
        }

        isConsecrated = true;
        state = GraveState.Consecrated;

        StartCoroutine(ConsecrationSequence());
    }

    IEnumerator ConsecrationSequence()
    {
        // 1. Sonido sagrado de consagración y campanas
        PlaySound(offeringSfx ?? bellTollSfx, 1.0f);

        // 2. Encender alfombra de cempasúchil y veladoras
        if (offeringDecorations != null)
        {
            offeringDecorations.SetActive(true);
        }

        if (graveLight != null)
        {
            graveLight.color = new Color(1.0f, 0.90f, 0.40f);
            graveLight.intensity = 4.5f;
            graveLight.pointLightOuterRadius = 8.5f;
        }

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("✦ 'Gracias, Carlos... al fin mis hijos y yo descansaremos en paz...' ✦", 5.0f);
        }

        // 3. Notificar a La Llorona para su ascenso celestial
        if (LloronaBossAI.Instance != null)
        {
            LloronaBossAI.Instance.AscendToHeaven();
        }

        yield return new WaitForSeconds(2.0f);

        // 4. Sonido de campanas de catedral y apertura de reja
        PlaySound(bellTollSfx, 1.0f);
        PlaySound(gateOpenSfx, 1.0f);

        // 5. Abrir la Gran Reja Gótica de escape
        UnlockExitGate();

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡¡¡EL ALMA DE LA LLORONA HA SIDO LIBERADA!!!\n¡¡La Gran Reja se ha abierto!! ¡Corre a tu hogar!", 8.0f);
        }
    }

    void UnlockExitGate()
    {
        // Activar el trigger de victoria
        var goal = FindAnyObjectByType<LevelGoal>(FindObjectsInactive.Include);
        if (goal != null)
        {
            goal.gameObject.SetActive(true);
            var col = goal.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }

        // Desactivar barrera sólida de la reja
        var barrier = GameObject.Find("Barrera_Solida_Sello");
        if (barrier != null) barrier.SetActive(false);

        // Iluminar la reja en verde esmeralda triunfal
        var gateLight = GameObject.Find("LuzSalvacion")?.GetComponent<Light2D>();
        if (gateLight != null)
        {
            gateLight.color = new Color(0.15f, 1.0f, 0.45f);
            gateLight.intensity = 4.0f;
            gateLight.pointLightOuterRadius = 7.5f;
        }

        var label = GameObject.Find("LabelMeta")?.GetComponent<TextMesh>();
        if (label != null)
        {
            label.text = "¡¡¡LIBERTAD (A TU HOGAR)!!! ➔";
            label.color = new Color(0.2f, 1.0f, 0.5f);
        }
    }

    void PlaySound(AudioClip clip, float vol)
    {
        if (clip == null) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(clip, vol);
        else AudioSource.PlayClipAtPoint(clip, transform.position, vol);
    }
}
