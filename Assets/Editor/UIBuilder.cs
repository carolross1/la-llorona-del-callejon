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
/// Usa los sprites reales del proyecto (interfaz_0..31, logo_0) para el HUD,
/// botones, corazones, velas y pantallas de menú.
///
/// Sprites de interfaz.png:
///   interfaz_0 = corazón rojo (vida llena)
///   interfaz_1 = corazón negro (vida vacía)
///   interfaz_2 = vela (inventario)
///   interfaz_3 = ojo (icono detección)
///   interfaz_4 = barra madera (fondo HUD/panel)
///   interfaz_5 = botón normal
///   interfaz_6 = botón resaltado (hover)
///   interfaz_7 = botón presionado
/// </summary>
public static class UIBuilder
{
    // ── Rutas de sprites ────────────────────────────────────────────────────
    const string InterfazPath = "Assets/Sprites/UI/interfaz.png";
    const string LogoPath     = "Assets/Sprites/UI/logo.png";

    // ── Sprites cargados una vez por sesión de build ─────────────────────────
    static Sprite[] _interfazSprites;
    static Sprite   _logoSprite;

    static Sprite[] InterfazSprites =>
        _interfazSprites ??= AssetDatabase.LoadAllAssetsAtPath(InterfazPath)
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();

    static Sprite LogoSprite =>
        _logoSprite ??= AssetDatabase.LoadAssetAtPath<Sprite>(LogoPath);

    static Sprite Iface(int index) =>
        (InterfazSprites != null && index < InterfazSprites.Length) ? InterfazSprites[index] : null;

    // ── Punto de entrada principal ──────────────────────────────────────────

    [MenuItem("La Llorona/Generar Escenas y UI")]
    public static void BuildAllScenesAndUI()
    {
        Debug.Log("[UIBuilder] Iniciando generación completa de UI y escenas...");
        Directory.CreateDirectory("Assets/Scenes");

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

        // Fondo negro
        if (Camera.main != null) Camera.main.backgroundColor = Color.black;

        GameObject canvasGo = CreateCanvas("SplashCanvas");

        // Fondo completo con el logo del juego
        GameObject logoGo = CreateImageSprite(canvasGo.transform, "LogoImagen", LogoSprite,
            Vector2.zero, Vector2.one);

        // Texto "Presiona cualquier tecla"
        CreateText(canvasGo.transform, "ContinueText",
            "Presiona cualquier tecla para continuar",
            24, TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.85f, 0.9f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(600, 40))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 50f);

        SplashScreen splash = canvasGo.AddComponent<SplashScreen>();

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

        // Panel central translúcido con barra de madera (interfaz_4) de fondo
        GameObject panelGo = CreateImageSprite(canvasGo.transform, "MenuPanel",
            Iface(4),    // barra de madera — usamos como panel decorativo
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(620, 520));
        panelGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.88f);

        // Título del juego encima del panel
        CreateText(panelGo.transform, "Titulo", "La Llorona del Callejón",
            34, TextAnchor.MiddleCenter, new Color(0.9f, 0.8f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(580, 60))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 30f);

        // Botones del menú principal usando sprites reales
        float startY = 180f, step = 72f;
        Button btnJugar    = CreateSpriteButton(panelGo.transform, "Btn_Jugar",    "JUGAR",          new Vector2(0f, startY),         new Vector2(380, 58));
        Button btnContinue = CreateSpriteButton(panelGo.transform, "Btn_Continuar","CONTINUAR",      new Vector2(0f, startY - step),   new Vector2(380, 58));
        Button btnSelect   = CreateSpriteButton(panelGo.transform, "Btn_Niveles",  "SELECCIONAR NIVEL", new Vector2(0f, startY - step*2),new Vector2(380, 58));
        Button btnOpciones = CreateSpriteButton(panelGo.transform, "Btn_Opciones", "OPCIONES",       new Vector2(0f, startY - step*3), new Vector2(380, 58));
        Button btnCredits  = CreateSpriteButton(panelGo.transform, "Btn_Creditos", "CRÉDITOS",       new Vector2(0f, startY - step*4), new Vector2(380, 58));
        Button btnSalir    = CreateSpriteButton(panelGo.transform, "Btn_Salir",    "SALIR",          new Vector2(0f, startY - step*5), new Vector2(380, 58));

        MainMenuUI menu = canvasGo.AddComponent<MainMenuUI>();
        SerializedObject so = new SerializedObject(menu);
        so.FindProperty("playButton").objectReferenceValue          = btnJugar;
        so.FindProperty("continueButton").objectReferenceValue      = btnContinue;
        so.FindProperty("levelSelectButton").objectReferenceValue   = btnSelect;
        so.FindProperty("optionsButton").objectReferenceValue       = btnOpciones;
        so.FindProperty("creditsButton").objectReferenceValue       = btnCredits;
        so.FindProperty("quitButton").objectReferenceValue          = btnSalir;
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

        // Fondo oscuro con logo
        GameObject bg = CreateImageSprite(canvasGo.transform, "Fondo", LogoSprite, Vector2.zero, Vector2.one);
        bg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

        // Título
        CreateText(canvasGo.transform, "Titulo", "SELECCIONAR NIVEL",
            38, TextAnchor.MiddleCenter, new Color(0.9f, 0.8f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(700, 70))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -50f);

        // 4 botones de nivel en grid 2×2
        string[] nombres = { "Nivel 1\nEl Mercado", "Nivel 2\nEl Callejón", "Nivel 3\nEl Atrio", "Nivel 4\nEl Panteón" };
        Vector2[] posiciones = {
            new Vector2(-200f,  100f), new Vector2( 200f,  100f),
            new Vector2(-200f, -100f), new Vector2( 200f, -100f)
        };

        Button[] levelBtns = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            levelBtns[i] = CreateSpriteButton(canvasGo.transform, $"Btn_Nivel{i+1}",
                nombres[i], posiciones[i], new Vector2(280, 130));
        }

        // Botón volver
        Button btnBack = CreateSpriteButton(canvasGo.transform, "Btn_Volver",
            "← Volver", new Vector2(0f, -280f), new Vector2(260, 54));

        LevelSelectUI lvlSel = canvasGo.AddComponent<LevelSelectUI>();
        SerializedObject so = new SerializedObject(lvlSel);
        SerializedProperty btns = so.FindProperty("levelButtons");
        btns.arraySize = 4;
        for (int i = 0; i < 4; i++) btns.GetArrayElementAtIndex(i).objectReferenceValue = levelBtns[i];
        so.FindProperty("backButton").objectReferenceValue = btnBack;
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

        // Fondo con logo tenue
        GameObject bg = CreateImageSprite(canvasGo.transform, "Fondo", LogoSprite, Vector2.zero, Vector2.one);
        bg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

        // Texto de créditos
        CreateText(canvasGo.transform, "Titulo", "CRÉDITOS",
            38, TextAnchor.UpperCenter, new Color(0.9f, 0.8f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(700, 60))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -60f);

        string credText =
            "Desarrollo: Carol & Princes\n" +
            "Motor: Unity 6.6 (Universal 2D)\n" +
            "Proyecto Escolar UTNG — 2026\n\n" +
            "Arte generado con IA generativa de imágenes\n" +
            "(Adobe Firefly / DALL·E)\n\n" +
            "Audio: pendiente (Freesound CC0)\n\n" +
            "Agradecimientos: Prof. de Desarrollo de Videojuegos UTNG";

        CreateText(canvasGo.transform, "TextoCreditos", credText,
            20, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(700, 500));

        Button btnBack = CreateSpriteButton(canvasGo.transform, "Btn_Volver",
            "← Volver al Menú", new Vector2(0f, -250f), new Vector2(320, 56));

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        Debug.Log("[UIBuilder] Escena Credits generada.");
    }

    // ── Canvas de Niveles (HUD + modales) ───────────────────────────────────

    public static void BuildLevel1UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level1_Mercado.unity",
        "Objetivo: Cruza el mercado sin que La Llorona te vea");
    public static void BuildLevel2UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level2_Callejon.unity",
        "Objetivo: Llega al final del callejón");
    public static void BuildLevel3UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level3_Atrio.unity",
        "Objetivo: Cruza el atrio — usa velas para distraerla");
    public static void BuildLevel4UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level4_Cementerio.unity",
        "Objetivo: ¡Llega a tu casa! La Llorona es más rápida");

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
        // HUD
        // ════════════════════════════════════════════════════════
        GameObject hudGo = new GameObject("HUD");
        hudGo.transform.SetParent(canvasGo.transform, false);
        HUD hud = hudGo.AddComponent<HUD>();

        // ── Barra superior de madera (interfaz_4) como fondo del HUD ─────────
        GameObject topBar = CreateImageSprite(hudGo.transform, "BarraHUD", Iface(4),
            new Vector2(0f, 1f), new Vector2(1f, 1f));
        topBar.GetComponent<RectTransform>().sizeDelta    = new Vector2(0f, 72f);
        topBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
        topBar.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);

        // ── Corazones (vidas) — sprites interfaz_0 y interfaz_1 ─────────────
        GameObject heartsRoot = new GameObject("Vidas_Corazones");
        heartsRoot.transform.SetParent(hudGo.transform, false);
        RectTransform rtH = heartsRoot.AddComponent<RectTransform>();
        rtH.anchorMin = new Vector2(0f, 1f);
        rtH.anchorMax = new Vector2(0f, 1f);
        rtH.pivot     = new Vector2(0f, 1f);
        rtH.anchoredPosition = new Vector2(14f, -10f);
        rtH.sizeDelta = new Vector2(160f, 52f);

        Image[] heartImgs = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject h = new GameObject($"Corazon_{i + 1}");
            h.transform.SetParent(heartsRoot.transform, false);
            Image hImg = h.AddComponent<Image>();
            hImg.sprite = Iface(0);          // corazón rojo lleno
            hImg.preserveAspect = true;
            RectTransform hRt = h.GetComponent<RectTransform>();
            hRt.anchorMin = hRt.anchorMax = new Vector2(0f, 0.5f);
            hRt.pivot     = new Vector2(0f, 0.5f);
            hRt.sizeDelta = new Vector2(48f, 48f);
            hRt.anchoredPosition = new Vector2(i * 54f, 0f);
            heartImgs[i] = hImg;
        }

        // ── Timer (centro arriba) ─────────────────────────────────────────────
        Text timerTxt = CreateText(hudGo.transform, "TimerText", "00:00", 30,
            TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.82f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(180f, 56f))
            .GetComponent<Text>();
        var timerRt = timerTxt.GetComponent<RectTransform>();
        timerRt.pivot = new Vector2(0.5f, 1f);
        timerRt.anchoredPosition = new Vector2(0f, -8f);
        timerTxt.fontStyle = FontStyle.Bold;
        timerTxt.gameObject.AddComponent<UnityEngine.UI.Shadow>().effectColor = new Color(0f,0f,0f,0.9f);

        // ── Puntaje (derecha del timer) ───────────────────────────────────────
        Text scoreTxt = CreateText(hudGo.transform, "ScoreText", "0 pts", 22,
            TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.4f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(180f, 34f))
            .GetComponent<Text>();
        var scoreRt = scoreTxt.GetComponent<RectTransform>();
        scoreRt.pivot = new Vector2(0.5f, 1f);
        scoreRt.anchoredPosition = new Vector2(0f, -46f);

        // ── Objetivo (arriba derecha — texto pequeño) ─────────────────────────
        Text objTxt = CreateText(hudGo.transform, "ObjectiveText",
            string.IsNullOrEmpty(objetivoTexto) ? "Objetivo: llega al otro extremo" : objetivoTexto,
            16, TextAnchor.MiddleRight, new Color(0.78f, 0.93f, 1f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(440f, 34f))
            .GetComponent<Text>();
        var objRt = objTxt.GetComponent<RectTransform>();
        objRt.pivot = new Vector2(1f, 1f);
        objRt.anchoredPosition = new Vector2(-14f, -18f);

        // ── Inventario de velas — icono sprite interfaz_2 + texto ────────────
        GameObject candleRoot = new GameObject("Velas_Inventario");
        candleRoot.transform.SetParent(hudGo.transform, false);
        RectTransform rtC = candleRoot.AddComponent<RectTransform>();
        rtC.anchorMin = rtC.anchorMax = new Vector2(0f, 1f);
        rtC.pivot     = new Vector2(0f, 1f);
        rtC.anchoredPosition = new Vector2(14f, -68f);
        rtC.sizeDelta = new Vector2(260f, 38f);

        // Icono vela
        GameObject velaIconGo = new GameObject("VelaIcon");
        velaIconGo.transform.SetParent(candleRoot.transform, false);
        Image velaIcon = velaIconGo.AddComponent<Image>();
        velaIcon.sprite = Iface(2);
        velaIcon.preserveAspect = true;
        RectTransform velaRt = velaIconGo.GetComponent<RectTransform>();
        velaRt.anchorMin = velaRt.anchorMax = new Vector2(0f, 0.5f);
        velaRt.pivot     = new Vector2(0f, 0.5f);
        velaRt.sizeDelta = new Vector2(32f, 32f);
        velaRt.anchoredPosition = new Vector2(0f, 0f);

        Text candleTxt = CreateText(candleRoot.transform, "CandleCountText",
            "×0  [F] Colocar", 18, TextAnchor.MiddleLeft,
            new Color(1f, 0.88f, 0.45f),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(200f, 36f))
            .GetComponent<Text>();
        candleTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(38f, 0f);
        candleRoot.SetActive(false);

        // ── Banner tutorial (franja inferior) ────────────────────────────────
        GameObject tutPanel = new GameObject("TutorialPanel");
        tutPanel.transform.SetParent(hudGo.transform, false);
        Image tutImg = tutPanel.AddComponent<Image>();
        tutImg.color = new Color(0.06f, 0.04f, 0.12f, 0.93f);
        RectTransform tutRt = tutPanel.GetComponent<RectTransform>();
        tutRt.anchorMin = new Vector2(0f, 0f);
        tutRt.anchorMax = new Vector2(1f, 0f);
        tutRt.pivot     = new Vector2(0.5f, 0f);
        tutRt.sizeDelta = new Vector2(0f, 88f);
        tutRt.anchoredPosition = new Vector2(0f, 0f);

        // Línea de acento izquierda (ojo — interfaz_3) como icono
        GameObject eyeGo = new GameObject("IconoOjo");
        eyeGo.transform.SetParent(tutPanel.transform, false);
        Image eyeImg = eyeGo.AddComponent<Image>();
        eyeImg.sprite = Iface(3);
        eyeImg.preserveAspect = true;
        RectTransform eyeRt = eyeGo.GetComponent<RectTransform>();
        eyeRt.anchorMin = eyeRt.anchorMax = new Vector2(0f, 0.5f);
        eyeRt.pivot     = new Vector2(0f, 0.5f);
        eyeRt.sizeDelta = new Vector2(56f, 56f);
        eyeRt.anchoredPosition = new Vector2(20f, 0f);

        CanvasGroup tutCg = tutPanel.AddComponent<CanvasGroup>();
        Text tutTxt = CreateText(tutPanel.transform, "TutorialText",
            "WASD para moverte · Shift para correr · E para esconderte",
            20, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-120f, 80f))
            .GetComponent<Text>();
        tutTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(40f, 0f);
        tutPanel.SetActive(false);

        // Enlazar HUD
        SerializedObject soHud = new SerializedObject(hud);
        SerializedProperty hpArray = soHud.FindProperty("heartImages");
        hpArray.arraySize = 3;
        for (int i = 0; i < 3; i++) hpArray.GetArrayElementAtIndex(i).objectReferenceValue = heartImgs[i];
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

        GameObject pausePanel = CreateImageSprite(pauseGo.transform, "PausePanel", Iface(4),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 480f));
        pausePanel.GetComponent<Image>().color = new Color(0.5f, 0.35f, 0.2f, 0.97f);

        CreateText(pausePanel.transform, "PausaTitulo", "PAUSA",
            44, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.7f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(420f, 68f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -20f);

        Button pResume  = CreateSpriteButton(pausePanel.transform, "Btn_Reanudar",  "Reanudar",       new Vector2(0f,  110f), new Vector2(340f, 56f));
        Button pRestart = CreateSpriteButton(pausePanel.transform, "Btn_Reiniciar", "Reiniciar Nivel",new Vector2(0f,  44f),  new Vector2(340f, 56f));
        Button pOptions = CreateSpriteButton(pausePanel.transform, "Btn_Opciones",  "Opciones",       new Vector2(0f, -22f),  new Vector2(340f, 56f));
        Button pMenu    = CreateSpriteButton(pausePanel.transform, "Btn_Menu",      "Menú Principal", new Vector2(0f, -88f),  new Vector2(340f, 56f));
        pausePanel.SetActive(false);

        SerializedObject soPause = new SerializedObject(pause);
        soPause.FindProperty("pausePanel").objectReferenceValue   = pausePanel;
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

        GameObject vicPanel = CreateImageSprite(victoryGo.transform, "VictoryPanel", Iface(4),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 460f));
        vicPanel.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.25f, 0.97f);

        CreateText(vicPanel.transform, "VicTitulo", "¡NIVEL COMPLETADO!",
            36, TextAnchor.MiddleCenter, new Color(0.5f, 1f, 0.5f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(500f, 60f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -28f);

        Text vicTime  = CreateText(vicPanel.transform, "VicTiempo", "Tiempo: 01:23", 26,
            TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420f, 40f))
            .GetComponent<Text>();
        vicTime.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);

        Text vicScore = CreateText(vicPanel.transform, "VicScore", "Puntos: 850", 26,
            TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.4f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420f, 40f))
            .GetComponent<Text>();
        vicScore.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 16f);

        Text vicHigh  = CreateText(vicPanel.transform, "VicHigh", "Récord: 900", 20,
            TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.8f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420f, 34f))
            .GetComponent<Text>();
        vicHigh.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -20f);

        Button vicNext = CreateSpriteButton(vicPanel.transform, "Btn_Siguiente", "Siguiente Nivel →", new Vector2(0f, -90f),  new Vector2(320f, 54f));
        Button vicMenu = CreateSpriteButton(vicPanel.transform, "Btn_Menu",      "Menú Principal",   new Vector2(0f, -152f), new Vector2(320f, 54f));
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
        GameObject overGo = new GameObject("GameOverUI");
        overGo.transform.SetParent(canvasGo.transform, false);
        GameOverUI gameOver = overGo.AddComponent<GameOverUI>();

        GameObject overPanel = CreateImageSprite(overGo.transform, "GameOverPanel", Iface(4),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 390f));
        overPanel.GetComponent<Image>().color = new Color(0.45f, 0.08f, 0.08f, 0.97f);

        CreateText(overPanel.transform, "OverTitulo", "LA LLORONA TE ATRAPÓ",
            34, TextAnchor.MiddleCenter, new Color(1f, 0.3f, 0.3f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(480f, 60f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -28f);

        CreateText(overPanel.transform, "OverMensaje", "Te has quedado sin vidas...",
            22, TextAnchor.MiddleCenter, new Color(1f, 0.8f, 0.8f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420f, 40f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 50f);

        Button overRetry = CreateSpriteButton(overPanel.transform, "Btn_Reintentar", "Reintentar",     new Vector2(0f, -40f),  new Vector2(300f, 54f));
        Button overMenu  = CreateSpriteButton(overPanel.transform, "Btn_Menu",       "Menú Principal", new Vector2(0f, -102f), new Vector2(300f, 54f));
        overPanel.SetActive(false);

        SerializedObject soOver = new SerializedObject(gameOver);
        soOver.FindProperty("panel").objectReferenceValue       = overPanel;
        soOver.FindProperty("retryButton").objectReferenceValue = overRetry;
        soOver.FindProperty("menuButton").objectReferenceValue  = overMenu;
        soOver.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[UIBuilder] Canvas UI integrado en: " + path);
    }

    // ── Build Settings ────────────────────────────────────────────────────────

    public static void UpdateBuildSettings()
    {
        string[] scenePaths = {
            "Assets/Scenes/Splash.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/LevelSelect.unity",
            "Assets/Scenes/Level1_Mercado.unity",
            "Assets/Scenes/Level2_Callejon.unity",
            "Assets/Scenes/Level3_Atrio.unity",
            "Assets/Scenes/Level4_Cementerio.unity",
            "Assets/Scenes/Credits.unity"
        };

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[scenePaths.Length];
        for (int i = 0; i < scenePaths.Length; i++)
            scenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);

        EditorBuildSettings.scenes = scenes;
        Debug.Log("[UIBuilder] Build Settings actualizado con las 8 escenas del juego.");
    }

    // ════════════════════════════════════════════════════════════════════════
    // Helpers internos
    // ════════════════════════════════════════════════════════════════════════

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
        if (Object.FindAnyObjectByType<EventSystem>() == null)
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

    /// <summary>Crea un Image usando un sprite real del proyecto.</summary>
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

    /// <summary>Crea un Image con color sólido (sin sprite).</summary>
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
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = sizeDelta;
        return go;
    }

    /// <summary>Botón con sprite de madera real (interfaz_5/6/7) y texto blanco.</summary>
    static Button CreateSpriteButton(Transform parent, string name, string text,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.sprite = Iface(5);           // botón normal de madera
        img.type   = Image.Type.Sliced;

        Button btn = go.AddComponent<Button>();
        // Usar sprites de estado reales
        SpriteState ss = new SpriteState
        {
            highlightedSprite = Iface(6),  // botón resaltado (dorado)
            pressedSprite     = Iface(7),  // botón presionado (oscuro)
            disabledSprite    = Iface(7)
        };
        btn.spriteState   = ss;
        btn.transition    = Selectable.Transition.SpriteSwap;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        // Texto del botón — color cálido para contrastar con la madera
        CreateText(go.transform, "Text", text, 22,
            TextAnchor.MiddleCenter, new Color(1f, 0.93f, 0.78f),
            Vector2.zero, Vector2.one, Vector2.zero);

        return btn;
    }

    static Slider CreateSlider(Transform parent, string name, Vector2 anchorCenter, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchorCenter;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = sizeDelta;

        CreateImage(go.transform, "Background", new Color(0.2f, 0.2f, 0.25f), Vector2.zero, Vector2.one);
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one;
        GameObject fill = CreateImage(fillArea.transform, "Fill", new Color(0.7f, 0.55f, 0.25f), Vector2.zero, Vector2.one);

        Slider slider = go.AddComponent<Slider>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;
        return slider;
    }
}
