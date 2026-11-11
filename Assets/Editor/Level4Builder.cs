// Assets/Editor/Level4Builder.cs
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Generador del Nivel 4: "El Panteón de las Ánimas" (BOSS FIGHT FINAL).
/// - Transforma el nivel 4 en el gran enfrentamiento contra La Llorona.
/// - Sistema de combate: Carlos lanza Jarros de Agua Bendita con [Clic] o [F].
/// - 3 Pilas de cantera para recargar agua bendita [E].
/// - Barra de Vida de Jefe superior dividida en 3 Fases (LloronaBossAI).
/// - Al derrotarla, se purifica en luz celestial y se abre la Gran Reja hacia la libertad.
/// </summary>
public static class Level4Builder
{
    const string ScenePath = "Assets/Scenes/Level4_Cementerio.unity";
    const string SpritesDir = "Assets/Sprites/Environment/Nivel 4/Sprites";

    [MenuItem("Llorona/Niveles/Construir Nivel 4 - Boss Fight (Climax)")]
    public static void BuildLevel4Scene()
    {
        Debug.Log("[Level4Builder] === INICIANDO CONSTRUCCIÓN DEL NIVEL 4 (BOSS FIGHT CLÍMAX) ===");

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        CleanOldSceneGarbage(scene);

        // Layers
        int wallsLayer = LayerMask.NameToLayer("Walls");
        if (wallsLayer < 0) wallsLayer = LayerMask.NameToLayer("Default");
        int hideSpotLayer = LayerMask.NameToLayer("HideSpot");
        if (hideSpotLayer < 0) hideSpotLayer = LayerMask.NameToLayer("Default");
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer < 0) playerLayer = LayerMask.NameToLayer("Default");
        int lloronaLayer = LayerMask.NameToLayer("Enemy");
        if (lloronaLayer < 0) lloronaLayer = LayerMask.NameToLayer("Default");

        // ----------------------------------------------------
        // 1. CARGA DE SPRITES DE AMBIENTACIÓN
        // ----------------------------------------------------
        Sprite[] terrainTiles = new Sprite[12];
        for (int i = 0; i < 12; i++)
        {
            terrainTiles[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Tile_Panteon_{i:D2}.png");
        }

        Sprite mausoleoGrande   = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Mausoleo_Grande_Guanajuato.png");
        Sprite mausoleoMedio    = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Mausoleo_Capilla_Medio.png");
        Sprite mausoleoCripta   = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Mausoleo_Cripta_Piedra.png");
        Sprite cabanaSepul      = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Cabana_Sepulturero.png");
        Sprite rejaSalida       = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Reja_Gotica_Salida.png");

        Sprite sarcofagoDoble   = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Sarcofago_Doble_Cantera.png");
        Sprite sarcofagoCruz    = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Sarcofago_Colonial_Cruz.png");
        Sprite tumbaOrnamental  = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Tumba_Ornamental.png");
        Sprite tumbaCruz        = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Tumba_Cruz_Cantera.png");
        Sprite tumbaFosa        = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Tumba_Abierta_Fosa.png");
        Sprite lapidaCantera    = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Lapida_Colonial_Cantera.png");
        Sprite obelisco         = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Obelisco_Funerario.png");
        Sprite angelDoliente    = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Angel_Doliente_Estatua.png");
        Sprite criptaPared      = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Cripta_Pared_Colonial.png");
        Sprite pilarCantera     = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Pilar_Cantera_Panteon.png");
        Sprite muroVerja        = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Muro_Panteon_Verja.png");

        Sprite altarMagno       = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Altar_Ofrenda_Magno.png");
        Sprite altarArco        = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Altar_Arco_Cempasuchil.png");
        Sprite ofrendaColumna   = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Ofrenda_Columna_Flores.png");
        Sprite ofrendaMesa      = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Ofrenda_Mesa_Calaveritas.png");
        Sprite velaSprite       = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Veladoras_Cempasuchil.png");
        Sprite tapeteFlores     = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Tapete_Cempasuchil_Suelo.png");

        Sprite arbolSeco        = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Arbol_Seco_Retorcido.png");
        Sprite sauceFunerario   = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Sauce_Funerario.png");
        Sprite cipresAlto       = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Cipres_Panteon_Alto.png");
        Sprite matorral1        = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Matorral_Espinoso_1.png");
        Sprite matorral2        = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Matorral_Espinoso_2.png");
        Sprite nieblaSprite     = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/Textura_Niebla_Atmocion.png");

        Sprite bottleSprite     = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Botellas_charcolodo_Nvl1.png")
            .OfType<Sprite>().FirstOrDefault(s => s.name.StartsWith("Botella"))
            ?? velaSprite;

        // SFX de clímax y combate
        AudioClip campanaSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_campanas_catedral.wav");
        AudioClip portalSfx  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_portal_apertura.wav");
        AudioClip screamSfx  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_llorona_grito_detect.wav")
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/llorona_grito.wav");
        AudioClip ambientSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_llorona_lamento_loop.wav");
        AudioClip holySfx    = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_nicho_consagrado.wav");
        AudioClip refillSfx  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_tomar_vela.wav");
        AudioClip emptySfx   = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_ui_click_stone.wav");
        AudioClip throwSfx   = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_tepalcates_crunch.wav");

        // Raíz del entorno
        GameObject rootEnv = GameObject.Find("Escenario_Panteon_Oficial");
        if (rootEnv == null) rootEnv = new GameObject("Escenario_Panteon_Oficial");

        // ----------------------------------------------------
        // 2. MOSAICO DE SUELO MODULAR (46 x 28)
        // ----------------------------------------------------
        GameObject groundGroup = GetOrCreateChild(rootEnv, "Suelo_Modular_Cantera");
        int widthTiles = 46;
        int heightTiles = 28;
        for (int x = 0; x < widthTiles; x++)
        {
            for (int y = 0; y < heightTiles; y++)
            {
                int tileIdx = 0;
                int hash = (x * 73856093) ^ (y * 19349663);
                int rnd = Mathf.Abs(hash) % 100;

                if (rnd < 45) tileIdx = 0;
                else if (rnd < 65) tileIdx = 1;
                else if (rnd < 80) tileIdx = 2;
                else if (rnd < 90) tileIdx = 3;
                else if (rnd < 96) tileIdx = 4;
                else tileIdx = 5;

                Sprite chosenTile = terrainTiles[tileIdx] ?? terrainTiles[0];
                if (chosenTile != null)
                {
                    CreatePiece(groundGroup, $"Tile_{x}_{y}", new Vector3(x + 0.5f, y + 0.5f, 0f), chosenTile, "Default", 0, Vector3.one);
                }
            }
        }

        // Tapetes de Cempasúchil en entradas y senderos principales
        if (tapeteFlores != null)
        {
            GameObject matsGroup = GetOrCreateChild(rootEnv, "Senderos_Cempasuchil");
            CreatePiece(matsGroup, "Tapete_Entrada", new Vector3(4.5f, 14.0f, 0f), tapeteFlores, "Props", 1, new Vector3(1.1f, 1.1f, 1f));
            CreatePiece(matsGroup, "Tapete_Sendero_1", new Vector3(14.0f, 14.0f, 0f), tapeteFlores, "Props", 1, new Vector3(1.0f, 1.0f, 1f));
            CreatePiece(matsGroup, "Tapete_Sendero_2", new Vector3(28.0f, 14.0f, 0f), tapeteFlores, "Props", 1, new Vector3(1.2f, 1.2f, 1f));
            CreatePiece(matsGroup, "Tapete_Salida", new Vector3(41.0f, 14.0f, 0f), tapeteFlores, "Props", 1, new Vector3(1.1f, 1.1f, 1f));
        }

        // ----------------------------------------------------
        // 3. MUROS PERIMETRALES Y VERJAS
        // ----------------------------------------------------
        GameObject boundsGroup = GetOrCreateChild(rootEnv, "Limites_Perimetrales");
        GameObject wallsGroup  = GetOrCreateChild(rootEnv, "Muros_Verjas_Visuales");

        // Muro Norte (Y = 27.5)
        CreateSolidBarrier(boundsGroup, "Col_Muro_Norte", new Vector2(23.0f, 27.5f), new Vector2(46.0f, 1.2f), wallsLayer);
        for (float x = 1.5f; x <= 44.5f; x += 3.2f)
        {
            CreatePiece(wallsGroup, $"Verja_Norte_{x:F0}", new Vector3(x, 27.0f, 0f), muroVerja, "Default", 2, new Vector3(0.85f, 0.85f, 1f));
        }

        // Muro Sur (Y = 0.5)
        CreateSolidBarrier(boundsGroup, "Col_Muro_Sur", new Vector2(23.0f, 0.5f), new Vector2(46.0f, 1.2f), wallsLayer);
        for (float x = 1.5f; x <= 44.5f; x += 3.2f)
        {
            CreatePiece(wallsGroup, $"Verja_Sur_{x:F0}", new Vector3(x, 1.0f, 0f), muroVerja, "Default", 2, new Vector3(0.85f, 0.85f, 1f));
        }

        // Muro Oeste (Entrada sellada, X = 0.8, Y: 0 a 28)
        CreateSolidBarrier(boundsGroup, "Col_Muro_Oeste", new Vector2(0.8f, 14.0f), new Vector2(1.2f, 28.0f), wallsLayer);
        for (float y = 2.5f; y <= 26.0f; y += 4.5f)
        {
            CreatePiece(wallsGroup, $"Pilar_Oeste_{y:F0}", new Vector3(1.0f, y, 0f), pilarCantera, "Default", 2, new Vector3(0.70f, 0.70f, 1f));
        }

        // Muro Este (Salida a casa con apertura para la Gran Reja Gótica entre Y=11.5 e Y=16.5)
        CreateSolidBarrier(boundsGroup, "Col_Muro_Este_Norte", new Vector2(45.2f, 22.0f), new Vector2(1.2f, 11.0f), wallsLayer);
        CreateSolidBarrier(boundsGroup, "Col_Muro_Este_Sur",   new Vector2(45.2f,  6.0f), new Vector2(1.2f, 11.0f), wallsLayer);
        for (float y = 17.5f; y <= 26.0f; y += 4.5f)
        {
            CreatePiece(wallsGroup, $"Pilar_Este_N_{y:F0}", new Vector3(45.0f, y, 0f), pilarCantera, "Default", 2, new Vector3(0.70f, 0.70f, 1f));
        }
        for (float y = 2.5f; y <= 10.5f; y += 4.5f)
        {
            CreatePiece(wallsGroup, $"Pilar_Este_S_{y:F0}", new Vector3(45.0f, y, 0f), pilarCantera, "Default", 2, new Vector3(0.70f, 0.70f, 1f));
        }

        // ----------------------------------------------------
        // 4. MUROS INTERIORES DEL LABERINTO (PASILLOS AMPLIOS DE 7 METROS)
        // ----------------------------------------------------
        CreateSolidBarrier(boundsGroup, "Col_Interior_Oeste_Norte", new Vector2(11.0f, 22.0f), new Vector2(1.0f, 9.0f), wallsLayer);
        CreateSolidBarrier(boundsGroup, "Col_Interior_Oeste_Sur",   new Vector2(11.0f,  6.0f), new Vector2(1.0f, 9.0f), wallsLayer);
        for (float y = 18.0f; y <= 25.5f; y += 3.0f)
        {
            CreatePiece(wallsGroup, $"Verja_Int_ON_{y:F0}", new Vector3(11.0f, y, 0f), muroVerja, "Default", 2, new Vector3(0.75f, 0.75f, 1f));
        }
        for (float y = 2.0f; y <= 9.5f; y += 3.0f)
        {
            CreatePiece(wallsGroup, $"Verja_Int_OS_{y:F0}", new Vector3(11.0f, y, 0f), muroVerja, "Default", 2, new Vector3(0.75f, 0.75f, 1f));
        }

        CreateSolidBarrier(boundsGroup, "Col_Interior_Este_Norte", new Vector2(33.5f, 22.0f), new Vector2(1.0f, 9.0f), wallsLayer);
        CreateSolidBarrier(boundsGroup, "Col_Interior_Este_Sur",   new Vector2(33.5f,  6.0f), new Vector2(1.0f, 9.0f), wallsLayer);
        for (float y = 18.0f; y <= 25.5f; y += 3.0f)
        {
            CreatePiece(wallsGroup, $"Verja_Int_EN_{y:F0}", new Vector3(33.5f, y, 0f), muroVerja, "Default", 2, new Vector3(0.75f, 0.75f, 1f));
        }
        for (float y = 2.0f; y <= 9.5f; y += 3.0f)
        {
            CreatePiece(wallsGroup, $"Verja_Int_ES_{y:F0}", new Vector3(33.5f, y, 0f), muroVerja, "Default", 2, new Vector3(0.75f, 0.75f, 1f));
        }

        // ----------------------------------------------------
        // 5. MAUSOLEOS MONUMENTALES Y CRIPTAS (COLISIÓN EXCLUSIVA EN BASE)
        // ----------------------------------------------------
        GameObject mausoleosGroup = GetOrCreateChild(rootEnv, "Mausoleos_Labyrinth");
        CreateMausoleum(mausoleosGroup, "Mausoleo_Grande_Norte", new Vector3(17.0f, 19.5f, 0f), mausoleoGrande, new Vector3(0.65f, 0.65f, 1f), new Vector2(3.2f, 1.4f), -1.6f, wallsLayer);
        CreateMausoleum(mausoleosGroup, "Cripta_Pared_Norte", new Vector3(23.5f, 20.0f, 0f), criptaPared, new Vector3(0.80f, 0.80f, 1f), new Vector2(2.4f, 1.2f), -1.0f, wallsLayer);
        CreateMausoleum(mausoleosGroup, "Mausoleo_Capilla_Noreste", new Vector3(29.5f, 20.5f, 0f), mausoleoMedio, new Vector3(0.75f, 0.75f, 1f), new Vector2(2.4f, 1.3f), -1.3f, wallsLayer);
        CreateMausoleum(mausoleosGroup, "Mausoleo_Cripta_Suroeste", new Vector3(16.5f, 7.5f, 0f), mausoleoCripta, new Vector3(0.75f, 0.75f, 1f), new Vector2(2.2f, 1.3f), -1.1f, wallsLayer);
        CreateMausoleum(mausoleosGroup, "Mausoleo_Capilla_Sur", new Vector3(23.0f, 7.5f, 0f), mausoleoMedio, new Vector3(0.75f, 0.75f, 1f), new Vector2(2.4f, 1.3f), -1.3f, wallsLayer);
        CreateMausoleum(mausoleosGroup, "Mausoleo_Cripta_Sureste", new Vector3(29.5f, 7.5f, 0f), mausoleoCripta, new Vector3(0.75f, 0.75f, 1f), new Vector2(2.2f, 1.3f), -1.1f, wallsLayer);

        // ----------------------------------------------------
        // 6. ESCONDITES DE SIGILO (BASE SÓLIDA + TRIGGER [E])
        // ----------------------------------------------------
        GameObject stealthGroup = GetOrCreateChild(rootEnv, "Escondites_Sigilo");
        CreateSarcophagusHideSpot(stealthGroup, "Sarcofago_Inicio", new Vector3(8.0f, 12.5f, 0f), sarcofagoCruz, new Vector3(0.75f, 0.75f, 1f), hideSpotLayer, wallsLayer);
        CreateSarcophagusHideSpot(stealthGroup, "Sarcofago_Laberinto_Central", new Vector3(19.5f, 11.8f, 0f), sarcofagoDoble, new Vector3(0.75f, 0.75f, 1f), hideSpotLayer, wallsLayer);
        CreateSarcophagusHideSpot(stealthGroup, "Sarcofago_Criptas_Norte", new Vector3(26.5f, 25.5f, 0f), sarcofagoCruz, new Vector3(0.75f, 0.75f, 1f), hideSpotLayer, wallsLayer);
        CreateSarcophagusHideSpot(stealthGroup, "Tumba_Escondite_Sur", new Vector3(26.0f, 3.8f, 0f), tumbaOrnamental, new Vector3(0.75f, 0.75f, 1f), hideSpotLayer, wallsLayer);
        CreateCabinHideSpot(stealthGroup, "Cabana_Sepulturero", new Vector3(38.5f, 19.5f, 0f), cabanaSepul, new Vector3(0.60f, 0.60f, 1f), wallsLayer, hideSpotLayer);

        // ----------------------------------------------------
        // 7. PILAS DE AGUA BENDITA (ESTACIONES DE RECARGA PARA EL BOSS FIGHT)
        // ----------------------------------------------------
        GameObject holySupplyGroup = GetOrCreateChild(rootEnv, "Pilas_Agua_Bendita");
        CreateHolyWaterSupplyStation(holySupplyGroup, "Pila_Agua_Oeste", new Vector3( 7.5f, 14.0f, 0f), pilarCantera, refillSfx, wallsLayer);
        CreateHolyWaterSupplyStation(holySupplyGroup, "Pila_Agua_Norte", new Vector3(21.0f, 21.0f, 0f), pilarCantera, refillSfx, wallsLayer);
        CreateHolyWaterSupplyStation(holySupplyGroup, "Pila_Agua_Sur",   new Vector3(20.5f,  6.8f, 0f), pilarCantera, refillSfx, wallsLayer);

        // ----------------------------------------------------
        // 8. ALTARES Y LA GRAN TUMBA SAGRADA DE LA LLORONA
        // ----------------------------------------------------
        GameObject shrinesGroup = GetOrCreateChild(rootEnv, "Altares_Cempasuchil");
        CreateShrineAltar(shrinesGroup, "Altar_Arco_Oeste", new Vector3(6.5f, 20.0f, 0f), altarArco, "Altar de la Piedad", new Vector3(0.85f, 0.85f, 1f), wallsLayer);
        CreateShrineAltar(shrinesGroup, "Altar_Mesa_Sur", new Vector3(22.5f, 3.5f, 0f), ofrendaMesa, "Ofrenda de las Ánimas", new Vector3(0.90f, 0.90f, 1f), wallsLayer);

        // Gran Tumba Olvidada de La Llorona (Ofrenda final del ritual para abrir la Gran Reja)
        CreateLloronaGrave(
            shrinesGroup,
            new Vector3(28.0f, 14.0f, 0f),
            altarMagno,
            angelDoliente,
            tapeteFlores,
            velaSprite,
            holySfx,
            campanaSfx,
            portalSfx,
            wallsLayer);

        // Ítem interactivo para buscar en el panteón: Ofrenda Sagrada de Cempasúchil y Veladoras
        CreateSacredOfferingPickupItem(shrinesGroup, new Vector3(22.5f, 4.3f, 0f), velaSprite, refillSfx);

        if (ofrendaColumna != null)
        {
            CreatePiece(shrinesGroup, "Columna_Flores_Izq", new Vector3(25.5f, 15.8f, 0f), ofrendaColumna, "Props", 3, new Vector3(0.8f, 0.8f, 1f));
            CreatePiece(shrinesGroup, "Columna_Flores_Der", new Vector3(30.5f, 15.8f, 0f), ofrendaColumna, "Props", 3, new Vector3(0.8f, 0.8f, 1f));
        }

        // ----------------------------------------------------
        // 9. NATURALEZA TÉTRICA Y ÁRBOLES FUNERARIOS
        // ----------------------------------------------------
        GameObject treesGroup = GetOrCreateChild(rootEnv, "Naturaleza_Funeraria");
        CreatePieceWithCollider(treesGroup, "Arbol_Seco_Oeste", new Vector3( 4.0f, 23.5f, 0f), arbolSeco, "Props", 10, new Vector3(0.75f, 0.75f, 1f), new Vector2(1.0f, 0.8f), -0.5f, wallsLayer);
        CreatePieceWithCollider(treesGroup, "Arbol_Seco_Este",  new Vector3(36.0f,  6.5f, 0f), arbolSeco, "Props", 10, new Vector3(0.70f, 0.70f, 1f), new Vector2(1.0f, 0.8f), -0.5f, wallsLayer);
        CreatePieceWithCollider(treesGroup, "Sauce_Oeste",      new Vector3( 8.5f,  7.0f, 0f), sauceFunerario, "Props", 10, new Vector3(0.75f, 0.75f, 1f), new Vector2(1.0f, 0.8f), -0.5f, wallsLayer);
        CreatePieceWithCollider(treesGroup, "Sauce_Noreste",    new Vector3(32.0f, 22.5f, 0f), sauceFunerario, "Props", 10, new Vector3(0.75f, 0.75f, 1f), new Vector2(1.0f, 0.8f), -0.5f, wallsLayer);

        CreatePiece(treesGroup, "Cipres_1", new Vector3( 3.0f, 19.5f, 0f), cipresAlto, "Props", 4, new Vector3(0.85f, 0.85f, 1f));
        CreatePiece(treesGroup, "Cipres_2", new Vector3(42.5f,  8.5f, 0f), cipresAlto, "Props", 4, new Vector3(0.85f, 0.85f, 1f));
        CreatePiece(treesGroup, "Cipres_3", new Vector3(42.5f, 20.0f, 0f), cipresAlto, "Props", 4, new Vector3(0.85f, 0.85f, 1f));

        CreatePiece(treesGroup, "Zarza_1", new Vector3( 9.0f,  4.5f, 0f), matorral1, "Props", 2, new Vector3(0.65f, 0.65f, 1f));
        CreatePiece(treesGroup, "Zarza_2", new Vector3(25.0f, 25.5f, 0f), matorral2, "Props", 2, new Vector3(0.65f, 0.65f, 1f));
        CreatePiece(treesGroup, "Zarza_3", new Vector3(38.0f,  9.5f, 0f), matorral1, "Props", 2, new Vector3(0.65f, 0.65f, 1f));

        // ----------------------------------------------------
        // 10. LÁPIDAS, CRUCES Y FOSAS
        // ----------------------------------------------------
        GameObject tombsGroup = GetOrCreateChild(rootEnv, "Tumbas_Lapidas_Cantera");
        Vector2[] lapidasP1 = new Vector2[]
        {
            new Vector2( 5.0f, 11.0f), new Vector2( 7.0f, 11.0f),
            new Vector2( 5.0f,  5.5f), new Vector2( 7.0f,  5.5f),
            new Vector2(18.5f, 25.8f), new Vector2(23.5f, 25.8f),
            new Vector2(36.5f, 16.8f), new Vector2(36.5f, 11.2f)
        };
        for (int i = 0; i < lapidasP1.Length; i++)
        {
            Sprite s = (i % 2 == 0) ? tumbaCruz : lapidaCantera;
            CreateSolidProp(tombsGroup, $"Lapida_{i+1}", lapidasP1[i], s, new Vector3(0.65f, 0.65f, 1f), new Vector2(0.6f, 0.4f), -0.2f, wallsLayer);
        }

        CreateSolidProp(tombsGroup, "Angel_Doliente_Oeste", new Vector3(13.5f, 16.5f, 0f), angelDoliente, new Vector3(0.70f, 0.70f, 1f), new Vector2(0.8f, 0.6f), -0.4f, wallsLayer);
        CreateSolidProp(tombsGroup, "Angel_Doliente_Este",  new Vector3(31.5f, 16.5f, 0f), angelDoliente, new Vector3(0.70f, 0.70f, 1f), new Vector2(0.8f, 0.6f), -0.4f, wallsLayer);
        CreateSolidProp(tombsGroup, "Obelisco_Sur",   new Vector3( 9.0f,  9.5f, 0f), obelisco, new Vector3(0.70f, 0.70f, 1f), new Vector2(0.6f, 0.6f), -0.5f, wallsLayer);
        CreateSolidProp(tombsGroup, "Obelisco_Norte", new Vector3(21.0f, 25.8f, 0f), obelisco, new Vector3(0.70f, 0.70f, 1f), new Vector2(0.6f, 0.6f), -0.5f, wallsLayer);

        CreateSolidProp(tombsGroup, "Fosa_Abierta_1", new Vector2(18.0f, 2.6f), tumbaFosa, new Vector3(0.75f, 0.75f, 1f), new Vector2(1.5f, 0.8f), 0f, wallsLayer);
        CreateSolidProp(tombsGroup, "Fosa_Abierta_2", new Vector2(30.0f, 2.6f), tumbaFosa, new Vector3(0.75f, 0.75f, 1f), new Vector2(1.5f, 0.8f), 0f, wallsLayer);

        // ----------------------------------------------------
        // 11. PELIGROS AMBIENTALES
        // ----------------------------------------------------
        GameObject hazardsGroup = GetOrCreateChild(rootEnv, "Peligros_Ambientales");
        CreateMudPuddle(hazardsGroup, 13.0f, 12.0f, 1.2f);
        CreateMudPuddle(hazardsGroup, 23.5f, 12.0f, 1.2f);
        CreateMudPuddle(hazardsGroup, 34.0f, 12.0f, 1.2f);
        CreateMudPuddle(hazardsGroup, 18.5f,  3.8f, 1.1f);

        // ----------------------------------------------------
        // 12. ESPECTROS MENORES DE LA FASE 2 (DURMIENTES)
        // ----------------------------------------------------
        GameObject spectersGroup = GetOrCreateChild(rootEnv, "Amenazas_Espectros");
        GameObject specter1 = CreateCemeterySpecter(spectersGroup, "Espectro_Anima_1", new Vector3(16.0f, 10.5f, 0f), 13.0f, 28.0f, 1.45f, screamSfx, ambientSfx);
        GameObject specter2 = CreateCemeterySpecter(spectersGroup, "Espectro_Anima_2", new Vector3(25.0f, 24.0f, 0f), 15.0f, 32.0f, 1.55f, screamSfx, ambientSfx);
        specter1.SetActive(false);
        specter2.SetActive(false);

        // ----------------------------------------------------
        // 13. PORTAL MONUMENTAL DE SALIDA SELLADO (GRAN REJA GÓTICA)
        // ----------------------------------------------------
        GameObject gateObj;
        Collider2D exitTrigger;
        Collider2D gateSolid;
        Light2D gateLight;
        TextMesh gateLabel;
        CreateExitGate(rootEnv, new Vector3(44.2f, 14.0f, 0f), rejaSalida, wallsLayer,
            out gateObj, out exitTrigger, out gateSolid, out gateLight, out gateLabel);

        // ----------------------------------------------------
        // 14. NIEBLA AMBIENTAL AZUL VIOLÁCEA
        // ----------------------------------------------------
        if (nieblaSprite != null)
        {
            GameObject fogRoot = GetOrCreateChild(rootEnv, "Niebla_Atmocion_Panteon");
            for (float fx = 6.0f; fx <= 42.0f; fx += 12.0f)
            {
                for (float fy = 6.0f; fy <= 24.0f; fy += 12.0f)
                {
                    GameObject fogSeg = CreatePiece(fogRoot, $"Niebla_{fx:F0}_{fy:F0}", new Vector3(fx, fy, 0f), nieblaSprite, "Props", 20, new Vector3(1.1f, 1.1f, 1f));
                    SpriteRenderer fsr = fogSeg.GetComponent<SpriteRenderer>();
                    if (fsr != null)
                    {
                        fsr.color = new Color(0.25f, 0.20f, 0.45f, 0.22f);
                    }
                }
            }
        }

        // ----------------------------------------------------
        // 15. ILUMINACIÓN GLOBAL Y FAROLES TENUES (URP 2D)
        // ----------------------------------------------------
        EnsureGlobalLight(0.34f);
        Light2D globalLight = GameObject.Find("Global Light 2D")?.GetComponent<Light2D>();

        GameObject lightsRoot = GetOrCreateChild(rootEnv, "Faroles_Iluminacion");
        Vector2[] lanternPositions = new Vector2[]
        {
            new Vector2( 3.5f, 14.0f),
            new Vector2( 9.0f, 19.5f),
            new Vector2(13.5f, 14.0f),
            new Vector2(16.5f, 24.0f),
            new Vector2(16.5f,  4.0f),
            new Vector2(23.0f, 14.0f),
            new Vector2(25.5f, 20.0f),
            new Vector2(28.0f, 16.5f),
            new Vector2(38.0f, 16.5f),
            new Vector2(43.5f, 14.0f)
        };
        for (int i = 0; i < lanternPositions.Length; i++)
        {
            CreateLantern(lightsRoot, $"Farol_Tenue_{i+1}", lanternPositions[i]);
        }

        // ----------------------------------------------------
        // 16. CARLOS (PLAYER) Y SISTEMA DE COMBATE CON AGUA BENDITA
        // ----------------------------------------------------
        GameObject playerObj = SetupPlayer(new Vector2(3.5f, 14.0f), playerLayer);
        SetupPlayerCombat(playerObj, bottleSprite, throwSfx, holySfx, emptySfx);

        // ----------------------------------------------------
        // 17. LA LLORONA (BOSS FIGHT FINAL - 3 FASES)
        // ----------------------------------------------------
        SetupBossLlorona(
            pos:         new Vector2(23.5f, 14.0f),
            sp1:         specter1,
            sp2:         specter2,
            scream:      screamSfx,
            hurt:        screamSfx,
            purified:    portalSfx,
            globalLight: globalLight,
            wallsLayer:  wallsLayer,
            lloronaLayer: lloronaLayer
        );

        // ----------------------------------------------------
        // 18. BARRA DE VIDA DE JEFE EN EL CANVAS
        // ----------------------------------------------------
        Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            CreateBossHealthBarUI(canvas.transform);
        }

        // ----------------------------------------------------
        // 19. TRIGGERS NARRATIVOS Y TUTORIAL DEL BOSS FIGHT
        // ----------------------------------------------------
        GameObject triggersRoot = GetOrCreateChild(rootEnv, "Triggers_Tutorial");
        CreateTutorialTrigger(triggersRoot.transform, "Tut_Inicio", new Vector2(3.5f, 14.0f), new Vector2(4.5f, 6.0f),
            "¡¡EL ENFRENTAMIENTO FINAL!!\n¡Armate de valor y purifica a La Llorona para romper su maldición!\n• [Clic Izquierdo] o [F]: Lanzar Agua Bendita.\n• [E]: Recargar en las Pilas de Cantera.",
            showOnStart: true);

        // ----------------------------------------------------
        // 20. CÁMARA CON SEGUIMIENTO SUAVE
        // ----------------------------------------------------
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5.2f;
        cam.backgroundColor = new Color(0.04f, 0.04f, 0.08f);

        CameraFollow2D camFollow = GetOrAddComponent<CameraFollow2D>(cam.gameObject);
        camFollow.SetBounds(new Vector2(1.5f, 2.0f), new Vector2(44.5f, 26.0f));
        camFollow.SetOrthoSize(5.2f);

        // GameManager y AudioManager nivel 4
        SetupGameManager(4);
        SetupAudioManager();

        // Guardar escena
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Level4Builder] === NIVEL 4 (BOSS FIGHT CLÍMAX) CONSTRUIDO Y GUARDADO EXITOSAMENTE ===");
    }

    // ========================================================
    // CONSTRUCTORES AUXILIARES Y SISTEMA DE BOSS FIGHT
    // ========================================================

    static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }

    static GameObject CreatePiece(GameObject parent, string name, Vector3 pos, Sprite sprite, string sortingLayer, int order, Vector3 scale)
    {
        GameObject obj = GetOrCreateChild(parent, name);
        obj.transform.position = pos;
        obj.transform.localScale = scale;

        if (sprite != null)
        {
            SpriteRenderer sr = GetOrAddComponent<SpriteRenderer>(obj);
            sr.sprite = sprite;
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = order;
        }
        return obj;
    }

    static void CreateSolidBarrier(GameObject parent, string name, Vector2 pos, Vector2 size, int layer)
    {
        GameObject obj = GetOrCreateChild(parent, name);
        obj.transform.position = new Vector3(pos.x, pos.y, 0f);
        if (layer >= 0) obj.layer = layer;

        BoxCollider2D bc = GetOrAddComponent<BoxCollider2D>(obj);
        bc.size = size;
        bc.offset = Vector2.zero;
        bc.isTrigger = false;
    }

    static void CreatePieceWithCollider(GameObject parent, string name, Vector3 pos, Sprite sprite, string sortingLayer, int order, Vector3 scale, Vector2 colSize, float offsetY, int layer)
    {
        GameObject obj = CreatePiece(parent, name, pos, sprite, sortingLayer, order, scale);
        if (layer >= 0) obj.layer = layer;

        BoxCollider2D bc = GetOrAddComponent<BoxCollider2D>(obj);
        bc.size = colSize;
        bc.offset = new Vector2(0f, offsetY);
        bc.isTrigger = false;
    }

    static void CreateMausoleum(GameObject parent, string name, Vector3 pos, Sprite sprite, Vector3 scale, Vector2 baseSize, float offsetY, int layer)
    {
        GameObject obj = CreatePiece(parent, name, pos, sprite, "Props", 4, scale);
        if (layer >= 0) obj.layer = layer;

        BoxCollider2D bc = GetOrAddComponent<BoxCollider2D>(obj);
        bc.size = baseSize;
        bc.offset = new Vector2(0f, offsetY);
        bc.isTrigger = false;
    }

    static void CreateSolidProp(GameObject parent, string name, Vector2 pos, Sprite sprite, Vector3 scale, Vector2 colSize, float offsetY, int layer)
    {
        GameObject obj = CreatePiece(parent, name, new Vector3(pos.x, pos.y, 0f), sprite, "Props", 3, scale);
        if (layer >= 0) obj.layer = layer;

        BoxCollider2D bc = GetOrAddComponent<BoxCollider2D>(obj);
        bc.size = colSize;
        bc.offset = new Vector2(0f, offsetY);
        bc.isTrigger = false;
    }

    static void CreateSarcophagusHideSpot(GameObject parent, string name, Vector3 pos, Sprite sprite, Vector3 scale, int hideSpotLayer, int wallsLayer)
    {
        GameObject obj = CreatePiece(parent, name, pos, sprite, "Props", 3, scale);
        if (wallsLayer >= 0) obj.layer = wallsLayer;

        BoxCollider2D solidBc = GetOrAddComponent<BoxCollider2D>(obj);
        solidBc.size = new Vector2(1.5f, 0.7f);
        solidBc.offset = new Vector2(0f, -0.2f);
        solidBc.isTrigger = false;

        GameObject triggerChild = GetOrCreateChild(obj, "Trigger_Escondite");
        triggerChild.transform.localPosition = Vector3.zero;
        if (hideSpotLayer >= 0) triggerChild.layer = hideSpotLayer;

        BoxCollider2D triggerBc = GetOrAddComponent<BoxCollider2D>(triggerChild);
        triggerBc.size = new Vector2(2.4f, 1.5f);
        triggerBc.offset = Vector2.zero;
        triggerBc.isTrigger = true;

        HideSpot hs = GetOrAddComponent<HideSpot>(triggerChild);
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("spotType").enumValueIndex = 3;
        so.ApplyModifiedProperties();

        GameObject promptObj = GetOrCreateChild(triggerChild, "Prompt_E");
        promptObj.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        TextMesh tm = GetOrAddComponent<TextMesh>(promptObj);
        tm.text = "[E]";
        tm.fontSize = 24;
        tm.characterSize = 0.13f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.9f, 0.95f, 1.0f);
        MeshRenderer mr = promptObj.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 15;
    }

    static void CreateCabinHideSpot(GameObject parent, string name, Vector3 pos, Sprite sprite, Vector3 scale, int wallsLayer, int hideSpotLayer)
    {
        GameObject obj = CreatePiece(parent, name, pos, sprite, "Props", 4, scale);
        if (wallsLayer >= 0) obj.layer = wallsLayer;

        BoxCollider2D solidBc = GetOrAddComponent<BoxCollider2D>(obj);
        solidBc.size = new Vector2(3.8f, 3.2f);
        solidBc.offset = new Vector2(0f, 0.4f);
        solidBc.isTrigger = false;

        GameObject doorSpot = GetOrCreateChild(obj, "Puerta_Escondite_Cabana");
        doorSpot.transform.localPosition = new Vector3(0f, -1.2f, 0f);
        if (hideSpotLayer >= 0) doorSpot.layer = hideSpotLayer;

        BoxCollider2D triggerBc = GetOrAddComponent<BoxCollider2D>(doorSpot);
        triggerBc.size = new Vector2(2.0f, 1.6f);
        triggerBc.isTrigger = true;

        HideSpot hs = GetOrAddComponent<HideSpot>(doorSpot);
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("spotType").enumValueIndex = 3;
        so.ApplyModifiedProperties();

        GameObject promptObj = GetOrCreateChild(doorSpot, "Prompt_E");
        promptObj.transform.localPosition = new Vector3(0f, 1.3f, 0f);
        TextMesh tm = GetOrAddComponent<TextMesh>(promptObj);
        tm.text = "[E] Refugio";
        tm.fontSize = 24;
        tm.characterSize = 0.13f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1.0f, 0.90f, 0.50f);
        MeshRenderer mr = promptObj.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 15;
    }

    static void CreateShrineAltar(GameObject parent, string name, Vector3 pos, Sprite sprite, string shrineTitle, Vector3 scale, int wallsLayer)
    {
        GameObject obj = CreatePiece(parent, name, pos, sprite, "Props", 3, scale);
        if (wallsLayer >= 0) obj.layer = wallsLayer;

        BoxCollider2D solidBc = GetOrAddComponent<BoxCollider2D>(obj);
        solidBc.size = new Vector2(1.8f, 0.8f);
        solidBc.offset = new Vector2(0f, -0.3f);
        solidBc.isTrigger = false;

        CircleCollider2D cc = GetOrAddComponent<CircleCollider2D>(obj);
        cc.isTrigger = true;
        cc.radius = 2.0f;

        ShrineNiche shrine = GetOrAddComponent<ShrineNiche>(obj);

        GameObject lightObj = GetOrCreateChild(obj, "LuzSantuario");
        lightObj.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        Light2D l2d = GetOrAddComponent<Light2D>(lightObj);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.70f, 0.22f);
        l2d.intensity = 2.4f;
        l2d.pointLightOuterRadius = 4.5f;
        l2d.pointLightInnerRadius = 0.8f;
        l2d.enabled = true;
        if (lightObj.GetComponent<LightFlicker>() == null) lightObj.AddComponent<LightFlicker>();

        GameObject dome = GetOrCreateChild(obj, "AuraProtectora");
        dome.transform.localPosition = Vector3.zero;
        Light2D domeLight = GetOrAddComponent<Light2D>(dome);
        domeLight.lightType = Light2D.LightType.Point;
        domeLight.color = new Color(1.0f, 0.85f, 0.30f, 0.40f);
        domeLight.intensity = 1.5f;
        domeLight.pointLightOuterRadius = 3.5f;
        dome.SetActive(true);

        SerializedObject so = new SerializedObject(shrine);
        so.FindProperty("shrineName").stringValue = shrineTitle;
        so.FindProperty("sanctuaryRadius").floatValue = 3.5f;
        so.FindProperty("shrineLight").objectReferenceValue = l2d;
        so.FindProperty("sanctuaryDome").objectReferenceValue = dome;
        so.ApplyModifiedProperties();
    }

    static GameObject CreateLloronaGrave(
        GameObject parent,
        Vector3 pos,
        Sprite graveSprite,
        Sprite angelSprite,
        Sprite flowerCarpet,
        Sprite candlesSprite,
        AudioClip offeringClip,
        AudioClip bellClip,
        AudioClip gateClip,
        int wallsLayer)
    {
        GameObject graveObj = CreatePiece(parent, "Tumba_De_La_Llorona", pos, graveSprite, "Props", 5, new Vector3(0.95f, 0.95f, 1f));
        if (wallsLayer >= 0) graveObj.layer = wallsLayer;

        BoxCollider2D bc = GetOrAddComponent<BoxCollider2D>(graveObj);
        bc.size = new Vector2(2.2f, 1.2f);
        bc.offset = new Vector2(0f, -0.3f);
        bc.isTrigger = false;

        // Ángel Doliente sobre la tumba
        if (angelSprite != null)
        {
            CreatePiece(graveObj, "Estatua_Angel", new Vector3(0f, 1.3f, 0f), angelSprite, "Props", 6, new Vector3(0.65f, 0.65f, 1f));
        }

        // Decoraciones de Ofrenda (Alfombra de cempasúchil y veladoras)
        GameObject decoRoot = GetOrCreateChild(graveObj, "Ofrenda_Consagrada_Decoraciones");
        decoRoot.transform.localPosition = Vector3.zero;

        if (flowerCarpet != null)
        {
            CreatePiece(decoRoot, "Alfombra_Flores", new Vector3(0f, -0.6f, 0f), flowerCarpet, "Props", 3, new Vector3(1.1f, 1.1f, 1f));
        }
        if (candlesSprite != null)
        {
            CreatePiece(decoRoot, "Veladoras_Izq", new Vector3(-1.1f, -0.4f, 0f), candlesSprite, "Props", 7, new Vector3(0.85f, 0.85f, 1f));
            CreatePiece(decoRoot, "Veladoras_Der", new Vector3( 1.1f, -0.4f, 0f), candlesSprite, "Props", 7, new Vector3(0.85f, 0.85f, 1f));
        }

        // Luz de la tumba
        GameObject lightObj = GetOrCreateChild(graveObj, "LuzTumbaLlorona");
        lightObj.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        Light2D l2d = GetOrAddComponent<Light2D>(lightObj);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.65f, 0.35f, 0.95f); // Violeta místico inicial
        l2d.intensity = 2.2f;
        l2d.pointLightOuterRadius = 4.8f;
        if (lightObj.GetComponent<LightFlicker>() == null) lightObj.AddComponent<LightFlicker>();

        // Prompt de interacción
        GameObject promptObj = GetOrCreateChild(graveObj, "Prompt_E");
        promptObj.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        TextMesh tm = GetOrAddComponent<TextMesh>(promptObj);
        tm.text = "✦ Tumba Olvidada de La Llorona ✦\n[Sellada]";
        tm.fontSize = 24;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.8f, 0.5f, 1.0f);
        MeshRenderer mr = promptObj.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 20;

        LloronaGraveOffering offering = GetOrAddComponent<LloronaGraveOffering>(graveObj);
        SerializedObject so = new SerializedObject(offering);
        so.FindProperty("offeringDecorations").objectReferenceValue = decoRoot;
        so.FindProperty("graveLight").objectReferenceValue = l2d;
        so.FindProperty("promptObject").objectReferenceValue = promptObj;
        so.FindProperty("promptTextMesh").objectReferenceValue = tm;
        so.FindProperty("offeringSfx").objectReferenceValue = offeringClip;
        so.FindProperty("bellTollSfx").objectReferenceValue = bellClip;
        so.FindProperty("gateOpenSfx").objectReferenceValue = gateClip;
        so.FindProperty("interactRadius").floatValue = 3.5f;
        so.ApplyModifiedProperties();

        return graveObj;
    }

    static GameObject CreateSacredOfferingPickupItem(
        GameObject parent,
        Vector3 pos,
        Sprite candleSprite,
        AudioClip pickupSfx)
    {
        GameObject itemObj = CreatePiece(parent, "Ofrenda_Sagrada_Pickup", pos, candleSprite, "Props", 8, new Vector3(0.95f, 0.95f, 1f));

        GameObject lightObj = GetOrCreateChild(itemObj, "LuzDoradaOfrenda");
        lightObj.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        Light2D l2d = GetOrAddComponent<Light2D>(lightObj);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.85f, 0.25f);
        l2d.intensity = 3.2f;
        l2d.pointLightOuterRadius = 4.8f;
        if (lightObj.GetComponent<LightFlicker>() == null) lightObj.AddComponent<LightFlicker>();

        GameObject promptObj = GetOrCreateChild(itemObj, "Prompt_E");
        promptObj.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        TextMesh tm = GetOrAddComponent<TextMesh>(promptObj);
        tm.text = "✦ Ofrenda Sagrada de Cempasúchil ✦\n[E] Tomar Flores y Veladoras";
        tm.fontSize = 24;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1.0f, 0.90f, 0.35f);
        MeshRenderer mr = promptObj.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 20;

        SacredOfferingPickup pickup = GetOrAddComponent<SacredOfferingPickup>(itemObj);
        SerializedObject so = new SerializedObject(pickup);
        so.FindProperty("interactRadius").floatValue = 2.8f;
        so.FindProperty("pickupSfx").objectReferenceValue = pickupSfx;
        so.FindProperty("promptObject").objectReferenceValue = promptObj;
        so.FindProperty("promptTextMesh").objectReferenceValue = tm;
        so.FindProperty("itemLight").objectReferenceValue = l2d;
        so.ApplyModifiedProperties();

        return itemObj;
    }

    static void CreateHolyWaterSupplyStation(GameObject parent, string name, Vector3 pos, Sprite sprite, AudioClip refillSfx, int wallsLayer)
    {
        GameObject fontObj = CreatePiece(parent, name, pos, sprite, "Props", 3, new Vector3(0.85f, 0.85f, 1f));
        if (wallsLayer >= 0) fontObj.layer = wallsLayer;

        BoxCollider2D bc = GetOrAddComponent<BoxCollider2D>(fontObj);
        bc.size = new Vector2(1.2f, 1.2f);
        bc.offset = new Vector2(0f, -0.2f);
        bc.isTrigger = false;

        GameObject lightObj = GetOrCreateChild(fontObj, "LuzAguaBendita");
        lightObj.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        Light2D l2d = GetOrAddComponent<Light2D>(lightObj);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.25f, 0.85f, 1.0f);
        l2d.intensity = 1.8f;
        l2d.pointLightOuterRadius = 3.5f;

        GameObject promptObj = GetOrCreateChild(fontObj, "Prompt_E");
        promptObj.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        TextMesh tm = GetOrAddComponent<TextMesh>(promptObj);
        tm.text = "[E] Tomar Agua Bendita";
        tm.fontSize = 24;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.35f, 0.95f, 1.0f);
        MeshRenderer mr = promptObj.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 15;

        HolyWaterSupply supply = GetOrAddComponent<HolyWaterSupply>(fontObj);
        SerializedObject so = new SerializedObject(supply);
        so.FindProperty("fontName").stringValue = "Pila de Agua Bendita";
        so.FindProperty("interactRadius").floatValue = 3.2f;
        so.FindProperty("refillSfx").objectReferenceValue = refillSfx;
        so.FindProperty("promptObject").objectReferenceValue = promptObj;
        so.FindProperty("promptTextMesh").objectReferenceValue = tm;
        so.FindProperty("fontLight").objectReferenceValue = l2d;
        so.ApplyModifiedProperties();
    }

    static GameObject CreateCemeterySpecter(GameObject parent, string name, Vector3 startPos, float minX, float maxX, float speed, AudioClip scream, AudioClip ambient)
    {
        GameObject specterObj = GetOrCreateChild(parent, name);
        specterObj.transform.position = startPos;
        specterObj.transform.localScale = new Vector3(0.48f, 0.48f, 1f);

        Sprite specterSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Llorona/llorona_movimiento.png")
            .OfType<Sprite>().FirstOrDefault(s => s.name.Contains("0"))
            ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Llorona/llorona.png");

        SpriteRenderer sr = GetOrAddComponent<SpriteRenderer>(specterObj);
        sr.sprite = specterSprite;
        sr.color = new Color(0.35f, 0.85f, 1.0f, 0.78f);
        sr.sortingLayerName = "Characters";
        sr.sortingOrder = 11;

        GameObject lightChild = GetOrCreateChild(specterObj, "Luz_Espectral");
        lightChild.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        Light2D l2d = GetOrAddComponent<Light2D>(lightChild);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.25f, 0.65f, 1.0f, 1.0f);
        l2d.pointLightOuterRadius = 3.8f;
        l2d.pointLightInnerRadius = 0.5f;
        l2d.intensity = 1.0f;

        BoxCollider2D specterCol = GetOrAddComponent<BoxCollider2D>(specterObj);
        specterCol.size = new Vector2(3.0f, 3.5f);
        specterCol.offset = new Vector2(0f, -0.2f);
        specterCol.isTrigger = true;

        SpecterAI ai = GetOrAddComponent<SpecterAI>(specterObj);
        ai.ConfigurePatrol(minX, maxX, speed, scream, ambient, vRange: 3.5f, balcony: false);

        return specterObj;
    }

    static void CreateExitGate(
        GameObject parent,
        Vector3 pos,
        Sprite sprite,
        int wallsLayer,
        out GameObject gateObj,
        out Collider2D exitTrigger,
        out Collider2D gateSolid,
        out Light2D gateLight,
        out TextMesh gateLabel)
    {
        gateObj = CreatePiece(parent, "Reja_Gotica_Meta", pos, sprite, "Props", 6, new Vector3(0.90f, 0.90f, 1f));

        BoxCollider2D bcTrigger = GetOrAddComponent<BoxCollider2D>(gateObj);
        bcTrigger.isTrigger = true;
        bcTrigger.size = new Vector2(3.2f, 4.5f);
        bcTrigger.offset = Vector2.zero;
        if (gateObj.GetComponent<LevelGoal>() == null) gateObj.AddComponent<LevelGoal>();
        bcTrigger.enabled = false;
        exitTrigger = bcTrigger;

        GameObject barrierObj = GetOrCreateChild(gateObj, "Barrera_Solida_Sello");
        barrierObj.transform.localPosition = Vector3.zero;
        if (wallsLayer >= 0) barrierObj.layer = wallsLayer;
        BoxCollider2D bcSolid = GetOrAddComponent<BoxCollider2D>(barrierObj);
        bcSolid.isTrigger = false;
        bcSolid.size = new Vector2(2.5f, 5.0f);
        gateSolid = bcSolid;

        GameObject lightObj = GetOrCreateChild(gateObj, "LuzSalvacion");
        lightObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        Light2D l2d = GetOrAddComponent<Light2D>(lightObj);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.65f, 0.15f, 0.95f);
        l2d.intensity = 1.8f;
        l2d.pointLightOuterRadius = 5.5f;
        if (lightObj.GetComponent<LightFlicker>() == null) lightObj.AddComponent<LightFlicker>();
        gateLight = l2d;

        GameObject labelGo = GetOrCreateChild(gateObj, "LabelMeta");
        labelGo.transform.localPosition = new Vector3(0f, 3.2f, 0f);
        TextMesh tm = GetOrAddComponent<TextMesh>(labelGo);
        tm.text = "✦ LA GRAN REJA DE TU HOGAR ✦\nDerrota y Purifica a La Llorona para abrirla";
        tm.fontSize = 24;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1.0f, 0.45f, 0.35f);
        MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 16;
        gateLabel = tm;
    }

    static void CreateBossHealthBarUI(Transform canvasTransform)
    {
        if (canvasTransform == null) return;

        Transform old = canvasTransform.Find("Barra_Jefe_Llorona");
        if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

        GameObject bossBarGo = new GameObject("Barra_Jefe_Llorona");
        bossBarGo.transform.SetParent(canvasTransform, false);

        RectTransform rt = bossBarGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -18f);
        rt.sizeDelta = new Vector2(620f, 85f);

        Image bgPanel = bossBarGo.AddComponent<Image>();
        bgPanel.color = new Color(0.04f, 0.04f, 0.08f, 0.88f);

        GameObject titleGo = new GameObject("BossTitle");
        titleGo.transform.SetParent(bossBarGo.transform, false);
        Text titleTxt = titleGo.AddComponent<Text>();
        titleTxt.text = "✦ LA LLORONA - EL ALMA EN PENA ✦";
        titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleTxt.fontSize = 18;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(1.0f, 0.85f, 0.35f);
        RectTransform rtTitle = titleGo.GetComponent<RectTransform>();
        rtTitle.anchorMin = new Vector2(0.5f, 1f);
        rtTitle.anchorMax = new Vector2(0.5f, 1f);
        rtTitle.pivot = new Vector2(0.5f, 1f);
        rtTitle.anchoredPosition = new Vector2(0f, -6f);
        rtTitle.sizeDelta = new Vector2(600f, 24f);

        GameObject subGo = new GameObject("BossSubtitle");
        subGo.transform.SetParent(bossBarGo.transform, false);
        Text subTxt = subGo.AddComponent<Text>();
        subTxt.text = "Fase 1: El Llanto de las Sombras";
        subTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subTxt.fontSize = 13;
        subTxt.alignment = TextAnchor.MiddleCenter;
        subTxt.color = new Color(0.35f, 0.85f, 1.0f);
        RectTransform rtSub = subGo.GetComponent<RectTransform>();
        rtSub.anchorMin = new Vector2(0.5f, 1f);
        rtSub.anchorMax = new Vector2(0.5f, 1f);
        rtSub.pivot = new Vector2(0.5f, 1f);
        rtSub.anchoredPosition = new Vector2(0f, -28f);
        rtSub.sizeDelta = new Vector2(600f, 18f);

        GameObject barBgGo = new GameObject("HealthBar_BG");
        barBgGo.transform.SetParent(bossBarGo.transform, false);
        Image barBgImg = barBgGo.AddComponent<Image>();
        barBgImg.color = new Color(0.18f, 0.08f, 0.12f, 0.95f);
        RectTransform rtBarBg = barBgGo.GetComponent<RectTransform>();
        rtBarBg.anchorMin = new Vector2(0.5f, 1f);
        rtBarBg.anchorMax = new Vector2(0.5f, 1f);
        rtBarBg.pivot = new Vector2(0.5f, 1f);
        rtBarBg.anchoredPosition = new Vector2(0f, -48f);
        rtBarBg.sizeDelta = new Vector2(560f, 14f);

        GameObject barFillGo = new GameObject("HealthBar_Fill");
        barFillGo.transform.SetParent(barBgGo.transform, false);
        Image barFillImg = barFillGo.AddComponent<Image>();
        barFillImg.type = Image.Type.Filled;
        barFillImg.fillMethod = Image.FillMethod.Horizontal;
        barFillImg.fillAmount = 1.0f;
        barFillImg.color = new Color(0.20f, 0.80f, 1.0f);
        RectTransform rtFill = barFillGo.GetComponent<RectTransform>();
        rtFill.anchorMin = Vector2.zero;
        rtFill.anchorMax = Vector2.one;
        rtFill.sizeDelta = Vector2.zero;

        GameObject ammoGo = new GameObject("AmmoText");
        ammoGo.transform.SetParent(bossBarGo.transform, false);
        Text ammoTxt = ammoGo.AddComponent<Text>();
        ammoTxt.text = "💧 Agua Bendita: [ ● ● ● ]  (Clic / [F] para lanzar)";
        ammoTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ammoTxt.fontSize = 13;
        ammoTxt.fontStyle = FontStyle.Bold;
        ammoTxt.alignment = TextAnchor.MiddleCenter;
        ammoTxt.color = new Color(0.35f, 0.90f, 1.0f);
        RectTransform rtAmmo = ammoGo.GetComponent<RectTransform>();
        rtAmmo.anchorMin = new Vector2(0.5f, 1f);
        rtAmmo.anchorMax = new Vector2(0.5f, 1f);
        rtAmmo.pivot = new Vector2(0.5f, 1f);
        rtAmmo.anchoredPosition = new Vector2(0f, -66f);
        rtAmmo.sizeDelta = new Vector2(600f, 16f);

        BossHealthBarUI ui = bossBarGo.AddComponent<BossHealthBarUI>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("rootPanel").objectReferenceValue = bossBarGo;
        so.FindProperty("bossTitleText").objectReferenceValue = titleTxt;
        so.FindProperty("phaseSubtitleText").objectReferenceValue = subTxt;
        so.FindProperty("healthBarFill").objectReferenceValue = barFillImg;
        so.FindProperty("healthBarBackground").objectReferenceValue = barBgImg;
        so.FindProperty("ammoText").objectReferenceValue = ammoTxt;
        so.ApplyModifiedProperties();
    }

    static void CreateMudPuddle(GameObject parent, float x, float y, float scale = 1.0f)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Botellas_charcolodo_Nvl1.png").OfType<Sprite>().FirstOrDefault(s => s.name.StartsWith("Charco"));
        GameObject go = GetOrCreateChild(parent, $"Charco_Lodo_{x:F0}_{y:F0}");
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = GetOrAddComponent<SpriteRenderer>(go);
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        CircleCollider2D cc = GetOrAddComponent<CircleCollider2D>(go);
        cc.isTrigger = true;
        cc.radius = 1.1f;

        if (go.GetComponent<MudPuddle>() == null) go.AddComponent<MudPuddle>();
    }

    static void CreateLantern(GameObject parent, string name, Vector2 pos)
    {
        GameObject go = GetOrCreateChild(parent, name);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        Light2D l2d = GetOrAddComponent<Light2D>(go);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.85f, 0.70f, 0.45f);
        l2d.intensity = 1.3f;
        l2d.pointLightOuterRadius = 4.2f;

        if (go.GetComponent<LightFlicker>() == null) go.AddComponent<LightFlicker>();
    }

    static void EnsureGlobalLight(float intensity)
    {
        GameObject glGo = GameObject.Find("Global Light 2D");
        if (glGo == null) glGo = new GameObject("Global Light 2D");
        Light2D gl = GetOrAddComponent<Light2D>(glGo);
        gl.lightType = Light2D.LightType.Global;
        gl.color = new Color(0.32f, 0.36f, 0.58f);
        gl.intensity = intensity;
    }

    static void CreateTutorialTrigger(Transform parent, string name, Vector2 pos, Vector2 size, string text, bool showOnStart = false)
    {
        GameObject go = GetOrCreateChild(parent.gameObject, name);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        BoxCollider2D bc = GetOrAddComponent<BoxCollider2D>(go);
        bc.isTrigger = true;
        bc.size = size;

        TutorialTrigger tt = GetOrAddComponent<TutorialTrigger>(go);
        SerializedObject so = new SerializedObject(tt);
        so.FindProperty("tutorialMessage").stringValue = text;
        so.FindProperty("displayDuration").floatValue = showOnStart ? 6.5f : 4.5f;
        so.FindProperty("showOnStart").boolValue = showOnStart;
        so.ApplyModifiedProperties();
    }

    static GameObject SetupPlayer(Vector2 pos, int playerLayer)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = new GameObject("Player");
            player.tag = "Player";
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            player.AddComponent<CircleCollider2D>().radius = 0.4f;
            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/Carlos.png");
            sr.sortingOrder = 5;
            player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerAnimation>();
            player.AddComponent<HideSystem>();
        }
        player.transform.position = new Vector3(pos.x, pos.y, 0f);
        player.transform.rotation = Quaternion.identity;
        var prb = player.GetComponent<Rigidbody2D>();
        if (prb != null) { prb.freezeRotation = true; prb.gravityScale = 0f; }
        if (playerLayer >= 0) player.layer = playerLayer;
        return player;
    }

    static void SetupPlayerCombat(
        GameObject player,
        Sprite bottleSprite,
        AudioClip throwSfx,
        AudioClip impactSfx,
        AudioClip emptySfx)
    {
        PlayerHolyWaterLauncher launcher = GetOrAddComponent<PlayerHolyWaterLauncher>(player);
        launcher.Configure(bottleSprite, throwSfx, impactSfx, emptySfx);
    }

    static void SetupBossLlorona(
        Vector2 pos,
        GameObject sp1,
        GameObject sp2,
        AudioClip scream,
        AudioClip hurt,
        AudioClip purified,
        Light2D globalLight,
        int wallsLayer,
        int lloronaLayer)
    {
        GameObject lGo = GameObject.Find("Llorona");
        if (lGo == null)
        {
            lGo = new GameObject("Llorona");
            SpriteRenderer sr = lGo.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Llorona/llorona.png");
            sr.sortingOrder = 5;
            lGo.AddComponent<LloronaAnimation>();
        }
        lGo.transform.position = new Vector3(pos.x, pos.y, 0f);
        lGo.transform.rotation = Quaternion.identity;

        var lrb = GetOrAddComponent<Rigidbody2D>(lGo);
        lrb.freezeRotation = true;
        lrb.gravityScale = 0f;

        CircleCollider2D lcol = GetOrAddComponent<CircleCollider2D>(lGo);
        lcol.radius = 0.85f;
        lcol.isTrigger = false;

        if (lloronaLayer >= 0) lGo.layer = lloronaLayer;

        LloronaAI oldAI = lGo.GetComponent<LloronaAI>();
        if (oldAI != null) UnityEngine.Object.DestroyImmediate(oldAI);

        LloronaBossAI boss = GetOrAddComponent<LloronaBossAI>(lGo);

        GameObject lLight = GetOrCreateChild(lGo, "LuzBoss");
        Light2D l2d = GetOrAddComponent<Light2D>(lLight);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.35f, 0.80f, 1.0f);
        l2d.intensity = 2.0f;
        l2d.pointLightOuterRadius = 4.8f;

        SerializedObject so = new SerializedObject(boss);
        so.FindProperty("phase1Speed").floatValue = 2.35f;
        so.FindProperty("phase2Speed").floatValue = 2.75f;
        so.FindProperty("phase3Speed").floatValue = 3.15f;
        so.FindProperty("lungeSpeed").floatValue = 5.2f;
        so.FindProperty("specter1").objectReferenceValue = sp1;
        so.FindProperty("specter2").objectReferenceValue = sp2;
        so.FindProperty("screamSfx").objectReferenceValue = scream;
        so.FindProperty("hurtSfx").objectReferenceValue = hurt;
        so.FindProperty("lungeSfx").objectReferenceValue = scream;
        so.FindProperty("purifiedSfx").objectReferenceValue = purified;
        so.FindProperty("bossLight").objectReferenceValue = l2d;
        so.FindProperty("globalLight").objectReferenceValue = globalLight;
        so.ApplyModifiedProperties();
    }

    static void SetupGameManager(int levelNum)
    {
        GameManager gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject gmGo = new GameObject("GameManager");
            gm = gmGo.AddComponent<GameManager>();
        }
        gm.SetLevelNumber(levelNum);
        EditorUtility.SetDirty(gm);
    }

    static void SetupAudioManager()
    {
        AudioManager am = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
        if (am == null)
        {
            GameObject amGo = new GameObject("AudioManager");
            am = amGo.AddComponent<AudioManager>();
        }
        AudioClip clip4 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Nivel4/cementerio.mp3")
            ?? Resources.Load<AudioClip>("Audio/Music/Nivel4/cementerio");
        if (clip4 != null)
        {
            am.musicaNivel4 = clip4;
        }
        EditorUtility.SetDirty(am);
    }

    static GameObject GetOrCreateChild(GameObject parent, string name)
    {
        Transform child = parent.transform.Find(name);
        if (child != null) return child.gameObject;
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        return obj;
    }

    static void CleanOldSceneGarbage(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        HashSet<string> preserveNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Main Camera", "Player", "Canvas", "UICanvas", "EventSystem",
            "GameManager", "AudioManager", "Global Light 2D", "Llorona"
        };

        foreach (var root in roots)
        {
            if (!preserveNames.Contains(root.name))
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
