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
/// Reconstrucción fiel con arte real:
/// - Fachada monumental de la Parroquia de Nuestra Señora de los Dolores (Dolores Hidalgo, Gto.)
/// - Fuente octagonal central de cantera rosa
/// - Perímetro colonial con reja de hierro forjado y farolas de pie
/// - Escondites temáticos: Confesionario colonial, bancas de hierro, cripta y cipreses
/// - Trampas y decals: Charcos de agua bendita, tepalcates rotos, ofrendas de flores y veladoras coleccionables
/// - Salida monumental (Salida_2) al cementerio
/// </summary>
public static class Level3Builder
{
    const string ScenePath = "Assets/Scenes/Level3_Atrio.unity";

    [MenuItem("La Llorona/Construir Nivel 3")]
    public static void BuildLevel3Scene()
    {
        Debug.Log("[Level3Builder] Iniciando reconstrucción oficial del Nivel 3 (El Atrio de Dolores)...");

        // 1. Asegurar slicing y recarga de sprites oficiales
        SpriteSheetSlicer.SliceNivel3();
        AssetDatabase.Refresh();

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 3 Oficial");

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

        Sprite salida2Sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Salidas.png")
            .OfType<Sprite>().FirstOrDefault(s => s.name == "Salida_2") ??
            AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Salidas.png").OfType<Sprite>().ElementAtOrDefault(2);

        // 2. Grid y Suelo con Tilemap
        GameObject gridGo = GameObject.Find("Grid") ?? new GameObject("Grid");
        if (gridGo.GetComponent<Grid>() == null) gridGo.AddComponent<Grid>();

        Tilemap suelo = GetOrCreateTilemap(gridGo.transform, "Suelo", 0, -1, false);
        Tilemap paredes = GetOrCreateTilemap(gridGo.transform, "Paredes", 1, wallsLayer, true);
        suelo.ClearAllTiles();
        paredes.ClearAllTiles();

        TileBase[] allTiles = LoadTiles();
        // Llenar el atrio: X de -2 a 42, Y de 0 a 24
        for (int x = -2; x <= 42; x++)
        {
            for (int y = 0; y <= 24; y++)
            {
                // Alternancia de baldosas de cantera y empedrado colonial
                TileBase t = ((x + y) % 5 == 0) ? allTiles[1] : allTiles[0];
                suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // Limpiar ra�ces obsoletas del Nivel 3 previo
        string[] oldRoots = new string[] { "Props", "Hazards", "Barricadas", "Decoracion", "Luces", "Limites" };
        foreach (var r in oldRoots)
        {
            GameObject ex = GameObject.Find(r);
            if (ex != null) Undo.DestroyObjectImmediate(ex);
        }

        // 3. Fachada Monumental de la Parroquia al Norte (X: 18.0f, Y: 21.0f)
        Transform bgGroup = FreshRoot("Fondo_Monumental");
        if (catedralSprite != null)
        {
            GameObject catGo = new GameObject("Parroquia_Dolores_Hidalgo");
            catGo.transform.SetParent(bgGroup, false);
            catGo.transform.position = new Vector3(18.0f, 21.5f, 0f);
            catGo.transform.localScale = Vector3.one * 1.55f; // Monumental e imponente

            SpriteRenderer srCat = catGo.AddComponent<SpriteRenderer>();
            srCat.sprite = catedralSprite;
            srCat.sortingOrder = 2; // Fondo elevado
            srCat.color = new Color(0.95f, 0.92f, 0.98f);

            // Muro de colisión que protege la base de la catedral
            BoxCollider2D catCol = catGo.AddComponent<BoxCollider2D>();
            catCol.size = new Vector2(14.0f, 3.5f);
            catCol.offset = new Vector2(0f, 1.8f);
            if (wallsLayer >= 0) catGo.layer = wallsLayer;

            // Iluminación cálida de entrada a la parroquia
            GameObject lCat = new GameObject("Luz_Portal_Catedral");
            lCat.transform.SetParent(catGo.transform, false);
            lCat.transform.localPosition = new Vector3(0f, -3.2f, 0f);
            Light2D l2dCat = lCat.AddComponent<Light2D>();
            l2dCat.lightType = Light2D.LightType.Point;
            l2dCat.color = new Color(1.0f, 0.78f, 0.45f);
            l2dCat.intensity = 1.8f;
            l2dCat.pointLightOuterRadius = 7.5f;
            lCat.AddComponent<LightFlicker>();
        }

        // 4. Perímetro y Rejas (Límites cerrados para que Carlos no pueda salirse)
        Transform rejasGroup = FreshRoot("Rejas_Perimetro");
        Sprite rejaSup = rejaSprites.FirstOrDefault(s => s.name == "Reja_Superior") ?? rejaSprites.ElementAtOrDefault(0);
        Sprite rejaInf = rejaSprites.FirstOrDefault(s => s.name == "Reja_Inferior") ?? rejaSprites.ElementAtOrDefault(6);
        Sprite farolaPie = rejaSprites.FirstOrDefault(s => s.name == "Farola_Pie_N3") ?? rejaSprites.ElementAtOrDefault(5);

        // Reja sur inferior (de X = 0 a 36 en Y = 1.5f)
        for (float rx = 1.5f; rx <= 36.5f; rx += 7.0f)
        {
            CreateFencePiece(rejasGroup, $"Reja_Sur_{rx:0}", rejaInf, rx, 1.2f, wallsLayer, new Vector2(6.5f, 1.2f));
        }

        // Farolas de pie en el perímetro y plazas
        float[] farolasX = new float[] { 2.0f, 10.0f, 26.0f, 34.0f };
        foreach (float fx in farolasX)
        {
            CreateStandingLantern(rejasGroup, $"Farola_Sur_{fx:0}", farolaPie, fx, 2.5f, wallsLayer);
            CreateStandingLantern(rejasGroup, $"Farola_Norte_{fx:0}", farolaPie, fx, 17.5f, wallsLayer);
        }

        // Límites invisibles sólidos en los 4 extremos
        Transform limits = FreshRoot("Limites_Atrio");
        CreateInvisibleWall(limits, "Lim_Oeste",  -1.5f, 12.0f, 26f, wallsLayer);
        CreateInvisibleWall(limits, "Lim_Este_Sup", 38.5f, 19.0f, 12f, wallsLayer);
        CreateInvisibleWall(limits, "Lim_Este_Inf", 38.5f,  5.0f, 12f, wallsLayer);
        CreateInvisibleWall(limits, "Lim_Sur",    18.0f,  0.2f,  1f, wallsLayer, 42.0f);
        CreateInvisibleWall(limits, "Lim_Norte",  18.0f, 24.5f,  1f, wallsLayer, 42.0f);

        // 5. Fuente Central Monumental (X: 18.0f, Y: 11.5f)
        Transform props = FreshRoot("Props_Atrio");
        Sprite fuenteSprite = fuenteSprites.FirstOrDefault(s => s.name == "Fuente_Octagonal_N3") ?? fuenteSprites.ElementAtOrDefault(0);
        if (fuenteSprite != null)
        {
            GameObject fGo = new GameObject("Fuente_Octagonal_Central");
            fGo.transform.SetParent(props, false);
            fGo.transform.position = new Vector3(18.0f, 11.5f, 0f);
            fGo.transform.localScale = Vector3.one * 1.15f;

            SpriteRenderer srF = fGo.AddComponent<SpriteRenderer>();
            srF.sprite = fuenteSprite;
            srF.sortingOrder = 3;

            CircleCollider2D cc = fGo.AddComponent<CircleCollider2D>();
            cc.radius = 2.6f;
            cc.offset = new Vector2(0f, -0.2f);
            if (wallsLayer >= 0) fGo.layer = wallsLayer;

            // Luz suave azulada reflejada en el agua
            GameObject lf = new GameObject("Luz_Agua_Fuente");
            lf.transform.SetParent(fGo.transform, false);
            Light2D l2df = lf.AddComponent<Light2D>();
            l2df.lightType = Light2D.LightType.Point;
            l2df.color = new Color(0.45f, 0.75f, 1.0f);
            l2df.intensity = 1.1f;
            l2df.pointLightOuterRadius = 5.2f;
        }

        // 6. Escondites Temáticos Oficiales ([E])
        Sprite sConfesionario = accSprites.FirstOrDefault(s => s.name == "Confesionario_N3") ?? accSprites.ElementAtOrDefault(1);
        Sprite sBanca         = accSprites.FirstOrDefault(s => s.name == "Banca_Hierro_N3") ?? accSprites.ElementAtOrDefault(0);
        Sprite sCripta        = accSprites.FirstOrDefault(s => s.name == "Cripta_Cantera_N3") ?? accSprites.ElementAtOrDefault(2);
        Sprite sCipres        = accSprites.FirstOrDefault(s => s.name == "Cipres_Colonial_1") ?? accSprites.ElementAtOrDefault(4);
        Sprite sAltar         = accSprites.FirstOrDefault(s => s.name == "Altar_Atrio_N3") ?? accSprites.ElementAtOrDefault(8);

        // Confesionarios coloniales
        CreateHideSpotOfficial(props, "Escondite_Confesionario_Oeste", sConfesionario,  7.0f, 15.5f, 1.15f, hideSpotLayer);
        CreateHideSpotOfficial(props, "Escondite_Confesionario_Este",  sConfesionario, 29.0f, 15.5f, 1.15f, hideSpotLayer);

        // Bancas de hierro forjado
        CreateHideSpotOfficial(props, "Escondite_Banca_SurOeste", sBanca,  9.5f, 6.5f, 1.1f, hideSpotLayer);
        CreateHideSpotOfficial(props, "Escondite_Banca_SurEste",  sBanca, 26.5f, 6.5f, 1.1f, hideSpotLayer);

        // Criptas de cantera
        CreateHideSpotOfficial(props, "Escondite_Cripta_NorteOeste", sCripta,  6.0f, 10.0f, 1.1f, hideSpotLayer);
        CreateHideSpotOfficial(props, "Escondite_Cripta_NorteEste",  sCripta, 30.0f, 10.0f, 1.1f, hideSpotLayer);

        // Altar mayor al aire libre con veladoras
        if (sAltar != null)
        {
            CreateSolidProp(props, "Altar_Atrio", sAltar, 18.0f, 17.5f, 1.15f, wallsLayer, new Vector2(4.8f, 1.8f), new Vector2(0f, 0.9f));
            // Luz de velas en el altar
            GameObject lAltar = new GameObject("Luz_Altar");
            lAltar.transform.SetParent(props, false);
            lAltar.transform.position = new Vector3(18.0f, 18.0f, 0f);
            Light2D l2dAltar = lAltar.AddComponent<Light2D>();
            l2dAltar.lightType = Light2D.LightType.Point;
            l2dAltar.color = new Color(1.0f, 0.70f, 0.25f);
            l2dAltar.intensity = 1.6f;
            l2dAltar.pointLightOuterRadius = 4.8f;
            lAltar.AddComponent<LightFlicker>();
        }

        // Cipreses decorativos
        if (sCipres != null)
        {
            float[] cipresesX = new float[] { 3.5f, 5.0f, 31.0f, 32.5f };
            foreach (float cx in cipresesX)
            {
                CreateSolidProp(props, $"Cipres_{cx:0}", sCipres, cx, 12.0f, 1.25f, wallsLayer, new Vector2(0.9f, 0.8f), new Vector2(0f, 0.45f));
            }
        }

        // 7. Decals, Obstáculos y Coleccionables
        Transform hazards = FreshRoot("Obstaculos_Ofrendas");
        Sprite sAguaBendita = ofrendaSprites.FirstOrDefault(s => s.name == "Charco_AguaBendita") ?? ofrendaSprites.ElementAtOrDefault(0);
        Sprite sTepalcate   = ofrendaSprites.FirstOrDefault(s => s.name == "Vidrios_Jarron_Roto") ?? ofrendaSprites.ElementAtOrDefault(5);
        Sprite sSahumador   = ofrendaSprites.FirstOrDefault(s => s.name == "Sahumador_Roto") ?? ofrendaSprites.ElementAtOrDefault(6);
        Sprite sVelaRoja    = ofrendaSprites.FirstOrDefault(s => s.name == "Vaso_Vela_Roja_1") ?? ofrendaSprites.ElementAtOrDefault(1);

        // Charcos de agua bendita alrededor de la fuente
        CreateMudPuddleOfficial(hazards, sAguaBendita, 13.5f, 12.5f, 0.85f);
        CreateMudPuddleOfficial(hazards, sAguaBendita, 22.5f, 12.5f, 0.85f);
        CreateMudPuddleOfficial(hazards, sAguaBendita, 18.0f,  6.8f, 0.95f);
        CreateMudPuddleOfficial(hazards, sAguaBendita, 18.0f, 15.2f, 0.95f);

        // Trampas de ruido (Tepalcates y sahumador roto alertan a La Llorona)
        CreateNoiseObstacleOfficial(hazards, sTepalcate, 13.0f,  9.0f);
        CreateNoiseObstacleOfficial(hazards, sSahumador, 23.0f,  9.0f);
        CreateNoiseObstacleOfficial(hazards, sTepalcate, 13.0f, 14.0f);
        CreateNoiseObstacleOfficial(hazards, sTepalcate, 23.0f, 14.0f);

        // Velas coleccionables de ofrenda (para distraer con [F] a La Llorona)
        CreateCandlePickupOfficial(hazards, "Vela_Ofrenda_1", sVelaRoja, new Vector2( 7.0f,  8.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Ofrenda_2", sVelaRoja, new Vector2(18.0f,  4.5f));
        CreateCandlePickupOfficial(hazards, "Vela_Ofrenda_3", sVelaRoja, new Vector2(29.0f,  8.0f));

        // 8. Perro Callejero (duerme custodiando una esquina de velas)
        CreateStrayDog(hazards, 28.5f, 4.2f);

        // 9. Waypoints de Patrulla Circular para La Llorona (Rodeando la fuente)
        Transform wpRoot = FreshRoot("Waypoints");
        Transform[] wps = new Transform[6];
        wps[0] = CreateEmpty(wpRoot, "WP_Norte",      18.0f, 16.5f);
        wps[1] = CreateEmpty(wpRoot, "WP_Noreste",    23.5f, 14.5f);
        wps[2] = CreateEmpty(wpRoot, "WP_Sureste",    23.5f,  8.5f);
        wps[3] = CreateEmpty(wpRoot, "WP_Sur",        18.0f,  6.5f);
        wps[4] = CreateEmpty(wpRoot, "WP_Suroeste",   12.5f,  8.5f);
        wps[5] = CreateEmpty(wpRoot, "WP_Noroeste",   12.5f, 14.5f);

        // 10. Salida Monumental al Cementerio (Salida_2)
        Transform exitGroup = FreshRoot("Salida_Meta");
        CreateExitPortal(exitGroup, 36.5f, 11.5f, salida2Sprite);

        // 11. Iluminación Global URP 2D (Noche en el atrio: 0.50f)
        EnsureGlobalLight(0.50f);

        // 12. Player (Carlos inicia al oeste del atrio)
        SetupPlayer(new Vector2(2.5f, 11.5f), playerLayer);

        // 13. La Llorona (Posición inicial sobre el WP_Norte)
        SetupLloronaCircular(new Vector2(18.0f, 16.5f), wps, wallsLayer, lloronaLayer, 1.6f, 2.8f, 5.5f, 110.0f);

        // 14. Cámara y Confiner 2D
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(2.5f, 11.5f, -10f);
        cam.backgroundColor = new Color(0.04f, 0.03f, 0.08f);
        CameraFollow2D camFollow = cam.GetComponent<CameraFollow2D>() ?? cam.gameObject.AddComponent<CameraFollow2D>();
        camFollow.SetBounds(new Vector2(7.0f, 6.5f), new Vector2(30.0f, 16.5f));

        // 15. Triggers de Tutorial narrativo
        Transform tutRoot = FreshRoot("TutorialTriggers");
        CreateTutorialTrigger(tutRoot, "Tutorial_Inicio",
            new Vector2(2.5f, 11.5f), new Vector2(4f, 8f),
            "¡El Atrio de la Parroquia de Dolores!\n• Camina sobre las veladoras para recogerlas\n• Presiona [F] para orar y distraer a La Llorona\n• Presiona [E] para esconderte en confesionarios y bancas",
            showOnStart: true);
        CreateTutorialTrigger(tutRoot, "Tutorial_Fuente",
            new Vector2(13.0f, 11.5f), new Vector2(3f, 6f),
            "La Llorona ronda en circulos por la fuente.\nUsa los confesionarios y bancas para ocultarte con [E].");
        CreateTutorialTrigger(tutRoot, "Tutorial_Salida",
            new Vector2(33.0f, 11.5f), new Vector2(3f, 8f),
            "¡El porton al Panteon de las Animas esta abierto!\nSigue las flores de cempasuchil hacia el Porton Este para escapar.");

        // 16. GameManager y AudioManager
        SetupGameManager(3);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.SaveScene(scene, ScenePath);
        UIBuilder.BuildLevel3UICanvas();
        Debug.Log("[Level3Builder] ¡Nivel 3 oficial (El Atrio de Dolores) reconstruido y guardado con éxito!");
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

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.35f;
        col.offset = new Vector2(0f, 0.35f);
        if (layer >= 0) go.layer = layer;

        GameObject lightGo = new GameObject("Luz");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.82f, 0.45f);
        l2d.intensity = 1.4f;
        l2d.pointLightOuterRadius = 5.2f;

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

        // Base solida física en layer Walls (impide que Carlos la atraviese)
        BoxCollider2D solidCol = go.AddComponent<BoxCollider2D>();
        if (name.Contains("Banca"))
        {
            solidCol.size = new Vector2(2.6f, 0.8f);
            solidCol.offset = new Vector2(0f, 0.45f);
        }
        else if (name.Contains("Confesionario"))
        {
            solidCol.size = new Vector2(2.0f, 1.4f);
            solidCol.offset = new Vector2(0f, 0.75f);
        }
        else // Cripta
        {
            solidCol.size = new Vector2(2.6f, 1.2f);
            solidCol.offset = new Vector2(0f, 0.65f);
        }
        int wallsLayer = LayerMask.NameToLayer("Walls");
        if (wallsLayer >= 0) go.layer = wallsLayer;

        // Trigger de proximidad como hijo en layer HideSpot para activar [C] Esconderse
        GameObject trigGo = new GameObject("Trigger_Escondite");
        trigGo.transform.SetParent(go.transform, false);
        if (layer >= 0) trigGo.layer = layer;
        BoxCollider2D trigCol = trigGo.AddComponent<BoxCollider2D>();
        trigCol.isTrigger = true;
        trigCol.size = solidCol.size + new Vector2(1.2f, 1.2f);
        trigCol.offset = solidCol.offset;

        GameObject hp = new GameObject("HidePoint");
        hp.transform.SetParent(go.transform, false);
        hp.transform.localPosition = new Vector3(0f, solidCol.offset.y + 0.2f, 0f);

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

    static void CreateStrayDog(Transform parent, float x, float y)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Perro/perro.png").OfType<Sprite>().FirstOrDefault();
        GameObject go = new GameObject("Perro_Callejero");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 1.0f;

        go.AddComponent<StrayDogAI>();
        Undo.RegisterCreatedObjectUndo(go, "Perro_Callejero");
    }

    static void CreateExitPortal(Transform parent, float x, float y, Sprite customSprite)
    {
        GameObject go = new GameObject("Portal_Salida_Meta");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = customSprite;
        sr.sortingOrder = 4;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(3.5f, 5.0f);
        go.AddComponent<LevelGoal>();

        GameObject lightGo = new GameObject("LuzSalida");
        lightGo.transform.SetParent(go.transform, false);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.3f, 1.0f, 0.5f);
        l2d.intensity = 2.2f;
        l2d.pointLightOuterRadius = 5.5f;
        lightGo.AddComponent<LightFlicker>();

        GameObject labelGo = new GameObject("LabelSalida");
        labelGo.transform.SetParent(go.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 3.2f, 0f);
        TextMesh tm = labelGo.AddComponent<TextMesh>();
        tm.text = "SALIDA AL PANTEON ➔";
        tm.fontSize = 32;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.35f, 1f, 0.55f);
        MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;

        Undo.RegisterCreatedObjectUndo(go, "Portal_Salida_Meta");
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
        so.FindProperty("tutorialMessage").stringValue = text;
        so.FindProperty("displayDuration").floatValue  = showOnStart ? 5.0f : 4.5f;
        so.FindProperty("showOnStart").boolValue       = showOnStart;
        so.ApplyModifiedProperties();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void SetupPlayer(Vector2 pos, int playerLayer)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = new GameObject("Player");
            player.tag = "Player";
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            player.AddComponent<CircleCollider2D>().radius = 0.4f;
            player.AddComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/Carlos.png");
            player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerAnimation>();
            player.AddComponent<HideSystem>();
        }
        player.transform.position = new Vector3(pos.x, pos.y, 0f);
        player.transform.rotation = Quaternion.identity;
        var prb = player.GetComponent<Rigidbody2D>();
        if (prb != null) { prb.freezeRotation = true; prb.gravityScale = 0f; }
        if (playerLayer >= 0) player.layer = playerLayer;
    }

    static void SetupLloronaCircular(Vector2 pos, Transform[] wps, int wallsLayer, int lloronaLayer, float pSpeed, float cSpeed, float vDist, float vAngle)
    {
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona == null)
        {
            GameObject lGo = new GameObject("Llorona");
            llorona = lGo.AddComponent<LloronaAI>();
        }
        Undo.RecordObject(llorona.transform, "Posicionar Llorona");
        llorona.transform.position = new Vector3(pos.x, pos.y, 0f);
        llorona.transform.rotation = Quaternion.identity;
        var lrb = llorona.GetComponent<Rigidbody2D>();
        if (lrb != null) { lrb.freezeRotation = true; lrb.gravityScale = 0f; }
        if (lloronaLayer >= 0) llorona.gameObject.layer = lloronaLayer;

        SerializedObject so = new SerializedObject(llorona);
        SerializedProperty wpProp = so.FindProperty("waypoints");
        if (wpProp != null)
        {
            wpProp.arraySize = wps.Length;
            for (int i = 0; i < wps.Length; i++)
                wpProp.GetArrayElementAtIndex(i).objectReferenceValue = wps[i];
        }
        so.FindProperty("patrolSpeed").floatValue  = pSpeed;
        so.FindProperty("chaseSpeed").floatValue   = cSpeed;
        so.FindProperty("viewDistance").floatValue = vDist;
        so.FindProperty("viewAngle").floatValue    = vAngle;

        LloronaSettings s3 = AssetDatabase.LoadAssetAtPath<LloronaSettings>("Assets/Settings/LloronaSettings_Level3.asset");
        if (s3 != null) so.FindProperty("settings").objectReferenceValue = s3;

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

    static void SetupGameManager(int lvl)
    {
        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject old = GameObject.Find("GameManager");
            if (old != null) Undo.DestroyObjectImmediate(old);

            GameObject gmGo = new GameObject("GameManager");
            gm = gmGo.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gmGo, "GameManager");
        }
        gm.SetLevelNumber(lvl);
        EditorUtility.SetDirty(gm);
    }
}

