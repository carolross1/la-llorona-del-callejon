// Assets/Scripts/UI/HUD.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la visualización de datos en pantalla durante el juego:
/// Corazones de vida, cronómetro, puntaje, objetivo y mensajes de tutorial contextual.
/// Presenta la información de forma ordenada, de alto contraste y 100% legible.
/// </summary>
public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }
    public static int ActiveShrinesLit = 0;
    public static int TotalShrines = 0;

    [Header("Corazones de Vida")]
    [SerializeField] Image[] heartImages;
    [SerializeField] Sprite activeHeartSprite;
    [SerializeField] Sprite lostHeartSprite;
    [SerializeField] Color activeHeartColor = Color.white;
    [SerializeField] Color lostHeartColor = new Color(0.25f, 0.25f, 0.25f, 0.45f);

    [Header("Barra de Vida Continua")]
    [SerializeField] GameObject healthBarRoot;
    [SerializeField] Image healthFillImage;
    [SerializeField] Text healthValueText;
    [SerializeField] CanvasGroup healthCanvasGroup;

    [Header("Indicadores de Texto")]
    [SerializeField] Text timerText;
    [SerializeField] Text scoreText;
    [SerializeField] Text objectiveText;

    [Header("Inventario de Velas / Ofrendas")]
    [SerializeField] bool showCandleInventory = true;
    [SerializeField] GameObject candleInventoryRoot;
    [SerializeField] Text candleCountText;

    [Header("Estamina")]
    [SerializeField] Image staminaFillImage;
    [SerializeField] CanvasGroup staminaCanvasGroup;

    [Header("Alerta Llorona (Ojo)")]
    [SerializeField] Image eyeIconImage;
    [SerializeField] Sprite eyeNormalSprite;
    [SerializeField] Sprite eyeAlertSprite;

    [Header("Avisos de Evento (Franja Superior - 140px)")]
    [SerializeField] GameObject eventNoticePanel;
    [SerializeField] Text eventNoticeText;
    [SerializeField] CanvasGroup eventNoticeCanvasGroup;
    Coroutine eventNoticeCoroutine;

    [Header("Tutorial Contextual (Franja Inferior - 60px)")]
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] Text tutorialText;
    [SerializeField] CanvasGroup tutorialCanvasGroup;

    Coroutine tutorialCoroutine;

    [Header("Indicador de Salida Dinámico")]
    [SerializeField] Transform exitGoalTransform;
    Transform playerCachedTransform;

    void Awake()
    {
        Instance = this;
        if (tutorialPanel != null && tutorialCanvasGroup == null)
            tutorialCanvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
        if (eventNoticePanel != null && eventNoticeCanvasGroup == null)
            eventNoticeCanvasGroup = eventNoticePanel.GetComponent<CanvasGroup>();

        NormalizeColonialHUDLayout();
    }

    void Start()
    {
        Candle.OnCandlesCountChanged += UpdateCandleCount;
        PlayerHolyWaterLauncher.OnJarsChanged += UpdateHolyWaterJars;
        UpdateCandleCount(Candle.PlayerCandlesCount);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnTimeChanged += UpdateTime;
            GameManager.Instance.OnScoreChanged += UpdateScore;

            // Inicializar valores
            int currentLives = GameSession.Instance != null ? GameSession.Instance.CurrentLives : 3;
            UpdateLives(currentLives);
            UpdateTime(GameManager.Instance.LevelTimer);
            UpdateScore(GameManager.Instance.LevelScore);
        }

        if (objectiveText != null && string.IsNullOrEmpty(objectiveText.text))
        {
            objectiveText.text = "Objetivo: Cruza el callejón sin que La Llorona te vea";
        }
    }

    /// <summary>
    /// Normaliza las proporciones y márgenes internos de las placas del HUD colonial y el medallón central
    /// para evitar que los textos o corazones invadan los bordes dorados o las flores centrales.
    /// </summary>
    void NormalizeColonialHUDLayout()
    {
        // 1. Medallón central (Tiempo y Puntos centrados en el óvalo negro)
        if (timerText != null && timerText.transform.parent != null)
        {
            RectTransform medRt = timerText.transform.parent as RectTransform;
            if (medRt != null)
            {
                medRt.anchorMin = medRt.anchorMax = new Vector2(0.5f, 1f);
                medRt.pivot = new Vector2(0.5f, 1f);
                medRt.anchoredPosition = new Vector2(0f, -8f);
                medRt.sizeDelta = new Vector2(142f, 184f);

                Image medImg = medRt.GetComponent<Image>();
                if (medImg != null)
                {
                    medImg.type = Image.Type.Simple;
                    medImg.preserveAspect = true;
                }
            }

            RectTransform rtTimer = timerText.GetComponent<RectTransform>();
            if (rtTimer != null)
            {
                rtTimer.anchorMin = rtTimer.anchorMax = new Vector2(0.5f, 0.5f);
                rtTimer.pivot = new Vector2(0.5f, 0.5f);
                rtTimer.anchoredPosition = new Vector2(0f, 6f);
                rtTimer.sizeDelta = new Vector2(96f, 26f);
            }
            timerText.fontSize = 19;
            timerText.alignment = TextAnchor.MiddleCenter;
            timerText.horizontalOverflow = HorizontalWrapMode.Overflow;
            timerText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        if (scoreText != null)
        {
            RectTransform rtScore = scoreText.GetComponent<RectTransform>();
            if (rtScore != null)
            {
                rtScore.anchorMin = rtScore.anchorMax = new Vector2(0.5f, 0.5f);
                rtScore.pivot = new Vector2(0.5f, 0.5f);
                rtScore.anchoredPosition = new Vector2(0f, -13f);
                rtScore.sizeDelta = new Vector2(96f, 20f);
            }
            scoreText.fontSize = 13;
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.horizontalOverflow = HorizontalWrapMode.Overflow;
            scoreText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        // 2. Placa izquierda (Corazones, Estamina e Inventario en 2 filas equilibradas)
        Transform leftPlaqueTr = transform.Find("HUD_Placa_Izquierda");
        if (leftPlaqueTr == null && heartImages != null && heartImages.Length > 0 && heartImages[0] != null && heartImages[0].transform.parent != null)
            leftPlaqueTr = heartImages[0].transform.parent.parent;

        if (leftPlaqueTr != null)
        {
            RectTransform rtLeft = leftPlaqueTr as RectTransform;
            if (rtLeft != null)
            {
                rtLeft.anchorMin = rtLeft.anchorMax = new Vector2(0f, 1f);
                rtLeft.pivot = new Vector2(0f, 1f);
                rtLeft.anchoredPosition = new Vector2(20f, -12f);
                rtLeft.sizeDelta = new Vector2(468f, 148f);
            }

            Image leftImg = leftPlaqueTr.GetComponent<Image>();
            if (leftImg != null)
            {
                leftImg.type = Image.Type.Simple;
                leftImg.preserveAspect = false;
            }

            Transform heartsTr = leftPlaqueTr.Find("Corazones_Vida");
            if (heartsTr == null && heartImages != null && heartImages.Length > 0 && heartImages[0] != null)
                heartsTr = heartImages[0].transform.parent;

            if (heartsTr != null)
            {
                RectTransform rtH = heartsTr as RectTransform;
                if (rtH != null)
                {
                    rtH.anchorMin = rtH.anchorMax = new Vector2(0f, 0.5f);
                    rtH.pivot = new Vector2(0f, 0.5f);
                    rtH.anchoredPosition = new Vector2(52f, 14f);
                    rtH.sizeDelta = new Vector2(140f, 36f);
                }

                float hSize = showCandleInventory ? 32f : 36f;
                float hStep = showCandleInventory ? 36f : 42f;
                if (heartImages != null)
                {
                    for (int i = 0; i < heartImages.Length; i++)
                    {
                        if (heartImages[i] == null) continue;
                        RectTransform hRt = heartImages[i].GetComponent<RectTransform>();
                        if (hRt != null)
                        {
                            hRt.anchorMin = hRt.anchorMax = new Vector2(0f, 0.5f);
                            hRt.pivot = new Vector2(0f, 0.5f);
                            hRt.sizeDelta = new Vector2(hSize, hSize);
                            hRt.anchoredPosition = new Vector2(i * hStep, 0f);
                        }
                    }
                }
            }

            Transform stamRootTr = leftPlaqueTr.Find("Estamina_Barra");
            if (stamRootTr == null && staminaCanvasGroup != null)
                stamRootTr = staminaCanvasGroup.transform;

            if (stamRootTr != null)
            {
                RectTransform rtStam = stamRootTr as RectTransform;
                Transform lblTr = stamRootTr.Find("StaminaLabel");
                Transform barTr = stamRootTr.Find("MarcoBarra");
                if (barTr == null && staminaFillImage != null)
                    barTr = staminaFillImage.transform.parent;

                if (!showCandleInventory)
                {
                    if (rtStam != null)
                    {
                        rtStam.anchorMin = rtStam.anchorMax = new Vector2(0f, 0.5f);
                        rtStam.pivot = new Vector2(0f, 0.5f);
                        rtStam.anchoredPosition = new Vector2(52f, -14f);
                        rtStam.sizeDelta = new Vector2(364f, 22f);
                    }
                    if (lblTr != null)
                    {
                        Text stTxt = lblTr.GetComponent<Text>();
                        if (stTxt != null) { stTxt.text = "ESTAMINA"; stTxt.fontSize = 14; }
                        RectTransform lRt = lblTr as RectTransform;
                        if (lRt != null)
                        {
                            lRt.anchorMin = lRt.anchorMax = new Vector2(0f, 0.5f);
                            lRt.pivot = new Vector2(0f, 0.5f);
                            lRt.anchoredPosition = Vector2.zero;
                            lRt.sizeDelta = new Vector2(88f, 20f);
                        }
                    }
                    if (barTr != null)
                    {
                        RectTransform bRt = barTr as RectTransform;
                        if (bRt != null)
                        {
                            bRt.anchorMin = bRt.anchorMax = new Vector2(0f, 0.5f);
                            bRt.pivot = new Vector2(0f, 0.5f);
                            bRt.anchoredPosition = new Vector2(92f, 0f);
                            bRt.sizeDelta = new Vector2(264f, 16f);
                        }
                    }
                }
                else
                {
                    // Con inventario (Nivel 3 y 4): Estamina va en Fila 1 derecha (evitando la flor central)
                    if (rtStam != null)
                    {
                        rtStam.anchorMin = rtStam.anchorMax = new Vector2(0f, 0.5f);
                        rtStam.pivot = new Vector2(0f, 0.5f);
                        rtStam.anchoredPosition = new Vector2(236f, 14f);
                        rtStam.sizeDelta = new Vector2(180f, 20f);
                    }
                    if (lblTr != null)
                    {
                        Text stTxt = lblTr.GetComponent<Text>();
                        if (stTxt != null) { stTxt.text = "EST"; stTxt.fontSize = 12; }
                        RectTransform lRt = lblTr as RectTransform;
                        if (lRt != null)
                        {
                            lRt.anchorMin = lRt.anchorMax = new Vector2(0f, 0.5f);
                            lRt.pivot = new Vector2(0f, 0.5f);
                            lRt.anchoredPosition = Vector2.zero;
                            lRt.sizeDelta = new Vector2(34f, 18f);
                        }
                    }
                    if (barTr != null)
                    {
                        RectTransform bRt = barTr as RectTransform;
                        if (bRt != null)
                        {
                            bRt.anchorMin = bRt.anchorMax = new Vector2(0f, 0.5f);
                            bRt.pivot = new Vector2(0f, 0.5f);
                            bRt.anchoredPosition = new Vector2(36f, 0f);
                            bRt.sizeDelta = new Vector2(140f, 14f);
                        }
                    }
                }

                // Marco limpio para la barra de estamina
                if (barTr != null)
                {
                    Image barBg = barTr.GetComponent<Image>();
                    if (barBg != null)
                    {
                        barBg.sprite = ColonialMenuModalHelper.GetWhiteSprite();
                        barBg.type = Image.Type.Simple;
                        barBg.color = new Color(0.06f, 0.04f, 0.10f, 0.96f);
                    }
                    Outline barOut = barTr.GetComponent<Outline>();
                    if (barOut == null) barOut = barTr.gameObject.AddComponent<Outline>();
                    barOut.effectColor = new Color(0.84f, 0.66f, 0.26f, 0.90f);
                    barOut.effectDistance = new Vector2(1.2f, -1.2f);
                }

                if (staminaFillImage != null)
                {
                    staminaFillImage.sprite = ColonialMenuModalHelper.GetWhiteSprite();
                    staminaFillImage.type = Image.Type.Filled;
                    staminaFillImage.fillMethod = Image.FillMethod.Horizontal;
                    staminaFillImage.fillOrigin = 0;
                    staminaFillImage.color = new Color(0.96f, 0.78f, 0.20f, 1f);
                    RectTransform fRt = staminaFillImage.GetComponent<RectTransform>();
                    if (fRt != null)
                    {
                        fRt.anchorMin = new Vector2(0.015f, 0.16f);
                        fRt.anchorMax = new Vector2(0.985f, 0.84f);
                        fRt.offsetMin = Vector2.zero;
                        fRt.offsetMax = Vector2.zero;
                    }
                }
            }

            if (candleInventoryRoot != null)
            {
                RectTransform rtC = candleInventoryRoot.GetComponent<RectTransform>();
                if (rtC != null)
                {
                    rtC.anchorMin = rtC.anchorMax = new Vector2(0f, 0.5f);
                    rtC.pivot = new Vector2(0f, 0.5f);
                    rtC.anchoredPosition = new Vector2(52f, -15f);
                    rtC.sizeDelta = new Vector2(364f, 26f);
                }

                Transform iconTr = candleInventoryRoot.transform.Find("IconoInventario") ?? candleInventoryRoot.transform.Find("IconoVela");
                if (iconTr != null)
                {
                    RectTransform icRt = iconTr as RectTransform;
                    if (icRt != null)
                    {
                        icRt.anchorMin = icRt.anchorMax = new Vector2(0f, 0.5f);
                        icRt.pivot = new Vector2(0f, 0.5f);
                        icRt.anchoredPosition = Vector2.zero;
                        icRt.sizeDelta = new Vector2(26f, 26f);
                    }
                }

                if (candleCountText != null)
                {
                    candleCountText.fontSize = 15;
                    candleCountText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    candleCountText.verticalOverflow = VerticalWrapMode.Overflow;
                    RectTransform cTxtRt = candleCountText.GetComponent<RectTransform>();
                    if (cTxtRt != null)
                    {
                        cTxtRt.anchorMin = cTxtRt.anchorMax = new Vector2(0f, 0.5f);
                        cTxtRt.pivot = new Vector2(0f, 0.5f);
                        cTxtRt.anchoredPosition = new Vector2(32f, 0f);
                        cTxtRt.sizeDelta = new Vector2(330f, 24f);
                    }
                }
            }
        }

        // 3. Placa derecha (Brújula, Ojo de Alerta, Objetivo y Distancia a Salida)
        if (objectiveText != null && objectiveText.transform.parent != null)
        {
            RectTransform rtRight = objectiveText.transform.parent as RectTransform;
            if (rtRight != null)
            {
                rtRight.anchorMin = rtRight.anchorMax = new Vector2(1f, 1f);
                rtRight.pivot = new Vector2(1f, 1f);
                rtRight.anchoredPosition = new Vector2(-20f, -12f);
                rtRight.sizeDelta = new Vector2(468f, 148f);

                Image rightImg = rtRight.GetComponent<Image>();
                if (rightImg != null)
                {
                    rightImg.type = Image.Type.Simple;
                    rightImg.preserveAspect = false;
                }

                Transform compTr = rtRight.Find("IconoBrujula");
                if (compTr != null)
                {
                    RectTransform compRt = compTr as RectTransform;
                    if (compRt != null)
                    {
                        compRt.anchorMin = compRt.anchorMax = new Vector2(0f, 0.5f);
                        compRt.pivot = new Vector2(0f, 0.5f);
                        compRt.anchoredPosition = new Vector2(50f, 0f);
                        compRt.sizeDelta = new Vector2(38f, 38f);
                    }
                }

                if (eyeIconImage != null)
                {
                    RectTransform eyeRt = eyeIconImage.GetComponent<RectTransform>();
                    if (eyeRt != null)
                    {
                        eyeRt.anchorMin = eyeRt.anchorMax = new Vector2(0f, 0.5f);
                        eyeRt.pivot = new Vector2(0f, 0.5f);
                        eyeRt.anchoredPosition = new Vector2(94f, 0f);
                        eyeRt.sizeDelta = new Vector2(38f, 32f);
                    }
                }

                RectTransform objRt = objectiveText.GetComponent<RectTransform>();
                if (objRt != null)
                {
                    objRt.anchorMin = objRt.anchorMax = new Vector2(0f, 0.5f);
                    objRt.pivot = new Vector2(0f, 0.5f);
                    objRt.anchoredPosition = new Vector2(140f, 11f);
                    objRt.sizeDelta = new Vector2(278f, 34f);
                }
                objectiveText.fontSize = 14;
                objectiveText.lineSpacing = 0.95f;
                objectiveText.alignment = TextAnchor.MiddleLeft;
                objectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
                objectiveText.verticalOverflow = VerticalWrapMode.Overflow;

                Transform metaTr = rtRight.Find("MetaText");
                if (metaTr != null)
                {
                    Text metaTxt = metaTr.GetComponent<Text>();
                    if (metaTxt != null)
                    {
                        metaTxt.fontSize = 15;
                        metaTxt.alignment = TextAnchor.MiddleLeft;
                        metaTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                        metaTxt.verticalOverflow = VerticalWrapMode.Overflow;
                    }
                    RectTransform metaRt = metaTr as RectTransform;
                    if (metaRt != null)
                    {
                        metaRt.anchorMin = metaRt.anchorMax = new Vector2(0f, 0.5f);
                        metaRt.pivot = new Vector2(0f, 0.5f);
                        metaRt.anchoredPosition = new Vector2(140f, -15f);
                        metaRt.sizeDelta = new Vector2(278f, 22f);
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        Candle.OnCandlesCountChanged -= UpdateCandleCount;
        PlayerHolyWaterLauncher.OnJarsChanged -= UpdateHolyWaterJars;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnTimeChanged -= UpdateTime;
            GameManager.Instance.OnScoreChanged -= UpdateScore;
        }
    }

    public void UpdateCandleCount(int count)
    {
        if (!showCandleInventory)
        {
            if (candleInventoryRoot != null) candleInventoryRoot.SetActive(false);
            return;
        }

        if (candleInventoryRoot != null)
            candleInventoryRoot.SetActive(true);
        if (candleCountText != null)
        {
            if (TotalShrines > 0)
            {
                string statusVelas = count > 0
                    ? $"Velas: {count} [F]"
                    : "Velas: 0 (Vacío)";
                candleCountText.text = $"Nichos: {ActiveShrinesLit}/{TotalShrines}  ·  {statusVelas}";
            }
            else
            {
                candleCountText.text = count > 0
                    ? $"Velas: {count}  [F] Ofrendar"
                    : "Velas: 0 (Sin velas)";
            }

            candleCountText.color = count > 0
                ? new Color(1.0f, 0.94f, 0.45f)
                : new Color(0.85f, 0.85f, 0.85f, 0.88f);
        }
    }

    public void UpdateHolyWaterJars(int current, int max)
    {
        if (!showCandleInventory || candleCountText == null) return;
        candleCountText.text = $"Agua Bendita: {current}/{max}  [Clic / F]";
        candleCountText.color = current > 0
            ? new Color(0.40f, 0.95f, 1.0f)
            : new Color(0.85f, 0.85f, 0.85f, 0.88f);
    }

    public void UpdateLives(int lives)
    {
        int max = (GameSession.Instance != null && GameSession.Instance.MaxLives > 0) ? GameSession.Instance.MaxLives : 3;

        // 1. Actualizar barra continua de vida (si está asignada)
        if (healthFillImage != null)
        {
            float targetFill = Mathf.Clamp01((float)lives / max);
            healthFillImage.fillAmount = targetFill;
        }

        if (healthValueText != null)
        {
            healthValueText.text = $"VIDA {lives}/{max}";
        }

        // 2. Actualizar corazones discretos (si están asignados)
        if (heartImages == null) return;
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                if (activeHeartSprite != null && lostHeartSprite != null)
                {
                    heartImages[i].sprite = (i < lives) ? activeHeartSprite : lostHeartSprite;
                    heartImages[i].color = (i < lives) ? activeHeartColor : lostHeartColor;
                }
                else
                {
                    heartImages[i].color = (i < lives) ? activeHeartColor : lostHeartColor;
                }
            }
        }
    }

    public void UpdateTime(float seconds)
    {
        if (timerText == null) return;
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);
        timerText.text = $"{min:00}:{sec:00}";
    }

    public void UpdateScore(int score)
    {
        if (scoreText == null) return;
        scoreText.text = $"{score} PTS";
    }

    public void SetObjective(string text)
    {
        if (objectiveText != null)
            objectiveText.text = text;
    }

    /// <summary>
    /// Actualiza la barra de estamina con valor normalizado [0..1] y parpadeo si llega a 0.
    /// </summary>
    public void UpdateStamina(float normalized)
    {
        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = Mathf.Clamp01(normalized);
        }

        if (staminaCanvasGroup != null)
        {
            if (normalized <= 0.01f)
            {
                // Parpadeo cuando llega a cero
                staminaCanvasGroup.alpha = Mathf.PingPong(Time.time * 6f, 0.7f) + 0.3f;
            }
            else
            {
                staminaCanvasGroup.alpha = 1f;
            }
        }
    }

    /// <summary>
    /// Enciende o apaga el icono del ojo rojo alertado por la Llorona.
    /// </summary>
    public void SetEyeAlert(bool alert)
    {
        if (eyeIconImage != null)
        {
            if (alert && eyeAlertSprite != null)
                eyeIconImage.sprite = eyeAlertSprite;
            else if (!alert && eyeNormalSprite != null)
                eyeIconImage.sprite = eyeNormalSprite;

            eyeIconImage.color = alert ? new Color(1f, 0.25f, 0.25f, 1f) : Color.white;
        }
    }

    /// <summary>
    /// Muestra un aviso de evento breve ("Pisaste lodo", "El perro despertó", etc.)
    /// en la franja superior (a 140px del borde) sin invadir el centro.
    /// </summary>
    public void ShowEventNotice(string message, float duration = 2.5f)
    {
        if (eventNoticeCoroutine != null)
            StopCoroutine(eventNoticeCoroutine);

        if (eventNoticePanel != null)
        {
            eventNoticeCoroutine = StartCoroutine(EventNoticeRoutine(message, duration));
        }
        else
        {
            // Respaldo contextual
            ShowTutorial(message, duration);
        }
    }

    IEnumerator EventNoticeRoutine(string message, float duration)
    {
        if (eventNoticePanel != null) eventNoticePanel.SetActive(true);
        if (eventNoticeText != null) eventNoticeText.text = message;

        if (eventNoticeCanvasGroup != null)
        {
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.unscaledDeltaTime;
                eventNoticeCanvasGroup.alpha = Mathf.Clamp01(t / 0.2f);
                yield return null;
            }
            eventNoticeCanvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(duration);

            t = 0.2f;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                eventNoticeCanvasGroup.alpha = Mathf.Clamp01(t / 0.2f);
                yield return null;
            }
            eventNoticeCanvasGroup.alpha = 0f;
        }
        else
        {
            yield return new WaitForSecondsRealtime(duration);
        }

        if (eventNoticePanel != null) eventNoticePanel.SetActive(false);
    }

    /// <summary>
    /// Muestra un mensaje de tutorial contextual durante un número de segundos.
    /// </summary>
    public void ShowTutorial(string message, float duration = 4.5f)
    {
        if (tutorialCoroutine != null)
            StopCoroutine(tutorialCoroutine);

        tutorialCoroutine = StartCoroutine(TutorialRoutine(message, duration));
    }

    IEnumerator TutorialRoutine(string message, float duration)
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        if (tutorialText != null) tutorialText.text = message;

        if (tutorialCanvasGroup != null)
        {
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.unscaledDeltaTime;
                tutorialCanvasGroup.alpha = Mathf.Clamp01(t / 0.25f);
                yield return null;
            }
            tutorialCanvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(duration);

            t = 0.25f;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                tutorialCanvasGroup.alpha = Mathf.Clamp01(t / 0.25f);
                yield return null;
            }
            tutorialCanvasGroup.alpha = 0f;
        }
        else
        {
            yield return new WaitForSecondsRealtime(duration);
        }

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }
}
