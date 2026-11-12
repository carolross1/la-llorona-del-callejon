// Assets/Editor/Level3Builder.cs
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Constructor oficial y automatizado del Nivel 3: El Atrio Monumental de Dolores.
/// Reconstrucción de gran escala con distribución limpia y cero empalmes:
/// - Espaciado riguroso: cada prop, árbol, banca, mesa y farol posee su propia coordenada
///   con holgura de 4 a 10 metros, eliminando cualquier empalme visual.
/// - IA de La Llorona intensificada: mayor rango de visión (12.5 m), amplio campo auditivo (5.0 m / 10 m al correr),
///   patrullaje activo (2.35 m/s) y persecución implacable (3.70 m/s), forzando el uso de escondites y sigilo.
/// - Textos HD ultra-nítidos: TextMesh de mundo renderizados a 72-80 px con escala fina (0.045f)
///   y Canvas con dynamicPixelsPerUnit = 2.0f para eliminar cualquier borrosidad.
/// - Tutorial escalonado en 4 pasos contextuales breves y claros.
/// </summary>
public static class Level3Builder
{
    const string ScenePath = "Assets/Scenes/Level3_Atrio.unity";

    [MenuItem("La Llorona/Construir Nivel 3")]
    public static void BuildLevel3Scene()
    {
        Debug.Log("[Level3Builder] Reconstruyendo Nivel 3 (Atrio Monumental - Cero Empalmes + IA Activa + Textos HD)...");

        // 1. Asegurar slicing y configuración de sprites de Nivel 3
        SpriteSheetSlicer.SliceNivel3();
        AssetDatabase.Refresh();

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 3 Monumental Oficial");

        // Limpieza de objetos residuales de la escena para evitar basura o colisiones duplicadas
        CleanOldSceneGarbage(scene);

        int wallsLayer = LayerMask.NameToLayer("Walls");
        if (wallsLayer < 0) wallsLayer = LayerMask.NameToLayer("Default");
        int hideSpotLayer = LayerMask.NameToLayer("HideSpot");
        if (hideSpotLayer < 0) hideSpotLayer = LayerMask.NameToLayer("Default");
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer < 0) playerLayer = LayerMask.NameToLayer("Default");
        int lloronaLayer = LayerMask.NameToLayer("Llorona");
        if (lloronaLayer < 0) lloronaLayer = LayerMask.NameToLayer("Default");

        // Cargar sprites oficiales nativos de Nivel 3
        Sprite catedralSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/catedral.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sAltarSagrado = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/altar_sagrado.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sMesaVelas = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/mesa_velas.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sSelloPuerta = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/sello_puerta.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sCirculoSantuario = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/circulo_santuario.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sPuertasAbiertas = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/puertas_abiertas.png")
            .OfType<Sprite>().FirstOrDefault();

        // Cargar los 5 NUEVOS sprites pixel art de Nivel 3
        Sprite sAltarCustom = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Nivel3/Altar de Día de Muertos en píxeles.png")
            .OfType<Sprite>().FirstOrDefault() ?? sAltarSagrado;
        Sprite sArcadaCustom = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Nivel3/Arcada gótica iluminada por faroles.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sCipresCustom = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Nivel3/Ciprés gótico en pixel art.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sEscombrosCustom = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Nivel3/Escombros de ruinas en pixel art.png")
            .OfType<Sprite>().FirstOrDefault();
        Sprite sMausoleoCustom = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Nivel3/Mausoleo gótico con velas y flores.png")
            .OfType<Sprite>().FirstOrDefault();

        // Cargar clips de audio oficiales del Nivel 3
        AudioClip sfxCampanas           = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_campanas_catedral.wav");
        AudioClip sfxLloronaGrito       = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_llorona_grito_detect.wav");
        AudioClip sfxLloronaLlanto      = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_llorona_lamento_loop.wav");
        AudioClip sfxNichoConsagrado    = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_nicho_consagrado.wav");
        AudioClip sfxPortalApertura     = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_portal_apertura.wav");
        AudioClip sfxPrenderVela        = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_prender_vela.wav");
        AudioClip sfxPuertaConfesionario = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_puerta_confesionario.wav");
        AudioClip sfxRespiroAlivio      = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_respiro_alivio.wav");
        AudioClip sfxSplashCharco       = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_splash_charco.wav");
        AudioClip sfxTepalcatesCrunch   = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_tepalcates_crunch.wav");
        AudioClip sfxTomarVela          = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_tomar_vela.wav");
        AudioClip sfxUiClickStone       = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_ui_click_stone.wav");

        Sprite[] accSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/accesorios.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] fuenteSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/fuente.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] rejaSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/rejaYfaroles.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] ofrendaSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/ofrendas.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();

        // 2. Grid y Suelo Monumental con Tilemap Ampliado (X: -12 a 48, Y: 0 a 46)
        GameObject gridGo = GameObject.Find("Grid");
        if (gridGo == null) gridGo = new GameObject("Grid");
        if (gridGo.GetComponent<Grid>() == null) gridGo.AddComponent<Grid>();

        Tilemap suelo = GetOrCreateTilemap(gridGo.transform, "Suelo", 0, -1, false);
        Tilemap paredes = GetOrCreateTilemap(gridGo.transform, "Paredes", 1, wallsLayer, true);
        suelo.ClearAllTiles();
        paredes.ClearAllTiles();

        TileBase[] allTiles = LoadTiles();
        for (int x = -12; x <= 48; x++)
        {
            for (int y = 0; y <= 46; y++)
            {
                TileBase t = ((x + y) % 5 == 0) ? allTiles[1] : allTiles[0];
                suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // 3. Fachada Monumental de la Parroquia de Dolores (Al Norte del atrio, Y = 39.5f)
        Transform bgGroup = FreshRoot("Fondo_Monumental");
        if (catedralSprite != null)
        {
            GameObject catGo = new GameObject("Parroquia_Dolores_Hidalgo");
            catGo.transform.SetParent(bgGroup, false);
            catGo.transform.position = new Vector3(18.0f, 39.5f, 0f);
            catGo.transform.localScale = new Vector3(0.82f, 0.82f, 1.0f);

            SpriteRenderer srCat = catGo.AddComponent<SpriteRenderer>();
            srCat.sprite = catedralSprite;
            srCat.sortingOrder = 2;
            srCat.color = new Color(0.98f, 0.96f, 1.0f);

            // Muros laterales que cubren las alas de la fachada y torres
            BoxCollider2D leftWallCol = catGo.AddComponent<BoxCollider2D>();
            leftWallCol.size = new Vector2(4.8f, 3.4f);
            leftWallCol.offset = new Vector2(-4.0f, 1.2f);

            BoxCollider2D rightWallCol = catGo.AddComponent<BoxCollider2D>();
            rightWallCol.size = new Vector2(4.8f, 3.4f);
            rightWallCol.offset = new Vector2(4.0f, 1.2f);

            // Muro superior detrás de la puerta
            BoxCollider2D doorBackWall = catGo.AddComponent<BoxCollider2D>();
            doorBackWall.size = new Vector2(3.4f, 1.0f);
            doorBackWall.offset = new Vector2(0f, 2.0f);

            if (wallsLayer >= 0) catGo.layer = wallsLayer;
        }

        // 4. Perímetro y Rejas (Límites cerrados para el atrio monumental)
        Transform rejasGroup = FreshRoot("Rejas_Perimetro");
        Sprite rejaInf = rejaSprites.FirstOrDefault(s => s.name == "Reja_Inferior") ?? rejaSprites.ElementAtOrDefault(6);
        Sprite farolaPie = rejaSprites.FirstOrDefault(s => s.name == "Farola_Pie_N3") ?? rejaSprites.ElementAtOrDefault(5);

        // Reja sur inferior (dejando entrada central limpia en X = 13..23)
        for (float rx = -6.0f; rx <= 9.0f; rx += 5.5f)
        {
            CreateFencePiece(rejasGroup, $"Reja_Sur_Oeste_{rx:0}", rejaInf, rx, 0.8f, wallsLayer, new Vector2(5.5f, 1.0f));
        }
        for (float rx = 27.0f; rx <= 42.0f; rx += 5.5f)
        {
            CreateFencePiece(rejasGroup, $"Reja_Sur_Este_{rx:0}", rejaInf, rx, 0.8f, wallsLayer, new Vector2(5.5f, 1.0f));
        }

        // Farolas de pie distribuidas con distancia y sin obstruir objetos
        CreateStandingLantern(rejasGroup, "Farola_Entrada_O", farolaPie, 13.0f, 1.8f, wallsLayer);
        CreateStandingLantern(rejasGroup, "Farola_Entrada_E", farolaPie, 23.0f, 1.8f, wallsLayer);
        CreateStandingLantern(rejasGroup, "Farola_Plaza_SO", farolaPie,  8.0f, 16.0f, wallsLayer);
        CreateStandingLantern(rejasGroup, "Farola_Plaza_SE", farolaPie, 28.0f, 16.0f, wallsLayer);
        CreateStandingLantern(rejasGroup, "Farola_Plaza_NO", farolaPie,  8.0f, 29.0f, wallsLayer);
        CreateStandingLantern(rejasGroup, "Farola_Plaza_NE", farolaPie, 28.0f, 29.0f, wallsLayer);
        CreateStandingLantern(rejasGroup, "Farola_AtrioSup_O", farolaPie, 12.0f, 36.5f, wallsLayer);
        CreateStandingLantern(rejasGroup, "Farola_AtrioSup_E", farolaPie, 24.0f, 36.5f, wallsLayer);

        // Límites invisibles sólidos del atrio completo
        Transform limits = FreshRoot("Limites_Atrio");
        CreateInvisibleWall(limits, "Lim_Oeste", -10.0f,  22.5f, 46f, wallsLayer);
        CreateInvisibleWall(limits, "Lim_Este",   46.0f,  22.5f, 46f, wallsLayer);
        CreateInvisibleWall(limits, "Lim_Sur",    18.0f,   0.5f,  1f, wallsLayer, 58.0f);
        CreateInvisibleWall(limits, "Lim_Norte",  18.0f,  44.5f,  1f, wallsLayer, 58.0f);

        // 5. SECTOR 3: Fuente Central Monumental en la Gran Plaza (X = 18.0f, Y = 21.0f)
        Transform props = FreshRoot("Props_Atrio");
        Sprite fuenteSprite = fuenteSprites.FirstOrDefault(s => s.name == "Fuente_Octagonal_N3") ?? fuenteSprites.ElementAtOrDefault(0);
        if (fuenteSprite != null)
        {
            GameObject fGo = new GameObject("Fuente_Octagonal_Central");
            fGo.transform.SetParent(props, false);
            fGo.transform.position = new Vector3(18.0f, 21.0f, 0f);
            fGo.transform.localScale = Vector3.one * 1.25f;

            SpriteRenderer srF = fGo.AddComponent<SpriteRenderer>();
            srF.sprite = fuenteSprite;
            srF.sortingOrder = 3;

            CircleCollider2D cc = fGo.AddComponent<CircleCollider2D>();
            cc.radius = 2.1f;
            cc.offset = new Vector2(0f, -0.2f);
            if (wallsLayer >= 0) fGo.layer = wallsLayer;

            GameObject lf = new GameObject("Luz_Agua_Fuente");
            lf.transform.SetParent(fGo.transform, false);
            Light2D l2df = lf.AddComponent<Light2D>();
            l2df.lightType = Light2D.LightType.Point;
            l2df.color = new Color(0.40f, 0.70f, 1.0f);
            l2df.intensity = 1.4f;
            l2df.pointLightOuterRadius = 6.0f;
        }

        // 6. SECTOR 1 (SUR): Gran Calzada de los Cipreses Góticos (ESPACIADO LIMPIO Y SIN EMPALMES)
        Sprite sCipres = sCipresCustom ?? accSprites.FirstOrDefault(s => s.name == "Cipres_Colonial_1") ?? accSprites.ElementAtOrDefault(4);
        if (sCipres != null)
        {
            // Flanco Oeste de la Calzada
            CreateCypressTree(props, "Cipres_Calzada_O1", sCipres, 10.0f,  7.0f, wallsLayer);
            CreateCypressTree(props, "Cipres_Calzada_O2", sCipres, 10.0f, 12.5f, wallsLayer);

            // Flanco Este de la Calzada (Completamente separado de la mesa y bancas)
            CreateCypressTree(props, "Cipres_Calzada_E1", sCipres, 26.0f, 12.5f, wallsLayer);

            // Esquinas del Panteón Este
            CreateCypressTree(props, "Cipres_Panteon_SE", sCipres, 44.0f, 16.5f, wallsLayer);
            CreateCypressTree(props, "Cipres_Panteon_NE", sCipres, 44.0f, 32.0f, wallsLayer);

            // Terraza Superior Norte enmarcando la Parroquia
            CreateCypressTree(props, "Cipres_Atrio_Oeste", sCipres,  9.0f, 35.0f, wallsLayer);
            CreateCypressTree(props, "Cipres_Atrio_Este",  sCipres, 27.0f, 35.0f, wallsLayer);
        }

        // 7. SECTOR 2 (OESTE): Claustro de las Arcadas Góticas Iluminadas
        if (sArcadaCustom != null)
        {
            CreateGothicArcade(props, "Arcada_Gotica_Norte", sArcadaCustom, -3.0f, 31.0f, wallsLayer);
            CreateGothicArcade(props, "Arcada_Gotica_Sur",   sArcadaCustom, -3.0f, 17.0f, wallsLayer);
        }

        // 8. SECTOR 4 (ESTE): Gran Mausoleo Gótico en el Panteón de Criptas
        if (sMausoleoCustom != null)
        {
            CreateMausoleumBuilding(props, "Gran_Mausoleo_Gotico", sMausoleoCustom, 39.0f, 28.0f, wallsLayer);
        }

        // 9. Escondites Coloniales Espaciados (Bancas, Confesionarios y Criptas - SIN EMPALMES)
        Sprite sConfesionario = accSprites.FirstOrDefault(s => s.name == "Confesionario_N3") ?? accSprites.ElementAtOrDefault(1);
        Sprite sBanca         = accSprites.FirstOrDefault(s => s.name == "Banca_Hierro_N3") ?? accSprites.ElementAtOrDefault(0);
        Sprite sCripta        = accSprites.FirstOrDefault(s => s.name == "Cripta_Cantera_N3") ?? accSprites.ElementAtOrDefault(2);

        // Calzada Sur: bancas en las zonas exteriores de los jardines
        CreateHideSpotOfficial(props, "Banca_Calzada_Oeste", sBanca,  6.5f, 4.0f, 1.1f, hideSpotLayer, wallsLayer,
            new Vector2(2.8f, 0.9f), new Vector2(0f, 0.2f), new Vector2(3.2f, 1.6f), new Vector2(0f, -0.4f));
        CreateHideSpotOfficial(props, "Banca_Calzada_Este",  sBanca, 29.5f, 4.0f, 1.1f, hideSpotLayer, wallsLayer,
            new Vector2(2.8f, 0.9f), new Vector2(0f, 0.2f), new Vector2(3.2f, 1.6f), new Vector2(0f, -0.4f));

        // Claustro Oeste
        CreateHideSpotOfficial(props, "Confesionario_Claustro", sConfesionario, -8.0f, 24.0f, 1.15f, hideSpotLayer, wallsLayer,
            new Vector2(2.0f, 1.4f), new Vector2(0f, 0.4f), new Vector2(2.4f, 1.8f), new Vector2(0f, -0.4f));
        CreateHideSpotOfficial(props, "Banca_Claustro", sBanca, -3.0f, 20.5f, 1.1f, hideSpotLayer, wallsLayer,
            new Vector2(2.8f, 0.9f), new Vector2(0f, 0.2f), new Vector2(3.2f, 1.6f), new Vector2(0f, -0.4f));

        // Panteón Este
        CreateHideSpotOfficial(props, "Cripta_Panteon_Este", sCripta, 44.0f, 24.0f, 1.1f, hideSpotLayer, wallsLayer,
            new Vector2(2.4f, 1.4f), new Vector2(0f, 0.4f), new Vector2(2.8f, 1.8f), new Vector2(0f, -0.4f));
        CreateHideSpotOfficial(props, "Banca_Panteon", sBanca, 34.0f, 24.0f, 1.1f, hideSpotLayer, wallsLayer,
            new Vector2(2.8f, 0.9f), new Vector2(0f, 0.2f), new Vector2(3.2f, 1.6f), new Vector2(0f, -0.4f));

        // Terraza Superior cerca de la Parroquia
        CreateHideSpotOfficial(props, "Confesionario_Atrio_Oeste", sConfesionario,  5.0f, 38.5f, 1.15f, hideSpotLayer, wallsLayer,
            new Vector2(2.0f, 1.4f), new Vector2(0f, 0.4f), new Vector2(2.4f, 1.8f), new Vector2(0f, -0.4f));
        CreateHideSpotOfficial(props, "Confesionario_Atrio_Este",  sConfesionario, 31.0f, 38.5f, 1.15f, hideSpotLayer, wallsLayer,
            new Vector2(2.0f, 1.4f), new Vector2(0f, 0.4f), new Vector2(2.4f, 1.8f), new Vector2(0f, -0.4f));

        // 10. LOS 4 NICHOS SAGRADOS DEL RITUAL (4/4) ([F] para ofrendar veladora - TEXTOS HD)
        Transform nichosGroup = FreshRoot("Nichos_Sagrados");
        Sprite sFlama = ofrendaSprites.FirstOrDefault(s => s.name == "Vela_Blanca_Grande") ?? ofrendaSprites.ElementAtOrDefault(4);
        Sprite sFlores = ofrendaSprites.FirstOrDefault(s => s.name == "Ramo_Cempasuchil_1") ?? ofrendaSprites.ElementAtOrDefault(7);
        Sprite sCandelabro = ofrendaSprites.FirstOrDefault(s => s.name == "Candelabro_Vela") ?? ofrendaSprites.ElementAtOrDefault(3);

        // Nicho 1: Claustro de las Arcadas (Oeste)
        CreateShrineNiche(nichosGroup, "Nicho_Altar_Oeste", "Altar del Claustro (Oeste)",
            -3.0f, 24.0f, sAltarCustom, sFlama, sFlores, sCirculoSantuario, wallsLayer, sfxPrenderVela, sfxNichoConsagrado);

        // Nicho 2: Pocito de la Fuente Sagrada (Centro)
        CreateShrineNiche(nichosGroup, "Nicho_Fuente_Centro", "Pocito de la Fuente (Centro)",
            18.0f, 26.5f, sAltarCustom, sFlama, sFlores, sCirculoSantuario, wallsLayer, sfxPrenderVela, sfxNichoConsagrado);

        // Nicho 3: Cripta de Ánimas en el Panteón (Este)
        CreateShrineNiche(nichosGroup, "Nicho_Mausoleo_Este", "Cripta de Ánimas (Este)",
            39.0f, 19.5f, sAltarCustom, sFlama, sFlores, sCirculoSantuario, wallsLayer, sfxPrenderVela, sfxNichoConsagrado);

        // Nicho 4: Altar Mayor de Dolores (Norte, a los pies de la escalinata)
        CreateShrineNiche(nichosGroup, "Nicho_Altar_Norte", "Altar Mayor de Dolores (Norte)",
            18.0f, 33.5f, sAltarCustom, sFlama, sFlores, sCirculoSantuario, wallsLayer, sfxPrenderVela, sfxNichoConsagrado);

        // Mesa de Cirios Benditos: ubicación despejada en la calzada derecha (X = 27.5, Y = 8.0)
        CreateCandleStandOfficial(nichosGroup, 27.5f, 8.0f, sMesaVelas ?? sAltarCustom, sCandelabro, wallsLayer, sfxTomarVela);

        // 11. TRAMPAS DE TERRENO Y OBSTÁCULOS
        Transform hazards = FreshRoot("Obstaculos_Ofrendas");
        Sprite sAguaBendita = ofrendaSprites.FirstOrDefault(s => s.name == "Charco_AguaBendita") ?? ofrendaSprites.ElementAtOrDefault(0);
        Sprite sTepalcate   = ofrendaSprites.FirstOrDefault(s => s.name == "Vidrios_Jarron_Roto") ?? ofrendaSprites.ElementAtOrDefault(5);
        Sprite sVelaRoja    = ofrendaSprites.FirstOrDefault(s => s.name == "Vaso_Vela_Roja_1") ?? ofrendaSprites.ElementAtOrDefault(1);

        // Charcos ralentizantes en la plaza central
        CreateMudPuddleOfficial(hazards, sAguaBendita, 12.0f, 21.0f, 0.9f, sfxSplashCharco);
        CreateMudPuddleOfficial(hazards, sAguaBendita, 24.0f, 21.0f, 0.9f, sfxSplashCharco);

        // Escombros de cantera en los accesos clave
        Sprite rubbleSprite = sEscombrosCustom ?? sTepalcate;
        CreateNoiseRubbleOfficial(hazards, "Escombros_Acceso_Claustro_S", rubbleSprite,  4.5f, 18.0f, sfxTepalcatesCrunch);
        CreateNoiseRubbleOfficial(hazards, "Escombros_Acceso_Claustro_N", rubbleSprite,  4.5f, 28.0f, sfxTepalcatesCrunch);
        CreateNoiseRubbleOfficial(hazards, "Escombros_Acceso_Panteon_S",  rubbleSprite, 31.5f, 18.0f, sfxTepalcatesCrunch);
        CreateNoiseRubbleOfficial(hazards, "Escombros_Acceso_Panteon_N",  rubbleSprite, 31.5f, 28.0f, sfxTepalcatesCrunch);
        CreateNoiseRubbleOfficial(hazards, "Escombros_Escalinata_Oeste",  rubbleSprite, 13.0f, 32.0f, sfxTepalcatesCrunch);
        CreateNoiseRubbleOfficial(hazards, "Escombros_Escalinata_Este",   rubbleSprite, 23.0f, 32.0f, sfxTepalcatesCrunch);

        // 13 Veladoras Coleccionables bien distribuidas en las zonas (El inicio permanece despejado sin veladoras)
        CreateCandlePickupOfficial(hazards, "Vela_Calzada_Oeste",  sVelaRoja, new Vector2(14.0f,  9.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Calzada_Este",   sVelaRoja, new Vector2(22.0f,  9.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Claustro_Sur",   sVelaRoja, new Vector2(-5.0f, 20.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Claustro_Norte", sVelaRoja, new Vector2(-2.0f, 28.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Claustro_Paso",  sVelaRoja, new Vector2( 2.0f, 24.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Plaza_Oeste",    sVelaRoja, new Vector2(14.0f, 18.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Plaza_Este",     sVelaRoja, new Vector2(22.0f, 18.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Plaza_Norte",    sVelaRoja, new Vector2(18.0f, 28.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Panteon_Sur",    sVelaRoja, new Vector2(35.0f, 20.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Panteon_Centro", sVelaRoja, new Vector2(38.0f, 24.0f));
        CreateCandlePickupOfficial(hazards, "Vela_Mausoleo",       sVelaRoja, new Vector2(41.0f, 28.0f));
        CreateCandlePickupOfficial(hazards, "Vela_AtrioSup_Oeste", sVelaRoja, new Vector2(13.0f, 34.5f));
        CreateCandlePickupOfficial(hazards, "Vela_AtrioSup_Este",  sVelaRoja, new Vector2(23.0f, 34.5f));

        // 12. WAYPOINTS DE PATRULLA EXTENDIDA PARA LA LLORONA (10 Puntos espaciados)
        Transform wpRoot = FreshRoot("Waypoints");
        Transform[] wps = new Transform[10];
        wps[0] = CreateEmpty(wpRoot, "WP_Calzada_Sur",        18.0f,  6.0f);
        wps[1] = CreateEmpty(wpRoot, "WP_Acceso_Claustro_SO",  3.0f, 18.0f);
        wps[2] = CreateEmpty(wpRoot, "WP_Claustro_Oeste",     -3.0f, 24.0f);
        wps[3] = CreateEmpty(wpRoot, "WP_Explanada_NO",        8.0f, 33.0f);
        wps[4] = CreateEmpty(wpRoot, "WP_AltarMayor_Norte",   18.0f, 33.5f);
        wps[5] = CreateEmpty(wpRoot, "WP_Explanada_NE",       28.0f, 33.0f);
        wps[6] = CreateEmpty(wpRoot, "WP_Gran_Mausoleo_NE",   39.0f, 28.0f);
        wps[7] = CreateEmpty(wpRoot, "WP_Panteon_SE",         39.0f, 20.0f);
        wps[8] = CreateEmpty(wpRoot, "WP_Acceso_Panteon_SE",  31.5f, 18.0f);
        wps[9] = CreateEmpty(wpRoot, "WP_Fuente_Centro",      18.0f, 21.0f);

        // 13. META DEL NIVEL: Puertas Principales de la Parroquia de Dolores (Coronando las escaleras en Y = 40.05f)
        Transform exitGroup = FreshRoot("Salida_Meta");
        CreateParroquiaEntranceGoalWithSeal(exitGroup, 18.0f, 40.05f, 4, sSelloPuerta, sPuertasAbiertas, sfxPortalApertura, sfxCampanas);

        // 14. Iluminación Global URP 2D (Noche en el atrio: 0.50f)
        EnsureGlobalLight(0.50f);

        // 15. Player (Carlos inicia en la calzada sur en Y = 2.5f)
        SetupPlayer(new Vector2(18.0f, 2.5f), playerLayer, sfxPuertaConfesionario, sfxRespiroAlivio);

        // 16. La Llorona (Inicia en la entrada de la plaza patrullando activamente hacia la calzada y claustro)
        SetupLloronaCircular(new Vector2(18.0f, 15.0f), wps, wallsLayer, lloronaLayer,
            patrolSpeed: 2.35f, chaseSpeed: 3.70f, visionDist: 12.5f, fov: 130.0f, hearDist: 5.0f,
            waitTime: 0.6f, loseSightTime: 4.5f, runNoiseMultiplier: 2.0f,
            screamClip: sfxLloronaGrito, cryClip: sfxLloronaLlanto);

        // 17. Cámara y Confiner 2D (Zoom dinámico cinematográfico para mapa monumental)
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 3.6f;
        cam.transform.position = new Vector3(18.0f, 4.0f, -10f);
        cam.backgroundColor = new Color(0.04f, 0.03f, 0.08f);

        CameraFollow2D camFollow = cam.GetComponent<CameraFollow2D>();
        if (camFollow == null) camFollow = cam.gameObject.AddComponent<CameraFollow2D>();
        camFollow.SetOrthoSize(3.6f);
        camFollow.SetOffset(new Vector3(0f, 1.1f, -10f));
        camFollow.SetBounds(new Vector2(-6.0f, 3.8f), new Vector2(42.0f, 41.5f));
        camFollow.EnableDynamicZoom(minY: 22.0f, maxY: 39.5f, minSize: 3.6f, maxSize: 5.2f, normalOffY: 1.1f, peakOffY: 2.8f);

        // 18. TRIGGERS DE TUTORIAL PROGRESIVOS Y ESCALONADOS (Mensajes claros, legibles y oportunos)
        Transform tutRoot = FreshRoot("TutorialTriggers");

        // Paso 1: Introducción clara al objetivo principal en la entrada
        CreateTutorialTrigger(tutRoot, "Tutorial_1_Inicio",
            new Vector2(18.0f, 2.5f), new Vector2(12f, 4f),
            "✦ EL ATRIO DE DOLORES ✦\nLas puertas del templo están selladas por el luto espectral.\nExplora el atrio y consagra los 4 nichos sagrados con veladoras.",
            duration: 5.5f, showOnStart: true);

        // Paso 2: Explicación de las veladoras junto a la Mesa de Cirios
        CreateTutorialTrigger(tutRoot, "Tutorial_2_MesaVelas",
            new Vector2(27.5f, 8.0f), new Vector2(8f, 4f),
            "✦ MESA DE CIRIOS BENDITOS ✦\nPulsa [E] para tomar veladoras. Necesitas 1 para consagrar cada nicho.",
            duration: 5.0f, showOnStart: false);

        // Paso 3: Mecánica de sigilo al aproximarse a la gran plaza
        CreateTutorialTrigger(tutRoot, "Tutorial_3_Sigilo",
            new Vector2(18.0f, 12.0f), new Vector2(18f, 3f),
            "✦ ESCONDITES DE EMERGENCIA ✦\nSi escuchas el llanto de La Llorona, pulsa [E] en bancas y confesionarios para ocultarte.",
            duration: 5.0f, showOnStart: false);

        // Paso 4: Alerta de escombros de ruinas al acercarse a los pasajes laterales
        CreateTutorialTrigger(tutRoot, "Tutorial_4_Escombros",
            new Vector2(18.0f, 16.0f), new Vector2(24f, 3f),
            "✦ ¡CUIDADO CON LOS ESCOMBROS! ✦\nPisar ruinas de cantera cruje fuertemente y alertará a La Llorona.",
            duration: 5.0f, showOnStart: false);

        // 19. GameManager y AudioManager
        SetupGameManager(3, sfxCampanas, sfxPrenderVela, sfxPuertaConfesionario, sfxLloronaGrito, sfxLloronaLlanto, sfxUiClickStone);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        UIBuilder.BuildLevel3UICanvas();
        Debug.Log("[Level3Builder] ¡Nivel 3 monumental (El Atrio de Dolores - 4 Ofrendas) reconstruido y guardado con éxito!");
    }

    static void CleanOldSceneGarbage(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        HashSet<string> preserveNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Main Camera", "Player", "Llorona", "GameManager", "AudioManager", "Global Light 2D", "UICanvas", "Canvas", "EventSystem"
        };

        foreach (var root in roots)
        {
            if (!preserveNames.Contains(root.name))
            {
                Undo.DestroyObjectImmediate(root);
            }
        }

        // Remover cualquier LevelGoal huérfano de ejecuciones anteriores
        foreach (var lg in UnityEngine.Object.FindObjectsByType<LevelGoal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Undo.DestroyObjectImmediate(lg);
        }
    }

    static void CreateGothicArcade(Transform parent, string name, Sprite sprite, float x, float y, int wallsLayer)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * 0.38f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;
        sr.color = new Color(0.96f, 0.94f, 0.90f);

        // Columnas laterales sólidas dejando el arco central libre para caminar
        BoxCollider2D leftCol = go.AddComponent<BoxCollider2D>();
        leftCol.size = new Vector2(1.2f, 2.0f);
        leftCol.offset = new Vector2(-4.0f, 0.5f);

        BoxCollider2D rightCol = go.AddComponent<BoxCollider2D>();
        rightCol.size = new Vector2(1.2f, 2.0f);
        rightCol.offset = new Vector2(4.0f, 0.5f);

        if (wallsLayer >= 0) go.layer = wallsLayer;

        // Farol colgante con luz cálida
        GameObject lGo = new GameObject("Luz_Farol_Arcada");
        lGo.transform.SetParent(go.transform, false);
        lGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        Light2D l2d = lGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.76f, 0.40f);
        l2d.intensity = 1.8f;
        l2d.pointLightOuterRadius = 6.0f;
        lGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateMausoleumBuilding(Transform parent, string name, Sprite sprite, float x, float y, int wallsLayer)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * 0.35f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;
        sr.color = new Color(0.96f, 0.92f, 0.88f);

        // Collider sólido en el cuerpo del mausoleo
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(3.8f, 2.4f);
        col.offset = new Vector2(0f, 0.6f);
        if (wallsLayer >= 0) go.layer = wallsLayer;

        // Luz de veladoras votivas frente al mausoleo
        GameObject lGo = new GameObject("Luz_Velas_Mausoleo");
        lGo.transform.SetParent(go.transform, false);
        lGo.transform.localPosition = new Vector3(0f, -0.4f, 0f);
        Light2D l2d = lGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.68f, 0.28f);
        l2d.intensity = 2.0f;
        l2d.pointLightOuterRadius = 5.5f;
        lGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateCypressTree(Transform parent, string name, Sprite sprite, float x, float y, int wallsLayer)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * 0.28f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;
        sr.color = new Color(0.92f, 0.94f, 0.90f);

        // Collider ceñido exclusivamente a la base del tronco para permitir paso alrededor
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.85f, 0.65f);
        col.offset = new Vector2(0f, 0.35f);
        if (wallsLayer >= 0) go.layer = wallsLayer;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateNoiseRubbleOfficial(Transform parent, string name, Sprite sprite, float x, float y, AudioClip crunchSfx = null)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * 0.18f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;
        sr.color = new Color(0.90f, 0.88f, 0.85f);

        // Trigger amplio para detectar pisadas de Carlos
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(3.0f, 1.8f);
        bc.offset = Vector2.zero;

        NoiseObstacle no = go.AddComponent<NoiseObstacle>();
        if (crunchSfx != null)
        {
            SerializedObject sono = new SerializedObject(no);
            sono.FindProperty("glassBreakSound").objectReferenceValue = crunchSfx;
            sono.ApplyModifiedProperties();
        }

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateShrineNiche(Transform parent, string goName, string displayName, float x, float y,
        Sprite pedestalSprite, Sprite flameSprite, Sprite flowerSprite, Sprite circuloSprite,
        int wallsLayer, AudioClip lightSfx = null, AudioClip blessSfx = null)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        // Pedestal base
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = pedestalSprite;
        sr.sortingOrder = 3;
        sr.color = new Color(0.96f, 0.94f, 0.90f);

        float scaleFactor = (pedestalSprite != null && pedestalSprite.rect.width > 800) ? 0.22f : 0.85f;
        go.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

        // Collider físico para que el altar sea un obstáculo sólido
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2.4f, 1.2f);
        col.offset = new Vector2(0f, 0.4f);
        if (wallsLayer >= 0) go.layer = wallsLayer;

        // Ramilletes de cempasúchil decorativos
        if (flowerSprite != null)
        {
            GameObject fl1 = new GameObject("Flores_Izq");
            fl1.transform.SetParent(go.transform, false);
            fl1.transform.localPosition = new Vector3(-1.1f, 0.1f, 0f);
            SpriteRenderer srFl1 = fl1.AddComponent<SpriteRenderer>();
            srFl1.sprite = flowerSprite;
            srFl1.sortingOrder = 4;
            fl1.transform.localScale = Vector3.one * 0.9f;

            GameObject fl2 = new GameObject("Flores_Der");
            fl2.transform.SetParent(go.transform, false);
            fl2.transform.localPosition = new Vector3(1.1f, 0.1f, 0f);
            SpriteRenderer srFl2 = fl2.AddComponent<SpriteRenderer>();
            srFl2.sprite = flowerSprite;
            srFl2.sortingOrder = 4;
            fl2.transform.localScale = new Vector3(-0.9f, 0.9f, 1f);
        }

        // Visual de flama sagrada (inicia inactivo hasta consagrarse)
        GameObject flameGo = new GameObject("Flama_Sagrada");
        flameGo.transform.SetParent(go.transform, false);
        flameGo.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        SpriteRenderer srFlame = flameGo.AddComponent<SpriteRenderer>();
        srFlame.sprite = flameSprite;
        srFlame.sortingOrder = 5;
        flameGo.SetActive(false);

        // Luz divina dorada del nicho consagrado
        GameObject lightGo = new GameObject("Luz_Nicho");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 1.0f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.84f, 0.42f);
        l2d.intensity = 2.6f;
        l2d.pointLightOuterRadius = 6.2f;
        lightGo.AddComponent<LightFlicker>();
        l2d.enabled = false;

        // Luz guía etérea (Faro celestial que llama al jugador desde lejos)
        GameObject beaconGo = new GameObject("Luz_Guia_Faro");
        beaconGo.transform.SetParent(go.transform, false);
        beaconGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        Light2D l2dBeacon = beaconGo.AddComponent<Light2D>();
        l2dBeacon.lightType = Light2D.LightType.Point;
        l2dBeacon.color = new Color(0.65f, 0.90f, 1.0f);
        l2dBeacon.intensity = 1.8f;
        l2dBeacon.pointLightOuterRadius = 6.5f;
        beaconGo.AddComponent<LightFlicker>();

        // Aura de domo santuario (círculo ritual de cempasúchil en el suelo)
        GameObject domeGo = new GameObject("Aura_Santuario");
        domeGo.transform.SetParent(go.transform, false);
        domeGo.transform.localPosition = new Vector3(0f, 0.4f, 0f);

        if (circuloSprite != null)
        {
            SpriteRenderer srDome = domeGo.AddComponent<SpriteRenderer>();
            srDome.sprite = circuloSprite;
            srDome.sortingOrder = 1;
            srDome.color = new Color(1.0f, 0.95f, 0.75f, 0.88f);
            float spriteWidth = circuloSprite.bounds.size.x > 0.01f ? circuloSprite.bounds.size.x : 5.1f;
            float targetDomeScale = (3.2f * 2.0f) / (spriteWidth * scaleFactor);
            domeGo.transform.localScale = new Vector3(targetDomeScale, targetDomeScale, 1f);
        }

        Light2D l2dDome = domeGo.AddComponent<Light2D>();
        l2dDome.lightType = Light2D.LightType.Point;
        l2dDome.color = new Color(1.0f, 0.88f, 0.35f);
        l2dDome.intensity = 1.6f;
        l2dDome.pointLightOuterRadius = 3.6f;
        domeGo.SetActive(false);

        // Prompt de interacción [F] con TextMesh nativo en ALTA DEFINICIÓN (fontSize 72, characterSize 0.045f)
        GameObject promptGo = new GameObject("PromptInteraccion");
        promptGo.transform.SetParent(go.transform, false);
        promptGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        TextMesh tm = promptGo.AddComponent<TextMesh>();
        tm.text = $"✦ {displayName} ✦";
        tm.fontSize = 72;
        tm.characterSize = 0.045f;
        tm.fontStyle = FontStyle.Bold;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1.0f, 0.95f, 0.50f);
        MeshRenderer mr = promptGo.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;
        promptGo.SetActive(false);

        // Componente ShrineNiche
        ShrineNiche sn = go.AddComponent<ShrineNiche>();
        SerializedObject so = new SerializedObject(sn);
        so.FindProperty("shrineName").stringValue = displayName;
        so.FindProperty("sanctuaryRadius").floatValue = 3.4f;
        so.FindProperty("flameVisual").objectReferenceValue = flameGo;
        so.FindProperty("shrineLight").objectReferenceValue = l2d;
        so.FindProperty("beaconLight").objectReferenceValue = l2dBeacon;
        so.FindProperty("sanctuaryDome").objectReferenceValue = domeGo;
        so.FindProperty("promptObject").objectReferenceValue = promptGo;
        so.FindProperty("promptTextMesh").objectReferenceValue = tm;
        if (lightSfx != null) so.FindProperty("lightSound").objectReferenceValue = lightSfx;
        if (blessSfx != null) so.FindProperty("blessingSound").objectReferenceValue = blessSfx;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, goName);
    }

    static void CreateCandleStandOfficial(Transform parent, float x, float y, Sprite altarSprite, Sprite candelabroSprite, int wallsLayer, AudioClip takeSfx = null)
    {
        GameObject go = new GameObject("Mesa_Cirios_Benditos");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = altarSprite;
        sr.sortingOrder = 3;
        sr.color = new Color(0.95f, 0.90f, 0.85f);
        float scaleMesa = (altarSprite != null && altarSprite.rect.width > 800) ? 0.22f : 0.72f;
        go.transform.localScale = new Vector3(scaleMesa, scaleMesa, 1.0f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2.6f, 1.2f);
        col.offset = new Vector2(0f, 0.4f);
        if (wallsLayer >= 0) go.layer = wallsLayer;

        if (candelabroSprite != null)
        {
            GameObject candleChild = new GameObject("Cirios_Mesa");
            candleChild.transform.SetParent(go.transform, false);
            candleChild.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            SpriteRenderer srC = candleChild.AddComponent<SpriteRenderer>();
            srC.sprite = candelabroSprite;
            srC.sortingOrder = 4;
        }

        GameObject lightGo = new GameObject("Luz_Mesa_Cirios");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.88f, 0.50f);
        l2d.intensity = 2.0f;
        l2d.pointLightOuterRadius = 4.5f;
        lightGo.AddComponent<LightFlicker>();

        // TextMesh HD (fontSize 68, characterSize 0.045f)
        GameObject promptGo = new GameObject("PromptMesa");
        promptGo.transform.SetParent(go.transform, false);
        promptGo.transform.localPosition = new Vector3(0f, 2.0f, 0f);
        TextMesh tm = promptGo.AddComponent<TextMesh>();
        tm.text = "[E] Tomar Veladora Bendita";
        tm.fontSize = 68;
        tm.characterSize = 0.045f;
        tm.fontStyle = FontStyle.Bold;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1.0f, 0.95f, 0.45f);
        MeshRenderer mr = promptGo.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;
        promptGo.SetActive(false);

        CandleStand cs = go.AddComponent<CandleStand>();
        SerializedObject so = new SerializedObject(cs);
        so.FindProperty("interactionRadius").floatValue = 2.4f;
        so.FindProperty("maxCandlesAllowed").intValue = 3;
        so.FindProperty("promptContainer").objectReferenceValue = promptGo;
        so.FindProperty("promptTextMesh").objectReferenceValue = tm;
        if (takeSfx != null) so.FindProperty("takeCandleSound").objectReferenceValue = takeSfx;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, "Mesa_Cirios_Benditos");
    }

    static void CreateParroquiaEntranceGoalWithSeal(Transform parent, float x, float y, int requiredShrines, Sprite selloSprite, Sprite openDoorsSprite, AudioClip openDoorsSfx = null, AudioClip bellsSfx = null)
    {
        GameObject go = new GameObject("Puertas_Parroquia_Meta");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        // Collider de victoria en la puerta de madera (inicia desactivado hasta consagrar las 4 ofrendas)
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(2.6f, 1.2f);
        bc.offset = new Vector2(0f, 0.6f);
        bc.enabled = false;
        go.AddComponent<LevelGoal>();

        // Niebla espectral de sellado
        GameObject sealMistGo = new GameObject("Aura_Sello_Espectral");
        sealMistGo.transform.SetParent(go.transform, false);
        sealMistGo.transform.localPosition = new Vector3(0f, 0.7f, 0f);

        if (selloSprite != null)
        {
            SpriteRenderer srSeal = sealMistGo.AddComponent<SpriteRenderer>();
            srSeal.sprite = selloSprite;
            srSeal.sortingOrder = 5;
            srSeal.color = new Color(0.95f, 0.88f, 1.0f, 0.90f);
            sealMistGo.transform.localScale = new Vector3(0.40f, 0.40f, 1f);
        }

        Light2D l2dSeal = sealMistGo.AddComponent<Light2D>();
        l2dSeal.lightType = Light2D.LightType.Point;
        l2dSeal.color = new Color(0.65f, 0.25f, 0.85f);
        l2dSeal.intensity = 2.2f;
        l2dSeal.pointLightOuterRadius = 3.8f;
        sealMistGo.AddComponent<LightFlicker>();

        // Visual de puertas abiertas e interior sagrado iluminado (inicia desactivado hasta consagrar 4/4 nichos)
        GameObject openDoorsGo = new GameObject("Entrada_Puertas_Abiertas");
        openDoorsGo.transform.SetParent(go.transform, false);
        openDoorsGo.transform.localPosition = Vector3.zero;

        if (openDoorsSprite != null)
        {
            SpriteRenderer srOpen = openDoorsGo.AddComponent<SpriteRenderer>();
            srOpen.sprite = openDoorsSprite;
            srOpen.sortingOrder = 4;
            openDoorsGo.transform.localScale = new Vector3(0.50f, 0.50f, 1f);
        }
        openDoorsGo.SetActive(false);

        // Luz divina del santuario (dorada/cálida, se enciende al abrir)
        GameObject lightGo = new GameObject("Luz_Entrada_Parroquia");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.88f, 0.48f);
        l2d.intensity = 2.8f;
        l2d.pointLightOuterRadius = 7.0f;
        lightGo.AddComponent<LightFlicker>();
        l2d.enabled = false;

        // Letrero de estado de la puerta con TextMesh HD (fontSize 80, characterSize 0.045f)
        GameObject labelGo = new GameObject("LabelPuerta");
        labelGo.transform.SetParent(go.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        TextMesh tm = labelGo.AddComponent<TextMesh>();
        tm.text = $"PUERTAS SELLADAS\n(Ofrendas: 0/{requiredShrines})";
        tm.fontSize = 80;
        tm.characterSize = 0.045f;
        tm.fontStyle = FontStyle.Bold;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.95f, 0.60f, 0.85f);
        MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;

        // Controlador de sellado y apertura
        ParroquiaDoorController pdc = go.AddComponent<ParroquiaDoorController>();
        SerializedObject so = new SerializedObject(pdc);
        so.FindProperty("requiredShrines").intValue = requiredShrines;
        so.FindProperty("doorSealVisual").objectReferenceValue = sealMistGo;
        so.FindProperty("openDoorsVisual").objectReferenceValue = openDoorsGo;
        so.FindProperty("sanctuaryLight").objectReferenceValue = l2d;
        so.FindProperty("goalCollider").objectReferenceValue = bc;
        so.FindProperty("statusLabel").objectReferenceValue = tm;
        if (openDoorsSfx != null) so.FindProperty("openDoorsSound").objectReferenceValue = openDoorsSfx;
        if (bellsSfx != null) so.FindProperty("bellsSound").objectReferenceValue = bellsSfx;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, "Puertas_Parroquia_Meta");
    }

    static TileBase[] LoadTiles()
    {
        TileBase[] tiles = new TileBase[48];
        for (int i = 0; i < 48; i++)
            tiles[i] = AssetDatabase.LoadAssetAtPath<TileBase>($"Assets/Tiles/tiles_{i}.asset");
        return tiles;
    }

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

    static void CreateFencePiece(Transform parent, string name, Sprite sprite, float x, float y, int layer, Vector2 colSize)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.size = colSize;
        bc.offset = new Vector2(0f, 0.6f);
        if (layer >= 0) go.layer = layer;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateStandingLantern(Transform parent, string name, Sprite sprite, float x, float y, int layer)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.radius = 0.45f;
        cc.offset = new Vector2(0f, 0.25f);
        if (layer >= 0) go.layer = layer;

        GameObject lightGo = new GameObject("Luz");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.78f, 0.45f);
        l2d.intensity = 1.9f;
        l2d.pointLightOuterRadius = 6.2f;
        lightGo.AddComponent<LightFlicker>();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateHideSpotOfficial(Transform parent, string name, Sprite sprite, float x, float y, float scale, int hideSpotLayer, int wallsLayer, Vector2 solidColSize, Vector2 solidColOffset, Vector2 trigColSize, Vector2 trigColOffset)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        BoxCollider2D solidCol = go.AddComponent<BoxCollider2D>();
        solidCol.size = solidColSize;
        solidCol.offset = solidColOffset;
        if (wallsLayer >= 0) go.layer = wallsLayer;

        GameObject trigGo = new GameObject("TriggerOcultarse");
        trigGo.transform.SetParent(go.transform, false);
        BoxCollider2D trigCol = trigGo.AddComponent<BoxCollider2D>();
        trigCol.isTrigger = true;
        trigCol.size = trigColSize;
        trigCol.offset = trigColOffset;
        if (hideSpotLayer >= 0) trigGo.layer = hideSpotLayer;

        GameObject hp = new GameObject("HidePoint");
        hp.transform.SetParent(go.transform, false);
        hp.transform.localPosition = new Vector3(0f, solidColOffset.y + 0.15f, 0f);

        HideSpot hs = go.AddComponent<HideSpot>();
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("hidePoint").objectReferenceValue = hp.transform;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateMudPuddleOfficial(Transform parent, Sprite sprite, float x, float y, float scale = 1.0f, AudioClip splashSfx = null)
    {
        GameObject go = new GameObject("Charco_AguaBendita");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        CapsuleCollider2D cc = go.AddComponent<CapsuleCollider2D>();
        cc.direction = CapsuleDirection2D.Horizontal;
        cc.isTrigger = true;
        cc.size = new Vector2(1.1f, 0.55f);
        cc.offset = new Vector2(0f, -0.05f);

        MudPuddle mp = go.AddComponent<MudPuddle>();
        if (splashSfx != null)
        {
            SerializedObject somp = new SerializedObject(mp);
            somp.FindProperty("splashSound").objectReferenceValue = splashSfx;
            somp.ApplyModifiedProperties();
        }
        Undo.RegisterCreatedObjectUndo(go, "Charco_AguaBendita");
    }

    static void CreateCandlePickupOfficial(Transform parent, string name, Sprite sprite, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.55f;
        col.offset = Vector2.zero;

        GameObject lightGo = new GameObject("LuzVela");
        lightGo.transform.SetParent(go.transform, false);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.45f, 0.25f);
        l2d.intensity = 1.2f;
        l2d.pointLightOuterRadius = 3.0f;

        go.AddComponent<Candle>();
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateInvisibleWall(Transform parent, string name, float x, float y, float height, int layer, float width = 1.0f)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        if (layer >= 0) go.layer = layer;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(width, height);
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void EnsureGlobalLight(float intensity)
    {
        GameObject glGo = GameObject.Find("Global Light 2D");
        if (glGo == null) glGo = new GameObject("Global Light 2D");
        Light2D gl = glGo.GetComponent<Light2D>();
        if (gl == null) gl = glGo.AddComponent<Light2D>();
        gl.lightType = Light2D.LightType.Global;
        gl.color = new Color(0.90f, 0.88f, 0.82f);
        gl.intensity = intensity;
    }

    static void SetupPlayer(Vector2 spawnPos, int layer, AudioClip hideInSfx = null, AudioClip reliefSfx = null)
    {
        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            playerGo = prefab != null ? PrefabUtility.InstantiatePrefab(prefab) as GameObject : new GameObject("Player");
            playerGo.tag = "Player";
        }
        playerGo.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
        if (layer >= 0) playerGo.layer = layer;

        PlayerMovement pm = playerGo.GetComponent<PlayerMovement>();
        if (pm == null) pm = playerGo.AddComponent<PlayerMovement>();
        if (playerGo.GetComponent<PlayerAnimation>() == null) playerGo.AddComponent<PlayerAnimation>();
        HideSystem hs = playerGo.GetComponent<HideSystem>();
        if (hs == null) hs = playerGo.AddComponent<HideSystem>();

        if (hideInSfx != null || reliefSfx != null)
        {
            SerializedObject soHs = new SerializedObject(hs);
            if (hideInSfx != null) soHs.FindProperty("hideInSound").objectReferenceValue = hideInSfx;
            if (reliefSfx != null) soHs.FindProperty("reliefBreathSound").objectReferenceValue = reliefSfx;
            soHs.ApplyModifiedProperties();
        }

        SpriteRenderer sr = playerGo.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 5;

        CircleCollider2D col = playerGo.GetComponent<CircleCollider2D>();
        if (col == null) col = playerGo.AddComponent<CircleCollider2D>();
        col.radius = 0.28f;
        col.offset = new Vector2(0f, -0.45f);

        Rigidbody2D rb = playerGo.GetComponent<Rigidbody2D>();
        if (rb == null) rb = playerGo.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Candle.ResetInventory();
    }

    static void SetupLloronaCircular(Vector2 spawnPos, Transform[] wps, int wallsLayer, int lloronaLayer,
        float patrolSpeed, float chaseSpeed, float visionDist, float fov, float hearDist,
        float waitTime, float loseSightTime, float runNoiseMultiplier,
        AudioClip screamClip = null, AudioClip cryClip = null)
    {
        GameObject lloronaGo = GameObject.Find("Llorona");
        if (lloronaGo == null)
        {
            var aiEx = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
            if (aiEx != null) lloronaGo = aiEx.gameObject;
        }
        if (lloronaGo == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Llorona.prefab");
            lloronaGo = prefab != null ? PrefabUtility.InstantiatePrefab(prefab) as GameObject : new GameObject("Llorona");
        }
        lloronaGo.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
        if (lloronaLayer >= 0) lloronaGo.layer = lloronaLayer;

        LloronaAI ai = lloronaGo.GetComponent<LloronaAI>();
        if (ai == null) ai = lloronaGo.AddComponent<LloronaAI>();
        ai.Waypoints = wps;

        // Configurar ScriptableObject específico del Nivel 3 para dificultad y persistencia
        var settingsAsset = AssetDatabase.LoadAssetAtPath<LloronaSettings>("Assets/Settings/LloronaSettings_Level3.asset");
        if (settingsAsset != null)
        {
            SerializedObject soSet = new SerializedObject(settingsAsset);
            soSet.FindProperty("patrolSpeed").floatValue = patrolSpeed;
            soSet.FindProperty("chaseSpeed").floatValue = chaseSpeed;
            soSet.FindProperty("viewDistance").floatValue = visionDist;
            soSet.FindProperty("viewAngle").floatValue = fov;
            soSet.FindProperty("hearDistance").floatValue = hearDist;
            soSet.FindProperty("waitTime").floatValue = waitTime;
            soSet.FindProperty("loseSightTime").floatValue = loseSightTime;
            soSet.FindProperty("runNoiseMultiplier").floatValue = runNoiseMultiplier;
            soSet.ApplyModifiedProperties();
            EditorUtility.SetDirty(settingsAsset);
        }

        SerializedObject so = new SerializedObject(ai);
        var sProp = so.FindProperty("settings");
        if (sProp != null && settingsAsset != null) sProp.objectReferenceValue = settingsAsset;

        var pSpeed = so.FindProperty("patrolSpeed");
        if (pSpeed != null) pSpeed.floatValue = patrolSpeed;
        var cSpeed = so.FindProperty("chaseSpeed");
        if (cSpeed != null) cSpeed.floatValue = chaseSpeed;
        var vDist = so.FindProperty("viewDistance");
        if (vDist != null) vDist.floatValue = visionDist;
        var vAngle = so.FindProperty("viewAngle");
        if (vAngle != null) vAngle.floatValue = fov;
        var hDist = so.FindProperty("hearDistance");
        if (hDist != null) hDist.floatValue = hearDist;
        var wTime = so.FindProperty("waitTime");
        if (wTime != null) wTime.floatValue = waitTime;
        var lsTime = so.FindProperty("loseSightTime");
        if (lsTime != null) lsTime.floatValue = loseSightTime;
        var rNoise = so.FindProperty("runNoiseMultiplier");
        if (rNoise != null) rNoise.floatValue = runNoiseMultiplier;

        var oMask = so.FindProperty("obstacleMask");
        if (oMask != null) oMask.intValue = 1 << wallsLayer;
        if (screamClip != null)
        {
            var p = so.FindProperty("screamSfx");
            if (p != null) p.objectReferenceValue = screamClip;
        }
        if (cryClip != null)
        {
            var p = so.FindProperty("cryLoopClip");
            if (p != null) p.objectReferenceValue = cryClip;
        }
        so.ApplyModifiedProperties();
    }

    static void CreateTutorialTrigger(Transform parent, string name, Vector2 pos, Vector2 size, string text, float duration = 5.0f, bool showOnStart = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = size;

        TutorialTrigger tt = go.AddComponent<TutorialTrigger>();
        SerializedObject so = new SerializedObject(tt);
        var msgProp = so.FindProperty("tutorialMessage");
        if (msgProp != null) msgProp.stringValue = text;
        var durProp = so.FindProperty("displayDuration");
        if (durProp != null) durProp.floatValue = duration;
        var startProp = so.FindProperty("showOnStart");
        if (startProp != null) startProp.boolValue = showOnStart;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void SetupGameManager(int levelNumber, AudioClip sfxCampanas = null, AudioClip sfxPrenderVela = null, AudioClip sfxPuertaConfesionario = null, AudioClip sfxLloronaGrito = null, AudioClip sfxLloronaLlanto = null, AudioClip sfxUiClickStone = null)
    {
        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject gmGo = new GameObject("GameManager");
            gm = gmGo.AddComponent<GameManager>();
        }
        gm.SetLevelNumber(levelNumber);
        SerializedObject so = new SerializedObject(gm);
        var prop = so.FindProperty("currentLevelNumber");
        if (prop != null) prop.intValue = levelNumber;
        so.ApplyModifiedProperties();

        AudioManager am = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
        if (am == null)
        {
            GameObject amGo = new GameObject("AudioManager");
            am = amGo.AddComponent<AudioManager>();
        }
        if (sfxCampanas != null) am.campanaLejana = sfxCampanas;
        if (sfxPrenderVela != null) am.vela = sfxPrenderVela;
        if (sfxPuertaConfesionario != null) am.escondite = sfxPuertaConfesionario;
        if (sfxLloronaGrito != null) am.lloronaGrito = sfxLloronaGrito;
        if (sfxLloronaLlanto != null) am.lloronaLlanto = sfxLloronaLlanto;
        if (sfxUiClickStone != null) am.clicBoton = sfxUiClickStone;
    }

    static Transform CreateEmpty(Transform parent, string name, float x, float y)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        Undo.RegisterCreatedObjectUndo(go, name);
        return go.transform;
    }
}
