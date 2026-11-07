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
        Sprite casaTerracota    = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Casa_Colonial_2.png");         // Casa terracota colonial
        Sprite casaOcre         = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Casa_Colonial_3.png");         // Casa ocre colonial alta
        Sprite muroMamposteria  = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Fachada_Naranja.png");         // Muro de piedra/mampostería para contención
        Sprite muroPiedraLiso   = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Muro_Piedra_Bajo.png");

        // Salida: Túnel Colonial de cantera con arco profundo
        Sprite tunelSalida      = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Fachada_Alta_Este.png");

        // Escondites de Sigilo: Portones y Puertas Coloniales (Entradas a zaguanes)
        Sprite portonColonial   = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Porton_Colonial.png");
        Sprite puertaColonial   = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Puertas_nvl2.png").OfType<Sprite>().FirstOrDefault();

        // Accesorios de Hierro y Decoración
        Sprite barandalSprite   = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Balcon_Hierro_Barandal.png");
        Sprite farolPared       = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Farol_Pared_Muro.png");
        Sprite letreroFlecha    = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Poste_Senaletica_1.png");
        Sprite letreroCallejon  = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Poste_Senaletica_Callejon.png");
        Sprite macetaFlores     = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Maceta_Colgante.png");
        Sprite jardineraVides   = AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Jardinera_Balcon.png");

        // Suelo: ÚNICAMENTE los adoquines empedrados oscuros homogéneos (00 y 08)
        // 100% idénticos en tono y textura (Guanajuato nocturno puro, cero mosaicos)
        Sprite[] adoquinesPuros = new[]
        {
            AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Tile_Adoquin_00.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(Nvl2Dir + "Tile_Adoquin_08.png")
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
        // 2. FONDO Y TELÓN NOCTURNO EXPANDIDO
        // ----------------------------------------------------
        GameObject bgGroup = GetOrCreateChild(rootEnv, "Fondo_Guanajuato_Nocturno");
        CreateSolidBackdrop(bgGroup, "Telon_Cielo_Nocturno", new Vector3(25.0f, 6.0f, 0f), new Vector3(120f, 28f, 1f));

        // ----------------------------------------------------
        // 3. FACHADAS CERCANAS JUGABLES (4 NIVELES DE ALTURA)
        // ----------------------------------------------------
        GameObject facadesGroup = GetOrCreateChild(rootEnv, "Fachadas_Coloniales");

        // ── Fachadas Planta Baja (Tier 1: Calle Honda, Y = 0.0) ──
        CreatePiece(facadesGroup, "Casa_Inicio_Amarilla", new Vector3(-7.5f, 1.63f, 0f), casa1PisoYellow, "Background", -6);
        CreatePiece(facadesGroup, "Muro_Piedra_1",        new Vector3(-2.0f, 1.63f, 0f), muroMamposteria, "Background", -6);
        CreatePiece(facadesGroup, "Muro_Piedra_Stairs1",  new Vector3( 0.5f, 2.00f, 0f), muroMamposteria, "Background", -7);
        CreatePiece(facadesGroup, "Casa_Central_2Pisos",  new Vector3( 3.5f, 2.92f, 0f), casa2PisosOrange, "Background", -8);
        CreatePiece(facadesGroup, "Casa_Calle_Naranja",   new Vector3( 8.0f, 1.25f, 0f), casa1PisoOrange, "Background", -6);
        CreatePiece(facadesGroup, "Muro_Piedra_Stairs1B", new Vector3(18.5f, 2.00f, 0f), muroMamposteria, "Background", -7);
        CreatePiece(facadesGroup, "Casa_Calle_Amarilla",  new Vector3(25.5f, 1.63f, 0f), casa1PisoYellow, "Background", -6);
        CreatePiece(facadesGroup, "Casa_Segunda_2Pisos",  new Vector3(33.0f, 2.92f, 0f), casa2PisosOrange, "Background", -8);
        CreatePiece(facadesGroup, "Casa_Calle_Naranja2",  new Vector3(41.0f, 1.25f, 0f), casa1PisoOrange, "Background", -6);
        CreatePiece(facadesGroup, "Muro_Piedra_Stairs4C", new Vector3(51.0f, 2.00f, 0f), muroMamposteria, "Background", -7);
        CreatePiece(facadesGroup, "Muro_Piedra_PreMeta",  new Vector3(54.0f, 1.63f, 0f), muroMamposteria, "Background", -6);

        // ── Telón de Mampostería de Retención (Evita huecos negros entre casas) ──
        for (float mx = 4.0f; mx <= 52.0f; mx += 6.0f)
        {
            CreatePiece(facadesGroup, $"Muro_Fondo_Tier2_{mx:F0}", new Vector3(mx, 4.88f, 0f), muroMamposteria, "Background", -9);
        }
        for (float mx = 14.0f; mx <= 48.0f; mx += 6.0f)
        {
            CreatePiece(facadesGroup, $"Muro_Fondo_Tier3_{mx:F0}", new Vector3(mx, 8.13f, 0f), muroMamposteria, "Background", -9);
        }

        // ── Fachadas Tier 2 (Terraza y Balcón Continuo, Y = 3.25) ──
        CreatePiece(facadesGroup, "Casa_Terraza_1",        new Vector3( 7.5f, 4.88f, 0f), casa1PisoYellow, "Background", -6);
        CreatePiece(facadesGroup, "Muro_Piedra_Stairs2",   new Vector3(11.5f, 5.25f, 0f), muroMamposteria, "Background", -7);
        CreatePiece(facadesGroup, "Casa_Terraza_2",        new Vector3(16.5f, 4.88f, 0f), casa1PisoOrange, "Background", -6);
        CreatePiece(facadesGroup, "Casa_Terraza_Mid",      new Vector3(21.0f, 4.88f, 0f), casaTerracota ?? casa1PisoYellow, "Background", -6);
        CreatePiece(facadesGroup, "Casa_Terraza_3",        new Vector3(26.0f, 4.88f, 0f), casa1PisoYellow, "Background", -6);
        CreatePiece(facadesGroup, "Casa_Terraza_4",        new Vector3(33.0f, 4.88f, 0f), casa1PisoOrange, "Background", -6);
        CreatePiece(facadesGroup, "Casa_Terraza_5",        new Vector3(41.0f, 4.88f, 0f), casaTerracota ?? casa1PisoYellow, "Background", -6);
        CreatePiece(facadesGroup, "Casa_Terraza_Este",     new Vector3(49.5f, 4.88f, 0f), casa1PisoOrange, "Background", -6);

        // ── Fachadas Tier 3 (Pasarela Mirador Continua, Y = 6.50) ──
        CreatePiece(facadesGroup, "Casa_Tier3_1",          new Vector3(18.0f, 8.13f, 0f), casa1PisoYellow, "Background", -6);
        CreatePiece(facadesGroup, "Muro_Piedra_Stairs3",   new Vector3(21.0f, 8.50f, 0f), muroMamposteria, "Background", -7);
        CreatePiece(facadesGroup, "Casa_Tier3_2",          new Vector3(28.5f, 7.75f, 0f), casa1PisoOrange, "Background", -6);
        CreatePiece(facadesGroup, "Casa_Tier3_3",          new Vector3(36.0f, 8.13f, 0f), casa1PisoYellow, "Background", -6);
        CreatePiece(facadesGroup, "Casa_Tier3_Este",       new Vector3(43.5f, 8.13f, 0f), casa1PisoOrange, "Background", -6);

        // ── Fachadas Tier 4 (Azoteas Coloniales, Y = 9.75) ──
        CreatePiece(facadesGroup, "Casa_Tier4_1",          new Vector3(27.0f, 11.38f, 0f), casa1PisoYellow, "Background", -6);
        CreatePiece(facadesGroup, "Casa_Tier4_2",          new Vector3(34.0f, 11.00f, 0f), casa1PisoOrange, "Background", -6);
        CreatePiece(facadesGroup, "Muro_Piedra_Stairs4A",  new Vector3(39.0f, 11.50f, 0f), muroMamposteria, "Background", -7);

        // ----------------------------------------------------
        // 4. ESTRUCTURA DE CALLES, ESCALERAS Y PASARELAS (4 PISOS CONECTADOS Y RUTAS ALTERNATIVAS)
        // ----------------------------------------------------
        GameObject pathwaysGroup = GetOrCreateChild(rootEnv, "Vias_Plataformas_Jugables");
        GameObject stairsGroup   = GetOrCreateChild(rootEnv, "Escaleras_Cantera");
        GameObject railingsGroup = GetOrCreateChild(rootEnv, "Barandales_Hierro");
        GameObject decoGroup     = GetOrCreateChild(rootEnv, "Decoracion_Callejon");
        GameObject markersGroup  = GetOrCreateChild(rootEnv, "Marcadores_Rutas");
        GameObject boundsRoot    = GetOrCreateChild(rootEnv, "Limites_Solidos");

        // ── TIER 1: CALLE HONDA EMPEDRADA (X: -12 a 62, Y = 0.0) ──
        BuildEmpedradoStreet(pathwaysGroup, boundsRoot, "Calle_Principal", -12.0f, 74.0f, 0.0f, adoquinesPuros,
            topGaps: new List<(float, float)> { (0.0f, 4.2f), (18.0f, 22.2f), (51.0f, 54.8f) });

        // ── ESCALERA 1: CALLE A TIER 2 (Sube de Y = 0.05 a Y = 3.25, X: 0.0 a 4.1) ──
        BuildStaircase(stairsGroup, "Escalera_Calle_a_Tier2", 0.0f, 0.05f, 4.1f, 3.25f, true, stairsSprite);

        // ── TIER 2: TERRAZA DEL BESO Y BALCÓN CONTINUO COMPLETO (X: 4.1 a 51.5, Y = 3.25) ──
        // Permite caminar libremente por todo el piso 2 sin obligar a subir ni a bajar
        BuildEmpedradoWalkway(pathwaysGroup, railingsGroup, boundsRoot, "Terraza_Tier2", 4.1f, 47.4f, 3.25f, adoquinesPuros, muroMamposteria, barandalSprite,
            backGaps: new List<(float, float)> { (10.5f, 14.8f) },
            frontGaps: new List<(float, float)> { (18.0f, 22.2f) });

        // ── ESCALERA 1B: BAJADA DE TIER 2 A CALLE HONDA (Baja de Y = 3.25 a Y = 0.05, X: 18.0 a 22.1) ──
        BuildStaircase(stairsGroup, "Escalera_Tier2_a_Calle", 18.0f, 3.25f, 22.1f, 0.05f, false, stairsSprite, floorContinuesAtTop: true);

        // ── ESCALERA 2: SUBIDA DE TIER 2 A TIER 3 (Sube de Y = 3.30 a Y = 6.50, X: 10.5 a 14.6) ──
        BuildStaircase(stairsGroup, "Escalera_Tier2_a_Tier3", 10.5f, 3.30f, 14.6f, 6.50f, true, stairsSprite);

        // ── TIER 3: PASARELA MIRADOR Y BALCÓN CONTINUO COMPLETO (X: 14.6 a 45.0, Y = 6.50) ──
        // Si el jugador no quiere subir por la 3ra escalera, puede seguir caminando derecho por este balcón
        BuildEmpedradoWalkway(pathwaysGroup, railingsGroup, boundsRoot, "Pasarela_Tier3", 14.6f, 30.4f, 6.50f, adoquinesPuros, muroMamposteria, barandalSprite,
            backGaps: new List<(float, float)> { (20.0f, 24.3f) });

        // ── ESCALERA 3: SUBIDA DE TIER 3 A TIER 4 (Sube de Y = 6.55 a Y = 9.75, X: 20.0 a 24.1) ──
        BuildStaircase(stairsGroup, "Escalera_Tier3_a_Tier4", 20.0f, 6.55f, 24.1f, 9.75f, true, stairsSprite);

        // ── TIER 4: AZOTEAS COLONIALES / MIRADOR DE LAS CAMPANAS (X: 24.1 a 38.0, Y = 9.75) ──
        BuildEmpedradoWalkway(pathwaysGroup, railingsGroup, boundsRoot, "Azoteas_Tier4", 24.1f, 13.9f, 9.75f, adoquinesPuros, muroMamposteria, barandalSprite);

        // ── ESCALERA 4A: BAJADA DE TIER 4 A TIER 3 ESTE (Baja de Y = 9.75 a Y = 6.50, X: 38.0 a 42.1) ──
        BuildStaircase(stairsGroup, "Escalera_Tier4_a_Tier3Este", 38.0f, 9.75f, 42.1f, 6.50f, false, stairsSprite);

        // ── ESCALERA 4B: BAJADA DE TIER 3 ESTE A TIER 2 ESTE (Baja de Y = 6.50 a Y = 3.25, X: 45.0 a 49.1) ──
        BuildStaircase(stairsGroup, "Escalera_Tier3Este_a_Tier2Este", 45.0f, 6.50f, 49.1f, 3.25f, false, stairsSprite);

        // ── ESCALERA 4C: BAJADA FINAL A LA ENTRADA DEL TÚNEL (Baja de Y = 3.25 a Y = 0.05, X: 51.5 a 54.5) ──
        BuildStaircase(stairsGroup, "Escalera_Tier2Este_a_CalleMeta", 51.5f, 3.25f, 54.5f, 0.05f, false, stairsSprite);

        // ── BARRERAS LATERALES DEL NIVEL ──
        CreateSolidBarrier(boundsRoot, "Tope_Izquierdo", new Vector2(-22.0f, 6.0f), new Vector2(0.8f, 16.0f));
        CreateSolidBarrier(boundsRoot, "Tope_Derecho",   new Vector2( 62.0f, 6.0f), new Vector2(0.8f, 16.0f));

        // ----------------------------------------------------
        // 5. META MONUMENTAL: TÚNEL COLONIAL DE GUANAJUATO (X = 57.0)
        // ----------------------------------------------------
        GameObject goalObj = GetOrCreateChild(rootEnv, "Salida_Tunel_Colonial");
        goalObj.transform.position = new Vector3(57.0f, 2.50f, 0f);

        CreatePiece(goalObj, "Boveda_Tunel_Meta", Vector3.zero, tunelSalida, "Default", 2, new Vector3(0.85f, 0.85f, 1f));

        BoxCollider2D goalCol = goalObj.GetComponent<BoxCollider2D>();
        if (goalCol == null) goalCol = goalObj.AddComponent<BoxCollider2D>();
        goalCol.size = new Vector2(2.5f, 3.5f);
        goalCol.offset = new Vector2(0f, -0.8f);
        goalCol.isTrigger = true;
        if (goalObj.GetComponent<LevelGoal>() == null) goalObj.AddComponent<LevelGoal>();

        Level2Lighting.SpawnExitBeaconLight(goalObj, new Vector3(0f, -0.6f, 0f));

        // ----------------------------------------------------
        // 6. POSTES DE SEÑALÉTICA CON FLECHAS
        // ----------------------------------------------------
        if (letreroFlecha != null)
        {
            // 1. Al inicio de la calle
            CreatePiece(markersGroup, "Senal_Flecha_Inicio", new Vector3(-4.5f, 0.45f, 0f), letreroFlecha, "Props", 14, new Vector3(0.22f, 0.22f, 1f));
            // 2. En Tier 2
            CreatePiece(markersGroup, "Senal_Flecha_Tier2",  new Vector3( 9.0f, 3.70f, 0f), letreroFlecha, "Props", 14, new Vector3(0.22f, 0.22f, 1f));
            // 3. En Tier 3
            CreatePiece(markersGroup, "Senal_Flecha_Tier3",  new Vector3(18.0f, 6.95f, 0f), letreroFlecha, "Props", 14, new Vector3(0.22f, 0.22f, 1f));
            // 4. En Tier 4 (Ruta Aérea)
            CreatePiece(markersGroup, "Senal_Flecha_Tier4",  new Vector3(26.5f, 10.20f, 0f), letreroFlecha, "Props", 14, new Vector3(0.22f, 0.22f, 1f));
            // 5. En la calle antes del túnel de meta
            CreatePiece(markersGroup, "Senal_Flecha_Meta",   new Vector3(53.0f, 0.45f, 0f), letreroFlecha, "Props", 14, new Vector3(0.22f, 0.22f, 1f));
        }

        // Letrero colonial colgante en la fachada
        if (letreroCallejon != null)
        {
            CreatePiece(decoGroup, "Letrero_Callejon", new Vector3(-1.0f, 2.10f, 0f), letreroCallejon, "Props", 14, new Vector3(0.20f, 0.20f, 1f));
        }

        // ----------------------------------------------------
        // 7. FAROLES CÁLIDOS DE MURO (DISTRIBUIDOS EN LOS 4 PISOS)
        // ----------------------------------------------------
        GameObject lightsGroup = GetOrCreateChild(rootEnv, "Faroles_Iluminacion");
        Vector3[] wallLanterns = new Vector3[]
        {
            new Vector3(-5.5f, 1.8f, 0f),  // Tier 1: Inicio
            new Vector3( 6.0f, 1.8f, 0f),  // Tier 1: Bajo la escalera 1
            new Vector3(24.0f, 1.8f, 0f),  // Tier 1: Calle media
            new Vector3(35.0f, 1.8f, 0f),  // Tier 1: Zona de Llorona
            new Vector3(46.0f, 1.8f, 0f),  // Tier 1: Pre-meta
            new Vector3(55.5f, 2.0f, 0f),  // Tier 1: Fachada túnel meta
            new Vector3( 8.5f, 5.0f, 0f),  // Tier 2: Pared casa terraza
            new Vector3(16.0f, 5.0f, 0f),  // Tier 2: Antes de subir a Tier 3
            new Vector3(21.0f, 5.0f, 0f),  // Tier 2: Frente a la bajada de escalera 1B
            new Vector3(28.0f, 5.0f, 0f),  // Tier 2: Balcón medio
            new Vector3(36.0f, 5.0f, 0f),  // Tier 2: Balcón centro-este
            new Vector3(44.0f, 5.0f, 0f),  // Tier 2: Balcón este
            new Vector3(17.5f, 8.2f, 0f),  // Tier 3: Pasarela Mirador
            new Vector3(25.0f, 8.2f, 0f),  // Tier 3: Pasarela Mirador zona espectro
            new Vector3(32.0f, 8.2f, 0f),  // Tier 3: Balcón medio
            new Vector3(39.0f, 8.2f, 0f),  // Tier 3: Balcón este
            new Vector3(26.5f, 11.4f, 0f), // Tier 4: Azoteas altas
            new Vector3(35.5f, 11.4f, 0f)  // Tier 4: Antes de la escalera de bajada
        };

        for (int i = 0; i < wallLanterns.Length; i++)
        {
            Level2Lighting.SpawnWallLantern(lightsGroup, $"Farol_Muro_{i + 1}", wallLanterns[i], farolPared);
        }

        // ----------------------------------------------------
        // 8. ESCONDITES DE SIGILO (PORTONES Y ZAGUANES COLONIALES - [E] PARA OCULTARSE)
        // ----------------------------------------------------
        GameObject stealthRoot = GetOrCreateChild(rootEnv, "Escondites_Sigilo");

        // Portones coloniales en las paredes donde Carlos se oculta en la penumbra (CERO tropiezos)
        CreateDoorHideSpot(stealthRoot, "Zaguan_Escondite_Calle1", new Vector3( 5.0f, 1.05f, 0f), portonColonial, new Vector3(0.38f, 0.38f, 1f));
        CreateDoorHideSpot(stealthRoot, "Zaguan_Escondite_Calle2", new Vector3(32.0f, 1.05f, 0f), portonColonial, new Vector3(0.38f, 0.38f, 1f));
        CreateDoorHideSpot(stealthRoot, "Puerta_Escondite_Tier2",  new Vector3( 9.0f, 4.35f, 0f), puertaColonial ?? portonColonial, new Vector3(0.35f, 0.35f, 1f));
        CreateDoorHideSpot(stealthRoot, "Zaguan_Escondite_Tier3",  new Vector3(21.5f, 7.55f, 0f), portonColonial, new Vector3(0.38f, 0.38f, 1f));
        CreateDoorHideSpot(stealthRoot, "Puerta_Escondite_Tier4",  new Vector3(34.5f, 10.85f, 0f), puertaColonial ?? portonColonial, new Vector3(0.35f, 0.35f, 1f));

        // ----------------------------------------------------
        // 9. OBSTÁCULOS DE CARRERA (CAJAS, HUACALES, BARRIL - SALTAR CON [ESPACIO])
        // ----------------------------------------------------
        GameObject obsRoot = GetOrCreateChild(rootEnv, "Obstaculos_Carrera");

        // 1. Calle Baja (Piso 1):
        CreateCrateProp(obsRoot, "Cajas_Calle1",     new Vector3(24.5f, 0.25f, 0f), crateSprite);
        CreateCrateProp(obsRoot, "Barril_Calle2",    new Vector3(38.0f, 0.25f, 0f), barrelSprite);
        CreateCrateProp(obsRoot, "Huacales_PreMeta", new Vector3(47.0f, 0.25f, 0f), crateSprite);

        // 2. Pisos Superiores (Pisos 2, 3 y 4):
        CreateCrateProp(obsRoot, "Sacos_Tier2",      new Vector3( 7.0f, 3.50f, 0f), sackSprite);
        CreateCrateProp(obsRoot, "Huacales_Tier3",   new Vector3(27.0f, 6.75f, 0f), crateSprite);
        CreateCrateProp(obsRoot, "Sacos_Tier4",      new Vector3(31.0f, 10.00f, 0f), sackSprite);

        // Macetas decorativas con flores (colocadas en fachadas y esquinas, NUNCA en el pasillo de Carlos)
        if (macetaFlores != null)
        {
            CreatePiece(decoGroup, "Maceta_Inicio", new Vector3(-8.2f, 0.25f, 0f), macetaFlores, "Background", -2, new Vector3(0.22f, 0.22f, 1f));
            CreatePiece(decoGroup, "Maceta_Fachada_Tier2", new Vector3( 7.5f, 4.40f, 0f), macetaFlores, "Background", -4, new Vector3(0.22f, 0.22f, 1f));
            CreatePiece(decoGroup, "Maceta_Fachada_Tier3", new Vector3(18.0f, 7.60f, 0f), macetaFlores, "Background", -4, new Vector3(0.22f, 0.22f, 1f));
            CreatePiece(decoGroup, "Maceta_Fachada_Tier4", new Vector3(27.0f, 10.85f, 0f), macetaFlores, "Background", -4, new Vector3(0.22f, 0.22f, 1f));
        }

        // ----------------------------------------------------
        // 9. PATRULLA DE LA LLORONA EN LA CALLE HONDA (5 WAYPOINTS)
        // ----------------------------------------------------
        GameObject llorona = GameObject.Find("Llorona");
        if (llorona != null)
        {
            // La Llorona inicia detrás de Carlos (Carlos está en X = -6.0) para la persecución continua
            llorona.transform.position = new Vector3(-12.5f, 0.40f, 0f);
            llorona.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

            var lloCol = llorona.GetComponent<Collider2D>();
            if (lloCol != null) lloCol.isTrigger = true;
            var lloRb = llorona.GetComponent<Rigidbody2D>();
            if (lloRb != null) lloRb.bodyType = RigidbodyType2D.Kinematic;

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
            wp2.transform.position = new Vector3(19.0f, 0.40f, 0f);

            GameObject wp3 = GetOrCreateChild(waypointsRoot, "WP_Patrulla3");
            wp3.transform.position = new Vector3(30.0f, 0.40f, 0f);

            GameObject wp4 = GetOrCreateChild(waypointsRoot, "WP_Patrulla4");
            wp4.transform.position = new Vector3(41.0f, 0.40f, 0f);

            GameObject wp5 = GetOrCreateChild(waypointsRoot, "WP_Patrulla5");
            wp5.transform.position = new Vector3(49.0f, 0.40f, 0f);

            LloronaAI ai = llorona.GetComponent<LloronaAI>();
            if (ai != null)
            {
                ai.Waypoints = new Transform[] { wp1.transform, wp2.transform, wp3.transform, wp4.transform, wp5.transform };
                SerializedObject soAI = new SerializedObject(ai);
                var propWp = soAI.FindProperty("waypoints");
                if (propWp != null)
                {
                    propWp.arraySize = 5;
                    propWp.GetArrayElementAtIndex(0).objectReferenceValue = wp1.transform;
                    propWp.GetArrayElementAtIndex(1).objectReferenceValue = wp2.transform;
                    propWp.GetArrayElementAtIndex(2).objectReferenceValue = wp3.transform;
                    propWp.GetArrayElementAtIndex(3).objectReferenceValue = wp4.transform;
                    propWp.GetArrayElementAtIndex(4).objectReferenceValue = wp5.transform;
                }
                soAI.ApplyModifiedProperties();
            }
        }

        // ----------------------------------------------------
        // 10. ESPECTRO MENOR DEL PISO 3 (PASARELA MIRADOR)
        // ----------------------------------------------------
        SpawnBalconySpecter(rootEnv);
    }

    static void SpawnBalconySpecter(GameObject rootEnv)
    {
        GameObject specterGroup = GetOrCreateChild(rootEnv, "Amenazas_Balcon");
        GameObject specterObj = GetOrCreateChild(specterGroup, "Espectro_Piso3");
        specterObj.transform.position = new Vector3(26.0f, 6.95f, 0f);
        specterObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

        // Sprite espectral translúcido
        Sprite specterSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Llorona/llorona_movimiento.png")
            .OfType<Sprite>().FirstOrDefault(s => s.name.Contains("0"))
            ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Llorona/llorona.png");

        SpriteRenderer sr = specterObj.GetComponent<SpriteRenderer>();
        if (sr == null) sr = specterObj.AddComponent<SpriteRenderer>();
        sr.sprite = specterSprite;
        sr.color = new Color(0.35f, 0.85f, 1.0f, 0.72f);
        sr.sortingLayerName = "Characters";
        sr.sortingOrder = 11;

        // Luz tenue espectral 2D
        GameObject lightChild = GetOrCreateChild(specterObj, "Luz_Espectral");
        lightChild.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        Light2D l2d = lightChild.GetComponent<Light2D>();
        if (l2d == null) l2d = lightChild.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.25f, 0.65f, 1.0f, 1.0f);
        l2d.pointLightOuterRadius = 3.2f;
        l2d.pointLightInnerRadius = 0.4f;
        l2d.intensity = 0.85f;

        // Collider trigger amplio para detección y obstáculos (abarca desde el espectro hasta el suelo)
        BoxCollider2D specterCol = specterObj.GetComponent<BoxCollider2D>();
        if (specterCol == null) specterCol = specterObj.AddComponent<BoxCollider2D>();
        specterCol.size = new Vector2(3.5f, 4.5f);
        specterCol.offset = new Vector2(0f, -1.0f);
        specterCol.isTrigger = true;

        // Componente SpecterAI
        SpecterAI ai = specterObj.GetComponent<SpecterAI>();
        if (ai == null) ai = specterObj.AddComponent<SpecterAI>();

        AudioClip scream = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_llorona_grito_detect.wav")
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/llorona_grito.wav");
        AudioClip ambient = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_llorona_lamento_loop.wav");

        ai.ConfigurePatrol(18.0f, 40.0f, 1.35f, scream, ambient);

        // Obstáculo vertical para el modo Runner: si pasas por su piso sin subir a las azoteas, suma 1 golpe
        if (specterObj.GetComponent<RunnerObstacle>() == null)
        {
            var ro = specterObj.AddComponent<RunnerObstacle>();
            SerializedObject soRO = new SerializedObject(ro);
            var propJump = soRO.FindProperty("canBeJumpedOver");
            if (propJump != null) propJump.boolValue = false; // No se puede brincar a pie: obliga a subir a las azoteas
            soRO.ApplyModifiedProperties();
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
        List<(float start, float end)> backGaps = null, List<(float start, float end)> frontGaps = null)
    {
        GameObject segObj = GetOrCreateChild(parent, name);

        // 1. Pavimento empedrado de la terraza (superficie caminable) y cornisa de soporte inferior
        if (adoquines != null && adoquines.Length > 0)
        {
            int idx = 0;
            for (float x = startX; x < startX + length; x += 1.0f)
            {
                // Pavimento donde pisa Carlos
                CreatePiece(segObj, $"Adoquin_{name}_{idx}", new Vector3(x, walkwayY - 0.45f, 0f), adoquines[idx % adoquines.Length], "Background", -3);
                // Viga / cornisa continua de piedra de contención
                CreatePiece(segObj, $"Cornisa_{name}_{idx}", new Vector3(x, walkwayY - 1.40f, 0f), adoquines[(idx + 1) % adoquines.Length], "Background", -4);
                idx++;
            }
        }

        // 2. Barandal de hierro forjado al frente (Props orden 15: Carlos camina DETRÁS del barandal)
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

        // Barreras: barandal frontal (en el borde inferior para evitar caer sin obstruir el paso) y pared trasera
        if (frontGaps != null && frontGaps.Count > 0)
            CreateBarrierWithGaps(boundsRoot, $"BarandalFisico_{name}", startX, endX, walkwayY - 0.55f, 0.25f, frontGaps);
        else
            CreateSolidBarrier(boundsRoot, $"BarandalFisico_{name}", new Vector2((startX + endX) * 0.5f, walkwayY - 0.55f), new Vector2(length, 0.25f));

        if (backGaps != null && backGaps.Count > 0)
            CreateBarrierWithGaps(boundsRoot, $"ParedAlta_{name}", startX, endX, walkwayY + 0.75f, 0.35f, backGaps);
        else
            CreateSolidBarrier(boundsRoot, $"ParedAlta_{name}", new Vector2((startX + endX) * 0.5f, walkwayY + 0.75f), new Vector2(length, 0.35f));
    }

    static void BuildStaircase(GameObject stairsGroup, string name, float startX, float startY, float endX, float endY, bool isAscendingRight, Sprite stairsSprite, bool floorContinuesAtTop = false)
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
        col.size = new Vector2(width, height + 1.0f);
        col.isTrigger = true;

        Stairs2D stairs = triggerObj.GetComponent<Stairs2D>();
        if (stairs == null) stairs = triggerObj.AddComponent<Stairs2D>();
        stairs.bottomY = Mathf.Min(startY, endY);
        stairs.topY    = Mathf.Max(startY, endY);
        stairs.leftX   = Mathf.Min(startX, endX);
        stairs.rightX  = Mathf.Max(startX, endX);
        stairs.isAscendingRight = isAscendingRight;
        stairs.floorContinuesAtTop = floorContinuesAtTop;
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
        // Se renderiza en Characters (orden 9) para que destaque sobre el suelo
        GameObject obj = CreatePiece(parent, name, pos, sprite, "Characters", 9, new Vector3(0.35f, 0.35f, 1f));
        BoxCollider2D col = obj.GetComponent<BoxCollider2D>();
        if (col == null) col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.6f);
        col.isTrigger = true;

        // Limpiar cualquier HideSpot previo para que sea obstáculo puro de salto con [Espacio]
        HideSpot hs = obj.GetComponent<HideSpot>();
        if (hs != null) UnityEngine.Object.DestroyImmediate(hs);

        if (obj.GetComponent<RunnerObstacle>() == null) obj.AddComponent<RunnerObstacle>();
    }

    static void CreateDoorHideSpot(GameObject parent, string name, Vector3 pos, Sprite sprite, Vector3 scale)
    {
        // Se renderiza en Props (orden 4), montado en la fachada de fondo
        GameObject obj = CreatePiece(parent, name, pos, sprite, "Props", 4, scale);
        BoxCollider2D col = obj.GetComponent<BoxCollider2D>();
        if (col == null) col = obj.AddComponent<BoxCollider2D>();

        // Área de interacción generosa: cubre de lado a lado y se extiende ampliamente hacia el suelo
        // Para que Carlos al correr NUNCA pase por debajo y siempre detecte el trigger
        col.size = new Vector2(7.0f, 7.5f);
        col.offset = new Vector2(0f, -1.2f);
        col.isTrigger = true;

        // Punto de anclaje a nivel de suelo para ocultar a Carlos dentro de la penumbra
        GameObject hidePt = GetOrCreateChild(obj, "HidePoint");
        hidePt.transform.localPosition = new Vector3(0f, -2.4f, 0f);

        HideSpot hs = obj.GetComponent<HideSpot>();
        if (hs == null) hs = obj.AddComponent<HideSpot>();
        hs.spotType = HideSpot.SpotType.Porton;
        hs.canExitAnytime = true;

        SerializedObject soHs = new SerializedObject(hs);
        var hpProp = soHs.FindProperty("hidePoint");
        if (hpProp != null)
        {
            hpProp.objectReferenceValue = hidePt.transform;
            soHs.ApplyModifiedProperties();
        }

        // Limpiar cualquier RunnerObstacle para que NUNCA haga tropezar a Carlos
        RunnerObstacle ro = obj.GetComponent<RunnerObstacle>();
        if (ro != null) UnityEngine.Object.DestroyImmediate(ro);
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
        if (obj.GetComponent<RunnerObstacle>() == null) obj.AddComponent<RunnerObstacle>();
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
        if (obj.GetComponent<RunnerObstacle>() == null) obj.AddComponent<RunnerObstacle>();
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
