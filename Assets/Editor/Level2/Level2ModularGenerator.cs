#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Generador Oficial y Definitivo del Nivel 2: El Callejón del Beso (Guanajuato Nocturno).
/// Recrea fielmente el estilo, arquitectura y paleta de la imagen de referencia:
/// - Suelo: ÚNICAMENTE adoquines oscuros empedrados auténticos de Guanajuato (cero mosaicos ni ladrillos rojos).
/// - Fachadas: Casas coloniales de 1 y 2 pisos asentadas directamente en el suelo empedrado (puertas a nivel de calle).
/// - Escalera Central de Cantera: Conecta directamente la calle baja con la terraza alta sin obstáculos en el camino.
/// - Terraza Alta del Beso: Pasarela elevada con piso empedrado, barandal de hierro forjado al frente y casas al fondo.
/// - Iluminación: Faroles de muro cálidos montados en las paredes (no atravesados en la calle).
/// - Meta: Entrada a la Bóveda / Túnel Colonial de cantera al final de la calle.
/// - Señalética: CERO textos 3D en el mundo; solo 3 postes con flechas direccionales y el halo dorado en el túnel de meta.
/// </summary>
public static class Level2ModularGenerator
{
    const string Nvl2Dir = "Assets/Sprites/Environment/Nivel 2/Sprites/";
    const string CrateSpritesPath = "Assets/Sprites/_originals/Contenedores_Sacos_Nvl1.png";
    const string ObsSpritesPath = "Assets/Sprites/Environment/Botellas_charcolodo_Nvl1.png";

    public static void GenerateLevel(GameObject rootEnv)
    {
        // ----------------------------------------------------
        // 1. CARGA DE SPRITES AUTÉNTICOS SEGÚN SU VERDADERA IDENTIDAD
        // ----------------------------------------------------
        // Escalera de Cantera del Callejón del Beso
        Sprite stairsSprite     = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Callejon_Beso_Central.png");

        // Casas Coloniales
        Sprite casa2PisosOrange = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Arco_Colonial.png");          // Casa colonial de 2 pisos con balcón
        Sprite casa1PisoYellow  = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Casa_Colonial_1.png");         // Casa de 1 piso amarilla con puerta y farol
        Sprite casa1PisoOrange  = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Monumento_Salida.png");        // Casa de 1 piso naranja con puerta y ventana
        Sprite muroMamposteria  = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Fachada_Naranja.png");         // Muro de piedra/mampostería para contención
        Sprite muroPiedraLiso   = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Muro_Piedra_Bajo.png");

        // Salida: Túnel Colonial de cantera con arco profundo
        Sprite tunelSalida      = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Fachada_Alta_Este.png");

        // Accesorios de Hierro y Decoración
        Sprite barandalSprite   = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Balcon_Hierro_Barandal.png");
        Sprite farolPared       = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Farol_Pared_Muro.png");
        Sprite letreroFlecha    = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Poste_Senaletica_1.png");
        Sprite letreroCallejon  = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Poste_Senaletica_Callejon.png");
        Sprite macetaFlores     = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Maceta_Colgante.png");
        Sprite jardineraVides   = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Jardinera_Balcon.png");

        // Suelo: ÚNICAMENTE los adoquines empedrados oscuros homogéneos (00, 01, 02, 03, 08, 09)
        // Se excluyen 04, 07 (ladrillos rojos) y 10 (mármol blanco) para evitar el efecto mosaico
        Sprite[] adoquinesPuros = new[]
        {
            AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Tile_Adoquin_00.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Tile_Adoquin_01.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Tile_Adoquin_02.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Tile_Adoquin_03.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Tile_Adoquin_08.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Tile_Adoquin_09.png")
        }.Where(s => s != null).ToArray();

        // Props de sigilo y obstáculos
        Sprite[] crateSprites = AssetDatabase.LoadAllAssetsAtPath(CrateSpritesPath).OfType<Sprite>().ToArray();
        Sprite crateSprite  = crateSprites.FirstOrDefault(s => s.name.Contains("0") || s.name.Contains("Huacal")) ?? crateSprites.FirstOrDefault();
        Sprite sackSprite   = crateSprites.FirstOrDefault(s => s.name.Contains("Costal") || s.name.Contains("maiz")) ?? crateSprite;
        Sprite barrelSprite = crateSprites.FirstOrDefault(s => s.name.Contains("Canasta") || s.name.Contains("4")) ?? crateSprite;

        Sprite[] obsSprites = AssetDatabase.LoadAllAssetsAtPath(ObsSpritesPath).OfType<Sprite>().ToArray();
        Sprite mudSprite    = obsSprites.FirstOrDefault(s => s.name.Contains("Charco")) ?? obsSprites.FirstOrDefault();
        Sprite botSprite    = obsSprites.FirstOrDefault(s => s.name.Contains("Botella")) ?? obsSprites.LastOrDefault();
        Sprite brokenSprite = obsSprites.FirstOrDefault(s => s.name == "Vidrios_rotos");
        AudioClip glassSound= AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/vidrio.mp3");

        // ----------------------------------------------------
        // 2. FONDO Y TELÓN NOCTURNO
        // ----------------------------------------------------
        GameObject bgGroup = GetOrCreateChild(rootEnv, "Fondo_Guanajuato_Nocturno");
        CreateSolidBackdrop(bgGroup, "Telon_Cielo_Nocturno", new Vector3(20.0f, 4.0f, 0f), new Vector3(90f, 22f, 1f));

        // ----------------------------------------------------
        // 3. FACHADAS CERCANAS JUGABLES (ASENTADAS A NIVEL DE CALLE Y TERRAZA)
        // ----------------------------------------------------
        GameObject facadesGroup = GetOrCreateChild(rootEnv, "Fachadas_Coloniales");

        // Planta Baja (Calle): Casas asentadas en Y = 0.0 (suelo empedrado)
        // - X = -7.5: Casa colonial de 1 piso amarilla (Inicio)
        CreatePiece(facadesGroup, "Casa_Inicio_Amarilla", new Vector3(-7.5f, 1.63f, 0f), casa1PisoYellow, "Background", -6);
        // - X = -2.0: Muro de piedra de transición
        CreatePiece(facadesGroup, "Muro_Piedra_1", new Vector3(-2.0f, 1.63f, 0f), muroMamposteria, "Background", -6);
        // - X = 0.5: Muro de cantera detrás de la escalera central
        CreatePiece(facadesGroup, "Muro_Piedra_Stairs1", new Vector3(0.5f, 2.0f, 0f), muroMamposteria, "Background", -7);
        // - X = 3.5: Casa colonial de 2 pisos (detrás de la escalera central)
        CreatePiece(facadesGroup, "Casa_Central_2Pisos", new Vector3(3.5f, 2.92f, 0f), casa2PisosOrange, "Background", -8);
        // - X = 9.0: Casa de 1 piso naranja
        CreatePiece(facadesGroup, "Casa_Calle_Naranja", new Vector3(9.0f, 1.25f, 0f), casa1PisoOrange, "Background", -6);
        // - X = 15.0: Casa de 1 piso amarilla
        CreatePiece(facadesGroup, "Casa_Calle_Amarilla", new Vector3(15.0f, 1.63f, 0f), casa1PisoYellow, "Background", -6);
        // - X = 19.5: Muro de cantera detrás de la escalera de bajada
        CreatePiece(facadesGroup, "Muro_Piedra_Stairs2", new Vector3(19.5f, 2.0f, 0f), muroMamposteria, "Background", -7);
        // - X = 21.0: Casa colonial de 2 pisos
        CreatePiece(facadesGroup, "Casa_Segunda_2Pisos", new Vector3(21.0f, 2.92f, 0f), casa2PisosOrange, "Background", -8);
        // - X = 27.5: Casa de 1 piso naranja
        CreatePiece(facadesGroup, "Casa_Calle_Naranja2", new Vector3(27.5f, 1.25f, 0f), casa1PisoOrange, "Background", -6);
        // - X = 33.0: Muro de piedra previo a la meta
        CreatePiece(facadesGroup, "Muro_Piedra_Meta", new Vector3(33.0f, 1.63f, 0f), muroMamposteria, "Background", -6);

        // Fachadas de la Planta Alta (Terraza del Beso, Y = 3.2): Casas en el segundo piso
        CreatePiece(facadesGroup, "Casa_Terraza_1", new Vector3( 9.0f, 4.87f, 0f), casa1PisoYellow, "Background", -6);
        CreatePiece(facadesGroup, "Casa_Terraza_2", new Vector3(15.0f, 4.49f, 0f), casa1PisoOrange, "Background", -6);

        // ----------------------------------------------------
        // 4. ESTRUCTURA DE CALLES, ESCALERAS Y PASARELAS
        // ----------------------------------------------------
        GameObject pathwaysGroup = GetOrCreateChild(rootEnv, "Vias_Plataformas_Jugables");
        GameObject stairsGroup   = GetOrCreateChild(rootEnv, "Escaleras_Cantera");
        GameObject railingsGroup = GetOrCreateChild(rootEnv, "Barandales_Hierro");
        GameObject decoGroup     = GetOrCreateChild(rootEnv, "Decoracion_Callejon");
        GameObject markersGroup  = GetOrCreateChild(rootEnv, "Marcadores_Rutas");
        GameObject boundsRoot    = GetOrCreateChild(rootEnv, "Limites_Solidos");

        // ── PISO 1: CALLE BAJA EMPEDRADA HOMOGÉNEA (X: -11 a 45, Y = 0.0) ──
        BuildEmpedradoStreet(pathwaysGroup, boundsRoot, "Calle_Principal", -11.0f, 56.0f, 0.0f, adoquinesPuros,
            topGaps: new List<(float, float)> { (0.0f, 4.2f), (18.5f, 22.7f) });

        // ── ESCALERA CENTRAL DE CANTERA (Sube de Y = 0.05 a Y = 3.20, X: 0.0 a 4.1) ──
        BuildStaircase(stairsGroup, "Escalera_Beso_Central", 0.0f, 0.05f, 4.1f, 3.20f, true, stairsSprite);

        // ── PISO 2: TERRAZA ELEVADA DEL BESO (X: 3.8 a 19.0, Y = 3.20) ──
        BuildEmpedradoWalkway(pathwaysGroup, railingsGroup, boundsRoot, "Terraza_Beso", 3.8f, 15.2f, 3.20f, adoquinesPuros, muroMamposteria, barandalSprite,
            frontGaps: new List<(float, float)> { (3.6f, 4.3f) });

        // ── ESCALERA DE BAJADA DE LA TERRAZA A LA CALLE (Baja de Y = 3.20 a Y = 0.05, X: 18.8 a 22.9) ──
        BuildStaircase(stairsGroup, "Escalera_Baja_Calle", 18.8f, 3.20f, 22.9f, 0.05f, false, stairsSprite);

        // ── BARRERAS LATERALES DEL NIVEL ──
        CreateSolidBarrier(boundsRoot, "Tope_Izquierdo", new Vector2(-10.5f, 1.5f), new Vector2(0.8f, 8.0f));
        CreateSolidBarrier(boundsRoot, "Tope_Derecho",   new Vector2( 44.5f, 1.5f), new Vector2(0.8f, 8.0f));

        // ----------------------------------------------------
        // 5. META MONUMENTAL: TÚNEL COLONIAL DE GUANAJUATO (X = 38.5)
        // ----------------------------------------------------
        GameObject goalObj = GetOrCreateChild(rootEnv, "Salida_Tunel_Colonial");
        goalObj.transform.position = new Vector3(38.5f, 2.50f, 0f);

        CreatePiece(goalObj, "Boveda_Tunel_Meta", Vector3.zero, tunelSalida, "Default", 2, new Vector3(0.85f, 0.85f, 1f));

        BoxCollider2D goalCol = goalObj.GetComponent<BoxCollider2D>();
        if (goalCol == null) goalCol = goalObj.AddComponent<BoxCollider2D>();
        goalCol.size = new Vector2(2.5f, 3.5f);
        goalCol.offset = new Vector2(0f, -0.8f);
        goalCol.isTrigger = true;
        if (goalObj.GetComponent<LevelGoal>() == null) goalObj.AddComponent<LevelGoal>();

        Level2Lighting.SpawnExitBeaconLight(goalObj, new Vector3(0f, -0.6f, 0f));

        // ----------------------------------------------------
        // 6. POSTES DE SEÑALÉTICA CON FLECHAS (SOLO 3, PROPORCIONADOS)
        // ----------------------------------------------------
        if (letreroFlecha != null)
        {
            // 1. Al inicio de la calle apuntando hacia el callejón (a un lado de la acera)
            CreatePiece(markersGroup, "Senal_Flecha_Inicio", new Vector3(-4.5f, 0.45f, 0f), letreroFlecha, "Props", 14, new Vector3(0.22f, 0.22f, 1f));

            // 2. En la terraza guiando hacia adelante
            CreatePiece(markersGroup, "Senal_Flecha_Terraza", new Vector3(11.0f, 3.65f, 0f), letreroFlecha, "Props", 14, new Vector3(0.22f, 0.22f, 1f));

            // 3. En la calle antes del túnel de meta
            CreatePiece(markersGroup, "Senal_Flecha_Meta", new Vector3(34.0f, 0.45f, 0f), letreroFlecha, "Props", 14, new Vector3(0.22f, 0.22f, 1f));
        }

        // Letrero colonial colgante en la fachada
        if (letreroCallejon != null)
        {
            CreatePiece(decoGroup, "Letrero_Callejon", new Vector3(-1.0f, 2.10f, 0f), letreroCallejon, "Props", 14, new Vector3(0.20f, 0.20f, 1f));
        }

        // ----------------------------------------------------
        // 7. FAROLES CÁLIDOS DE MURO (ADOSADOS A LAS PAREDES, NO EN MEDIO DEL CAMINO)
        // ----------------------------------------------------
        GameObject lightsGroup = GetOrCreateChild(rootEnv, "Faroles_Iluminacion");
        Vector3[] wallLanterns = new Vector3[]
        {
            new Vector3(-5.5f, 1.8f, 0f), // En la pared de la casa amarilla
            new Vector3( 5.5f, 1.8f, 0f), // En la pared al lado de la escalera
            new Vector3(11.0f, 4.8f, 0f), // En la pared del segundo piso
            new Vector3(17.0f, 1.8f, 0f), // En la pared baja
            new Vector3(23.5f, 1.8f, 0f), // En la pared de la casa naranja
            new Vector3(30.0f, 1.8f, 0f), // En la pared de mampostería
            new Vector3(36.0f, 2.0f, 0f)  // En el arco del túnel de meta
        };

        for (int i = 0; i < wallLanterns.Length; i++)
        {
            Level2Lighting.SpawnWallLantern(lightsGroup, $"Farol_Muro_{i + 1}", wallLanterns[i], farolPared);
        }

        // ----------------------------------------------------
        // 8. ESCONDITES DE SIGILO Y OBSTÁCULOS (PEGADOS AL PIE DE PAREDES)
        // ----------------------------------------------------
        GameObject stealthRoot = GetOrCreateChild(rootEnv, "Escondites_Sigilo");
        GameObject obsRoot     = GetOrCreateChild(rootEnv, "Obstaculos_Callejon");

        // Cajas y barriles al pie de las paredes (bien apoyados en el suelo, no sobre barandales)
        CreateCrateProp(stealthRoot, "Huacales_Inicio",      new Vector3(-2.8f, 0.25f, 0f), crateSprite);
        CreateCrateProp(stealthRoot, "Barril_Escalera",     new Vector3( 4.8f, 0.25f, 0f), barrelSprite);
        CreateCrateProp(stealthRoot, "Sacos_Terraza",       new Vector3(13.5f, 3.45f, 0f), sackSprite);
        CreateCrateProp(stealthRoot, "Cajas_Cruce",         new Vector3(25.0f, 0.25f, 0f), crateSprite);
        CreateCrateProp(stealthRoot, "Barril_PreMeta",      new Vector3(32.0f, 0.25f, 0f), barrelSprite);

        // Charcos y botellas en la calle baja
        CreateMudPuddle(obsRoot, "Charco_1",   new Vector3(11.5f, 0.10f, 0f), mudSprite);
        CreateNoiseObstacle(obsRoot, "Botellas_1", new Vector3(16.5f, 0.10f, 0f), botSprite, brokenSprite, glassSound);
        CreateMudPuddle(obsRoot, "Charco_2",   new Vector3(28.5f, 0.10f, 0f), mudSprite);

        // Macetas decorativas en las banquetas
        if (macetaFlores != null)
        {
            CreatePiece(decoGroup, "Maceta_Inicio",  new Vector3(-6.2f, 0.20f, 0f), macetaFlores, "Props", 10, new Vector3(0.25f, 0.25f, 1f));
            CreatePiece(decoGroup, "Maceta_Escalera",new Vector3( 6.0f, 0.20f, 0f), macetaFlores, "Props", 10, new Vector3(0.25f, 0.25f, 1f));
            CreatePiece(decoGroup, "Maceta_Terraza", new Vector3(16.0f, 3.40f, 0f), macetaFlores, "Props", 16, new Vector3(0.25f, 0.25f, 1f));
        }

        // ----------------------------------------------------
        // 9. PATRULLA DE LA LLORONA EN LA CALLE
        // ----------------------------------------------------
        GameObject llorona = GameObject.Find("Llorona");
        if (llorona != null)
        {
            llorona.transform.position = new Vector3(15.0f, 0.40f, 0f);
            llorona.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            SpriteRenderer lloSr = llorona.GetComponent<SpriteRenderer>();
            if (lloSr != null)
            {
                lloSr.sortingLayerName = "Characters";
                lloSr.sortingOrder = 10;
            }

            GameObject waypointsRoot = GetOrCreateChild(rootEnv, "Waypoints_Llorona");
            GameObject wp1 = GetOrCreateChild(waypointsRoot, "WP_Patrulla1");
            wp1.transform.position = new Vector3(8.0f, 0.40f, 0f);

            GameObject wp2 = GetOrCreateChild(waypointsRoot, "WP_Patrulla2");
            wp2.transform.position = new Vector3(20.0f, 0.40f, 0f);

            GameObject wp3 = GetOrCreateChild(waypointsRoot, "WP_Patrulla3");
            wp3.transform.position = new Vector3(31.0f, 0.40f, 0f);

            LloronaAI ai = llorona.GetComponent<LloronaAI>();
            if (ai != null)
            {
                ai.Waypoints = new Transform[] { wp1.transform, wp2.transform, wp3.transform };
                SerializedObject soAI = new SerializedObject(ai);
                var propWp = soAI.FindProperty("waypoints");
                if (propWp != null)
                {
                    propWp.arraySize = 3;
                    propWp.GetArrayElementAtIndex(0).objectReferenceValue = wp1.transform;
                    propWp.GetArrayElementAtIndex(1).objectReferenceValue = wp2.transform;
                    propWp.GetArrayElementAtIndex(2).objectReferenceValue = wp3.transform;
                }
                soAI.ApplyModifiedProperties();
            }
        }
    }

    // ========================================================
    // CONSTRUCTORES DE CALLE Y TERRAZA HOMOGÉNEAS
    // ========================================================

    static void BuildEmpedradoStreet(GameObject parent, GameObject boundsRoot, string name, float startX, float length, float streetY, Sprite[] adoquines,
        List<(float start, float end)> topGaps = null)
    {
        GameObject segObj = GetOrCreateChild(parent, name);

        // Suelo empedrado continuo: 3 hileras de adoquín puro (cero ladrillo ni mármol)
        if (adoquines != null && adoquines.Length > 0)
        {
            int idx = 0;
            for (float x = startX; x < startX + length; x += 1.0f)
            {
                // Hilera Centro (superficie donde camina Carlos)
                CreatePiece(segObj, $"Empedrado_C_{idx}", new Vector3(x, streetY - 0.45f, 0f), adoquines[idx % adoquines.Length], "Background", -3);
                // Hilera Frente (profundidad hacia abajo)
                CreatePiece(segObj, $"Empedrado_F_{idx}", new Vector3(x, streetY - 1.40f, 0f), adoquines[(idx + 1) % adoquines.Length], "Background", -4);
                // Hilera Fondo (relleno total sin vacíos)
                CreatePiece(segObj, $"Empedrado_B_{idx}", new Vector3(x, streetY - 2.30f, 0f), adoquines[(idx + 2) % adoquines.Length], "Background", -5);
                idx++;
            }
        }

        float endX = startX + length;

        // Barrera inferior (borde de la acera al frente)
        CreateSolidBarrier(boundsRoot, $"Bordillo_{name}", new Vector2((startX + endX) * 0.5f, streetY - 0.35f), new Vector2(length, 0.40f));
        // Pared de las casas con huecos en las escaleras
        CreateBarrierWithGaps(boundsRoot, $"Pared_{name}", startX, endX, streetY + 0.85f, 0.40f, topGaps);
    }

    static void BuildEmpedradoWalkway(GameObject parent, GameObject railingsGroup, GameObject boundsRoot, string name, float startX, float length, float walkwayY, Sprite[] adoquines, Sprite muroSoporte, Sprite barandalSprite,
        List<(float start, float end)> frontGaps = null)
    {
        GameObject segObj = GetOrCreateChild(parent, name);

        // 1. Pavimento empedrado de la terraza
        if (adoquines != null && adoquines.Length > 0)
        {
            int idx = 0;
            for (float x = startX; x < startX + length; x += 1.0f)
            {
                CreatePiece(segObj, $"Adoquin_Terraza_{idx}", new Vector3(x, walkwayY - 0.45f, 0f), adoquines[idx % adoquines.Length], "Background", -3);
                idx++;
            }
        }

        // 2. Muro de mampostería de soporte debajo de la terraza (conecta con la calle baja)
        if (muroSoporte != null)
        {
            int sIdx = 0;
            for (float x = startX; x < startX + length; x += 3.0f)
            {
                CreatePiece(segObj, $"Soporte_{sIdx++}", new Vector3(x + 1.5f, walkwayY - 1.60f, 0f), muroSoporte, "Background", -5, new Vector3(0.50f, 0.50f, 1f));
            }
        }

        // 3. Barandal de hierro forjado al frente (Props orden 15: Carlos camina DETRÁS del barandal)
        if (barandalSprite != null)
        {
            int bIdx = 0;
            for (float x = startX; x < startX + length; x += 2.8f)
            {
                bool inGap = frontGaps != null && frontGaps.Any(g => x >= g.start - 0.5f && x <= g.end);
                if (!inGap)
                {
                    float bx = Mathf.Min(x + 1.4f, startX + length - 0.8f);
                    CreatePiece(railingsGroup, $"Barandal_{name}_{bIdx++}", new Vector3(bx, walkwayY + 0.35f, 0f), barandalSprite, "Props", 15, new Vector3(0.35f, 0.35f, 1f));
                }
            }
        }

        float endX = startX + length;

        // Barreras: barandal frontal y pared trasera de las casas
        CreateBarrierWithGaps(boundsRoot, $"BarandalFisico_{name}", startX, endX, walkwayY - 0.15f, 0.35f, frontGaps);
        CreateSolidBarrier(boundsRoot, $"ParedAlta_{name}", new Vector2((startX + endX) * 0.5f, walkwayY + 0.85f), new Vector2(length, 0.40f));
    }

    static void BuildStaircase(GameObject stairsGroup, string name, float startX, float startY, float endX, float endY, bool isAscendingRight, Sprite stairsSprite)
    {
        GameObject stairObj = GetOrCreateChild(stairsGroup, name);
        float centerX = (startX + endX) * 0.5f;
        float centerY = (startY + endY) * 0.5f;
        float width = Mathf.Abs(endX - startX);
        float height = Mathf.Abs(endY - startY);

        // Visual de escalera de cantera colonial (escala 0.40 para cubrir 4.1 u de ancho y 3.2 u de alto)
        CreatePiece(stairObj, "Cantera_Escalones", new Vector3(centerX, centerY, 0f), stairsSprite, "Default", 2,
            isAscendingRight ? new Vector3(0.40f, 0.40f, 1f) : new Vector3(-0.40f, 0.40f, 1f));

        // Trigger Stairs2D con pendiente suave y continua
        GameObject triggerObj = GetOrCreateChild(stairObj, "Trigger_Stairs2D");
        triggerObj.transform.position = new Vector3(centerX, centerY, 0f);

        BoxCollider2D col = triggerObj.GetComponent<BoxCollider2D>();
        if (col == null) col = triggerObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(width + 0.6f, height + 1.0f);
        col.isTrigger = true;

        Stairs2D stairs = triggerObj.GetComponent<Stairs2D>();
        if (stairs == null) stairs = triggerObj.AddComponent<Stairs2D>();
        stairs.bottomY = Mathf.Min(startY, endY);
        stairs.topY    = Mathf.Max(startY, endY);
        stairs.leftX   = Mathf.Min(startX, endX);
        stairs.rightX  = Mathf.Max(startX, endX);
        stairs.isAscendingRight = isAscendingRight;
    }

    static void CreateBarrierWithGaps(GameObject parent, string prefix, float startX, float endX, float y, float thickness, List<(float start, float end)> gaps)
    {
        if (gaps == null || gaps.Count == 0)
        {
            CreateSolidBarrier(parent, prefix, new Vector2((startX + endX) * 0.5f, y), new Vector2(endX - startX, thickness));
            return;
        }

        var sorted = gaps.OrderBy(g => g.start).ToList();
        float currentX = startX;
        int segIdx = 0;

        foreach (var gap in sorted)
        {
            float segStart = Mathf.Max(currentX, gap.start);
            float segLen = segStart - currentX;
            if (segLen > 0.25f)
            {
                CreateSolidBarrier(parent, $"{prefix}_Seg{segIdx++}", new Vector2(currentX + segLen * 0.5f, y), new Vector2(segLen, thickness));
            }
            currentX = Mathf.Max(currentX, gap.end);
        }

        float finalLen = endX - currentX;
        if (finalLen > 0.25f)
        {
            CreateSolidBarrier(parent, $"{prefix}_Seg{segIdx}", new Vector2(currentX + finalLen * 0.5f, y), new Vector2(finalLen, thickness));
        }
    }

    static void CreateSolidBarrier(GameObject parent, string name, Vector2 center, Vector2 size)
    {
        GameObject barrier = GetOrCreateChild(parent, name);
        barrier.transform.position = new Vector3(center.x, center.y, 0f);
        BoxCollider2D col = barrier.GetComponent<BoxCollider2D>();
        if (col == null) col = barrier.AddComponent<BoxCollider2D>();
        col.size = size;
        col.offset = Vector2.zero;
    }

    static void CreateCrateProp(GameObject parent, string name, Vector3 pos, Sprite sprite)
    {
        GameObject obj = CreatePiece(parent, name, pos, sprite, "Props", 12, new Vector3(0.35f, 0.35f, 1f));
        BoxCollider2D col = obj.GetComponent<BoxCollider2D>();
        if (col == null) col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.6f);
        col.isTrigger = true;
        if (obj.GetComponent<HideSpot>() == null) obj.AddComponent<HideSpot>();
    }

    static void CreateSolidBackdrop(GameObject parent, string name, Vector3 pos, Vector3 scale)
    {
        GameObject bgObj = GetOrCreateChild(parent, name);
        bgObj.transform.position = pos;
        bgObj.transform.localScale = scale;

        SpriteRenderer sr = bgObj.GetComponent<SpriteRenderer>();
        if (sr == null) sr = bgObj.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(2, 2);
        Color c = new Color(0.04f, 0.04f, 0.08f, 1.0f);
        tex.SetPixels(new Color[] { c, c, c, c });
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f);
        sr.sortingLayerName = "Background";
        sr.sortingOrder = -25;
    }

    static GameObject CreatePiece(GameObject parent, string name, Vector3 pos, Sprite sprite, string sortingLayer, int sortingOrder, Vector3? scale = null)
    {
        GameObject obj = GetOrCreateChild(parent, name);
        if (parent == null || parent.transform.position == Vector3.zero)
            obj.transform.position = pos;
        else
            obj.transform.localPosition = pos;

        if (scale.HasValue) obj.transform.localScale = scale.Value;
        else obj.transform.localScale = Vector3.one;

        if (sprite != null)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr == null) sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = sortingOrder;
        }
        return obj;
    }

    static void CreateMudPuddle(GameObject parent, string name, Vector3 pos, Sprite sprite)
    {
        GameObject obj = CreatePiece(parent, name, pos, sprite, "Props", 5, new Vector3(0.50f, 0.50f, 1f));
        BoxCollider2D col = obj.GetComponent<BoxCollider2D>();
        if (col == null) col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.0f, 0.4f);
        col.isTrigger = true;
        if (obj.GetComponent<MudPuddle>() == null) obj.AddComponent<MudPuddle>();
    }

    static void CreateNoiseObstacle(GameObject parent, string name, Vector3 pos, Sprite spriteNormal, Sprite spriteBroken, AudioClip sfx)
    {
        GameObject obj = CreatePiece(parent, name, pos, spriteNormal, "Props", 5, new Vector3(0.50f, 0.50f, 1f));
        BoxCollider2D col = obj.GetComponent<BoxCollider2D>();
        if (col == null) col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.4f);
        col.isTrigger = true;

        NoiseObstacle obs = obj.GetComponent<NoiseObstacle>();
        if (obs == null) obs = obj.AddComponent<NoiseObstacle>();
        if (sfx != null)
        {
            SerializedObject so = new SerializedObject(obs);
            var propSfx = so.FindProperty("breakSound");
            if (propSfx != null) propSfx.objectReferenceValue = sfx;
            so.ApplyModifiedProperties();
        }
    }

    static GameObject GetOrCreateChild(GameObject parent, string name)
    {
        Transform child = parent.transform.Find(name);
        if (child != null) return child.gameObject;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go;
    }
}
#endif
