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
/// Constructor automatizado del Nivel 3: El Atrio de la Parroquia.
/// Plaza abierta de 36 x 26 con fuente central donde La Llorona patrulla en círculo.
/// Incluye recolección de velas, escondites, charcos de agua de la fuente,
/// botellas ruidosas, perro callejero y portal monumental de salida.
/// </summary>
public static class Level3Builder
{
    const string ScenePath = "Assets/Scenes/Level3_Atrio.unity";

    [MenuItem("La Llorona/Construir Nivel 3")]
    public static void BuildLevel3Scene()
    {
        Debug.Log("[Level3Builder] Iniciando construcción del Nivel 3 (El Atrio)...");

        SpriteAssetGenerator.GenerateAllSprites();

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

        TileBase[] allTiles = LoadTiles();
        Sprite[] hideSprites = LoadSubSprites("Assets/Sprites/Environment/escondites.png");
        Sprite[] decorSprites = LoadSubSprites("Assets/Sprites/Environment/decoracion.png");
        Sprite velaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/vela.png");
        Sprite[] salidas = LoadSubSprites("Assets/Sprites/Environment/Salidas.png");

        // 1. Grid y Tilemaps
        GameObject gridGo = GameObject.Find("Grid") ?? new GameObject("Grid");
        if (gridGo.GetComponent<Grid>() == null) gridGo.AddComponent<Grid>();

        Tilemap suelo = GetOrCreateTilemap(gridGo.transform, "Suelo", 0, -1, false);
        Tilemap paredes = GetOrCreateTilemap(gridGo.transform, "Paredes", 1, wallsLayer, true);
        suelo.ClearAllTiles();
        paredes.ClearAllTiles();

        // 2. Dimensiones: 36 x 26 (X: 0 a 36, Y: 0 a 26)
        TileBase floorTile = allTiles[0];
        TileBase wallUpper = allTiles[36];
        TileBase wallLower = allTiles[24];

        for (int x = 0; x <= 36; x++)
        {
            for (int y = 2; y <= 24; y++)
            {
                TileBase t = (x < 4 || x > 32) ? allTiles[12] : allTiles[(x + y) % 6];
                suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // Muros superior e inferior
        for (int x = 0; x <= 36; x++)
        {
            paredes.SetTile(new Vector3Int(x, 26, 0), wallUpper);
            paredes.SetTile(new Vector3Int(x, 25, 0), wallLower);
            paredes.SetTile(new Vector3Int(x, 1, 0), wallUpper);
            paredes.SetTile(new Vector3Int(x, 0, 0), wallLower);
        }

        ConfigureCompositeCollider(paredes.gameObject);

        // 3. Límites laterales
        Transform limits = FreshRoot("Limites");
        CreateInvisibleWall(limits, "Lim_Oeste", -0.5f, 13.0f, 26f, wallsLayer);
        CreateInvisibleWall(limits, "Lim_Este",  36.5f, 13.0f, 26f, wallsLayer);

        // 4. Props y Escondites
        Transform props = FreshRoot("Props");

        Sprite hDoor = hideSprites != null && hideSprites.Length > 2 ? hideSprites[2] : null;
        Sprite hBarrel = hideSprites != null && hideSprites.Length > 1 ? hideSprites[1] : null;
        Sprite hBoxes = hideSprites != null && hideSprites.Length > 0 ? hideSprites[0] : null;

        // Fuente central en X=18, Y=13
        CreateFuente(props, "Fuente_Central", 18.0f, 13.0f, wallsLayer);

        // Escondites
        CreateHideSpot(props, "Escondite_Cajas_1", hBoxes, 8.0f, 13.0f, 1.2f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Puerta_1", hDoor, 18.0f, 24.5f, 1.0f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Barril_1", hBarrel, 18.0f, 2.5f, 1.05f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Barril_2", hBarrel, 28.0f, 13.0f, 1.05f, hideSpotLayer);

        // Faroles
        Sprite farol = decorSprites != null && decorSprites.Length > 0 ? decorSprites[0] : null;
        Sprite banca = decorSprites != null && decorSprites.Length > 2 ? decorSprites[2] : null;

        CreateFarol(props, "Farol_NO", farol, 6.0f, 20.0f, wallsLayer);
        CreateFarol(props, "Farol_NE", farol, 30.0f, 20.0f, wallsLayer);
        CreateFarol(props, "Farol_SO", farol, 6.0f, 5.0f, wallsLayer);
        CreateFarol(props, "Farol_SE", farol, 30.0f, 5.0f, wallsLayer);

        CreateSolidProp(props, "Banca_1", banca, 11.0f, 19.0f, 1.0f, wallsLayer, new Vector2(1.2f, 0.4f), new Vector2(0f, -0.3f));
        CreateSolidProp(props, "Banca_2", banca, 25.0f, 19.0f, 1.0f, wallsLayer, new Vector2(1.2f, 0.4f), new Vector2(0f, -0.3f));

        // Obstáculos interactivos
        CreateMudPuddle(props, 14.5f, 10.5f, 1.2f);
        CreateMudPuddle(props, 21.5f, 15.5f, 1.2f);
        CreateNoiseObstacle(props, 10.0f, 13.0f);
        CreateNoiseObstacle(props, 26.0f, 13.0f);
        CreateStrayDog(props, 24.0f, 6.5f);

        // 5. Velas Coleccionables
        Transform candlesRoot = FreshRoot("OfrendasVelas");
        CreateCandlePickup(candlesRoot, "Vela_1", velaSprite, new Vector2(9.0f, 6.0f));
        CreateCandlePickup(candlesRoot, "Vela_2", velaSprite, new Vector2(18.0f, 21.0f));
        CreateCandlePickup(candlesRoot, "Vela_3", velaSprite, new Vector2(27.0f, 6.0f));

        // 6. Portal Monumental de Salida (X ≈ 35.0, Y = 13.0)
        Sprite exitSprite = (salidas != null && salidas.Length > 2) ? salidas[2] : (salidas != null && salidas.Length > 0 ? salidas[0] : null);
        CreateExitPortal(props, 35.0f, 13.0f, exitSprite);

        // 7. Iluminación Global
        EnsureGlobalLight(0.50f);

        // 8. Waypoints en Círculo (6 puntos alrededor de la fuente central)
        Transform wpRoot = FreshRoot("Waypoints");
        Transform[] wps = new Transform[6];
        wps[0] = CreateEmpty(wpRoot, "WP_Norte",   18.0f, 18.5f);
        wps[1] = CreateEmpty(wpRoot, "WP_NorEste", 22.5f, 16.0f);
        wps[2] = CreateEmpty(wpRoot, "WP_SurEste", 22.5f, 10.0f);
        wps[3] = CreateEmpty(wpRoot, "WP_Sur",     18.0f,  7.5f);
        wps[4] = CreateEmpty(wpRoot, "WP_SurOeste",13.5f, 10.0f);
        wps[5] = CreateEmpty(wpRoot, "WP_NorOeste",13.5f, 16.0f);

        // 9. Meta de seguridad en X ≈ 35.2
        GameObject goal = new GameObject("Meta");
        goal.transform.position = new Vector3(35.2f, 13.0f, 0f);
        BoxCollider2D gCol = goal.AddComponent<BoxCollider2D>();
        gCol.isTrigger = true;
        gCol.size = new Vector2(2.5f, 20.0f);
        goal.AddComponent<LevelGoal>();
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
            "¡Atrio de la parroquia! Recoge velas (E) y colócalas (F)\npara distraer a La Llorona unos segundos.",
            showOnStart: true);
        CreateTutorialTrigger(tutRoot, "Tutorial_Fuente",
            new Vector2(14.0f, 13.0f), new Vector2(3f, 6f),
            "La Llorona circula alrededor de la fuente.\n¡Espera el momento justo y cruza con sigilo!");
        CreateTutorialTrigger(tutRoot, "Tutorial_Salida",
            new Vector2(32.0f, 13.0f), new Vector2(3f, 8f),
            "¡La salida al panteón está justo enfrente!");

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

    static Sprite[] LoadSubSprites(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();

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

    static void ConfigureCompositeCollider(GameObject paredesGo)
    {
        CompositeCollider2D comp = paredesGo.GetComponent<CompositeCollider2D>() ?? paredesGo.AddComponent<CompositeCollider2D>();
        comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
        Rigidbody2D rb = paredesGo.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.bodyType = RigidbodyType2D.Static; rb.simulated = true; }
    }

    static void CreateFuente(Transform parent, string name, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/puesto_mercado.png");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.color = new Color(0.6f, 0.7f, 0.85f);
        sr.sortingOrder = 2;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.radius = 2.2f;
        if (layer >= 0) go.layer = layer;

        GameObject lightGo = new GameObject("LuzFuente");
        lightGo.transform.SetParent(go.transform, false);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.4f, 0.7f, 1.0f);
        l2d.intensity = 0.9f;
        l2d.pointLightOuterRadius = 4.5f;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateFarol(Transform parent, string name, Sprite sprite, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;
        col.offset = new Vector2(0f, -0.6f);
        if (layer >= 0) go.layer = layer;

        GameObject lightGo = new GameObject("Luz");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.85f, 0.50f);
        l2d.intensity = 1.35f;
        l2d.pointLightOuterRadius = 4.5f;

        lightGo.AddComponent<LightFlicker>();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateMudPuddle(Transform parent, float x, float y, float scale = 1.0f)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Botellas_charcolodo_Nvl1.png").OfType<Sprite>().FirstOrDefault(s => s.name.StartsWith("Charco"));
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
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Botellas_charcolodo_Nvl1.png").OfType<Sprite>().FirstOrDefault(s => s.name.StartsWith("Botella"));
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
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Perro/perro.png").OfType<Sprite>().FirstOrDefault();
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

    static void CreateExitPortal(Transform parent, float x, float y, Sprite customSprite = null)
    {
        Sprite portalSprite = customSprite ?? AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Salidas.png").OfType<Sprite>().FirstOrDefault();

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
        tm.text = "SALIDA ➔";
        tm.fontSize = 32;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.35f, 1f, 0.55f);
        MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;

        Undo.RegisterCreatedObjectUndo(go, "Portal_Salida_Meta");
    }

    static void CreateHideSpot(Transform parent, string name, Sprite sprite, float x, float y, float scale, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        BoxCollider2D trig = go.AddComponent<BoxCollider2D>();
        trig.isTrigger = true;
        trig.size = new Vector2(1.2f, 1.2f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.4f);
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

    static void CreateSolidProp(Transform parent, string name, Sprite sprite, float x, float y, float scale, int layer, Vector2 colSize, Vector2 colOffset)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = colSize;
        col.offset = colOffset;
        if (layer >= 0) go.layer = layer;
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateCandlePickup(Transform parent, string name, Sprite sprite, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        GameObject lightGo = new GameObject("LuzVela");
        lightGo.transform.SetParent(go.transform, false);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.75f, 0.35f);
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
        gl.color = new Color(0.92f, 0.88f, 0.78f);
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
        if (llorona == null) return;
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
