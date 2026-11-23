// Assets/Scripts/UI/ColonialMenuModalHelper.cs
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Constructor y estilizador en tiempo de ejecución para los modales de Pausa,
/// Opciones (con barras interactivas de Música, SFX y Pantalla Completa) y
/// Controles del Juego, garantizando estética colonial consistente en todos los niveles.
/// </summary>
public static class ColonialMenuModalHelper
{
    static Sprite cachedWhiteSprite;

    public static Sprite GetWhiteSprite()
    {
        if (cachedWhiteSprite != null) return cachedWhiteSprite;
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color[] cols = new Color[16];
        for (int i = 0; i < 16; i++) cols[i] = Color.white;
        tex.SetPixels(cols);
        tex.Apply();
        cachedWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        return cachedWhiteSprite;
    }

    public static Font GetMenuFont(Font fallback = null)
    {
        if (fallback != null) return fallback;
        Text anyText = UnityEngine.Object.FindAnyObjectByType<Text>();
        if (anyText != null && anyText.font != null) return anyText.font;
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    /// <summary>
    /// Estiliza un panel modal para que luzca como un marco colonial oscuro con doble borde dorado
    /// y línea divisoria bajo el título, eliminando cajas negras planas o recortes defectuosos.
    /// </summary>
    public static void StyleColonialModalPanel(GameObject panelGo, string titleText, Vector2 size, Font font)
    {
        if (panelGo == null) return;
        font = GetMenuFont(font);

        RectTransform rt = panelGo.GetComponent<RectTransform>();
        if (rt == null) rt = panelGo.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image outerImg = panelGo.GetComponent<Image>();
        if (outerImg == null) outerImg = panelGo.AddComponent<Image>();
        outerImg.sprite = GetWhiteSprite();
        outerImg.type = Image.Type.Simple;
        outerImg.color = new Color(0.82f, 0.64f, 0.26f, 0.98f); // Borde exterior dorado colonial

        Outline outerShadow = panelGo.GetComponent<Outline>();
        if (outerShadow == null) outerShadow = panelGo.AddComponent<Outline>();
        outerShadow.effectColor = new Color(0f, 0f, 0f, 0.90f);
        outerShadow.effectDistance = new Vector2(4f, -4f);

        // Fondo interior oscuro (terciopelo nocturno)
        Transform oldOpaqueTr = panelGo.transform.Find("FondoOpacoModal");
        GameObject oldOpaqueGo;
        if (oldOpaqueTr == null)
        {
            oldOpaqueGo = new GameObject("FondoOpacoModal", typeof(RectTransform));
            oldOpaqueGo.transform.SetParent(panelGo.transform, false);
        }
        else
        {
            oldOpaqueGo = oldOpaqueTr.gameObject;
        }
        RectTransform inRt = oldOpaqueGo.GetComponent<RectTransform>();
        if (inRt == null) inRt = oldOpaqueGo.AddComponent<RectTransform>();
        oldOpaqueGo.transform.SetAsFirstSibling();
        inRt.anchorMin = Vector2.zero;
        inRt.anchorMax = Vector2.one;
        inRt.offsetMin = new Vector2(4f, 4f);
        inRt.offsetMax = new Vector2(-4f, -4f);

        Image inImg = oldOpaqueGo.GetComponent<Image>();
        if (inImg == null) inImg = oldOpaqueGo.AddComponent<Image>();
        inImg.sprite = GetWhiteSprite();
        inImg.color = new Color(0.07f, 0.05f, 0.11f, 0.98f);

        // Marco interno fino dorado
        Transform innerBorder = panelGo.transform.Find("MarcoInternoDorado");
        if (innerBorder == null)
        {
            GameObject ibGo = new GameObject("MarcoInternoDorado", typeof(RectTransform));
            ibGo.transform.SetParent(panelGo.transform, false);
            ibGo.transform.SetSiblingIndex(1);
            RectTransform ibRt = ibGo.GetComponent<RectTransform>();
            ibRt.anchorMin = Vector2.zero;
            ibRt.anchorMax = Vector2.one;
            ibRt.offsetMin = new Vector2(12f, 12f);
            ibRt.offsetMax = new Vector2(-12f, -12f);
            Image ibImg = ibGo.AddComponent<Image>();
            ibImg.sprite = GetWhiteSprite();
            ibImg.color = new Color(0.11f, 0.08f, 0.16f, 0.98f);
            Outline ibOut = ibGo.AddComponent<Outline>();
            ibOut.effectColor = new Color(0.72f, 0.54f, 0.22f, 0.55f);
            ibOut.effectDistance = new Vector2(1.5f, -1.5f);
        }

        // Título superior bien proporcionado
        Transform titleTr = panelGo.transform.Find("TituloModal");
        GameObject titleGo;
        Text titleComp;
        if (titleTr == null)
        {
            titleGo = CreateColonialText(panelGo.transform, "TituloModal", titleText, 30,
                new Color(1f, 0.88f, 0.38f), TextAnchor.MiddleCenter, font);
            titleComp = titleGo.GetComponent<Text>();
        }
        else
        {
            titleGo = titleTr.gameObject;
            titleComp = titleGo.GetComponent<Text>();
            if (titleComp != null)
            {
                titleComp.text = titleText;
                titleComp.fontSize = 30;
                titleComp.fontStyle = FontStyle.Bold;
                titleComp.color = new Color(1f, 0.88f, 0.38f);
                titleComp.alignment = TextAnchor.MiddleCenter;
            }
        }
        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        if (titleRt == null) titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -22f);
        titleRt.sizeDelta = new Vector2(size.x - 48f, 44f);
        titleGo.transform.SetAsLastSibling();

        // Línea divisoria ornamental dorada bajo el título
        Transform divTr = panelGo.transform.Find("DivisorTitulo");
        if (divTr == null)
        {
            GameObject divGo = new GameObject("DivisorTitulo", typeof(RectTransform));
            divGo.transform.SetParent(panelGo.transform, false);
            RectTransform divRt = divGo.GetComponent<RectTransform>();
            divRt.anchorMin = divRt.anchorMax = new Vector2(0.5f, 1f);
            divRt.pivot = new Vector2(0.5f, 0.5f);
            divRt.anchoredPosition = new Vector2(0f, -70f);
            divRt.sizeDelta = new Vector2(size.x - 80f, 2f);
            Image divImg = divGo.AddComponent<Image>();
            divImg.sprite = GetWhiteSprite();
            divImg.color = new Color(0.85f, 0.68f, 0.28f, 0.75f);
        }
    }

    /// <summary>
    /// Crea o re-estiliza un botón con acabado colonial limpio (sin usar barras estiradas ni deformadas).
    /// </summary>
    public static Button EnsureColonialButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Vector2 size, int fontSize, bool isPrimary, Font font)
    {
        font = GetMenuFont(font);
        Transform existing = parent.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        if (existing == null) go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        go.transform.SetAsLastSibling();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        if (img == null) img = go.AddComponent<Image>();
        img.sprite = GetWhiteSprite();
        img.type = Image.Type.Simple;

        Color normalBg = isPrimary
            ? new Color(0.36f, 0.18f, 0.10f, 0.98f)
            : new Color(0.16f, 0.11f, 0.22f, 0.98f);
        img.color = Color.white;

        Outline border = go.GetComponent<Outline>();
        if (border == null) border = go.AddComponent<Outline>();
        border.effectColor = isPrimary
            ? new Color(1.0f, 0.80f, 0.30f, 0.98f)
            : new Color(0.78f, 0.60f, 0.25f, 0.90f);
        border.effectDistance = new Vector2(2f, -2f);

        Button btn = go.GetComponent<Button>();
        if (btn == null) btn = go.AddComponent<Button>();
        btn.transition = Selectable.Transition.ColorTint;
        ColorBlock cb = btn.colors;
        cb.normalColor = normalBg;
        cb.highlightedColor = isPrimary
            ? new Color(0.52f, 0.26f, 0.12f, 1f)
            : new Color(0.28f, 0.19f, 0.38f, 1f);
        cb.pressedColor = new Color(0.12f, 0.08f, 0.16f, 1f);
        cb.selectedColor = cb.highlightedColor;
        cb.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        btn.colors = cb;
        btn.targetGraphic = img;

        if (go.GetComponent<ButtonHoverEffect>() == null)
            go.AddComponent<ButtonHoverEffect>();

        Transform txtTr = go.transform.Find("Text");
        if (txtTr == null && go.transform.childCount > 0)
            txtTr = go.transform.GetChild(0);

        Text t;
        if (txtTr == null)
        {
            GameObject tGo = CreateColonialText(go.transform, "Text", label, fontSize,
                isPrimary ? new Color(1f, 0.96f, 0.82f) : new Color(0.96f, 0.90f, 0.74f),
                TextAnchor.MiddleCenter, font);
            t = tGo.GetComponent<Text>();
        }
        else
        {
            t = txtTr.GetComponent<Text>();
            if (t != null)
            {
                t.text = label;
                t.fontSize = fontSize;
                t.fontStyle = FontStyle.Bold;
                t.alignment = TextAnchor.MiddleCenter;
                t.color = isPrimary ? new Color(1f, 0.96f, 0.82f) : new Color(0.96f, 0.90f, 0.74f);
            }
        }
        if (t != null)
        {
            RectTransform tRt = t.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(8f, 2f);
            tRt.offsetMax = new Vector2(-8f, -2f);
        }

        return btn;
    }

    /// <summary>
    /// Construye el panel interactivo de Opciones (Volumen de Música, Volumen SFX, Pantalla Completa y acceso a Controles).
    /// </summary>
    public static GameObject BuildOptionsSubPanel(Transform parentOverlay, Font font,
        Action onCloseClicked, Action onOpenControlsClicked)
    {
        font = GetMenuFont(font);
        Transform existing = parentOverlay.Find("PanelOpciones");
        if (existing != null)
        {
            existing.gameObject.name = "PanelOpciones_Old";
            existing.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(existing.gameObject);
        }

        GameObject panel = new GameObject("PanelOpciones", typeof(RectTransform));
        panel.transform.SetParent(parentOverlay, false);
        StyleColonialModalPanel(panel, "OPCIONES", new Vector2(620f, 500f), font);

        // 1. Control de Volumen de Música
        CreateVolumeRow(panel.transform, "FilaMusica", "VOLUMEN DE MÚSICA",
            new Vector2(0f, 110f), SaveSystem.GetMusicVolume(), font,
            val =>
            {
                SaveSystem.SetMusicVolume(val);
            });

        // 2. Control de Volumen de Efectos (SFX)
        CreateVolumeRow(panel.transform, "FilaSFX", "EFECTOS DE SONIDO (SFX)",
            new Vector2(0f, 20f), SaveSystem.GetSfxVolume(), font,
            val =>
            {
                SaveSystem.SetSfxVolume(val);
            });

        // 3. Botón de Pantalla Completa
        bool isFull = Screen.fullScreen;
        Button fullBtn = EnsureColonialButton(panel.transform, "Btn_PantallaCompleta",
            $"PANTALLA COMPLETA: {(isFull ? "ACTIVA" : "VENTANA")}",
            new Vector2(0f, -68f), new Vector2(420f, 48f), 19, false, font);
        Text fullTxt = fullBtn.GetComponentInChildren<Text>();
        fullBtn.onClick.RemoveAllListeners();
        fullBtn.onClick.AddListener(() =>
        {
            bool nextState = !Screen.fullScreen;
            Screen.fullScreen = nextState;
            if (fullTxt != null)
                fullTxt.text = $"PANTALLA COMPLETA: {(nextState ? "ACTIVA" : "VENTANA")}";
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        });

        // 4. Botón para abrir Controles desde Opciones
        if (onOpenControlsClicked != null)
        {
            Button ctrlBtn = EnsureColonialButton(panel.transform, "Btn_VerControles",
                "VER CONTROLES DEL JUEGO",
                new Vector2(0f, -128f), new Vector2(420f, 48f), 19, false, font);
            ctrlBtn.onClick.RemoveAllListeners();
            ctrlBtn.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
                onOpenControlsClicked.Invoke();
            });
        }

        // 5. Botón Volver / Cerrar
        Button closeBtn = EnsureColonialButton(panel.transform, "Btn_CerrarOpciones",
            "← VOLVER",
            new Vector2(0f, -195f), new Vector2(280f, 52f), 21, true, font);
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
            onCloseClicked?.Invoke();
        });

        panel.SetActive(false);
        return panel;
    }

    static void CreateVolumeRow(Transform parent, string name, string labelText,
        Vector2 anchoredPos, float initialValue, Font font, Action<float> onValueChanged)
    {
        GameObject row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(520f, 74f);

        // Etiqueta superior izquierda
        GameObject lblGo = CreateColonialText(row.transform, "Label", labelText, 18,
            new Color(1f, 0.92f, 0.68f), TextAnchor.MiddleLeft, font);
        RectTransform lblRt = lblGo.GetComponent<RectTransform>();
        lblRt.anchorMin = lblRt.anchorMax = new Vector2(0f, 1f);
        lblRt.pivot = new Vector2(0f, 1f);
        lblRt.anchoredPosition = new Vector2(10f, -2f);
        lblRt.sizeDelta = new Vector2(360f, 26f);

        // Porcentaje superior derecha
        GameObject pctGo = CreateColonialText(row.transform, "ValueText",
            $"{Mathf.RoundToInt(initialValue * 100f)}%", 18,
            new Color(1f, 0.86f, 0.32f), TextAnchor.MiddleRight, font);
        RectTransform pctRt = pctGo.GetComponent<RectTransform>();
        pctRt.anchorMin = pctRt.anchorMax = new Vector2(1f, 1f);
        pctRt.pivot = new Vector2(1f, 1f);
        pctRt.anchoredPosition = new Vector2(-10f, -2f);
        pctRt.sizeDelta = new Vector2(120f, 26f);
        Text pctTxt = pctGo.GetComponent<Text>();

        // Slider interactivo central
        GameObject sliderGo = new GameObject("Slider", typeof(RectTransform));
        sliderGo.transform.SetParent(row.transform, false);
        RectTransform slRt = sliderGo.GetComponent<RectTransform>();
        slRt.anchorMin = slRt.anchorMax = new Vector2(0.5f, 0f);
        slRt.pivot = new Vector2(0.5f, 0.5f);
        slRt.anchoredPosition = new Vector2(0f, 20f);
        slRt.sizeDelta = new Vector2(390f, 24f);

        // Fondo de la barra
        GameObject bgGo = new GameObject("Background", typeof(RectTransform));
        bgGo.transform.SetParent(sliderGo.transform, false);
        RectTransform bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.2f);
        bgRt.anchorMax = new Vector2(1f, 0.8f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgGo.AddComponent<Image>();
        bgImg.sprite = GetWhiteSprite();
        bgImg.color = new Color(0.05f, 0.03f, 0.08f, 0.98f);
        Outline bgOut = bgGo.AddComponent<Outline>();
        bgOut.effectColor = new Color(0.75f, 0.58f, 0.24f, 0.85f);
        bgOut.effectDistance = new Vector2(1.5f, -1.5f);

        // Área de relleno
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGo.transform, false);
        RectTransform faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0f, 0.2f);
        faRt.anchorMax = new Vector2(1f, 0.8f);
        faRt.offsetMin = new Vector2(3f, 3f);
        faRt.offsetMax = new Vector2(-3f, -3f);

        GameObject fillGo = new GameObject("Fill", typeof(RectTransform));
        fillGo.transform.SetParent(fillArea.transform, false);
        RectTransform fRt = fillGo.GetComponent<RectTransform>();
        fRt.anchorMin = Vector2.zero;
        fRt.anchorMax = Vector2.one;
        fRt.offsetMin = Vector2.zero;
        fRt.offsetMax = Vector2.zero;
        Image fillImg = fillGo.AddComponent<Image>();
        fillImg.sprite = GetWhiteSprite();
        fillImg.color = new Color(0.95f, 0.75f, 0.22f, 1f);

        // Manija deslizable (Handle)
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderGo.transform, false);
        RectTransform haRt = handleArea.GetComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero;
        haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(8f, 0f);
        haRt.offsetMax = new Vector2(-8f, 0f);

        GameObject handleGo = new GameObject("Handle", typeof(RectTransform));
        handleGo.transform.SetParent(handleArea.transform, false);
        RectTransform hRt = handleGo.GetComponent<RectTransform>();
        hRt.sizeDelta = new Vector2(18f, 26f);
        Image handleImg = handleGo.AddComponent<Image>();
        handleImg.sprite = GetWhiteSprite();
        handleImg.color = new Color(1f, 0.94f, 0.72f, 1f);
        Outline hOut = handleGo.AddComponent<Outline>();
        hOut.effectColor = new Color(0.2f, 0.1f, 0.02f, 1f);
        hOut.effectDistance = new Vector2(1.5f, -1.5f);

        Slider slider = sliderGo.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.fillRect = fRt;
        slider.handleRect = hRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.SetValueWithoutNotify(Mathf.Clamp01(initialValue));

        slider.onValueChanged.AddListener(v =>
        {
            if (pctTxt != null)
                pctTxt.text = $"{Mathf.RoundToInt(v * 100f)}%";
            onValueChanged?.Invoke(v);
        });

        // Botones [-] y [+] a los lados del slider para ajuste preciso con clic
        Button minusBtn = EnsureColonialButton(row.transform, "Btn_Menos", "-",
            new Vector2(-232f, -16f), new Vector2(44f, 34f), 22, false, font);
        minusBtn.onClick.RemoveAllListeners();
        minusBtn.onClick.AddListener(() =>
        {
            slider.value = Mathf.Clamp01(slider.value - 0.1f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        });

        Button plusBtn = EnsureColonialButton(row.transform, "Btn_Mas", "+",
            new Vector2(232f, -16f), new Vector2(44f, 34f), 22, false, font);
        plusBtn.onClick.RemoveAllListeners();
        plusBtn.onClick.AddListener(() =>
        {
            slider.value = Mathf.Clamp01(slider.value + 0.1f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        });
    }

    /// <summary>
    /// Construye el panel informativo con todos los Controles del Juego para que el jugador sepa qué teclas usar.
    /// </summary>
    public static GameObject BuildControlsSubPanel(Transform parentOverlay, Font font, Action onCloseClicked)
    {
        font = GetMenuFont(font);
        Transform existing = parentOverlay.Find("PanelControles");
        if (existing != null)
        {
            existing.gameObject.name = "PanelControles_Old";
            existing.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(existing.gameObject);
        }

        GameObject panel = new GameObject("PanelControles", typeof(RectTransform));
        panel.transform.SetParent(parentOverlay, false);
        StyleColonialModalPanel(panel, "CONTROLES DEL JUEGO", new Vector2(680f, 540f), font);

        string[,] controls = new string[,]
        {
            { "W, A, S, D  /  Flechas", "Mover a Carlos por el escenario" },
            { "Shift Izquierdo",        "Correr más rápido (consume estamina)" },
            { "Espacio",                "Saltar obstáculos bajos (Nivel 2)" },
            { "Tecla [E]",              "Esconderse / Recoger veladoras o Agua Bendita" },
            { "Tecla [F]",              "Ofrendar vela / Consagrar nicho / Agua Bendita" },
            { "Clic Izquierdo",         "Lanzar Agua Bendita hacia el cursor (Nivel 4)" },
            { "Esc  o  Tecla [P]",      "Pausar o reanudar la partida" }
        };

        float startY = 138f;
        float stepY = 46f;

        for (int i = 0; i < controls.GetLength(0); i++)
        {
            float y = startY - i * stepY;
            CreateControlRow(panel.transform, $"ControlRow_{i}", controls[i, 0], controls[i, 1], y, font);
        }

        Button closeBtn = EnsureColonialButton(panel.transform, "Btn_CerrarControles",
            "← VOLVER",
            new Vector2(0f, -212f), new Vector2(280f, 52f), 21, true, font);
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
            onCloseClicked?.Invoke();
        });

        panel.SetActive(false);
        return panel;
    }

    static void CreateControlRow(Transform parent, string name, string keyText, string actionText, float yPos, Font font)
    {
        GameObject row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, yPos);
        rt.sizeDelta = new Vector2(610f, 38f);

        // Fondo sutil de fila
        Image rowBg = row.AddComponent<Image>();
        rowBg.sprite = GetWhiteSprite();
        rowBg.color = new Color(0.14f, 0.10f, 0.20f, 0.70f);

        // Insignia de tecla (izquierda)
        GameObject keyBadge = new GameObject("KeyBadge", typeof(RectTransform));
        keyBadge.transform.SetParent(row.transform, false);
        RectTransform kbRt = keyBadge.GetComponent<RectTransform>();
        kbRt.anchorMin = kbRt.anchorMax = new Vector2(0f, 0.5f);
        kbRt.pivot = new Vector2(0f, 0.5f);
        kbRt.anchoredPosition = new Vector2(8f, 0f);
        kbRt.sizeDelta = new Vector2(210f, 30f);

        Image kbImg = keyBadge.AddComponent<Image>();
        kbImg.sprite = GetWhiteSprite();
        kbImg.color = new Color(0.28f, 0.16f, 0.10f, 0.95f);
        Outline kbOut = keyBadge.AddComponent<Outline>();
        kbOut.effectColor = new Color(0.90f, 0.72f, 0.28f, 0.90f);
        kbOut.effectDistance = new Vector2(1.2f, -1.2f);

        GameObject kTxtGo = CreateColonialText(keyBadge.transform, "KeyText", keyText, 15,
            new Color(1f, 0.92f, 0.48f), TextAnchor.MiddleCenter, font);
        RectTransform kTxtRt = kTxtGo.GetComponent<RectTransform>();
        kTxtRt.anchorMin = Vector2.zero;
        kTxtRt.anchorMax = Vector2.one;
        kTxtRt.offsetMin = Vector2.zero;
        kTxtRt.offsetMax = Vector2.zero;

        // Descripción de acción (derecha)
        GameObject actTxtGo = CreateColonialText(row.transform, "ActionText", actionText, 16,
            new Color(0.96f, 0.94f, 0.88f), TextAnchor.MiddleLeft, font);
        RectTransform actRt = actTxtGo.GetComponent<RectTransform>();
        actRt.anchorMin = actRt.anchorMax = new Vector2(0f, 0.5f);
        actRt.pivot = new Vector2(0f, 0.5f);
        actRt.anchoredPosition = new Vector2(232f, 0f);
        actRt.sizeDelta = new Vector2(368f, 34f);
    }

    public static GameObject CreateColonialText(Transform parent, string name, string content, int fontSize,
        Color color, TextAnchor align, Font font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = GetMenuFont(font);
        t.text = content;
        t.fontSize = fontSize;
        t.fontStyle = FontStyle.Bold;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;

        Shadow sh = go.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.92f);
        sh.effectDistance = new Vector2(1.5f, -1.5f);
        return go;
    }
}
