// Assets/Scripts/UI/BossHealthBarUI.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de Vida e interfaz de combate para el Boss Fight final (Nivel 4):
/// - Barra de salud superior con el título del jefe: "✦ LA LLORONA - EL ALMA EN PENA ✦"
/// - Cambio dinámico de color y texto según la fase activa (Azul -> Violeta -> Carmesí).
/// - Drenado de barra dual (fillAmount y redimensionamiento de RectTransform para infalibilidad visual).
/// - Contador numérico de vida [ Vida: X / 9 ] y munición de Agua Bendita [💧 ● ● ●].
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    [Header("Elementos de la Barra de Jefe")]
    [SerializeField] GameObject rootPanel;
    [SerializeField] Text bossTitleText;
    [SerializeField] Text phaseSubtitleText;
    [SerializeField] Image healthBarFill;
    [SerializeField] Image healthBarBackground;

    [Header("Contador de Agua Bendita")]
    [SerializeField] Text ammoText;

    Color phase1Color = new Color(0.20f, 0.85f, 1.0f); // Celeste celestial
    Color phase2Color = new Color(0.80f, 0.35f, 1.0f); // Violeta espectral
    Color phase3Color = new Color(1.0f, 0.25f, 0.20f); // Carmesí furia

    int currentPhase = 1;
    string phaseTitle = "Fase 1: El Llanto de las Sombras";

    void OnEnable()
    {
        LloronaBossAI.OnHealthChanged += HandleHealthChanged;
        LloronaBossAI.OnPhaseChanged += HandlePhaseChanged;
        LloronaBossAI.OnDefeated += HandleBossDefeated;
        PlayerHolyWaterLauncher.OnJarsChanged += HandleJarsChanged;
    }

    void OnDisable()
    {
        LloronaBossAI.OnHealthChanged -= HandleHealthChanged;
        LloronaBossAI.OnPhaseChanged -= HandlePhaseChanged;
        LloronaBossAI.OnDefeated -= HandleBossDefeated;
        PlayerHolyWaterLauncher.OnJarsChanged -= HandleJarsChanged;
    }

    void Start()
    {
        if (bossTitleText != null) bossTitleText.text = "✦ LA LLORONA - EL ALMA EN PENA ✦";
        UpdateSubtitleWithHealth(9, 9);

        if (healthBarFill != null)
        {
            healthBarFill.color = phase1Color;
            SetFillRatio(1.0f);
        }

        var launcher = FindAnyObjectByType<PlayerHolyWaterLauncher>();
        if (launcher != null)
        {
            HandleJarsChanged(launcher.CurrentJars, launcher.MaxJars);
        }
    }

    void HandleHealthChanged(int current, int max)
    {
        float ratio = Mathf.Clamp01((float)current / Mathf.Max(1, max));
        SetFillRatio(ratio);
        UpdateSubtitleWithHealth(current, max);
    }

    void SetFillRatio(float ratio)
    {
        if (healthBarFill == null) return;

        // 1. Fill clásico si tiene sprite
        healthBarFill.fillAmount = ratio;

        // 2. Redimensionamiento de RectTransform (100% garantizado sin depender de Sprite)
        RectTransform rt = healthBarFill.rectTransform;
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(ratio, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    void UpdateSubtitleWithHealth(int current, int max)
    {
        if (phaseSubtitleText == null) return;
        phaseSubtitleText.text = $"{phaseTitle}  •  [ Vida: {current} / {max} ]";
    }

    void HandlePhaseChanged(int phase)
    {
        currentPhase = phase;
        if (healthBarFill == null || phaseSubtitleText == null) return;

        switch (phase)
        {
            case 1:
                phaseTitle = "Fase 1: El Llanto de las Sombras";
                healthBarFill.color = phase1Color;
                phaseSubtitleText.color = phase1Color;
                break;
            case 2:
                phaseTitle = "Fase 2: La Furia de las Ánimas";
                healthBarFill.color = phase2Color;
                phaseSubtitleText.color = phase2Color;
                break;
            case 3:
                phaseTitle = "Fase 3: El Alarido Final";
                healthBarFill.color = phase3Color;
                phaseSubtitleText.color = phase3Color;
                break;
        }

        var boss = FindAnyObjectByType<LloronaBossAI>();
        if (boss != null)
        {
            UpdateSubtitleWithHealth(boss.CurrentHealth, boss.MaxHealth);
        }
    }

    void HandleJarsChanged(int current, int max)
    {
        if (ammoText == null) return;

        string dots = "";
        for (int i = 0; i < max; i++)
        {
            dots += (i < current) ? "● " : "○ ";
        }

        ammoText.text = $"💧 Agua Bendita: [ {dots.Trim()} ]  (Clic / [F] para lanzar)";
        ammoText.color = (current > 0) ? new Color(0.4f, 0.9f, 1.0f) : new Color(1.0f, 0.4f, 0.4f);
    }

    void HandleBossDefeated()
    {
        if (rootPanel != null) rootPanel.SetActive(false);
    }
}
