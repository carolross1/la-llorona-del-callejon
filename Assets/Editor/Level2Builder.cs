// Assets/Editor/Level2Builder.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Constructor automatizado del Nivel 2: "El Callejón del Beso".
/// Callejón estrecho (ancho ≈ 6, largo ≈ 50 con giro en L), faroles parpadeantes (LightFlicker),
/// 3 escondites, patrulla de La Llorona A -> B y meta al final del callejón.
///
/// Ejecución:
/// Menú Unity: La Llorona > Construir Nivel 2 (Callejón)
/// Línea de comandos (batchmode): -executeMethod Level2Builder.BuildLevel2Scene
/// </summary>
public static class Level2Builder
{
    const string ScenePath = "Assets/Scenes/Level2_Callejon.unity";

    [MenuItem("La Llorona/Construir Nivel 2 (Callejón)")]
    public static void BuildLevel2Scene()
    {
        Debug.Log("[Level2Builder] Construyendo Nivel 2: El Callejón del Beso...");
        Directory.CreateDirectory("Assets/Scenes");

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

        if (wallsLayer < 0) wallsLayer = 0;
        if (hideSpotLayer < 0) hideSpotLayer = 0;

        TileBase[] allTiles = LoadTiles();
        Sprite[] decorSprites = LoadSubSprites("Assets/Sprites/Environment/decoracion.png");
        Sprite[] hideSprites = LoadSubSprites("Assets/Sprites/Environment/escondites.png");

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

        // 2. Geometría del Callejón en L (Ancho 6)
        // Tramo 1 (Horizontal): X de 0 a 30, Y de 2 a 7 (ancho 6).
        // Tramo 2 (Vertical): X de 24 a 29, Y de 8 a 46 (ancho 6).
        TileBase floorTile = allTiles[0];
        TileBase puddleTile = allTiles[6];
        TileBase wallUpper = allTiles[36];
        TileBase wallLower = allTiles[24];

        System.Random rnd = new System.Random(42);

        // Suelo Tramo Horizontal
        for (int x = 0; x <= 30; x++)
        {
            for (int y = 2; y <= 7; y++)
            {
                TileBase t = (rnd.NextDouble() < 0.15) ? puddleTile : floorTile;
                suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // Suelo Tramo Vertical
        for (int x = 24; x <= 29; x++)
        {
            for (int y = 8; y <= 46; y++)
            {
                TileBase t = (rnd.NextDouble() < 0.15) ? puddleTile : floorTile;
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

        // 3. Props y Escondites
        Transform props = FreshRoot("Props");

        Sprite hDoor = hideSprites != null && hideSprites.Length > 2 ? hideSprites[2] : null;
        Sprite hBarrel = hideSprites != null && hideSprites.Length > 1 ? hideSprites[1] : null;
        Sprite farol = decorSprites != null && decorSprites.Length > 0 ? decorSprites[0] : null;

        // 3 escondites en el callejón
        CreateHideSpot(props, "Escondite_Puerta_1", hDoor, 10.0f, 7.8f, 1.0f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Barril", hBarrel, 22.0f, 3.2f, 1.05f, hideSpotLayer);
        CreateHideSpot(props, "Escondite_Puerta_2", hDoor, 28.5f, 26.0f, 1.0f, hideSpotLayer);

        // Faroles con LightFlicker (parpadeo colonial)
        CreateFlickeringFarol(props, "Farol_Flicker_1", farol, 4.0f, 7.5f, wallsLayer);
        CreateFlickeringFarol(props, "Farol_Flicker_2", farol, 16.0f, 2.8f, wallsLayer);
        CreateFlickeringFarol(props, "Farol_Flicker_3", farol, 25.0f, 14.0f, wallsLayer);
        CreateFlickeringFarol(props, "Farol_Flicker_4", farol, 28.5f, 34.0f, wallsLayer);

        // 4. Luz Global — callejón nocturno pero visible
        EnsureGlobalLight(0.42f);

        // 5. Waypoints de patrulla (A → B en el tramo largo vertical)
        Transform wpRoot = FreshRoot("Waypoints");
        Transform wpA = CreateEmpty(wpRoot, "WP_A", 26.5f, 40.0f);
        Transform wpB = CreateEmpty(wpRoot, "WP_B", 26.5f, 12.0f);

        // 6. Meta al final del callejón (Y ≈ 45.5) con señal verde
        GameObject goal = new GameObject("Meta");
        goal.transform.position = new Vector3(26.5f, 45.5f, 0f);
        BoxCollider2D gCol = goal.AddComponent<BoxCollider2D>();
        gCol.isTrigger = true;
        gCol.size = new Vector2(6.0f, 1.5f);
        goal.AddComponent<LevelGoal>();
        // Luz verde de salida
        GameObject exitLight = new GameObject("LuzSalida");
        exitLight.transform.SetParent(goal.transform, false);
        exitLight.transform.localPosition = Vector3.zero;
        Light2D exitL2d = exitLight.AddComponent<Light2D>();
        exitL2d.lightType = Light2D.LightType.Point;
        exitL2d.color = new Color(0.2f, 1f, 0.35f);
        exitL2d.intensity = 2.0f;
        exitL2d.pointLightOuterRadius = 5f;
        Undo.RegisterCreatedObjectUndo(goal, "Meta");

        // 7. Player
        SetupPlayer(new Vector2(26.5f, 4.5f), playerLayer);

        // 8. La Llorona (Dificultad Nivel 2)
        SetupLlorona(new Vector2(26.5f, 40.0f), wpA, wpB, wallsLayer, lloronaLayer, 1.5f, 2.5f, 5.0f, 100.0f);

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
        cam.transform.position = new Vector3(26.5f, 4.5f, -10f);
        cam.backgroundColor = new Color(0.04f, 0.03f, 0.10f);
        CameraFollow2D camFollow = cam.GetComponent<CameraFollow2D>();
        if (camFollow == null) camFollow = cam.gameObject.AddComponent<CameraFollow2D>();
        camFollow.SetBounds(new Vector2(27f, 4f), new Vector2(27f, 44f));

        // 10. Tutorials
        Transform tutRoot = FreshRoot("TutorialTriggers");
        CreateTutorialTrigger(tutRoot, "Tutorial_Inicio",
            new Vector2(26.5f, 4.5f), new Vector2(5f, 5f),
            "¡Callejón estrecho! Usa los escondites (E) y\nevita correr — La Llorona te escucha.",
            showOnStart: true);
        CreateTutorialTrigger(tutRoot, "Tutorial_Giro",
            new Vector2(26.5f, 7.0f), new Vector2(4f, 3f),
            "El callejón gira más adelante. ¡Espía antes de doblar!");
        CreateTutorialTrigger(tutRoot, "Tutorial_Salida",
            new Vector2(26.5f, 42.0f), new Vector2(5f, 4f),
            "¡La salida está cerca! ¡Llega a la luz verde!");

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

    static void CreateFlickeringFarol(Transform parent, string name, Sprite sprite, float x, float y, int layer)
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
        lightGo.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.72f, 0.35f);
        l2d.intensity = 1.25f;
        l2d.pointLightOuterRadius = 4.8f;

        lightGo.AddComponent<LightFlicker>();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void EnsureGlobalLight(float intensity)
    {
        GameObject glGo = GameObject.Find("Global Light 2D");
        if (glGo == null) glGo = new GameObject("Global Light 2D");
        Light2D gl = glGo.GetComponent<Light2D>() ?? glGo.AddComponent<Light2D>();
        gl.lightType = Light2D.LightType.Global;
        gl.color = new Color(0.18f, 0.14f, 0.32f);
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
            // Cargar o crear Carlos básico
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

    static void SetupLlorona(Vector2 pos, Transform wpA, Transform wpB, int wallsLayer, int lloronaLayer, float pSpeed, float cSpeed, float vDist, float vAngle)
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
        SerializedProperty wps = so.FindProperty("waypoints");
        wps.arraySize = 2;
        wps.GetArrayElementAtIndex(0).objectReferenceValue = wpA;
        wps.GetArrayElementAtIndex(1).objectReferenceValue = wpB;
        if (wallsLayer >= 0) so.FindProperty("obstacleMask").intValue = 1 << wallsLayer;

        LloronaSettings s = AssetDatabase.LoadAssetAtPath<LloronaSettings>("Assets/Settings/LloronaSettings_Level2.asset");
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
