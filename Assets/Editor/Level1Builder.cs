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
/// Constructor oficial del Nivel 1 (El Mercado Vacío - 56 × 20 unidades).
/// Cumple estrictamente con la especificación visual y jugabilidad de PROMPT_VISUAL.md:
/// - Dimensiones 56 × 20 (zona jugable y=2 a y=18, fachadas en y=0-1 y y=18-19, muros en x=-1 y x=56).
/// - 4 zonas diferenciadas: Zona 1 Entrada (tierra), Zona 2 Calle de puestos (adoquín), Zona 3 Rincón del perro (tierra), Zona 4 Salida (adoquín).
/// - 7 puestos grandes alternando Puesto_frutas y Puesto_verduras con colisión únicamente en la base.
/// - 5 escondites funcionales organizados como clústeres.
/// - Laberinto suave de huacales y costales con rutas transitables (pasillos >= 2.5 u).
/// - 6 charcos oficiales (Charco_0 a Charco_5) y montones de botellas de vidrio reactivas.
/// - Perro callejero durmiendo en (45, 3.8) con ruta alterna superior libre.
/// - Salida_0 como escena monumental de cierre conectada sin cortes, con luz cálida interior y sin flechas.
/// - CERO BANCAS.
/// - Iluminación Global URP 2D a 0.55 y faroles cálidos con LightFlicker.
/// </summary>
public static class Level1Builder
{
    const string ScenePath = "Assets/Scenes/Level1_Mercado.unity";

    // ── Spritesheets oficiales ────────────────────────────────────────────────
    const string PuestosPath    = "Assets/Sprites/Environment/Puestos_Nivl1.png";
    const string SacosPath      = "Assets/Sprites/Environment/Contenedores_Sacos_Nvl1.png";
    const string BotellasPath   = "Assets/Sprites/Environment/Botellas_charcolodo_Nvl1.png";
    const string SalidasPath    = "Assets/Sprites/Environment/Salidas.png";
    const string PerroPath      = "Assets/Sprites/Perro/perro.png";
    const string DecoracionPath = "Assets/Sprites/Environment/decoracion.png";
    const string EsconditesPath = "Assets/Sprites/Environment/escondites.png";

    static Sprite GetSpriteByName(string path, string name, int fallbackIndex = 0)
    {
        if (!File.Exists(path)) return null;
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        var match = sprites.FirstOrDefault(s => s.name == name);
        if (match != null) return match;
        if (fallbackIndex >= 0 && fallbackIndex < sprites.Length) return sprites[fallbackIndex];
        return sprites.FirstOrDefault() ?? AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    [MenuItem("La Llorona/Construir Nivel 1 Completo")]
    public static void BuildLevel1Scene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[Level1Builder] Deteniendo Modo Juego antes de construir el Nivel 1...");
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => BuildLevel1Scene();
            return;
        }

        Debug.Log("[Level1Builder] Iniciando reconstrucción oficial del Nivel 1 (56 × 20)...");

        // Asegurar que las hojas de sprites estén debidamente recortadas
        SpriteSheetSlicer.SliceAllSpriteSheets();

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 1 Mercado");

        // Limpiar TODOS los objetos antiguos o duplicados dentro de la escena abierta
        string[] oldNames = { "puesto_mercado", "puesto_mercado2", "HidePoint", "WP_A", "WP_B", "Arriba", "Abajo", "Derecha", "Izquierda", "TutorialTriggers", "Meta" };
        foreach (var oldName in oldNames)
        {
            var oldObjs = UnityEngine.Object.FindObjectsByType<GameObject>().Where(g => g != null && g.name == oldName).ToArray();
            foreach (var o in oldObjs) Undo.DestroyObjectImmediate(o);
        }

        // Eliminar cualquier LevelGoal invisible residual que no pertenezca a la puerta monumental
        var oldGoals = UnityEngine.Object.FindObjectsByType<LevelGoal>();
        foreach (var g in oldGoals)
        {
            if (g != null && (g.transform.parent == null || !g.transform.parent.name.Contains("Salida")))
            {
                Undo.DestroyObjectImmediate(g.gameObject);
            }
        }

        // Capas físicas
        int wallsLayer    = LayerMask.NameToLayer("Walls");
        int hideSpotLayer = LayerMask.NameToLayer("HideSpot");
        int playerLayer   = LayerMask.NameToLayer("Player");
        int lloronaLayer  = LayerMask.NameToLayer("Llorona");

        if (wallsLayer < 0) wallsLayer = 0;
        if (hideSpotLayer < 0) hideSpotLayer = 0;

        // Cargar baldosas oficiales (0..47)
        TileBase[] allTiles = LoadTiles();
        TileBase[] floorTiles   = allTiles.Take(12).ToArray();               // Adoquín (filas 0-1)
        TileBase[] dirtTiles    = allTiles.Skip(12).Take(12).ToArray();      // Tierra (filas 2-3)
        TileBase[] wallConcrete = allTiles.Skip(24).Take(12).ToArray();      // Muro de concreto (filas 4-5)
        TileBase[] wallFacade   = allTiles.Skip(36).Take(12).ToArray();      // Fachadas con ventanas (filas 6-7)

        // ════════════════════════════════════════════════════════════════════════
        // 1. GRID Y TILEMAPS (Ground y Walls)
        // ════════════════════════════════════════════════════════════════════════
        GameObject gridGo = GameObject.Find("Grid") ?? new GameObject("Grid");
        if (gridGo.GetComponent<Grid>() == null) gridGo.AddComponent<Grid>();
        Undo.RegisterCreatedObjectUndo(gridGo, "Grid");

        Tilemap sueloMap   = GetOrCreateTilemap(gridGo.transform, "Suelo",   -5, -1, false);
        Tilemap paredesMap = GetOrCreateTilemap(gridGo.transform, "Paredes",  3, wallsLayer, true);

        // Ajuste de contraste/post-procesado del suelo:
        // Se atenúa ligeramente el microcontraste y la saturación del empedrado
        // para que Carlos, los puestos, las botellas y los charcos destaquen nítidamente como elementos de juego
        sueloMap.color = new Color(0.64f, 0.65f, 0.72f, 1.0f);
        paredesMap.color = new Color(0.88f, 0.88f, 0.90f, 1.0f);

        sueloMap.ClearAllTiles();
        paredesMap.ClearAllTiles();

        System.Random rnd = new System.Random(42);

        // A. Suelo (x: 0 a 55, y: 2 a 17)
        for (int x = 0; x < 56; x++)
        {
            for (int y = 2; y <= 17; y++)
            {
                // Zona 1 (Entrada x: 0..8) -> Tierra
                // Zona 2 (Calle de puestos x: 8..40) -> Adoquín
                // Zona 3 (Rincón del perro x: 40..48) -> Tierra
                // Zona 4 (Salida x: 48..55) -> Adoquín sin cortes
                bool isDirt = (x <= 8) || (x >= 40 && x < 48);

                TileBase t;
                if (isDirt)
                {
                    t = dirtTiles.Length > 0 ? dirtTiles[rnd.Next(dirtTiles.Length)] : null;
                }
                else
                {
                    t = floorTiles.Length > 0 ? floorTiles[rnd.Next(floorTiles.Length)] : null;
                }

                if (t != null)
                    sueloMap.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // B. Fachadas Superior (y = 18 y 19) e Inferior (y = 0 y 1)
        for (int x = -1; x <= 56; x++)
        {
            if (wallFacade.Length > 0)
            {
                int idx = Math.Abs(x) % wallFacade.Length;
                paredesMap.SetTile(new Vector3Int(x, 19, 0), wallFacade[idx]);
                paredesMap.SetTile(new Vector3Int(x, 18, 0), wallFacade[(idx + 1) % wallFacade.Length]);
                paredesMap.SetTile(new Vector3Int(x,  1, 0), wallFacade[idx]);
                paredesMap.SetTile(new Vector3Int(x,  0, 0), wallFacade[(idx + 1) % wallFacade.Length]);
            }
        }

        // C. Muros Laterales de Concreto (x = -1 y x = 56, para y = 0 a 19)
        for (int y = 0; y <= 19; y++)
        {
            if (wallConcrete.Length > 0)
            {
                int idx = y % wallConcrete.Length;
                paredesMap.SetTile(new Vector3Int(-1, y, 0), wallConcrete[idx]);
                paredesMap.SetTile(new Vector3Int(56, y, 0), wallConcrete[idx]);
            }
        }

        ConfigureCompositeCollider(paredesMap.gameObject);

        // ════════════════════════════════════════════════════════════════════════
        // 2. ELEMENTOS DEL ESCENARIO (Props, Puestos, Escondites, Obstáculos)
        // ════════════════════════════════════════════════════════════════════════
        Transform propsRoot   = FreshRoot("Props");
        Transform decalsRoot  = FreshRoot("Decals");
        Transform decorRoot   = FreshRoot("Decoracion");
        Transform limitsRoot  = FreshRoot("LimitesInvisibles");

        // Cargar sprites individuales de los spritesheets
        Sprite puestoFrutas   = GetSpriteByName(PuestosPath, "Puesto_frutas", 0);
        Sprite puestoVerduras = GetSpriteByName(PuestosPath, "Puesto_verduras", 1);

        Sprite huacales       = GetSpriteByName(SacosPath, "Huacales_apilados", 0);
        Sprite huacalRoto     = GetSpriteByName(SacosPath, "Huacal_roto_paja", 1);
        Sprite canastaVacia   = GetSpriteByName(SacosPath, "Canasta_vacia", 2);
        Sprite canastaChiles  = GetSpriteByName(SacosPath, "Canasta_chiles", 3);
        Sprite costales       = GetSpriteByName(SacosPath, "Costales_apilados", 4);
        Sprite costalMaiz     = GetSpriteByName(SacosPath, "Costal_maiz_abierto", 5);

        Sprite escCajas       = GetSpriteByName(EsconditesPath, "Escondite_cajas", 0) ?? huacales;
        Sprite escBarril      = GetSpriteByName(EsconditesPath, "Escondite_barril", 1) ?? canastaVacia;

        Sprite farolSprite    = GetSpriteByName(DecoracionPath, "Farol", 0);
        Sprite boteSprite     = GetSpriteByName(DecoracionPath, "Bote", 1);
        Sprite cartelSprite   = GetSpriteByName(DecoracionPath, "Cartel_feria", 4);
        Sprite papelSprite    = GetSpriteByName(DecoracionPath, "Papel_picado", 5);

        Sprite salida0        = GetSpriteByName(SalidasPath, "Salida_0", 0);

        // ── A. PUESTOS DE MERCADO (7 en total, 3 y 4 por hilera) ──────────────
        // Hilera Superior (3 puestos, base y ≈ 12, dejando libre el corredor x=33)
        CreatePuesto(propsRoot, "Puesto_Sup_1", puestoFrutas,   13.0f, 12.0f, wallsLayer);
        CreatePuesto(propsRoot, "Puesto_Sup_2", puestoVerduras, 22.0f, 12.0f, wallsLayer);
        CreatePuesto(propsRoot, "Puesto_Sup_3", puestoFrutas,   29.5f, 12.0f, wallsLayer);

        // Hilera Inferior (4 puestos, base y ≈ 4, dejando libre el corredor x=33)
        CreatePuesto(propsRoot, "Puesto_Inf_1", puestoVerduras, 11.0f, 4.0f, wallsLayer);
        CreatePuesto(propsRoot, "Puesto_Inf_2", puestoFrutas,   19.0f, 4.0f, wallsLayer);
        CreatePuesto(propsRoot, "Puesto_Inf_3", puestoVerduras, 27.0f, 4.0f, wallsLayer);
        CreatePuesto(propsRoot, "Puesto_Inf_4", puestoFrutas,   36.5f, 4.0f, wallsLayer);

        // ── B. LABERINTO DE HUACALES Y COSTALES (Pasillos >= 2.5 u, corredor vertical x=33 libre)
        CreateSolidStack(propsRoot, "Barricada_1", huacales, costales, 16.0f, 8.0f, wallsLayer);
        CreateSolidStack(propsRoot, "Barricada_2", costales, canastaChiles, 24.5f, 7.8f, wallsLayer);
        CreateSolidStack(propsRoot, "Barricada_3", huacales, costalMaiz, 26.5f, 7.8f, wallsLayer);
        CreateSolidStack(propsRoot, "Cajas_Paso_1", huacalRoto, canastaVacia, 7.5f, 5.0f, wallsLayer);
        CreateSolidStack(propsRoot, "Cajas_Paso_2", costales, huacales, 39.5f, 10.5f, wallsLayer);

        // ── C. ESCONDITES FUNCIONALES (5 en total, clústeres) ──────────────────
        // 1. Cerca de la entrada (9.5, 14.5)
        CreateClusterHideSpot(propsRoot, "Escondite_Entrada_Cajas", escCajas, huacales, canastaVacia,
            9.5f, 14.5f, hideSpotLayer, wallsLayer);

        // 2. Clúster de huacales y costales (17.5, 8.5)
        CreateClusterHideSpot(propsRoot, "Escondite_Huacales_Costales", huacales, costales, canastaChiles,
            17.5f, 8.5f, hideSpotLayer, wallsLayer);

        // 3. Escondite barril (26.5, 12.8)
        CreateClusterHideSpot(propsRoot, "Escondite_Barril_Mercado", escBarril, huacalRoto, costalMaiz,
            26.5f, 12.8f, hideSpotLayer, wallsLayer);

        // 4. Clúster de costales (36.5, 9.0)
        CreateClusterHideSpot(propsRoot, "Escondite_Costales_Cluster", costales, costalMaiz, huacales,
            36.5f, 9.0f, hideSpotLayer, wallsLayer);

        // 5. Junto a la zona del perro (42.0, 6.0)
        CreateClusterHideSpot(propsRoot, "Escondite_ZonaPerro_Cajas", escCajas, canastaChiles, huacalRoto,
            42.0f, 6.0f, hideSpotLayer, wallsLayer);

        // ── D. CHARCOS OFICIALES (6 en total: Charco_0 a Charco_5) ─────────────
        CreateMudPuddle(decalsRoot, "Charco_0_Entrada",
            GetSpriteByName(BotellasPath, "Charco_0", 0), 5.0f, 6.0f, 1.1f);

        CreateMudPuddle(decalsRoot, "Charco_1_Pasillo",
            GetSpriteByName(BotellasPath, "Charco_1", 1), 15.0f, 7.8f, 1.2f);

        CreateMudPuddle(decalsRoot, "Charco_2_JuntoPuesto",
            GetSpriteByName(BotellasPath, "Charco_2", 2), 23.0f, 5.0f, 1.0f);

        CreateMudPuddle(decalsRoot, "Charco_3_Pasillo",
            GetSpriteByName(BotellasPath, "Charco_3", 3), 29.0f, 8.2f, 1.2f);

        CreateMudPuddle(decalsRoot, "Charco_4_Perro",
            GetSpriteByName(BotellasPath, "Charco_4", 4), 42.5f, 4.0f, 1.3f);

        CreateMudPuddle(decalsRoot, "Charco_5_Perro",
            GetSpriteByName(BotellasPath, "Charco_5", 5), 46.0f, 5.5f, 1.3f);

        // ── E. BOTELLAS DE VIDRIO REACTIVAS (6 montones con Vidrios_rotos) ──────
        Sprite vidriosRotos = GetSpriteByName(BotellasPath, "Vidrios_rotos", 5);

        // Ubicadas estratégicamente junto a huacales y puestos (evitando invadir el centro del pasillo)
        CreateNoiseObstacle(decalsRoot, "Botellas_1",
            GetSpriteByName(BotellasPath, "Botella_verde", 2), vidriosRotos, 19.5f, 8.0f);

        CreateNoiseObstacle(decalsRoot, "Botellas_2",
            GetSpriteByName(BotellasPath, "Botella_azul", 7), vidriosRotos, 28.0f, 11.2f);

        CreateNoiseObstacle(decalsRoot, "Botellas_3",
            GetSpriteByName(BotellasPath, "Botella_ambar_etiqueta_acostada", 4), vidriosRotos, 30.5f, 15.0f);

        CreateNoiseObstacle(decalsRoot, "Botellas_4",
            GetSpriteByName(BotellasPath, "Botella_transparente_acostada", 3), vidriosRotos, 38.0f, 6.0f);

        CreateNoiseObstacle(decalsRoot, "Botellas_5",
            GetSpriteByName(BotellasPath, "Botella_rota", 6), vidriosRotos, 43.0f, 5.0f);

        CreateNoiseObstacle(decalsRoot, "Botellas_6",
            GetSpriteByName(BotellasPath, "Botellas_caidas", 8), vidriosRotos, 47.0f, 4.5f);

        // ── F. PERRO CALLEJERO (Zona 3: Rincón del perro en 45, 3.8) ───────────
        // Cama de paja en el suelo que enmarca visualmente el rincón del perro
        GameObject dogBed = new GameObject("Cama_Perro");
        dogBed.transform.SetParent(propsRoot, false);
        dogBed.transform.position = new Vector3(45.0f, 3.6f, 0f);
        SpriteRenderer bedSr = dogBed.AddComponent<SpriteRenderer>();
        bedSr.sprite = huacalRoto;
        bedSr.sortingOrder = 2; // Detrás del perro
        bedSr.color = new Color(0.95f, 0.88f, 0.75f, 1f);

        Sprite perroLatDer = GetSpriteByName(PerroPath, "Perro_lado_der", 3);
        Sprite perroFrente = GetSpriteByName(PerroPath, "Perro_frente", 0);
        Sprite perroEspalda = GetSpriteByName(PerroPath, "Perro_espalda", 1);
        CreateStrayDog(propsRoot, 45.0f, 3.8f, perroLatDer, perroFrente, perroEspalda);

        // ── G. SALIDA MONUMENTAL (Zona 4: Salida_0 integrada al final de la calle) ──────
        CreateExitScene(propsRoot, 52.0f, 7.2f, salida0, wallsLayer);

        // ── H. DECORACIÓN: FAROLES, PAPEL PICADO Y BOTES (CERO BANCAS) ──────────
        // 7 Faroles con Point Light 2D cálida (incluyendo farol que baña el rincón del perro)
        CreateFarol(decorRoot, "Farol_1", farolSprite,  2.0f, 15.0f, wallsLayer);
        CreateFarol(decorRoot, "Farol_2", farolSprite,  2.0f,  5.0f, wallsLayer);
        CreateFarol(decorRoot, "Farol_3", farolSprite, 16.0f, 17.0f, wallsLayer);
        CreateFarol(decorRoot, "Farol_4", farolSprite, 30.0f, 17.0f, wallsLayer);
        CreateFarol(decorRoot, "Farol_5", farolSprite, 38.0f, 17.0f, wallsLayer);
        CreateFarol(decorRoot, "Farol_6", farolSprite, 44.0f, 15.0f, wallsLayer);
        CreateFarol(decorRoot, "Farol_Perro", farolSprite, 42.0f, 4.0f, wallsLayer);

        // Papel picado tradicional colgando en Foreground (Order 10)
        CreateForegroundProp(decorRoot, "Papel_1", papelSprite,  6.0f, 17.0f, 1.3f);
        CreateForegroundProp(decorRoot, "Papel_2", papelSprite, 17.0f, 16.0f, 1.3f);
        CreateForegroundProp(decorRoot, "Papel_3", papelSprite, 26.0f, 16.0f, 1.3f);
        CreateForegroundProp(decorRoot, "Papel_4", papelSprite, 33.0f, 16.0f, 1.3f);

        // Cartel feria en la entrada y botes de basura sólidos
        CreateDecoProp(decorRoot, "Cartel_Feria", cartelSprite, 4.0f, 14.0f, 1.0f, wallsLayer);
        CreateBote(decorRoot, "Bote_1", boteSprite, 8.0f, 3.0f, wallsLayer);
        CreateBote(decorRoot, "Bote_2", boteSprite, 39.0f, 15.0f, wallsLayer);

        // Límites invisibles
        CreateInvisibleWall(limitsRoot, "Limite_Oeste", -1.5f, 10.0f, wallsLayer);
        CreateInvisibleWall(limitsRoot, "Limite_Este",  56.5f, 10.0f, wallsLayer);

        // ════════════════════════════════════════════════════════════════════════
        // 3. ILUMINACIÓN GLOBAL URP 2D (0.55 Tenue Cálido)
        // ════════════════════════════════════════════════════════════════════════
        EnsureGlobalLight(0.55f);

        // ════════════════════════════════════════════════════════════════════════
        // 4. WAYPOINTS Y PATRULLA DE LA LLORONA (x ≈ 33, vertical entre y=4 y y=16)
        // ════════════════════════════════════════════════════════════════════════
        Transform wpRoot = FreshRoot("Waypoints");
        Transform wpA = CreateEmpty(wpRoot, "WP_Llorona_A", 33.0f, 15.5f);
        Transform wpB = CreateEmpty(wpRoot, "WP_Llorona_B", 33.0f,  4.5f);

        SetupLlorona(new Vector2(33.0f, 10.0f), wpA, wpB, wallsLayer, lloronaLayer);

        // ════════════════════════════════════════════════════════════════════════
        // 5. CARLOS (PLAYER) Y CÁMARA CONFINER (Bounds 56 × 20)
        // ════════════════════════════════════════════════════════════════════════
        SetupPlayer(new Vector2(3.0f, 10.0f), playerLayer);
        SetupCamera(new Vector2(3.0f, 10.0f));

        SetupManagers();

        // ════════════════════════════════════════════════════════════════════════
        // 6. GENERAR Y CONECTAR HUD OFICIAL (FASE 2)
        // ════════════════════════════════════════════════════════════════════════
        UIBuilder.BuildLevel1UICanvas();

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        Debug.Log("[Level1Builder] ¡Nivel 1 (El Mercado Vacío) reconstruido con éxito bajo PROMPT_VISUAL.md!");
    }

    // ── Métodos de Ayuda de Construcción ─────────────────────────────────────

    static void CreatePuesto(Transform parent, string name, Sprite sprite, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        // Colisión sólida sobre el mostrador/mesa (y in [-0.5, 1.3])
        // Bloquea totalmente el paso físico a través de la mercancía y la estructura
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size   = new Vector2(4.4f, 1.8f);
        col.offset = new Vector2(0f, 0.45f);

        if (layer >= 0) go.layer = layer;
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateClusterHideSpot(Transform parent, string name, Sprite mainSprite, Sprite decorA, Sprite decorB,
        float x, float y, int hideSpotLayer, int wallsLayer)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(x, y, 0f);

        // Pieza principal
        GameObject mainGo = new GameObject("Cuerpo_Principal");
        mainGo.transform.SetParent(root.transform, false);
        SpriteRenderer srMain = mainGo.AddComponent<SpriteRenderer>();
        srMain.sprite = mainSprite;
        srMain.sortingOrder = 3;

        // Decoración lateral A
        if (decorA != null)
        {
            GameObject decAGo = new GameObject("Deco_Izq");
            decAGo.transform.SetParent(root.transform, false);
            decAGo.transform.localPosition = new Vector3(-0.9f, -0.2f, 0f);
            SpriteRenderer srA = decAGo.AddComponent<SpriteRenderer>();
            srA.sprite = decorA;
            srA.sortingOrder = 3;
        }

        // Decoración lateral B
        if (decorB != null)
        {
            GameObject decBGo = new GameObject("Deco_Der");
            decBGo.transform.SetParent(root.transform, false);
            decBGo.transform.localPosition = new Vector3(0.9f, -0.2f, 0f);
            SpriteRenderer srB = decBGo.AddComponent<SpriteRenderer>();
            srB.sprite = decorB;
            srB.sortingOrder = 3;
        }

        // Base sólida que bloquea el paso físico (NO se puede atravesar)
        BoxCollider2D solidCol = root.AddComponent<BoxCollider2D>();
        solidCol.size   = new Vector2(2.8f, 1.7f);
        solidCol.offset = new Vector2(0f, 0.45f);
        if (wallsLayer >= 0) root.layer = wallsLayer;

        // Trigger frontal que sobresale hacia abajo para activación de [E]
        GameObject trigGo = new GameObject("TriggerOcultarse");
        trigGo.transform.SetParent(root.transform, false);
        BoxCollider2D trigCol = trigGo.AddComponent<BoxCollider2D>();
        trigCol.isTrigger = true;
        trigCol.size   = new Vector2(3.2f, 1.4f);
        trigCol.offset = new Vector2(0f, -0.6f);
        if (hideSpotLayer >= 0) trigGo.layer = hideSpotLayer;

        // Punto detrás del clúster donde se oculta Carlos
        GameObject hidePt = new GameObject("HidePoint");
        hidePt.transform.SetParent(root.transform, false);
        hidePt.transform.localPosition = new Vector3(0f, 0.35f, 0f);

        HideSpot hs = root.AddComponent<HideSpot>();
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("hidePoint").objectReferenceValue = hidePt.transform;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(root, name);
    }

    static void CreateSolidStack(Transform parent, string name, Sprite main, Sprite sec, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = main;
        sr.sortingOrder = 3;

        if (sec != null)
        {
            GameObject secGo = new GameObject("Secundario");
            secGo.transform.SetParent(go.transform, false);
            secGo.transform.localPosition = new Vector3(0.6f, -0.2f, 0f);
            SpriteRenderer srSec = secGo.AddComponent<SpriteRenderer>();
            srSec.sprite = sec;
            srSec.sortingOrder = 3;
        }

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size   = new Vector2(sec != null ? 2.6f : 1.8f, 1.7f);
        col.offset = new Vector2(sec != null ? 0.25f : 0f, 0.45f);
        if (layer >= 0) go.layer = layer;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateMudPuddle(Transform parent, string name, Sprite sprite, float x, float y, float scale = 1.0f)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1; // Decals
        sr.color = new Color(0.85f, 0.78f, 0.70f, 0.95f); // Tinte terroso que distingue el lodo del empedrado

        // Colisionador elíptico compacto ajustado exactamente a la mancha de lodo interior
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(1.1f, 0.40f);
        bc.offset = new Vector2(0f, -0.05f);

        go.AddComponent<MudPuddle>();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateNoiseObstacle(Transform parent, string name, Sprite sprite, Sprite brokenSprite, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2; // Decal superior
        sr.color = new Color(1.15f, 1.25f, 1.15f, 1.0f); // Brillo verde cristalino para destacar sobre el suelo oscuro

        // Colisionador ceñido milimétricamente al montoncito de botellas
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(0.40f, 0.25f);

        NoiseObstacle no = go.AddComponent<NoiseObstacle>();
        SerializedObject so = new SerializedObject(no);
        so.FindProperty("brokenSprite").objectReferenceValue = brokenSprite;
        AudioClip sfxVidrio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/vidrio.mp3");
        if (sfxVidrio != null) so.FindProperty("glassBreakSound").objectReferenceValue = sfxVidrio;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateStrayDog(Transform parent, float x, float y, Sprite lateral, Sprite frente, Sprite espalda)
    {
        GameObject go = new GameObject("Perro_Callejero_Mercado");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = lateral;
        sr.sortingOrder = 4;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 1.2f;

        StrayDogAI dogAI = go.AddComponent<StrayDogAI>();
        SerializedObject so = new SerializedObject(dogAI);

        var latProp = so.FindProperty("lateralView");
        if (latProp != null) latProp.objectReferenceValue = lateral;

        var frontProp = so.FindProperty("frontView");
        if (frontProp != null) frontProp.objectReferenceValue = frente;

        var backProp = so.FindProperty("backView");
        if (backProp != null) backProp.objectReferenceValue = espalda;

        var runProp = so.FindProperty("runWakeRadius");
        if (runProp != null) runProp.floatValue = 4.0f;

        var walkProp = so.FindProperty("walkWakeRadius");
        if (walkProp != null) walkProp.floatValue = 2.5f;

        AudioClip sfxPerros = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/perro.mp3");
        var barkProp = so.FindProperty("barkSfx");
        if (barkProp != null) barkProp.objectReferenceValue = sfxPerros;

        so.ApplyModifiedProperties();

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;

        // Luz suave localizada que permite ver al perro durmiendo en la penumbra
        GameObject dogLightGo = new GameObject("LuzPerro");
        dogLightGo.transform.SetParent(go.transform, false);
        dogLightGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        Light2D dogLight = dogLightGo.AddComponent<Light2D>();
        dogLight.lightType = Light2D.LightType.Point;
        dogLight.color = new Color(1.0f, 0.85f, 0.55f);
        dogLight.intensity = 0.85f;
        dogLight.pointLightInnerRadius = 0.4f;
        dogLight.pointLightOuterRadius = 2.4f;

        Undo.RegisterCreatedObjectUndo(go, "Perro_Callejero_Mercado");
    }

    static void CreateExitScene(Transform parent, float x, float y, Sprite exitSprite, int wallsLayer)
    {
        GameObject go = new GameObject("Salida_Mercado_Meta");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = exitSprite ?? GetSpriteByName(SalidasPath, "Salida_0", 0);
        sr.sortingOrder = 3;

        // Muros y pilares laterales sólidos (ajustados al pivote Bottom-Center)
        BoxCollider2D leftCol = go.AddComponent<BoxCollider2D>();
        leftCol.size   = new Vector2(2.5f, 5.0f);
        leftCol.offset = new Vector2(-2.6f, 2.5f);

        BoxCollider2D rightCol = go.AddComponent<BoxCollider2D>();
        rightCol.size   = new Vector2(2.5f, 5.0f);
        rightCol.offset = new Vector2(2.6f, 2.5f);

        if (wallsLayer >= 0) go.layer = wallsLayer;

        // Hueco central como trigger que dispara LevelGoal
        GameObject goalGo = new GameObject("MetaTrigger");
        goalGo.transform.SetParent(go.transform, false);
        goalGo.transform.localPosition = new Vector3(0f, 2.0f, 0f);

        BoxCollider2D centerTrigger = goalGo.AddComponent<BoxCollider2D>();
        centerTrigger.isTrigger = true;
        centerTrigger.size = new Vector2(2.2f, 4.0f);
        goalGo.AddComponent<LevelGoal>();

        // Point Light 2D cálida interior brillante con pulso suave (sin flechas)
        GameObject lightGo = new GameObject("LuzSalida");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.88f, 0.55f);
        l2d.intensity = 2.4f;
        l2d.pointLightInnerRadius = 1.0f;
        l2d.pointLightOuterRadius = 6.5f;
        lightGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, "Salida_Mercado_Meta");
    }

    static void CreateFarol(Transform parent, string name, Sprite sprite, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;

        // Colisión sólida vertical ajustada a las proporciones reales del poste
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 2.2f);
        col.offset = new Vector2(0f, -0.4f);
        if (layer >= 0) go.layer = layer;

        GameObject lightGo = new GameObject("Luz");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.4f, 0f);

        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.84f, 0.48f);
        l2d.intensity = 1.6f;
        l2d.pointLightInnerRadius = 0.8f;
        l2d.pointLightOuterRadius = 5.2f;
        lightGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateBote(Transform parent, string name, Sprite sprite, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.2f, 1.8f);
        col.offset = new Vector2(0f, 0f);
        if (layer >= 0) go.layer = layer;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateForegroundProp(Transform parent, string name, Sprite sprite, float x, float y, float scale)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 10; // Foreground
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateDecoProp(Transform parent, string name, Sprite sprite, float x, float y, float scale, int layer = -1)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2.2f, 1.8f);
        col.offset = new Vector2(0f, 0f);
        if (layer >= 0) go.layer = layer;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateInvisibleWall(Transform parent, string name, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 20f);

        if (layer >= 0) go.layer = layer;
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void EnsureGlobalLight(float intensity = 0.50f)
    {
        var allLights = UnityEngine.Object.FindObjectsByType<Light2D>(FindObjectsInactive.Include);
        Light2D mainGlobal = null;
        foreach (var l in allLights)
        {
            if (l == null) continue;
            if (l.lightType == Light2D.LightType.Global || l.gameObject.name.ToLower().Contains("global"))
            {
                if (mainGlobal == null)
                {
                    mainGlobal = l;
                }
                else
                {
                    Undo.DestroyObjectImmediate(l.gameObject);
                }
            }
        }

        if (mainGlobal == null)
        {
            GameObject glGo = new GameObject("Global Light 2D");
            mainGlobal = glGo.AddComponent<Light2D>();
            Undo.RegisterCreatedObjectUndo(glGo, "Global Light 2D");
        }

        mainGlobal.gameObject.name = "Global Light 2D";
        mainGlobal.lightType = Light2D.LightType.Global;
        // Tinte azul pizarra nocturno tenue: separa el fondo ambiental frío de las luces cálidas de Carlos y faroles
        mainGlobal.color = new Color(0.38f, 0.42f, 0.58f, 1.0f);
        mainGlobal.intensity = intensity;
    }

    static Transform CreateEmpty(Transform parent, string name, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        Undo.RegisterCreatedObjectUndo(go, name);
        return go.transform;
    }

    static void SetupPlayer(Vector2 pos, int playerLayer)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Undo.RecordObject(player.transform, "Posicionar Player");
            player.transform.position = new Vector3(pos.x, pos.y, 0f);
            player.transform.rotation = Quaternion.identity;
            if (playerLayer >= 0) player.layer = playerLayer;

            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 10;
                // Leve realce luminoso para despegar a Carlos del empedrado oscuro
                sr.color = new Color(1.08f, 1.04f, 0.98f, 1.0f);
            }

            // Asegurar Animator y PlayerController
            Animator anim = player.GetComponent<Animator>();
            if (anim == null) anim = player.AddComponent<Animator>();
            RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Player/PlayerController.controller");
            if (ctrl != null)
            {
                anim.runtimeAnimatorController = ctrl;
                anim.enabled = true;
            }

            if (player.GetComponent<PlayerMovement>() == null)
                player.AddComponent<PlayerMovement>();

            if (player.GetComponent<PlayerAnimation>() == null)
                player.AddComponent<PlayerAnimation>();

            PlayerAnimation pa = player.GetComponent<PlayerAnimation>();
            if (pa != null)
            {
                AudioClip sfxPasosLentos = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/pasos_lentos.wav");
                AudioClip sfxPasosRapidos = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/pasos_rapidos.wav");
                SerializedObject soPa = new SerializedObject(pa);
                soPa.FindProperty("pasosLentos").objectReferenceValue = sfxPasosLentos;
                soPa.FindProperty("pasosRapidos").objectReferenceValue = sfxPasosRapidos;
                soPa.ApplyModifiedProperties();
                EditorUtility.SetDirty(pa);
            }

            // Luz 2D puntual suave y cálida que sigue al jugador (halo tenue de linterna/vela)
            Transform existingLight = player.transform.Find("LuzCarlos");
            GameObject pLightGo = existingLight != null ? existingLight.gameObject : new GameObject("LuzCarlos");
            pLightGo.transform.SetParent(player.transform, false);
            pLightGo.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            Light2D pLight = pLightGo.GetComponent<Light2D>() ?? pLightGo.AddComponent<Light2D>();
            pLight.lightType = Light2D.LightType.Point;
            pLight.color = new Color(1.0f, 0.88f, 0.62f, 1.0f);
            pLight.intensity = 1.25f;
            pLight.pointLightInnerRadius = 0.8f;
            pLight.pointLightOuterRadius = 3.8f;

            if (pLightGo.GetComponent<LightFlicker>() == null)
                pLightGo.AddComponent<LightFlicker>();

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.freezeRotation = true;
                rb.gravityScale = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            BoxCollider2D col = player.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = new Vector2(0.6f, 0.8f);
                col.offset = new Vector2(0f, -0.3f);
            }
        }
    }

    static void SetupLlorona(Vector2 pos, Transform wpA, Transform wpB, int wallsLayer, int lloronaLayer)
    {
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona == null) return;

        Undo.RecordObject(llorona.transform, "Posicionar Llorona");
        llorona.transform.position = new Vector3(pos.x, pos.y, 0f);
        llorona.transform.rotation = Quaternion.identity;
        if (lloronaLayer >= 0) llorona.gameObject.layer = lloronaLayer;

        Rigidbody2D rb = llorona.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        Collider2D col = llorona.GetComponent<Collider2D>();
        if (col != null)
        {
            if (col is CapsuleCollider2D cap)
            {
                cap.size = new Vector2(0.8f, 1.2f);
                cap.offset = new Vector2(0f, -0.3f);
            }
            else if (col is BoxCollider2D box)
            {
                box.size = new Vector2(0.8f, 1.2f);
                box.offset = new Vector2(0f, -0.3f);
            }
        }

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
            maskProp.intValue = 1 << wallsLayer;

        LloronaSettings s1 = AssetDatabase.LoadAssetAtPath<LloronaSettings>("Assets/Settings/LloronaSettings_Level1.asset");
        if (s1 != null)
            so.FindProperty("settings").objectReferenceValue = s1;

        AudioClip sfxLlorona = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/llorona_nivel1.mpeg.wav");
        var screamProp = so.FindProperty("screamSfx");
        if (screamProp != null) screamProp.objectReferenceValue = sfxLlorona;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(llorona);
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

        CameraFollow2D follow = cam.GetComponent<CameraFollow2D>() ?? cam.gameObject.AddComponent<CameraFollow2D>();
        // Confinar a los límites 56 × 20 permitiendo visibilidad completa de la salida
        follow.SetBounds(new Vector2(8.8f, 5.0f), new Vector2(48.5f, 15.0f));
    }

    static void SetupManagers()
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
        gm.SetLevelNumber(1);
        EditorUtility.SetDirty(gm);

        // Configuración centralizada de AudioManager para Nivel 1
        AudioManager am = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
        if (am == null)
        {
            GameObject oldAm = GameObject.Find("AudioManager");
            if (oldAm != null) Undo.DestroyObjectImmediate(oldAm);

            GameObject amGo = new GameObject("AudioManager");
            am = amGo.AddComponent<AudioManager>();
            Undo.RegisterCreatedObjectUndo(amGo, "AudioManager");
        }

        AudioClip n1Music = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Nivel1/Breath_Behind_the_Stall.mp3");
        AudioClip sfxPasosLentos = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/pasos_lentos.wav");
        AudioClip sfxPasosRapidos = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/pasos_rapidos.wav");
        AudioClip sfxPerros = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/perro.mp3");
        AudioClip sfxLlorona = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/llorona_nivel1.mpeg.wav");
        AudioClip sfxVidrio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/vidrio.mp3");

        SerializedObject soAm = new SerializedObject(am);
        soAm.FindProperty("musicaNivel").objectReferenceValue = n1Music;
        soAm.FindProperty("musicaNivel1").objectReferenceValue = n1Music;
        soAm.FindProperty("pasosLentos").objectReferenceValue = sfxPasosLentos;
        soAm.FindProperty("pasosRapidos").objectReferenceValue = sfxPasosRapidos;
        soAm.FindProperty("sonidoPerro").objectReferenceValue = sfxPerros;
        soAm.FindProperty("lloronaGrito").objectReferenceValue = sfxLlorona;
        soAm.FindProperty("sonidoVidrio").objectReferenceValue = sfxVidrio;
        soAm.ApplyModifiedProperties();
        EditorUtility.SetDirty(am);
    }

    [MenuItem("La Llorona/Configurar Audios Nivel 1")]
    public static void ConfigureLevel1AudioMenu()
    {
        AudioClip n1Music = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Nivel1/Breath_Behind_the_Stall.mp3");
        AudioClip sfxPasosLentos = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/pasos_lentos.wav");
        AudioClip sfxPasosRapidos = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/pasos_rapidos.wav");
        AudioClip sfxPerros = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/perro.mp3");
        AudioClip sfxLlorona = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/llorona_nivel1.mpeg.wav");
        AudioClip sfxVidrio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/vidrio.mp3");

        // 1. AudioManager
        AudioManager am = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
        if (am == null)
        {
            GameObject amGo = new GameObject("AudioManager");
            am = amGo.AddComponent<AudioManager>();
            Undo.RegisterCreatedObjectUndo(amGo, "AudioManager");
        }
        SerializedObject soAm = new SerializedObject(am);
        soAm.FindProperty("musicaNivel").objectReferenceValue = n1Music;
        soAm.FindProperty("musicaNivel1").objectReferenceValue = n1Music;
        soAm.FindProperty("pasosLentos").objectReferenceValue = sfxPasosLentos;
        soAm.FindProperty("pasosRapidos").objectReferenceValue = sfxPasosRapidos;
        soAm.FindProperty("sonidoPerro").objectReferenceValue = sfxPerros;
        soAm.FindProperty("lloronaGrito").objectReferenceValue = sfxLlorona;
        soAm.FindProperty("sonidoVidrio").objectReferenceValue = sfxVidrio;
        soAm.ApplyModifiedProperties();
        EditorUtility.SetDirty(am);

        // 2. Player Footsteps
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerAnimation pa = player.GetComponent<PlayerAnimation>() ?? player.AddComponent<PlayerAnimation>();
            SerializedObject soPa = new SerializedObject(pa);
            soPa.FindProperty("pasosLentos").objectReferenceValue = sfxPasosLentos;
            soPa.FindProperty("pasosRapidos").objectReferenceValue = sfxPasosRapidos;
            soPa.ApplyModifiedProperties();
            EditorUtility.SetDirty(pa);
        }

        // 3. Perro Callejero (sonido de perro.mp3)
        StrayDogAI dog = UnityEngine.Object.FindAnyObjectByType<StrayDogAI>();
        if (dog != null)
        {
            SerializedObject soDog = new SerializedObject(dog);
            soDog.FindProperty("barkSfx").objectReferenceValue = sfxPerros;
            soDog.ApplyModifiedProperties();
            EditorUtility.SetDirty(dog);
        }

        // 4. Llorona (llorona_nivel1.mpeg.wav)
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona != null)
        {
            SerializedObject soLlorona = new SerializedObject(llorona);
            soLlorona.FindProperty("screamSfx").objectReferenceValue = sfxLlorona;
            soLlorona.ApplyModifiedProperties();
            EditorUtility.SetDirty(llorona);
        }

        // 5. Botellas (sonido de vidrio.mp3)
        NoiseObstacle[] botellas = UnityEngine.Object.FindObjectsByType<NoiseObstacle>(FindObjectsInactive.Include);
        foreach (var b in botellas)
        {
            if (b != null)
            {
                SerializedObject soB = new SerializedObject(b);
                soB.FindProperty("glassBreakSound").objectReferenceValue = sfxVidrio;
                soB.ApplyModifiedProperties();
                EditorUtility.SetDirty(b);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("Audios de Nivel 1 configurados exitosamente en la escena activa.");
    }

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
        GameObject go = (child != null) ? child.gameObject : new GameObject(name);
        if (child == null)
        {
            go.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(go, name);
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
}
