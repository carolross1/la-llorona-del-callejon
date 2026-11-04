#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Módulo de Configuración del HUD y Pantallas de UI para el Nivel 2.
/// Adapta la interfaz de usuario con fidelidad visual al estilo de referencia:
/// - Arriba izquierda: corazones discretos con medios corazones y "ESTAMINA [Shift]".
/// - Arriba centro: placa ornamental colonial con "TIEMPO 00:00" y "PUNTOS 0".
/// - Arriba derecha: panel "OBJETIVO: Sigue las flechas..." con "Salida: XXm" dinámico en metros.
/// - Abajo derecha: ranuras de objetos.
/// - Pantalla de Victoria (LevelCompletedUI): modal con Tiempo, Puntos, Récord y botón de Siguiente Nivel.
/// </summary>
public static class Level2HUD
{
    public static void SetupHUD()
    {
        HUD hud = UnityEngine.Object.FindAnyObjectByType<HUD>();
        if (hud == null) return;

        Sprite[] uiSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/UI/interfaz.png").OfType<Sprite>().ToArray();
        Sprite heartFull   = uiSprites.FirstOrDefault(s => s.name == "Corazon_lleno");
        Sprite heartEmpty  = uiSprites.FirstOrDefault(s => s.name == "Corazon_vacio");
        Sprite plateNormal = uiSprites.FirstOrDefault(s => s.name == "Placa_normal");
        Sprite plateHighlight = uiSprites.FirstOrDefault(s => s.name == "Placa_resaltada") ?? plateNormal;
        Sprite platePressed   = uiSprites.FirstOrDefault(s => s.name == "Placa_presionada") ?? plateNormal;
        Sprite barFrame    = uiSprites.FirstOrDefault(s => s.name == "Barra_marco");
        Font legacyFont    = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // Limpiar elementos huérfanos o duplicados directos bajo canvas para evitar HUD doble
        string[] orphanNames = { "HUD_Izquierda_Estado", "HUD_Centro_Tiempo", "HUD_Centro_TiempoPuntos", "HUD_Derecha_Objetivo", "HUD_Inferior_Corazones_Duplicados" };
        foreach (var oName in orphanNames)
        {
            Transform orphan = canvas.transform.Find(oName);
            if (orphan != null && orphan.gameObject != hud.gameObject)
            {
                UnityEngine.Object.DestroyImmediate(orphan.gameObject);
            }
        }

        // ========================================================
        // 1. ARRIBA IZQUIERDA: CORAZONES Y ESTAMINA [Shift]
        // ========================================================
        GameObject hudLeft = GetOrCreateUIRect(hud.transform, "HUD_Izquierda_Estado");
        hudLeft.SetActive(true);

        RectTransform hudLeftRect = hudLeft.GetComponent<RectTransform>();
        hudLeftRect.anchorMin = new Vector2(0f, 1f);
        hudLeftRect.anchorMax = new Vector2(0f, 1f);
        hudLeftRect.pivot     = new Vector2(0f, 1f);
        hudLeftRect.anchoredPosition = new Vector2(32f, -28f);
        hudLeftRect.sizeDelta = new Vector2(340f, 120f);

        // Contenedor de corazones discretos (con medios corazones)
        GameObject heartsRoot = GetOrCreateUIRect(hudLeft.transform, "Corazones_Vida_Contenedor");
        RectTransform heartsRootRect = heartsRoot.GetComponent<RectTransform>();
        heartsRootRect.anchorMin = new Vector2(0f, 1f);
        heartsRootRect.anchorMax = new Vector2(0f, 1f);
        heartsRootRect.pivot     = new Vector2(0f, 1f);
        heartsRootRect.anchoredPosition = new Vector2(0f, 0f);
        heartsRootRect.sizeDelta = new Vector2(280f, 38f);

        Image[] heartImgs = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject hObj = GetOrCreateUIRect(heartsRoot.transform, $"Corazon_{i}");
            RectTransform hRect = hObj.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 0.5f);
            hRect.anchorMax = new Vector2(0f, 0.5f);
            hRect.pivot     = new Vector2(0f, 0.5f);
            hRect.anchoredPosition = new Vector2(i * 38f, 0f);
            hRect.sizeDelta = new Vector2(34f, 34f);

            Image img = GetOrAddComponent<Image>(hObj);
            img.sprite = (i < 3) ? (heartFull ?? heartEmpty) : (heartEmpty ?? heartFull);
            img.color = (i < 3) ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.55f);
            heartImgs[i] = img;
        }

        // Fila de medio corazón / estamina
        GameObject halfHeartObj = GetOrCreateUIRect(heartsRoot.transform, "Corazon_Medio_Status");
        RectTransform halfRect = halfHeartObj.GetComponent<RectTransform>();
        halfRect.anchorMin = new Vector2(0f, 1f);
        halfRect.anchorMax = new Vector2(0f, 1f);
        halfRect.pivot     = new Vector2(0f, 1f);
        halfRect.anchoredPosition = new Vector2(0f, -38f);
        halfRect.sizeDelta = new Vector2(24f, 24f);
        Image halfImg = GetOrAddComponent<Image>(halfHeartObj);
        halfImg.sprite = heartFull ?? heartEmpty;
        halfImg.type = Image.Type.Filled;
        halfImg.fillMethod = Image.FillMethod.Horizontal;
        halfImg.fillAmount = 0.5f;

        // Etiqueta "ESTAMINA [Shift]"
        GameObject staminaTextObj = GetOrCreateUIRect(hudLeft.transform, "Texto_Estamina_Label");
        RectTransform stTxtRect = staminaTextObj.GetComponent<RectTransform>();
        stTxtRect.anchorMin = new Vector2(0f, 1f);
        stTxtRect.anchorMax = new Vector2(0f, 1f);
        stTxtRect.pivot     = new Vector2(0f, 1f);
        stTxtRect.anchoredPosition = new Vector2(30f, -42f);
        stTxtRect.sizeDelta = new Vector2(220f, 24f);

        Text stText = GetOrAddComponent<Text>(staminaTextObj);
        stText.text = "ESTAMINA [Shift]";
        stText.font = legacyFont;
        stText.fontSize = 14;
        stText.fontStyle = FontStyle.Bold;
        stText.color = new Color(1.0f, 0.92f, 0.55f, 1.0f);

        // Barra de estamina debajo de la etiqueta
        GameObject staminaBarObj = GetOrCreateUIRect(hudLeft.transform, "Estamina_Barra");
        RectTransform sbRect = staminaBarObj.GetComponent<RectTransform>();
        sbRect.anchorMin = new Vector2(0f, 1f);
        sbRect.anchorMax = new Vector2(0f, 1f);
        sbRect.pivot     = new Vector2(0f, 1f);
        sbRect.anchoredPosition = new Vector2(0f, -68f);
        sbRect.sizeDelta = new Vector2(220f, 14f);

        Image sbBg = GetOrAddComponent<Image>(staminaBarObj);
        sbBg.color = new Color(0.15f, 0.10f, 0.10f, 0.85f);

        GameObject sbFillObj = GetOrCreateUIRect(staminaBarObj.transform, "Relleno_Estamina");
        RectTransform sbFillRect = sbFillObj.GetComponent<RectTransform>();
        sbFillRect.anchorMin = Vector2.zero;
        sbFillRect.anchorMax = Vector2.one;
        sbFillRect.offsetMin = Vector2.zero;
        sbFillRect.offsetMax = Vector2.zero;

        Image sbFillImg = GetOrAddComponent<Image>(sbFillObj);
        sbFillImg.type = Image.Type.Filled;
        sbFillImg.fillMethod = Image.FillMethod.Horizontal;
        sbFillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        sbFillImg.fillAmount = 1.0f;
        sbFillImg.color = new Color(0.92f, 0.75f, 0.22f, 1.0f);

        // ========================================================
        // 2. ARRIBA CENTRO: PLACA COLONIAL "TIEMPO 00:00" Y "PUNTOS 0"
        // ========================================================
        GameObject centerTimerObj = GetOrCreateUIRect(hud.transform, "HUD_Centro_TiempoPuntos");
        centerTimerObj.SetActive(true);

        RectTransform centerRect = centerTimerObj.GetComponent<RectTransform>();
        centerRect.anchorMin = new Vector2(0.5f, 1f);
        centerRect.anchorMax = new Vector2(0.5f, 1f);
        centerRect.pivot     = new Vector2(0.5f, 1f);
        centerRect.anchoredPosition = new Vector2(0f, -22f);
        centerRect.sizeDelta = new Vector2(300f, 68f);

        Image centerPlate = GetOrAddComponent<Image>(centerTimerObj);
        if (plateNormal != null)
        {
            centerPlate.sprite = plateNormal;
            centerPlate.type = Image.Type.Sliced;
            centerPlate.color = new Color(0.14f, 0.10f, 0.12f, 0.92f);
        }

        GameObject timerTxtObj = GetOrCreateUIRect(centerTimerObj.transform, "Texto_Tiempo");
        RectTransform tRect = timerTxtObj.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0f, 0.5f);
        tRect.anchorMax = new Vector2(1f, 1f);
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;

        Text timerComp = GetOrAddComponent<Text>(timerTxtObj);
        timerComp.text = "TIEMPO 00:00";
        timerComp.font = legacyFont;
        timerComp.fontSize = 15;
        timerComp.fontStyle = FontStyle.Bold;
        timerComp.alignment = TextAnchor.MiddleCenter;
        timerComp.color = new Color(1.0f, 0.92f, 0.70f, 1f);

        GameObject scoreTxtObj = GetOrCreateUIRect(centerTimerObj.transform, "Texto_Puntos");
        RectTransform scRect = scoreTxtObj.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0f, 0f);
        scRect.anchorMax = new Vector2(1f, 0.5f);
        scRect.offsetMin = Vector2.zero;
        scRect.offsetMax = Vector2.zero;

        Text scoreComp = GetOrAddComponent<Text>(scoreTxtObj);
        scoreComp.text = "PUNTOS 0";
        scoreComp.font = legacyFont;
        scoreComp.fontSize = 14;
        scoreComp.fontStyle = FontStyle.Bold;
        scoreComp.alignment = TextAnchor.MiddleCenter;
        scoreComp.color = new Color(0.95f, 0.85f, 0.65f, 1f);

        // ========================================================
        // 3. ARRIBA DERECHA: PANEL DE OBJETIVO CON "Salida: XXm"
        // ========================================================
        GameObject objPanel = GetOrCreateUIRect(hud.transform, "HUD_Derecha_Objetivo");
        objPanel.SetActive(true);

        RectTransform objPanelRect = objPanel.GetComponent<RectTransform>();
        objPanelRect.anchorMin = new Vector2(1f, 1f);
        objPanelRect.anchorMax = new Vector2(1f, 1f);
        objPanelRect.pivot     = new Vector2(1f, 1f);
        objPanelRect.anchoredPosition = new Vector2(-28f, -22f);
        objPanelRect.sizeDelta = new Vector2(330f, 75f);

        Image objBg = GetOrAddComponent<Image>(objPanel);
        if (plateNormal != null)
        {
            objBg.sprite = plateNormal;
            objBg.type = Image.Type.Sliced;
            objBg.color = new Color(0.12f, 0.08f, 0.10f, 0.92f);
        }

        GameObject objTitleObj = GetOrCreateUIRect(objPanel.transform, "Texto_Objetivo_Desc");
        RectTransform otRect = objTitleObj.GetComponent<RectTransform>();
        otRect.anchorMin = new Vector2(0f, 0.35f);
        otRect.anchorMax = new Vector2(1f, 1f);
        otRect.offsetMin = new Vector2(12f, 0f);
        otRect.offsetMax = new Vector2(-12f, -6f);

        Text otComp = GetOrAddComponent<Text>(objTitleObj);
        otComp.text = "OBJETIVO: Sigue las flechas hacia la Salida...";
        otComp.font = legacyFont;
        otComp.fontSize = 12;
        otComp.fontStyle = FontStyle.Bold;
        otComp.alignment = TextAnchor.UpperLeft;
        otComp.color = new Color(1.0f, 0.92f, 0.70f, 1f);

        GameObject distTxtObj = GetOrCreateUIRect(objPanel.transform, "Texto_Distancia_Salida");
        RectTransform dtRect = distTxtObj.GetComponent<RectTransform>();
        dtRect.anchorMin = new Vector2(0f, 0f);
        dtRect.anchorMax = new Vector2(1f, 0.45f);
        dtRect.offsetMin = new Vector2(12f, 6f);
        dtRect.offsetMax = new Vector2(-12f, 0f);

        Text dtComp = GetOrAddComponent<Text>(distTxtObj);
        dtComp.text = "Salida: 60m";
        dtComp.font = legacyFont;
        dtComp.fontSize = 14;
        dtComp.fontStyle = FontStyle.Bold;
        dtComp.alignment = TextAnchor.MiddleLeft;
        dtComp.color = new Color(1.0f, 0.85f, 0.35f, 1f);

        GoalCompassUI compass = GetOrAddComponent<GoalCompassUI>(objPanel);
        SerializedObject soCompass = new SerializedObject(compass);
        var propDist = soCompass.FindProperty("distanceText");
        if (propDist != null) propDist.objectReferenceValue = dtComp;
        soCompass.ApplyModifiedProperties();

        // ========================================================
        // 4. MODAL DE VICTORIA (LevelCompletedUI) - Resuelve el bloqueo en la meta
        // ========================================================
        BuildVictoryModal(canvas.transform, plateNormal, plateHighlight, platePressed, legacyFont);

        // ========================================================
        // 5. ASIGNAR REFERENCIAS AL HUD
        // ========================================================
        SerializedObject soHUD = new SerializedObject(hud);
        var propTimer = soHUD.FindProperty("timerText");
        if (propTimer != null) propTimer.objectReferenceValue = timerComp;
        var propScore = soHUD.FindProperty("scoreText");
        if (propScore != null) propScore.objectReferenceValue = scoreComp;
        var propObj = soHUD.FindProperty("objectiveText");
        if (propObj != null) propObj.objectReferenceValue = otComp;
        var propStamina = soHUD.FindProperty("staminaFillImage");
        if (propStamina != null) propStamina.objectReferenceValue = sbFillImg;

        var propHearts = soHUD.FindProperty("heartImages");
        if (propHearts != null)
        {
            propHearts.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                propHearts.GetArrayElementAtIndex(i).objectReferenceValue = heartImgs[i];
            }
        }
        var propActiveH = soHUD.FindProperty("activeHeartSprite");
        if (propActiveH != null && heartFull != null) propActiveH.objectReferenceValue = heartFull;
        var propLostH = soHUD.FindProperty("lostHeartSprite");
        if (propLostH != null && heartEmpty != null) propLostH.objectReferenceValue = heartEmpty;

        soHUD.ApplyModifiedProperties();

        // 5. Configurar Modal de Victoria (LevelCompletedUI)
        BuildVictoryModal(canvas.transform, plateNormal, plateHighlight, platePressed, legacyFont);
    }

    static void BuildVictoryModal(Transform canvasTransform, Sprite plateNormal, Sprite plateHighlight, Sprite platePressed, Font font)
    {
        Transform existing = canvasTransform.Find("LevelCompletedUI");
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

        GameObject victoryGo = new GameObject("LevelCompletedUI");
        victoryGo.transform.SetParent(canvasTransform, false);
        RectTransform rtVic = victoryGo.AddComponent<RectTransform>();
        rtVic.anchorMin = Vector2.zero;
        rtVic.anchorMax = Vector2.one;
        rtVic.offsetMin = Vector2.zero;
        rtVic.offsetMax = Vector2.zero;
        LevelCompletedUI victory = victoryGo.AddComponent<LevelCompletedUI>();

        // Fondo oscuro 75%
        GameObject modalRoot = new GameObject("VictoryOverlay");
        modalRoot.transform.SetParent(victoryGo.transform, false);
        RectTransform rtOverlay = modalRoot.AddComponent<RectTransform>();
        rtOverlay.anchorMin = Vector2.zero;
        rtOverlay.anchorMax = Vector2.one;
        rtOverlay.offsetMin = Vector2.zero;
        rtOverlay.offsetMax = Vector2.zero;
        Image imgOverlay = modalRoot.AddComponent<Image>();
        imgOverlay.color = new Color(0f, 0f, 0f, 0.80f);

        // Panel decorativo de la placa
        GameObject panel = new GameObject("VictoryPanel");
        panel.transform.SetParent(modalRoot.transform, false);
        RectTransform rtPanel = panel.AddComponent<RectTransform>();
        rtPanel.anchorMin = rtPanel.anchorMax = new Vector2(0.5f, 0.5f);
        rtPanel.pivot = new Vector2(0.5f, 0.5f);
        rtPanel.sizeDelta = new Vector2(580f, 480f);

        Image imgPanel = panel.AddComponent<Image>();
        if (plateNormal != null)
        {
            imgPanel.sprite = plateNormal;
            imgPanel.type = Image.Type.Sliced;
        }
        imgPanel.color = Color.white;

        // Título "¡NIVEL COMPLETADO!"
        GameObject titleObj = new GameObject("Titulo_Victoria");
        titleObj.transform.SetParent(panel.transform, false);
        RectTransform rtTitle = titleObj.AddComponent<RectTransform>();
        rtTitle.anchorMin = rtTitle.anchorMax = new Vector2(0.5f, 1f);
        rtTitle.pivot = new Vector2(0.5f, 1f);
        rtTitle.anchoredPosition = new Vector2(0f, -28f);
        rtTitle.sizeDelta = new Vector2(520f, 60f);

        Text txtTitle = titleObj.AddComponent<Text>();
        txtTitle.text = "¡NIVEL COMPLETADO!";
        txtTitle.font = font;
        txtTitle.fontSize = 32;
        txtTitle.fontStyle = FontStyle.Bold;
        txtTitle.alignment = TextAnchor.MiddleCenter;
        txtTitle.color = new Color(1f, 0.88f, 0.40f, 1f);

        // Fondo oscuro para estadísticas
        GameObject statsBox = new GameObject("StatsBox");
        statsBox.transform.SetParent(panel.transform, false);
        RectTransform rtStats = statsBox.AddComponent<RectTransform>();
        rtStats.anchorMin = rtStats.anchorMax = new Vector2(0.5f, 0.5f);
        rtStats.pivot = new Vector2(0.5f, 0.5f);
        rtStats.anchoredPosition = new Vector2(0f, 35f);
        rtStats.sizeDelta = new Vector2(490f, 145f);

        Image imgStats = statsBox.AddComponent<Image>();
        imgStats.color = new Color(0.06f, 0.04f, 0.10f, 0.90f);

        // Textos de estadísticas
        Text txtTime = CreateStatText(statsBox.transform, "VicTiempo", "Tiempo: 00:00", new Vector2(0f, 40f), font, 24, new Color(1f, 0.97f, 0.90f));
        Text txtScore = CreateStatText(statsBox.transform, "VicPuntaje", "Puntos: 0", new Vector2(0f, 0f), font, 24, new Color(1f, 0.89f, 0.35f));
        Text txtHigh = CreateStatText(statsBox.transform, "VicRecord", "Récord: 0", new Vector2(0f, -40f), font, 22, new Color(0.92f, 0.92f, 0.92f));

        // Botón Siguiente Nivel
        Button btnNext = CreateButton(panel.transform, "Btn_Siguiente", "Siguiente Nivel [Espacio]", new Vector2(0f, -80f), new Vector2(380f, 60f), plateHighlight ?? plateNormal, font, 22, true);
        Button btnMenu = CreateButton(panel.transform, "Btn_Menu", "Menú Principal [Esc]", new Vector2(0f, -155f), new Vector2(380f, 58f), plateNormal, font, 20, false);

        modalRoot.SetActive(false);

        SerializedObject soVic = new SerializedObject(victory);
        soVic.FindProperty("panel").objectReferenceValue           = modalRoot;
        soVic.FindProperty("timeResultText").objectReferenceValue  = txtTime;
        soVic.FindProperty("scoreResultText").objectReferenceValue = txtScore;
        soVic.FindProperty("highScoreText").objectReferenceValue   = txtHigh;
        soVic.FindProperty("nextLevelButton").objectReferenceValue = btnNext;
        soVic.FindProperty("menuButton").objectReferenceValue      = btnMenu;
        soVic.ApplyModifiedProperties();
    }

    static Text CreateStatText(Transform parent, string name, string text, Vector2 pos, Font font, int fontSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(440f, 36f);

        Text t = go.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        return t;
    }

    static Button CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, Sprite sprite, Font font, int fontSize, bool isPrimary)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }

        Button btn = go.AddComponent<Button>();
        btn.transition = Selectable.Transition.ColorTint;

        GameObject txtGo = new GameObject("Text");
        txtGo.transform.SetParent(go.transform, false);
        RectTransform rtTxt = txtGo.AddComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = Vector2.zero;
        rtTxt.offsetMax = Vector2.zero;

        Text t = txtGo.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = isPrimary ? Color.white : new Color(1f, 0.94f, 0.75f, 1f);

        return btn;
    }

    static GameObject GetOrCreateUIRect(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t != null) return t.gameObject;
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        T comp = obj.GetComponent<T>();
        if (comp == null) comp = obj.AddComponent<T>();
        return comp;
    }
}
#endif
