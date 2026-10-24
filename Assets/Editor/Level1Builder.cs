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
/// Constructor automatizado del Nivel 1 (El Mercado Vacío).
/// Incluye mapa de 40 x 16, iluminación URP 2D, escondites,
/// obstáculos de lodo ralentizante, latas ruidosas, perro callejero territorial
/// y portal monumental de salida claramente señalizado.
/// </summary>
public static class Level1Builder
{
    const string ScenePath = "Assets/Scenes/Level1_Mercado.unity";

    [MenuItem("La Llorona/Construir Nivel 1 Completo")]
    public static void BuildLevel1Scene()
    {
        Debug.Log("[Level1Builder] Iniciando construcción del Nivel 1...");

        // Asegurar que existan todos los sprites del juego
        SpriteAssetGenerator.GenerateAllSprites();

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

        // 3. Fachadas y Muros (Filas 0-1 abajo y 14-15 arriba)
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

        // 5. Props, Escondites y Obstáculos
        Transform propsRoot = FreshRoot("Props");

        // Escondites
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

        // Faroles con Light2D puntual cálida marcando el sendero principal
        CreateFarol(propsRoot, "Farol_1", farol,  2.2f, 11.0f, wallsLayer);
        CreateFarol(propsRoot, "Farol_2", farol, 10.0f,  2.5f, wallsLayer);
        CreateFarol(propsRoot, "Farol_3", farol, 10.0f, 12.5f, wallsLayer);
        CreateFarol(propsRoot, "Farol_4", farol, 17.0f,  2.5f, wallsLayer);
        CreateFarol(propsRoot, "Farol_5", farol, 17.0f, 12.5f, wallsLayer);
        CreateFarol(propsRoot, "Farol_6", farol, 24.0f,  7.5f, wallsLayer);
        CreateFarol(propsRoot, "Farol_7", farol, 30.5f, 12.0f, wallsLayer);
        CreateFarol(propsRoot, "Farol_8", farol, 30.5f,  2.5f, wallsLayer);
        CreateFarol(propsRoot, "Farol_9", farol, 37.0f,  7.5f, wallsLayer);

        // Obstáculos del entorno con consecuencias
        CreateMudPuddle(propsRoot, 11.0f, 7.0f, 1.2f);
        CreateMudPuddle(propsRoot, 22.0f, 6.5f, 1.1f);
        CreateNoiseObstacle(propsRoot, 15.0f, 11.2f);
        CreateNoiseObstacle(propsRoot, 26.5f, 5.0f);
        CreateStrayDog(propsRoot, 18.0f, 3.6f);

        // Decoración secundaria
        CreateSolidProp(propsRoot, "Bote",       bote,  11.5f,  3.8f, 0.7f, wallsLayer, new Vector2(0.6f, 0.4f), new Vector2(0f, -0.3f));
        CreateSolidProp(propsRoot, "Banca",      banca, 12.0f, 12.4f, 0.9f, wallsLayer, new Vector2(1.2f, 0.4f), new Vector2(0f, -0.3f));
        CreateSolidProp(propsRoot, "Costales_1", sacos, 25.0f,  9.7f, 0.95f, wallsLayer, new Vector2(1.0f, 0.5f), new Vector2(0f, -0.2f));
        CreateSolidProp(propsRoot, "Costales_2", sacos, 34.0f,  2.8f, 0.95f, wallsLayer, new Vector2(1.0f, 0.5f), new Vector2(0f, -0.2f));

        CreateDecoProp(propsRoot, "Cartel",        cartel, 4.2f, 10.8f, 1.1f);
        CreateDecoProp(propsRoot, "PapelPicado_1", papel,  5.5f, 12.9f, 1.3f);
        CreateDecoProp(propsRoot, "PapelPicado_2", papel, 33.0f, 12.9f, 1.3f);

        // ── Portal Monumental de Salida (X ≈ 38.5) ──────────────────────────
        CreateExitPortal(propsRoot, 38.5f, 7.5f);

        // 6. Límites laterales invisibles
        Transform limitsRoot = FreshRoot("LimitesInvisibles");
        CreateInvisibleWall(limitsRoot, "Limite_Izquierdo", -0.5f, 8.0f, wallsLayer);
        CreateInvisibleWall(limitsRoot, "Limite_Derecho",   40.5f, 8.0f, wallsLayer);

        // 7. Iluminación Global URP 2D (más brillante para visibilidad clara)
        EnsureGlobalLight();

        // 8. Waypoints de patrulla de La Llorona
        Transform wpRoot = FreshRoot("Waypoints");
        Transform wpA = CreateEmpty(wpRoot, "WP_A", 29.0f, 12.4f);
        Transform wpB = CreateEmpty(wpRoot, "WP_B", 29.0f,  3.6f);

        // 9. Meta de seguridad en X ≈ 39.0 (columna de escape)
        GameObject oldGoal = GameObject.Find("Meta");
        if (oldGoal != null) Undo.DestroyObjectImmediate(oldGoal);

        GameObject goal = new GameObject("Meta");
        goal.transform.position = new Vector3(39.0f, 7.5f, 0f);
        BoxCollider2D goalCol = goal.AddComponent<BoxCollider2D>();
        goalCol.isTrigger = true;
        goalCol.size = new Vector2(2.5f, 12.0f);
        goal.AddComponent<LevelGoal>();
        Undo.RegisterCreatedObjectUndo(goal, "Meta");

        // 10. Triggers de Tutorial Contextual
        Transform tutRoot = FreshRoot("TutorialTriggers");

        CreateTutorialTrigger(tutRoot, "Tutorial_Inicio",
            new Vector2(2.5f, 7.0f), new Vector2(4f, 8f),
            "Usa WASD para moverte y SHIFT para correr.\n¡Sigue la flecha en la brújula para no perderte!",
            showOnStart: true);

        CreateTutorialTrigger(tutRoot, "Tutorial_Charco",
            new Vector2(10.5f, 7.0f), new Vector2(3f, 4f),
            "¡Charco de lodo! Te ralentiza al cruzarlo.\nEvítalo si La Llorona está cerca.");

        CreateTutorialTrigger(tutRoot, "Tutorial_Escondite",
            new Vector2(8.0f, 11.0f), new Vector2(3f, 4f),
            "Presiona E cerca de un escondite para ocultarte.\nVuelve a presionar E para salir.");

        CreateTutorialTrigger(tutRoot, "Tutorial_Ruido",
            new Vector2(14.5f, 11.0f), new Vector2(3f, 3f),
            "¡Cuidado con las latas en el suelo!\nSi las pisas, La Llorona escuchará el ruido.");

        CreateTutorialTrigger(tutRoot, "Tutorial_Perro",
            new Vector2(17.5f, 4.5f), new Vector2(4f, 3f),
            "¡Un perro callejero vigila la esquina!\nCamina despacio; si corres, ladrará y alertará a La Llorona.");

        CreateTutorialTrigger(tutRoot, "Tutorial_Peligro",
            new Vector2(24.0f, 7.5f), new Vector2(2f, 8f),
            "¡Cuidado! Se escucha un llanto cercano.\nEspera a que La Llorona se aleje antes de cruzar.");

        CreateTutorialTrigger(tutRoot, "Tutorial_Salida",
            new Vector2(35.5f, 7.5f), new Vector2(3f, 8f),
            "¡La salida está justo enfrente! Cruza el arco verde luminoso.");

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

        // Construir HUD
        UIBuilder.BuildLevel1UICanvas();
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

    static Sprite LoadSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

    static Sprite[] LoadSubSprites(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();

    static Sprite GetDecorSprite(Sprite[] arr, int index) =>
        (arr != null && index < arr.Length) ? arr[index] : null;

    static Transform FreshRoot(string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null) Undo.DestroyObjectImmediate(existing);
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, name);
        return go.transform;
    }

    static Tilemap GetOrCreateTilemap(Transform parent, string name, int order, int layer, bool hasCollider)
    {
        Transform child = parent.Find(name);
        GameObject go;
        if (child == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(go, name);
        }
        else
        {
            go = child.gameObject;
        }

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
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
            rb.simulated = true;
        }
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
        l2d.color = new Color(1.0f, 0.82f, 0.45f);
        l2d.intensity = 1.35f;
        l2d.pointLightInnerRadius = 0.5f;
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
        bc.size = new Vector2(3.0f, 4.5f);
        go.AddComponent<LevelGoal>();

        // Luz esmeralda mística pulsante
        GameObject lightGo = new GameObject("LuzSalida");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = Vector3.zero;
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.2f, 1.0f, 0.45f);
        l2d.intensity = 2.4f;
        l2d.pointLightOuterRadius = 5.5f;
        lightGo.AddComponent<LightFlicker>();

        // Cartel flotante superior
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

        BoxCollider2D triggerCol = go.AddComponent<BoxCollider2D>();
        triggerCol.isTrigger = true;
        triggerCol.size = new Vector2(1.2f, 1.2f);

        BoxCollider2D solidCol = go.AddComponent<BoxCollider2D>();
        solidCol.isTrigger = false;
        solidCol.size = new Vector2(0.8f, 0.4f);
        solidCol.offset = new Vector2(0f, -0.4f);

        GameObject hidePt = new GameObject("HidePoint");
        hidePt.transform.SetParent(go.transform, false);
        hidePt.transform.localPosition = new Vector3(0f, -0.1f, 0f);

        HideSpot hs = go.AddComponent<HideSpot>();
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("hidePoint").objectReferenceValue = hidePt.transform;
        so.ApplyModifiedProperties();

        if (layer >= 0) go.layer = layer;
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateSolidProp(Transform parent, string name, Sprite sprite, float x, float y, float scale,
        int layer, Vector2 colSize, Vector2 colOffset)
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

    static void CreateDecoProp(Transform parent, string name, Sprite sprite, float x, float y, float scale)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateInvisibleWall(Transform parent, string name, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 16f);

        if (layer >= 0) go.layer = layer;
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void EnsureGlobalLight()
    {
        Light2D gl = UnityEngine.Object.FindAnyObjectByType<Light2D>();
        GameObject glGo = null;
        if (gl != null && gl.lightType == Light2D.LightType.Global)
        {
            glGo = gl.gameObject;
        }
        else
        {
            glGo = new GameObject("LuzGlobal2D");
            gl = glGo.AddComponent<Light2D>();
            Undo.RegisterCreatedObjectUndo(glGo, "LuzGlobal2D");
        }
        gl.lightType = Light2D.LightType.Global;
        gl.color = new Color(0.95f, 0.88f, 0.72f);
        gl.intensity = 0.55f;
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
        else
        {
            Debug.LogWarning("[Level1Builder] No se encontró GameObject con tag 'Player'.");
        }
    }

    static void SetupLlorona(Vector2 pos, Transform wpA, Transform wpB, int wallsLayer, int lloronaLayer)
    {
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona == null)
        {
            Debug.LogWarning("[Level1Builder] No se encontró LloronaAI en la escena.");
            return;
        }

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

        SerializedProperty maskProp = so.FindProperty("obstacleMask");
        if (maskProp != null && wallsLayer >= 0)
        {
            maskProp.intValue = 1 << wallsLayer;
        }

        LloronaSettings s1 = AssetDatabase.LoadAssetAtPath<LloronaSettings>("Assets/Settings/LloronaSettings_Level1.asset");
        if (s1 != null)
        {
            so.FindProperty("settings").objectReferenceValue = s1;
        }

        so.ApplyModifiedProperties();
    }

    static void SetupCamera(Vector2 pos)
    {
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
        cam.transform.position = new Vector3(pos.x, pos.y, -10f);

        CameraFollow2D follow = cam.GetComponent<CameraFollow2D>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow2D>();
        follow.SetBounds(new Vector2(8.8f, 5.0f), new Vector2(31.2f, 11.0f));
    }

    static void SetupManagers()
    {
        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject gmGo = new GameObject("GameManager");
            gm = gmGo.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gmGo, "GameManager");
        }
        SerializedObject so = new SerializedObject(gm);
        so.FindProperty("levelNumber").intValue = 1;
        so.ApplyModifiedProperties();
    }
}
