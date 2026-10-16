// Assets/Editor/Level1Builder.cs
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
/// Constructor automatizado del Nivel 1 (El Mercado Vacío) según la especificación del GDD.
/// Mapa de 40 x 16 unidades, iluminación URP 2D, escondites con HidePoint,
/// colisiones compuestas, waypoints de patrulla y triggers de tutorial.
///
/// Ejecución:
/// Menú Unity: La Llorona > Construir Nivel 1 Completo
/// Línea de comandos (batchmode): -executeMethod Level1Builder.BuildLevel1Scene
/// </summary>
public static class Level1Builder
{
    const string ScenePath = "Assets/Scenes/Level1_Mercado.unity";

    [MenuItem("La Llorona/Construir Nivel 1 Completo")]
    public static void BuildLevel1Scene()
    {
        Debug.Log("[Level1Builder] Iniciando construcción del Nivel 1...");

        // Abrir o crear la escena
        Scene scene;
        if (File.Exists(ScenePath))
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        else
        {
            Directory.CreateDirectory("Assets/Scenes");
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 1 Completo");

        // Capas y tags
        int wallsLayer = LayerMask.NameToLayer("Walls");
        int hideSpotLayer = LayerMask.NameToLayer("HideSpot");
        int playerLayer = LayerMask.NameToLayer("Player");
        int lloronaLayer = LayerMask.NameToLayer("Llorona");

        if (wallsLayer < 0) wallsLayer = 0;
        if (hideSpotLayer < 0) hideSpotLayer = 0;

        // Cargar recursos
        TileBase[] allTiles = LoadTiles();
        Sprite puestoSprite = LoadSprite("Assets/Sprites/Environment/puesto_mercado.png");
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

        Tilemap sueloMap = GetOrCreateTilemap(gridGo.transform, "Suelo", 0, -1, false);
        Tilemap paredesMap = GetOrCreateTilemap(gridGo.transform, "Paredes", 1, wallsLayer, true);

        sueloMap.ClearAllTiles();
        paredesMap.ClearAllTiles();

        // 2. Construir Suelo (40 x 16)
        // Zona jugable Y: 2 a 13. Tierra en X < 4 y X >= 36; Adoquín con 12% charcos al centro.
        TileBase[] dirtTiles = allTiles.Skip(12).Take(12).ToArray();
        TileBase[] floorTiles = allTiles.Take(6).ToArray();
        TileBase[] puddleTiles = allTiles.Skip(6).Take(6).ToArray();

        System.Random rnd = new System.Random(17);
        for (int x = 0; x < 40; x++)
        {
            for (int y = 2; y <= 13; y++)
            {
                bool isDirt = (x < 4 || x >= 36);
                TileBase t;
                if (isDirt)
                {
                    t = dirtTiles.Length > 0 ? dirtTiles[rnd.Next(dirtTiles.Length)] : null;
                }
                else
                {
                    bool hasPuddle = (puddleTiles.Length > 0 && rnd.NextDouble() < 0.12);
                    t = hasPuddle
                        ? puddleTiles[rnd.Next(puddleTiles.Length)]
                        : (floorTiles.Length > 0 ? floorTiles[rnd.Next(floorTiles.Length)] : null);
                }

                if (t != null)
                    sueloMap.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // 3. Construir Fachadas y Muros (Filas 0-1 abajo y 14-15 arriba)
        TileBase[] wallUpper = allTiles.Skip(36).Take(12).ToArray();
        TileBase[] wallLower = allTiles.Skip(24).Take(12).ToArray();

        for (int x = 0; x < 40; x++)
        {
            if (wallUpper.Length > 0)
            {
                paredesMap.SetTile(new Vector3Int(x, 15, 0), wallUpper[x % wallUpper.Length]);
                paredesMap.SetTile(new Vector3Int(x, 1, 0), wallUpper[x % wallUpper.Length]);
            }
            if (wallLower.Length > 0)
            {
                paredesMap.SetTile(new Vector3Int(x, 14, 0), wallLower[x % wallLower.Length]);
                paredesMap.SetTile(new Vector3Int(x, 0, 0), wallLower[x % wallLower.Length]);
            }
        }

        // 4. Configurar CompositeCollider2D en Paredes (Unity 6)
        ConfigureCompositeCollider(paredesMap.gameObject);

        // 5. Props y Escondites
        Transform propsRoot = FreshRoot("Props");

        // Escondites con HideSpot y HidePoint
        Sprite h1 = hideSprites != null && hideSprites.Length > 0 ? hideSprites[0] : null;
        Sprite h2 = hideSprites != null && hideSprites.Length > 1 ? hideSprites[1] : null;
        Sprite h3 = hideSprites != null && hideSprites.Length > 2 ? hideSprites[2] : null;

        CreateHideSpot(propsRoot, "Escondite_1_Cajas", h1, 8.0f, 11.0f, 1.3f, hideSpotLayer);
        CreateHideSpot(propsRoot, "Escondite_2_Barril", h2, 18.0f, 11.0f, 1.05f, hideSpotLayer);
        CreateHideSpot(propsRoot, "Escondite_3_Puerta", h3, 26.5f, 13.0f, 1.0f, hideSpotLayer);

        // Obstáculos sólidos (Puestos)
        CreateSolidProp(propsRoot, "Puesto_1", puestoSprite, 14.5f, 6.8f, 1.0f, wallsLayer, new Vector2(2.5f, 0.8f), new Vector2(0f, -0.6f));
        CreateSolidProp(propsRoot, "Puesto_2", puestoSprite, 21.5f, 3.4f, 1.0f, wallsLayer, new Vector2(2.5f, 0.8f), new Vector2(0f, -0.6f));

        // Sprites de decoración
        Sprite farol = GetDecorSprite(decorSprites, 0);
        Sprite sacos = GetDecorSprite(decorSprites, 1);
        Sprite banca = GetDecorSprite(decorSprites, 2);
        Sprite bote = GetDecorSprite(decorSprites, 3);
        Sprite cartel = GetDecorSprite(decorSprites, 4);
        Sprite papel = GetDecorSprite(decorSprites, 5);

        // Faroles con Light2D puntual cálida
        CreateFarol(propsRoot, "Farol_1", farol, 2.2f, 11.0f, wallsLayer);
        CreateFarol(propsRoot, "Farol_2", farol, 17.0f, 2.5f, wallsLayer);
        CreateFarol(propsRoot, "Farol_3", farol, 30.5f, 12.0f, wallsLayer);
        CreateFarol(propsRoot, "Farol_4", farol, 30.5f, 2.5f, wallsLayer);

        // Otros obstáculos y decoración
        CreateSolidProp(propsRoot, "Bote", bote, 11.5f, 3.8f, 0.7f, wallsLayer, new Vector2(0.6f, 0.4f), new Vector2(0f, -0.3f));
        CreateSolidProp(propsRoot, "Banca", banca, 12.0f, 12.4f, 0.9f, wallsLayer, new Vector2(1.2f, 0.4f), new Vector2(0f, -0.3f));
        CreateSolidProp(propsRoot, "Costales_1", sacos, 25.0f, 9.7f, 0.95f, wallsLayer, new Vector2(1.0f, 0.5f), new Vector2(0f, -0.2f));
        CreateSolidProp(propsRoot, "Costales_2", sacos, 34.0f, 2.8f, 0.95f, wallsLayer, new Vector2(1.0f, 0.5f), new Vector2(0f, -0.2f));

        CreateDecoProp(propsRoot, "Cartel", cartel, 4.2f, 10.8f, 1.1f);
        CreateDecoProp(propsRoot, "PapelPicado_1", papel, 5.5f, 12.9f, 1.3f);
        CreateDecoProp(propsRoot, "PapelPicado_2", papel, 33.0f, 12.9f, 1.3f);

        // 6. Límites laterales invisibles
        Transform limitsRoot = FreshRoot("LimitesInvisibles");
        CreateInvisibleWall(limitsRoot, "Limite_Izquierdo", -0.5f, 8.0f, wallsLayer);
        CreateInvisibleWall(limitsRoot, "Limite_Derecho", 40.5f, 8.0f, wallsLayer);

        // 7. Iluminación Global URP 2D
        EnsureGlobalLight();

        // 8. Waypoints de patrulla de La Llorona
        Transform wpRoot = FreshRoot("Waypoints");
        Transform wpA = CreateEmpty(wpRoot, "WP_A", 29.0f, 12.4f);
        Transform wpB = CreateEmpty(wpRoot, "WP_B", 29.0f, 3.6f);

        // 9. Meta de pared a pared en x ≈ 39.2
        GameObject oldGoal = GameObject.Find("Meta");
        if (oldGoal != null) Undo.DestroyObjectImmediate(oldGoal);

        GameObject goal = new GameObject("Meta");
        goal.transform.position = new Vector3(39.2f, 8.0f, 0f);
        BoxCollider2D goalCol = goal.AddComponent<BoxCollider2D>();
        goalCol.isTrigger = true;
        goalCol.size = new Vector2(1.5f, 13.0f); // De pared a pared
        goal.AddComponent<LevelGoal>();
        Undo.RegisterCreatedObjectUndo(goal, "Meta");

        // 10. Triggers de Tutorial Contextual
        Transform tutRoot = FreshRoot("TutorialTriggers");
        CreateTutorialTrigger(tutRoot, "Tutorial_Movimiento", new Vector2(4.0f, 7.0f), new Vector2(2f, 6f),
            "WASD o Flechas para moverte.\nMantén Shift para correr (pero haces ruido).");
        CreateTutorialTrigger(tutRoot, "Tutorial_Escondite", new Vector2(7.0f, 10.5f), new Vector2(3f, 4f),
            "Acércate al escondite y presiona E para ocultarte.");
        CreateTutorialTrigger(tutRoot, "Tutorial_Peligro", new Vector2(23.5f, 7.5f), new Vector2(2f, 8f),
            "¡Cuidado! Se escucha un llanto cercano.\nEspera a que La Llorona se aleje antes de cruzar.");

        // 11. Jugador (Carlos)
        SetupPlayer(new Vector2(2.5f, 7.0f), playerLayer);

        // 12. La Llorona
        SetupLlorona(new Vector2(29.0f, 3.6f), wpA, wpB, wallsLayer, lloronaLayer);

        // 13. Cámara
        SetupCamera(new Vector2(2.5f, 7.0f));

        // 14. Managers de la escena
        SetupManagers();

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("[Level1Builder] ¡Nivel 1 construido y guardado con éxito en: " + ScenePath + "!");
    }

    // ---------------- Métodos Auxiliares ----------------

    static TileBase[] LoadTiles()
    {
        TileBase[] tiles = new TileBase[48];
        for (int i = 0; i < 48; i++)
        {
            string path = $"Assets/Tiles/tiles_{i}.asset";
            tiles[i] = AssetDatabase.LoadAssetAtPath<TileBase>(path);
        }
        return tiles;
    }

    static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Sprite[] LoadSubSprites(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }

    static Sprite GetDecorSprite(Sprite[] arr, int index)
    {
        if (arr != null && index >= 0 && index < arr.Length)
            return arr[index];
        return null;
    }

    static Tilemap GetOrCreateTilemap(Transform grid, string name, int order, int layer, bool hasCollider)
    {
        Transform child = grid.Find(name);
        GameObject go;
        if (child == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(grid, false);
            go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>();
            Undo.RegisterCreatedObjectUndo(go, "Tilemap " + name);
        }
        else
        {
            go = child.gameObject;
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
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        // Unity 6 API: Collider2D.compositeOperation
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

        Vector2 size = sprite.bounds.size;

        // Trigger que sobresale 0.6 por el frente
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(size.x + (0.6f / scale), size.y + (0.6f / scale));

        // Componente HideSpot y objeto hijo HidePoint
        HideSpot hs = go.AddComponent<HideSpot>();

        GameObject hp = new GameObject("HidePoint");
        hp.transform.SetParent(go.transform, false);
        hp.transform.localPosition = new Vector3(0f, 0.4f, 0f); // Detrás del objeto

        // Asignar campo hidePoint mediante SerializedObject
        SerializedObject so = new SerializedObject(hs);
        SerializedProperty hpProp = so.FindProperty("hidePoint");
        if (hpProp != null) hpProp.objectReferenceValue = hp.transform;
        so.ApplyModifiedProperties();

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

    static void CreateDecoProp(Transform parent, string name, Sprite sprite, float x, float y, float scale)
    {
        if (sprite == null) return;

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(scale, scale, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

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

        // Luz cálida 2D
        GameObject lightGo = new GameObject("LuzFarol");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.75f, 0.4f); // Cálida
        l2d.intensity = 1.3f;
        l2d.pointLightOuterRadius = 5.0f;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateInvisibleWall(Transform parent, string name, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        if (layer >= 0) go.layer = layer;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(1.0f, 16.0f);
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void EnsureGlobalLight()
    {
        GameObject glGo = GameObject.Find("Global Light 2D");
        if (glGo == null)
        {
            glGo = new GameObject("Global Light 2D");
            Undo.RegisterCreatedObjectUndo(glGo, "Global Light 2D");
        }

        Light2D gl = glGo.GetComponent<Light2D>();
        if (gl == null) gl = glGo.AddComponent<Light2D>();

        gl.lightType = Light2D.LightType.Global;
        gl.color = new Color(0.22f, 0.18f, 0.38f); // Azul morado nocturno
        gl.intensity = 0.22f;
    }

    static void CreateTutorialTrigger(Transform parent, string name, Vector2 pos, Vector2 size, string text)
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
        so.FindProperty("displayDuration").floatValue = 4.5f;
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
        else
        {
            Debug.LogWarning("[Level1Builder] No se encontró GameObject con tag 'Player'.");
        }
    }

    static void SetupLlorona(Vector2 pos, Transform wpA, Transform wpB, int wallsLayer, int lloronaLayer)
    {
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona != null)
        {
            Undo.RecordObject(llorona.transform, "Posicionar Llorona");
            llorona.transform.position = new Vector3(pos.x, pos.y, 0f);
            if (lloronaLayer >= 0) llorona.gameObject.layer = lloronaLayer;

            SerializedObject so = new SerializedObject(llorona);
            SerializedProperty wps = so.FindProperty("waypoints");
            wps.arraySize = 2;
            wps.GetArrayElementAtIndex(0).objectReferenceValue = wpA;
            wps.GetArrayElementAtIndex(1).objectReferenceValue = wpB;

            if (wallsLayer >= 0)
                so.FindProperty("obstacleMask").intValue = 1 << wallsLayer;

            LloronaSettings s = AssetDatabase.LoadAssetAtPath<LloronaSettings>("Assets/Settings/LloronaSettings_Level1.asset");
            if (s != null) so.FindProperty("settings").objectReferenceValue = s;

            // Dificultad Nivel 1 según GDD:
            // patrolSpeed 1.2, chaseSpeed 2.0, viewDistance 4.0, viewAngle 90
            so.FindProperty("patrolSpeed").floatValue = 1.2f;
            so.FindProperty("chaseSpeed").floatValue = 2.0f;
            so.FindProperty("viewDistance").floatValue = 4.0f;
            so.FindProperty("viewAngle").floatValue = 90.0f;

            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("[Level1Builder] No se encontró LloronaAI en la escena.");
        }
    }

    static void SetupCamera(Vector2 pos)
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Undo.RecordObject(cam.transform, "Configurar Cámara");
            cam.transform.position = new Vector3(pos.x, pos.y, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;

            CameraFollow2D follow = cam.GetComponent<CameraFollow2D>();
            if (follow == null)
                follow = cam.gameObject.AddComponent<CameraFollow2D>();

            follow.SetBounds(new Vector2(8.8f, 7.0f), new Vector2(31.2f, 9.0f));
        }
    }

    static void SetupManagers()
    {
        if (UnityEngine.Object.FindAnyObjectByType<GameManager>() == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gm, "GameManager");
        }
    }
}