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
/// Constructor automatizado del Nivel 4: "El Panteón de las Ánimas".
/// Cementerio con tumbas, criptas y niebla; la casa de Carlos al fondo.
/// La Llorona es más rápida y persigue activamente. Una ofrenda la detiene 3 s.
/// Grito final y cinemática de cierre al llegar a la puerta de la casa.
///
/// Ejecución:
/// Menú Unity: La Llorona > Construir Nivel 4 (Cementerio)
/// </summary>
public static class Level4Builder
{
    const string ScenePath = "Assets/Scenes/Level4_Cementerio.unity";

    [MenuItem("La Llorona/Construir Nivel 4 (Cementerio)")]
    public static void BuildLevel4Scene()
    {
        Debug.Log("[Level4Builder] Construyendo Nivel 4: El Panteón de las Ánimas...");
        Directory.CreateDirectory("Assets/Scenes");

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 4");

        int wallsLayer     = LayerMask.NameToLayer("Walls");
        int hideSpotLayer  = LayerMask.NameToLayer("HideSpot");
        int playerLayer    = LayerMask.NameToLayer("Player");
        int lloronaLayer   = LayerMask.NameToLayer("Llorona");

        if (wallsLayer    < 0) wallsLayer    = 0;
        if (hideSpotLayer < 0) hideSpotLayer = 0;

        TileBase[] allTiles  = LoadTiles();
        Sprite[] hideSprites = LoadSubSprites("Assets/Sprites/Environment/escondites.png");
        Sprite[] decorSprites= LoadSubSprites("Assets/Sprites/Environment/decoracion.png");
        Sprite velaSprite    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/vela.png");

        // ──── 1. Grid y Tilemaps ────────────────────────────────────────────────
        GameObject gridGo = GameObject.Find("Grid");
        if (gridGo == null)
        {
            gridGo = new GameObject("Grid");
            gridGo.AddComponent<Grid>();
            Undo.RegisterCreatedObjectUndo(gridGo, "Grid");
        }

        Tilemap suelo   = GetOrCreateTilemap(gridGo.transform, "Suelo",  0, -1, false);
        Tilemap paredes = GetOrCreateTilemap(gridGo.transform, "Paredes", 1, wallsLayer, true);
        suelo.ClearAllTiles();
        paredes.ClearAllTiles();

        // ──── 2. Layout del Cementerio (38 × 28 unidades) ──────────────────────
        // Paleta oscura: tiles 0-11 adoquín (suelo), 24-35 muro concreto (paredes)
        // Estilo: charcos frecuentes para dar atmósfera sombría
        TileBase floorTile  = allTiles.Length > 0  ? allTiles[0]  : null;
        TileBase puddleTile = allTiles.Length > 6  ? allTiles[6]  : null;
        TileBase wallTile   = allTiles.Length > 24 ? allTiles[24] : null;
        TileBase wallTop    = allTiles.Length > 36 ? allTiles[36] : null;

        System.Random rnd = new System.Random(404);

        // Suelo del cementerio (zona jugable 2..25 en Y, 0..37 en X)
        for (int x = 0; x < 38; x++)
        {
            for (int y = 2; y <= 25; y++)
            {
                // Más charcos para atmosfera sombría (25%)
                TileBase t = (rnd.NextDouble() < 0.25) ? puddleTile : floorTile;
                if (t != null) suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // Borde superior (fachadas y muros)
        for (int x = 0; x < 38; x++)
        {
            if (wallTop  != null) paredes.SetTile(new Vector3Int(x, 26, 0), wallTop);
            if (wallTile != null) paredes.SetTile(new Vector3Int(x,  1, 0), wallTile);
        }
        // Borde izquierdo y derecho
        for (int y = 2; y <= 25; y++)
        {
            if (wallTile != null)
            {
                paredes.SetTile(new Vector3Int(-1,  y, 0), wallTile);
                paredes.SetTile(new Vector3Int(38,  y, 0), wallTile);
            }
        }

        // Bloques de criptas interiores (obstáculos + escondites)
        // Cripta 1: esquina noroeste
        PaintWallRect(paredes, wallTile, 3, 20, 6, 24);
        // Cripta 2: centro norte
        PaintWallRect(paredes, wallTile, 14, 20, 18, 24);
        // Cripta 3: centro-este
        PaintWallRect(paredes, wallTile, 25, 14, 29, 18);
        // Cripta 4: esquina suroeste (pequeña)
        PaintWallRect(paredes, wallTile, 3, 5, 6, 9);

        ConfigureCompositeCollider(gridGo.transform.Find("Paredes").gameObject);

        // ──── 3. Iluminación: muy oscura (horror máximo) ───────────────────────
        EnsureGlobalLight(0.08f); // Casi negro
        // Faroles tenues en las criptas
        Transform luces = FreshRoot("Luces");
        CreateFarolTenue(luces, "FarolCripta1",  7f,  21f);
        CreateFarolTenue(luces, "FarolCripta2", 19f,  21f);
        CreateFarolTenue(luces, "FarolCripta3", 30f,  15f);
        CreateFarolTenue(luces, "FarolEntrada",  1f,  13f);
        CreateFarolTenue(luces, "FarolSalida",  36f,  13f);

        // ──── 4. Escondites (4 en nivel final) ─────────────────────────────────
        Transform props = FreshRoot("Props");
        Sprite escBarril = hideSprites.Length > 1 ? hideSprites[1] : null;
        Sprite escCajas  = hideSprites.Length > 0 ? hideSprites[0] : null;
        Sprite escPuerta = hideSprites.Length > 2 ? hideSprites[2] : null;

        CreateHideSpot(props, "Escondite_Barril1",  escBarril,  7f, 12f, 0.83f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Cajas1",   escCajas,  20f, 12f, 0.83f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Puerta1",  escPuerta, 11f, 22f, 0.83f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Cajas2",   escCajas,  31f,  7f, 0.83f, hideSpotLayer);

        // ──── 5. Tumbas decorativas ─────────────────────────────────────────────
        // Usamos decorSprites como marcadores visuales de tumbas
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

        // ──── 6. Velas coleccionables (3 en nivel 4) ──────────────────────────
        Transform sistemasGo = FreshRoot("Sistemas");
        CreateCandlePickup(sistemasGo, "Vela_01", velaSprite, 9f,  15f);
        CreateCandlePickup(sistemasGo, "Vela_02", velaSprite, 22f, 17f);
        CreateCandlePickup(sistemasGo, "Vela_03", velaSprite, 33f, 10f);

        // ──── 7. Carlos (jugador) ───────────────────────────────────────────────
        SetupPlayer(new Vector2(1f, 13f), playerLayer);

        // ──── 8. La Llorona (nivel 4: rápida y agresiva) ───────────────────────
        // 6 waypoints en L: de izq a der, sube, cruza y baja, formando un circuito
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
            pSpeed:     2.0f,    // Nivel 4: máxima dificultad
            cSpeed:     3.4f,
            vDist:      6.5f,
            vAngle:     120f
        );

        // ──── 9. GameManager ────────────────────────────────────────────────────
        SetupGameManager(4);

        // ──── 10. Meta: La puerta de la casa de Carlos ─────────────────────────
        // Al llegar aquí = victoria + grito final + cinemática
        GameObject metaGo = GameObject.Find("Meta_Casa");
        if (metaGo == null)
        {
            metaGo = new GameObject("Meta_Casa");
            Undo.RegisterCreatedObjectUndo(metaGo, "Meta_Casa");
        }
        metaGo.transform.position = new Vector3(36.5f, 13f, 0f);

        BoxCollider2D metaCol = metaGo.GetComponent<BoxCollider2D>();
        if (metaCol == null) metaCol = metaGo.AddComponent<BoxCollider2D>();
        metaCol.isTrigger = true;
        metaCol.size = new Vector2(1.5f, 4f);

        LevelGoal lg = metaGo.GetComponent<LevelGoal>();
        if (lg == null) lg = metaGo.AddComponent<LevelGoal>();

        // Sprite de puerta para la meta
        SpriteRenderer metaSR = metaGo.GetComponent<SpriteRenderer>();
        if (metaSR == null) metaSR = metaGo.AddComponent<SpriteRenderer>();
        if (escPuerta != null) metaSR.sprite = escPuerta;
        metaSR.sortingOrder = 3;

        // ──── 11. Tutorial triggers ─────────────────────────────────────────────
        Transform triggers = FreshRoot("Triggers");
        CreateTutorialTrigger(triggers, "TutFinal", new Vector2(2f, 13f), new Vector2(3f, 4f),
            "¡Estás casi en casa! Usa [F] para colocar una vela y distraer a La Llorona.");
        CreateTutorialTrigger(triggers, "TutNiebla", new Vector2(15f, 13f), new Vector2(3f, 4f),
            "La Llorona es más rápida aquí. ¡Escóndete antes de que te vea!");

        // ──── 12. Cámara ────────────────────────────────────────────────────────
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
        cam.transform.position = new Vector3(19f, 13f, -10f);
        cam.backgroundColor = new Color(0.04f, 0.02f, 0.08f);

        CameraFollow2D follow = cam.GetComponent<CameraFollow2D>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow2D>();

        // ──── 13. Canvas HUD ────────────────────────────────────────────────────
        UIBuilder.BuildLevelUICanvas(ScenePath);

        // ──── 14. Guardar escena ────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.Refresh();

        Debug.Log("[Level4Builder] ✅ Nivel 4 construido y guardado en: " + ScenePath);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Helpers
    // ══════════════════════════════════════════════════════════════════════════

    static TileBase[] LoadTiles()
    {
        var tiles = new System.Collections.Generic.List<TileBase>();
        for (int i = 0; i < 48; i++)
        {
            TileBase t = AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Tiles/tiles_{i}.asset");
            tiles.Add(t); // puede ser null si falta
        }
        return tiles.ToArray();
    }

    static Sprite[] LoadSubSprites(string path)
    {
        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
        return objs.OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }

    static Tilemap GetOrCreateTilemap(Transform parent, string name, int order, int layer, bool hasCollider)
    {
        Transform existing = parent.Find(name);
        GameObject go;
        if (existing != null)
        {
            go = existing.gameObject;
        }
        else
        {
            go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>();
            Undo.RegisterCreatedObjectUndo(go, name);
        }
        TilemapRenderer r = go.GetComponent<TilemapRenderer>();
        if (r != null) r.sortingOrder = order;
        if (layer >= 0) go.layer = layer;
        if (hasCollider && go.GetComponent<TilemapCollider2D>() == null)
            go.AddComponent<TilemapCollider2D>();
        return go.GetComponent<Tilemap>();
    }

    static void ConfigureCompositeCollider(GameObject go)
    {
        TilemapCollider2D tc = go.GetComponent<TilemapCollider2D>();
        if (tc == null) tc = go.AddComponent<TilemapCollider2D>();
        CompositeCollider2D cc = go.GetComponent<CompositeCollider2D>();
        if (cc == null) cc = go.AddComponent<CompositeCollider2D>();
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        tc.compositeOperation = Collider2D.CompositeOperation.Merge;
    }

    static void PaintWallRect(Tilemap map, TileBase tile, int x0, int y0, int x1, int y1)
    {
        if (tile == null) return;
        for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
                map.SetTile(new Vector3Int(x, y, 0), tile);
    }

    static Transform FreshRoot(string name)
    {
        GameObject old = GameObject.Find(name);
        if (old != null) Undo.DestroyObjectImmediate(old);
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, name);
        return go.transform;
    }

    static Transform CreateEmpty(Transform parent, string name, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        Undo.RegisterCreatedObjectUndo(go, name);
        return go.transform;
    }

    static void CreateDecorSprite(Transform parent, string name, Sprite sprite, float x, float y)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        // Colisionador pequeño (base de la tumba)
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.size   = new Vector2(0.8f, 0.5f);
        bc.offset = new Vector2(0f, -0.3f);
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateHideSpot(Transform parent, string name, Sprite sprite, float x, float y, float scale, int layer)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(scale, scale, 1f);
        if (layer >= 0) go.layer = layer;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = sprite.bounds.size + new Vector3(0.6f / scale, 0.6f / scale, 0f);

        HideSpot hs = go.AddComponent<HideSpot>();
        GameObject hp = new GameObject("HidePoint");
        hp.transform.SetParent(go.transform, false);
        hp.transform.localPosition = new Vector3(0f, 0.4f, 0f);

        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("hidePoint").objectReferenceValue = hp.transform;
        so.ApplyModifiedProperties();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateCandlePickup(Transform parent, string name, Sprite sprite, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        if (sprite != null)
        {
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;
        }

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.4f;

        go.AddComponent<Candle>();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateFarolTenue(Transform parent, string name, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        // Luz muy tenue, azulada/morada para el cementerio
        Light2D l2d = go.AddComponent<Light2D>();
        l2d.lightType          = Light2D.LightType.Point;
        l2d.color              = new Color(0.45f, 0.35f, 0.85f); // violáceo
        l2d.intensity          = 0.85f;
        l2d.pointLightOuterRadius = 4.0f;

        go.AddComponent<LightFlicker>();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void EnsureGlobalLight(float intensity)
    {
        GameObject glGo = GameObject.Find("Global Light 2D");
        if (glGo == null) glGo = new GameObject("Global Light 2D");
        Light2D gl = glGo.GetComponent<Light2D>() ?? glGo.AddComponent<Light2D>();
        gl.lightType  = Light2D.LightType.Global;
        gl.color      = new Color(0.10f, 0.08f, 0.20f); // morado casi negro
        gl.intensity  = intensity;
    }

    static void CreateTutorialTrigger(Transform parent, string name, Vector2 pos, Vector2 size, string text)
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
        LloronaAI llorona = Object.FindAnyObjectByType<LloronaAI>();
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

    static void SetupGameManager(int levelNum)
    {
        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject gmGo = new GameObject("GameManager");
            gm = gmGo.AddComponent<GameManager>();
        }
        SerializedObject so = new SerializedObject(gm);
        so.FindProperty("currentLevelNumber").intValue = levelNum;
        so.ApplyModifiedProperties();
    }
}
