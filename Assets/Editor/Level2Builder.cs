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
/// Utiliza los recursos auténticos de Guanajuato ubicados en:
/// Assets/Sprites/Environment/Nivel 2/Sprites/
/// 
/// Características del Nivel 2:
/// - Suelo: 12 baldosas de adoquín auténtico colonial (Tile_Adoquin_00 .. 11).
/// - Centro del Callejón del Beso: Pieza central emblemática de los balcones enfrentados (Callejon_Beso_Central).
/// - Fachadas coloniales continuas de 2 pisos (Fachada_Alta_Oeste, Fachada_Alta_Este, Fachada_Naranja, etc.).
/// - Accesorios de hierro forjado: Farola_Hierro_Alta, Farol_Pared_Muro con LightFlicker, Farol_Colgante, Balcon_Hierro_Barandal.
/// - Vegetación colonial: Enredadera_Cascada, Enredadera_Muro, Jardinera_Balcon, Maceta_Colgante, Maceta_Barro.
/// - Señalética auténtica de callejón: Poste_Senaletica_Callejon, Poste_Senaletica_Farol, Poste_Senaletica_3.
/// - Escondites funcionales: Portones coloniales (Porton_Colonial) con aviso [E], barriles y cajas.
/// - Obstáculos calibrados: 4 charcos de lodo y 4 pilas de botellas ruidosas (vidrio.mp3).
/// - Perro callejero durmiendo en lecho de paja con detección de proximidad (perro.mp3).
/// - Salida monumental con Arco_Colonial y Monumento_Salida.
/// - Patrulla de La Llorona (A -> B -> C) con audio posicional 3D y sistema de detección.
/// - Carlos con controlador de 4 direcciones, estamina, luz ambiental URP 2D y halo personal.
/// </summary>
public static class Level2Builder
{
    const string ScenePath = "Assets/Scenes/Level2_Callejon.unity";
    const string Nvl2Dir   = "Assets/Sprites/Environment/Nivel 2/Sprites/";

    // Rutas de Spritesheets auxiliares
    const string TilesMuroPath  = "Assets/Sprites/Environment/tiles_nvl2.png";
    const string SalidasPath    = "Assets/Sprites/Environment/Salidas.png";
    const string BotellasPath   = "Assets/Sprites/Environment/Botellas_charcolodo_Nvl1.png";
    const string EsconditesPath = "Assets/Sprites/Environment/escondites.png";
    const string SacosPath      = "Assets/Sprites/Environment/Contenedores_Sacos_Nvl1.png";
    const string PerroPath      = "Assets/Sprites/Perro/perro.png";
    const string DecoracionPath = "Assets/Sprites/Environment/decoracion.png";

    static Sprite GetSprite(string path, string name = null, int fallback = 0)
    {
        if (!File.Exists(path)) return null;
        var direct = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (direct != null) return direct;

        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if (string.IsNullOrEmpty(name)) return sprites.FirstOrDefault();
        var match = sprites.FirstOrDefault(s => s.name == name);
        if (match != null) return match;
        if (fallback >= 0 && fallback < sprites.Length) return sprites[fallback];
        return sprites.FirstOrDefault();
    }

    static Sprite GetNvl2Sprite(string filename)
    {
        string fullPath = Path.Combine(Nvl2Dir, filename);
        return GetSprite(fullPath);
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

        Debug.Log("[Level2Builder] Iniciando construcción del Nivel 2 (El Callejón del Beso) con assets oficiales de Nivel 2...");

        // Asegurar que las hojas de sprites auxiliares estén debidamente recortadas
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

        // Cargar baldosas de adoquín auténtico de Nivel 2
        TileBase[] floorTiles = EnsureNivel2FloorTiles();
        TileBase[] wallTiles  = LoadWallTiles();

        // ════════════════════════════════════════════════════════════════════════
        // 1. GRID Y TILEMAPS (Suelo empedrado colonial y Paredes perimetrales)
        // ════════════════════════════════════════════════════════════════════════
        GameObject gridGo = GameObject.Find("Grid") ?? new GameObject("Grid");
        if (gridGo.GetComponent<Grid>() == null) gridGo.AddComponent<Grid>();
        Undo.RegisterCreatedObjectUndo(gridGo, "Grid");

        Tilemap sueloMap   = GetOrCreateTilemap(gridGo.transform, "Suelo",   -5, -1, false);
        Tilemap paredesMap = GetOrCreateTilemap(gridGo.transform, "Paredes",  3, wallsLayer, true);

        sueloMap.ClearAllTiles();
        paredesMap.ClearAllTiles();

        sueloMap.color   = new Color(1.0f, 0.98f, 0.92f, 1.0f);
        paredesMap.color = new Color(0.30f, 0.30f, 0.38f, 1.0f);

        System.Random rnd = new System.Random(42);

        // ENVOLVENTE COMPLETA: x in [-6, 48], y in [-4, 27] (Cero huecos)
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
        // 2. LÍMITES PERIMETRALES DEFINIDOS (BoxCollider2D impenetrable)
        // ════════════════════════════════════════════════════════════════════════
        Transform limitsRoot = FreshRoot("Limites_Callejon");
        CreateStreetBoundaries(limitsRoot, wallsLayer);

        // ════════════════════════════════════════════════════════════════════════
        // 3. FACHADAS COLONIALES CONTINUAS (SpriteRenderer con assets de Nivel 2)
        // ════════════════════════════════════════════════════════════════════════
        Transform propsRoot  = FreshRoot("Props");
        Transform decalsRoot = FreshRoot("Decals");
        Transform decorRoot  = FreshRoot("Decoracion");

        CreateStreetFacades(propsRoot, wallsLayer);

        // Cargar sprites auténticos de Nivel 2
        Sprite portonColonial    = GetNvl2Sprite("Porton_Colonial.png");
        Sprite callejonBesoPiece = GetNvl2Sprite("Callejon_Beso_Central.png");
        Sprite farolaHierro      = GetNvl2Sprite("Farola_Hierro_Alta.png");
        Sprite farolPared        = GetNvl2Sprite("Farol_Pared_Muro.png");
        Sprite farolColgante     = GetNvl2Sprite("Farol_Colgante.png");
        Sprite farolChico        = GetNvl2Sprite("Farol_Pared_Chico.png");
        Sprite barandalBalcon    = GetNvl2Sprite("Balcon_Hierro_Barandal.png");
        Sprite enredaderaCascada = GetNvl2Sprite("Enredadera_Cascada.png");
        Sprite enredaderaMuro    = GetNvl2Sprite("Enredadera_Muro.png");
        Sprite jardineraBalcon   = GetNvl2Sprite("Jardinera_Balcon.png");
        Sprite macetaBarro       = GetNvl2Sprite("Maceta_Barro.png");
        Sprite macetaColgante    = GetNvl2Sprite("Maceta_Colgante.png");
        Sprite signCallejon      = GetNvl2Sprite("Poste_Senaletica_Callejon.png");
        Sprite signFarol         = GetNvl2Sprite("Poste_Senaletica_Farol.png");
        Sprite signPlaca3        = GetNvl2Sprite("Poste_Senaletica_3.png");
        Sprite signPlaca1        = GetNvl2Sprite("Poste_Senaletica_1.png");
        Sprite monumentoSalida   = GetNvl2Sprite("Monumento_Salida.png");
        Sprite arcoColonial      = GetNvl2Sprite("Arco_Colonial.png");

        // Sprites auxiliares
        Sprite barril        = GetSprite(EsconditesPath, "Escondite_barril", 1);
        Sprite cajas         = GetSprite(EsconditesPath, "Escondite_cajas", 0);
        Sprite huacales      = GetSprite(SacosPath, "Huacales_apilados", 0);
        Sprite huacalRoto    = GetSprite(SacosPath, "Huacal_roto_paja", 1);
        Sprite costales      = GetSprite(SacosPath, "Costales_apilados", 4);
        Sprite canasta       = GetSprite(SacosPath, "Canasta_chiles", 3);
        Sprite canastaVacia  = GetSprite(SacosPath, "Canasta_vacia", 2);

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

        // ── A. 7 ESCONDITES VISIBLES Y ACCESIBLES PARA CARLOS (GDD: puertas y barriles) ─
        // 1. Escondite 1 (Inicio Tramo 1 en x=6.5, y=3.2): Huacales y barril colonial
        CreateClusterHideSpot(propsRoot, "Escondite_1_HuacalesBarril", barril, huacales, canasta,
            6.5f, 3.2f, hideSpotLayer, wallsLayer);

        // 2. Escondite 2 (Tramo 1 en x=11.5, y=6.5): Portón colonial auténtico con maceta de barro
        CreateDoorHideSpot(propsRoot, "Escondite_2_PortonCantera", portonColonial, macetaBarro, enredaderaMuro,
            11.5f, 6.5f, hideSpotLayer, wallsLayer);

        // 3. Escondite 3 (Curva del Codo 1 en x=18.5, y=3.8): Cajas coloniales y costales
        CreateClusterHideSpot(propsRoot, "Escondite_3_Codo1Cajas", cajas, costales, canastaVacia,
            18.5f, 3.8f, hideSpotLayer, wallsLayer);

        // 4. Escondite 4 (Callejón del Beso vertical en x=15.8, y=11.5): Portón colonial
        CreateDoorHideSpot(propsRoot, "Escondite_4_PortonBeso", portonColonial, enredaderaCascada, macetaBarro,
            15.8f, 11.5f, hideSpotLayer, wallsLayer);

        // 5. Escondite 5 (Curva del Codo 2 en x=18.5, y=14.5): Barril y huacales
        CreateClusterHideSpot(propsRoot, "Escondite_5_Codo2Barril", barril, huacales, costales,
            18.5f, 14.5f, hideSpotLayer, wallsLayer);

        // 6. Escondite 6 (Tramo Superior en x=27.5, y=20.5): Portón colonial
        CreateDoorHideSpot(propsRoot, "Escondite_6_PortonSuperior", portonColonial, macetaBarro, canasta,
            27.5f, 20.5f, hideSpotLayer, wallsLayer);

        // 7. Escondite 7 (Recta Final pegado al muro sur): Gran clúster de cajas y barril colonial
        CreateClusterHideSpot(propsRoot, "Escondite_7_SalidaBarril", cajas, barril, huacales,
            34.0f, 15.6f, hideSpotLayer, wallsLayer);

        // ── B. EL CALLEJÓN DEL BESO: BALCONES ENFRENTADOS (Callejón del Beso) ───
        CreateBalcony(decorRoot, "Balcon_Oeste_Beso", barandalBalcon, 15.0f, 13.0f, true);
        CreateBalcony(decorRoot, "Balcon_Este_Beso",  barandalBalcon, 20.0f, 13.5f, false);

        // Balcones de hierro forjado adicionales
        CreateBalcony(decorRoot, "Balcon_Oeste_Beso", barandalBalcon, 15.0f, 10.5f, true);
        CreateBalcony(decorRoot, "Balcon_Este_Beso",  barandalBalcon, 20.0f, 16.5f, false);
        CreateBalcony(decorRoot, "Balcon_Horizontal_1", barandalBalcon,  5.0f,  7.8f, false);
        CreateBalcony(decorRoot, "Balcon_Superior_1",   barandalBalcon, 24.0f, 21.8f, false);
        CreateBalcony(decorRoot, "Balcon_Superior_2",   barandalBalcon, 33.0f, 21.8f, false);

        // ── C. VEGETACIÓN Y ENREDADERAS COLONIALES ────────────────────────────────
        CreateDecoProp(decorRoot, "Enredadera_1", enredaderaCascada, 14.8f, 15.5f, 1.0f);
        CreateDecoProp(decorRoot, "Enredadera_2", enredaderaCascada, 20.2f, 11.5f, 1.0f);
        CreateDecoProp(decorRoot, "Enredadera_3", enredaderaMuro,     6.0f,  7.6f, 1.0f);
        CreateDecoProp(decorRoot, "Enredadera_4", enredaderaMuro,    34.0f, 21.8f, 1.0f);

        CreateDecoProp(decorRoot, "Jardinera_1", jardineraBalcon, 15.0f, 10.0f, 1.0f);
        CreateDecoProp(decorRoot, "Jardinera_2", jardineraBalcon, 20.0f, 16.0f, 1.0f);

        CreateDecoProp(decorRoot, "Maceta_Colg_1", macetaColgante, 15.2f, 13.8f, 1.0f);
        CreateDecoProp(decorRoot, "Maceta_Colg_2", macetaColgante, 19.8f, 14.2f, 1.0f);
        CreateDecoProp(decorRoot, "Maceta_Colg_3", macetaColgante, 25.0f, 22.0f, 1.0f);

        CreateDecoProp(decorRoot, "Maceta_Acera_1", macetaBarro,  3.5f,  2.2f, 1.0f, wallsLayer);
        CreateDecoProp(decorRoot, "Maceta_Acera_2", macetaBarro, 14.0f,  2.2f, 1.0f, wallsLayer);
        CreateDecoProp(decorRoot, "Maceta_Acera_3", macetaBarro, 21.2f,  8.5f, 1.0f, wallsLayer);
        CreateDecoProp(decorRoot, "Maceta_Acera_4", macetaBarro, 21.8f, 16.5f, 1.0f, wallsLayer);
        CreateDecoProp(decorRoot, "Maceta_Acera_5", macetaBarro, 37.5f, 15.2f, 1.0f, wallsLayer);

        // ── D. SEÑALÉTICA AUTÉNTICA Y GUÍAS DE DIRECCIÓN ────────────────────────
        CreateDecoProp(decorRoot, "Senal_Callejon_Beso", signCallejon, 3.0f, 3.0f, 1.0f);
        CreateDecoProp(decorRoot, "Senal_Farol_Entrada", signFarol, 15.2f, 6.2f, 1.0f);
        CreateDecoProp(decorRoot, "Senal_Placa_Historica", signPlaca3, 19.5f, 11.0f, 1.0f);
        CreateDecoProp(decorRoot, "Senal_Salida_Placa", signPlaca1, 34.0f, 17.0f, 1.0f);

        // Guías direccionales iluminadas para indicar por dónde subir y doblar
        CreateDirectionSign(decorRoot, "Guia_Codo1_Subir_Beso", 17.5f, 3.0f, 2.8f, 6.5f);
        CreateDirectionSign(decorRoot, "Guia_Codo2_Giro_Salida", 18.5f, 20.8f, 2.6f, 6.0f);

        // Papel picado tradicional de pueblo mágico
        CreateForegroundProp(decorRoot, "Papel_1", papelPicado,  8.0f,  6.8f, 1.2f);
        CreateForegroundProp(decorRoot, "Papel_2", papelPicado, 17.5f,  9.5f, 1.2f);
        CreateForegroundProp(decorRoot, "Papel_3", papelPicado, 30.0f, 20.8f, 1.2f);

        // ── E. FAROLES DE PARED PARPADEANTES Y FAROLAS DE HIERRO (GDD) ───────────
        // Farolas de pie altas
        CreateStreetLamp(decorRoot, "Farola_Inicio", farolaHierro, 1.5f, 3.0f, wallsLayer);
        CreateStreetLamp(decorRoot, "Farola_Final",  farolaHierro, 38.0f, 20.8f, wallsLayer);

        // Faroles de pared con LightFlicker
        CreateWallFarol(decorRoot, "Farol_Pared_1", farolPared,  4.0f,  7.4f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_2", farolPared, 11.5f,  7.4f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_3", farolPared, 14.2f,  4.8f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_4", farolPared, 14.5f, 10.5f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_5", farolPared, 20.5f, 15.0f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_6", farolPared, 27.5f, 21.4f, wallsLayer);
        CreateWallFarol(decorRoot, "Farol_Pared_7", farolPared, 35.0f, 21.4f, wallsLayer);

        // Faroles colgantes
        CreateHangingLantern(decorRoot, "Farol_Colgante_1", farolColgante, 17.5f, 16.5f);
        CreateHangingLantern(decorRoot, "Farol_Colgante_2", farolColgante, 23.0f, 17.0f);

        // Sconces chicos
        CreateWallSconce(decorRoot, "Farol_Chico_1", farolChico, 14.5f, 8.5f);
        CreateWallSconce(decorRoot, "Farol_Chico_2", farolChico, 20.5f, 18.5f);

        // ── F. CHARCOS DE LODO Y BOTELLAS EN ESQUINAS CIEGAS ─────────────────────
        CreateMudPuddle(decalsRoot, "Charco_1", charco0,  9.0f,  4.2f);
        CreateMudPuddle(decalsRoot, "Charco_2", charco1, 17.0f,  5.5f);
        CreateMudPuddle(decalsRoot, "Charco_3", charco2, 17.5f, 12.0f);
        CreateMudPuddle(decalsRoot, "Charco_4", charco3, 31.0f, 18.0f);

        CreateNoiseObstacle(decalsRoot, "Botellas_1", botellas, botellasRotas, 13.5f, 3.2f);
        CreateNoiseObstacle(decalsRoot, "Botellas_2", botVerde,  botellasRotas, 18.8f, 6.2f);
        CreateNoiseObstacle(decalsRoot, "Botellas_3", botellas, botellasRotas, 16.0f, 14.2f);
        CreateNoiseObstacle(decalsRoot, "Botellas_4", botVerde,  botellasRotas, 24.5f, 17.2f);

        // ── G. PERRO CALLEJERO TERRITORIAL DURMIENDO (GDD: ladra y alerta) ───────
        CreateStrayDog(propsRoot, 19.8f, 15.0f, perroLatDer, perroFrente, perroEspalda, huacalRoto);

        // ── H. SALIDA MONUMENTAL DEL CALLEJÓN (Monumento_Salida y Arco_Colonial) ─
        CreateExitSceneNivel2(propsRoot, 40.5f, 16.0f, arcoColonial, monumentoSalida, wallsLayer);

        // ════════════════════════════════════════════════════════════════════════
        // 4. WAYPOINTS Y PATRULLA DE LA LLORONA (GDD: Patrulla A -> B -> C)
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
        Debug.Log("[Level2Builder] ¡Nivel 2 (El Callejón del Beso) construido exitosamente con assets oficiales de Nivel 2!");
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

        // 8. Salida Este (detrás del arco de Salida)
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
        // Variedad de fachadas auténticas de Guanajuato (Nivel 2)
        Sprite[] facadeSprites = new Sprite[]
        {
            GetNvl2Sprite("Fachada_Alta_Oeste.png"),
            GetNvl2Sprite("Fachada_Alta_Este.png"),
            GetNvl2Sprite("Fachada_Naranja.png"),
            GetNvl2Sprite("Fachada_Amarilla.png"),
            GetNvl2Sprite("Fachada_Terracota.png"),
            GetNvl2Sprite("Fachada_Ocre.png"),
            GetNvl2Sprite("Casa_Colonial_1.png"),
            GetNvl2Sprite("Casa_Colonial_2.png"),
            GetNvl2Sprite("Casa_Colonial_3.png"),
            GetNvl2Sprite("Casa_Colonial_4.png")
        }.Where(s => s != null).ToArray();

        if (facadeSprites.Length == 0)
        {
            // Respaldo de seguridad si algún sprite falla al cargar
            facadeSprites = new Sprite[] { GetSprite(TilesMuroPath, "Fachada_ocre_1", 1) };
        }

        GameObject facadesRoot = new GameObject("Fachadas_Coloniales");
        facadesRoot.transform.SetParent(parent, false);

        int idx = 0;

        // Tramo 1 Sur (x: -1 a 20.5, y = 0.6)
        for (float x = -1.0f; x <= 20.5f; x += 3.2f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T1_Sur_{idx}", facadeSprites[idx % facadeSprites.Length], x, 0.6f, 2, wallsLayer);
            idx++;
        }

        // Tramo 1 Norte (x: -1 a 14.5, y = 8.5)
        for (float x = -1.0f; x <= 14.5f; x += 3.2f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T1_Norte_{idx}", facadeSprites[idx % facadeSprites.Length], x, 8.5f, 3, wallsLayer);
            idx++;
        }

        // Tramo 2 Oeste (Callejón del Beso: y in [7.5, 21.0], x = 13.5)
        for (float y = 7.5f; y <= 21.0f; y += 3.8f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T2_Oeste_{idx}", facadeSprites[idx % facadeSprites.Length], 13.5f, y, 3, wallsLayer);
            idx++;
        }

        // Tramo 2 Este (vertical: y in [0.6, 15.0], x = 21.5)
        for (float y = 0.6f; y <= 15.0f; y += 3.8f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T2_Este_{idx}", facadeSprites[idx % facadeSprites.Length], 21.5f, y, 3, wallsLayer);
            idx++;
        }

        // Tramo 3 Sur (x in [20.5, 41.5], y = 14.5)
        for (float x = 20.5f; x <= 41.5f; x += 3.2f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T3_Sur_{idx}", facadeSprites[idx % facadeSprites.Length], x, 14.5f, 2, wallsLayer);
            idx++;
        }

        // Tramo 3 Norte (x in [14.5, 41.5], y = 22.5)
        for (float x = 14.5f; x <= 41.5f; x += 3.2f)
        {
            SpawnFacade(facadesRoot.transform, $"Fachada_T3_Norte_{idx}", facadeSprites[idx % facadeSprites.Length], x, 22.5f, 3, wallsLayer);
            idx++;
        }

        // Cierre Entrada Oeste (x = -1.2, y in [1.5, 7.5])
        for (float y = 1.5f; y <= 7.5f; y += 3.0f)
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

    static TileBase[] EnsureNivel2FloorTiles()
    {
        string dir = "Assets/Tiles/Level2";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        TileBase[] tiles = new TileBase[12];
        for (int i = 0; i < 12; i++)
        {
            string spritePath = $"{Nvl2Dir}Tile_Adoquin_{i:D2}.png";
            string tilePath   = $"{dir}/adoquin_{i}.asset";

            Tile t = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            if (t == null)
            {
                t = ScriptableObject.CreateInstance<Tile>();
                t.sprite = sp;
                t.colliderType = Tile.ColliderType.None;
                AssetDatabase.CreateAsset(t, tilePath);
            }
            else
            {
                if (sp != null && t.sprite != sp)
                {
                    t.sprite = sp;
                    EditorUtility.SetDirty(t);
                }
            }
            tiles[i] = t;
        }

        AssetDatabase.SaveAssets();
        return tiles;
    }

    static TileBase[] LoadWallTiles()
    {
        TileBase[] tiles = new TileBase[12];
        for (int i = 0; i < 12; i++)
        {
            // Usar baldosas sólidas de muro
            tiles[i] = AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Tiles/tiles_{i}.asset");
        }
        return tiles;
    }

    // ── Pieza Emblemática Central del Callejón del Beso ──────────────────────

    static void CreateCentralCallejonPiece(Transform parent, string name, Sprite sprite, float x, float y)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 9; // Por encima de los muros y debajo de elementos de primer plano

        // Luz tenue cálida que emana entre los balcones
        GameObject balconyLightGo = new GameObject("LuzBalconesEnfrentados");
        balconyLightGo.transform.SetParent(go.transform, false);
        balconyLightGo.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        Light2D bLight = balconyLightGo.AddComponent<Light2D>();
        bLight.lightType = Light2D.LightType.Point;
        bLight.color = new Color(1.0f, 0.88f, 0.65f, 1.0f);
        bLight.intensity = 1.35f;
        bLight.pointLightInnerRadius = 0.5f;
        bLight.pointLightOuterRadius = 3.5f;
        balconyLightGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    // ── Escondites Funcionales ───────────────────────────────────────────────

    static void CreateDoorHideSpot(Transform parent, string name, Sprite doorSprite, Sprite decorLeft, Sprite decorRight,
        float x, float y, int hideSpotLayer, int wallsLayer)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(x, y, 0f);

        GameObject doorGo = new GameObject("PuertaPrincipal");
        doorGo.transform.SetParent(root.transform, false);
        SpriteRenderer srDoor = doorGo.AddComponent<SpriteRenderer>();
        srDoor.sprite = doorSprite;
        srDoor.sortingOrder = 3;

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

        BoxCollider2D solidCol = root.AddComponent<BoxCollider2D>();
        solidCol.size   = new Vector2(2.6f, 1.8f);
        solidCol.offset = new Vector2(0f, 0.45f);
        if (wallsLayer >= 0) root.layer = wallsLayer;

        GameObject trigGo = new GameObject("TriggerOcultarse");
        trigGo.transform.SetParent(root.transform, false);
        BoxCollider2D trigCol = trigGo.AddComponent<BoxCollider2D>();
        trigCol.isTrigger = true;
        trigCol.size   = new Vector2(3.2f, 1.6f);
        trigCol.offset = new Vector2(0f, -0.7f);
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

    static void CreateClusterHideSpot(Transform parent, string name, Sprite mainSprite, Sprite decorA, Sprite decorB,
        float x, float y, int hideSpotLayer, int wallsLayer)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(x, y, 0f);

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
        sr.sortingOrder = 10;
        sr.flipX = flipX;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateStreetLamp(Transform parent, string name, Sprite sprite, float x, float y, int layer)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;

        if (layer >= 0)
        {
            go.layer = layer;
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 0.5f);
            col.offset = new Vector2(0f, -1.2f);
        }

        GameObject lightGo = new GameObject("LuzFarola");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);

        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.88f, 0.55f);
        l2d.intensity = 1.9f;
        l2d.pointLightInnerRadius = 0.8f;
        l2d.pointLightOuterRadius = 5.2f;
        lightGo.AddComponent<LightFlicker>();

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

    static void CreateHangingLantern(Transform parent, string name, Sprite sprite, float x, float y)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;

        GameObject lightGo = new GameObject("LuzColgante");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, -0.2f, 0f);

        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.86f, 0.52f);
        l2d.intensity = 1.4f;
        l2d.pointLightInnerRadius = 0.6f;
        l2d.pointLightOuterRadius = 3.6f;
        lightGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateWallSconce(Transform parent, string name, Sprite sprite, float x, float y)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;

        GameObject lightGo = new GameObject("LuzChica");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0f, 0f);

        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.82f, 0.50f);
        l2d.intensity = 1.1f;
        l2d.pointLightInnerRadius = 0.4f;
        l2d.pointLightOuterRadius = 2.8f;
        lightGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateDirectionSign(Transform parent, string name, float x, float y, float intensity, float radius)
    {
        Sprite placa = UIBuilder.PlacaResaltada;
        if (placa == null) return;

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * 0.55f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = placa;
        sr.sortingOrder = 12;

        GameObject lightGo = new GameObject("LuzGuia");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = Vector3.zero;

        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.92f, 0.55f);
        l2d.intensity = intensity;
        l2d.pointLightInnerRadius = 0.8f;
        l2d.pointLightOuterRadius = radius;
        lightGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateMetaMarkerSign(Transform parent, float x, float y, float intensity, float radius)
    {
        Sprite placa = UIBuilder.PlacaResaltada;
        if (placa == null) return;

        GameObject go = new GameObject("Cartel_Luminoso_Meta");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * 0.70f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = placa;
        sr.sortingOrder = 12;

        GameObject lightGo = new GameObject("LuzBeaconMeta");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = Vector3.zero;

        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.95f, 0.45f);
        l2d.intensity = intensity;
        l2d.pointLightInnerRadius = 1.0f;
        l2d.pointLightOuterRadius = radius;
        lightGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, "Cartel_Luminoso_Meta");
    }

    static void CreateMudPuddle(Transform parent, string name, Sprite sprite, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;
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
        sr.sortingOrder = 2;
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

    static void CreateExitSceneNivel2(Transform parent, float x, float y, Sprite arco, Sprite monumento, int wallsLayer)
    {
        GameObject go = new GameObject("Salida_Callejon_Meta");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        // Arco colonial
        GameObject archGo = new GameObject("ArcoColonial");
        archGo.transform.SetParent(go.transform, false);
        SpriteRenderer srArch = archGo.AddComponent<SpriteRenderer>();
        srArch.sprite = arco ?? GetSprite(SalidasPath, "Salida_1", 1);
        srArch.sortingOrder = 3;

        // Monumento / pedestal histórico a un costado
        if (monumento != null)
        {
            GameObject monGo = new GameObject("MonumentoHistorico");
            monGo.transform.SetParent(go.transform, false);
            monGo.transform.localPosition = new Vector3(-2.0f, -0.5f, 0f);
            SpriteRenderer srMon = monGo.AddComponent<SpriteRenderer>();
            srMon.sprite = monumento;
            srMon.sortingOrder = 3;
        }

        if (wallsLayer >= 0) go.layer = wallsLayer;

        // Zona amplia de meta en el umbral del arco (sin colisionadores solidos que bloqueen el paso)
        GameObject goalGo = new GameObject("MetaTrigger");
        goalGo.transform.SetParent(go.transform, false);
        goalGo.transform.localPosition = new Vector3(-1.5f, 2.0f, 0f);

        BoxCollider2D centerTrigger = goalGo.AddComponent<BoxCollider2D>();
        centerTrigger.isTrigger = true;
        centerTrigger.size = new Vector2(4.5f, 5.0f);
        goalGo.AddComponent<LevelGoal>();

        GameObject lightGo = new GameObject("LuzSalida");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(-0.5f, 2.0f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.90f, 0.50f);
        l2d.intensity = 3.6f;
        l2d.pointLightInnerRadius = 1.5f;
        l2d.pointLightOuterRadius = 8.5f;
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

    static void CreateForegroundProp(Transform parent, string name, Sprite sprite, float x, float y, float scale)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 10;
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

        if (player.GetComponent<HideSystem>() == null)
            player.AddComponent<HideSystem>();

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
