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
/// Constructor oficial y completo del Nivel 2: El Callejón del Beso (recorrido fluido en zigzag de ~60 u).
/// Cumple estrictamente con la especificación visual y jugabilidad de PROMPT_VISUAL.md:
/// - Callejón continuo en zigzag con dos giros a 90°:
///     * Tramo 1 (Horizontal oeste): x in [0, 20], y in [2, 7].
///     * Tramo 2 (Vertical norte): x in [15, 20], y in [7, 21] (Balcones del Beso enfrentados).
///     * Tramo 3 (Horizontal este hacia salida): x in [20, 41], y in [16, 21].
/// - Envolvente total sin huecos (x in [-6, 48], y in [-4, 27]): toda celda fuera de la calle
///   está sellada con suelo/muros coloniales sólidos sin texturas rotas.
/// - 8 Límites perimetrales físicos (BoxCollider2D) que impiden a Carlos y La Llorona salirse del callejón.
/// - Fachadas continuas coloniales (SpriteRenderer) alineadas en ambos lados de todos los tramos.
/// - 7 escondites funcionales y visibles para Carlos (3 portones coloniales Puerta_0..2,
///   barriles coloniales y clústeres de huacales/sacos) con aviso [E].
/// - Carlos con Animator completo y PlayerController.controller conectado (animaciones en las 4 direcciones).
/// - Decoración densa: balcones enfrentados de hierro forjado, macetas colgantes y en acera,
///   faroles de pared con LightFlicker, botes y papel picado festivo.
/// - Obstáculos: 4 charcos de lodo y 4 pilas de botellas reflectantes calibradas a los pies.
/// - Perro callejero durmiendo en su lecho de paja con farol.
/// - Salida monumental Salida_1 conectada sin cortes, con luz cálida interior.
/// - Iluminación URP 2D a 0.46 con halo personal de Carlos (LuzCarlos).
/// </summary>
public static class Level2Builder
{
    const string ScenePath = "Assets/Scenes/Level2_Callejon.unity";

    // Rutas de Spritesheets oficiales
    const string TilesMuroPath     = "Assets/Sprites/Environment/tiles_nvl2.png";
    const string TilesAdoquinPath  = "Assets/Sprites/Environment/tiles_adoquin_nvl2.png";
    const string PuertasPath       = "Assets/Sprites/Environment/Puertas_nvl2.png";
    const string AccesoriosPath    = "Assets/Sprites/Environment/accesorios_nvl2.png";
    const string SalidasPath       = "Assets/Sprites/Environment/Salidas.png";
    const string BotellasPath      = "Assets/Sprites/Environment/Botellas_charcolodo_Nvl1.png";
    const string EsconditesPath    = "Assets/Sprites/Environment/escondites.png";
    const string SacosPath         = "Assets/Sprites/Environment/Contenedores_Sacos_Nvl1.png";
    const string PerroPath         = "Assets/Sprites/Perro/perro.png";
    const string DecoracionPath    = "Assets/Sprites/Environment/decoracion.png";

    static Sprite GetSprite(string path, string name, int fallback = 0)
    {
        if (!File.Exists(path)) return null;
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        var match = sprites.FirstOrDefault(s => s.name == name);
        if (match != null) return match;
        if (fallback >= 0 && fallback < sprites.Length) return sprites[fallback];
        return sprites.FirstOrDefault();
    }

    [MenuItem("La Llorona/Construir Nivel 2 Completo")]
    public static void BuildLevel2Scene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[Level2Builder] Deteniendo Modo Juego antes de construir el Nivel 2...");
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => BuildLevel2Scene();
            return;
        }

        Debug.Log("[Level2Builder] Iniciando construcción oficial y densa del Nivel 2 (El Callejón del Beso)...");

        // Asegurar que las hojas de sprites estén debidamente recortadas
        SpriteSheetSlicer.SliceAllSpriteSheets();

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 2 Callejón Oficial");

        // Capas físicas
        int wallsLayer    = LayerMask.NameToLayer("Walls");
        int hideSpotLayer = LayerMask.NameToLayer("HideSpot");
        int playerLayer   = LayerMask.NameToLayer("Player");
        int lloronaLayer  = LayerMask.NameToLayer("Llorona");

        if (wallsLayer < 0) wallsLayer = 0;
        if (hideSpotLayer < 0) hideSpotLayer = 0;

        // Cargar baldosas persistentes y verificadas guardadas en disco (.asset)
        TileBase[] floorTiles = LoadFloorTiles();
        TileBase[] wallTiles  = LoadWallTiles();

        // ════════════════════════════════════════════════════════════════════════
        // 1. GRID Y TILEMAPS (Suelo empedrado continuo y Paredes envolventes)
        // ════════════════════════════════════════════════════════════════════════
        GameObject gridGo = GameObject.Find("Grid") ?? new GameObject("Grid");
        if (gridGo.GetComponent<Grid>() == null) gridGo.AddComponent<Grid>();
        Undo.RegisterCreatedObjectUndo(gridGo, "Grid");

        Tilemap sueloMap   = GetOrCreateTilemap(gridGo.transform, "Suelo",   -5, -1, false);
        Tilemap paredesMap = GetOrCreateTilemap(gridGo.transform, "Paredes",  3, wallsLayer, true);

        sueloMap.ClearAllTiles();
        paredesMap.ClearAllTiles();

        // Tinte atmosférico para evitar microcontraste excesivo
        sueloMap.color   = new Color(0.65f, 0.65f, 0.72f, 1.0f);
        paredesMap.color = new Color(0.88f, 0.88f, 0.90f, 1.0f);

        System.Random rnd = new System.Random(42);

        // ENVOLVENTE COMPLETA: x in [-6, 48], y in [-4, 27]
        // Se cubre absolutamente toda la cuadrícula: Suelo en la calle, Paredes en el resto (Cero huecos)
        for (int x = -6; x <= 48; x++)
        {
            for (int y = -4; y <= 27; y++)
            {
                if (IsPlayableStreet(x, y))
                {
                    TileBase t = floorTiles[rnd.Next(floorTiles.Length)];
                    sueloMap.SetTile(new Vector3Int(x, y, 0), t);
                }
                else
                {
                    TileBase m = wallTiles[Math.Abs(x * 3 + y * 7) % wallTiles.Length];
                    paredesMap.SetTile(new Vector3Int(x, y, 0), m);
                }
            }
        }

        ConfigureCompositeCollider(paredesMap.gameObject);

        // ════════════════════════════════════════════════════════════════════════
        // 2. LÍMITES PERIMETRALES DEFINIDOS (Carlos NO se puede salir del callejón)
        // ════════════════════════════════════════════════════════════════════════
        Transform limitsRoot = FreshRoot("Limites_Callejon");
        CreateStreetBoundaries(limitsRoot, wallsLayer);

        // ════════════════════════════════════════════════════════════════════════
        // 3. FACHADAS COLONIALES CONTINUAS (Muros visibles de 2 pisos en ambos lados)
        // ════════════════════════════════════════════════════════════════════════
        Transform propsRoot  = FreshRoot("Props");
        Transform decalsRoot = FreshRoot("Decals");
        Transform decorRoot  = FreshRoot("Decoracion");

        CreateStreetFacades(propsRoot, wallsLayer);

        // Cargar sprites individuales oficiales
        Sprite puerta0       = GetSprite(PuertasPath, "Puerta_0", 0);
        Sprite puerta1       = GetSprite(PuertasPath, "Puerta_1", 1);
        Sprite puerta2       = GetSprite(PuertasPath, "Puerta_2", 2);

        Sprite barril        = GetSprite(EsconditesPath, "Escondite_barril", 1);
        Sprite cajas         = GetSprite(EsconditesPath, "Escondite_cajas", 0);
        Sprite huacales      = GetSprite(SacosPath, "Huacales_apilados", 0);
        Sprite huacalRoto    = GetSprite(SacosPath, "Huacal_roto_paja", 1);
        Sprite costales      = GetSprite(SacosPath, "Costales_apilados", 4);
        Sprite canasta       = GetSprite(SacosPath, "Canasta_chiles", 3);
        Sprite canastaVacia  = GetSprite(SacosPath, "Canasta_vacia", 2);

        Sprite balcon        = GetSprite(AccesoriosPath, "Balcon_hierro", 2);
        Sprite macetaColg    = GetSprite(AccesoriosPath, "Maceta_colgante", 0);
        Sprite macetaSeca    = GetSprite(AccesoriosPath, "Maceta_suelo_seca", 1);
        Sprite farolPared    = GetSprite(AccesoriosPath, "Farol_pared", 3);

        Sprite charco0       = GetSprite(BotellasPath, "Charco_0", 0);
        Sprite charco1       = GetSprite(BotellasPath, "Charco_1", 1);
        Sprite charco2       = GetSprite(BotellasPath, "Charco_2", 2);
        Sprite charco3       = GetSprite(BotellasPath, "Charco_3", 3);
        Sprite botellas      = GetSprite(BotellasPath, "Botellas_caidas", 8);
        Sprite botVerde      = GetSprite(BotellasPath, "Botella_verde", 0);
        Sprite botellasRotas = GetSprite(BotellasPath, "Vidrios_rotos", 7);

        Sprite perroLatDer   = GetSprite(PerroPath, "Perro_lado_der", 3);
        Sprite perroFrente   = GetSprite(PerroPath, "Perro_frente", 0);
        Sprite perroEspalda  = GetSprite(PerroPath, "Perro_espalda", 1);

        Sprite salida1       = GetSprite(SalidasPath, "Salida_1", 1);
        Sprite papelPicado   = GetSprite(DecoracionPath, "Papel_picado", 5);
        Sprite boteSprite    = GetSprite(DecoracionPath, "Bote", 1);

        // ── A. 7 ESCONDITES VISIBLES Y ACCESIBLES PARA CARLOS ───────────────────
        // 1. Escondite 1 (Inicio en x=6.5, y=3.2): Clúster de huacales y barril
        CreateClusterHideSpot(propsRoot, "Escondite_1_HuacalesBarril", barril, huacales, canasta,
            6.5f, 3.2f, hideSpotLayer, wallsLayer);

        // 2. Escondite 2 (Tramo 1 en x=11.5, y=6.5): Portón colonial Puerta_0
        CreateDoorHideSpot(propsRoot, "Escondite_2_PortonCantera", puerta0, macetaSeca, huacales,
            11.5f, 6.5f, hideSpotLayer, wallsLayer);

        // 3. Escondite 3 (Curva del Codo 1 en x=18.5, y=3.8): Clúster de cajas coloniales
        CreateClusterHideSpot(propsRoot, "Escondite_3_Codo1Cajas", cajas, costales, canastaVacia,
            18.5f, 3.8f, hideSpotLayer, wallsLayer);

        // 4. Escondite 4 (Callejón del Beso vertical en x=15.8, y=11.5): Portón colonial Puerta_1
        CreateDoorHideSpot(propsRoot, "Escondite_4_PortonBeso", puerta1, huacalRoto, macetaSeca,
            15.8f, 11.5f, hideSpotLayer, wallsLayer);

        // 5. Escondite 5 (Curva del Codo 2 en x=18.5, y=14.5): Clúster de barril y huacales
        CreateClusterHideSpot(propsRoot, "Escondite_5_Codo2Barril", barril, huacales, costales,
            18.5f, 14.5f, hideSpotLayer, wallsLayer);

        // 6. Escondite 6 (Tramo Superior en x=27.5, y=20.5): Portón colonial Puerta_2
        CreateDoorHideSpot(propsRoot, "Escondite_6_PortonSuperior", puerta2, macetaSeca, canasta,
            27.5f, 20.5f, hideSpotLayer, wallsLayer);

        // 7. Escondite 7 (Recta Final en x=36.0, y=17.2): Gran clúster de cajas y barril
        CreateClusterHideSpot(propsRoot, "Escondite_7_SalidaBarril", cajas, barril, huacales,
            36.0f, 17.2f, hideSpotLayer, wallsLayer);

        // ── B. BALCONES DEL BESO ENFRENTADOS (Callejón del Beso) ─────────────────
        CreateBalcony(decorRoot, "Balcon_Oeste_Beso", balcon, 15.0f, 13.0f, true);
        CreateBalcony(decorRoot, "Balcon_Este_Beso",  balcon, 20.0f, 13.5f, false);

        CreateBalcony(decorRoot, "Balcon_Horizontal_1", balcon,  5.0f,  7.8f, false);
        CreateBalcony(decorRoot, "Balcon_Horizontal_2", balcon, 16.0f,  7.8f, false);
        CreateBalcony(decorRoot, "Balcon_Superior_1",   balcon, 24.0f, 21.8f, false);
        CreateBalcony(decorRoot, "Balcon_Superior_2",   balcon, 33.0f, 21.8f, false);

        // ── C. MACETAS COLONIALES Y BOTES EN LAS ACERAS ──────────────────────────
        CreateDecoProp(decorRoot, "Maceta_Colg_1", macetaColg, 15.0f, 14.2f, 1.0f);
        CreateDecoProp(decorRoot, "Maceta_Colg_2", macetaColg, 20.0f, 14.7f, 1.0f);
        CreateDecoProp(decorRoot, "Maceta_Colg_3", macetaColg, 24.0f, 22.8f, 1.0f);

        CreateDecoProp(decorRoot, "Maceta_Acera_1", macetaSeca,  3.5f,  2.2f, 1.0f, wallsLayer);
        CreateDecoProp(decorRoot, "Maceta_Acera_2", macetaSeca, 14.0f,  2.2f, 1.0f, wallsLayer);
        CreateDecoProp(decorRoot, "Maceta_Acera_3", macetaSeca, 21.2f,  8.5f, 1.0f, wallsLayer);
        CreateDecoProp(decorRoot, "Maceta_Acera_4", macetaSeca, 21.8f, 16.5f, 1.0f, wallsLayer);
        CreateDecoProp(decorRoot, "Maceta_Acera_5", macetaSeca, 38.5f, 16.5f, 1.0f, wallsLayer);

        CreateBote(decorRoot, "Bote_Callejon_1", boteSprite,  8.0f, 2.2f, wallsLayer);
        CreateBote(decorRoot, "Bote_Callejon_2", boteSprite, 22.5f, 20.8f, wallsLayer);

        // Papel picado tradicional
        CreateForegroundProp(decorRoot, "Papel_1", papelPicado,  8.0f,  6.8f, 1.2f);
        CreateForegroundProp(decorRoot, "Papel_2", papelPicado, 17.5f, 10.5f, 1.2f);
        CreateForegroundProp(decorRoot, "Papel_3", papelPicado, 30.0f, 20.8f, 1.2f);

        // ── D. FAROLES DE PARED PARPADEANTES (LightFlicker) ──────────────────────
        CreateWallFarol(decorRoot, "Farol_Pared_1", farolPared,  4.0f,  7.4f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_2", farolPared, 11.5f,  7.4f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_3", farolPared, 14.2f,  4.8f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_4", farolPared, 14.5f, 10.5f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_5", farolPared, 20.5f, 15.0f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_6", farolPared, 27.5f, 21.4f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_7", farolPared, 35.0f, 21.4f, wallsLayer);

        // ── E. CHARCOS DE LODO Y BOTELLAS EN ESQUINAS CIEGAS ─────────────────────
        CreateMudPuddle(decalsRoot, "Charco_1", charco0,  9.0f,  4.2f);
        CreateMudPuddle(decalsRoot, "Charco_2", charco1, 17.0f,  5.5f);
        CreateMudPuddle(decalsRoot, "Charco_3", charco2, 17.5f, 12.0f);
        CreateMudPuddle(decalsRoot, "Charco_4", charco3, 31.0f, 18.0f);

        CreateNoiseObstacle(decalsRoot, "Botellas_1", botellas, botellasRotas, 13.5f, 3.2f);
        CreateNoiseObstacle(decalsRoot, "Botellas_2", botVerde,  botellasRotas, 18.8f, 6.2f);
        CreateNoiseObstacle(decalsRoot, "Botellas_3", botellas, botellasRotas, 16.0f, 14.2f);
        CreateNoiseObstacle(decalsRoot, "Botellas_4", botVerde,  botellasRotas, 24.5f, 17.2f);

        // ── F. PERRO CALLEJERO TERRITORIAL DURMIENDO ─────────────────────────────
        CreateStrayDog(propsRoot, 19.8f, 15.0f, perroLatDer, perroFrente, perroEspalda, huacalRoto);

        // ── G. SALIDA MONUMENTAL DEL CALLEJÓN (Salida_1) ─────────────────────────
        CreateExitScene(propsRoot, 40.5f, 16.0f, salida1, wallsLayer);

        // ════════════════════════════════════════════════════════════════════════
        // 4. WAYPOINTS Y PATRULLA DE LA LLORONA
        // ════════════════════════════════════════════════════════════════════════
        Transform wpRoot = FreshRoot("Waypoints");
        Transform wpA = CreateEmpty(wpRoot, "WP_Codo1",   17.5f,  4.5f);
        Transform wpB = CreateEmpty(wpRoot, "WP_Balcones", 17.5f, 13.0f);
        Transform wpC = CreateEmpty(wpRoot, "WP_Codo2",   27.0f, 18.5f);

        EnsureGlobalLight(0.46f);
        SetupLlorona(new Vector2(17.5f, 18.5f), wpA, wpB, wpC, wallsLayer, lloronaLayer);

        // ════════════════════════════════════════════════════════════════════════
        // 5. CARLOS (PLAYER) Y CÁMARA
        // ════════════════════════════════════════════════════════════════════════
        SetupPlayer(new Vector2(2.5f, 4.5f), playerLayer);
        SetupCamera(new Vector2(2.5f, 4.5f));

        SetupGameManager(2);

        // ════════════════════════════════════════════════════════════════════════
        // 6. HUD OFICIAL (FASE 2)
        // ════════════════════════════════════════════════════════════════════════
        UIBuilder.BuildLevel2UICanvas();

        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("[Level2Builder] ¡Nivel 2 (El Callejón del Beso) construido con éxito, denso y sin huecos!");
    }

    // ── Lógica de Calles y Geometría ─────────────────────────────────────────

    static bool IsPlayableStreet(int x, int y)
    {
        // Tramo 1 (Horizontal oeste): x in [0, 20], y in [2, 7]
        if (x >= 0 && x <= 20 && y >= 2 && y <= 7) return true;

        // Tramo 2 (Vertical norte): x in [15, 20], y in [7, 21] (conecta con tramo 1 y 3)
        if (x >= 15 && x <= 20 && y >= 7 && y <= 21) return true;

        // Tramo 3 (Horizontal este hacia salida): x in [20, 41], y in [16, 21]
        if (x >= 20 && x <= 41 && y >= 16 && y <= 21) return true;

        return false;
    }

    // ── Límites Perimetrales Físicos (BoxCollider2D) ─────────────────────────

    static void CreateStreetBoundaries(Transform parent, int wallsLayer)
    {
        // 1. Inicio Oeste (detrás de Carlos al empezar)
        CreateLimitWall(parent, "Limite_Oeste_Inicio", -0.6f, 4.5f, 1.2f, 6.2f, wallsLayer);

        // 2. Tramo 1 Sur (x: -0.5 a 20.5, y = 1.4)
        CreateLimitWall(parent, "Limite_Sur_Tramo1", 10.0f, 1.4f, 22.0f, 1.2f, wallsLayer);

        // 3. Tramo 1 Norte (x: -0.5 a 14.5, y = 7.6)
        CreateLimitWall(parent, "Limite_Norte_Tramo1", 7.0f, 7.6f, 16.0f, 1.2f, wallsLayer);

        // 4. Tramo 2 Oeste (Callejón del Beso: y in [7.5, 21.5], x = 14.4)
        CreateLimitWall(parent, "Limite_Oeste_Beso", 14.4f, 14.5f, 1.2f, 14.2f, wallsLayer);

        // 5. Tramo 2 Este (y in [1.5, 15.5], x = 20.6)
        CreateLimitWall(parent, "Limite_Este_Beso", 20.6f, 8.5f, 1.2f, 14.2f, wallsLayer);

        // 6. Tramo 3 Sur (x in [20.5, 41.5], y = 15.4)
        CreateLimitWall(parent, "Limite_Sur_Tramo3", 31.0f, 15.4f, 22.0f, 1.2f, wallsLayer);

        // 7. Tramo 3 Norte (x in [14.5, 41.5], y = 21.6)
        CreateLimitWall(parent, "Limite_Norte_Tramo3", 28.0f, 21.6f, 28.0f, 1.2f, wallsLayer);

        // 8. Salida Este (detrás del arco de Salida_1)
        CreateLimitWall(parent, "Limite_Este_Salida", 42.6f, 18.5f, 1.2f, 6.2f, wallsLayer);
    }

    static void CreateLimitWall(Transform parent, string name, float x, float y, float w, float h, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(w, h);

        if (layer >= 0) go.layer = layer;
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    // ── Fachadas Coloniales Continuas (SpriteRenderer) ────────────────────────

    static void CreateStreetFacades(Transform parent, int wallsLayer)
    {
        Sprite[] facadeSprites = new Sprite[7];
        string[] names = {
            "Fachada_balcon_roja_0", "Fachada_ocre_1", "Fachada_azul_2",
            "Fachada_roja_3", "Fachada_ocre_4", "Fachada_morada_5", "Fachada_ventana_roja_6"
        };
        for (int i = 0; i < names.Length; i++)
        {
            facadeSprites[i] = GetSprite(TilesMuroPath, names[i], i);
        }

        GameObject facadesRoot = new GameObject("Fachadas_Coloniales");
        facadesRoot.transform.SetParent(parent, false);

        int idx = 0;

        // Tramo 1 Sur (x: -1 a 20.5, y = 0.6): Fachadas con teja
        for (float x = -1.0f; x <= 20.5f; x += 1.6f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T1_Sur_{idx}", facadeSprites[idx % facadeSprites.Length], x, 0.6f, 2, wallsLayer);
            idx++;
        }

        // Tramo 1 Norte (x: -1 a 14.5, y = 8.5): Fachadas norte
        for (float x = -1.0f; x <= 14.5f; x += 1.6f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T1_Norte_{idx}", facadeSprites[idx % facadeSprites.Length], x, 8.5f, 3, wallsLayer);
            idx++;
        }

        // Tramo 2 Oeste (Callejón del Beso: y in [7.5, 21.0], x = 13.8)
        for (float y = 7.5f; y <= 21.0f; y += 2.2f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T2_Oeste_{idx}", facadeSprites[idx % facadeSprites.Length], 13.8f, y, 3, wallsLayer);
            idx++;
        }

        // Tramo 2 Este (vertical: y in [0.6, 15.0], x = 21.2)
        for (float y = 0.6f; y <= 15.0f; y += 2.2f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T2_Este_{idx}", facadeSprites[idx % facadeSprites.Length], 21.2f, y, 3, wallsLayer);
            idx++;
        }

        // Tramo 3 Sur (x in [20.5, 41.5], y = 14.5)
        for (float x = 20.5f; x <= 41.5f; x += 1.6f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T3_Sur_{idx}", facadeSprites[idx % facadeSprites.Length], x, 14.5f, 2, wallsLayer);
            idx++;
        }

        // Tramo 3 Norte (x in [14.5, 41.5], y = 22.5)
        for (float x = 14.5f; x <= 41.5f; x += 1.6f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T3_Norte_{idx}", facadeSprites[idx % facadeSprites.Length], x, 22.5f, 3, wallsLayer);
            idx++;
        }

        // Cierre Entrada Oeste (x = -1.2, y in [1.5, 7.5])
        for (float y = 1.5f; y <= 7.5f; y += 2.0f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_Entrada_Oeste_{idx}", facadeSprites[idx % facadeSprites.Length], -1.2f, y, 3, wallsLayer);
            idx++;
        }
    }

    static void SpawnFacade(Transform parent, string name, Sprite sprite, float x, float y, int order, int layer)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;

        if (layer >= 0) go.layer = layer;
    }

    // ── Cargadores de Tilesets Persistentes (.asset) ─────────────────────────

    static TileBase[] LoadFloorTiles()
    {
        TileBase[] tiles = new TileBase[12];
        for (int i = 0; i < 12; i++)
        {
            tiles[i] = AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Tiles/tiles_{i}.asset");
        }
        return tiles;
    }

    static TileBase[] LoadWallTiles()
    {
        TileBase[] tiles = new TileBase[24];
        for (int i = 0; i < 24; i++)
        {
            tiles[i] = AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Tiles/tiles_{24 + i}.asset");
        }
        return tiles;
    }

    // ── Escondites Funcionales ───────────────────────────────────────────────

    static void CreateDoorHideSpot(Transform parent, string name, Sprite doorSprite, Sprite decorLeft, Sprite decorRight,
        float x, float y, int hideSpotLayer, int wallsLayer)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(x, y, 0f);

        // Puerta principal colonial
        GameObject doorGo = new GameObject("PuertaPrincipal");
        doorGo.transform.SetParent(root.transform, false);
        SpriteRenderer srDoor = doorGo.AddComponent<SpriteRenderer>();
        srDoor.sprite = doorSprite;
        srDoor.sortingOrder = 3;

        // Decoraciones a los lados para enmarcar el portón
        if (decorLeft != null)
        {
            GameObject decL = new GameObject("Deco_Izq");
            decL.transform.SetParent(root.transform, false);
            decL.transform.localPosition = new Vector3(-0.95f, -0.2f, 0f);
            SpriteRenderer srL = decL.AddComponent<SpriteRenderer>();
            srL.sprite = decorLeft;
            srL.sortingOrder = 3;
        }

        if (decorRight != null)
        {
            GameObject decR = new GameObject("Deco_Der");
            decR.transform.SetParent(root.transform, false);
            decR.transform.localPosition = new Vector3(0.95f, -0.2f, 0f);
            SpriteRenderer srR = decR.AddComponent<SpriteRenderer>();
            srR.sprite = decorRight;
            srR.sortingOrder = 3;
        }

        // Base sólida que bloquea el paso físico
        BoxCollider2D solidCol = root.AddComponent<BoxCollider2D>();
        solidCol.size   = new Vector2(2.6f, 1.8f);
        solidCol.offset = new Vector2(0f, 0.45f);
        if (wallsLayer >= 0) root.layer = wallsLayer;

        // Trigger frontal prominente para ocultarse pulsando [E]
        GameObject trigGo = new GameObject("TriggerOcultarse");
        trigGo.transform.SetParent(root.transform, false);
        BoxCollider2D trigCol = trigGo.AddComponent<BoxCollider2D>();
        trigCol.isTrigger = true;
        trigCol.size   = new Vector2(3.2f, 1.6f);
        trigCol.offset = new Vector2(0f, -0.7f);
        if (hideSpotLayer >= 0) trigGo.layer = hideSpotLayer;

        // Punto interior donde se oculta Carlos
        GameObject hidePt = new GameObject("HidePoint");
        hidePt.transform.SetParent(root.transform, false);
        hidePt.transform.localPosition = new Vector3(0f, 0.35f, 0f);

        HideSpot hs = root.AddComponent<HideSpot>();
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("hidePoint").objectReferenceValue = hidePt.transform;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(root, name);
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

        if (decorA != null)
        {
            GameObject decAGo = new GameObject("Deco_Izq");
            decAGo.transform.SetParent(root.transform, false);
            decAGo.transform.localPosition = new Vector3(-0.9f, -0.2f, 0f);
            SpriteRenderer srA = decAGo.AddComponent<SpriteRenderer>();
            srA.sprite = decorA;
            srA.sortingOrder = 3;
        }

        if (decorB != null)
        {
            GameObject decBGo = new GameObject("Deco_Der");
            decBGo.transform.SetParent(root.transform, false);
            decBGo.transform.localPosition = new Vector3(0.9f, -0.2f, 0f);
            SpriteRenderer srB = decBGo.AddComponent<SpriteRenderer>();
            srB.sprite = decorB;
            srB.sortingOrder = 3;
        }

        BoxCollider2D solidCol = root.AddComponent<BoxCollider2D>();
        solidCol.size   = new Vector2(2.8f, 1.7f);
        solidCol.offset = new Vector2(0f, 0.45f);
        if (wallsLayer >= 0) root.layer = wallsLayer;

        GameObject trigGo = new GameObject("TriggerOcultarse");
        trigGo.transform.SetParent(root.transform, false);
        BoxCollider2D trigCol = trigGo.AddComponent<BoxCollider2D>();
        trigCol.isTrigger = true;
        trigCol.size   = new Vector2(3.2f, 1.4f);
        trigCol.offset = new Vector2(0f, -0.6f);
        if (hideSpotLayer >= 0) trigGo.layer = hideSpotLayer;

        GameObject hidePt = new GameObject("HidePoint");
        hidePt.transform.SetParent(root.transform, false);
        hidePt.transform.localPosition = new Vector3(0f, 0.35f, 0f);

        HideSpot hs = root.AddComponent<HideSpot>();
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("hidePoint").objectReferenceValue = hidePt.transform;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(root, name);
    }

    // ── Decoración y Props ───────────────────────────────────────────────────

    static void CreateBalcony(Transform parent, string name, Sprite sprite, float x, float y, bool flipX)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 10; // Foreground
        sr.flipX = flipX;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateWallFarol(Transform parent, string name, Sprite sprite, float x, float y, int layer)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;

        if (layer >= 0) go.layer = layer;

        GameObject lightGo = new GameObject("Luz");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.1f, 0f);

        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.84f, 0.48f);
        l2d.intensity = 1.6f;
        l2d.pointLightInnerRadius = 0.8f;
        l2d.pointLightOuterRadius = 4.8f;
        lightGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateMudPuddle(Transform parent, string name, Sprite sprite, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1; // Decals
        sr.color = new Color(0.85f, 0.78f, 0.70f, 0.95f);

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
        sr.color = new Color(1.15f, 1.25f, 1.15f, 1.0f);

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(0.40f, 0.25f);

        NoiseObstacle no = go.AddComponent<NoiseObstacle>();
        SerializedObject so = new SerializedObject(no);
        so.FindProperty("brokenSprite").objectReferenceValue = brokenSprite;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateStrayDog(Transform parent, float x, float y, Sprite lateral, Sprite frente, Sprite espalda, Sprite strawBed)
    {
        GameObject bedGo = new GameObject("Cama_Perro");
        bedGo.transform.SetParent(parent, false);
        bedGo.transform.position = new Vector3(x, y - 0.2f, 0f);
        SpriteRenderer bedSr = bedGo.AddComponent<SpriteRenderer>();
        bedSr.sprite = strawBed;
        bedSr.sortingOrder = 2;
        bedSr.color = new Color(0.95f, 0.88f, 0.75f, 1f);

        GameObject go = new GameObject("Perro_Callejero_Callejon");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = lateral;
        sr.sortingOrder = 4;
        sr.color = new Color(1.05f, 0.98f, 0.90f, 1.0f);

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
        so.ApplyModifiedProperties();

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;

        GameObject dogLightGo = new GameObject("LuzPerro");
        dogLightGo.transform.SetParent(go.transform, false);
        dogLightGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        Light2D dogLight = dogLightGo.AddComponent<Light2D>();
        dogLight.lightType = Light2D.LightType.Point;
        dogLight.color = new Color(1.0f, 0.85f, 0.55f);
        dogLight.intensity = 0.85f;
        dogLight.pointLightInnerRadius = 0.4f;
        dogLight.pointLightOuterRadius = 2.4f;

        Undo.RegisterCreatedObjectUndo(go, "Perro_Callejero_Callejon");
    }

    static void CreateExitScene(Transform parent, float x, float y, Sprite exitSprite, int wallsLayer)
    {
        GameObject go = new GameObject("Salida_Callejon_Meta");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = exitSprite ?? GetSprite(SalidasPath, "Salida_1", 1);
        sr.sortingOrder = 3;

        BoxCollider2D leftCol = go.AddComponent<BoxCollider2D>();
        leftCol.size   = new Vector2(2.4f, 5.0f);
        leftCol.offset = new Vector2(-2.5f, 2.5f);

        BoxCollider2D rightCol = go.AddComponent<BoxCollider2D>();
        rightCol.size   = new Vector2(2.4f, 5.0f);
        rightCol.offset = new Vector2(2.5f, 2.5f);

        if (wallsLayer >= 0) go.layer = wallsLayer;

        GameObject goalGo = new GameObject("MetaTrigger");
        goalGo.transform.SetParent(go.transform, false);
        goalGo.transform.localPosition = new Vector3(0f, 2.0f, 0f);

        BoxCollider2D centerTrigger = goalGo.AddComponent<BoxCollider2D>();
        centerTrigger.isTrigger = true;
        centerTrigger.size = new Vector2(2.2f, 4.0f);
        goalGo.AddComponent<LevelGoal>();

        GameObject lightGo = new GameObject("LuzSalida");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.88f, 0.55f);
        l2d.intensity = 2.4f;
        l2d.pointLightInnerRadius = 1.0f;
        l2d.pointLightOuterRadius = 6.0f;
        lightGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, "Salida_Callejon_Meta");
    }

    static void CreateDecoProp(Transform parent, string name, Sprite sprite, float x, float y, float scale, int layer = -1)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        if (layer >= 0)
        {
            go.layer = layer;
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);
        }

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

    // ── Iluminación, Cámara y Entidades ──────────────────────────────────────

    static void EnsureGlobalLight(float intensity = 0.46f)
    {
        var allLights = UnityEngine.Object.FindObjectsByType<Light2D>(FindObjectsInactive.Include);
        Light2D mainGlobal = null;
        foreach (var l in allLights)
        {
            if (l == null) continue;
            if (l.lightType == Light2D.LightType.Global || l.gameObject.name.ToLower().Contains("global"))
            {
                if (mainGlobal == null) mainGlobal = l;
                else Undo.DestroyObjectImmediate(l.gameObject);
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
        mainGlobal.color = new Color(0.32f, 0.36f, 0.50f, 1.0f);
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
        if (player == null)
        {
            player = new GameObject("Player");
            player.tag = "Player";
            Undo.RegisterCreatedObjectUndo(player, "Create Player");
        }

        Undo.RecordObject(player.transform, "Posicionar Player");
        player.transform.position = new Vector3(pos.x, pos.y, 0f);
        player.transform.rotation = Quaternion.identity;
        if (playerLayer >= 0) player.layer = playerLayer;

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr == null) sr = player.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        sr.color = new Color(1.08f, 1.04f, 0.98f, 1.0f);

        // ANIMATOR COMPLETO CON CONTROLADOR OFICIAL
        Animator anim = player.GetComponent<Animator>();
        if (anim == null) anim = player.AddComponent<Animator>();
        RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Player/PlayerController.controller");
        if (ctrl != null)
        {
            anim.runtimeAnimatorController = ctrl;
            anim.enabled = true;
        }

        // SCRIPTS DE CONTROL Y ESTAMINA
        if (player.GetComponent<PlayerMovement>() == null)
            player.AddComponent<PlayerMovement>();

        if (player.GetComponent<PlayerAnimation>() == null)
            player.AddComponent<PlayerAnimation>();

        if (player.GetComponent<HideSystem>() == null)
            player.AddComponent<HideSystem>();

        // LUZ PERSONAL CÁLIDA DE CARLOS
        Transform existingLight = player.transform.Find("LuzCarlos");
        GameObject pLightGo = existingLight != null ? existingLight.gameObject : new GameObject("LuzCarlos");
        pLightGo.transform.SetParent(player.transform, false);
        pLightGo.transform.localPosition = new Vector3(0f, 0.15f, 0f);

        Light2D pLight = pLightGo.GetComponent<Light2D>();
        if (pLight == null) pLight = pLightGo.AddComponent<Light2D>();
        pLight.lightType = Light2D.LightType.Point;
        pLight.color = new Color(1.0f, 0.88f, 0.62f, 1.0f);
        pLight.intensity = 1.25f;
        pLight.pointLightInnerRadius = 0.8f;
        pLight.pointLightOuterRadius = 3.8f;

        if (pLightGo.GetComponent<LightFlicker>() == null)
            pLightGo.AddComponent<LightFlicker>();

        // FÍSICA Y COLISIÓN CALIBRADA A LOS PIES
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null) rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D col = player.GetComponent<BoxCollider2D>();
        if (col == null) col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.5f);
        col.offset = new Vector2(0f, -0.45f);

        CircleCollider2D cc = player.GetComponent<CircleCollider2D>();
        if (cc != null) Undo.DestroyObjectImmediate(cc);
    }

    static void SetupLlorona(Vector2 pos, Transform wpA, Transform wpB, Transform wpC, int wallsLayer, int lloronaLayer)
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
            wpProp.arraySize = 3;
            wpProp.GetArrayElementAtIndex(0).objectReferenceValue = wpA;
            wpProp.GetArrayElementAtIndex(1).objectReferenceValue = wpB;
            wpProp.GetArrayElementAtIndex(2).objectReferenceValue = wpC;
        }

        LloronaSettings s2 = AssetDatabase.LoadAssetAtPath<LloronaSettings>("Assets/Settings/LloronaSettings_Level2.asset");
        if (s2 != null) so.FindProperty("settings").objectReferenceValue = s2;

        so.ApplyModifiedProperties();
    }

    static void SetupCamera(Vector2 playerPos)
    {
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
        cam.transform.position = new Vector3(playerPos.x, playerPos.y, -10f);
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.05f);

        CameraFollow2D cf = cam.GetComponent<CameraFollow2D>();
        if (cf == null) cf = cam.gameObject.AddComponent<CameraFollow2D>();
        cf.SetBounds(new Vector2(6f, 4f), new Vector2(36f, 19f));
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

        Tilemap tm = go.GetComponent<Tilemap>();
        if (tm == null) tm = go.AddComponent<Tilemap>();
        TilemapRenderer tr = go.GetComponent<TilemapRenderer>();
        if (tr == null) tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = order;

        if (layer >= 0) go.layer = layer;
        if (hasCollider)
        {
            TilemapCollider2D tc = go.GetComponent<TilemapCollider2D>();
            if (tc == null) tc = go.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
        }

        return tm;
    }

    static void ConfigureCompositeCollider(GameObject go)
    {
        CompositeCollider2D comp = go.GetComponent<CompositeCollider2D>();
        if (comp == null) comp = go.AddComponent<CompositeCollider2D>();
        comp.geometryType = CompositeCollider2D.GeometryType.Polygons;

        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
            rb.simulated = true;
        }
    }
}
