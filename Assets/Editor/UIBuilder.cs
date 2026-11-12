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
    const string MenuFondoPath   = "Assets/Sprites/UI/menu_fondo.png";
    const string MarcoHudPath    = "Assets/Sprites/UI/marco_hud.png";
    const string BannersMenuPath = "Assets/Sprites/UI/banners_menu.png";
    const string IconosHudPath   = "Assets/Sprites/UI/iconos_hud.png";
    const string HudPlacaOrnadaPath     = "Assets/Sprites/UI/HUD_Placa_Ornada.png";
    const string HudPlacaRectaPath      = "Assets/Sprites/UI/HUD_Placa_Recta.png";
    const string HudMedallonPath        = "Assets/Sprites/UI/HUD_Medallon.png";
    const string HudCorazonLlenoPath    = "Assets/Sprites/UI/HUD_Corazon_Lleno.png";
    const string HudCorazonVacioPath    = "Assets/Sprites/UI/HUD_Corazon_Vacio.png";
    const string HudVelaPath            = "Assets/Sprites/UI/HUD_Vela.png";
    const string HudOfrendaPath         = "Assets/Sprites/UI/HUD_Ofrenda.png";
    const string HudBrujulaPath         = "Assets/Sprites/UI/HUD_Brujula.png";
    const string HudOjoPath             = "Assets/Sprites/UI/HUD_Ojo.png";
    const string HudBarraEstaminaPath   = "Assets/Sprites/UI/HUD_Barra_Estamina.png";
    const string HudCruzPath            = "Assets/Sprites/UI/HUD_Cruz.png";
    const string HudAguaBenditaPath     = "Assets/Sprites/UI/HUD_AguaBendita.png";
    const string HudCalaveraPath        = "Assets/Sprites/UI/HUD_Calavera.png";

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
    public static Sprite MenuFondoSprite   => AssetDatabase.LoadAssetAtPath<Sprite>(MenuFondoPath);
    public static Sprite MarcoHudSprite    => AssetDatabase.LoadAssetAtPath<Sprite>(MarcoHudPath);
    public static Sprite BannersMenuSprite => AssetDatabase.LoadAssetAtPath<Sprite>(BannersMenuPath);
    public static Sprite IconosHudSprite   => AssetDatabase.LoadAssetAtPath<Sprite>(IconosHudPath);
    public static Sprite HudPlacaOrnada     => AssetDatabase.LoadAssetAtPath<Sprite>(HudPlacaOrnadaPath);
    public static Sprite HudPlacaRecta      => AssetDatabase.LoadAssetAtPath<Sprite>(HudPlacaRectaPath);
    public static Sprite HudMedallon        => AssetDatabase.LoadAssetAtPath<Sprite>(HudMedallonPath);
    public static Sprite HudCorazonLleno    => AssetDatabase.LoadAssetAtPath<Sprite>(HudCorazonLlenoPath);
    public static Sprite HudCorazonVacio    => AssetDatabase.LoadAssetAtPath<Sprite>(HudCorazonVacioPath);
    public static Sprite HudVela            => AssetDatabase.LoadAssetAtPath<Sprite>(HudVelaPath);
    public static Sprite HudOfrenda         => AssetDatabase.LoadAssetAtPath<Sprite>(HudOfrendaPath);
    public static Sprite HudBrujula         => AssetDatabase.LoadAssetAtPath<Sprite>(HudBrujulaPath);
    public static Sprite HudOjo             => AssetDatabase.LoadAssetAtPath<Sprite>(HudOjoPath);
    public static Sprite HudBarraEstamina   => AssetDatabase.LoadAssetAtPath<Sprite>(HudBarraEstaminaPath);
    public static Sprite HudCruz            => AssetDatabase.LoadAssetAtPath<Sprite>(HudCruzPath);
    public static Sprite HudAguaBendita     => AssetDatabase.LoadAssetAtPath<Sprite>(HudAguaBenditaPath);
    public static Sprite HudCalavera        => AssetDatabase.LoadAssetAtPath<Sprite>(HudCalaveraPath);

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

    [MenuItem("La Llorona/Generar HUD en Todos los Niveles")]
    public static void BuildAllLevelsHUD()
    {
        BuildLevel1UICanvas();
        BuildLevel2UICanvas();
        BuildLevel3UICanvas();
        BuildLevel4UICanvas();
        Debug.Log("[UIBuilder] \u00a1HUD HD Colonial generado con \u00e9xito en todos los niveles (1, 2, 3 y 4)!");
    }

    [MenuItem("La Llorona/Actualizar HUD Escena Actual")]
    public static void RebuildCurrentSceneHUD()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        string objetivo = "Cruza el callej\u00f3n sin que La Llorona te vea";
        if (activeScene.name.Contains("Mercado") || activeScene.name.Contains("Level1"))
            objetivo = "Cruza el mercado sin que La Llorona te vea";
        else if (activeScene.name.Contains("Callejon") || activeScene.name.Contains("Level2"))
            objetivo = "Llega al final del callej\u00f3n esquivando los obst\u00e1culos";
        else if (activeScene.name.Contains("Atrio") || activeScene.name.Contains("Level3"))
            objetivo = "Consagra las 4 ofrendas y llega a la Parroquia";
        else if (activeScene.name.Contains("Cementerio") || activeScene.name.Contains("Level4"))
            objetivo = "\u00a1Escapa hacia tu casa! Cuidado con las \u00e1nimas y el perro";

        BuildLevelUICanvas(activeScene.path, objetivo);
        Debug.Log($"[UIBuilder] \u00a1HUD Colonial HD actualizado con \u00e9xito en {activeScene.name}!");
    }

    public static void BuildSplashScene()
    {
        string path = "Assets/Scenes/Splash.unity";
        Scene scene = File.Exists(path)
            ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Limpieza de objetos raíz para evitar duplicación
        foreach (var root in scene.GetRootGameObjects())
        {
            Undo.DestroyObjectImmediate(root);
        }

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

        // Limpieza total de GameObjects raíz para eliminar duplicados viejos y paneles blancos obsoletos
        foreach (var root in scene.GetRootGameObjects())
        {
            Undo.DestroyObjectImmediate(root);
        }

        CreateCamera();
        EnsureEventSystem();
        if (Camera.main != null) Camera.main.backgroundColor = new Color(0.04f, 0.02f, 0.07f);

        GameObject canvasGo = CreateCanvas("MainMenuCanvas");

        // Fondo ilustrado oficial (logo.png) que muestra el callejón colonial y el título brillante "La Llorona del Callejón"
        Sprite bgSprite = LogoSprite ?? MenuFondoSprite;
        GameObject bgGo = CreateImageSprite(canvasGo.transform, "FondoMenuIlustrado", bgSprite,
            Vector2.zero, Vector2.one);
        Image bgImg = bgGo.GetComponent<Image>();
        if (bgImg != null)
        {
            bgImg.preserveAspect = false;
            bgImg.color = Color.white;
        }

        // Sombra suave en la base de adoquines para asentar los botones sin tapar el título central iluminado
        GameObject bottomShadow = CreateImage(canvasGo.transform, "SombraInferior",
            new Color(0.02f, 0.01f, 0.04f, 0.55f),
            new Vector2(0f, 0f), new Vector2(1f, 0.35f));

        // Contenedor de botones centrado abajo en los adoquines (área despejada del título)
        GameObject btnContainer = new GameObject("BotonesMenu");
        btnContainer.transform.SetParent(canvasGo.transform, false);
        RectTransform rtBtns = btnContainer.AddComponent<RectTransform>();
        rtBtns.anchorMin = rtBtns.anchorMax = new Vector2(0.5f, 0f);
        rtBtns.pivot = new Vector2(0.5f, 0f);
        rtBtns.anchoredPosition = new Vector2(0f, 32f);
        rtBtns.sizeDelta = new Vector2(400f, 260f);

        // 5 botones ordenados y estilizados con banners góticos
        Button btnJugar = CreatePlacaButton(btnContainer.transform, "Btn_Jugar", "JUGAR",
            new Vector2(0f, 208f), new Vector2(380f, 48f), 22, true);

        Button btnSelect = CreatePlacaButton(btnContainer.transform, "Btn_Niveles", "SELECCIONAR NIVEL",
            new Vector2(0f, 156f), new Vector2(380f, 48f), 20);

        Button btnOpciones = CreatePlacaButton(btnContainer.transform, "Btn_Opciones", "OPCIONES",
            new Vector2(0f, 104f), new Vector2(380f, 48f), 20);

        Button btnCredits = CreatePlacaButton(btnContainer.transform, "Btn_Creditos", "CRÉDITOS",
            new Vector2(0f, 52f), new Vector2(380f, 48f), 20);

        Button btnSalir = CreatePlacaButton(btnContainer.transform, "Btn_Salir", "SALIR",
            new Vector2(0f, 0f), new Vector2(380f, 48f), 20);

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

        // Limpieza de objetos raíz para evitar duplicación
        foreach (var root in scene.GetRootGameObjects())
        {
            Undo.DestroyObjectImmediate(root);
        }

        CreateCamera();
        EnsureEventSystem();
        if (Camera.main != null) Camera.main.backgroundColor = new Color(0.04f, 0.02f, 0.08f);

        GameObject canvasGo = CreateCanvas("LevelSelectCanvas");

        // Fondo ilustrado con tinte nocturno
        if (MenuFondoSprite != null)
        {
            GameObject bgGo = CreateImageSprite(canvasGo.transform, "FondoIlustrado", MenuFondoSprite,
                Vector2.zero, Vector2.one);
            Image bgImg = bgGo.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.preserveAspect = false;
                bgImg.color = new Color(0.35f, 0.32f, 0.42f, 1f);
            }
        }
        else
        {
            CreateImage(canvasGo.transform, "Fondo", new Color(0.05f, 0.03f, 0.08f, 1f), Vector2.zero, Vector2.one);
        }

        // Título superior SIEMPRE visible
        GameObject titleCard = new GameObject("TitleCard");
        titleCard.transform.SetParent(canvasGo.transform, false);
        RectTransform titleRt = titleCard.AddComponent<RectTransform>();
        titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -30f);
        titleRt.sizeDelta = new Vector2(700f, 70f);

        CreateText(titleCard.transform, "Titulo", "SELECCIONAR NIVEL",
            38, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.40f),
            Vector2.zero, Vector2.one, Vector2.zero);

        // Cuadrícula 2x2 para las 4 tarjetas de nivel
        string[] nombres = { "Nivel 1\nEl Mercado", "Nivel 2\nEl Callejón", "Nivel 3\nEl Atrio", "Nivel 4\nEl Panteón" };
        string[] escenas = { "Level1_Mercado", "Level2_Callejon", "Level3_Atrio", "Level4_Cementerio" };
        Vector2[] pos = {
            new Vector2(-290f,  75f), new Vector2( 290f,  75f),
            new Vector2(-290f, -145f), new Vector2( 290f, -145f)
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
            cardRt.sizeDelta = new Vector2(540f, 185f);

            // Fondo de la tarjeta con PlacaNormal bien proporcionada
            Image cardBg = cardGo.AddComponent<Image>();
            cardBg.sprite = PlacaNormal;
            cardBg.type = Image.Type.Sliced;
            cardBg.color = new Color(0.14f, 0.10f, 0.18f, 0.96f);

            // Relleno interior oscuro
            GameObject cardIn = new GameObject("InnerDark");
            cardIn.transform.SetParent(cardGo.transform, false);
            RectTransform cardInRt = cardIn.AddComponent<RectTransform>();
            cardInRt.anchorMin = Vector2.zero;
            cardInRt.anchorMax = Vector2.one;
            cardInRt.offsetMin = new Vector2(8f, 8f);
            cardInRt.offsetMax = new Vector2(-8f, -8f);
            Image cardInImg = cardIn.AddComponent<Image>();
            cardInImg.color = new Color(0.06f, 0.04f, 0.09f, 0.92f);

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

            // Miniatura del nivel
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
            thumbRt.sizeDelta = new Vector2(175f, 135f);

            // Texto con el nombre del nivel
            CreateText(cardGo.transform, "NombreNivel", nombres[i], 24,
                TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.80f),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(300f, 80f))
                .GetComponent<RectTransform>().anchoredPosition = new Vector2(220f, 10f);

            // Candado / Bloqueado
            GameObject lockGo = new GameObject("BloqueadoTag");
            lockGo.transform.SetParent(cardGo.transform, false);
            RectTransform lockRt = lockGo.AddComponent<RectTransform>();
            lockRt.anchorMin = lockRt.anchorMax = new Vector2(0.5f, 0.5f);
            lockRt.anchoredPosition = Vector2.zero;
            lockRt.sizeDelta = new Vector2(520f, 165f);

            CreateImage(lockGo.transform, "SombraBloqueo",
                new Color(0.04f, 0.04f, 0.06f, 0.82f), Vector2.zero, Vector2.one);

            CreateText(lockGo.transform, "LockText", "🔒 BLOQUEADO", 24,
                TextAnchor.MiddleCenter, new Color(0.95f, 0.35f, 0.35f),
                Vector2.zero, Vector2.one, Vector2.zero);

            lockIcons[i] = lockGo;
            lockGo.SetActive(false);
        }

        // Botón Volver SIEMPRE visible en la parte inferior
        Button btnBack = CreatePlacaButton(canvasGo.transform, "Btn_Volver", "← VOLVER AL MENÚ",
            new Vector2(0f, 36f), new Vector2(360f, 56f), 22, true);
        RectTransform rtBack = btnBack.GetComponent<RectTransform>();
        rtBack.anchorMin = rtBack.anchorMax = new Vector2(0.5f, 0f);
        rtBack.pivot = new Vector2(0.5f, 0f);
        rtBack.anchoredPosition = new Vector2(0f, 36f);

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

        // Limpieza de objetos raíz para evitar duplicación
        foreach (var root in scene.GetRootGameObjects())
        {
            Undo.DestroyObjectImmediate(root);
        }

        CreateCamera();
        EnsureEventSystem();
        if (Camera.main != null) Camera.main.backgroundColor = new Color(0.04f, 0.02f, 0.08f);

        GameObject canvasGo = CreateCanvas("CreditsCanvas");
        if (MenuFondoSprite != null)
        {
            GameObject bgGo = CreateImageSprite(canvasGo.transform, "FondoIlustrado", MenuFondoSprite,
                Vector2.zero, Vector2.one);
            Image bgImg = bgGo.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.preserveAspect = false;
                bgImg.color = new Color(0.35f, 0.32f, 0.42f, 1f);
            }
        }
        else
        {
            CreateImage(canvasGo.transform, "Fondo", new Color(0.05f, 0.03f, 0.08f, 1f), Vector2.zero, Vector2.one);
        }

        CreateText(canvasGo.transform, "Titulo", "CRÉDITOS", 44,
            TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.40f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(600f, 70f))
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);

        // Tarjeta amplia y majestuosa (1040 × 580 px) para que el texto respire y no quede comprimido ni deforme
        GameObject cardGo = CreateImageSprite(canvasGo.transform, "CardCreditos", PlacaNormal,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1040f, 580f));
        Image cardImg = cardGo.GetComponent<Image>();
        cardImg.type = Image.Type.Sliced;
        cardImg.color = new Color(0.12f, 0.08f, 0.16f, 0.98f);
        RectTransform rtCard = cardGo.GetComponent<RectTransform>();
        rtCard.anchoredPosition = new Vector2(0f, 20f);

        // Fondo interior negro terciopelo al 96% de opacidad para máximo contraste
        GameObject cardInner = new GameObject("InnerDark");
        cardInner.transform.SetParent(cardGo.transform, false);
        RectTransform rtCi = cardInner.AddComponent<RectTransform>();
        rtCi.anchorMin = Vector2.zero;
        rtCi.anchorMax = Vector2.one;
        rtCi.offsetMin = new Vector2(14f, 14f);
        rtCi.offsetMax = new Vector2(-14f, -14f);
        Image ciImg = cardInner.AddComponent<Image>();
        ciImg.color = new Color(0.05f, 0.03f, 0.08f, 0.96f);

        string credText =
            "✦  LA LLORONA DEL CALLEJÓN  ✦\n" +
            "Proyecto de Desarrollo de Videojuegos 2D\n\n" +
            "DIRECCIÓN Y DISEÑO\n" +
            "Carol y Princes · UTNG\n\n" +
            "ARTE Y CONCEPTO VISUAL\n" +
            "Concepto Original y Sprites Oficiales del Proyecto\n\n" +
            "EFECTOS DE SONIDO Y AMBIENTE\n" +
            "Freesound (Licencia CC0) · UTNG\n\n" +
            "Agradecimiento especial a la comunidad y docentes de UTNG";

        GameObject txtGo = CreateText(cardInner.transform, "TextoCreditos", credText, 23,
            TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.85f),
            Vector2.zero, Vector2.one, new Vector2(-50f, -40f));
        Text txtComp = txtGo.GetComponent<Text>();
        txtComp.lineSpacing = 1.25f;

        // Botón Volver SIEMPRE visible y anclado a la base inferior
        Button btnBack = CreatePlacaButton(canvasGo.transform, "Btn_Volver", "← VOLVER AL MENÚ",
            new Vector2(0f, 36f), new Vector2(360f, 56f), 22, true);
        RectTransform rtBack = btnBack.GetComponent<RectTransform>();
        rtBack.anchorMin = rtBack.anchorMax = new Vector2(0.5f, 0f);
        rtBack.pivot = new Vector2(0.5f, 0f);
        rtBack.anchoredPosition = new Vector2(0f, 36f);

        // Enlazar componente CreditsUI para garantizar que el botón funcione SIEMPRE al hacer clic
        CreditsUI credUi = canvasGo.AddComponent<CreditsUI>();
        SerializedObject soCred = new SerializedObject(credUi);
        soCred.FindProperty("backButton").objectReferenceValue = btnBack;
        soCred.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        Debug.Log("[UIBuilder] Escena Credits generada.");
    }

    // ── 5. Canvas HUD de Niveles (Franja Superior 120px, Centro Libre) ────────

    public static void BuildLevel1UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level1_Mercado.unity", "Cruza el mercado sin que La Llorona te vea");
    public static void BuildLevel2UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level2_Callejon.unity", "Llega al final del callej\u00f3n esquivando los obst\u00e1culos");
    public static void BuildLevel3UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level3_Atrio.unity", "Consagra las 4 ofrendas y llega a la Parroquia", false);
    public static void BuildLevel4UICanvas() => BuildLevelUICanvas("Assets/Scenes/Level4_Cementerio.unity", "¡Purifica a La Llorona arrojándole Agua Bendita!");

    public static void BuildLevelUICanvas(string path, string objetivoTexto = "", bool minimalHud = false)
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
        // BARRA SUPERIOR UNIFICADA DECORATIVA GÓTICA (Cubre la franja superior de 80px)
        // ════════════════════════════════════════════════════════════════════════
        bool isAtrio = path.Contains("Atrio") || path.Contains("Level3");
        bool isCementerio = path.Contains("Cementerio") || path.Contains("Level4");
        bool usesInventory = isAtrio || isCementerio;
        float plaqueWidth = usesInventory ? 500f : 480f;
        float plaqueHeight = usesInventory ? 160f : 124f;

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // 1. PLACA IZQUIERDA: ESTADO (Corazones, Estamina e Inventario Velas/Agua)
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        GameObject leftPlaque = new GameObject("HUD_Placa_Izquierda");
        leftPlaque.transform.SetParent(hudGo.transform, false);
        RectTransform rtLeft = leftPlaque.AddComponent<RectTransform>();
        rtLeft.anchorMin = rtLeft.anchorMax = new Vector2(0f, 1f);
        rtLeft.pivot = new Vector2(0f, 1f);
        rtLeft.anchoredPosition = new Vector2(24f, -16f);
        rtLeft.sizeDelta = new Vector2(plaqueWidth, plaqueHeight);

        Image leftBg = leftPlaque.AddComponent<Image>();
        leftBg.sprite = HudPlacaRecta ?? PlacaNormal;
        leftBg.type = Image.Type.Sliced;
        leftBg.color = Color.white;

        // Fila 1: Corazones (3x de 44 Ã— 44 px, espaciado generoso de 48 px)
        float heartY = usesInventory ? 36f : 12f;
        GameObject heartsRoot = new GameObject("Corazones_Vida");
        heartsRoot.transform.SetParent(leftPlaque.transform, false);
        RectTransform rtH = heartsRoot.AddComponent<RectTransform>();
        rtH.anchorMin = rtH.anchorMax = new Vector2(0f, 0.5f);
        rtH.pivot = new Vector2(0f, 0.5f);
        rtH.anchoredPosition = new Vector2(46f, heartY);
        rtH.sizeDelta = new Vector2(160f, 44f);

        Image[] heartImgs = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject h = new GameObject($"Corazon_{i + 1}");
            h.transform.SetParent(heartsRoot.transform, false);
            Image hImg = h.AddComponent<Image>();
            hImg.sprite = HudCorazonLleno ?? CorazonLleno;
            hImg.preserveAspect = true;
            RectTransform hRt = h.GetComponent<RectTransform>();
            hRt.anchorMin = hRt.anchorMax = new Vector2(0f, 0.5f);
            hRt.pivot = new Vector2(0f, 0.5f);
            hRt.sizeDelta = new Vector2(44f, 44f);
            hRt.anchoredPosition = new Vector2(i * 48f, 0f);
            heartImgs[i] = hImg;
        }

        // Fila 2: Barra de Estamina Amplia (ancho 280-290 px, altura 20 px)
        float stamY = usesInventory ? -4f : -18f;
        GameObject staminaRoot = new GameObject("Estamina_Barra");
        staminaRoot.transform.SetParent(leftPlaque.transform, false);
        RectTransform rtStam = staminaRoot.AddComponent<RectTransform>();
        rtStam.anchorMin = rtStam.anchorMax = new Vector2(0f, 0.5f);
        rtStam.pivot = new Vector2(0f, 0.5f);
        rtStam.anchoredPosition = new Vector2(46f, stamY);
        rtStam.sizeDelta = new Vector2(plaqueWidth - 92f, 26f);

        Text stamLabel = CreateText(staminaRoot.transform, "StaminaLabel", "ESTAMINA", 15,
            TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.50f),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(90f, 22f))
            .GetComponent<Text>();
        stamLabel.fontStyle = FontStyle.Bold;
        stamLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

        GameObject stamBarGo = new GameObject("MarcoBarra");
        stamBarGo.transform.SetParent(staminaRoot.transform, false);
        RectTransform barRt = stamBarGo.AddComponent<RectTransform>();
        barRt.anchorMin = barRt.anchorMax = new Vector2(0f, 0.5f);
        barRt.pivot = new Vector2(0f, 0.5f);
        barRt.anchoredPosition = new Vector2(96f, 0f);
        barRt.sizeDelta = new Vector2(usesInventory ? 290f : 280f, 20f);

        Image stamBg = stamBarGo.AddComponent<Image>();
        stamBg.sprite = BarraMarco ?? PlacaNormal;
        stamBg.type = Image.Type.Sliced;
        CanvasGroup stamCg = staminaRoot.AddComponent<CanvasGroup>();

        GameObject stamFillGo = new GameObject("Fill");
        stamFillGo.transform.SetParent(stamBarGo.transform, false);
        Image stamFill = stamFillGo.AddComponent<Image>();
        stamFill.sprite = null;
        stamFill.type = Image.Type.Filled;
        stamFill.fillMethod = Image.FillMethod.Horizontal;
        stamFill.fillOrigin = 0;
        stamFill.fillAmount = 1f;
        stamFill.color = new Color(0.98f, 0.82f, 0.20f);
        RectTransform fillRt = stamFillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0.03f, 0.15f);
        fillRt.anchorMax = new Vector2(0.97f, 0.85f);
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

        // Fila 3: Inventario de Velas / Agua Bendita (SOLO en niveles 3 y 4)
        GameObject candleRoot = new GameObject("Velas_Inventario");
        candleRoot.transform.SetParent(leftPlaque.transform, false);
        RectTransform rtC = candleRoot.AddComponent<RectTransform>();
        rtC.anchorMin = rtC.anchorMax = new Vector2(0f, 0.5f);
        rtC.pivot = new Vector2(0f, 0.5f);
        rtC.anchoredPosition = new Vector2(46f, -44f);
        rtC.sizeDelta = new Vector2(plaqueWidth - 92f, 32f);

        GameObject velaIconGo = new GameObject("IconoInventario");
        velaIconGo.transform.SetParent(candleRoot.transform, false);
        Image velaIcon = velaIconGo.AddComponent<Image>();
        velaIcon.sprite = isCementerio ? (HudAguaBendita ?? HudOfrenda ?? HudVela) : (HudOfrenda ?? HudVela ?? IconoVela);
        velaIcon.preserveAspect = true;
        RectTransform velaRt = velaIconGo.GetComponent<RectTransform>();
        velaRt.anchorMin = velaRt.anchorMax = new Vector2(0f, 0.5f);
        velaRt.pivot = new Vector2(0f, 0.5f);
        velaRt.sizeDelta = new Vector2(34f, 34f);
        velaRt.anchoredPosition = new Vector2(0f, 0f);

        string defaultCandleText = isAtrio 
            ? "Nichos: 0/4   |   Velas: 0"
            : (isCementerio ? "Agua Bendita: 5/5" : "");

        Text candleTxt = CreateText(candleRoot.transform, "CandleCountText", defaultCandleText, 15,
            TextAnchor.MiddleLeft, new Color(1f, 0.94f, 0.48f),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(plaqueWidth - 145f, 28f))
            .GetComponent<Text>();
        candleTxt.fontStyle = FontStyle.Bold;
        candleTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        candleTxt.verticalOverflow = VerticalWrapMode.Truncate;
        candleTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(42f, 0f);
        var candleOutline = candleTxt.gameObject.AddComponent<Outline>();
        candleOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        candleOutline.effectDistance = new Vector2(1.2f, -1.2f);

        if (!usesInventory)
        {
            candleRoot.SetActive(false);
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // 2. MEDALLÃ“N CENTRAL: TIEMPO Y PUNTOS (Anchor Top-Center, 180 Ã— 160 px)
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        GameObject centerMedallon = new GameObject("HUD_Medallon_Centro");
        centerMedallon.transform.SetParent(hudGo.transform, false);
        RectTransform rtCenter = centerMedallon.AddComponent<RectTransform>();
        rtCenter.anchorMin = rtCenter.anchorMax = new Vector2(0.5f, 1f);
        rtCenter.pivot = new Vector2(0.5f, 1f);
        rtCenter.anchoredPosition = new Vector2(0f, -14f);
        rtCenter.sizeDelta = new Vector2(180f, 160f);

        Image centerBg = centerMedallon.AddComponent<Image>();
        centerBg.sprite = HudMedallon ?? HudPlacaRecta;
        centerBg.preserveAspect = true;
        centerBg.color = Color.white;

        Text timerTxt = CreateText(centerMedallon.transform, "TimerText", "00:00", 20,
            TextAnchor.MiddleCenter, new Color(1f, 0.98f, 0.92f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(100f, 26f))
            .GetComponent<Text>();
        timerTxt.fontStyle = FontStyle.Bold;
        timerTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -10f);
        var timerOutline = timerTxt.gameObject.AddComponent<Outline>();
        timerOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        timerOutline.effectDistance = new Vector2(1.2f, -1.2f);

        Text scoreTxt = CreateText(centerMedallon.transform, "ScoreText", "0 PTS", 14,
            TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.32f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(100f, 20f))
            .GetComponent<Text>();
        scoreTxt.fontStyle = FontStyle.Bold;
        scoreTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -32f);
        var scoreOutline = scoreTxt.gameObject.AddComponent<Outline>();
        scoreOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        scoreOutline.effectDistance = new Vector2(1.2f, -1.2f);

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // 3. PLACA DERECHA: OBJETIVO, META Y OJO ROJO (Anchor Top-Right, 480-500 px)
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        GameObject rightPlaque = new GameObject("HUD_Placa_Derecha");
        rightPlaque.transform.SetParent(hudGo.transform, false);
        RectTransform rtRight = rightPlaque.AddComponent<RectTransform>();
        rtRight.anchorMin = rtRight.anchorMax = new Vector2(1f, 1f);
        rtRight.pivot = new Vector2(1f, 1f);
        rtRight.anchoredPosition = new Vector2(-24f, -16f);
        rtRight.sizeDelta = new Vector2(plaqueWidth, plaqueHeight);

        Image rightBg = rightPlaque.AddComponent<Image>();
        rightBg.sprite = HudPlacaRecta ?? PlacaNormal;
        rightBg.type = Image.Type.Sliced;
        rightBg.color = Color.white;

        // BrÃºjula en el lado izquierdo dentro de la placa (44 Ã— 44 px)
        GameObject compassGo = new GameObject("IconoBrujula");
        compassGo.transform.SetParent(rightPlaque.transform, false);
        Image compassImg = compassGo.AddComponent<Image>();
        compassImg.sprite = HudBrujula ?? IconoVela;
        compassImg.preserveAspect = true;
        RectTransform compRt = compassGo.GetComponent<RectTransform>();
        compRt.anchorMin = compRt.anchorMax = new Vector2(0f, 0.5f);
        compRt.pivot = new Vector2(0f, 0.5f);
        compRt.sizeDelta = new Vector2(44f, 44f);
        compRt.anchoredPosition = new Vector2(46f, 0f);

        // Ojo de alerta al lado de la brÃºjula (44 Ã— 36 px)
        GameObject eyeGo = new GameObject("IconoOjo_Alerta");
        eyeGo.transform.SetParent(rightPlaque.transform, false);
        Image eyeImg = eyeGo.AddComponent<Image>();
        eyeImg.sprite = HudOjo ?? IconoOjo;
        eyeImg.preserveAspect = true;
        RectTransform eyeRt = eyeGo.GetComponent<RectTransform>();
        eyeRt.anchorMin = eyeRt.anchorMax = new Vector2(0f, 0.5f);
        eyeRt.pivot = new Vector2(0f, 0.5f);
        eyeRt.sizeDelta = new Vector2(44f, 36f);
        eyeRt.anchoredPosition = new Vector2(98f, 0f);

        // Textos dentro de la placa derecha (X = 152f, ancho generoso de 282-302 px)
        float txtObjY = usesInventory ? 18f : 8f;
        float txtMetaY = usesInventory ? -24f : -18f;
        float rightTextW = plaqueWidth - 198f;

        Text objTxt = CreateText(rightPlaque.transform, "ObjectiveText",
            objetivoTexto, 15, TextAnchor.MiddleLeft,
            new Color(0.36f, 0.98f, 0.90f),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(rightTextW, usesInventory ? 48f : 34f))
            .GetComponent<Text>();
        objTxt.fontStyle = FontStyle.Bold;
        objTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        objTxt.verticalOverflow = VerticalWrapMode.Truncate;
        objTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(152f, txtObjY);
        var objOutline = objTxt.gameObject.AddComponent<Outline>();
        objOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        objOutline.effectDistance = new Vector2(1.2f, -1.2f);

        Text distTxt = CreateText(rightPlaque.transform, "MetaText", "SALIDA: --m", 16,
            TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.40f),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(rightTextW, 26f))
            .GetComponent<Text>();
        distTxt.fontStyle = FontStyle.Bold;
        distTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(152f, txtMetaY);
        var distOutline = distTxt.gameObject.AddComponent<Outline>();
        distOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        distOutline.effectDistance = new Vector2(1.2f, -1.2f);

        GoalCompassUI compass = rightPlaque.AddComponent<GoalCompassUI>();
        SerializedObject soComp = new SerializedObject(compass);
        soComp.FindProperty("arrowRect").objectReferenceValue = compRt;
        soComp.FindProperty("distanceText").objectReferenceValue = distTxt;
        soComp.ApplyModifiedProperties();
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // D. AVISOS DE EVENTO (A 135 px del borde superior, centro libre)
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        GameObject evtNoticeGo = new GameObject("AvisoEvento_Top140");
        evtNoticeGo.transform.SetParent(hudGo.transform, false);
        RectTransform evtRt = evtNoticeGo.AddComponent<RectTransform>();
        evtRt.anchorMin = evtRt.anchorMax = new Vector2(0.5f, 1f);
        evtRt.pivot = new Vector2(0.5f, 1f);
        evtRt.anchoredPosition = new Vector2(0f, -95f);
        evtRt.sizeDelta = new Vector2(980f, 72f);

        Image evtBg = evtNoticeGo.AddComponent<Image>();
        evtBg.sprite = BannersMenuSprite ?? PlacaNormal;
        evtBg.type = Image.Type.Sliced;
        evtBg.color = new Color(0.06f, 0.04f, 0.10f, 0.95f);
        CanvasGroup evtCg = evtNoticeGo.AddComponent<CanvasGroup>();

        Text evtTxt = CreateText(evtNoticeGo.transform, "AvisoTexto", "", 24,
            TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.65f),
            Vector2.zero, Vector2.one, Vector2.zero)
            .GetComponent<Text>();
        evtTxt.fontStyle = FontStyle.Bold;
        evtTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        evtTxt.verticalOverflow = VerticalWrapMode.Overflow;
        evtTxt.lineSpacing = 1.15f;
        var evtOutline = evtTxt.gameObject.AddComponent<Outline>();
        evtOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        evtOutline.effectDistance = new Vector2(1.5f, -1.5f);
        evtNoticeGo.SetActive(false);

        // ════════════════════════════════════════════════════════════════════════
        // E. TUTORIAL FLOTANTE (A 55 px del borde inferior)
        // ════════════════════════════════════════════════════════════════════════
        GameObject tutGo = new GameObject("Tutorial_Bottom60");
        tutGo.transform.SetParent(hudGo.transform, false);
        RectTransform tutRt = tutGo.AddComponent<RectTransform>();
        tutRt.anchorMin = tutRt.anchorMax = new Vector2(0.5f, 0f);
        tutRt.pivot = new Vector2(0.5f, 0f);
        tutRt.anchoredPosition = new Vector2(0f, 55f);
        tutRt.sizeDelta = new Vector2(1040f, 96f);

        Image tutBg = tutGo.AddComponent<Image>();
        tutBg.sprite = PlacaNormal;
        tutBg.type = Image.Type.Sliced;
        tutBg.color = new Color(0.06f, 0.04f, 0.10f, 0.95f);
        CanvasGroup tutCg = tutGo.AddComponent<CanvasGroup>();

        Text tutTxt = CreateText(tutGo.transform, "TutorialText",
            "WASD moverte · Shift correr · [E] esconderse · [F] vela", 25,
            TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.82f),
            Vector2.zero, Vector2.one, Vector2.zero)
            .GetComponent<Text>();
        tutTxt.fontStyle = FontStyle.Bold;
        tutTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        tutTxt.verticalOverflow = VerticalWrapMode.Overflow;
        tutTxt.lineSpacing = 1.15f;
        var tutOutline = tutTxt.gameObject.AddComponent<Outline>();
        tutOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        tutOutline.effectDistance = new Vector2(1.5f, -1.5f);
        tutGo.SetActive(false);

        if (minimalHud)
        {
            // Reduce únicamente tarjetas accesorias si se pide HUD ultra-compacto
            scoreTxt.gameObject.SetActive(false);
            rightPlaque.SetActive(false);
            eyeGo.SetActive(false);
        }

        // Enlazar referencias de HUD.cs
        SerializedObject soHud = new SerializedObject(hud);
        SerializedProperty hpArray = soHud.FindProperty("heartImages");
        hpArray.arraySize = 3;
        for (int i = 0; i < 3; i++) hpArray.GetArrayElementAtIndex(i).objectReferenceValue = heartImgs[i];
        soHud.FindProperty("activeHeartSprite").objectReferenceValue   = HudCorazonLleno ?? CorazonLleno;
        soHud.FindProperty("lostHeartSprite").objectReferenceValue     = HudCorazonVacio ?? CorazonVacio;
        soHud.FindProperty("activeHeartColor").colorValue              = Color.white;
        soHud.FindProperty("lostHeartColor").colorValue                = Color.white;
        soHud.FindProperty("timerText").objectReferenceValue           = timerTxt;
        soHud.FindProperty("scoreText").objectReferenceValue           = scoreTxt;
        soHud.FindProperty("objectiveText").objectReferenceValue       = objTxt;
        soHud.FindProperty("showCandleInventory").boolValue            = usesInventory;
        soHud.FindProperty("candleInventoryRoot").objectReferenceValue = candleRoot;
        soHud.FindProperty("candleCountText").objectReferenceValue     = candleTxt;
        soHud.FindProperty("staminaFillImage").objectReferenceValue    = stamFill;
        soHud.FindProperty("staminaCanvasGroup").objectReferenceValue  = stamCg;
        soHud.FindProperty("eyeIconImage").objectReferenceValue        = eyeImg;
        soHud.FindProperty("eyeNormalSprite").objectReferenceValue     = HudOjo ?? IconoOjo;
        soHud.FindProperty("eyeAlertSprite").objectReferenceValue      = HudOjo ?? IconoOjo;
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
        // Fondo carbón oscuro al 90% (Dimmer): aísla por completo y previene que se trasluzcan textos o luces del mundo
        img.color = new Color(0.01f, 0.01f, 0.03f, 0.90f);
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

        // Marco gótico noble con proporciones limpias sin deformaciones
        Image img = panel.AddComponent<Image>();
        img.sprite = PlacaNormal;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.14f, 0.10f, 0.18f, 0.98f);

        // Relleno interior 100% opaco para asegurar cero sangrado visual del juego
        GameObject innerBack = new GameObject("FondoOpacoModal");
        innerBack.transform.SetParent(panel.transform, false);
        RectTransform inRt = innerBack.AddComponent<RectTransform>();
        inRt.anchorMin = Vector2.zero;
        inRt.anchorMax = Vector2.one;
        inRt.offsetMin = new Vector2(10f, 10f);
        inRt.offsetMax = new Vector2(-10f, -10f);
        Image inImg = innerBack.AddComponent<Image>();
        inImg.color = new Color(0.06f, 0.04f, 0.09f, 0.98f);

        CreateText(panel.transform, "TituloModal", title, 32,
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
        // Usar PlacaResaltada para botones primarios dorados y PlacaNormal para secundarios nobles
        Sprite baseSprite = (isPrimary && PlacaResaltada != null) ? PlacaResaltada : PlacaNormal;
        img.sprite = baseSprite;
        img.type   = Image.Type.Sliced;
        img.color  = isPrimary ? new Color(1f, 0.96f, 0.86f, 1f) : new Color(0.95f, 0.95f, 0.95f, 1f);

        Button btn = go.AddComponent<Button>();
        SpriteState ss = new SpriteState
        {
            highlightedSprite = (PlacaResaltada != null) ? PlacaResaltada : baseSprite,
            pressedSprite     = (PlacaPresionada != null) ? PlacaPresionada : baseSprite,
            disabledSprite    = (PlacaPresionada != null) ? PlacaPresionada : baseSprite
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
        cs.dynamicPixelsPerUnit = 2.0f; // Renderizado HD / Retina de fuentes para nitidez máxima sin bordes borrosos

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
        Font customFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
        t.font      = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Truncate;
        t.raycastTarget      = false;

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.90f);
        shadow.effectDistance = new Vector2(1.2f, -1.2f);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;

        float px = 0.5f;
        if (alignment == TextAnchor.UpperLeft || alignment == TextAnchor.MiddleLeft || alignment == TextAnchor.LowerLeft) px = 0f;
        else if (alignment == TextAnchor.UpperRight || alignment == TextAnchor.MiddleRight || alignment == TextAnchor.LowerRight) px = 1f;

        float py = 0.5f;
        if (alignment == TextAnchor.UpperLeft || alignment == TextAnchor.UpperCenter || alignment == TextAnchor.UpperRight) py = 1f;
        else if (alignment == TextAnchor.LowerLeft || alignment == TextAnchor.LowerCenter || alignment == TextAnchor.LowerRight) py = 0f;

        rt.pivot = new Vector2(px, py);
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
