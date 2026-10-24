// Assets/Editor/Level2Builder.cs
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
/// Constructor automatizado del Nivel 2: El Callejón del Beso.
/// Tramo horizontal largo con giro a 90° hacia el norte (forma de L).
/// Incluye charcos de lodo, botellas ruidosas, perro callejero, faroles y portal de salida.
/// </summary>
public static class Level2Builder
{
    const string ScenePath = "Assets/Scenes/Level2_Callejon.unity";

    [MenuItem("La Llorona/Construir Nivel 2")]
    public static void BuildLevel2Scene()
    {
        Debug.Log("[Level2Builder] Iniciando construcción del Nivel 2 (El Callejón)...");

        SpriteAssetGenerator.GenerateAllSprites();

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 2");

        int wallsLayer = LayerMask.NameToLayer("Walls");
        int hideSpotLayer = LayerMask.NameToLayer("HideSpot");
        int playerLayer = LayerMask.NameToLayer("Player");
        int lloronaLayer = LayerMask.NameToLayer("Llorona");

        TileBase[] allTiles = LoadTiles();
        Sprite[] hideSprites = LoadSubSprites("Assets/Sprites/Environment/escondites.png");
        Sprite[] decorSprites = LoadSubSprites("Assets/Sprites/Environment/decoracion.png");

        // 1. Grid y Tilemaps
        GameObject gridGo = GameObject.Find("Grid") ?? new GameObject("Grid");
        if (gridGo.GetComponent<Grid>() == null) gridGo.AddComponent<Grid>();

        Tilemap suelo = GetOrCreateTilemap(gridGo.transform, "Suelo", 0, -1, false);
        Tilemap paredes = GetOrCreateTilemap(gridGo.transform, "Paredes", 1, wallsLayer, true);
        suelo.ClearAllTiles();
        paredes.ClearAllTiles();

        // 2. Construcción en forma de L
        TileBase floorTile = allTiles[0];
        TileBase wallUpper = allTiles[36];
        TileBase wallLower = allTiles[24];

        // Tramo Horizontal: X de 0 a 30, Y de 2 a 7
        for (int x = 0; x <= 30; x++)
        {
            for (int y = 2; y <= 7; y++)
            {
                TileBase t = (x < 3) ? allTiles[12] : allTiles[x % 6];
                suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // Tramo Vertical: X de 24 a 29, Y de 8 a 46
        for (int y = 8; y <= 46; y++)
        {
            for (int x = 24; x <= 29; x++)
            {
                TileBase t = (y > 44) ? allTiles[12] : allTiles[(x + y) % 6];
                suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // Paredes Tramo Horizontal (Abajo Y=0,1 y Arriba Y=8,9 donde no gira)
        for (int x = 0; x <= 31; x++)
        {
            paredes.SetTile(new Vector3Int(x, 1, 0), wallUpper);
            paredes.SetTile(new Vector3Int(x, 0, 0), wallLower);

            if (x < 24)
            {
                paredes.SetTile(new Vector3Int(x, 9, 0), wallUpper);
                paredes.SetTile(new Vector3Int(x, 8, 0), wallLower);
            }
        }

        // Paredes Tramo Vertical (Izquierda X=22,23 y Derecha X=30,31)
        for (int y = 8; y <= 48; y++)
        {
            paredes.SetTile(new Vector3Int(23, y, 0), wallUpper);
            paredes.SetTile(new Vector3Int(22, y, 0), wallLower);

            paredes.SetTile(new Vector3Int(31, y, 0), wallUpper);
            paredes.SetTile(new Vector3Int(30, y, 0), wallLower);
        }

        // Fondo superior del callejón vertical (pared de cierre en Y=47,48)
        for (int x = 22; x <= 31; x++)
        {
            paredes.SetTile(new Vector3Int(x, 48, 0), wallUpper);
            paredes.SetTile(new Vector3Int(x, 47, 0), wallLower);
        }

        ConfigureCompositeCollider(paredes.gameObject);

        // 3. Props, Escondites y Obstáculos
        Transform props = FreshRoot("Props");

        Sprite hDoor = hideSprites != null && hideSprites.Length > 2 ? hideSprites[2] : null;
        Sprite hBarrel = hideSprites != null && hideSprites.Length > 1 ? hideSprites[1] : null;
        Sprite farol = decorSprites != null && decorSprites.Length > 0 ? decorSprites[0] : null;

        // Escondites
        CreateHideSpot(props, "Escondite_Puerta_1", hDoor, 10.0f, 7.8f, 1.0f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Barril", hBarrel, 22.0f, 3.2f, 1.05f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Puerta_2", hDoor, 28.5f, 26.0f, 1.0f, hideSpotLayer);

        // Faroles con LightFlicker
        CreateFlickeringFarol(props, "Farol_Flicker_1", farol, 4.0f, 7.5f, wallsLayer);
        CreateFlickeringFarol(props, "Farol_Flicker_2", farol, 16.0f, 2.8f, wallsLayer);
        CreateFlickeringFarol(props, "Farol_Flicker_3", farol, 25.0f, 14.0f, wallsLayer);
        CreateFlickeringFarol(props, "Farol_Flicker_4", farol, 28.5f, 34.0f, wallsLayer);

        // Obstáculos interactivos
        CreateMudPuddle(props, 12.0f, 4.5f, 1.1f);
        CreateNoiseObstacle(props, 19.5f, 5.0f);
        CreateStrayDog(props, 28.0f, 18.0f);
        CreateMudPuddle(props, 26.5f, 28.0f, 1.2f);
        CreateNoiseObstacle(props, 25.5f, 36.0f);

        // 4. Portal Monumental de Salida (Y ≈ 45.5)
        CreateExitPortal(props, 26.5f, 45.5f);

        // 5. Luz Global
        EnsureGlobalLight(0.48f);

        // 6. Waypoints de patrulla de La Llorona (tramo vertical)
        Transform wpRoot = FreshRoot("Waypoints");
        Transform wpA = CreateEmpty(wpRoot, "WP_A", 26.5f, 40.0f);
        Transform wpB = CreateEmpty(wpRoot, "WP_B", 26.5f, 12.0f);

        // 7. Player (Inicia al principio del callejón horizontal)
        SetupPlayer(new Vector2(3.5f, 4.5f), playerLayer);

        // 8. La Llorona (Dificultad Nivel 2)
        SetupLlorona(new Vector2(26.5f, 38.0f), wpA, wpB, wallsLayer, lloronaLayer, 1.5f, 2.5f, 5.0f, 100.0f);

        // 9. Cámara
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
        cam.transform.position = new Vector3(3.5f, 4.5f, -10f);
        cam.backgroundColor = new Color(0.04f, 0.03f, 0.10f);
        CameraFollow2D camFollow = cam.GetComponent<CameraFollow2D>() ?? cam.gameObject.AddComponent<CameraFollow2D>();
        camFollow.SetBounds(new Vector2(3.5f, 4.5f), new Vector2(27f, 44f));

        // 10. Triggers de Tutorial
        Transform tutRoot = FreshRoot("TutorialTriggers");
        CreateTutorialTrigger(tutRoot, "Tutorial_Inicio",
            new Vector2(3.5f, 4.5f), new Vector2(5f, 5f),
            "¡El Callejón del Beso! Cruza el callejón.\nSigue la flecha en la brújula hacia la salida.",
            showOnStart: true);

        CreateTutorialTrigger(tutRoot, "Tutorial_Giro",
            new Vector2(22.0f, 4.5f), new Vector2(3f, 4f),
            "El callejón gira hacia el norte. ¡Cuidado al doblar la esquina!");

        CreateTutorialTrigger(tutRoot, "Tutorial_Salida",
            new Vector2(26.5f, 41.0f), new Vector2(5f, 4f),
            "¡La salida está a pocos pasos! Cruza el portal verde.");

        // 11. Manager
        SetupGameManager(2);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.SaveScene(scene, ScenePath);
        UIBuilder.BuildLevel2UICanvas();
        Debug.Log("[Level2Builder] ¡Nivel 2 construido y guardado en " + ScenePath + "!");
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

    static void CreateFlickeringFarol(Transform parent, string name, Sprite sprite, float x, float y, int layer)
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
        l2d.pointLightOuterRadius = 4.2f;

        lightGo.AddComponent<LightFlicker>();
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
        bc.size = new Vector2(4.5f, 3.5f);
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

    static void EnsureGlobalLight(float intensity = 0.48f)
    {
        Light2D gl = UnityEngine.Object.FindAnyObjectByType<Light2D>();
        GameObject glGo = (gl != null && gl.lightType == Light2D.LightType.Global)
            ? gl.gameObject
            : new GameObject("LuzGlobal2D");
        if (gl == null || gl.lightType != Light2D.LightType.Global)
            gl = glGo.AddComponent<Light2D>();
        gl.lightType = Light2D.LightType.Global;
        gl.color = new Color(0.92f, 0.88f, 0.78f);
        gl.intensity = intensity;
    }

    static Transform CreateEmpty(Transform parent, string name, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        Undo.RegisterCreatedObjectUndo(go, name);
        return go.transform;
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
        if (player != null)
        {
            Undo.RecordObject(player.transform, "Posicionar Player");
            player.transform.position = new Vector3(pos.x, pos.y, 0f);
            if (playerLayer >= 0) player.layer = playerLayer;
        }
    }

    static void SetupLlorona(Vector2 pos, Transform wpA, Transform wpB, int wallsLayer, int lloronaLayer,
        float pSpeed, float cSpeed, float vDist, float vAngle)
    {
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona == null) return;
        Undo.RecordObject(llorona.transform, "Posicionar Llorona");
        llorona.transform.position = new Vector3(pos.x, pos.y, 0f);
        if (lloronaLayer >= 0) llorona.gameObject.layer = lloronaLayer;

        SerializedObject so = new SerializedObject(llorona);
        SerializedProperty wpProp = so.FindProperty("waypoints");
        if (wpProp != null)
        {
            wpProp.arraySize = 2;
            wpProp.GetArrayElementAtIndex(0).objectReferenceValue = wpA;
            wpProp.GetArrayElementAtIndex(1).objectReferenceValue = wpB;
        }
        so.FindProperty("patrolSpeed").floatValue  = pSpeed;
        so.FindProperty("chaseSpeed").floatValue   = cSpeed;
        so.FindProperty("viewDistance").floatValue = vDist;
        so.FindProperty("viewAngle").floatValue    = vAngle;

        LloronaSettings s2 = AssetDatabase.LoadAssetAtPath<LloronaSettings>("Assets/Settings/LloronaSettings_Level2.asset");
        if (s2 != null) so.FindProperty("settings").objectReferenceValue = s2;

        so.ApplyModifiedProperties();
    }

    static void SetupGameManager(int lvl)
    {
        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject gmGo = new GameObject("GameManager");
            gm = gmGo.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gmGo, "GameManager");
        }
        SerializedObject so = new SerializedObject(gm);
        SerializedProperty prop = so.FindProperty("currentLevelNumber") ?? so.FindProperty("levelNumber");
        if (prop != null) prop.intValue = lvl;
        so.ApplyModifiedProperties();
    }
}
