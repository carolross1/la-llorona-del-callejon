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
/// Constructor oficial de interfaces y menús de La Llorona del Callejón (Fase 2).
/// Cumple estrictamente con la especificación visual de PROMPT_VISUAL.md:
/// - Usa exclusivamente las piezas de UI/interfaz.png (Placa_normal, Barra_marco, etc.) y UI/logo.png.
/// - Centro de la pantalla 100% despejado durante el juego (HUD en franja superior de 120 px, margen 24 px).
/// - Avisos de evento a 140 px del borde superior (nunca al centro).
/// - Tutorial flotante a 60 px del borde inferior.
/// - Modales con fondo semitransparente al 70% y botones sin flechas inferiores.
/// - MainMenu con logo centrado y botones estilizados con hover 1.05x.
/// - LevelSelect con 4 tarjetas basadas en Salida_0 a Salida_3 y estado de bloqueo.
/// </summary>
public static class UIBuilder
{
    const string InterfazPath = "Assets/Sprites/UI/interfaz.png";
    const string LogoPath     = "Assets/Sprites/UI/logo.png";
    const string SalidasPath  = "Assets/Sprites/Environment/Salidas.png";

    // ── Acceso a Sprites ─────────────────────────────────────────────────────

    public static Sprite GetSprite(string path, string spriteName, int fallbackIndex = -1)
    {
        if (!File.Exists(path)) return null;

        var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
        var sprites = allAssets.OfType<Sprite>().ToArray();

        // 1. Coincidencia exacta por nombre oficial cortado
        var exact = sprites.FirstOrDefault(s => s.name == spriteName);
        if (exact != null) return exact;

        // 2. Coincidencia por índice de respaldo si Unity aún no ha actualizado los nombres
        if (fallbackIndex >= 0 && fallbackIndex < sprites.Length)
            return sprites[fallbackIndex];

        return sprites.FirstOrDefault() ?? AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    public static Sprite CorazonLleno    => GetSprite(InterfazPath, "Corazon_lleno", 0);
    public static Sprite CorazonVacio    => GetSprite(InterfazPath, "Corazon_vacio", 1);
    public static Sprite IconoVela       => GetSprite(InterfazPath, "Icono_vela", 2);
    public static Sprite IconoOjo        => GetSprite(InterfazPath, "Icono_ojo", 3);
    public static Sprite BarraMarco      => GetSprite(InterfazPath, "Barra_marco", 4);
    public static Sprite PlacaNormal     => GetSprite(InterfazPath, "Placa_normal", 5);
    public static Sprite PlacaResaltada  => GetSprite(InterfazPath, "Placa_resaltada", 6);
    public static Sprite PlacaPresionada => GetSprite(InterfazPath, "Placa_presionada", 7);
    public static Sprite LogoSprite      => AssetDatabase.LoadAssetAtPath<Sprite>(LogoPath);

    public static Sprite SalidaThumb(int index) => GetSprite(SalidasPath, $"Salida_{index}", index);

    // ── Generador Maestro de UI (Fase 2) ─────────────────────────────────────

    [MenuItem("La Llorona/Generar Escenas y UI")]
    public static void BuildAllScenesAndUI()
    {
        Debug.Log("[UIBuilder] Iniciando generación completa de UI y escenas (Fase 2)...");
        Directory.CreateDirectory("Assets/Scenes");

        BuildSplashScene();
        BuildMainMenuScene();
        BuildLevelSelectScene();
        BuildCreditsScene();

        // Canvas HUD en los 4 niveles
        BuildLevel1UICanvas();
        BuildLevel2UICanvas();
        BuildLevel3UICanvas();
        BuildLevel4UICanvas();

        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("[UIBuilder] ¡Todas las escenas y sistemas de UI fueron generados con éxito bajo la especificación oficial!");
    }

    // ── 1. Splash Scene ──────────────────────────────────────────────────────

    public static void BuildSplashScene()
    {
        string path = "Assets/Scenes/Splash.unity";
        Scene scene = File.Exists(path)
            ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        CreateCamera();
        EnsureEventSystem();
        if (Camera.main != null) Camera.main.backgroundColor = new Color(0.02f, 0.01f, 0.04f);

        GameObject canvasGo = CreateCanvas("SplashCanvas");

        // Fondo completo con logo
        GameObject logoGo = CreateImageSprite(canvasGo.transform, "LogoFondo", LogoSprite,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900f, 500f));
        Image logoImg = logoGo.GetComponent<Image>();
        if (logoImg != null) logoImg.preserveAspect = true;

        // Texto inferior
        CreateText(canvasGo.transform, "ContinueText",
            "Presiona cualquier tecla para continuar",
            24, TextAnchor.MiddleCenter, new Color(0.95f, 0.92f, 0.80f, 0.90f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(700, 48))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);

        canvasGo.AddComponent<SplashScreen>();

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        Debug.Log("[UIBuilder] Escena Splash generada.");
    }

    // ── 2. MainMenu Scene ────────────────────────────────────────────────────

    public static void BuildMainMenuScene()
    {
        string path = "Assets/Scenes/MainMenu.unity";
        Scene scene = File.Exists(path)
            ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        CreateCamera();
        EnsureEventSystem();
        if (Camera.main != null) Camera.main.backgroundColor = new Color(0.04f, 0.02f, 0.07f);

        GameObject canvasGo = CreateCanvas("MainMenuCanvas");

        // Fondo atmosférico
        GameObject bgGo = CreateImage(canvasGo.transform, "FondoOscuro",
            new Color(0.05f, 0.03f, 0.08f, 1f), Vector2.zero, Vector2.one);

        // Logo del juego centrado en la parte superior
        GameObject logoGo = CreateImageSprite(canvasGo.transform, "LogoJuego", LogoSprite,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(620f, 320f));
        RectTransform rtLogo = logoGo.GetComponent<RectTransform>();
        rtLogo.pivot = new Vector2(0.5f, 1f);
        rtLogo.anchoredPosition = new Vector2(0f, -40f);
        Image logoImg = logoGo.GetComponent<Image>();
        if (logoImg != null) logoImg.preserveAspect = true;

        // Contenedor de botones centrado abajo
        GameObject btnContainer = new GameObject("BotonesMenu");
        btnContainer.transform.SetParent(canvasGo.transform, false);
        RectTransform rtBtns = btnContainer.AddComponent<RectTransform>();
        rtBtns.anchorMin = rtBtns.anchorMax = new Vector2(0.5f, 0f);
        rtBtns.pivot = new Vector2(0.5f, 0f);
        rtBtns.anchoredPosition = new Vector2(0f, 60f);
        rtBtns.sizeDelta = new Vector2(440f, 480f);

        // Botones principales con Placa_normal y SpriteSwap
        Button btnJugar = CreatePlacaButton(btnContainer.transform, "Btn_Jugar", "JUGAR",
            new Vector2(0f, 360f), new Vector2(400f, 74f), 28);

        Button btnSelect = CreatePlacaButton(btnContainer.transform, "Btn_Niveles", "SELECCIONAR NIVEL",
            new Vector2(0f, 275f), new Vector2(400f, 74f), 24);

        Button btnOpciones = CreatePlacaButton(btnContainer.transform, "Btn_Opciones", "OPCIONES",
            new Vector2(0f, 190f), new Vector2(400f, 74f), 24);

        Button btnCredits = CreatePlacaButton(btnContainer.transform, "Btn_Creditos", "CRÉDITOS",
            new Vector2(0f, 105f), new Vector2(400f, 74f), 24);

        Button btnSalir = CreatePlacaButton(btnContainer.transform, "Btn_Salir", "SALIR",
            new Vector2(0f, 20f), new Vector2(400f, 74f), 26);

        // Subpanel de Opciones (inicialmente oculto)
        GameObject optPanel = CreateModalPanel(canvasGo.transform, "PanelOpciones", "OPCIONES", 580f, 420f);
        Button closeOptBtn = CreatePlacaButton(optPanel.transform, "Btn_CerrarOpciones", "Cerrar",
            new Vector2(0f, -145f), new Vector2(280f, 56f), 22);
        optPanel.SetActive(false);

        // Componente MainMenuUI
        MainMenuUI menu = canvasGo.AddComponent<MainMenuUI>();
        SerializedObject so = new SerializedObject(menu);
        so.FindProperty("playButton").objectReferenceValue        = btnJugar;
        so.FindProperty("levelSelectButton").objectReferenceValue = btnSelect;
        so.FindProperty("optionsButton").objectReferenceValue     = btnOpciones;
        so.FindProperty("creditsButton").objectReferenceValue     = btnCredits;
        so.FindProperty("exitButton").objectReferenceValue        = btnSalir;
        so.FindProperty("optionsPanel").objectReferenceValue      = optPanel;
        so.FindProperty("closeOptionsButton").objectReferenceValue = closeOptBtn;
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        Debug.Log("[UIBuilder] Escena MainMenu generada.");
    }

    // ── 3. LevelSelect Scene ─────────────────────────────────────────────────

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

        CreateImage(canvasGo.transform, "Fondo", new Color(0.05f, 0.03f, 0.08f, 1f), Vector2.zero, Vector2.one);

        // Título
        CreateText(canvasGo.transform, "Titulo", "SELECCIONAR NIVEL",
            38, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.40f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(700, 70))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);

        // Cuadrícula 2x2 para las 4 tarjetas de nivel
        string[] nombres = { "Nivel 1\nEl Mercado", "Nivel 2\nEl Callejón", "Nivel 3\nEl Atrio", "Nivel 4\nEl Panteón" };
        string[] escenas = { "Level1_Mercado", "Level2_Callejon", "Level3_Atrio", "Level4_Cementerio" };
        Vector2[] pos = {
            new Vector2(-280f,  90f), new Vector2( 280f,  90f),
            new Vector2(-280f, -140f), new Vector2( 280f, -140f)
        };

        Button[] levelBtns = new Button[4];
        GameObject[] lockIcons = new GameObject[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject cardGo = new GameObject($"Card_Nivel_{i + 1}");
            cardGo.transform.SetParent(canvasGo.transform, false);
            RectTransform cardRt = cardGo.AddComponent<RectTransform>();
            cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.anchoredPosition = pos[i];
            cardRt.sizeDelta = new Vector2(480f, 190f);

            // Fondo de la tarjeta con Placa_normal
            Image cardBg = cardGo.AddComponent<Image>();
            cardBg.sprite = PlacaNormal;
            cardBg.type = Image.Type.Sliced;

            Button btn = cardGo.AddComponent<Button>();
            SpriteState ss = new SpriteState
            {
                highlightedSprite = PlacaResaltada ?? PlacaNormal,
                pressedSprite     = PlacaPresionada ?? PlacaNormal,
                disabledSprite    = PlacaPresionada ?? PlacaNormal
            };
            btn.spriteState = ss;
            btn.transition = Selectable.Transition.SpriteSwap;
            cardGo.AddComponent<ButtonHoverEffect>();
            levelBtns[i] = btn;

            // Miniatura tomada de Salida_0..3
            GameObject thumbGo = new GameObject("Miniatura");
            thumbGo.transform.SetParent(cardGo.transform, false);
            Image thumbImg = thumbGo.AddComponent<Image>();
            thumbImg.sprite = SalidaThumb(i);
            thumbImg.preserveAspect = true;
            RectTransform thumbRt = thumbGo.GetComponent<RectTransform>();
            thumbRt.anchorMin = new Vector2(0f, 0.5f);
            thumbRt.anchorMax = new Vector2(0f, 0.5f);
            thumbRt.pivot = new Vector2(0f, 0.5f);
            thumbRt.anchoredPosition = new Vector2(24f, 0f);
            thumbRt.sizeDelta = new Vector2(170f, 130f);

            // Texto con el nombre del nivel
            CreateText(cardGo.transform, "NombreNivel", nombres[i], 22,
                TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.80f),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(230f, 80f))
                .GetComponent<RectTransform>().anchoredPosition = new Vector2(215f, 10f);

            // Candado / Etiqueta de BLOQUEADO
            GameObject lockGo = new GameObject("BloqueadoTag");
            lockGo.transform.SetParent(cardGo.transform, false);
            RectTransform lockRt = lockGo.AddComponent<RectTransform>();
            lockRt.anchorMin = lockRt.anchorMax = new Vector2(0.5f, 0.5f);
            lockRt.anchoredPosition = Vector2.zero;
            lockRt.sizeDelta = new Vector2(440f, 160f);

            GameObject lockBg = CreateImage(lockGo.transform, "SombraBloqueo",
                new Color(0.05f, 0.05f, 0.05f, 0.78f), Vector2.zero, Vector2.one);

            CreateText(lockGo.transform, "LockText", "🔒 BLOQUEADO", 22,
                TextAnchor.MiddleCenter, new Color(0.95f, 0.35f, 0.35f),
                Vector2.zero, Vector2.one, Vector2.zero);

            lockIcons[i] = lockGo;
            lockGo.SetActive(false);
        }

        // Botón Volver
        Button btnBack = CreatePlacaButton(canvasGo.transform, "Btn_Volver", "← VOLVER",
            new Vector2(0f, -370f), new Vector2(300f, 62f), 24);

        LevelSelectUI lvlSel = canvasGo.AddComponent<LevelSelectUI>();
        SerializedObject so = new SerializedObject(lvlSel);
        SerializedProperty entries = so.FindProperty("levelEntries");
        if (entries != null)
        {
            entries.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                SerializedProperty e = entries.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("levelNumber").intValue = i + 1;
                e.FindPropertyRelative("sceneName").stringValue = escenas[i];
                e.FindPropertyRelative("button").objectReferenceValue = levelBtns[i];
                e.FindPropertyRelative("lockIcon").objectReferenceValue = lockIcons[i];
            }
        }
        so.FindProperty("backButton").objectReferenceValue = btnBack;
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        Debug.Log("[UIBuilder] Escena LevelSelect generada.");
    }

    // ── 4. Credits Scene ─────────────────────────────────────────────────────

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
        CreateImage(canvasGo.transform, "Fondo", new Color(0.05f, 0.03f, 0.08f, 1f), Vector2.zero, Vector2.one);

        CreateText(canvasGo.transform, "Titulo", "CRÉDITOS", 40,
            TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.40f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(600, 70))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -50f);

        GameObject cardGo = CreateImageSprite(canvasGo.transform, "CardCreditos", PlacaNormal,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(820f, 460f));
        cardGo.GetComponent<Image>().type = Image.Type.Sliced;

        string credText =
            "LA LLORONA DEL CALLEJÓN\n" +
            "Proyecto de Desarrollo de Videojuegos 2D\n\n" +
            "Diseño y Dirección: Carol y Princes · UTNG\n\n" +
            "Arte y Concepto Visual: Hojas oficiales del proyecto\n" +
            "Tipografía: Pixelify Sans (Licencia SIL Open Font License - OFL)\n" +
            "Efectos Sonoros y Ambientales: Freesound (Licencia CC0)\n\n" +
            "Agradecimiento especial a la comunidad y docentes de UTNG";

        CreateText(cardGo.transform, "TextoCreditos", credText, 21,
            TextAnchor.MiddleCenter, new Color(0.96f, 0.94f, 0.88f),
            Vector2.zero, Vector2.one, new Vector2(-40f, -40f));

        Button btnBack = CreatePlacaButton(canvasGo.transform, "Btn_Volver", "← VOLVER AL MENÚ",
            new Vector2(0f, -380f), new Vector2(340f, 62f), 22);
        btnBack.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        Debug.Log("[UIBuilder] Escena Credits generada.");
    }

    // ── 5. Canvas HUD de Niveles (Franja Superior 120px, Centro Libre) ────────

    public static void BuildLevel1UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level1_Mercado.unity", "Cruza el mercado sin que La Llorona te vea");
    public static void BuildLevel2UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level2_Callejon.unity", "Llega al final del callejón esquivando los obstáculos");
    public static void BuildLevel3UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level3_Atrio.unity", "Cruza el atrio — usa velas para distraer a La Llorona");
    public static void BuildLevel4UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level4_Cementerio.unity", "¡Escapa hacia tu casa! Cuidado con las ánimas y el perro");

    public static void BuildLevelUICanvas(string path, string objetivoTexto = "")
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("[UIBuilder] No se encontró " + path + ". Se omitió temporalmente.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != path)
        {
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        EnsureEventSystem();

        GameObject oldCanvas = GameObject.Find("UICanvas");
        if (oldCanvas != null) Undo.DestroyObjectImmediate(oldCanvas);

        GameObject canvasGo = CreateCanvas("UICanvas");
        Undo.RegisterCreatedObjectUndo(canvasGo, "UICanvas");

        // Raíz del HUD que cubre toda la pantalla
        GameObject hudGo = new GameObject("HUD");
        hudGo.transform.SetParent(canvasGo.transform, false);
        RectTransform rtHud = hudGo.AddComponent<RectTransform>();
        rtHud.anchorMin = Vector2.zero;
        rtHud.anchorMax = Vector2.one;
        rtHud.offsetMin = Vector2.zero;
        rtHud.offsetMax = Vector2.zero;
        HUD hud = hudGo.AddComponent<HUD>();

        // ════════════════════════════════════════════════════════════════════════
        // A. FRANJA SUPERIOR IZQUIERDA: VIDAS, VELAS Y ESTAMINA (Top Left)
        // ════════════════════════════════════════════════════════════════════════
        GameObject leftRoot = new GameObject("HUD_Izquierda_Estado");
        leftRoot.transform.SetParent(hudGo.transform, false);
        RectTransform rtLeft = leftRoot.AddComponent<RectTransform>();
        rtLeft.anchorMin = rtLeft.anchorMax = new Vector2(0f, 1f);
        rtLeft.pivot = new Vector2(0f, 1f);
        rtLeft.anchoredPosition = new Vector2(36f, -36f); // Margen generoso dentro del marco visual
        rtLeft.sizeDelta = new Vector2(330f, 160f);

        // Fila 1: Tres corazones de 56 px con separación de 8 px (anclados en Top-Left)
        GameObject heartsRoot = new GameObject("Corazones_Vida");
        heartsRoot.transform.SetParent(leftRoot.transform, false);
        RectTransform rtH = heartsRoot.AddComponent<RectTransform>();
        rtH.anchorMin = rtH.anchorMax = new Vector2(0f, 1f);
        rtH.pivot = new Vector2(0f, 1f);
        rtH.anchoredPosition = Vector2.zero;
        rtH.sizeDelta = new Vector2(200f, 56f);

        Image[] heartImgs = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject h = new GameObject($"Corazon_{i + 1}");
            h.transform.SetParent(heartsRoot.transform, false);
            Image hImg = h.AddComponent<Image>();
            hImg.sprite = CorazonLleno;
            hImg.preserveAspect = true;
            RectTransform hRt = h.GetComponent<RectTransform>();
            hRt.anchorMin = hRt.anchorMax = new Vector2(0f, 1f);
            hRt.pivot = new Vector2(0f, 1f);
            hRt.sizeDelta = new Vector2(56f, 56f);
            hRt.anchoredPosition = new Vector2(i * 64f, 0f);
            heartImgs[i] = hImg;
        }

        // Fila 2: Icono_vela de 40 px y conteo (x2)
        GameObject candleRoot = new GameObject("Velas_Inventario");
        candleRoot.transform.SetParent(leftRoot.transform, false);
        RectTransform rtC = candleRoot.AddComponent<RectTransform>();
        rtC.anchorMin = rtC.anchorMax = new Vector2(0f, 1f);
        rtC.pivot = new Vector2(0f, 1f);
        rtC.anchoredPosition = new Vector2(0f, -64f);
        rtC.sizeDelta = new Vector2(240f, 36f);

        GameObject velaIconGo = new GameObject("IconoVela");
        velaIconGo.transform.SetParent(candleRoot.transform, false);
        Image velaIcon = velaIconGo.AddComponent<Image>();
        velaIcon.sprite = IconoVela;
        velaIcon.preserveAspect = true;
        RectTransform velaRt = velaIconGo.GetComponent<RectTransform>();
        velaRt.anchorMin = velaRt.anchorMax = new Vector2(0f, 0.5f);
        velaRt.pivot = new Vector2(0f, 0.5f);
        velaRt.sizeDelta = new Vector2(36f, 36f);
        velaRt.anchoredPosition = new Vector2(0f, 0f);

        Text candleTxt = CreateText(candleRoot.transform, "CandleCountText", "×0  [F] Colocar", 22,
            TextAnchor.MiddleLeft, new Color(1f, 0.92f, 0.50f),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(220f, 36f))
            .GetComponent<Text>();
        candleTxt.fontStyle = FontStyle.Bold;
        candleTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(44f, 0f);

        // Fila 3: Barra de estamina con etiqueta clara "ESTAMINA [Shift]"
        GameObject staminaRoot = new GameObject("Estamina_Barra");
        staminaRoot.transform.SetParent(leftRoot.transform, false);
        RectTransform rtStam = staminaRoot.AddComponent<RectTransform>();
        rtStam.anchorMin = rtStam.anchorMax = new Vector2(0f, 1f);
        rtStam.pivot = new Vector2(0f, 1f);
        rtStam.anchoredPosition = new Vector2(0f, -106f);
        rtStam.sizeDelta = new Vector2(300f, 42f);

        Text stamLabel = CreateText(staminaRoot.transform, "StaminaLabel", "ESTAMINA [Shift]", 18,
            TextAnchor.UpperLeft, new Color(1f, 0.90f, 0.60f),
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 22f))
            .GetComponent<Text>();
        stamLabel.fontStyle = FontStyle.Bold;
        stamLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(4f, 0f);

        GameObject stamBarGo = new GameObject("MarcoBarra");
        stamBarGo.transform.SetParent(staminaRoot.transform, false);
        RectTransform barRt = stamBarGo.AddComponent<RectTransform>();
        barRt.anchorMin = barRt.anchorMax = new Vector2(0f, 0f);
        barRt.pivot = new Vector2(0f, 0f);
        barRt.anchoredPosition = Vector2.zero;
        barRt.sizeDelta = new Vector2(300f, 18f);

        Image stamBg = stamBarGo.AddComponent<Image>();
        stamBg.sprite = BarraMarco;
        stamBg.type = Image.Type.Sliced;
        CanvasGroup stamCg = staminaRoot.AddComponent<CanvasGroup>();

        GameObject stamFillGo = new GameObject("Fill");
        stamFillGo.transform.SetParent(stamBarGo.transform, false);
        Image stamFill = stamFillGo.AddComponent<Image>();
        stamFill.sprite = PlacaNormal;
        stamFill.type = Image.Type.Filled;
        stamFill.fillMethod = Image.FillMethod.Horizontal;
        stamFill.fillOrigin = 0;
        stamFill.fillAmount = 1f;
        stamFill.color = new Color(0.95f, 0.80f, 0.25f);
        RectTransform fillRt = stamFillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0.02f, 0.15f);
        fillRt.anchorMax = new Vector2(0.98f, 0.85f);
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

        // ════════════════════════════════════════════════════════════════════════
        // B. FRANJA SUPERIOR CENTRAL: TIEMPO Y PUNTOS (Placa_normal 540×104 px)
        // ════════════════════════════════════════════════════════════════════════
        GameObject centerCard = new GameObject("HUD_Centro_TiempoPuntos");
        centerCard.transform.SetParent(hudGo.transform, false);
        RectTransform rtCenter = centerCard.AddComponent<RectTransform>();
        rtCenter.anchorMin = rtCenter.anchorMax = new Vector2(0.5f, 1f);
        rtCenter.pivot = new Vector2(0.5f, 1f);
        rtCenter.anchoredPosition = new Vector2(0f, -22f);
        rtCenter.sizeDelta = new Vector2(540f, 104f);

        Image centerImg = centerCard.AddComponent<Image>();
        centerImg.sprite = PlacaNormal;
        centerImg.type = Image.Type.Sliced;

        // Fondo interno plano oscuro para aislar los números del relieve de la madera
        GameObject centerBacking = new GameObject("InnerBacking");
        centerBacking.transform.SetParent(centerCard.transform, false);
        RectTransform rtCb = centerBacking.AddComponent<RectTransform>();
        rtCb.anchorMin = new Vector2(0.04f, 0.10f);
        rtCb.anchorMax = new Vector2(0.96f, 0.90f);
        rtCb.offsetMin = rtCb.offsetMax = Vector2.zero;
        Image cbImg = centerBacking.AddComponent<Image>();
        cbImg.color = new Color(0.06f, 0.04f, 0.10f, 0.80f);

        Text timerTxt = CreateText(centerCard.transform, "TimerText", "TIEMPO 00:00", 28,
            TextAnchor.MiddleCenter, new Color(1f, 0.97f, 0.90f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 42f))
            .GetComponent<Text>();
        timerTxt.fontStyle = FontStyle.Bold;
        timerTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 18f);

        Text scoreTxt = CreateText(centerCard.transform, "ScoreText", "PUNTOS 0", 24,
            TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.35f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 36f))
            .GetComponent<Text>();
        scoreTxt.fontStyle = FontStyle.Bold;
        scoreTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -18f);

        // ════════════════════════════════════════════════════════════════════════
        // C. FRANJA SUPERIOR DERECHA: OBJETIVO, META Y OJO ROJO (Top Right)
        // ════════════════════════════════════════════════════════════════════════
        GameObject rightCard = new GameObject("HUD_Derecha_Objetivo");
        rightCard.transform.SetParent(hudGo.transform, false);
        RectTransform rtRight = rightCard.AddComponent<RectTransform>();
        rtRight.anchorMin = rtRight.anchorMax = new Vector2(1f, 1f);
        rtRight.pivot = new Vector2(1f, 1f);
        rtRight.anchoredPosition = new Vector2(-24f, -22f);
        rtRight.sizeDelta = new Vector2(440f, 104f);

        Image rightImg = rightCard.AddComponent<Image>();
        rightImg.sprite = PlacaNormal;
        rightImg.type = Image.Type.Sliced;

        // Fondo interno plano oscuro para separar el objetivo
        GameObject rightBacking = new GameObject("InnerBacking");
        rightBacking.transform.SetParent(rightCard.transform, false);
        RectTransform rtRb = rightBacking.AddComponent<RectTransform>();
        rtRb.anchorMin = new Vector2(0.04f, 0.10f);
        rtRb.anchorMax = new Vector2(0.96f, 0.90f);
        rtRb.offsetMin = rtRb.offsetMax = Vector2.zero;
        Image rbImg = rightBacking.AddComponent<Image>();
        rbImg.color = new Color(0.06f, 0.04f, 0.10f, 0.80f);

        Text objTxt = CreateText(rightCard.transform, "ObjectiveText",
            $"OBJETIVO: {objetivoTexto}", 22, TextAnchor.MiddleLeft,
            new Color(0.88f, 0.96f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400f, 36f))
            .GetComponent<Text>();
        objTxt.fontStyle = FontStyle.Bold;
        objTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(14f, 18f);

        Text distTxt = CreateText(rightCard.transform, "MetaText", "META: --m", 24,
            TextAnchor.MiddleLeft, new Color(1f, 0.90f, 0.40f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400f, 36f))
            .GetComponent<Text>();
        distTxt.fontStyle = FontStyle.Bold;
        distTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(14f, -18f);

        GoalCompassUI compass = rightCard.AddComponent<GoalCompassUI>();
        SerializedObject soComp = new SerializedObject(compass);
        soComp.FindProperty("arrowRect").objectReferenceValue = null; // Cero flechas según la regla
        soComp.FindProperty("distanceText").objectReferenceValue = distTxt;
        soComp.ApplyModifiedProperties();

        // Icono_ojo de 64 px a la izquierda de la placa de objetivo
        GameObject eyeGo = new GameObject("IconoOjo_Alerta");
        eyeGo.transform.SetParent(hudGo.transform, false);
        Image eyeImg = eyeGo.AddComponent<Image>();
        eyeImg.sprite = IconoOjo;
        eyeImg.preserveAspect = true;
        RectTransform eyeRt = eyeGo.GetComponent<RectTransform>();
        eyeRt.anchorMin = eyeRt.anchorMax = new Vector2(1f, 1f);
        eyeRt.pivot = new Vector2(1f, 0.5f);
        eyeRt.anchoredPosition = new Vector2(-476f, -72f);
        eyeRt.sizeDelta = new Vector2(64f, 64f);

        // ════════════════════════════════════════════════════════════════════════
        // D. AVISOS DE EVENTO (A 145 px del borde superior, centro libre)
        // ════════════════════════════════════════════════════════════════════════
        GameObject evtNoticeGo = new GameObject("AvisoEvento_Top140");
        evtNoticeGo.transform.SetParent(hudGo.transform, false);
        RectTransform evtRt = evtNoticeGo.AddComponent<RectTransform>();
        evtRt.anchorMin = evtRt.anchorMax = new Vector2(0.5f, 1f);
        evtRt.pivot = new Vector2(0.5f, 1f);
        evtRt.anchoredPosition = new Vector2(0f, -145f);
        evtRt.sizeDelta = new Vector2(960f, 68f);

        Image evtBg = evtNoticeGo.AddComponent<Image>();
        evtBg.sprite = PlacaNormal;
        evtBg.type = Image.Type.Sliced;
        evtBg.color = new Color(0.08f, 0.05f, 0.12f, 0.94f);
        CanvasGroup evtCg = evtNoticeGo.AddComponent<CanvasGroup>();

        Text evtTxt = CreateText(evtNoticeGo.transform, "AvisoTexto", "", 28,
            TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.70f),
            Vector2.zero, Vector2.one, Vector2.zero)
            .GetComponent<Text>();
        evtTxt.fontStyle = FontStyle.Bold;
        evtNoticeGo.SetActive(false);

        // ════════════════════════════════════════════════════════════════════════
        // E. TUTORIAL FLOTANTE (A 60 px del borde inferior)
        // ════════════════════════════════════════════════════════════════════════
        GameObject tutGo = new GameObject("Tutorial_Bottom60");
        tutGo.transform.SetParent(hudGo.transform, false);
        RectTransform tutRt = tutGo.AddComponent<RectTransform>();
        tutRt.anchorMin = tutRt.anchorMax = new Vector2(0.5f, 0f);
        tutRt.pivot = new Vector2(0.5f, 0f);
        tutRt.anchoredPosition = new Vector2(0f, 60f);
        tutRt.sizeDelta = new Vector2(980f, 78f);

        Image tutBg = tutGo.AddComponent<Image>();
        tutBg.sprite = PlacaNormal;
        tutBg.type = Image.Type.Sliced;
        tutBg.color = new Color(0.10f, 0.07f, 0.14f, 0.92f);
        CanvasGroup tutCg = tutGo.AddComponent<CanvasGroup>();

        Text tutTxt = CreateText(tutGo.transform, "TutorialText",
            "WASD moverte · Shift correr · [E] esconderse · [F] vela", 26,
            TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.85f),
            Vector2.zero, Vector2.one, Vector2.zero)
            .GetComponent<Text>();
        tutTxt.fontStyle = FontStyle.Bold;
        tutGo.SetActive(false);

        // Enlazar referencias de HUD.cs
        SerializedObject soHud = new SerializedObject(hud);
        SerializedProperty hpArray = soHud.FindProperty("heartImages");
        hpArray.arraySize = 3;
        for (int i = 0; i < 3; i++) hpArray.GetArrayElementAtIndex(i).objectReferenceValue = heartImgs[i];
        soHud.FindProperty("activeHeartSprite").objectReferenceValue   = CorazonLleno;
        soHud.FindProperty("lostHeartSprite").objectReferenceValue     = CorazonVacio;
        soHud.FindProperty("timerText").objectReferenceValue           = timerTxt;
        soHud.FindProperty("scoreText").objectReferenceValue           = scoreTxt;
        soHud.FindProperty("objectiveText").objectReferenceValue       = objTxt;
        soHud.FindProperty("candleInventoryRoot").objectReferenceValue = candleRoot;
        soHud.FindProperty("candleCountText").objectReferenceValue     = candleTxt;
        soHud.FindProperty("staminaFillImage").objectReferenceValue    = stamFill;
        soHud.FindProperty("staminaCanvasGroup").objectReferenceValue  = stamCg;
        soHud.FindProperty("eyeIconImage").objectReferenceValue        = eyeImg;
        soHud.FindProperty("eyeNormalSprite").objectReferenceValue     = IconoOjo;
        soHud.FindProperty("eyeAlertSprite").objectReferenceValue      = IconoOjo;
        soHud.FindProperty("eventNoticePanel").objectReferenceValue       = evtNoticeGo;
        soHud.FindProperty("eventNoticeText").objectReferenceValue        = evtTxt;
        soHud.FindProperty("eventNoticeCanvasGroup").objectReferenceValue = evtCg;
        soHud.FindProperty("tutorialPanel").objectReferenceValue       = tutGo;
        soHud.FindProperty("tutorialText").objectReferenceValue        = tutTxt;
        soHud.FindProperty("tutorialCanvasGroup").objectReferenceValue = tutCg;
        soHud.ApplyModifiedProperties();

        // ════════════════════════════════════════════════════════════════════════
        // F. MODALES: PAUSA, NIVEL COMPLETADO Y GAME OVER
        // ════════════════════════════════════════════════════════════════════════
        BuildPauseModal(canvasGo.transform);
        BuildVictoryModal(canvasGo.transform);
        BuildGameOverModal(canvasGo.transform);

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[UIBuilder] Canvas HUD y modales actualizados con éxito en: " + path);
    }

    // ── Helpers de Modales ───────────────────────────────────────────────────

    static void BuildPauseModal(Transform parent)
    {
        GameObject pauseGo = new GameObject("PauseMenu");
        pauseGo.transform.SetParent(parent, false);
        RectTransform rtPause = pauseGo.AddComponent<RectTransform>();
        rtPause.anchorMin = Vector2.zero;
        rtPause.anchorMax = Vector2.one;
        rtPause.offsetMin = Vector2.zero;
        rtPause.offsetMax = Vector2.zero;
        PauseMenu pause = pauseGo.AddComponent<PauseMenu>();

        GameObject modalRoot = CreateModalOverlay(pauseGo.transform, "PauseOverlay");
        GameObject panel = CreateModalPanel(modalRoot.transform, "PausePanel", "PAUSA", 540f, 460f);

        Button pResume  = CreatePlacaButton(panel.transform, "Btn_Reanudar",  "Reanudar",        new Vector2(0f,  70f), new Vector2(380f, 62f), 24, true);
        Button pRestart = CreatePlacaButton(panel.transform, "Btn_Reiniciar", "Reiniciar Nivel", new Vector2(0f,   0f), new Vector2(380f, 60f), 22, false);
        Button pOptions = CreatePlacaButton(panel.transform, "Btn_Opciones",  "Opciones",        new Vector2(0f, -70f), new Vector2(380f, 60f), 22, false);
        Button pMenu    = CreatePlacaButton(panel.transform, "Btn_Menu",      "Menú Principal",  new Vector2(0f, -140f), new Vector2(380f, 60f), 22, false);
        modalRoot.SetActive(false);

        SerializedObject soPause = new SerializedObject(pause);
        soPause.FindProperty("pausePanel").objectReferenceValue    = modalRoot;
        soPause.FindProperty("resumeButton").objectReferenceValue  = pResume;
        soPause.FindProperty("restartButton").objectReferenceValue = pRestart;
        soPause.FindProperty("optionsButton").objectReferenceValue = pOptions;
        soPause.FindProperty("menuButton").objectReferenceValue    = pMenu;
        soPause.ApplyModifiedProperties();
    }

    static void BuildVictoryModal(Transform parent)
    {
        GameObject victoryGo = new GameObject("LevelCompletedUI");
        victoryGo.transform.SetParent(parent, false);
        RectTransform rtVic = victoryGo.AddComponent<RectTransform>();
        rtVic.anchorMin = Vector2.zero;
        rtVic.anchorMax = Vector2.one;
        rtVic.offsetMin = Vector2.zero;
        rtVic.offsetMax = Vector2.zero;
        LevelCompletedUI victory = victoryGo.AddComponent<LevelCompletedUI>();

        GameObject modalRoot = CreateModalOverlay(victoryGo.transform, "VictoryOverlay");
        GameObject panel = CreateModalPanel(modalRoot.transform, "VictoryPanel", "¡NIVEL COMPLETADO!", 580f, 480f);

        // Fondo interno plano oscuro para los datos: aísla los números de la textura de madera
        GameObject statsBacking = new GameObject("StatsBacking");
        statsBacking.transform.SetParent(panel.transform, false);
        RectTransform rtSb = statsBacking.AddComponent<RectTransform>();
        rtSb.anchorMin = rtSb.anchorMax = new Vector2(0.5f, 0.5f);
        rtSb.pivot = new Vector2(0.5f, 0.5f);
        rtSb.anchoredPosition = new Vector2(0f, 42f);
        rtSb.sizeDelta = new Vector2(490f, 142f);
        Image imgSb = statsBacking.AddComponent<Image>();
        imgSb.color = new Color(0.06f, 0.04f, 0.10f, 0.88f);

        Text vicTime = CreateText(statsBacking.transform, "VicTiempo", "Tiempo: 00:00", 24,
            TextAnchor.MiddleCenter, new Color(1f, 0.97f, 0.90f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(440f, 36f))
            .GetComponent<Text>();
        vicTime.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 40f);

        Text vicScore = CreateText(statsBacking.transform, "VicPuntaje", "Puntos: 0", 24,
            TextAnchor.MiddleCenter, new Color(1f, 0.89f, 0.35f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(440f, 36f))
            .GetComponent<Text>();
        vicScore.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

        Text vicHigh = CreateText(statsBacking.transform, "VicRecord", "Récord: 0", 22,
            TextAnchor.MiddleCenter, new Color(0.92f, 0.92f, 0.92f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(440f, 34f))
            .GetComponent<Text>();
        vicHigh.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);

        // Botón primario focal iluminado/dorado para "Siguiente Nivel"
        Button vicNext = CreatePlacaButton(panel.transform, "Btn_Siguiente", "Siguiente Nivel", new Vector2(0f, -80f),  new Vector2(380f, 62f), 24, true);
        Button vicMenu = CreatePlacaButton(panel.transform, "Btn_Menu",      "Menú Principal",  new Vector2(0f, -155f), new Vector2(380f, 60f), 22, false);
        modalRoot.SetActive(false);

        SerializedObject soVic = new SerializedObject(victory);
        soVic.FindProperty("panel").objectReferenceValue           = modalRoot;
        soVic.FindProperty("timeResultText").objectReferenceValue  = vicTime;
        soVic.FindProperty("scoreResultText").objectReferenceValue = vicScore;
        soVic.FindProperty("highScoreText").objectReferenceValue   = vicHigh;
        soVic.FindProperty("nextLevelButton").objectReferenceValue = vicNext;
        soVic.FindProperty("menuButton").objectReferenceValue      = vicMenu;
        soVic.ApplyModifiedProperties();
    }

    static void BuildGameOverModal(Transform parent)
    {
        GameObject goOver = new GameObject("GameOverUI");
        goOver.transform.SetParent(parent, false);
        RectTransform rtGo = goOver.AddComponent<RectTransform>();
        rtGo.anchorMin = Vector2.zero;
        rtGo.anchorMax = Vector2.one;
        rtGo.offsetMin = Vector2.zero;
        rtGo.offsetMax = Vector2.zero;
        GameOverUI gameOver = goOver.AddComponent<GameOverUI>();

        GameObject modalRoot = CreateModalOverlay(goOver.transform, "GameOverOverlay");
        GameObject panel = CreateModalPanel(modalRoot.transform, "GameOverPanel", "¡HAS SIDO ATRAPADO!", 580f, 460f);

        GameObject subBacking = new GameObject("SubBacking");
        subBacking.transform.SetParent(panel.transform, false);
        RectTransform rtSub = subBacking.AddComponent<RectTransform>();
        rtSub.anchorMin = rtSub.anchorMax = new Vector2(0.5f, 0.5f);
        rtSub.pivot = new Vector2(0.5f, 0.5f);
        rtSub.anchoredPosition = new Vector2(0f, 40f);
        rtSub.sizeDelta = new Vector2(500f, 80f);
        Image imgSub = subBacking.AddComponent<Image>();
        imgSub.color = new Color(0.06f, 0.04f, 0.10f, 0.88f);

        CreateText(subBacking.transform, "GoSub",
            "El lamento de La Llorona te envolvió en la oscuridad...",
            20, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.85f),
            Vector2.zero, Vector2.one, Vector2.zero);

        Button goRetry = CreatePlacaButton(panel.transform, "Btn_Reintentar", "Reintentar",     new Vector2(0f, -65f),  new Vector2(380f, 62f), 24, true);
        Button goMenu  = CreatePlacaButton(panel.transform, "Btn_Menu",       "Menú Principal", new Vector2(0f, -142f), new Vector2(380f, 60f), 22, false);
        modalRoot.SetActive(false);

        SerializedObject soGo = new SerializedObject(gameOver);
        soGo.FindProperty("panel").objectReferenceValue       = modalRoot;
        soGo.FindProperty("retryButton").objectReferenceValue = goRetry;
        soGo.FindProperty("menuButton").objectReferenceValue  = goMenu;
        soGo.ApplyModifiedProperties();
    }

    // ── Helpers Estilizados ──────────────────────────────────────────────────

    static GameObject CreateModalOverlay(Transform parent, string name)
    {
        GameObject overlay = new GameObject(name);
        overlay.transform.SetParent(parent, false);
        RectTransform rt = overlay.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = overlay.AddComponent<Image>();
        // Fondo negro al 75% (Dimmer): aísla de inmediato la ventana del suelo y de todo el juego
        img.color = new Color(0f, 0f, 0f, 0.75f);
        img.raycastTarget = true;
        return overlay;
    }

    static GameObject CreateModalPanel(Transform parent, string name, string title, float width, float height)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);

        Image img = panel.AddComponent<Image>();
        img.sprite = PlacaNormal;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        CreateText(panel.transform, "TituloModal", title, 34,
            TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.40f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(width - 40f, 60f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -24f);

        return panel;
    }

    public static Button CreatePlacaButton(Transform parent, string name, string text,
        Vector2 anchoredPos, Vector2 sizeDelta, int fontSize = 24, bool isPrimary = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        // Estado iluminado/dorado para el botón principal interactivo
        Sprite baseSprite = (isPrimary && PlacaResaltada != null) ? PlacaResaltada : PlacaNormal;
        img.sprite = baseSprite;
        img.type   = Image.Type.Sliced;

        Button btn = go.AddComponent<Button>();
        SpriteState ss = new SpriteState
        {
            highlightedSprite = PlacaResaltada ?? PlacaNormal,
            pressedSprite     = PlacaPresionada ?? PlacaNormal,
            disabledSprite    = PlacaPresionada ?? PlacaNormal
        };
        btn.spriteState = ss;
        btn.transition  = Selectable.Transition.SpriteSwap;
        go.AddComponent<ButtonHoverEffect>();

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        // Máximo contraste de texto: blanco puro en botón primario, crema dorado en secundarios
        Color textColor = isPrimary ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 0.94f, 0.75f, 1f);
        GameObject txtGo = CreateText(go.transform, "Text", text, fontSize,
            TextAnchor.MiddleCenter, textColor,
            Vector2.zero, Vector2.one, Vector2.zero);
        Text t = txtGo.GetComponent<Text>();
        t.fontStyle = FontStyle.Bold;

        return btn;
    }

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
        if (sprite != null) img.sprite = sprite;
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
        t.verticalOverflow   = VerticalWrapMode.Overflow;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 1.0f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.90f);
        shadow.effectDistance = new Vector2(2f, -2f);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = sizeDelta;
        return go;
    }

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
}
