// Assets/Editor/Level3Builder.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Constructor automatizado del Nivel 3: "El Atrio de la Parroquia".
/// Explanada abierta de 36 x 26 unidades con fuente central, patrulla circular en 6 waypoints,
/// 3 escondites, velas coleccionables/ofrendas que distraen a La Llorona por 3 s y meta al este.
///
/// Ejecución:
/// Menú Unity: La Llorona > Construir Nivel 3 (Atrio)
/// Línea de comandos (batchmode): -executeMethod Level3Builder.BuildLevel3Scene
/// </summary>
public static class Level3Builder
{
    const string ScenePath = "Assets/Scenes/Level3_Atrio.unity";

    [MenuItem("La Llorona/Construir Nivel 3 (Atrio)")]
    public static void BuildLevel3Scene()
    {
        Debug.Log("[Level3Builder] Construyendo Nivel 3: El Atrio de la Parroquia...");
        Directory.CreateDirectory("Assets/Scenes");

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 3");

        int wallsLayer = LayerMask.NameToLayer("Walls");
        int hideSpotLayer = LayerMask.NameToLayer("HideSpot");
        int playerLayer = LayerMask.NameToLayer("Player");
        int lloronaLayer = LayerMask.NameToLayer("Llorona");

        if (wallsLayer < 0) wallsLayer = 0;
        if (hideSpotLayer < 0) hideSpotLayer = 0;

        TileBase[] allTiles = LoadTiles();
        Sprite[] decorSprites = LoadSubSprites("Assets/Sprites/Environment/decoracion.png");
        Sprite[] hideSprites = LoadSubSprites("Assets/Sprites/Environment/escondites.png");
        Sprite velaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/vela.png");

        // 1. Grid y Tilemaps
        GameObject gridGo = GameObject.Find("Grid");
        if (gridGo == null)
        {
            gridGo = new GameObject("Grid");
            gridGo.AddComponent<Grid>();
            Undo.RegisterCreatedObjectUndo(gridGo, "Grid");
        }

        Tilemap suelo = GetOrCreateTilemap(gridGo.transform, "Suelo", 0, -1, false);
        Tilemap paredes = GetOrCreateTilemap(gridGo.transform, "Paredes", 1, wallsLayer, true);
        suelo.ClearAllTiles();
        paredes.ClearAllTiles();

        // 2. Construcción de Plaza del Atrio (36 x 26)
        TileBase floorTile = allTiles[0];
        TileBase puddleTile = allTiles[6];
        TileBase wallUpper = allTiles[36];
        TileBase wallLower = allTiles[24];

        System.Random rnd = new System.Random(88);

        // Suelo de la plaza
        for (int x = 0; x < 36; x++)
        {
            for (int y = 2; y <= 23; y++)
            {
                TileBase t = (rnd.NextDouble() < 0.10) ? puddleTile : floorTile;
                suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // Paredes: Fachada de parroquia al norte (Y=24,25) y muro colonial al sur (Y=0,1)
        for (int x = 0; x < 36; x++)
        {
            paredes.SetTile(new Vector3Int(x, 25, 0), wallUpper);
            paredes.SetTile(new Vector3Int(x, 24, 0), wallLower);
            paredes.SetTile(new Vector3Int(x, 1, 0), wallUpper);
            paredes.SetTile(new Vector3Int(x, 0, 0), wallLower);
        }

        ConfigureCompositeCollider(paredes.gameObject);

        // Límites laterales
        Transform limits = FreshRoot("LimitesInvisibles");
        CreateInvisibleWall(limits, "Limite_Izq", -0.5f, 13.0f, 26f, wallsLayer);
        CreateInvisibleWall(limits, "Limite_Der", 36.5f, 13.0f, 26f, wallsLayer);

        // 3. Fuente Central de Cantera (X=18, Y=13)
        Transform props = FreshRoot("Props");
        CreateFountain(props, "Fuente_Central", 18.0f, 13.0f, wallsLayer);

        // 4. Escondites
        Sprite hBoxes = hideSprites != null && hideSprites.Length > 0 ? hideSprites[0] : null;
        Sprite hDoor = hideSprites != null && hideSprites.Length > 2 ? hideSprites[2] : null;
        Sprite hBarrel = hideSprites != null && hideSprites.Length > 1 ? hideSprites[1] : null;

        CreateHideSpot(props, "Escondite_Cajas_1", hBoxes, 7.0f, 13.0f, 1.2f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Puerta_Parroquia", hDoor, 18.0f, 23.5f, 1.0f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Barril_2", hBarrel, 28.0f, 13.0f, 1.05f, hideSpotLayer);

        // 5. Faroles en los cuatro cuadrantes del atrio
        Sprite farol = decorSprites != null && decorSprites.Length > 0 ? decorSprites[0] : null;
        Sprite banca = decorSprites != null && decorSprites.Length > 2 ? decorSprites[2] : null;

        CreateFarol(props, "Farol_NO", farol, 6.0f, 20.0f, wallsLayer);
        CreateFarol(props, "Farol_NE", farol, 30.0f, 20.0f, wallsLayer);
        CreateFarol(props, "Farol_SO", farol, 6.0f, 5.0f, wallsLayer);
        CreateFarol(props, "Farol_SE", farol, 30.0f, 5.0f, wallsLayer);

        CreateSolidProp(props, "Banca_1", banca, 11.0f, 19.0f, 1.0f, wallsLayer, new Vector2(1.2f, 0.4f), new Vector2(0f, -0.3f));
        CreateSolidProp(props, "Banca_2", banca, 25.0f, 19.0f, 1.0f, wallsLayer, new Vector2(1.2f, 0.4f), new Vector2(0f, -0.3f));

        // 6. Velas Coleccionables (Ofrendas que distraen a La Llorona)
        Transform candlesRoot = FreshRoot("OfrendasVelas");
        CreateCandlePickup(candlesRoot, "Vela_1", velaSprite, new Vector2(9.0f, 6.0f));
        CreateCandlePickup(candlesRoot, "Vela_2", velaSprite, new Vector2(18.0f, 21.0f));
        CreateCandlePickup(candlesRoot, "Vela_3", velaSprite, new Vector2(27.0f, 6.0f));

        // 7. Iluminación Global URP 2D — atrio de noche, visible
        EnsureGlobalLight(0.48f);

        // 8. Waypoints en Círculo (6 puntos alrededor de la fuente central en X=18, Y=13)
        Transform wpRoot = FreshRoot("Waypoints");
        Transform[] wps = new Transform[6];
        wps[0] = CreateEmpty(wpRoot, "WP_Norte",   18.0f, 18.5f);
        wps[1] = CreateEmpty(wpRoot, "WP_NorEste", 22.5f, 16.0f);
        wps[2] = CreateEmpty(wpRoot, "WP_SurEste", 22.5f, 10.0f);
        wps[3] = CreateEmpty(wpRoot, "WP_Sur",     18.0f,  7.5f);
        wps[4] = CreateEmpty(wpRoot, "WP_SurOeste",13.5f, 10.0f);
        wps[5] = CreateEmpty(wpRoot, "WP_NorOeste",13.5f, 16.0f);

        // 9. Meta hacia el Cementerio (X ≈ 35.2) con señal verde
        GameObject goal = new GameObject("Meta");
        goal.transform.position = new Vector3(35.2f, 13.0f, 0f);
        BoxCollider2D gCol = goal.AddComponent<BoxCollider2D>();
        gCol.isTrigger = true;
        gCol.size = new Vector2(1.5f, 20.0f);
        goal.AddComponent<LevelGoal>();
        // Luz verde de salida
        GameObject exitLightGo = new GameObject("LuzSalida");
        exitLightGo.transform.SetParent(goal.transform, false);
        Light2D exitL = exitLightGo.AddComponent<Light2D>();
        exitL.lightType = Light2D.LightType.Point;
        exitL.color = new Color(0.2f, 1f, 0.35f);
        exitL.intensity = 2.0f;
        exitL.pointLightOuterRadius = 5f;
        Undo.RegisterCreatedObjectUndo(goal, "Meta");

        // 10. Player (inicio al oeste)
        SetupPlayer(new Vector2(2.5f, 13.0f), playerLayer);

        // 11. La Llorona (Dificultad Nivel 3)
        SetupLloronaCircular(new Vector2(18.0f, 18.5f), wps, wallsLayer, lloronaLayer, 1.6f, 2.8f, 5.5f, 110.0f);

        // 12. Cámara
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
        cam.transform.position = new Vector3(2.5f, 13.0f, -10f);
        cam.backgroundColor = new Color(0.05f, 0.04f, 0.12f);
        CameraFollow2D camFollow = cam.GetComponent<CameraFollow2D>() ?? cam.gameObject.AddComponent<CameraFollow2D>();
        camFollow.SetBounds(new Vector2(8.8f, 7.0f), new Vector2(27.2f, 19.0f));

        // 13. Tutorials
        Transform tutRoot = FreshRoot("TutorialTriggers");
        CreateTutorialTrigger(tutRoot, "Tutorial_Inicio",
            new Vector2(2.5f, 13.0f), new Vector2(4f, 8f),
            "¡Atrio de la parroquia! Recoge velas (E) y colócalas (F)\npara distraer a La Llorona 3 segundos.",
            showOnStart: true);
        CreateTutorialTrigger(tutRoot, "Tutorial_Fuente",
            new Vector2(14.0f, 13.0f), new Vector2(3f, 6f),
            "La Llorona circula alrededor de la fuente.\n¡Espera su momento y cruza deprisa!");
        CreateTutorialTrigger(tutRoot, "Tutorial_Salida",
            new Vector2(32.0f, 13.0f), new Vector2(3f, 8f),
            "¡La salida está a tu derecha! ¡Cruza la luz verde!");

        // 14. Manager
        SetupGameManager(3);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.SaveScene(scene, ScenePath);
        UIBuilder.BuildLevel3UICanvas();
        Debug.Log("[Level3Builder] ¡Nivel 3 construido y guardado en " + ScenePath + "!");
    }

    static TileBase[] LoadTiles()
    {
        TileBase[] tiles = new TileBase[48];
        for (int i = 0; i < 48; i++)
            tiles[i] = AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Tiles/tiles_{i}.asset");
        return tiles;
    }

    static Sprite[] LoadSubSprites(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }

    static Tilemap GetOrCreateTilemap(Transform grid, string name, int order, int layer, bool hasCollider)
    {
        Transform child = grid.Find(name);
        GameObject go = (child == null) ? new GameObject(name) : child.gameObject;
        if (child == null)
        {
            go.transform.SetParent(grid, false);
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

    static void CreateFountain(Transform parent, string name, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        if (layer >= 0) go.layer = layer;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.radius = 2.2f;

        // Luz tenue mística sobre el agua de la fuente
        GameObject lightGo = new GameObject("LuzFuente");
        lightGo.transform.SetParent(go.transform, false);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.3f, 0.6f, 0.85f);
        l2d.intensity = 0.9f;
        l2d.pointLightOuterRadius = 4.5f;

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

    static void CreateFarol(Transform parent, string name, Sprite sprite, float x, float y, int layer)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(0.83f, 0.83f, 1f);
        if (layer >= 0) go.layer = layer;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(0.5f, 0.4f);
        bc.offset = new Vector2(0f, -0.6f);

        GameObject lightGo = new GameObject("LuzFarol");
        lightGo.transform.SetParent(go.transform, false);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.75f, 0.4f);
        l2d.intensity = 1.3f;
        l2d.pointLightOuterRadius = 5.0f;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateSolidProp(Transform parent, string name, Sprite sprite, float x, float y, float scale, int layer, Vector2 colSize, Vector2 colOffset)
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
        bc.size = colSize;
        bc.offset = colOffset;
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateCandlePickup(Transform parent, string name, Sprite sprite, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.8f;

        Light2D l2d = go.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.70f, 0.30f);
        l2d.intensity = 1.1f;
        l2d.pointLightOuterRadius = 3.0f;

        go.AddComponent<Candle>();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateInvisibleWall(Transform parent, string name, float x, float y, float height, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        if (layer >= 0) go.layer = layer;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(1.0f, height);
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void EnsureGlobalLight(float intensity)
    {
        GameObject glGo = GameObject.Find("Global Light 2D");
        if (glGo == null) glGo = new GameObject("Global Light 2D");
        Light2D gl = glGo.GetComponent<Light2D>() ?? glGo.AddComponent<Light2D>();
        gl.lightType = Light2D.LightType.Global;
        gl.color = new Color(0.20f, 0.16f, 0.35f);
        gl.intensity = intensity;
    }

    static void CreateTutorialTrigger(Transform parent, string name, Vector2 pos, Vector2 size,
        string text, bool showOnStart = false)
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
            player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            player.AddComponent<CircleCollider2D>().radius = 0.4f;
            player.AddComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/Carlos.png");
            player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerAnimation>();
            player.AddComponent<HideSystem>();
        }
        player.transform.position = new Vector3(pos.x, pos.y, 0f);
        if (playerLayer >= 0) player.layer = playerLayer;
    }

    static void SetupLloronaCircular(Vector2 pos, Transform[] wps, int wallsLayer, int lloronaLayer, float pSpeed, float cSpeed, float vDist, float vAngle)
    {
        LloronaAI llorona = Object.FindAnyObjectByType<LloronaAI>();
        if (llorona == null)
        {
            GameObject lGo = new GameObject("Llorona");
            lGo.AddComponent<Rigidbody2D>().gravityScale = 0f;
            lGo.AddComponent<CircleCollider2D>().radius = 0.5f;
            lGo.AddComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Llorona/llorona.png");
            llorona = lGo.AddComponent<LloronaAI>();
            lGo.AddComponent<LloronaAnimation>();
        }
        llorona.transform.position = new Vector3(pos.x, pos.y, 0f);
        if (lloronaLayer >= 0) llorona.gameObject.layer = lloronaLayer;

        SerializedObject so = new SerializedObject(llorona);
        SerializedProperty wpsProp = so.FindProperty("waypoints");
        wpsProp.arraySize = wps.Length;
        for (int i = 0; i < wps.Length; i++)
            wpsProp.GetArrayElementAtIndex(i).objectReferenceValue = wps[i];

        if (wallsLayer >= 0) so.FindProperty("obstacleMask").intValue = 1 << wallsLayer;

        LloronaSettings s = AssetDatabase.LoadAssetAtPath<LloronaSettings>("Assets/Settings/LloronaSettings_Level3.asset");
        if (s != null) so.FindProperty("settings").objectReferenceValue = s;

        so.FindProperty("patrolSpeed").floatValue = pSpeed;
        so.FindProperty("chaseSpeed").floatValue = cSpeed;
        so.FindProperty("viewDistance").floatValue = vDist;
        so.FindProperty("viewAngle").floatValue = vAngle;
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
