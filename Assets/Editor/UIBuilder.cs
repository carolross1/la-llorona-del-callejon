// Assets/Editor/UIBuilder.cs
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Constructor automatizado de escenas y Canvas de interfaz:
/// - Splash.unity
/// - MainMenu.unity
/// - LevelSelect.unity
/// - Credits.unity
/// - Canvas de Level1_Mercado (HUD, Pausa, Fin de Nivel, Game Over)
///
/// Configura CanvasScaler (1920x1080, Match 0.5), anclajes y registro en Build Settings.
/// Ejecución:
/// Menú Unity: La Llorona > Generar Escenas y UI
/// Línea de comandos (batchmode): -executeMethod UIBuilder.BuildAllScenesAndUI
/// </summary>
public static class UIBuilder
{
    [MenuItem("La Llorona/Generar Escenas y UI")]
    public static void BuildAllScenesAndUI()
    {
        Debug.Log("[UIBuilder] Iniciando generación completa de UI y escenas...");
        Directory.CreateDirectory("Assets/Scenes");

        BuildSplashScene();
        BuildMainMenuScene();
        BuildLevelSelectScene();
        BuildCreditsScene();
        BuildLevel1UICanvas();

        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("[UIBuilder] ¡Todas las escenas y sistemas de UI fueron generados con éxito!");
    }

    // ---------------- Escena Splash ----------------
    public static void BuildSplashScene()
    {
        string path = "Assets/Scenes/Splash.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        EnsureEventSystem();

        GameObject canvasGo = CreateCanvas("SplashCanvas");
        SplashScreen splash = canvasGo.AddComponent<SplashScreen>();

        // Fondo oscuro
        CreateImage(canvasGo.transform, "Fondo", new Color(0.08f, 0.06f, 0.12f), Vector2.zero, Vector2.one);

        // Logo del equipo / juego
        Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/logo.png");
        GameObject logoGo = CreateImage(canvasGo.transform, "Logo", Color.white, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(400, 300));
        if (logoSprite != null)
            logoGo.GetComponent<Image>().sprite = logoSprite;

        // Título del juego
        CreateText(canvasGo.transform, "Titulo", "La Llorona del Callejón", 48, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), new Vector2(800, 80));

        // Subtítulo
        CreateText(canvasGo.transform, "Subtitulo", "UTNG — Proyecto escolar por Carol y Princes\n(Presiona cualquier tecla para continuar)", 22, TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.8f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(800, 60));

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[UIBuilder] Escena Splash generada.");
    }

    // ---------------- Escena MainMenu ----------------
    public static void BuildMainMenuScene()
    {
        string path = "Assets/Scenes/MainMenu.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        EnsureEventSystem();

        GameObject canvasGo = CreateCanvas("MainMenuCanvas");
        MainMenuUI menu = canvasGo.AddComponent<MainMenuUI>();

        // Fondo
        CreateImage(canvasGo.transform, "Fondo", new Color(0.09f, 0.07f, 0.14f), Vector2.zero, Vector2.one);

        // Título
        CreateText(canvasGo.transform, "Titulo", "La Llorona del Callejón", 56, TextAnchor.MiddleCenter, new Color(0.95f, 0.9f, 1f), new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(900, 90));

        // Botones del menú
        float startY = 0.62f;
        float stepY = 0.085f;

        Button btnPlay = CreateButton(canvasGo.transform, "Btn_Jugar", "Nueva Partida", new Vector2(0.5f, startY), new Vector2(320, 55));
        Button btnContinue = CreateButton(canvasGo.transform, "Btn_Continuar", "Continuar", new Vector2(0.5f, startY - stepY), new Vector2(320, 55));
        Button btnSelect = CreateButton(canvasGo.transform, "Btn_SeleccionNivel", "Seleccionar Nivel", new Vector2(0.5f, startY - stepY * 2), new Vector2(320, 55));
        Button btnOptions = CreateButton(canvasGo.transform, "Btn_Opciones", "Opciones", new Vector2(0.5f, startY - stepY * 3), new Vector2(320, 55));
        Button btnCredits = CreateButton(canvasGo.transform, "Btn_Creditos", "Créditos", new Vector2(0.5f, startY - stepY * 4), new Vector2(320, 55));
        Button btnExit = CreateButton(canvasGo.transform, "Btn_Salir", "Salir", new Vector2(0.5f, startY - stepY * 5), new Vector2(320, 55));

        // Panel modal de Opciones
        GameObject optPanel = CreateImage(canvasGo.transform, "PanelOpciones", new Color(0.12f, 0.10f, 0.18f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(600, 480));
        CreateText(optPanel.transform, "OptTitulo", "Opciones", 36, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(400, 50));

        CreateText(optPanel.transform, "MusicaLabel", "Volumen de Música", 22, TextAnchor.MiddleLeft, Color.white, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(450, 30));
        Slider sldMusic = CreateSlider(optPanel.transform, "SliderMusica", new Vector2(0.5f, 0.64f), new Vector2(450, 25));

        CreateText(optPanel.transform, "SfxLabel", "Volumen de Efectos", 22, TextAnchor.MiddleLeft, Color.white, new Vector2(0.5f, 0.50f), new Vector2(0.5f, 0.50f), new Vector2(450, 30));
        Slider sldSfx = CreateSlider(optPanel.transform, "SliderSfx", new Vector2(0.5f, 0.42f), new Vector2(450, 25));

        CreateText(optPanel.transform, "CalidadLabel", "Calidad Gráfica", 22, TextAnchor.MiddleLeft, Color.white, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), new Vector2(450, 30));
        Dropdown ddQuality = CreateDropdown(optPanel.transform, "DropdownCalidad", new Vector2(0.5f, 0.20f), new Vector2(450, 35));

        Button btnCloseOpt = CreateButton(optPanel.transform, "Btn_CerrarOpciones", "Guardar y Cerrar", new Vector2(0.5f, 0.08f), new Vector2(250, 45));
        optPanel.SetActive(false);

        // Conectar campos en MainMenuUI vía SerializedObject
        SerializedObject so = new SerializedObject(menu);
        so.FindProperty("playButton").objectReferenceValue = btnPlay;
        so.FindProperty("continueButton").objectReferenceValue = btnContinue;
        so.FindProperty("levelSelectButton").objectReferenceValue = btnSelect;
        so.FindProperty("optionsButton").objectReferenceValue = btnOptions;
        so.FindProperty("creditsButton").objectReferenceValue = btnCredits;
        so.FindProperty("exitButton").objectReferenceValue = btnExit;
        so.FindProperty("optionsPanel").objectReferenceValue = optPanel;
        so.FindProperty("musicSlider").objectReferenceValue = sldMusic;
        so.FindProperty("sfxSlider").objectReferenceValue = sldSfx;
        so.FindProperty("qualityDropdown").objectReferenceValue = ddQuality;
        so.FindProperty("closeOptionsButton").objectReferenceValue = btnCloseOpt;
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[UIBuilder] Escena MainMenu generada.");
    }

    // ---------------- Escena LevelSelect ----------------
    public static void BuildLevelSelectScene()
    {
        string path = "Assets/Scenes/LevelSelect.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        EnsureEventSystem();

        GameObject canvasGo = CreateCanvas("LevelSelectCanvas");
        LevelSelectUI selectUI = canvasGo.AddComponent<LevelSelectUI>();

        CreateImage(canvasGo.transform, "Fondo", new Color(0.08f, 0.07f, 0.13f), Vector2.zero, Vector2.one);
        CreateText(canvasGo.transform, "Titulo", "Selección de Nivel", 48, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(800, 70));

        SerializedObject so = new SerializedObject(selectUI);
        SerializedProperty entriesProp = so.FindProperty("levelEntries");
        entriesProp.arraySize = 4;

        string[] names = { "Nivel 1: El Mercado Vacío", "Nivel 2: El Callejón del Beso", "Nivel 3: El Atrio de la Parroquia", "Nivel 4: El Cementerio" };
        string[] scenes = { "Level1_Mercado", "Level2_Callejon", "Level3_Atrio", "Level4_Cementerio" };

        for (int i = 0; i < 4; i++)
        {
            float yPos = 0.70f - (i * 0.13f);
            Button btn = CreateButton(canvasGo.transform, $"Btn_Nivel_{i + 1}", names[i], new Vector2(0.5f, yPos), new Vector2(550, 60));

            GameObject lockIcon = CreateText(btn.transform, "Lock", "(Bloqueado)", 18, TextAnchor.MiddleRight, new Color(1f, 0.4f, 0.4f), new Vector2(0.95f, 0.5f), new Vector2(0.95f, 0.5f), new Vector2(150, 40));

            SerializedProperty elem = entriesProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("levelNumber").intValue = i + 1;
            elem.FindPropertyRelative("sceneName").stringValue = scenes[i];
            elem.FindPropertyRelative("button").objectReferenceValue = btn;
            elem.FindPropertyRelative("lockIcon").objectReferenceValue = lockIcon;
        }

        Button btnBack = CreateButton(canvasGo.transform, "Btn_Volver", "Volver al Menú", new Vector2(0.5f, 0.10f), new Vector2(260, 50));
        so.FindProperty("backButton").objectReferenceValue = btnBack;
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[UIBuilder] Escena LevelSelect generada.");
    }

    // ---------------- Escena Credits ----------------
    public static void BuildCreditsScene()
    {
        string path = "Assets/Scenes/Credits.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        EnsureEventSystem();

        GameObject canvasGo = CreateCanvas("CreditsCanvas");
        CreateImage(canvasGo.transform, "Fondo", new Color(0.07f, 0.05f, 0.11f), Vector2.zero, Vector2.one);

        CreateText(canvasGo.transform, "Titulo", "Créditos", 48, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(600, 70));

        string creditsContent =
            "LA LLORONA DEL CALLEJÓN\n" +
            "Proyecto escolar — UTNG (Universidad Tecnológica del Norte de Guanajuato)\n\n" +
            "Desarrolladoras:\n" +
            "• Carol Ross\n" +
            "• Princes\n\n" +
            "Herramientas y Tecnologías:\n" +
            "• Motor: Unity 6.6 Universal 2D (URP)\n" +
            "• Control de versiones: Git & GitHub\n" +
            "• Generación de arte base: IA Generativa de Imágenes\n" +
            "• Audio: Freesound (CC0 / Licencia Libre)\n\n" +
            "Ambientado en Dolores Hidalgo, C.I.N., Guanajuato.";

        CreateText(canvasGo.transform, "Contenido", creditsContent, 22, TextAnchor.MiddleCenter, new Color(0.9f, 0.9f, 0.9f), new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), new Vector2(950, 420));

        Button btnBack = CreateButton(canvasGo.transform, "Btn_Volver", "Volver al Menú", new Vector2(0.5f, 0.10f), new Vector2(260, 50));
        btnBack.onClick.AddListener(() => {
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene("MainMenu");
            else SceneManager.LoadScene("MainMenu");
        });

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[UIBuilder] Escena Credits generada.");
    }

    // ---------------- Canvas de Level1_Mercado ----------------
    public static void BuildLevel1UICanvas()
    {
        string path = "Assets/Scenes/Level1_Mercado.unity";
        if (!File.Exists(path))
        {
            Debug.LogWarning("[UIBuilder] No se encontró Level1_Mercado.unity. Se debe construir primero.");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        EnsureEventSystem();

        // Borrar canvas UI anterior si existe
        GameObject oldCanvas = GameObject.Find("UICanvas");
        if (oldCanvas != null) Undo.DestroyObjectImmediate(oldCanvas);

        GameObject canvasGo = CreateCanvas("UICanvas");
        Undo.RegisterCreatedObjectUndo(canvasGo, "UICanvas");

        // --- HUD ---
        GameObject hudGo = new GameObject("HUD");
        hudGo.transform.SetParent(canvasGo.transform, false);
        HUD hud = hudGo.AddComponent<HUD>();

        // Corazones de vida (Arriba Izquierda)
        GameObject heartsRoot = new GameObject("Vidas_Corazones");
        heartsRoot.transform.SetParent(hudGo.transform, false);
        RectTransform rtHearts = heartsRoot.AddComponent<RectTransform>();
        rtHearts.anchorMin = new Vector2(0.04f, 0.93f);
        rtHearts.anchorMax = new Vector2(0.04f, 0.93f);
        rtHearts.sizeDelta = new Vector2(160, 45);

        Image[] heartImgs = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject h = CreateImage(heartsRoot.transform, $"Corazon_{i + 1}", Color.red, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36, 36));
            RectTransform hRt = h.GetComponent<RectTransform>();
            hRt.anchoredPosition = new Vector2(i * 45f + 20f, 0f);
            heartImgs[i] = h.GetComponent<Image>();
        }

        // Cronómetro y Puntaje (Arriba Derecha)
        Text timerTxt = CreateText(hudGo.transform, "TimerText", "Tiempo: 00:00", 24, TextAnchor.MiddleRight, Color.white, new Vector2(0.96f, 0.95f), new Vector2(0.96f, 0.95f), new Vector2(250, 35)).GetComponent<Text>();
        Text scoreTxt = CreateText(hudGo.transform, "ScoreText", "Puntos: 0", 24, TextAnchor.MiddleRight, Color.yellow, new Vector2(0.96f, 0.90f), new Vector2(0.96f, 0.90f), new Vector2(250, 35)).GetComponent<Text>();

        // Objetivo (Arriba Centro)
        Text objTxt = CreateText(hudGo.transform, "ObjectiveText", "Objetivo: Cruza el mercado sin que La Llorona te vea", 22, TextAnchor.MiddleCenter, new Color(0.85f, 0.95f, 1f), new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.93f), new Vector2(700, 40)).GetComponent<Text>();

        // Banner de Tutorial Contextual (Abajo Centro)
        GameObject tutPanel = CreateImage(hudGo.transform, "TutorialPanel", new Color(0.1f, 0.1f, 0.15f, 0.9f), new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), new Vector2(800, 90));
        CanvasGroup tutCg = tutPanel.AddComponent<CanvasGroup>();
        Text tutTxt = CreateText(tutPanel.transform, "TutorialText", "WASD para moverte. Shift para correr.", 22, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760, 75)).GetComponent<Text>();
        tutPanel.SetActive(false);

        // Enlazar HUD
        SerializedObject soHud = new SerializedObject(hud);
        SerializedProperty hpArray = soHud.FindProperty("heartImages");
        hpArray.arraySize = 3;
        for (int i = 0; i < 3; i++) hpArray.GetArrayElementAtIndex(i).objectReferenceValue = heartImgs[i];
        soHud.FindProperty("timerText").objectReferenceValue = timerTxt;
        soHud.FindProperty("scoreText").objectReferenceValue = scoreTxt;
        soHud.FindProperty("objectiveText").objectReferenceValue = objTxt;
        soHud.FindProperty("tutorialPanel").objectReferenceValue = tutPanel;
        soHud.FindProperty("tutorialText").objectReferenceValue = tutTxt;
        soHud.FindProperty("tutorialCanvasGroup").objectReferenceValue = tutCg;
        soHud.ApplyModifiedProperties();

        // --- PauseMenu ---
        GameObject pauseGo = new GameObject("PauseMenu");
        pauseGo.transform.SetParent(canvasGo.transform, false);
        PauseMenu pause = pauseGo.AddComponent<PauseMenu>();

        GameObject pausePanel = CreateImage(pauseGo.transform, "PausePanel", new Color(0.06f, 0.05f, 0.10f, 0.95f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(480, 440));
        CreateText(pausePanel.transform, "PausaTitulo", "PAUSA", 42, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(300, 60));

        Button pResume = CreateButton(pausePanel.transform, "Btn_Reanudar", "Reanudar", new Vector2(0.5f, 0.65f), new Vector2(280, 50));
        Button pRestart = CreateButton(pausePanel.transform, "Btn_Reiniciar", "Reiniciar Nivel", new Vector2(0.5f, 0.50f), new Vector2(280, 50));
        Button pOptions = CreateButton(pausePanel.transform, "Btn_Opciones", "Opciones", new Vector2(0.5f, 0.35f), new Vector2(280, 50));
        Button pMenu = CreateButton(pausePanel.transform, "Btn_Menu", "Menú Principal", new Vector2(0.5f, 0.20f), new Vector2(280, 50));
        pausePanel.SetActive(false);

        SerializedObject soPause = new SerializedObject(pause);
        soPause.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        soPause.FindProperty("resumeButton").objectReferenceValue = pResume;
        soPause.FindProperty("restartButton").objectReferenceValue = pRestart;
        soPause.FindProperty("optionsButton").objectReferenceValue = pOptions;
        soPause.FindProperty("menuButton").objectReferenceValue = pMenu;
        soPause.ApplyModifiedProperties();

        // --- LevelCompletedUI ---
        GameObject victoryGo = new GameObject("LevelCompletedUI");
        victoryGo.transform.SetParent(canvasGo.transform, false);
        LevelCompletedUI victory = victoryGo.AddComponent<LevelCompletedUI>();

        GameObject vicPanel = CreateImage(victoryGo.transform, "VictoryPanel", new Color(0.08f, 0.12f, 0.08f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560, 450));
        CreateText(vicPanel.transform, "VicTitulo", "¡NIVEL COMPLETADO!", 38, TextAnchor.MiddleCenter, new Color(0.4f, 1f, 0.4f), new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), new Vector2(500, 60));

        Text vicTime = CreateText(vicPanel.transform, "VicTiempo", "Tiempo: 01:23", 26, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(400, 40)).GetComponent<Text>();
        Text vicScore = CreateText(vicPanel.transform, "VicScore", "Puntos: 850", 26, TextAnchor.MiddleCenter, Color.yellow, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), new Vector2(400, 40)).GetComponent<Text>();
        Text vicHigh = CreateText(vicPanel.transform, "VicHigh", "Récord: 900", 22, TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.8f), new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), new Vector2(400, 35)).GetComponent<Text>();

        Button vicNext = CreateButton(vicPanel.transform, "Btn_Siguiente", "Siguiente Nivel", new Vector2(0.5f, 0.28f), new Vector2(300, 50));
        Button vicMenu = CreateButton(vicPanel.transform, "Btn_Menu", "Menú Principal", new Vector2(0.5f, 0.14f), new Vector2(300, 50));
        vicPanel.SetActive(false);

        SerializedObject soVic = new SerializedObject(victory);
        soVic.FindProperty("panel").objectReferenceValue = vicPanel;
        soVic.FindProperty("timeResultText").objectReferenceValue = vicTime;
        soVic.FindProperty("scoreResultText").objectReferenceValue = vicScore;
        soVic.FindProperty("highScoreText").objectReferenceValue = vicHigh;
        soVic.FindProperty("nextLevelButton").objectReferenceValue = vicNext;
        soVic.FindProperty("menuButton").objectReferenceValue = vicMenu;
        soVic.ApplyModifiedProperties();

        // --- GameOverUI ---
        GameObject overGo = new GameObject("GameOverUI");
        overGo.transform.SetParent(canvasGo.transform, false);
        GameOverUI gameOver = overGo.AddComponent<GameOverUI>();

        GameObject overPanel = CreateImage(overGo.transform, "GameOverPanel", new Color(0.15f, 0.04f, 0.04f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520, 380));
        CreateText(overPanel.transform, "OverTitulo", "LA LLORONA TE ATRAPÓ", 36, TextAnchor.MiddleCenter, new Color(1f, 0.3f, 0.3f), new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(480, 60));
        CreateText(overPanel.transform, "OverMensaje", "Te has quedado sin vidas.", 22, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.64f), new Vector2(400, 40));

        Button overRetry = CreateButton(overPanel.transform, "Btn_Reintentar", "Reintentar Nivel", new Vector2(0.5f, 0.40f), new Vector2(280, 50));
        Button overMenu = CreateButton(overPanel.transform, "Btn_Menu", "Menú Principal", new Vector2(0.5f, 0.22f), new Vector2(280, 50));
        overPanel.SetActive(false);

        SerializedObject soOver = new SerializedObject(gameOver);
        soOver.FindProperty("panel").objectReferenceValue = overPanel;
        soOver.FindProperty("retryButton").objectReferenceValue = overRetry;
        soOver.FindProperty("menuButton").objectReferenceValue = overMenu;
        soOver.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[UIBuilder] Canvas y controladores de UI integrados en Level1_Mercado.");
    }

    // ---------------- Registro de Escenas en Build Settings ----------------
    public static void UpdateBuildSettings()
    {
        string[] scenePaths = {
            "Assets/Scenes/Splash.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/LevelSelect.unity",
            "Assets/Scenes/Level1_Mercado.unity",
            "Assets/Scenes/Credits.unity"
        };

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[scenePaths.Length];
        for (int i = 0; i < scenePaths.Length; i++)
        {
            scenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
        }

        EditorBuildSettings.scenes = scenes;
        Debug.Log("[UIBuilder] Build Settings actualizado con las 5 escenas del juego.");
    }

    // ---------------- Constructores Básicos de uGUI ----------------

    static GameObject CreateCanvas(string name)
    {
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 10;

        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
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
        }
    }

    static GameObject CreateImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta = default)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color = color;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = Vector2.zero;
        if (sizeDelta != default)
            rt.sizeDelta = sizeDelta;

        return go;
    }

    static GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Text t = go.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.color = color;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = sizeDelta;

        return go;
    }

    static Button CreateButton(Transform parent, string name, string text, Vector2 anchorCenter, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.24f, 0.20f, 0.36f, 0.95f);

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.24f, 0.20f, 0.36f, 0.95f);
        cb.highlightedColor = new Color(0.38f, 0.32f, 0.58f, 1.0f);
        cb.pressedColor = new Color(0.18f, 0.14f, 0.28f, 1.0f);
        btn.colors = cb;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorCenter;
        rt.anchorMax = anchorCenter;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = sizeDelta;

        CreateText(go.transform, "Text", text, 22, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, Vector2.zero);

        return btn;
    }

    static Slider CreateSlider(Transform parent, string name, Vector2 anchorCenter, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorCenter;
        rt.anchorMax = anchorCenter;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = sizeDelta;

        // Fondo
        GameObject bg = CreateImage(go.transform, "Background", new Color(0.2f, 0.2f, 0.25f), Vector2.zero, Vector2.one);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;

        GameObject fill = CreateImage(fillArea.transform, "Fill", new Color(0.4f, 0.7f, 1.0f), Vector2.zero, Vector2.one);

        Slider slider = go.AddComponent<Slider>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        return slider;
    }

    static Dropdown CreateDropdown(Transform parent, string name, Vector2 anchorCenter, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorCenter;
        rt.anchorMax = anchorCenter;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = sizeDelta;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.22f, 0.20f, 0.32f);

        Dropdown dd = go.AddComponent<Dropdown>();
        Text label = CreateText(go.transform, "Label", "Calidad", 20, TextAnchor.MiddleLeft, Color.white, Vector2.zero, Vector2.one, Vector2.zero).GetComponent<Text>();
        dd.captionText = label;

        return dd;
    }
}
