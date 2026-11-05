// Assets/Scripts/Systems/ParroquiaDoorController.cs
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controla el sellado místico y apertura de las puertas de la Parroquia de Dolores.
/// Las puertas permanecen cerradas y protegidas por una niebla espectral hasta que
/// Carlos consagra los 3 nichos del atrio con veladoras.
/// Al completarse las 3 ofrendas, tañen las campanas y el portal de victoria se activa.
/// Utiliza TextMesh nativo de Unity para cero dependencias externas.
/// </summary>
public class ParroquiaDoorController : MonoBehaviour
{
    public static ParroquiaDoorController Instance { get; private set; }

    [Header("Objetivo de Ofrendas")]
    [SerializeField] int requiredShrines = 3;

    [Header("Componentes de la Puerta")]
    [SerializeField] GameObject doorSealVisual;
    [SerializeField] GameObject openDoorsVisual;
    [SerializeField] Light2D sanctuaryLight;
    [SerializeField] Collider2D goalCollider;
    [SerializeField] TextMesh statusLabel;

    int litCount = 0;
    bool isOpen = false;

    public bool IsOpen => isOpen;
    public int RequiredShrines => requiredShrines;
    public int LitCount => litCount;

    void Awake()
    {
        Instance = this;
        litCount = 0;
        isOpen = false;
    }

    void Start()
    {
        UpdateDoorState();
        HUD.ActiveShrinesLit = 0;
        HUD.TotalShrines = requiredShrines;
        if (HUD.Instance != null)
        {
            HUD.Instance.UpdateCandleCount(Candle.PlayerCandlesCount);
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        HUD.TotalShrines = 0;
        HUD.ActiveShrinesLit = 0;
    }

    public void OnShrineLit(ShrineNiche shrine)
    {
        litCount++;
        HUD.ActiveShrinesLit = litCount;
        HUD.TotalShrines = requiredShrines;

        if (HUD.Instance != null)
        {
            HUD.Instance.UpdateCandleCount(Candle.PlayerCandlesCount);
        }

        if (litCount >= requiredShrines && !isOpen)
        {
            OpenParroquiaDoors();
        }
        else
        {
            UpdateDoorState();
        }
    }

    void UpdateDoorState()
    {
        if (isOpen) return;

        if (doorSealVisual != null) doorSealVisual.SetActive(true);
        if (openDoorsVisual != null) openDoorsVisual.SetActive(false);
        if (sanctuaryLight != null) sanctuaryLight.enabled = false;
        if (goalCollider != null) goalCollider.enabled = false;

        if (statusLabel != null)
        {
            statusLabel.text = $"PUERTAS SELLADAS\n(Ofrendas: {litCount}/{requiredShrines})";
            statusLabel.color = new Color(0.9f, 0.5f, 0.8f);
            statusLabel.gameObject.SetActive(true);
        }
    }

    void OpenParroquiaDoors()
    {
        isOpen = true;

        if (doorSealVisual != null) doorSealVisual.SetActive(false);
        if (openDoorsVisual != null) openDoorsVisual.SetActive(true);
        if (sanctuaryLight != null) sanctuaryLight.enabled = true;
        if (goalCollider != null) goalCollider.enabled = true;

        if (statusLabel != null)
        {
            statusLabel.text = "¡PUERTAS ABIERTAS!\n(SANTUARIO)";
            statusLabel.color = new Color(1.0f, 0.95f, 0.55f);
            // Ocultar el letrero para que la entrada y la fachada queden limpias
            statusLabel.gameObject.SetActive(false);
        }

        // Tañido triunfal de las campanas de la iglesia
        if (AudioManager.Instance != null)
        {
            if (AudioManager.Instance.campanaLejana != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.campanaLejana, 1.0f);
            AudioManager.Instance.DuckMusic(5.0f, 0.25f);
        }

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowTutorial("¡El atrio ha sido consagrado!\nLas puertas de la Parroquia se han abierto. ¡Sube las escaleras para entrar al santuario!", 6.0f);
        }
    }
}
