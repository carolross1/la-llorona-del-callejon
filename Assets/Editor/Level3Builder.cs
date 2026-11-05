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
        Sprite sAltarSagrado = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/altar_sagrado.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sMesaVelas = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/mesa_velas.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sSelloPuerta = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/sello_puerta.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sCirculoSantuario = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/circulo_santuario.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sPuertasAbiertas = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/puertas_abiertas.png")
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

        // 3. Fachada Monumental de la Parroquia de Dolores (Al Norte del atrio, Y = 19.8f)
        Transform bgGroup = FreshRoot("Fondo_Monumental");
        if (catedralSprite != null)
        {
            GameObject catGo = new GameObject("Parroquia_Dolores_Hidalgo");
            catGo.transform.SetParent(bgGroup, false);
            catGo.transform.position = new Vector3(18.0f, 19.8f, 0f);
            catGo.transform.localScale = new Vector3(0.78f, 0.78f, 1.0f);

            SpriteRenderer srCat = catGo.AddComponent<SpriteRenderer>();
            srCat.sprite = catedralSprite;
            srCat.sortingOrder = 2; // Fondo para que Carlos camine sobre las escaleras (sortingOrder 5)
            srCat.color = new Color(0.98f, 0.96f, 1.0f);

            // Muros laterales que cubren las alas de la fachada y torres
            BoxCollider2D leftWallCol = catGo.AddComponent<BoxCollider2D>();
            leftWallCol.size = new Vector2(4.5f, 3.2f);
            leftWallCol.offset = new Vector2(-3.8f, 1.2f);

            BoxCollider2D rightWallCol = catGo.AddComponent<BoxCollider2D>();
            rightWallCol.size = new Vector2(4.5f, 3.2f);
            rightWallCol.offset = new Vector2(3.8f, 1.2f);

            // Muro superior detrás de la puerta
            BoxCollider2D doorBackWall = catGo.AddComponent<BoxCollider2D>();
            doorBackWall.size = new Vector2(3.2f, 1.0f);
            doorBackWall.offset = new Vector2(0f, 1.9f);

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
        CreateHideSpotOfficial(props, "Banca_Calzada_Oeste", sBanca,  8.5f, 3.8f, 1.1f, hideSpotLayer, wallsLayer,
            new Vector2(2.8f, 0.9f), new Vector2(0f, 0.2f), new Vector2(3.2f, 1.6f), new Vector2(0f, -0.4f));
        CreateHideSpotOfficial(props, "Banca_Calzada_Este",  sBanca, 27.5f, 3.8f, 1.1f, hideSpotLayer, wallsLayer,
            new Vector2(2.8f, 0.9f), new Vector2(0f, 0.2f), new Vector2(3.2f, 1.6f), new Vector2(0f, -0.4f));

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
        CreateHideSpotOfficial(props, "Confesionario_Ala_Oeste", sConfesionario, 5.5f, 9.5f, 1.15f, hideSpotLayer, wallsLayer,
            new Vector2(2.0f, 1.4f), new Vector2(0f, 0.4f), new Vector2(2.4f, 1.8f), new Vector2(0f, -0.4f));

        // Sección 2: Criptas en el Ala Este (Jardín de las Criptas)
        CreateHideSpotOfficial(props, "Cripta_Ala_Este", sCripta, 28.5f, 14.0f, 1.1f, hideSpotLayer, wallsLayer,
            new Vector2(2.4f, 1.4f), new Vector2(0f, 0.4f), new Vector2(2.8f, 1.8f), new Vector2(0f, -0.4f));
        CreateHideSpotOfficial(props, "Banca_Jardin_Este", sBanca, 30.5f, 9.5f, 1.1f, hideSpotLayer, wallsLayer,
            new Vector2(2.8f, 0.9f), new Vector2(0f, 0.2f), new Vector2(3.2f, 1.6f), new Vector2(0f, -0.4f));

        // Sección 3: Confesionario y Cripta adicionales en la explanada superior
        CreateHideSpotOfficial(props, "Confesionario_Atrio_Sup", sConfesionario,  7.0f, 19.5f, 1.15f, hideSpotLayer, wallsLayer,
            new Vector2(2.0f, 1.4f), new Vector2(0f, 0.4f), new Vector2(2.4f, 1.8f), new Vector2(0f, -0.4f));
        CreateHideSpotOfficial(props, "Cripta_Atrio_Sup",       sCripta,        29.0f, 19.5f, 1.1f, hideSpotLayer, wallsLayer,
            new Vector2(2.4f, 1.4f), new Vector2(0f, 0.4f), new Vector2(2.8f, 1.8f), new Vector2(0f, -0.4f));

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
        Sprite sAltarN3 = sAltarSagrado ?? accSprites.FirstOrDefault(s => s.name == "Altar_Atrio_N3") ?? ofrendaSprites.ElementAtOrDefault(3);
        Sprite sCriptaN3 = sAltarSagrado ?? accSprites.FirstOrDefault(s => s.name == "Cripta_Cantera_N3") ?? ofrendaSprites.ElementAtOrDefault(6);
        Sprite sCandelabro = ofrendaSprites.FirstOrDefault(s => s.name == "Candelabro_Vela") ?? ofrendaSprites.ElementAtOrDefault(3);
        Sprite sFlama = ofrendaSprites.FirstOrDefault(s => s.name == "Vela_Blanca_Grande") ?? ofrendaSprites.ElementAtOrDefault(4);
        Sprite sFlores = ofrendaSprites.FirstOrDefault(s => s.name == "Ramo_Cempasuchil_1") ?? ofrendaSprites.ElementAtOrDefault(7);

        // Nicho 1: Altar Sagrado (Ala Oeste)
        CreateShrineNiche(nichosGroup, "Nicho_Altar_Oeste", "Altar de la Cruz (Oeste)", 7.5f, 12.2f, sAltarN3, sFlama, sFlores, sCirculoSantuario, wallsLayer);

        // Nicho 2: Fuente Sagrada (Plaza Central)
        CreateShrineNiche(nichosGroup, "Nicho_Fuente_Centro", "Pocito de la Fuente (Centro)", 18.0f, 14.0f, sAltarN3, sFlama, sFlores, sCirculoSantuario, wallsLayer);

        // Nicho 3: Cripta Sagrada (Ala Este)
        CreateShrineNiche(nichosGroup, "Nicho_Cripta_Este", "Cripta de las Ánimas (Este)", 28.5f, 12.2f, sCriptaN3, sFlama, sFlores, sCirculoSantuario, wallsLayer);

        // Mesa de Cirios Benditos (Dispensador en la Calzada Sur, al costado para dejar libre el paso central)
        CreateCandleStandOfficial(nichosGroup, 22.5f, 3.2f, sMesaVelas ?? sAltarN3, sCandelabro, wallsLayer);

        // 8. Trampas de Terreno (Decals espaciados sin bloquear caminos)
        Transform hazards = FreshRoot("Obstaculos_Ofrendas");
        Sprite sAguaBendita = ofrendaSprites.FirstOrDefault(s => s.name == "Charco_AguaBendita") ?? ofrendaSprites.ElementAtOrDefault(0);
        Sprite sTepalcate   = ofrendaSprites.FirstOrDefault(s => s.name == "Vidrios_Jarron_Roto") ?? ofrendaSprites.ElementAtOrDefault(5);
        Sprite sVelaRoja    = ofrendaSprites.FirstOrDefault(s => s.name == "Vaso_Vela_Roja_1") ?? ofrendaSprites.ElementAtOrDefault(1);

        CreateMudPuddleOfficial(hazards, sAguaBendita, 12.0f, 8.5f, 0.85f);
        CreateMudPuddleOfficial(hazards, sAguaBendita, 24.0f, 16.5f, 0.85f);
        CreateNoiseObstacleOfficial(hazards, sTepalcate, 24.0f, 8.5f);
        CreateNoiseObstacleOfficial(hazards, sTepalcate, 12.0f, 16.5f);

        // 9 Veladoras Coleccionables repartidas en el atrio
        CreateCandlePickupOfficial(hazards, "Vela_Calzada_Sur",    sVelaRoja, new Vector2(18.0f,  2.5f));
        CreateCandlePickupOfficial(hazards, "Vela_Entrada_Oeste",  sVelaRoja, new Vector2(14.0f,  4.5f));
        CreateCandlePickupOfficial(hazards, "Vela_Entrada_Este",   sVelaRoja, new Vector2(22.0f,  4.5f));
        CreateCandlePickupOfficial(hazards, "Vela_Jardin_Oeste",   sVelaRoja, new Vector2( 8.5f,  7.5f));
        CreateCandlePickupOfficial(hazards, "Vela_Jardin_Este",    sVelaRoja, new Vector2(27.5f,  7.5f));
        CreateCandlePickupOfficial(hazards, "Vela_Fuente_Sur",     sVelaRoja, new Vector2(18.0f,  9.5f));
        CreateCandlePickupOfficial(hazards, "Vela_Altar_Oeste",    sVelaRoja, new Vector2( 9.5f, 14.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Cripta_Este",    sVelaRoja, new Vector2(26.5f, 14.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Atrio_Superior", sVelaRoja, new Vector2(15.0f, 17.5f));

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

        // 10. META DEL NIVEL: Puertas Principales de la Parroquia de Dolores (Coronando las escaleras en Y = 21.2f)
        Transform exitGroup = FreshRoot("Salida_Meta");
        CreateParroquiaEntranceGoalWithSeal(exitGroup, 18.0f, 21.2f, sSelloPuerta, sPuertasAbiertas);

        // 11. Iluminación Global URP 2D (Noche en el atrio: 0.50f)
        EnsureGlobalLight(0.50f);

        // 12. Player (Carlos inicia en la calzada sur en Y = 1.8f)
        SetupPlayer(new Vector2(18.0f, 1.8f), playerLayer);

        // 13. La Llorona (Inicia patrullando entre la fuente y el paseo superior)
        SetupLloronaCircular(new Vector2(18.0f, 11.0f), wps, wallsLayer, lloronaLayer, 1.6f, 2.8f, 5.5f, 110.0f);

        // 14. Cámara y Confiner 2D (Zoom dinámico cinematográfico para revelar la catedral completa)
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 3.6f;
        cam.transform.position = new Vector3(18.0f, 4.0f, -10f);
        cam.backgroundColor = new Color(0.04f, 0.03f, 0.08f);

        CameraFollow2D camFollow = cam.GetComponent<CameraFollow2D>() ?? cam.gameObject.AddComponent<CameraFollow2D>();
        camFollow.SetOrthoSize(3.6f);
        camFollow.SetOffset(new Vector3(0f, 1.1f, -10f));
        camFollow.SetBounds(new Vector2(10.0f, 3.8f), new Vector2(26.0f, 25.5f));
        camFollow.EnableDynamicZoom(minY: 12.0f, maxY: 21.2f, minSize: 3.6f, maxSize: 4.8f, normalOffY: 1.1f, peakOffY: 2.6f);

        // 15. Triggers de Tutorial narrativo
        Transform tutRoot = FreshRoot("TutorialTriggers");
        CreateTutorialTrigger(tutRoot, "Tutorial_Inicio",
            new Vector2(18.0f, 2.0f), new Vector2(8f, 3f),
            "¡El Atrio de la Parroquia de Dolores!\nLas puertas del templo están selladas por el luto espectral.\n• Busca las 3 columnas de luz divina en el atrio (Oeste, Centro, Este).\n• Acércate y pulsa [F] con una veladora para consagrar cada Nicho Sagrado.\n• Si te quedas sin veladoras, acude a la Mesa Bendita en la entrada para reponer.\n• Pulsa [E] para esconderte de La Llorona en bancas y confesionarios.",
            showOnStart: true);

        // 16. GameManager y AudioManager
        SetupGameManager(3);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.SaveScene(scene, ScenePath);
        UIBuilder.BuildLevel3UICanvas();
        Debug.Log("[Level3Builder] ¡Nivel 3 monumental (El Atrio de Dolores - Opción A) reconstruido y guardado con éxito!");
    }

    static void CreateShrineNiche(Transform parent, string goName, string displayName, float x, float y, Sprite pedestalSprite, Sprite flameSprite, Sprite flowerSprite, Sprite circuloSprite, int wallsLayer)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        // Pedestal base (Altar de cantera o Cripta)
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = pedestalSprite;
        sr.sortingOrder = 3;
        sr.color = new Color(0.95f, 0.92f, 0.88f);

        float scaleFactor = (pedestalSprite != null && pedestalSprite.rect.width > 800) ? 0.35f : 0.85f;
        go.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

        // Collider físico para que el pedestal sea un obstáculo sólido
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2.4f, 1.2f);
        col.offset = new Vector2(0f, 0.4f);
        if (wallsLayer >= 0) go.layer = wallsLayer;

        // Ramilletes de cempasúchil a los lados
        if (flowerSprite != null)
        {
            GameObject fl1 = new GameObject("Flores_Izq");
            fl1.transform.SetParent(go.transform, false);
            fl1.transform.localPosition = new Vector3(-1.1f, 0.1f, 0f);
            SpriteRenderer srFl1 = fl1.AddComponent<SpriteRenderer>();
            srFl1.sprite = flowerSprite;
            srFl1.sortingOrder = 4;
            fl1.transform.localScale = Vector3.one * 0.9f;

            GameObject fl2 = new GameObject("Flores_Der");
            fl2.transform.SetParent(go.transform, false);
            fl2.transform.localPosition = new Vector3(1.1f, 0.1f, 0f);
            SpriteRenderer srFl2 = fl2.AddComponent<SpriteRenderer>();
            srFl2.sprite = flowerSprite;
            srFl2.sortingOrder = 4;
            fl2.transform.localScale = new Vector3(-0.9f, 0.9f, 1f);
        }

        // Visual de flama sagrada (inicia inactivo hasta consagrarse)
        GameObject flameGo = new GameObject("Flama_Sagrada");
        flameGo.transform.SetParent(go.transform, false);
        flameGo.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        SpriteRenderer srFlame = flameGo.AddComponent<SpriteRenderer>();
        srFlame.sprite = flameSprite;
        srFlame.sortingOrder = 5;
        flameGo.SetActive(false);

        // Luz divina dorada del nicho consagrado (Santuario)
        GameObject lightGo = new GameObject("Luz_Nicho");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 1.0f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.84f, 0.42f);
        l2d.intensity = 2.6f;
        l2d.pointLightOuterRadius = 6.2f;
        lightGo.AddComponent<LightFlicker>();
        l2d.enabled = false;

        // Luz guía etérea (Faro celestial que llama al jugador desde lejos mientras esté apagado)
        GameObject beaconGo = new GameObject("Luz_Guia_Faro");
        beaconGo.transform.SetParent(go.transform, false);
        beaconGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        Light2D l2dBeacon = beaconGo.AddComponent<Light2D>();
        l2dBeacon.lightType = Light2D.LightType.Point;
        l2dBeacon.color = new Color(0.65f, 0.90f, 1.0f);
        l2dBeacon.intensity = 1.8f;
        l2dBeacon.pointLightOuterRadius = 6.5f;
        beaconGo.AddComponent<LightFlicker>();

        // Aura de domo santuario (visible al consagrarse con el círculo ritual de cempasúchil en el suelo)
        GameObject domeGo = new GameObject("Aura_Santuario");
        domeGo.transform.SetParent(go.transform, false);
        domeGo.transform.localPosition = new Vector3(0f, 0.4f, 0f);

        if (circuloSprite != null)
        {
            SpriteRenderer srDome = domeGo.AddComponent<SpriteRenderer>();
            srDome.sprite = circuloSprite;
            srDome.sortingOrder = 1;
            srDome.color = new Color(1.0f, 0.95f, 0.75f, 0.88f);
            float spriteWidth = circuloSprite.bounds.size.x > 0.01f ? circuloSprite.bounds.size.x : 5.1f;
            float targetDomeScale = (3.2f * 2.0f) / (spriteWidth * scaleFactor);
            domeGo.transform.localScale = new Vector3(targetDomeScale, targetDomeScale, 1f);
        }

        Light2D l2dDome = domeGo.AddComponent<Light2D>();
        l2dDome.lightType = Light2D.LightType.Point;
        l2dDome.color = new Color(1.0f, 0.88f, 0.35f);
        l2dDome.intensity = 1.6f;
        l2dDome.pointLightOuterRadius = 3.6f;
        domeGo.SetActive(false);

        // Prompt de interacción [F] con TextMesh nativo legible
        GameObject promptGo = new GameObject("PromptInteraccion");
        promptGo.transform.SetParent(go.transform, false);
        promptGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        TextMesh tm = promptGo.AddComponent<TextMesh>();
        tm.text = $"✦ {displayName} ✦";
        tm.fontSize = 32;
        tm.characterSize = 0.16f;
        tm.fontStyle = FontStyle.Bold;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1.0f, 0.95f, 0.50f);
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
        so.FindProperty("beaconLight").objectReferenceValue = l2dBeacon;
        so.FindProperty("sanctuaryDome").objectReferenceValue = domeGo;
        so.FindProperty("promptObject").objectReferenceValue = promptGo;
        so.FindProperty("promptTextMesh").objectReferenceValue = tm;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, goName);
    }

    static void CreateCandleStandOfficial(Transform parent, float x, float y, Sprite altarSprite, Sprite candelabroSprite, int wallsLayer)
    {
        GameObject go = new GameObject("Mesa_Cirios_Benditos");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = altarSprite;
        sr.sortingOrder = 3;
        sr.color = new Color(0.95f, 0.90f, 0.85f);
        float scaleMesa = (altarSprite != null && altarSprite.rect.width > 800) ? 0.35f : 0.72f;
        go.transform.localScale = new Vector3(scaleMesa, scaleMesa, 1.0f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2.6f, 1.2f);
        col.offset = new Vector2(0f, 0.4f);
        if (wallsLayer >= 0) go.layer = wallsLayer;

        if (candelabroSprite != null)
        {
            GameObject candleChild = new GameObject("Cirios_Mesa");
            candleChild.transform.SetParent(go.transform, false);
            candleChild.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            SpriteRenderer srC = candleChild.AddComponent<SpriteRenderer>();
            srC.sprite = candelabroSprite;
            srC.sortingOrder = 4;
        }

        GameObject lightGo = new GameObject("Luz_Mesa_Cirios");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.88f, 0.50f);
        l2d.intensity = 2.0f;
        l2d.pointLightOuterRadius = 4.5f;
        lightGo.AddComponent<LightFlicker>();

        GameObject promptGo = new GameObject("PromptMesa");
        promptGo.transform.SetParent(go.transform, false);
        promptGo.transform.localPosition = new Vector3(0f, 2.0f, 0f);
        TextMesh tm = promptGo.AddComponent<TextMesh>();
        tm.text = "[E] Tomar Veladora Bendita";
        tm.fontSize = 30;
        tm.characterSize = 0.16f;
        tm.fontStyle = FontStyle.Bold;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1.0f, 0.95f, 0.45f);
        MeshRenderer mr = promptGo.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;
        promptGo.SetActive(false);

        CandleStand cs = go.AddComponent<CandleStand>();
        SerializedObject so = new SerializedObject(cs);
        so.FindProperty("interactionRadius").floatValue = 2.4f;
        so.FindProperty("maxCandlesAllowed").intValue = 3;
        so.FindProperty("promptContainer").objectReferenceValue = promptGo;
        so.FindProperty("promptTextMesh").objectReferenceValue = tm;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, "Mesa_Cirios_Benditos");
    }

    static void CreateParroquiaEntranceGoalWithSeal(Transform parent, float x, float y, Sprite selloSprite, Sprite openDoorsSprite)
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

        // Niebla espectral de sellado (morada/oscura) con el sprite oficial del estandarte espectral
        GameObject sealMistGo = new GameObject("Aura_Sello_Espectral");
        sealMistGo.transform.SetParent(go.transform, false);
        sealMistGo.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        if (selloSprite != null)
        {
            SpriteRenderer srSeal = sealMistGo.AddComponent<SpriteRenderer>();
            srSeal.sprite = selloSprite;
            srSeal.sortingOrder = 4;
            srSeal.color = new Color(0.95f, 0.88f, 1.0f, 0.90f);
            sealMistGo.transform.localScale = new Vector3(0.50f, 0.50f, 1f);
        }

        Light2D l2dSeal = sealMistGo.AddComponent<Light2D>();
        l2dSeal.lightType = Light2D.LightType.Point;
        l2dSeal.color = new Color(0.65f, 0.25f, 0.85f);
        l2dSeal.intensity = 2.2f;
        l2dSeal.pointLightOuterRadius = 4.8f;
        sealMistGo.AddComponent<LightFlicker>();

        // Visual de puertas abiertas e interior sagrado iluminado (inicia desactivado hasta consagrar 3/3 nichos)
        GameObject openDoorsGo = new GameObject("Entrada_Puertas_Abiertas");
        openDoorsGo.transform.SetParent(go.transform, false);
        openDoorsGo.transform.localPosition = new Vector3(0f, 0.35f, 0f);

        if (openDoorsSprite != null)
        {
            SpriteRenderer srOpen = openDoorsGo.AddComponent<SpriteRenderer>();
            srOpen.sprite = openDoorsSprite;
            srOpen.sortingOrder = 3;
            // catedral.png tiene sortingOrder 2. El sprite de puertas abiertas en sortingOrder 3 cubre la entrada de madera cerrada.
            openDoorsGo.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        }
        openDoorsGo.SetActive(false);

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

        // Letrero de estado de la puerta con TextMesh nativo
        GameObject labelGo = new GameObject("LabelPuerta");
        labelGo.transform.SetParent(go.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 1.9f, 0f);
        TextMesh tm = labelGo.AddComponent<TextMesh>();
        tm.text = "PUERTAS SELLADAS\n(Ofrendas: 0/3)";
        tm.fontSize = 34;
        tm.characterSize = 0.17f;
        tm.fontStyle = FontStyle.Bold;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.95f, 0.60f, 0.85f);
        MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;

        // Controlador de sellado y apertura
        ParroquiaDoorController pdc = go.AddComponent<ParroquiaDoorController>();
        SerializedObject so = new SerializedObject(pdc);
        so.FindProperty("requiredShrines").intValue = 3;
        so.FindProperty("doorSealVisual").objectReferenceValue = sealMistGo;
        so.FindProperty("openDoorsVisual").objectReferenceValue = openDoorsGo;
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

    static void CreateHideSpotOfficial(Transform parent, string name, Sprite sprite, float x, float y, float scale, int hideSpotLayer, int wallsLayer, Vector2 solidColSize, Vector2 solidColOffset, Vector2 trigColSize, Vector2 trigColOffset)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        // 1. Collider sólido en capa Walls para evitar que Carlos o La Llorona atraviesen el objeto
        BoxCollider2D solidCol = go.AddComponent<BoxCollider2D>();
        solidCol.size = solidColSize;
        solidCol.offset = solidColOffset;
        if (wallsLayer >= 0) go.layer = wallsLayer;

        // 2. Trigger en capa HideSpot para detección de proximidad con Carlos
        GameObject trigGo = new GameObject("TriggerOcultarse");
        trigGo.transform.SetParent(go.transform, false);
        BoxCollider2D trigCol = trigGo.AddComponent<BoxCollider2D>();
        trigCol.isTrigger = true;
        trigCol.size = trigColSize;
        trigCol.offset = trigColOffset;
        if (hideSpotLayer >= 0) trigGo.layer = hideSpotLayer;

        // 3. Punto de reposo oculto de Carlos
        GameObject hp = new GameObject("HidePoint");
        hp.transform.SetParent(go.transform, false);
        hp.transform.localPosition = new Vector3(0f, solidColOffset.y + 0.15f, 0f);

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

        // Cápsula horizontal ceñida exactamente al cuerpo visible del charco para no dispararse en falso
        CapsuleCollider2D cc = go.AddComponent<CapsuleCollider2D>();
        cc.direction = CapsuleDirection2D.Horizontal;
        cc.isTrigger = true;
        cc.size = new Vector2(1.1f, 0.55f);
        cc.offset = new Vector2(0f, -0.05f);

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

        // Collider generoso a lo ancho de los fragmentos de cerámica para crujir con cualquier pisada
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(2.2f, 1.1f);
        bc.offset = new Vector2(0f, -0.1f);

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
