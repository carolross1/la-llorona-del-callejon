using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Herramienta de editor: arma el Nivel 1 según el boceto (40 x 16 unidades).
// IMPORTANTE: este archivo debe estar en una carpeta llamada "Editor" (Assets/Editor/).
// Menú: La Llorona > Construir Nivel 1 (boceto)
//
// El origen (0,0) es la esquina inferior izquierda del mapa. 1 unidad = 1 tile.
public class Level1Builder : EditorWindow
{
    [Header("Pisos (selecciona varios tiles y arrástralos sobre el nombre de la lista)")]
    public TileBase[] floorTiles;       // empedrado (zonas 2 y 3)
    public TileBase[] puddleTiles;      // empedrado con charcos (se mezclan de vez en cuando)
    public TileBase[] dirtTiles;        // tierra con plantas (zonas 1 y 4)
    [Range(0f, 0.5f)] public float puddleChance = 0.12f;

    [Header("Paredes (en orden de izquierda a derecha, como vienen en la hoja)")]
    public TileBase[] wallUpperTiles;   // fila de arriba de la pared (con faroles y ventanas)
    public TileBase[] wallLowerTiles;   // fila de abajo de la pared (la más oscura)

    [Header("Escondites")]
    public Sprite hide1Sprite;          // puesto con cajas (zona 1)
    public Sprite hide2Sprite;          // barril con lona (zona 2)
    public Sprite hide3Sprite;          // puerta con cajas (zona 3)

    [Header("Obstáculos y decoración")]
    public Sprite puestoSprite;         // puesto azul grande (ya trae sus cajas)
    public Sprite farolSprite;
    public Sprite boteSprite;
    public Sprite sacosSprite;
    public Sprite bancaSprite;
    public Sprite cartelSprite;         // cartel "Feria Patronal"
    public Sprite papelPicadoSprite;

    [Header("Opciones")]
    public bool clearTiles = true;

    static readonly string[] FieldNames =
    {
        "floorTiles", "puddleTiles", "dirtTiles", "puddleChance",
        "wallUpperTiles", "wallLowerTiles",
        "hide1Sprite", "hide2Sprite", "hide3Sprite",
        "puestoSprite", "farolSprite", "boteSprite", "sacosSprite",
        "bancaSprite", "cartelSprite", "papelPicadoSprite",
        "clearTiles"
    };

    enum PropKind { Solid, Hide, Deco }

    Vector2 scroll;

    [MenuItem("La Llorona/Construir Nivel 1 (boceto)")]
    static void Open()
    {
        GetWindow<Level1Builder>("Nivel 1");
    }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.HelpBox(
            "Abre la escena Level1_Mercado, arrastra tus tiles y sprites, y presiona Construir.\n" +
            "Lo que dejes vacío se omite. Puedes volver a construir cuantas veces quieras (borra lo anterior).",
            MessageType.Info);

        SerializedObject so = new SerializedObject(this);
        so.Update();
        foreach (string n in FieldNames)
        {
            SerializedProperty p = so.FindProperty(n);
            if (p != null) EditorGUILayout.PropertyField(p, true);
        }
        so.ApplyModifiedProperties();

        EditorGUILayout.Space();
        if (GUILayout.Button("Construir Nivel 1", GUILayout.Height(34)))
            Build();

        EditorGUILayout.EndScrollView();
    }

    void Build()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Sal del modo Play antes de construir el nivel.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 1");

        int walls = LayerMask.NameToLayer("Walls");
        if (walls < 0) Debug.LogWarning("Falta la capa 'Walls' (Project Settings > Tags and Layers).");

        // ---------- Grid y tilemaps ----------
        GameObject gridGo = GameObject.Find("Grid");
        if (gridGo == null)
        {
            gridGo = new GameObject("Grid");
            gridGo.AddComponent<Grid>();
            Undo.RegisterCreatedObjectUndo(gridGo, "Grid");
        }

        Tilemap suelo = GetOrCreateTilemap(gridGo.transform, "Suelo", 0, -1, false);
        Tilemap paredes = GetOrCreateTilemap(gridGo.transform, "Paredes", 1, walls, true);

        if (clearTiles)
        {
            Undo.RegisterCompleteObjectUndo(suelo, "Limpiar suelo");
            Undo.RegisterCompleteObjectUndo(paredes, "Limpiar paredes");
            suelo.ClearAllTiles();
            paredes.ClearAllTiles();
        }

        // ---------- Piso: tierra en zonas 1 y 4, empedrado en el centro ----------
        if (floorTiles == null || floorTiles.Length == 0) Debug.LogWarning("Faltan los tiles del piso (empedrado).");
        System.Random rnd = new System.Random(7);   // semilla fija: siempre queda igual
        for (int x = 0; x < 40; x++)
        {
            for (int y = 2; y <= 13; y++)
            {
                bool edge = x < 4 || x >= 36;
                TileBase t;
                if (edge)
                {
                    t = Pick(dirtTiles != null && dirtTiles.Length > 0 ? dirtTiles : floorTiles, rnd);
                }
                else
                {
                    t = Pick(floorTiles, rnd);
                    if (puddleTiles != null && puddleTiles.Length > 0 && rnd.NextDouble() < puddleChance)
                        t = Pick(puddleTiles, rnd);
                }
                suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // ---------- Paredes: 2 filas arriba (y 14-15) y 2 abajo (y 0-1) ----------
        if (wallUpperTiles == null || wallUpperTiles.Length == 0) Debug.LogWarning("Faltan los tiles de la pared.");
        TileBase[] lower = wallLowerTiles != null && wallLowerTiles.Length > 0 ? wallLowerTiles : wallUpperTiles;
        for (int x = 0; x < 40; x++)
        {
            paredes.SetTile(new Vector3Int(x, 15, 0), InOrder(wallUpperTiles, x));
            paredes.SetTile(new Vector3Int(x, 14, 0), InOrder(lower, x));
            paredes.SetTile(new Vector3Int(x, 1, 0), InOrder(wallUpperTiles, x));
            paredes.SetTile(new Vector3Int(x, 0, 0), InOrder(lower, x));
        }

        // ---------- Props ----------
        // Los números del final son la escala: ajustan cada sprite al tamaño del boceto.
        Transform props = FreshRoot("Props");

        // Escondites (HideSpot, trigger)
        MakeProp(props, "Escondite_1", hide1Sprite, 8.2f, 12.0f, 1.30f, PropKind.Hide, walls);
        MakeProp(props, "Escondite_2", hide2Sprite, 17.8f, 12.0f, 1.05f, PropKind.Hide, walls);
        MakeProp(props, "Escondite_3", hide3Sprite, 26.1f, 14.2f, 1.00f, PropKind.Hide, walls);

        // Obstáculos sólidos
        MakeProp(props, "Puesto_1", puestoSprite, 14.5f, 9.0f, 1.0f, PropKind.Solid, walls);
        MakeProp(props, "Puesto_2", puestoSprite, 21.7f, 5.5f, 1.0f, PropKind.Solid, walls);
        MakeProp(props, "Farol_1", farolSprite, 2.2f, 12.0f, 0.83f, PropKind.Solid, walls);
        MakeProp(props, "Farol_2", farolSprite, 17.0f, 4.5f, 0.83f, PropKind.Solid, walls);
        MakeProp(props, "Farol_3", farolSprite, 30.5f, 13.4f, 0.83f, PropKind.Solid, walls);
        MakeProp(props, "Farol_4", farolSprite, 30.5f, 4.6f, 0.83f, PropKind.Solid, walls);
        MakeProp(props, "Sacos_1", sacosSprite, 24.9f, 10.7f, 0.95f, PropKind.Solid, walls);
        MakeProp(props, "Sacos_2", sacosSprite, 34.0f, 3.9f, 0.95f, PropKind.Solid, walls);
        MakeProp(props, "Bote", boteSprite, 11.3f, 4.4f, 0.64f, PropKind.Solid, walls);

        // Decoración (sin colisión)
        MakeProp(props, "Cartel", cartelSprite, 4.2f, 12.0f, 1.11f, PropKind.Deco, walls);
        MakeProp(props, "Papel_1", papelPicadoSprite, 8.0f, 13.9f, 1.35f, PropKind.Deco, walls);
        MakeProp(props, "Papel_2", papelPicadoSprite, 30.9f, 13.9f, 1.35f, PropKind.Deco, walls);
        MakeProp(props, "Banca", bancaSprite, 12.1f, 13.2f, 0.91f, PropKind.Deco, walls);

        // ---------- Límites invisibles a izquierda y derecha ----------
        Transform limits = FreshRoot("LimitesInvisibles");
        MakeWall(limits, "Limite_Izq", -0.5f, 8f, walls);
        MakeWall(limits, "Limite_Der", 40.5f, 8f, walls);

        // ---------- Waypoints de La Llorona ----------
        Transform wpRoot = FreshRoot("Waypoints");
        Transform wpA = MakeEmpty(wpRoot, "WP_A", 29.0f, 12.4f);
        Transform wpB = MakeEmpty(wpRoot, "WP_B", 29.0f, 3.6f);

        // ---------- Meta ----------
        GameObject oldGoal = GameObject.Find("Meta");
        if (oldGoal != null) Undo.DestroyObjectImmediate(oldGoal);
        GameObject goal = new GameObject("Meta");
        goal.transform.position = new Vector3(38.9f, 8.5f, 0f);
        BoxCollider2D goalCol = goal.AddComponent<BoxCollider2D>();
        goalCol.isTrigger = true;
        goalCol.size = new Vector2(1.6f, 3f);
        goal.AddComponent<LevelGoal>();
        Undo.RegisterCreatedObjectUndo(goal, "Meta");

        // ---------- Player, cámara y Llorona ----------
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Undo.RecordObject(player.transform, "Mover Player");
            player.transform.position = new Vector3(2.5f, 7.5f, 0f);
        }
        else
        {
            Debug.LogWarning("No encontré al Player (tag Player). Colócalo en (2.5, 7.5).");
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            Undo.RecordObject(cam.transform, "Mover cámara");
            cam.transform.position = new Vector3(2.5f, 7.5f, -10f);
        }

        LloronaAI llorona = FindFirstObjectByType<LloronaAI>();
        if (llorona != null)
        {
            Undo.RecordObject(llorona.transform, "Mover Llorona");
            llorona.transform.position = new Vector3(29.0f, 3.6f, 0f);

            // waypoints y obstacleMask son privados ([SerializeField]), por eso se asignan así
            SerializedObject llSo = new SerializedObject(llorona);
            SerializedProperty wps = llSo.FindProperty("waypoints");
            wps.arraySize = 2;
            wps.GetArrayElementAtIndex(0).objectReferenceValue = wpA;
            wps.GetArrayElementAtIndex(1).objectReferenceValue = wpB;
            if (walls >= 0)
                llSo.FindProperty("obstacleMask").intValue = 1 << walls;
            llSo.ApplyModifiedProperties();
        }
        else
        {
            Debug.Log("No hay La Llorona en la escena (LloronaAI). Agrégala y vuelve a construir.");
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Nivel 1 construido. Revisa la ventana Scene y guarda la escena (Ctrl+S).");
    }

    // ---------------- Auxiliares ----------------

    static TileBase Pick(TileBase[] arr, System.Random rnd)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[rnd.Next(arr.Length)];
    }

    // Respeta el orden de la hoja: columna 0, 1, 2... y vuelve a empezar
    static TileBase InOrder(TileBase[] arr, int x)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[x % arr.Length];
    }

    static Tilemap GetOrCreateTilemap(Transform grid, string name, int order, int layer, bool collider)
    {
        Transform t = grid.Find(name);
        GameObject go;
        if (t == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(grid, false);
            go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>();
            Undo.RegisterCreatedObjectUndo(go, "Tilemap " + name);
        }
        else
        {
            go = t.gameObject;
        }

        TilemapRenderer r = go.GetComponent<TilemapRenderer>();
        if (r != null) r.sortingOrder = order;
        if (layer >= 0) go.layer = layer;
        if (collider && go.GetComponent<TilemapCollider2D>() == null)
            go.AddComponent<TilemapCollider2D>();

        return go.GetComponent<Tilemap>();
    }

    // Borra el objeto raíz anterior (si existe) y crea uno nuevo, para poder reconstruir.
    static Transform FreshRoot(string name)
    {
        GameObject old = GameObject.Find(name);
        if (old != null) Undo.DestroyObjectImmediate(old);

        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, name);
        return go.transform;
    }

    static Transform MakeEmpty(Transform parent, string name, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        return go.transform;
    }

    static void MakeWall(Transform parent, string name, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(1f, 16f);
        if (layer >= 0) go.layer = layer;
    }

    static void MakeProp(Transform parent, string name, Sprite sprite, float x, float y,
                         float scale, PropKind kind, int walls)
    {
        if (sprite == null)
        {
            Debug.LogWarning("Sin sprite para " + name + " (se omite).");
            return;
        }

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(scale, scale, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        Vector2 size = sprite.bounds.size;   // en unidades locales: el collider se escala con el objeto

        switch (kind)
        {
            case PropKind.Solid:
                {
                    // Collider solo en la base, para poder pasar "por detrás" de la parte alta
                    BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
                    bc.size = new Vector2(size.x * 0.85f, size.y * 0.5f);
                    bc.offset = new Vector2(0f, -size.y * 0.25f);
                    if (walls >= 0) go.layer = walls;   // también bloquea la vista de La Llorona
                    break;
                }
            case PropKind.Hide:
                {
                    try { go.tag = "HideSpot"; }
                    catch (UnityException) { Debug.LogError("Falta el tag 'HideSpot' (Project Settings > Tags and Layers)."); }

                    BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
                    bc.isTrigger = true;
                    bc.size = size + new Vector2(0.6f, 0.6f) / scale;
                    break;
                }
        }

        Undo.RegisterCreatedObjectUndo(go, name);
    }
}