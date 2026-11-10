// Assets/Scripts/Systems/Level4RitualController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controlador del Gran Ritual de las Ánimas (Nivel 4 Clímax):
/// - Controla el bloqueo y apertura de la Gran Reja Gótica de salida.
/// - Gestiona la progresión de fases al consagrar cada uno de los 3 altares de cempasúchil.
/// - Despierta espectros (ánimas en pena) progresivamente.
/// - Desata el MODO FRENESÍ de La Llorona en el escape final.
/// </summary>
public class Level4RitualController : MonoBehaviour
{
    public static Level4RitualController Instance { get; private set; }

    [Header("Referencias de la Reja Gótica")]
    [SerializeField] GameObject exitGateObject;
    [SerializeField] Collider2D exitTriggerCollider;
    [SerializeField] Collider2D gateSolidCollider;
    [SerializeField] Light2D gateLight;
    [SerializeField] TextMesh gateLabel;

    [Header("Espectros Durmientes")]
    [SerializeField] GameObject specter1;
    [SerializeField] GameObject specter2;

    [Header("Efectos y Audio")]
    [SerializeField] AudioClip bellTollSfx;
    [SerializeField] AudioClip portalOpenSfx;
    [SerializeField] AudioClip frenzyRoarSfx;
    [SerializeField] Light2D globalLight;

    [Header("Configuración del Ritual")]
    [SerializeField] int totalAltarsNeeded = 3;

    int litAltarsCount = 0;
    bool isGateUnlocked = false;
    HashSet<ShrineNiche> countedShrines = new HashSet<ShrineNiche>();
    PlayerMovement player;
    LloronaAI llorona;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
        llorona = FindAnyObjectByType<LloronaAI>();

        // Estado inicial de la reja: COMPLETAMENTE SELLADA
        LockGateInitially();

        // Espectros inician dormidos/inactivos para que el inicio sea de sigilo tenso
        if (specter1 != null) specter1.SetActive(false);
        if (specter2 != null) specter2.SetActive(false);

        UpdateGateLabel();
    }

    void Update()
    {
        if (player == null) player = FindAnyObjectByType<PlayerMovement>();
        if (llorona == null) llorona = FindAnyObjectByType<LloronaAI>();

        CheckShrinesProgress();
        CheckPlayerGateProximity();
    }

    void LockGateInitially()
    {
        isGateUnlocked = false;

        // Desactivar el trigger de victoria hasta completar el ritual
        if (exitTriggerCollider != null)
        {
            exitTriggerCollider.enabled = false;
        }

        // Asegurar que la reja tenga collider sólido para impedir el paso temprano
        if (gateSolidCollider != null)
        {
            gateSolidCollider.enabled = true;
        }

        // Luz mística de sellado (púrpura espectral)
        if (gateLight != null)
        {
            gateLight.color = new Color(0.65f, 0.15f, 0.95f);
            gateLight.intensity = 1.6f;
        }
    }

    void CheckShrinesProgress()
    {
        if (isGateUnlocked) return;

        foreach (var shrine in ShrineNiche.AllShrines)
        {
            if (shrine != null && shrine.IsLit && !countedShrines.Contains(shrine))
            {
                countedShrines.Add(shrine);
                OnAltarLitProgress(countedShrines.Count);
            }
        }
    }

    void OnAltarLitProgress(int count)
    {
        litAltarsCount = count;
        UpdateGateLabel();

        switch (litAltarsCount)
        {
            case 1:
                StartCoroutine(Phase1_FirstAltarSequence());
                break;
            case 2:
                StartCoroutine(Phase2_SecondAltarSequence());
                break;
            case 3:
                StartCoroutine(Phase3_ClimaxEscapeSequence());
                break;
        }
    }

    void UpdateGateLabel()
    {
        if (gateLabel == null) return;

        if (!isGateUnlocked)
        {
            gateLabel.text = $"✦ SELLO DE LAS ÁNIMAS ✦\nAltares Consagrados: {litAltarsCount}/{totalAltarsNeeded}";
            gateLabel.color = new Color(1.0f, 0.45f, 0.35f);
        }
        else
        {
            gateLabel.text = "¡¡¡LA GRAN REJA ESTÁ ABIERTA!!!\n➔ ¡¡CRUZA Y ESCAPA A TU HOGAR!! ➔";
            gateLabel.color = new Color(0.2f, 1.0f, 0.5f);
        }
    }

    void CheckPlayerGateProximity()
    {
        if (isGateUnlocked || player == null || exitGateObject == null) return;

        float dist = Vector2.Distance(player.transform.position, exitGateObject.transform.position);
        if (dist < 4.2f && !player.IsHidden)
        {
            if (HUD.Instance != null && Time.frameCount % 180 == 0)
            {
                HUD.Instance.ShowEventNotice($"La Gran Reja está sellada por las ánimas ({litAltarsCount}/3).\nConsagra los altares del panteón usando veladoras [F].", 3.0f);
            }
        }
    }

    // ========================================================
    // SECUENCIAS CINEMATOGRÁFICAS DE FASES
    // ========================================================

    IEnumerator Phase1_FirstAltarSequence()
    {
        // 1. Sonido solemne de campana de catedral
        PlaySound(bellTollSfx, 1.0f);

        yield return new WaitForSeconds(0.4f);

        // 2. Despertar la primera Ánima en Pena (patrulla central)
        if (specter1 != null)
        {
            specter1.SetActive(true);
            var sr = specter1.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Efecto de aparición espectral suave
                sr.color = new Color(0.35f, 0.85f, 1.0f, 0.9f);
            }
        }

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡Primer Altar Consagrado (1/3)!\nUn tañido fúnebre resuena... Un alma en pena vigila las tumbas.", 4.5f);
        }
    }

    IEnumerator Phase2_SecondAltarSequence()
    {
        // 1. Doble tañido de campana
        PlaySound(bellTollSfx, 1.0f);

        yield return new WaitForSeconds(0.3f);

        // 2. Despertar la segunda Ánima en Pena (patrulla norte de criptas)
        if (specter2 != null)
        {
            specter2.SetActive(true);
        }

        // 3. Destello místico en la luz global
        if (globalLight != null)
        {
            float orig = globalLight.intensity;
            globalLight.intensity = 0.55f;
            yield return new WaitForSeconds(0.25f);
            globalLight.intensity = orig;
        }

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡Segundo Altar Consagrado (2/3)!\n¡El panteón se inquieta! Falta 1 altar para romper el sello.", 4.5f);
        }
    }

    IEnumerator Phase3_ClimaxEscapeSequence()
    {
        isGateUnlocked = true;

        // 1. Sonido masivo de apertura de portal y crujido de hierro
        PlaySound(portalOpenSfx, 1.0f);
        PlaySound(bellTollSfx, 1.0f);

        // 2. Destrabar la reja físicamente
        if (gateSolidCollider != null)
        {
            gateSolidCollider.enabled = false; // Ya se puede cruzar físicamente
        }
        if (exitTriggerCollider != null)
        {
            exitTriggerCollider.enabled = true; // Meta activada
        }

        // 3. Cambiar iluminación de la reja a Verde Esmeralda de Victoria
        if (gateLight != null)
        {
            gateLight.color = new Color(0.15f, 1.0f, 0.45f);
            gateLight.intensity = 3.5f;
            gateLight.pointLightOuterRadius = 7.5f;
        }

        UpdateGateLabel();

        yield return new WaitForSeconds(0.6f);

        // 4. ¡¡¡LA LLORONA ENTRA EN FRENESÍ TOTAL!!!
        if (llorona != null)
        {
            llorona.EnterFrenzyMode(4.0f);
        }
        else
        {
            PlaySound(frenzyRoarSfx, 1.0f);
        }

        // 5. Aviso de alarma máxima en HUD
        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡¡¡EL SELLO SE HA ROTO!!!\n¡¡LA LLORONA ESTÁ ENFURECIDA!! ¡¡CORRE A LA GRAN REJA!!", 6.5f);
            HUD.Instance.SetEyeAlert(true);
        }

        // 6. Efecto de parpadeo de tensión en faroles
        StartCoroutine(FlickerFrenzyAtmosphere());
    }

    IEnumerator FlickerFrenzyAtmosphere()
    {
        for (int i = 0; i < 6; i++)
        {
            if (globalLight != null)
            {
                globalLight.color = (i % 2 == 0) ? new Color(0.55f, 0.15f, 0.20f) : new Color(0.18f, 0.22f, 0.38f);
            }
            yield return new WaitForSeconds(0.18f);
        }
        if (globalLight != null)
        {
            globalLight.color = new Color(0.28f, 0.18f, 0.32f); // Ambiente tenso final
        }
    }

    void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : transform.position, volume);
        }
    }

    public void SetupReferences(
        GameObject gateObj,
        Collider2D exitTrigger,
        Collider2D gateSolid,
        Light2D gLight,
        TextMesh gLabel,
        GameObject sp1,
        GameObject sp2,
        AudioClip bell,
        AudioClip portal,
        AudioClip frenzy,
        Light2D gGlobalLight)
    {
        exitGateObject = gateObj;
        exitTriggerCollider = exitTrigger;
        gateSolidCollider = gateSolid;
        gateLight = gLight;
        gateLabel = gLabel;
        specter1 = sp1;
        specter2 = sp2;
        bellTollSfx = bell;
        portalOpenSfx = portal;
        frenzyRoarSfx = frenzy;
        globalLight = gGlobalLight;
    }
}
