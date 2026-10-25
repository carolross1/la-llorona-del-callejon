// Assets/Editor/Level4Builder.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Constructor automatizado del Nivel 4: El Panteón de las Ánimas (Nivel Final).
/// Escenario de 38 x 28. Incluye criptas, tumbas, obstáculos de lodo, botellas ruidosas,
/// perro callejero guardián, y la puerta/portal de escape a la casa de Carlos.
/// </summary>
public static class Level4Builder
{
    const string ScenePath = "Assets/Scenes/Level4_Cementerio.unity";

    [MenuItem("La Llorona/Construir Nivel 4")]
    public static void BuildLevel4Scene()
    {
        Debug.Log("[Level4Builder] Construyendo Nivel 4: El Panteón de las Ánimas...");

        SpriteAssetGenerator.GenerateAllSprites();

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 4");

        int wallsLayer     = LayerMask.NameToLayer("Walls");
        int hideSpotLayer  = LayerMask.NameToLayer("HideSpot");
        int playerLayer    = LayerMask.NameToLayer("Player");
        int lloronaLayer   = LayerMask.NameToLayer("Llorona");

        TileBase[] allTiles    = LoadTiles();
        Sprite[] hideSprites   = LoadSubSprites("Assets/Sprites/Environment/escondites.png");
        Sprite[] decorSprites  = LoadSubSprites("Assets/Sprites/Environment/decoracion.png");
        Sprite velaSprite      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/vela.png");

        // ──── 1. Grid y Tilemaps ───────────────────────────────────────────────
        GameObject gridGo = GameObject.Find("Grid") ?? new GameObject("Grid");
        if (gridGo.GetComponent<Grid>() == null) gridGo.AddComponent<Grid>();

        Tilemap suelo   = GetOrCreateTilemap(gridGo.transform, "Suelo",   0, -1,         false);
        Tilemap paredes = GetOrCreateTilemap(gridGo.transform, "Paredes", 1, wallsLayer, true);
        suelo.ClearAllTiles();
        paredes.ClearAllTiles();

        // ──── 2. Construcción: 38 x 28 ─────────────────────────────────────────
        TileBase floorTile  = allTiles.Length > 12 ? allTiles[12] : allTiles[0];
        TileBase floorDark  = allTiles.Length > 14 ? allTiles[14] : floorTile;
        TileBase wallTile   = allTiles.Length > 24 ? allTiles[24] : allTiles[0];

        // Suelo completo con variación de tierra y adoquín húmedo
        for (int x = 0; x <= 38; x++)
        {
            for (int y = 0; y <= 28; y++)
            {
                TileBase t = ((x + y * 3) % 5 == 0) ? floorDark : floorTile;
                suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // Muros perimetrales (con apertura este en Y=12..14 para la salida)
        for (int x = 0; x <= 38; x++)
        {
            paredes.SetTile(new Vector3Int(x, 28, 0), wallTile);
            paredes.SetTile(new Vector3Int(x, 27, 0), wallTile);
            paredes.SetTile(new Vector3Int(x, 0,  0), wallTile);
            paredes.SetTile(new Vector3Int(x, 1,  0), wallTile);
        }
        for (int y = 0; y <= 28; y++)
        {
            paredes.SetTile(new Vector3Int(0,  y, 0), wallTile);
            paredes.SetTile(new Vector3Int(1,  y, 0), wallTile);

            if (y < 11 || y > 15)
            {
                paredes.SetTile(new Vector3Int(37, y, 0), wallTile);
                paredes.SetTile(new Vector3Int(38, y, 0), wallTile);
            }
        }

        // Criptas interiores
        PaintWallRect(paredes, wallTile, 4, 18, 8, 22);
        PaintWallRect(paredes, wallTile, 14, 20, 18, 24);
        PaintWallRect(paredes, wallTile, 25, 14, 29, 18);
        PaintWallRect(paredes, wallTile, 3, 5, 6, 9);

        ConfigureCompositeCollider(gridGo.transform.Find("Paredes").gameObject);

        // ──── 3. Iluminación URP 2D (visible y atmosférica) ────────────────────
        EnsureGlobalLight(0.44f);
        Transform luces = FreshRoot("Luces");
        CreateFarolTenue(luces, "FarolCripta1",  7f,  21f);
        CreateFarolTenue(luces, "FarolCripta2", 19f,  21f);
        CreateFarolTenue(luces, "FarolCripta3", 30f,  15f);
        CreateFarolTenue(luces, "FarolEntrada",  2f,  13f);
        CreateFarolTenue(luces, "FarolSalida",  36f,  13f);

        // ──── 4. Escondites ───────────────────────────────────────────────────
        Transform props = FreshRoot("Props");
        Sprite escBarril = hideSprites.Length > 1 ? hideSprites[1] : null;
        Sprite escCajas  = hideSprites.Length > 0 ? hideSprites[0] : null;
        Sprite escPuerta = hideSprites.Length > 2 ? hideSprites[2] : null;

        CreateHideSpot(props, "Escondite_Barril1",  escBarril,  7f, 12f, 0.83f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Cajas1",   escCajas,  20f, 12f, 0.83f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Puerta1",  escPuerta, 11f, 22f, 0.83f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Cajas2",   escCajas,  31f,  7f, 0.83f, hideSpotLayer);

        // ──── 5. Obstáculos con consecuencias y enemigo secundario ─────────────
        CreateMudPuddle(props, 12f, 13f, 1.2f);
        CreateMudPuddle(props, 26f, 13f, 1.2f);
        CreateNoiseObstacle(props, 17f, 18f);
        CreateNoiseObstacle(props, 24f, 8f);
        CreateStrayDog(props, 27f, 9f);

        // ──── 6. Tumbas decorativas ─────────────────────────────────────────────
        Sprite tumbaSprite = decorSprites.Length > 3 ? decorSprites[3] : (decorSprites.Length > 0 ? decorSprites[0] : null);
        int[][] tumbaPos = new int[][]
        {
            new int[]{8,7}, new int[]{10,7}, new int[]{12,7}, new int[]{8,10},
            new int[]{12,10}, new int[]{21,8}, new int[]{23,8}, new int[]{25,8},
            new int[]{21,11}, new int[]{23,11}, new int[]{32,18}, new int[]{34,18},
            new int[]{32,21}, new int[]{34,21}, new int[]{16,14}, new int[]{18,14}
        };
        for (int i = 0; i < tumbaPos.Length; i++)
        {
            CreateDecorSprite(props, $"Tumba_{i:00}", tumbaSprite, tumbaPos[i][0], tumbaPos[i][1]);
        }

        // ──── 7. Velas coleccionables ──────────────────────────────────────────
        Transform sistemasGo = FreshRoot("Sistemas");
        CreateCandlePickup(sistemasGo, "Vela_01", velaSprite, 9f,  15f);
        CreateCandlePickup(sistemasGo, "Vela_02", velaSprite, 22f, 17f);
        CreateCandlePickup(sistemasGo, "Vela_03", velaSprite, 33f, 10f);

        // ──── 8. Portal Monumental de Salida (La reja hacia casa) ──────────────
        CreateExitPortal(props, 36.5f, 13.0f);

        // ──── 9. Carlos (jugador) ───────────────────────────────────────────────
        SetupPlayer(new Vector2(2.5f, 13f), playerLayer);

        // ──── 10. La Llorona ────────────────────────────────────────────────────
        Transform wps = FreshRoot("Waypoints");
        Transform[] wpList = new Transform[]
        {
            CreateEmpty(wps, "WP_A",  6f,  13f),
            CreateEmpty(wps, "WP_B", 14f,  22f),
            CreateEmpty(wps, "WP_C", 24f,  22f),
            CreateEmpty(wps, "WP_D", 32f,  14f),
            CreateEmpty(wps, "WP_E", 24f,   5f),
            CreateEmpty(wps, "WP_F", 14f,   5f),
        };

        SetupLlorona(
            pos:        new Vector2(15f, 14f),
            waypoints:  wpList,
            wallsLayer: wallsLayer,
            lloronaLayer: lloronaLayer,
            pSpeed:     2.0f,
            cSpeed:     3.2f,
            vDist:      6.0f,
            vAngle:     115f
        );

        // ──── 11. GameManager ───────────────────────────────────────────────────
        SetupGameManager(4);

        // ──── 12. Tutorial triggers ─────────────────────────────────────────────
        Transform triggers = FreshRoot("Triggers");
        CreateTutorialTrigger(triggers, "TutFinal", new Vector2(2.5f, 13f), new Vector2(4f, 6f),
            "¡El Panteón! Estás a un paso de tu casa.\nUsa velas (F) para distraer a La Llorona y esquiva los charcos.",
            showOnStart: true);
        CreateTutorialTrigger(triggers, "TutNiebla", new Vector2(15f, 13f), new Vector2(3f, 4f),
            "La Llorona es implacable aquí. ¡Usa los escondites ante el menor lamento!");
        CreateTutorialTrigger(triggers, "TutSalida", new Vector2(32f, 13f), new Vector2(3f, 6f),
            "¡Ahí está la reja de salida! ¡Cruza hacia tu hogar!");

        // ──── 13. Cámara ────────────────────────────────────────────────────────
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            Undo.RegisterCreatedObjectUndo(camGo, "Main Camera");
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(2.5f, 13f, -10f);
        cam.backgroundColor = new Color(0.04f, 0.02f, 0.08f);

        CameraFollow2D follow = cam.GetComponent<CameraFollow2D>() ?? cam.gameObject.AddComponent<CameraFollow2D>();
        follow.SetBounds(new Vector2(8.8f, 7.0f), new Vector2(29.2f, 21.0f));

        // ──── 14. Guardar escena y Canvas ───────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        UIBuilder.BuildLevel4UICanvas();

        AssetDatabase.Refresh();
        Debug.Log("[Level4Builder] ¡Nivel 4 construido con éxito en " + ScenePath + "!");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    static TileBase[] LoadTiles()
    {
        TileBase[] tiles = new TileBase[48];
        for (int i = 0; i < 48; i++)
            tiles[i] = AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Tiles/tiles_{i}.asset");
        return tiles;
    }

    static Sprite[] LoadSubSprites(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();

    static Tilemap GetOrCreateTilemap(Transform parent, string name, int sortingOrder, int layer, bool hasCollider)
    {
        Transform child = parent.Find(name);
        GameObject go = (child != null) ? child.gameObject : new GameObject(name);
        if (child == null)
        {
            go.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(go, name);
        }
        Tilemap tm = go.GetComponent<Tilemap>() ?? go.AddComponent<Tilemap>();
        TilemapRenderer tr = go.GetComponent<TilemapRenderer>() ?? go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = sortingOrder;
        if (layer >= 0) go.layer = layer;
        if (hasCollider)
        {
            TilemapCollider2D tc = go.GetComponent<TilemapCollider2D>() ?? go.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
        }
        return tm;
    }

    static void PaintWallRect(Tilemap tm, TileBase tile, int xMin, int yMin, int xMax, int yMax)
    {
        for (int x = xMin; x <= xMax; x++)
            for (int y = yMin; y <= yMax; y++)
                tm.SetTile(new Vector3Int(x, y, 0), tile);
    }

    static void ConfigureCompositeCollider(GameObject paredesGo)
    {
        CompositeCollider2D comp = paredesGo.GetComponent<CompositeCollider2D>() ?? paredesGo.AddComponent<CompositeCollider2D>();
        comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
        Rigidbody2D rb = paredesGo.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.bodyType = RigidbodyType2D.Static; rb.simulated = true; }
    }

    static Transform FreshRoot(string name)
    {
        GameObject ex = GameObject.Find(name);
        if (ex != null) Undo.DestroyObjectImmediate(ex);
        GameObject g = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(g, name);
        return g.transform;
    }

    static void CreateHideSpot(Transform parent, string name, Sprite sprite, float x, float y, float scale, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        BoxCollider2D trig = go.AddComponent<BoxCollider2D>();
        trig.isTrigger = true;
        trig.size      = new Vector2(1.2f, 1.2f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size   = new Vector2(0.8f, 0.4f);
        col.offset = new Vector2(0f, -0.4f);

        GameObject hp = new GameObject("HidePoint");
        hp.transform.SetParent(go.transform, false);
        hp.transform.localPosition = new Vector3(0f, -0.1f, 0f);

        HideSpot hs = go.AddComponent<HideSpot>();
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("hidePoint").objectReferenceValue = hp.transform;
        so.ApplyModifiedProperties();

        if (layer >= 0) go.layer = layer;
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateDecorSprite(Transform parent, string name, Sprite sprite, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateCandlePickup(Transform parent, string name, Sprite sprite, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.5f;

        GameObject lightGo = new GameObject("LuzVela");
        lightGo.transform.SetParent(go.transform, false);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType          = Light2D.LightType.Point;
        l2d.color              = new Color(1f, 0.75f, 0.35f);
        l2d.intensity          = 1.1f;
        l2d.pointLightOuterRadius = 3.0f;

        go.AddComponent<Candle>();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateMudPuddle(Transform parent, float x, float y, float scale = 1.0f)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/charco_lodo.png");
        GameObject go = new GameObject("Charco_Lodo");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 1.1f;

        go.AddComponent<MudPuddle>();
        Undo.RegisterCreatedObjectUndo(go, "Charco_Lodo");
    }

    static void CreateNoiseObstacle(Transform parent, float x, float y)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/basura_ruido.png");
        GameObject go = new GameObject("Basura_Ruido");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(1.8f, 1.0f);

        go.AddComponent<NoiseObstacle>();
        Undo.RegisterCreatedObjectUndo(go, "Basura_Ruido");
    }

    static void CreateStrayDog(Transform parent, float x, float y)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/perro_callejero.png");
        GameObject go = new GameObject("Perro_Callejero");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.9f;

        go.AddComponent<StrayDogAI>();

        GameObject eyeLight = new GameObject("LuzOjos");
        eyeLight.transform.SetParent(go.transform, false);
        Light2D l2d = eyeLight.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.25f, 0.15f);
        l2d.intensity = 0.9f;
        l2d.pointLightOuterRadius = 1.8f;

        Undo.RegisterCreatedObjectUndo(go, "Perro_Callejero");
    }

    static void CreateExitPortal(Transform parent, float x, float y)
    {
        Sprite portalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/portal_meta.png");

        GameObject go = new GameObject("Portal_Salida_Meta");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = portalSprite;
        sr.sortingOrder = 4;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(3.5f, 4.5f);
        go.AddComponent<LevelGoal>();

        GameObject lightGo = new GameObject("LuzSalida");
        lightGo.transform.SetParent(go.transform, false);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.2f, 1.0f, 0.45f);
        l2d.intensity = 2.4f;
        l2d.pointLightOuterRadius = 5.5f;
        lightGo.AddComponent<LightFlicker>();

        GameObject labelGo = new GameObject("LabelSalida");
        labelGo.transform.SetParent(go.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 2.8f, 0f);
        TextMesh tm = labelGo.AddComponent<TextMesh>();
        tm.text = "¡CASA (SALIDA)! ➔";
        tm.fontSize = 30;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.35f, 1f, 0.55f);
        MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;

        Undo.RegisterCreatedObjectUndo(go, "Portal_Salida_Meta");
    }

    static void CreateFarolTenue(Transform parent, string name, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        Light2D l2d = go.AddComponent<Light2D>();
        l2d.lightType          = Light2D.LightType.Point;
        l2d.color              = new Color(0.55f, 0.45f, 0.95f);
        l2d.intensity          = 1.1f;
        l2d.pointLightOuterRadius = 4.5f;

        go.AddComponent<LightFlicker>();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void EnsureGlobalLight(float intensity)
    {
        GameObject glGo = GameObject.Find("Global Light 2D");
        if (glGo == null) glGo = new GameObject("Global Light 2D");
        Light2D gl = glGo.GetComponent<Light2D>() ?? glGo.AddComponent<Light2D>();
        gl.lightType  = Light2D.LightType.Global;
        gl.color      = new Color(0.85f, 0.82f, 0.95f);
        gl.intensity  = intensity;
    }

    static void CreateTutorialTrigger(Transform parent, string name, Vector2 pos, Vector2 size, string text, bool showOnStart = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size      = size;

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
            player.AddComponent<CircleCollider2D>().radius = 0.4f;
            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/Carlos.png");
            sr.sortingOrder = 5;
            player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerAnimation>();
            player.AddComponent<HideSystem>();
        }
        player.transform.position = new Vector3(pos.x, pos.y, 0f);
        if (playerLayer >= 0) player.layer = playerLayer;
    }

    static void SetupLlorona(Vector2 pos, Transform[] waypoints, int wallsLayer, int lloronaLayer,
                              float pSpeed, float cSpeed, float vDist, float vAngle)
    {
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona == null)
        {
            GameObject lGo = new GameObject("Llorona");
            Rigidbody2D rb = lGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            lGo.AddComponent<CircleCollider2D>().radius = 0.5f;
            SpriteRenderer sr = lGo.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Llorona/llorona.png");
            sr.sortingOrder = 5;
            llorona = lGo.AddComponent<LloronaAI>();
            lGo.AddComponent<LloronaAnimation>();
        }
        llorona.transform.position = new Vector3(pos.x, pos.y, 0f);
        if (lloronaLayer >= 0) llorona.gameObject.layer = lloronaLayer;

        SerializedObject so = new SerializedObject(llorona);
        SerializedProperty wps = so.FindProperty("waypoints");
        wps.arraySize = waypoints.Length;
        for (int i = 0; i < waypoints.Length; i++)
            wps.GetArrayElementAtIndex(i).objectReferenceValue = waypoints[i];

        if (wallsLayer >= 0) so.FindProperty("obstacleMask").intValue = 1 << wallsLayer;

        LloronaSettings s = AssetDatabase.LoadAssetAtPath<LloronaSettings>("Assets/Settings/LloronaSettings_Level4.asset");
        if (s != null) so.FindProperty("settings").objectReferenceValue = s;

        so.FindProperty("patrolSpeed").floatValue  = pSpeed;
        so.FindProperty("chaseSpeed").floatValue   = cSpeed;
        so.FindProperty("viewDistance").floatValue = vDist;
        so.FindProperty("viewAngle").floatValue    = vAngle;
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

    static void SetupGameManager(int levelNum)
    {
        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject old = GameObject.Find("GameManager");
            if (old != null) Undo.DestroyObjectImmediate(old);

            GameObject gmGo = new GameObject("GameManager");
            gm = gmGo.AddComponent<GameManager>();
        }
        gm.SetLevelNumber(levelNum);
        EditorUtility.SetDirty(gm);
    }
}
