// Assets/Editor/Level3Builder.cs
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Constructor oficial y automatizado del Nivel 3: El Atrio de la Parroquia de Dolores.
/// Reconstrucción monumental (Opción A):
/// - Escenario amplio y expandido de 3 secciones (Calzada de Entrada, Gran Plaza de la Fuente, Explanada Superior y Parroquia).
/// - Fachada monumental de la Parroquia con sus escaleras de cantera nativas.
/// - Mecánica ritual de "Las Puertas Selladas": Carlos debe encontrar veladoras y consagrar 3 nichos sagrados (Altar, Fuente y Cripta) para abrir el santuario.
/// - Cada nicho consagrado crea una zona segura divina que repele a La Llorona, pero incrementa su agresión.
/// - Al encender las 3 ofrendas, las campanas repican, las puertas se abren y Carlos sube las escaleras hasta la puerta para terminar el nivel.
/// - HUD limpio con contador de Ofrendas (X/3) y Velas.
/// </summary>
public static class Level3Builder
{
    const string ScenePath = "Assets/Scenes/Level3_Atrio.unity";

    [MenuItem("La Llorona/Construir Nivel 3")]
    public static void BuildLevel3Scene()
    {
        Debug.Log("[Level3Builder] Iniciando reconstrucción monumental del Nivel 3 (El Atrio de Dolores - Opción A)...");

        // 1. Asegurar slicing de sprites del Nivel 3
        SpriteSheetSlicer.SliceNivel3();
        AssetDatabase.Refresh();

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 3 Monumental Oficial");

        int wallsLayer = LayerMask.NameToLayer("Walls");
        int hideSpotLayer = LayerMask.NameToLayer("HideSpot");
        int playerLayer = LayerMask.NameToLayer("Player");
        int lloronaLayer = LayerMask.NameToLayer("Llorona");

        // Cargar sprites oficiales del Nivel 3
        Sprite catedralSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/catedral.png")
            .OfType<Sprite>().FirstOrDefault();

        Sprite[] accSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/accesorios.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] fuenteSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/fuente.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] rejaSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/rejaYfaroles.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] ofrendaSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/ofrendas.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();

        // 2. Grid y Suelo con Tilemap Ampliado (Y de 0 a 28)
        GameObject gridGo = GameObject.Find("Grid") ?? new GameObject("Grid");
        if (gridGo.GetComponent<Grid>() == null) gridGo.AddComponent<Grid>();

        Tilemap suelo = GetOrCreateTilemap(gridGo.transform, "Suelo", 0, -1, false);
        Tilemap paredes = GetOrCreateTilemap(gridGo.transform, "Paredes", 1, wallsLayer, true);
        suelo.ClearAllTiles();
        paredes.ClearAllTiles();

        TileBase[] allTiles = LoadTiles();
        // Llenar el atrio monumental: X de -2 a 38, Y de 0 a 28
        for (int x = -2; x <= 38; x++)
        {
            for (int y = 0; y <= 28; y++)
            {
                TileBase t = ((x + y) % 5 == 0) ? allTiles[1] : allTiles[0];
                suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // Limpiar raíces obsoletas del Nivel 3 previo
        string[] oldRoots = new string[] { "Props", "Hazards", "Barricadas", "Decoracion", "Luces", "Limites", "Escalinata", "Nichos_Sagrados" };
        foreach (var r in oldRoots)
        {
            GameObject ex = GameObject.Find(r);
            if (ex != null) Undo.DestroyObjectImmediate(ex);
        }

        // 3. Fachada Monumental de la Parroquia de Dolores (Al Norte del atrio, Y = 21.0f)
        Transform bgGroup = FreshRoot("Fondo_Monumental");
        if (catedralSprite != null)
        {
            GameObject catGo = new GameObject("Parroquia_Dolores_Hidalgo");
            catGo.transform.SetParent(bgGroup, false);
            catGo.transform.position = new Vector3(18.0f, 21.0f, 0f);
            catGo.transform.localScale = Vector3.one * 1.15f;

            SpriteRenderer srCat = catGo.AddComponent<SpriteRenderer>();
            srCat.sprite = catedralSprite;
            srCat.sortingOrder = 2; // Fondo para que Carlos camine sobre las escaleras (sortingOrder 5)
            srCat.color = new Color(0.98f, 0.96f, 1.0f);

            // Muros laterales que cubren las alas de la fachada y torres
            BoxCollider2D leftWallCol = catGo.AddComponent<BoxCollider2D>();
            leftWallCol.size = new Vector2(5.5f, 3.5f);
            leftWallCol.offset = new Vector2(-4.6f, 1.5f);

            BoxCollider2D rightWallCol = catGo.AddComponent<BoxCollider2D>();
            rightWallCol.size = new Vector2(5.5f, 3.5f);
            rightWallCol.offset = new Vector2(4.6f, 1.5f);

            // Muro superior detrás de la puerta
            BoxCollider2D doorBackWall = catGo.AddComponent<BoxCollider2D>();
            doorBackWall.size = new Vector2(3.6f, 1.0f);
            doorBackWall.offset = new Vector2(0f, 2.3f);

            if (wallsLayer >= 0) catGo.layer = wallsLayer;
        }

        // 4. Perímetro y Rejas (Límites cerrados para el atrio extendido)
        Transform rejasGroup = FreshRoot("Rejas_Perimetro");
        Sprite rejaInf = rejaSprites.FirstOrDefault(s => s.name == "Reja_Inferior") ?? rejaSprites.ElementAtOrDefault(6);
        Sprite farolaPie = rejaSprites.FirstOrDefault(s => s.name == "Farola_Pie_N3") ?? rejaSprites.ElementAtOrDefault(5);

        // Reja sur inferior (dejando entrada central en X = 18)
        for (float rx = 2.5f; rx <= 13.5f; rx += 5.5f)
        {
            CreateFencePiece(rejasGroup, $"Reja_Sur_Oeste_{rx:0}", rejaInf, rx, 0.8f, wallsLayer, new Vector2(5.5f, 1.0f));
        }
        for (float rx = 22.5f; rx <= 33.5f; rx += 5.5f)
        {
            CreateFencePiece(rejasGroup, $"Reja_Sur_Este_{rx:0}", rejaInf, rx, 0.8f, wallsLayer, new Vector2(5.5f, 1.0f));
        }

        // Farolas de pie a lo largo de las 3 secciones
        float[] farolasCalzada = new float[] { 6.0f, 30.0f };
        foreach (float fx in farolasCalzada)
        {
            CreateStandingLantern(rejasGroup, $"Farola_Calzada_{fx:0}", farolaPie, fx, 2.0f, wallsLayer);
        }

        float[] farolasPlaza = new float[] { 6.0f, 12.0f, 24.0f, 30.0f };
        foreach (float fx in farolasPlaza)
        {
            CreateStandingLantern(rejasGroup, $"Farola_Plaza_{fx:0}", farolaPie, fx, 11.0f, wallsLayer);
        }

        float[] farolasAtrioSup = new float[] { 13.0f, 23.0f };
        foreach (float fx in farolasAtrioSup)
        {
            CreateStandingLantern(rejasGroup, $"Farola_AtrioSup_{fx:0}", farolaPie, fx, 18.5f, wallsLayer);
        }

        // Límites invisibles sólidos del atrio completo
        Transform limits = FreshRoot("Limites_Atrio");
        CreateInvisibleWall(limits, "Lim_Oeste",  2.0f,  13.5f, 27f, wallsLayer);
        CreateInvisibleWall(limits, "Lim_Este",   34.0f,  13.5f, 27f, wallsLayer);
        CreateInvisibleWall(limits, "Lim_Sur",    18.0f,  0.2f,   1f, wallsLayer, 36.0f);
        CreateInvisibleWall(limits, "Lim_Norte",  18.0f, 25.0f,   1f, wallsLayer, 36.0f);

        // 5. Fuente Central Monumental en la Plaza (X = 18.0f, Y = 11.0f)
        Transform props = FreshRoot("Props_Atrio");
        Sprite fuenteSprite = fuenteSprites.FirstOrDefault(s => s.name == "Fuente_Octagonal_N3") ?? fuenteSprites.ElementAtOrDefault(0);
        if (fuenteSprite != null)
        {
            GameObject fGo = new GameObject("Fuente_Octagonal_Central");
            fGo.transform.SetParent(props, false);
            fGo.transform.position = new Vector3(18.0f, 11.0f, 0f);
            fGo.transform.localScale = Vector3.one * 1.2f;

            SpriteRenderer srF = fGo.AddComponent<SpriteRenderer>();
            srF.sprite = fuenteSprite;
            srF.sortingOrder = 3;

            CircleCollider2D cc = fGo.AddComponent<CircleCollider2D>();
            cc.radius = 2.3f;
            cc.offset = new Vector2(0f, -0.2f);
            if (wallsLayer >= 0) fGo.layer = wallsLayer;

            GameObject lf = new GameObject("Luz_Agua_Fuente");
            lf.transform.SetParent(fGo.transform, false);
            Light2D l2df = lf.AddComponent<Light2D>();
            l2df.lightType = Light2D.LightType.Point;
            l2df.color = new Color(0.45f, 0.75f, 1.0f);
            l2df.intensity = 1.2f;
            l2df.pointLightOuterRadius = 5.2f;
        }

        // 6. Escondites y Props Coloniales (Distribuidos armónicamente en las 3 secciones)
        Sprite sConfesionario = accSprites.FirstOrDefault(s => s.name == "Confesionario_N3") ?? accSprites.ElementAtOrDefault(1);
        Sprite sBanca         = accSprites.FirstOrDefault(s => s.name == "Banca_Hierro_N3") ?? accSprites.ElementAtOrDefault(0);
        Sprite sCripta        = accSprites.FirstOrDefault(s => s.name == "Cripta_Cantera_N3") ?? accSprites.ElementAtOrDefault(2);
        Sprite sCipres        = accSprites.FirstOrDefault(s => s.name == "Cipres_Colonial_1") ?? accSprites.ElementAtOrDefault(4);
        Sprite sAltar         = accSprites.FirstOrDefault(s => s.name == "Altar_Atrio_N3") ?? accSprites.ElementAtOrDefault(8);

        // Sección 1: Bancas en la Calzada Sur
        CreateHideSpotOfficial(props, "Banca_Calzada_Oeste", sBanca,  8.5f, 3.8f, 1.1f, hideSpotLayer);
        CreateHideSpotOfficial(props, "Banca_Calzada_Este",  sBanca, 27.5f, 3.8f, 1.1f, hideSpotLayer);

        // Sección 2: Altar y Confesionario en el Ala Oeste (Plazoleta del Altar)
        if (sAltar != null)
        {
            CreateSolidProp(props, "Altar_Sagrado_Oeste", sAltar, 7.5f, 14.2f, 1.15f, wallsLayer, new Vector2(4.6f, 1.6f), new Vector2(0f, 0.8f));
            GameObject lAltar = new GameObject("Luz_Altar");
            lAltar.transform.SetParent(props, false);
            lAltar.transform.position = new Vector3(7.5f, 14.6f, 0f);
            Light2D l2dAltar = lAltar.AddComponent<Light2D>();
            l2dAltar.lightType = Light2D.LightType.Point;
            l2dAltar.color = new Color(1.0f, 0.70f, 0.25f);
            l2dAltar.intensity = 1.6f;
            l2dAltar.pointLightOuterRadius = 4.8f;
            lAltar.AddComponent<LightFlicker>();
        }
        CreateHideSpotOfficial(props, "Confesionario_Ala_Oeste", sConfesionario, 5.5f, 9.5f, 1.15f, hideSpotLayer);

        // Sección 2: Criptas en el Ala Este (Jardín de las Criptas)
        CreateHideSpotOfficial(props, "Cripta_Ala_Este", sCripta, 28.5f, 14.0f, 1.1f, hideSpotLayer);
        CreateHideSpotOfficial(props, "Banca_Jardin_Este", sBanca, 30.5f, 9.5f, 1.1f, hideSpotLayer);

        // Sección 3: Confesionario y Cripta adicionales en la explanada superior
        CreateHideSpotOfficial(props, "Confesionario_Atrio_Sup", sConfesionario,  7.0f, 19.5f, 1.15f, hideSpotLayer);
        CreateHideSpotOfficial(props, "Cripta_Atrio_Sup",       sCripta,        29.0f, 19.5f, 1.1f, hideSpotLayer);

        // Cipreses enmarcando el atrio
        if (sCipres != null)
        {
            CreateSolidProp(props, "Cipres_Calzada_Oeste", sCipres, 14.0f,  4.5f, 1.25f, wallsLayer, new Vector2(0.9f, 0.8f), new Vector2(0f, 0.45f));
            CreateSolidProp(props, "Cipres_Calzada_Este",  sCipres, 22.0f,  4.5f, 1.25f, wallsLayer, new Vector2(0.9f, 0.8f), new Vector2(0f, 0.45f));
            CreateSolidProp(props, "Cipres_Atrio_Oeste",    sCipres,  9.5f, 19.0f, 1.25f, wallsLayer, new Vector2(0.9f, 0.8f), new Vector2(0f, 0.45f));
            CreateSolidProp(props, "Cipres_Atrio_Este",     sCipres, 26.5f, 19.0f, 1.25f, wallsLayer, new Vector2(0.9f, 0.8f), new Vector2(0f, 0.45f));
        }

        // 7. LOS 3 NICHOS SAGRADOS DEL RITUAL ([F] para ofrendar veladora)
        Transform nichosGroup = FreshRoot("Nichos_Sagrados");
        Sprite sSahumador = ofrendaSprites.FirstOrDefault(s => s.name == "Sahumador_Roto") ?? ofrendaSprites.ElementAtOrDefault(6);
        Sprite sCandelabro = ofrendaSprites.FirstOrDefault(s => s.name == "Candelabro_Vela") ?? ofrendaSprites.ElementAtOrDefault(3);
        Sprite sFlama = ofrendaSprites.FirstOrDefault(s => s.name == "Vela_Blanca_Grande") ?? ofrendaSprites.ElementAtOrDefault(4);

        // Nicho 1: Altar Sagrado (Ala Oeste)
        CreateShrineNiche(nichosGroup, "Nicho_Altar_Oeste", "Altar de la Cruz (Oeste)", 7.5f, 12.2f, sSahumador, sFlama);

        // Nicho 2: Fuente Sagrada (Plaza Central)
        CreateShrineNiche(nichosGroup, "Nicho_Fuente_Centro", "Pocito de la Fuente (Centro)", 18.0f, 14.0f, sCandelabro, sFlama);

        // Nicho 3: Cripta Sagrada (Ala Este)
        CreateShrineNiche(nichosGroup, "Nicho_Cripta_Este", "Cripta de las Ánimas (Este)", 28.5f, 12.2f, sSahumador, sFlama);

        // 8. Trampas de Terreno (Decals espaciados sin bloquear caminos)
        Transform hazards = FreshRoot("Obstaculos_Ofrendas");
        Sprite sAguaBendita = ofrendaSprites.FirstOrDefault(s => s.name == "Charco_AguaBendita") ?? ofrendaSprites.ElementAtOrDefault(0);
        Sprite sTepalcate   = ofrendaSprites.FirstOrDefault(s => s.name == "Vidrios_Jarron_Roto") ?? ofrendaSprites.ElementAtOrDefault(5);
        Sprite sVelaRoja    = ofrendaSprites.FirstOrDefault(s => s.name == "Vaso_Vela_Roja_1") ?? ofrendaSprites.ElementAtOrDefault(1);

        CreateMudPuddleOfficial(hazards, sAguaBendita, 12.0f, 8.5f, 0.85f);
        CreateMudPuddleOfficial(hazards, sAguaBendita, 24.0f, 16.5f, 0.85f);
        CreateNoiseObstacleOfficial(hazards, sTepalcate, 24.0f, 8.5f);
        CreateNoiseObstacleOfficial(hazards, sTepalcate, 12.0f, 16.5f);

        // 5 Veladoras Coleccionables (3 necesarias para los nichos + 2 de reserva para distraer con [F])
        CreateCandlePickupOfficial(hazards, "Vela_Calzada_Sur",    sVelaRoja, new Vector2(18.0f,  3.5f));
        CreateCandlePickupOfficial(hazards, "Vela_Banca_Oeste",    sVelaRoja, new Vector2( 8.5f,  5.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Banca_Este",     sVelaRoja, new Vector2(27.5f,  5.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Fuente_Centro",  sVelaRoja, new Vector2(18.0f,  8.5f));
        CreateCandlePickupOfficial(hazards, "Vela_Atrio_Superior", sVelaRoja, new Vector2(13.0f, 17.5f));

        // 9. Waypoints de Patrulla para La Llorona (Circuito amplio en el atrio extendido)
        Transform wpRoot = FreshRoot("Waypoints");
        Transform[] wps = new Transform[8];
        wps[0] = CreateEmpty(wpRoot, "WP_Sur_Entrada",     18.0f,  4.5f);
        wps[1] = CreateEmpty(wpRoot, "WP_Plaza_Suroeste",  11.5f,  7.5f);
        wps[2] = CreateEmpty(wpRoot, "WP_Altar_Oeste",      8.5f, 12.0f);
        wps[3] = CreateEmpty(wpRoot, "WP_AtrioSup_Oeste",  13.0f, 17.5f);
        wps[4] = CreateEmpty(wpRoot, "WP_AtrioSup_Centro", 18.0f, 18.0f);
        wps[5] = CreateEmpty(wpRoot, "WP_AtrioSup_Este",   23.0f, 17.5f);
        wps[6] = CreateEmpty(wpRoot, "WP_Cripta_Este",     27.5f, 12.0f);
        wps[7] = CreateEmpty(wpRoot, "WP_Plaza_Sureste",   24.5f,  7.5f);

        // 10. META DEL NIVEL: Puertas Principales de la Parroquia de Dolores (Coronando las escaleras en Y = 22.7f)
        Transform exitGroup = FreshRoot("Salida_Meta");
        CreateParroquiaEntranceGoalWithSeal(exitGroup, 18.0f, 22.7f);

        // 11. Iluminación Global URP 2D (Noche en el atrio: 0.50f)
        EnsureGlobalLight(0.50f);

        // 12. Player (Carlos inicia en la calzada sur en Y = 1.8f)
        SetupPlayer(new Vector2(18.0f, 1.8f), playerLayer);

        // 13. La Llorona (Inicia patrullando entre la fuente y el paseo superior)
        SetupLloronaCircular(new Vector2(18.0f, 11.0f), wps, wallsLayer, lloronaLayer, 1.6f, 2.8f, 5.5f, 110.0f);

        // 14. Cámara y Confiner 2D (Seguimiento suave a lo largo del atrio monumental de Y = 1.8 a 22.7)
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 3.2f;
        cam.transform.position = new Vector3(18.0f, 4.0f, -10f);
        cam.backgroundColor = new Color(0.04f, 0.03f, 0.08f);

        CameraFollow2D camFollow = cam.GetComponent<CameraFollow2D>() ?? cam.gameObject.AddComponent<CameraFollow2D>();
        camFollow.SetOrthoSize(3.2f);
        camFollow.SetOffset(new Vector3(0f, 1.2f, -10f));
        camFollow.SetBounds(new Vector2(10.0f, 3.8f), new Vector2(26.0f, 23.5f));

        // 15. Triggers de Tutorial narrativo
        Transform tutRoot = FreshRoot("TutorialTriggers");
        CreateTutorialTrigger(tutRoot, "Tutorial_Inicio",
            new Vector2(18.0f, 2.0f), new Vector2(8f, 3f),
            "¡El Atrio de la Parroquia de Dolores!\nLas puertas del templo están selladas por el luto de La Llorona.\n• Encuentra veladoras y usa [F] en los 3 nichos sagrados para romper el sello.\n• Cada nicho encendido crea una zona segura divina.\n• Pulsa [E] para esconderte en bancas y confesionarios.",
            showOnStart: true);

        // 16. GameManager y AudioManager
        SetupGameManager(3);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.SaveScene(scene, ScenePath);
        UIBuilder.BuildLevel3UICanvas();
        Debug.Log("[Level3Builder] ¡Nivel 3 monumental (El Atrio de Dolores - Opción A) reconstruido y guardado con éxito!");
    }

    static void CreateShrineNiche(Transform parent, string goName, string displayName, float x, float y, Sprite pedestalSprite, Sprite flameSprite)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        // Pedestal base
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = pedestalSprite;
        sr.sortingOrder = 3;
        sr.color = new Color(0.92f, 0.88f, 0.82f);

        // Collider físico para que el pedestal sea un obstáculo sólido
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.2f, 0.8f);
        col.offset = new Vector2(0f, 0.3f);

        // Visual de flama sagrada (inicia inactivo)
        GameObject flameGo = new GameObject("Flama_Sagrada");
        flameGo.transform.SetParent(go.transform, false);
        flameGo.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        SpriteRenderer srFlame = flameGo.AddComponent<SpriteRenderer>();
        srFlame.sprite = flameSprite;
        srFlame.sortingOrder = 4;
        flameGo.SetActive(false);

        // Luz divina dorada del nicho
        GameObject lightGo = new GameObject("Luz_Nicho");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.75f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.82f, 0.40f);
        l2d.intensity = 2.4f;
        l2d.pointLightOuterRadius = 6.0f;
        lightGo.AddComponent<LightFlicker>();
        l2d.enabled = false;

        // Prompt de interacción [F]
        GameObject promptGo = new GameObject("PromptInteraccion");
        promptGo.transform.SetParent(go.transform, false);
        promptGo.transform.localPosition = new Vector3(0f, 1.4f, 0f);
        TextMesh tm = promptGo.AddComponent<TextMesh>();
        tm.text = "[F] Consagrar Ofrenda";
        tm.fontSize = 22;
        tm.characterSize = 0.09f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1.0f, 0.90f, 0.50f);
        MeshRenderer mr = promptGo.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;
        promptGo.SetActive(false);

        // Componente ShrineNiche
        ShrineNiche sn = go.AddComponent<ShrineNiche>();
        SerializedObject so = new SerializedObject(sn);
        so.FindProperty("shrineName").stringValue = displayName;
        so.FindProperty("sanctuaryRadius").floatValue = 3.2f;
        so.FindProperty("flameVisual").objectReferenceValue = flameGo;
        so.FindProperty("shrineLight").objectReferenceValue = l2d;
        so.FindProperty("promptObject").objectReferenceValue = promptGo;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, goName);
    }

    static void CreateParroquiaEntranceGoalWithSeal(Transform parent, float x, float y)
    {
        GameObject go = new GameObject("Puertas_Parroquia_Meta");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        // Collider de victoria en la puerta de madera (inicia desactivado hasta abrir las 3 ofrendas)
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(3.2f, 1.2f);
        bc.offset = Vector2.zero;
        bc.enabled = false;
        go.AddComponent<LevelGoal>();

        // Niebla espectral de sellado (morada/oscura)
        GameObject sealMistGo = new GameObject("Aura_Sello_Espectral");
        sealMistGo.transform.SetParent(go.transform, false);
        sealMistGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        Light2D l2dSeal = sealMistGo.AddComponent<Light2D>();
        l2dSeal.lightType = Light2D.LightType.Point;
        l2dSeal.color = new Color(0.65f, 0.25f, 0.85f);
        l2dSeal.intensity = 1.8f;
        l2dSeal.pointLightOuterRadius = 4.5f;
        sealMistGo.AddComponent<LightFlicker>();

        // Luz divina del santuario (dorada/cálida, se enciende al abrir)
        GameObject lightGo = new GameObject("Luz_Entrada_Parroquia");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.88f, 0.48f);
        l2d.intensity = 2.8f;
        l2d.pointLightOuterRadius = 8.0f;
        lightGo.AddComponent<LightFlicker>();
        l2d.enabled = false;

        // Letrero de estado de la puerta
        GameObject labelGo = new GameObject("LabelPuerta");
        labelGo.transform.SetParent(go.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        TextMesh tm = labelGo.AddComponent<TextMesh>();
        tm.text = "PUERTAS SELLADAS (Ofrendas: 0/3)";
        tm.fontSize = 26;
        tm.characterSize = 0.10f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.9f, 0.5f, 0.8f);
        MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;

        // Controlador de sellado y apertura
        ParroquiaDoorController pdc = go.AddComponent<ParroquiaDoorController>();
        SerializedObject so = new SerializedObject(pdc);
        so.FindProperty("requiredShrines").intValue = 3;
        so.FindProperty("doorSealVisual").objectReferenceValue = sealMistGo;
        so.FindProperty("sanctuaryLight").objectReferenceValue = l2d;
        so.FindProperty("goalCollider").objectReferenceValue = bc;
        so.FindProperty("statusLabel").objectReferenceValue = tm;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, "Puertas_Parroquia_Meta");
    }

    static TileBase[] LoadTiles()
    {
        TileBase[] tiles = new TileBase[48];
        for (int i = 0; i < 48; i++)
            tiles[i] = AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Tiles/tiles_{i}.asset");
        return tiles;
    }

    static Transform FreshRoot(string name)
    {
        GameObject ex = GameObject.Find(name);
        if (ex != null) Undo.DestroyObjectImmediate(ex);
        GameObject g = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(g, name);
        return g.transform;
    }

    static Tilemap GetOrCreateTilemap(Transform parent, string name, int order, int layer, bool hasCollider)
    {
        Transform ch = parent.Find(name);
        GameObject go = ch != null ? ch.gameObject : new GameObject(name);
        if (ch == null) { go.transform.SetParent(parent, false); Undo.RegisterCreatedObjectUndo(go, name); }
        Tilemap tm = go.GetComponent<Tilemap>() ?? go.AddComponent<Tilemap>();
        TilemapRenderer tr = go.GetComponent<TilemapRenderer>() ?? go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = order;
        if (layer >= 0) go.layer = layer;
        if (hasCollider)
        {
            TilemapCollider2D tc = go.GetComponent<TilemapCollider2D>() ?? go.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
        }
        return tm;
    }

    static void CreateFencePiece(Transform parent, string name, Sprite sprite, float x, float y, int layer, Vector2 colSize)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.size = colSize;
        bc.offset = new Vector2(0f, 0.6f);
        if (layer >= 0) go.layer = layer;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateStandingLantern(Transform parent, string name, Sprite sprite, float x, float y, int layer)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.radius = 0.45f;
        cc.offset = new Vector2(0f, 0.25f);
        if (layer >= 0) go.layer = layer;

        GameObject lightGo = new GameObject("Luz");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.78f, 0.45f);
        l2d.intensity = 1.9f;
        l2d.pointLightOuterRadius = 6.2f;
        lightGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateHideSpotOfficial(Transform parent, string name, Sprite sprite, float x, float y, float scale, int layer)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.8f, 1.4f);
        col.offset = new Vector2(0f, 0.35f);
        if (layer >= 0) go.layer = layer;

        GameObject hp = new GameObject("HidePoint");
        hp.transform.SetParent(go.transform, false);
        hp.transform.localPosition = new Vector3(0f, 0.2f, 0f);

        HideSpot hs = go.AddComponent<HideSpot>();
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("hidePoint").objectReferenceValue = hp.transform;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateSolidProp(Transform parent, string name, Sprite sprite, float x, float y, float scale, int layer, Vector2 colSize, Vector2 colOffset)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = colSize;
        col.offset = colOffset;
        if (layer >= 0) go.layer = layer;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateMudPuddleOfficial(Transform parent, Sprite sprite, float x, float y, float scale = 1.0f)
    {
        GameObject go = new GameObject("Charco_AguaBendita");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 1.2f;

        go.AddComponent<MudPuddle>();
        Undo.RegisterCreatedObjectUndo(go, "Charco_AguaBendita");
    }

    static void CreateNoiseObstacleOfficial(Transform parent, Sprite sprite, float x, float y)
    {
        GameObject go = new GameObject("Tepalcates_Ruido");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(1.5f, 1.2f);

        go.AddComponent<NoiseObstacle>();
        Undo.RegisterCreatedObjectUndo(go, "Tepalcates_Ruido");
    }

    static void CreateCandlePickupOfficial(Transform parent, string name, Sprite sprite, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.3f;
        col.offset = Vector2.zero;

        GameObject lightGo = new GameObject("LuzVela");
        lightGo.transform.SetParent(go.transform, false);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.45f, 0.25f);
        l2d.intensity = 1.2f;
        l2d.pointLightOuterRadius = 3.0f;

        go.AddComponent<Candle>();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateInvisibleWall(Transform parent, string name, float x, float y, float height, int layer, float width = 1.0f)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        if (layer >= 0) go.layer = layer;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(width, height);
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void EnsureGlobalLight(float intensity)
    {
        GameObject glGo = GameObject.Find("Global Light 2D");
        if (glGo == null) glGo = new GameObject("Global Light 2D");
        Light2D gl = glGo.GetComponent<Light2D>() ?? glGo.AddComponent<Light2D>();
        gl.lightType = Light2D.LightType.Global;
        gl.color = new Color(0.90f, 0.88f, 0.82f);
        gl.intensity = intensity;
    }

    static void SetupPlayer(Vector2 spawnPos, int layer)
    {
        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            playerGo = prefab != null ? PrefabUtility.InstantiatePrefab(prefab) as GameObject : new GameObject("Player");
            playerGo.tag = "Player";
        }
        playerGo.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
        if (layer >= 0) playerGo.layer = layer;

        PlayerMovement pm = playerGo.GetComponent<PlayerMovement>() ?? playerGo.AddComponent<PlayerMovement>();
        if (playerGo.GetComponent<PlayerAnimation>() == null) playerGo.AddComponent<PlayerAnimation>();
        if (playerGo.GetComponent<HideSystem>() == null) playerGo.AddComponent<HideSystem>();

        SpriteRenderer sr = playerGo.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 5;

        CircleCollider2D col = playerGo.GetComponent<CircleCollider2D>() ?? playerGo.AddComponent<CircleCollider2D>();
        col.radius = 0.28f;
        col.offset = new Vector2(0f, -0.45f);

        Rigidbody2D rb = playerGo.GetComponent<Rigidbody2D>() ?? playerGo.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Candle.ResetInventory();
    }

    static void SetupLloronaCircular(Vector2 spawnPos, Transform[] wps, int wallsLayer, int lloronaLayer, float patrolSpeed, float chaseSpeed, float visionDist, float fov)
    {
        GameObject lloronaGo = GameObject.Find("Llorona");
        if (lloronaGo == null)
        {
            var aiEx = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
            if (aiEx != null) lloronaGo = aiEx.gameObject;
        }
        if (lloronaGo == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Llorona.prefab");
            lloronaGo = prefab != null ? PrefabUtility.InstantiatePrefab(prefab) as GameObject : new GameObject("Llorona");
        }
        lloronaGo.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
        if (lloronaLayer >= 0) lloronaGo.layer = lloronaLayer;

        LloronaAI ai = lloronaGo.GetComponent<LloronaAI>() ?? lloronaGo.AddComponent<LloronaAI>();
        ai.Waypoints = wps;

        SerializedObject so = new SerializedObject(ai);
        var pSpeed = so.FindProperty("patrolSpeed");
        if (pSpeed != null) pSpeed.floatValue = patrolSpeed;
        var cSpeed = so.FindProperty("chaseSpeed");
        if (cSpeed != null) cSpeed.floatValue = chaseSpeed;
        var oMask = so.FindProperty("obstacleMask");
        if (oMask != null) oMask.intValue = 1 << wallsLayer;
        so.ApplyModifiedProperties();
    }

    static void CreateTutorialTrigger(Transform parent, string name, Vector2 pos, Vector2 size, string text, bool showOnStart = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = size;

        TutorialTrigger tt = go.AddComponent<TutorialTrigger>();
        SerializedObject so = new SerializedObject(tt);
        var msgProp = so.FindProperty("tutorialMessage");
        if (msgProp != null) msgProp.stringValue = text;
        var startProp = so.FindProperty("showOnStart");
        if (startProp != null) startProp.boolValue = showOnStart;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void SetupGameManager(int levelNumber)
    {
        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject gmGo = new GameObject("GameManager");
            gm = gmGo.AddComponent<GameManager>();
        }
        gm.SetLevelNumber(levelNumber);
        SerializedObject so = new SerializedObject(gm);
        var prop = so.FindProperty("currentLevelNumber");
        if (prop != null) prop.intValue = levelNumber;
        so.ApplyModifiedProperties();
    }

    static Transform CreateEmpty(Transform parent, string name, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        Undo.RegisterCreatedObjectUndo(go, name);
        return go.transform;
    }
}
