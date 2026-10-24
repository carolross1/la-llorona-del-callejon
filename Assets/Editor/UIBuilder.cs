// Assets/Editor/UIBuilder.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Constructor automatizado de escenas y Canvas de interfaz.
/// Usa los sprites reales del proyecto (interfaz_0..31, logo_0),
/// tarjetas semitransparentes elegantes (panel_hud_card.png)
/// y brújula de orientación hacia la meta (flecha_meta.png).
/// </summary>
public static class UIBuilder
{
    // ── Rutas de sprites ────────────────────────────────────────────────────
    const string InterfazPath = "Assets/Sprites/UI/interfaz.png";
    const string LogoPath     = "Assets/Sprites/UI/logo.png";
    const string CardPath     = "Assets/Sprites/UI/panel_hud_card.png";
    const string ArrowPath    = "Assets/Sprites/UI/flecha_meta.png";

    // ── Sprites cargados por sesión de build ─────────────────────────────────
    static Sprite[] _interfazSprites;
    static Sprite   _logoSprite;
    static Sprite   _cardSprite;
    static Sprite   _arrowSprite;

    static Sprite[] InterfazSprites =>
        _interfazSprites ??= AssetDatabase.LoadAllAssetsAtPath(InterfazPath)
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();

    static Sprite LogoSprite =>
        _logoSprite ??= AssetDatabase.LoadAssetAtPath<Sprite>(LogoPath);

    static Sprite CardSprite =>
        _cardSprite ??= AssetDatabase.LoadAssetAtPath<Sprite>(CardPath);

    static Sprite ArrowSprite =>
        _arrowSprite ??= AssetDatabase.LoadAssetAtPath<Sprite>(ArrowPath);

    static Sprite Iface(int index) =>
        (InterfazSprites != null && index < InterfazSprites.Length) ? InterfazSprites[index] : null;

    // ── Punto de entrada principal ──────────────────────────────────────────

    [MenuItem("La Llorona/Generar Escenas y UI")]
    public static void BuildAllScenesAndUI()
    {
        Debug.Log("[UIBuilder] Iniciando generación completa de UI y escenas...");
        Directory.CreateDirectory("Assets/Scenes");

        // Asegurar que existan todos los sprites procedimentales necesarios
        SpriteAssetGenerator.GenerateAllSprites();
        _cardSprite = null;
        _arrowSprite = null;
        _interfazSprites = null;
        _logoSprite = null;

        BuildSplashScene();
        BuildMainMenuScene();
        BuildLevelSelectScene();
        BuildCreditsScene();

        // Canvas HUD en todos los niveles jugables
        BuildLevel1UICanvas();
        BuildLevel2UICanvas();
        BuildLevel3UICanvas();
        BuildLevel4UICanvas();

        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("[UIBuilder] ¡Todas las escenas y sistemas de UI fueron generados con éxito!");
    }

    // ── Escena Splash ────────────────────────────────────────────────────────

    public static void BuildSplashScene()
    {
        string path = "Assets/Scenes/Splash.unity";
        Scene scene = File.Exists(path)
            ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        CreateCamera();
        EnsureEventSystem();

        if (Camera.main != null) Camera.main.backgroundColor = Color.black;

        GameObject canvasGo = CreateCanvas("SplashCanvas");

        // Fondo completo con el logo del juego
        CreateImageSprite(canvasGo.transform, "LogoImagen", LogoSprite,
            Vector2.zero, Vector2.one);

        // Texto "Presiona cualquier tecla"
        CreateText(canvasGo.transform, "ContinueText",
            "Presiona cualquier tecla para continuar",
            24, TextAnchor.MiddleCenter, new Color(0.95f, 0.95f, 0.95f, 0.95f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(650, 44))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 50f);

        canvasGo.AddComponent<SplashScreen>();

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        Debug.Log("[UIBuilder] Escena Splash generada.");
    }

    // ── Escena MainMenu ──────────────────────────────────────────────────────

    public static void BuildMainMenuScene()
    {
        string path = "Assets/Scenes/MainMenu.unity";
        Scene scene = File.Exists(path)
            ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        CreateCamera();
        EnsureEventSystem();
        if (Camera.main != null) Camera.main.backgroundColor = new Color(0.04f, 0.02f, 0.08f);

        GameObject canvasGo = CreateCanvas("MainMenuCanvas");

        // Fondo — logo a pantalla completa, oscurecido
        GameObject bgLogo = CreateImageSprite(canvasGo.transform, "Fondo_Logo", LogoSprite,
            Vector2.zero, Vector2.one);
        bgLogo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.35f);

        // Panel central con tarjeta redondeada oscura
        GameObject panelGo = CreateImageSprite(canvasGo.transform, "MenuPanel",
            CardSprite ?? Iface(4),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(580, 560));
        panelGo.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.10f, 0.92f);
        if (CardSprite != null) panelGo.GetComponent<Image>().type = Image.Type.Sliced;

        // Título del juego encima del panel
        CreateText(panelGo.transform, "Titulo", "La Llorona del Callejón",
            36, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.35f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(540, 60))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -30f);

        // Botones del menú principal
        float startY = 160f, step = 68f;
        Button btnJugar    = CreateSpriteButton(panelGo.transform, "Btn_Jugar",    "JUGAR",             new Vector2(0f, startY),          new Vector2(360, 54));
        Button btnContinue = CreateSpriteButton(panelGo.transform, "Btn_Continuar","CONTINUAR",         new Vector2(0f, startY - step),    new Vector2(360, 54));
        Button btnSelect   = CreateSpriteButton(panelGo.transform, "Btn_Niveles",  "SELECCIONAR NIVEL", new Vector2(0f, startY - step*2), new Vector2(360, 54));
        Button btnOpciones = CreateSpriteButton(panelGo.transform, "Btn_Opciones", "OPCIONES",          new Vector2(0f, startY - step*3), new Vector2(360, 54));
        Button btnCredits  = CreateSpriteButton(panelGo.transform, "Btn_Creditos", "CRÉDITOS",          new Vector2(0f, startY - step*4), new Vector2(360, 54));
        Button btnSalir    = CreateSpriteButton(panelGo.transform, "Btn_Salir",    "SALIR",             new Vector2(0f, startY - step*5), new Vector2(360, 54));

        MainMenuUI menu = canvasGo.AddComponent<MainMenuUI>();
        SerializedObject so = new SerializedObject(menu);
        void Assign(string propName, Object val)
        {
            var p = so.FindProperty(propName);
            if (p != null) p.objectReferenceValue = val;
        }
        Assign("playButton",        btnJugar);
        Assign("continueButton",    btnContinue);
        Assign("levelSelectButton", btnSelect);
        Assign("optionsButton",     btnOpciones);
        Assign("creditsButton",     btnCredits);
        Assign("exitButton",        btnSalir);
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        Debug.Log("[UIBuilder] Escena MainMenu generada.");
    }

    // ── Escena LevelSelect ───────────────────────────────────────────────────

    public static void BuildLevelSelectScene()
    {
        string path = "Assets/Scenes/LevelSelect.unity";
        Scene scene = File.Exists(path)
            ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        CreateCamera();
        EnsureEventSystem();
        if (Camera.main != null) Camera.main.backgroundColor = new Color(0.04f, 0.02f, 0.08f);

        GameObject canvasGo = CreateCanvas("LevelSelectCanvas");

        GameObject bg = CreateImageSprite(canvasGo.transform, "Fondo", LogoSprite, Vector2.zero, Vector2.one);
        bg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

        CreateText(canvasGo.transform, "Titulo", "SELECCIONAR NIVEL",
            38, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.35f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(700, 70))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -50f);

        string[] nombres = { "Nivel 1\nEl Mercado", "Nivel 2\nEl Callejón", "Nivel 3\nEl Atrio", "Nivel 4\nEl Panteón" };
        Vector2[] posiciones = {
            new Vector2(-220f,  90f), new Vector2( 220f,  90f),
            new Vector2(-220f, -110f), new Vector2( 220f, -110f)
        };

        Button[] levelBtns = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            levelBtns[i] = CreateSpriteButton(canvasGo.transform, $"Btn_Nivel{i+1}",
                nombres[i], posiciones[i], new Vector2(300, 140));
        }

        Button btnBack = CreateSpriteButton(canvasGo.transform, "Btn_Volver",
            "← Volver", new Vector2(0f, -270f), new Vector2(260, 54));

        string[] sceneNames = { "Level1_Mercado", "Level2_Callejon", "Level3_Atrio", "Level4_Cementerio" };

        LevelSelectUI lvlSel = canvasGo.AddComponent<LevelSelectUI>();
        SerializedObject so = new SerializedObject(lvlSel);

        SerializedProperty entries = so.FindProperty("levelEntries");
        if (entries != null)
        {
            entries.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("levelNumber").intValue        = i + 1;
                entry.FindPropertyRelative("sceneName").stringValue       = sceneNames[i];
                entry.FindPropertyRelative("button").objectReferenceValue = levelBtns[i];
            }
        }

        SerializedProperty backProp = so.FindProperty("backButton");
        if (backProp != null) backProp.objectReferenceValue = btnBack;
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        Debug.Log("[UIBuilder] Escena LevelSelect generada.");
    }

    // ── Escena Credits ───────────────────────────────────────────────────────

    public static void BuildCreditsScene()
    {
        string path = "Assets/Scenes/Credits.unity";
        Scene scene = File.Exists(path)
            ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        CreateCamera();
        EnsureEventSystem();
        if (Camera.main != null) Camera.main.backgroundColor = new Color(0.04f, 0.02f, 0.08f);

        GameObject canvasGo = CreateCanvas("CreditsCanvas");

        GameObject bg = CreateImageSprite(canvasGo.transform, "Fondo", LogoSprite, Vector2.zero, Vector2.one);
        bg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

        CreateText(canvasGo.transform, "Titulo", "CRÉDITOS",
            38, TextAnchor.UpperCenter, new Color(1f, 0.88f, 0.35f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(700, 60))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -60f);

        string credText =
            "Desarrollo: Carol & Princes\n" +
            "Motor: Unity 6.6 (Universal 2D)\n" +
            "Proyecto Escolar UTNG — 2026\n\n" +
            "Arte generado con IA generativa de imágenes\n" +
            "(Adobe Firefly / DALL·E)\n\n" +
            "Audio y Efectos: Freesound CC0\n\n" +
            "Agradecimientos: Docente de Desarrollo de Videojuegos UTNG";

        CreateText(canvasGo.transform, "TextoCreditos", credText,
            20, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(700, 500));

        CreateSpriteButton(canvasGo.transform, "Btn_Volver",
            "← Volver al Menú", new Vector2(0f, -250f), new Vector2(320, 56));

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        Debug.Log("[UIBuilder] Escena Credits generada.");
    }

    // ── Canvas de Niveles (HUD + modales) ───────────────────────────────────

    public static void BuildLevel1UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level1_Mercado.unity",
        "Objetivo: Cruza el mercado sin que La Llorona te vea");
    public static void BuildLevel2UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level2_Callejon.unity",
        "Objetivo: Llega al final del callejón esquivando los obstáculos");
    public static void BuildLevel3UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level3_Atrio.unity",
        "Objetivo: Cruza el atrio — usa velas para distraer a La Llorona");
    public static void BuildLevel4UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level4_Cementerio.unity",
        "Objetivo: ¡Escapa hacia tu casa! Cuidado con las ánimas y el perro");

    public static void BuildLevelUICanvas(string path, string objetivoTexto = "")
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("[UIBuilder] No se encontró " + path + ". Constrúyelo primero.");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        EnsureEventSystem();

        // Borrar canvas anterior si existe
        GameObject oldCanvas = GameObject.Find("UICanvas");
        if (oldCanvas != null) Undo.DestroyObjectImmediate(oldCanvas);

        GameObject canvasGo = CreateCanvas("UICanvas");
        Undo.RegisterCreatedObjectUndo(canvasGo, "UICanvas");

        // ════════════════════════════════════════════════════════
        // HUD REESTILIZADO: 3 TARJETAS DESPEJADAS Y LEGIBLES
        // ════════════════════════════════════════════════════════
        GameObject hudGo = new GameObject("HUD");
        hudGo.transform.SetParent(canvasGo.transform, false);
        HUD hud = hudGo.AddComponent<HUD>();

        Sprite cardSprite  = CardSprite;
        Sprite arrowSprite = ArrowSprite;

        // ── 1. TARJETA SUPERIOR IZQUIERDA: ESTADO (VIDAS Y VELAS) ───────────
        GameObject leftCard = CreateImageSprite(hudGo.transform, "Card_Jugador", cardSprite,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(250f, 100f));
        RectTransform rtLeft = leftCard.GetComponent<RectTransform>();
        rtLeft.pivot = new Vector2(0f, 1f);
        rtLeft.anchoredPosition = new Vector2(24f, -20f);
        if (cardSprite != null) leftCard.GetComponent<Image>().type = Image.Type.Sliced;
        leftCard.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.12f, 0.94f);

        // Corazones
        GameObject heartsRoot = new GameObject("Vidas_Corazones");
        heartsRoot.transform.SetParent(leftCard.transform, false);
        RectTransform rtH = heartsRoot.AddComponent<RectTransform>();
        rtH.anchorMin = rtH.anchorMax = new Vector2(0.5f, 1f);
        rtH.pivot = new Vector2(0.5f, 1f);
        rtH.anchoredPosition = new Vector2(0f, -8f);
        rtH.sizeDelta = new Vector2(170f, 46f);

        Image[] heartImgs = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject h = new GameObject($"Corazon_{i + 1}");
            h.transform.SetParent(heartsRoot.transform, false);
            Image hImg = h.AddComponent<Image>();
            hImg.sprite = Iface(0);
            hImg.preserveAspect = true;
            RectTransform hRt = h.GetComponent<RectTransform>();
            hRt.anchorMin = hRt.anchorMax = new Vector2(0f, 0.5f);
            hRt.pivot = new Vector2(0f, 0.5f);
            hRt.sizeDelta = new Vector2(44f, 44f);
            hRt.anchoredPosition = new Vector2(i * 54f, 0f);
            heartImgs[i] = hImg;
        }

        // Inventario de velas
        GameObject candleRoot = new GameObject("Velas_Inventario");
        candleRoot.transform.SetParent(leftCard.transform, false);
        RectTransform rtC = candleRoot.AddComponent<RectTransform>();
        rtC.anchorMin = rtC.anchorMax = new Vector2(0.5f, 0f);
        rtC.pivot = new Vector2(0.5f, 0f);
        rtC.anchoredPosition = new Vector2(0f, 8f);
        rtC.sizeDelta = new Vector2(210f, 32f);

        GameObject velaIconGo = new GameObject("VelaIcon");
        velaIconGo.transform.SetParent(candleRoot.transform, false);
        Image velaIcon = velaIconGo.AddComponent<Image>();
        velaIcon.sprite = Iface(2);
        velaIcon.preserveAspect = true;
        RectTransform velaRt = velaIconGo.GetComponent<RectTransform>();
        velaRt.anchorMin = velaRt.anchorMax = new Vector2(0f, 0.5f);
        velaRt.pivot = new Vector2(0f, 0.5f);
        velaRt.sizeDelta = new Vector2(28f, 28f);
        velaRt.anchoredPosition = new Vector2(0f, 0f);

        Text candleTxt = CreateText(candleRoot.transform, "CandleCountText",
            "×0  [F] Colocar", 17, TextAnchor.MiddleLeft,
            new Color(1f, 0.92f, 0.50f),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(170f, 32f))
            .GetComponent<Text>();
        candleTxt.fontStyle = FontStyle.Bold;
        candleTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(34f, 0f);
        candleRoot.SetActive(false);

        // ── 2. TARJETA SUPERIOR CENTRAL: CRONÓMETRO Y PUNTAJE ───────────────
        GameObject centerCard = CreateImageSprite(hudGo.transform, "Card_Tiempo", cardSprite,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(260f, 100f));
        RectTransform rtCenter = centerCard.GetComponent<RectTransform>();
        rtCenter.pivot = new Vector2(0.5f, 1f);
        rtCenter.anchoredPosition = new Vector2(0f, -20f);
        if (cardSprite != null) centerCard.GetComponent<Image>().type = Image.Type.Sliced;
        centerCard.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.12f, 0.94f);

        Text timerTxt = CreateText(centerCard.transform, "TimerText", "⏱ 00:00", 30,
            TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.86f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(240f, 46f))
            .GetComponent<Text>();
        timerTxt.fontStyle = FontStyle.Bold;
        var timerRt = timerTxt.GetComponent<RectTransform>();
        timerRt.pivot = new Vector2(0.5f, 1f);
        timerRt.anchoredPosition = new Vector2(0f, -8f);

        Text scoreTxt = CreateText(centerCard.transform, "ScoreText", "★ 0 pts", 20,
            TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.35f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(240f, 36f))
            .GetComponent<Text>();
        scoreTxt.fontStyle = FontStyle.Bold;
        var scoreRt = scoreTxt.GetComponent<RectTransform>();
        scoreRt.pivot = new Vector2(0.5f, 0f);
        scoreRt.anchoredPosition = new Vector2(0f, 8f);

        // ── 3. TARJETA SUPERIOR DERECHA: OBJETIVO Y BRÚJULA DE SALIDA ───────
        GameObject rightCard = CreateImageSprite(hudGo.transform, "Card_Objetivo", cardSprite,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(420f, 100f));
        RectTransform rtRight = rightCard.GetComponent<RectTransform>();
        rtRight.pivot = new Vector2(1f, 1f);
        rtRight.anchoredPosition = new Vector2(-24f, -20f);
        if (cardSprite != null) rightCard.GetComponent<Image>().type = Image.Type.Sliced;
        rightCard.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.12f, 0.94f);

        Text objTxt = CreateText(rightCard.transform, "ObjectiveText",
            string.IsNullOrEmpty(objetivoTexto) ? "Objetivo: Cruza el área hacia la salida" : objetivoTexto,
            15, TextAnchor.MiddleLeft, new Color(0.85f, 0.94f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(380f, 38f))
            .GetComponent<Text>();
        objTxt.fontStyle = FontStyle.Bold;
        var objRt = objTxt.GetComponent<RectTransform>();
        objRt.pivot = new Vector2(0.5f, 1f);
        objRt.anchoredPosition = new Vector2(0f, -8f);

        // Brújula / Indicador de dirección hacia la meta
        GameObject compassGo = new GameObject("BrujulaSalida");
        compassGo.transform.SetParent(rightCard.transform, false);
        RectTransform compRt = compassGo.AddComponent<RectTransform>();
        compRt.anchorMin = compRt.anchorMax = new Vector2(0.5f, 0f);
        compRt.pivot = new Vector2(0.5f, 0f);
        compRt.anchoredPosition = new Vector2(0f, 8f);
        compRt.sizeDelta = new Vector2(380f, 38f);

        GameObject arrowGo = new GameObject("FlechaIndicadora");
        arrowGo.transform.SetParent(compassGo.transform, false);
        Image arrowImg = arrowGo.AddComponent<Image>();
        arrowImg.sprite = arrowSprite;
        arrowImg.preserveAspect = true;
        RectTransform arrowRt = arrowGo.GetComponent<RectTransform>();
        arrowRt.anchorMin = arrowRt.anchorMax = new Vector2(0f, 0.5f);
        arrowRt.pivot = new Vector2(0.5f, 0.5f);
        arrowRt.sizeDelta = new Vector2(28f, 28f);
        arrowRt.anchoredPosition = new Vector2(18f, 0f);

        Text distTxt = CreateText(compassGo.transform, "DistanciaText", "Salida: --m", 18,
            TextAnchor.MiddleLeft, new Color(1f, 0.90f, 0.40f),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(310f, 32f))
            .GetComponent<Text>();
        distTxt.fontStyle = FontStyle.Bold;
        distTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(40f, 0f);

        GoalCompassUI compass = compassGo.AddComponent<GoalCompassUI>();
        SerializedObject soComp = new SerializedObject(compass);
        soComp.FindProperty("arrowRect").objectReferenceValue = arrowRt;
        soComp.FindProperty("distanceText").objectReferenceValue = distTxt;
        soComp.ApplyModifiedProperties();

        // ── 4. BANNER TUTORIAL FLOTANTE (INFERIOR) ───────────────────────────
        GameObject tutPanel = new GameObject("TutorialPanel");
        tutPanel.transform.SetParent(hudGo.transform, false);
        Image tutImg = tutPanel.AddComponent<Image>();
        tutImg.sprite = cardSprite;
        if (cardSprite != null) tutImg.type = Image.Type.Sliced;
        tutImg.color = new Color(0.05f, 0.04f, 0.12f, 0.95f);
        RectTransform tutRt = tutPanel.GetComponent<RectTransform>();
        tutRt.anchorMin = tutRt.anchorMax = new Vector2(0.5f, 0f);
        tutRt.pivot = new Vector2(0.5f, 0f);
        tutRt.sizeDelta = new Vector2(940f, 76f);
        tutRt.anchoredPosition = new Vector2(0f, 24f);

        GameObject eyeGo = new GameObject("IconoOjo");
        eyeGo.transform.SetParent(tutPanel.transform, false);
        Image eyeImg = eyeGo.AddComponent<Image>();
        eyeImg.sprite = Iface(3);
        eyeImg.preserveAspect = true;
        RectTransform eyeRt = eyeGo.GetComponent<RectTransform>();
        eyeRt.anchorMin = eyeRt.anchorMax = new Vector2(0f, 0.5f);
        eyeRt.pivot = new Vector2(0f, 0.5f);
        eyeRt.sizeDelta = new Vector2(50f, 50f);
        eyeRt.anchoredPosition = new Vector2(16f, 0f);

        CanvasGroup tutCg = tutPanel.AddComponent<CanvasGroup>();
        Text tutTxt = CreateText(tutPanel.transform, "TutorialText",
            "WASD para moverte · Shift para correr · E para esconderte",
            19, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(820f, 68f))
            .GetComponent<Text>();
        tutTxt.fontStyle = FontStyle.Bold;
        tutTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(25f, 0f);
        tutPanel.SetActive(false);

        // Enlazar HUD
        SerializedObject soHud = new SerializedObject(hud);
        SerializedProperty hpArray = soHud.FindProperty("heartImages");
        hpArray.arraySize = 3;
        for (int i = 0; i < 3; i++) hpArray.GetArrayElementAtIndex(i).objectReferenceValue = heartImgs[i];
        soHud.FindProperty("activeHeartSprite").objectReferenceValue   = Iface(0);
        soHud.FindProperty("lostHeartSprite").objectReferenceValue     = Iface(1);
        soHud.FindProperty("timerText").objectReferenceValue           = timerTxt;
        soHud.FindProperty("scoreText").objectReferenceValue           = scoreTxt;
        soHud.FindProperty("objectiveText").objectReferenceValue       = objTxt;
        soHud.FindProperty("candleInventoryRoot").objectReferenceValue = candleRoot;
        soHud.FindProperty("candleCountText").objectReferenceValue     = candleTxt;
        soHud.FindProperty("tutorialPanel").objectReferenceValue       = tutPanel;
        soHud.FindProperty("tutorialText").objectReferenceValue        = tutTxt;
        soHud.FindProperty("tutorialCanvasGroup").objectReferenceValue = tutCg;
        soHud.ApplyModifiedProperties();

        // ════════════════════════════════════════════════════════
        // PauseMenu
        // ════════════════════════════════════════════════════════
        GameObject pauseGo = new GameObject("PauseMenu");
        pauseGo.transform.SetParent(canvasGo.transform, false);
        PauseMenu pause = pauseGo.AddComponent<PauseMenu>();

        GameObject pausePanel = CreateImageSprite(pauseGo.transform, "PausePanel", cardSprite ?? Iface(4),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 480f));
        pausePanel.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.14f, 0.97f);
        if (cardSprite != null) pausePanel.GetComponent<Image>().type = Image.Type.Sliced;

        CreateText(pausePanel.transform, "PausaTitulo", "PAUSA",
            44, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.7f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(420f, 68f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -20f);

        Button pResume  = CreateSpriteButton(pausePanel.transform, "Btn_Reanudar",  "Reanudar",        new Vector2(0f,  110f), new Vector2(340f, 56f));
        Button pRestart = CreateSpriteButton(pausePanel.transform, "Btn_Reiniciar", "Reiniciar Nivel", new Vector2(0f,  44f),  new Vector2(340f, 56f));
        Button pOptions = CreateSpriteButton(pausePanel.transform, "Btn_Opciones",  "Opciones",        new Vector2(0f, -22f),  new Vector2(340f, 56f));
        Button pMenu    = CreateSpriteButton(pausePanel.transform, "Btn_Menu",      "Menú Principal",  new Vector2(0f, -88f),  new Vector2(340f, 56f));
        pausePanel.SetActive(false);

        SerializedObject soPause = new SerializedObject(pause);
        soPause.FindProperty("pausePanel").objectReferenceValue    = pausePanel;
        soPause.FindProperty("resumeButton").objectReferenceValue  = pResume;
        soPause.FindProperty("restartButton").objectReferenceValue = pRestart;
        soPause.FindProperty("optionsButton").objectReferenceValue = pOptions;
        soPause.FindProperty("menuButton").objectReferenceValue    = pMenu;
        soPause.ApplyModifiedProperties();

        // ════════════════════════════════════════════════════════
        // LevelCompletedUI
        // ════════════════════════════════════════════════════════
        GameObject victoryGo = new GameObject("LevelCompletedUI");
        victoryGo.transform.SetParent(canvasGo.transform, false);
        LevelCompletedUI victory = victoryGo.AddComponent<LevelCompletedUI>();

        GameObject vicPanel = CreateImageSprite(victoryGo.transform, "VictoryPanel", cardSprite ?? Iface(4),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 460f));
        vicPanel.GetComponent<Image>().color = new Color(0.08f, 0.16f, 0.10f, 0.97f);
        if (cardSprite != null) vicPanel.GetComponent<Image>().type = Image.Type.Sliced;

        CreateText(vicPanel.transform, "VicTitulo", "¡NIVEL COMPLETADO!",
            36, TextAnchor.MiddleCenter, new Color(0.4f, 1f, 0.5f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(500f, 60f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -20f);

        Text vicTime = CreateText(vicPanel.transform, "VicTiempo", "Tiempo: 00:00", 22,
            TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(360f, 36f))
            .GetComponent<Text>();
        vicTime.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 90f);

        Text vicScore = CreateText(vicPanel.transform, "VicPuntaje", "Puntos: 0", 22,
            TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.4f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(360f, 36f))
            .GetComponent<Text>();
        vicScore.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 50f);

        Text vicHigh = CreateText(vicPanel.transform, "VicRecord", "Récord: 0", 20,
            TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.8f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(360f, 34f))
            .GetComponent<Text>();
        vicHigh.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 10f);

        Button vicNext = CreateSpriteButton(vicPanel.transform, "Btn_Siguiente", "Siguiente Nivel", new Vector2(0f, -55f),  new Vector2(340f, 54f));
        Button vicMenu = CreateSpriteButton(vicPanel.transform, "Btn_Menu",      "Menú Principal",  new Vector2(0f, -120f), new Vector2(340f, 54f));
        vicPanel.SetActive(false);

        SerializedObject soVic = new SerializedObject(victory);
        soVic.FindProperty("panel").objectReferenceValue           = vicPanel;
        soVic.FindProperty("timeResultText").objectReferenceValue  = vicTime;
        soVic.FindProperty("scoreResultText").objectReferenceValue = vicScore;
        soVic.FindProperty("highScoreText").objectReferenceValue   = vicHigh;
        soVic.FindProperty("nextLevelButton").objectReferenceValue = vicNext;
        soVic.FindProperty("menuButton").objectReferenceValue      = vicMenu;
        soVic.ApplyModifiedProperties();

        // ════════════════════════════════════════════════════════
        // GameOverUI
        // ════════════════════════════════════════════════════════
        GameObject goOver = new GameObject("GameOverUI");
        goOver.transform.SetParent(canvasGo.transform, false);
        GameOverUI gameOver = goOver.AddComponent<GameOverUI>();

        GameObject goPanel = CreateImageSprite(goOver.transform, "GameOverPanel", cardSprite ?? Iface(4),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 440f));
        goPanel.GetComponent<Image>().color = new Color(0.18f, 0.05f, 0.05f, 0.97f);
        if (cardSprite != null) goPanel.GetComponent<Image>().type = Image.Type.Sliced;

        CreateText(goPanel.transform, "GoTitulo", "¡HAS SIDO ATRAPADO!",
            38, TextAnchor.MiddleCenter, new Color(1f, 0.25f, 0.25f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 66f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -20f);

        CreateText(goPanel.transform, "GoSub",
            "El lamento de La Llorona te envolvió en la oscuridad...",
            19, TextAnchor.MiddleCenter, new Color(0.85f, 0.75f, 0.75f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(480f, 60f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 40f);

        Button goRetry = CreateSpriteButton(goPanel.transform, "Btn_Reintentar", "Reintentar",      new Vector2(0f, -40f),  new Vector2(340f, 54f));
        Button goMenu  = CreateSpriteButton(goPanel.transform, "Btn_Menu",       "Menú Principal",  new Vector2(0f, -105f), new Vector2(340f, 54f));
        goPanel.SetActive(false);

        SerializedObject soGo = new SerializedObject(gameOver);
        soGo.FindProperty("panel").objectReferenceValue       = goPanel;
        soGo.FindProperty("retryButton").objectReferenceValue = goRetry;
        soGo.FindProperty("menuButton").objectReferenceValue  = goMenu;
        soGo.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[UIBuilder] Canvas HUD y modales actualizados con éxito en: " + path);
    }

    // ── Build Settings ───────────────────────────────────────────────────────

    static void UpdateBuildSettings()
    {
        string[] scenes = {
            "Assets/Scenes/Splash.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/LevelSelect.unity",
            "Assets/Scenes/Level1_Mercado.unity",
            "Assets/Scenes/Level2_Callejon.unity",
            "Assets/Scenes/Level3_Atrio.unity",
            "Assets/Scenes/Level4_Cementerio.unity",
            "Assets/Scenes/Credits.unity"
        };

        EditorBuildSettingsScene[] buildScenes = scenes
            .Where(File.Exists)
            .Select(s => new EditorBuildSettingsScene(s, true))
            .ToArray();

        EditorBuildSettings.scenes = buildScenes;
        Debug.Log($"[UIBuilder] {buildScenes.Length} escenas registradas en Build Settings.");
    }

    // ── Helpers de Creación UI ───────────────────────────────────────────────

    static GameObject CreateCanvas(string name)
    {
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode   = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 10;

        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    static void CreateCamera()
    {
        if (Camera.main == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();
        }
    }

    static GameObject CreateImageSprite(Transform parent, string name, Sprite sprite,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta = default)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = false;
        }
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = Vector2.zero;
        if (sizeDelta != default) rt.sizeDelta = sizeDelta;
        return go;
    }

    static GameObject CreateImage(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta = default)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = Vector2.zero;
        if (sizeDelta != default) rt.sizeDelta = sizeDelta;
        return go;
    }

    static GameObject CreateText(Transform parent, string name, string text, int fontSize,
        TextAnchor alignment, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.text      = text;
        t.fontSize  = fontSize;
        t.alignment = alignment;
        t.color     = color;
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.horizontalOverflow = HorizontalWrapMode.Wrap;

        // Borde y sombra para legibilidad 100% impecable sobre cualquier fondo
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(2f, -2f);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = sizeDelta;
        return go;
    }

    static Button CreateSpriteButton(Transform parent, string name, string text,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.sprite = Iface(5) ?? CardSprite;
        img.type   = Image.Type.Sliced;

        Button btn = go.AddComponent<Button>();
        SpriteState ss = new SpriteState
        {
            highlightedSprite = Iface(6) ?? CardSprite,
            pressedSprite     = Iface(7) ?? CardSprite,
            disabledSprite    = Iface(7) ?? CardSprite
        };
        btn.spriteState = ss;
        btn.transition  = Selectable.Transition.SpriteSwap;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        CreateText(go.transform, "Text", text, 22,
            TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.80f),
            Vector2.zero, Vector2.one, Vector2.zero);

        return btn;
    }
}
