// Assets/Editor/Level4Builder.cs
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Generador del Nivel 4: "El Panteón de las Ánimas" (Clímax Final).
/// Implementa la Opción A:
/// - Ritual Progresivo en 3 Fases (Level4RitualController)
/// - Reja Gótica Sellada que requiere los 3 Altares consagrados
/// - Despertar progresivo de Ánimas en Pena (SpecterAI)
/// - Laberinto colonial cerrado con muros interiores, mausoleos gigantes y esquinas ciegas
/// - Modo Frenesí y Persecución Final al romper el sello sagrado
/// </summary>
public static class Level4Builder
{
    const string ScenePath = "Assets/Scenes/Level4_Cementerio.unity";
    const string SpritesDir = "Assets/Sprites/Environment/Nivel 4/Sprites";

    [MenuItem("Llorona/Niveles/Construir Nivel 4 - Cementerio (Climax)")]
    public static void BuildLevel4Scene()
    {
        Debug.Log("[Level4Builder] === INICIANDO CONSTRUCCIÓN DEL NIVEL 4 (CLÍMAX) ===");

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

        // SFX de clímax
        AudioClip campanaSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_campanas_catedral.wav");
        AudioClip portalSfx  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_portal_apertura.wav");
        AudioClip screamSfx  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_llorona_grito_detect.wav")
            ?? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/llorona_grito.wav");
        AudioClip ambientSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel 3/sfx_llorona_lamento_loop.wav");

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
        // Muro Interior Oeste (Separa Patio de Entrada del Laberinto Central): X = 11.0
        // Deja pasillo central amplio y despejado entre Y = 10.5 y 17.5
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

        // Muro Interior Este (Separa Laberinto Central de la Explanada de Salida): X = 33.5
        // Deja pasillo central amplio y despejado entre Y = 10.5 y 17.5
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
        // 5. MAUSOLEOS MONUMENTALES Y CRIPTAS (COLISIÓN EXCLUSIVA EN LA BASE)
        // ----------------------------------------------------
        GameObject mausoleosGroup = GetOrCreateChild(rootEnv, "Mausoleos_Labyrinth");

        // 1. Gran Mausoleo de Guanajuato (Sector Norte): X = 17.0, Y = 19.5 (Base col: 3.2 x 1.4, offset -1.6)
        CreateMausoleum(mausoleosGroup, "Mausoleo_Grande_Norte", new Vector3(17.0f, 19.5f, 0f), mausoleoGrande, new Vector3(0.65f, 0.65f, 1f), new Vector2(3.2f, 1.4f), -1.6f, wallsLayer);

        // 2. Cripta de Pared Colonial (Sector Norte-Centro): X = 23.5, Y = 20.0 (Base col: 2.4 x 1.2, offset -1.0)
        CreateMausoleum(mausoleosGroup, "Cripta_Pared_Norte", new Vector3(23.5f, 20.0f, 0f), criptaPared, new Vector3(0.80f, 0.80f, 1f), new Vector2(2.4f, 1.2f), -1.0f, wallsLayer);

        // 3. Capilla Funeraria Norte-Este: X = 29.5, Y = 20.5 (Base col: 2.4 x 1.3, offset -1.3)
        CreateMausoleum(mausoleosGroup, "Mausoleo_Capilla_Noreste", new Vector3(29.5f, 20.5f, 0f), mausoleoMedio, new Vector3(0.75f, 0.75f, 1f), new Vector2(2.4f, 1.3f), -1.3f, wallsLayer);

        // 4. Cripta de Piedra Oscura (Sector Sur-Oeste): X = 16.5, Y = 7.5 (Base col: 2.2 x 1.3, offset -1.1)
        CreateMausoleum(mausoleosGroup, "Mausoleo_Cripta_Suroeste", new Vector3(16.5f, 7.5f, 0f), mausoleoCripta, new Vector3(0.75f, 0.75f, 1f), new Vector2(2.2f, 1.3f), -1.1f, wallsLayer);

        // 5. Capilla Funeraria Sur-Centro: X = 23.0, Y = 7.5 (Base col: 2.4 x 1.3, offset -1.3)
        CreateMausoleum(mausoleosGroup, "Mausoleo_Capilla_Sur", new Vector3(23.0f, 7.5f, 0f), mausoleoMedio, new Vector3(0.75f, 0.75f, 1f), new Vector2(2.4f, 1.3f), -1.3f, wallsLayer);

        // 6. Cripta de Piedra Sur-Este: X = 29.5, Y = 7.5 (Base col: 2.2 x 1.3, offset -1.1)
        CreateMausoleum(mausoleosGroup, "Mausoleo_Cripta_Sureste", new Vector3(29.5f, 7.5f, 0f), mausoleoCripta, new Vector3(0.75f, 0.75f, 1f), new Vector2(2.2f, 1.3f), -1.1f, wallsLayer);

        // ----------------------------------------------------
        // 6. ESCONDITES DE SIGILO (BASE SÓLIDA + TRIGGER [E] CIRCUNDANTE)
        // ----------------------------------------------------
        GameObject stealthGroup = GetOrCreateChild(rootEnv, "Escondites_Sigilo");

        // 1. Sarcófago Patio Oeste (Entrada): X = 8.0, Y = 12.5
        CreateSarcophagusHideSpot(stealthGroup, "Sarcofago_Inicio", new Vector3(8.0f, 12.5f, 0f), sarcofagoCruz, new Vector3(0.75f, 0.75f, 1f), hideSpotLayer, wallsLayer);

        // 2. Sarcófago Doble de Cantera (Laberinto Central, colocado a un costado del paseo): X = 19.5, Y = 11.8
        CreateSarcophagusHideSpot(stealthGroup, "Sarcofago_Laberinto_Central", new Vector3(19.5f, 11.8f, 0f), sarcofagoDoble, new Vector3(0.75f, 0.75f, 1f), hideSpotLayer, wallsLayer);

        // 3. Sarcófago Pasillo Criptas Norte: X = 26.5, Y = 25.5
        CreateSarcophagusHideSpot(stealthGroup, "Sarcofago_Criptas_Norte", new Vector3(26.5f, 25.5f, 0f), sarcofagoCruz, new Vector3(0.75f, 0.75f, 1f), hideSpotLayer, wallsLayer);

        // 4. Tumba Ornamental Sector Sur: X = 26.0, Y = 3.8
        CreateSarcophagusHideSpot(stealthGroup, "Tumba_Escondite_Sur", new Vector3(26.0f, 3.8f, 0f), tumbaOrnamental, new Vector3(0.75f, 0.75f, 1f), hideSpotLayer, wallsLayer);

        // 5. Cabaña del Sepulturero (Refugio clave en explanada este): X = 38.5, Y = 19.5
        CreateCabinHideSpot(stealthGroup, "Cabana_Sepulturero", new Vector3(38.5f, 19.5f, 0f), cabanaSepul, new Vector3(0.60f, 0.60f, 1f), wallsLayer, hideSpotLayer);

        // ----------------------------------------------------
        // 7. ALTARES SAGRADOS DE CEMPASÚCHIL (BASE SÓLIDA + TRIGGER [F])
        // ----------------------------------------------------
        GameObject shrinesGroup = GetOrCreateChild(rootEnv, "Altares_Cempasuchil");

        // Altar 1: Altar con Arco de Cempasúchil (Patio Oeste): X = 6.5, Y = 20.0
        CreateShrineAltar(shrinesGroup, "Altar_Arco_Oeste", new Vector3(6.5f, 20.0f, 0f), altarArco, "Altar de la Piedad", new Vector3(0.85f, 0.85f, 1f), wallsLayer);

        // Altar 2: Mesa de Ofrendas con Calaveritas (Sector Sur): X = 22.5, Y = 3.5
        CreateShrineAltar(shrinesGroup, "Altar_Mesa_Sur", new Vector3(22.5f, 3.5f, 0f), ofrendaMesa, "Ofrenda de las Ánimas", new Vector3(0.90f, 0.90f, 1f), wallsLayer);

        // Altar 3: Gran Altar Magno de Cempasúchil (Plaza Central): X = 28.0, Y = 14.0
        CreateShrineAltar(shrinesGroup, "Altar_Magno_Panteon", new Vector3(28.0f, 14.0f, 0f), altarMagno, "El Gran Altar de las Ánimas", new Vector3(0.95f, 0.95f, 1f), wallsLayer);
        if (ofrendaColumna != null)
        {
            CreatePiece(shrinesGroup, "Columna_Flores_Izq", new Vector3(26.0f, 15.8f, 0f), ofrendaColumna, "Props", 3, new Vector3(0.8f, 0.8f, 1f));
            CreatePiece(shrinesGroup, "Columna_Flores_Der", new Vector3(30.0f, 15.8f, 0f), ofrendaColumna, "Props", 3, new Vector3(0.8f, 0.8f, 1f));
        }

        // ----------------------------------------------------
        // 8. VELADORAS COLECCIONABLES REPARTIDAS
        // ----------------------------------------------------
        GameObject candlesGroup = GetOrCreateChild(rootEnv, "Veladoras_Coleccionables");
        CreateCandlePickup(candlesGroup, "Vela_Entrada_Oeste", velaSprite, new Vector3( 4.5f,  8.5f, 0f));
        CreateCandlePickup(candlesGroup, "Vela_Norte_Criptas", velaSprite, new Vector3(14.5f, 24.5f, 0f));
        CreateCandlePickup(candlesGroup, "Vela_Sur_Fosas",     velaSprite, new Vector3(19.0f,  2.5f, 0f));
        CreateCandlePickup(candlesGroup, "Vela_Cabana_Este",  velaSprite, new Vector3(36.0f, 23.0f, 0f));

        // ----------------------------------------------------
        // 9. NATURALEZA TÉTRICA Y ÁRBOLES FUNERARIOS
        // ----------------------------------------------------
        GameObject treesGroup = GetOrCreateChild(rootEnv, "Naturaleza_Funeraria");
        CreatePieceWithCollider(treesGroup, "Arbol_Seco_Oeste", new Vector3( 4.0f, 23.5f, 0f), arbolSeco, "Props", 10, new Vector3(0.75f, 0.75f, 1f), new Vector2(1.0f, 0.8f), -0.5f, wallsLayer);
        CreatePieceWithCollider(treesGroup, "Arbol_Seco_Este",  new Vector3(36.0f,  6.5f, 0f), arbolSeco, "Props", 10, new Vector3(0.70f, 0.70f, 1f), new Vector2(1.0f, 0.8f), -0.5f, wallsLayer);
        CreatePieceWithCollider(treesGroup, "Sauce_Oeste",      new Vector3( 8.5f,  7.0f, 0f), sauceFunerario, "Props", 10, new Vector3(0.75f, 0.75f, 1f), new Vector2(1.0f, 0.8f), -0.5f, wallsLayer);
        CreatePieceWithCollider(treesGroup, "Sauce_Noreste",    new Vector3(32.0f, 22.5f, 0f), sauceFunerario, "Props", 10, new Vector3(0.75f, 0.75f, 1f), new Vector2(1.0f, 0.8f), -0.5f, wallsLayer);

        // Cipreses en perímetro
        CreatePiece(treesGroup, "Cipres_1", new Vector3( 3.0f, 19.5f, 0f), cipresAlto, "Props", 4, new Vector3(0.85f, 0.85f, 1f));
        CreatePiece(treesGroup, "Cipres_2", new Vector3(42.5f,  8.5f, 0f), cipresAlto, "Props", 4, new Vector3(0.85f, 0.85f, 1f));
        CreatePiece(treesGroup, "Cipres_3", new Vector3(42.5f, 20.0f, 0f), cipresAlto, "Props", 4, new Vector3(0.85f, 0.85f, 1f));

        // Matorrales espinosos
        CreatePiece(treesGroup, "Zarza_1", new Vector3( 9.0f,  4.5f, 0f), matorral1, "Props", 2, new Vector3(0.65f, 0.65f, 1f));
        CreatePiece(treesGroup, "Zarza_2", new Vector3(25.0f, 25.5f, 0f), matorral2, "Props", 2, new Vector3(0.65f, 0.65f, 1f));
        CreatePiece(treesGroup, "Zarza_3", new Vector3(38.0f,  9.5f, 0f), matorral1, "Props", 2, new Vector3(0.65f, 0.65f, 1f));

        // ----------------------------------------------------
        // 10. LÁPIDAS, CRUCES Y FOSAS (COLISIONES LIMPIAS Y CAMINOS DESPEJADOS)
        // ----------------------------------------------------
        GameObject tombsGroup = GetOrCreateChild(rootEnv, "Tumbas_Lapidas_Cantera");
        Vector2[] lapidasP1 = new Vector2[]
        {
            new Vector2( 5.0f, 11.0f), new Vector2( 7.0f, 11.0f),
            new Vector2( 5.0f,  5.5f), new Vector2( 7.0f,  5.5f),
            new Vector2(18.5f, 25.8f), new Vector2(23.5f, 25.8f), // Pegadas al muro norte para no bloquear el paseo
            new Vector2(36.5f, 16.8f), new Vector2(36.5f, 11.2f)
        };
        for (int i = 0; i < lapidasP1.Length; i++)
        {
            Sprite s = (i % 2 == 0) ? tumbaCruz : lapidaCantera;
            CreateSolidProp(tombsGroup, $"Lapida_{i+1}", lapidasP1[i], s, new Vector3(0.65f, 0.65f, 1f), new Vector2(0.6f, 0.4f), -0.2f, wallsLayer);
        }

        // Estatuas de Ángeles Dolientes (Ubicadas en las esquinas de los muros interiores, dejando la calle abierta)
        CreateSolidProp(tombsGroup, "Angel_Doliente_Oeste", new Vector3(13.5f, 16.5f, 0f), angelDoliente, new Vector3(0.70f, 0.70f, 1f), new Vector2(0.8f, 0.6f), -0.4f, wallsLayer);
        CreateSolidProp(tombsGroup, "Angel_Doliente_Este",  new Vector3(31.5f, 16.5f, 0f), angelDoliente, new Vector3(0.70f, 0.70f, 1f), new Vector2(0.8f, 0.6f), -0.4f, wallsLayer);

        // Obeliscos funerarios pegados a los muros
        CreateSolidProp(tombsGroup, "Obelisco_Sur",   new Vector3( 9.0f,  9.5f, 0f), obelisco, new Vector3(0.70f, 0.70f, 1f), new Vector2(0.6f, 0.6f), -0.5f, wallsLayer);
        CreateSolidProp(tombsGroup, "Obelisco_Norte", new Vector3(21.0f, 25.8f, 0f), obelisco, new Vector3(0.70f, 0.70f, 1f), new Vector2(0.6f, 0.6f), -0.5f, wallsLayer);

        // Fosas abiertas malditas (Sólidas para no atravesarlas, pero colocadas a un costado del sendero sur)
        CreateSolidProp(tombsGroup, "Fosa_Abierta_1", new Vector2(18.0f, 2.6f), tumbaFosa, new Vector3(0.75f, 0.75f, 1f), new Vector2(1.5f, 0.8f), 0f, wallsLayer);
        CreateSolidProp(tombsGroup, "Fosa_Abierta_2", new Vector2(30.0f, 2.6f), tumbaFosa, new Vector3(0.75f, 0.75f, 1f), new Vector2(1.5f, 0.8f), 0f, wallsLayer);

        // ----------------------------------------------------
        // 11. PELIGROS AMBIENTALES (LODO ESQUIVABLE, RUIDO Y PERRO)
        // ----------------------------------------------------
        GameObject hazardsGroup = GetOrCreateChild(rootEnv, "Peligros_Ambientales");
        CreateMudPuddle(hazardsGroup, 13.0f, 12.0f, 1.2f); // Ligeramente fuera del eje central para poder esquivar
        CreateMudPuddle(hazardsGroup, 23.5f, 12.0f, 1.2f);
        CreateMudPuddle(hazardsGroup, 34.0f, 12.0f, 1.2f);
        CreateMudPuddle(hazardsGroup, 18.5f,  3.8f, 1.1f);

        CreateNoiseObstacle(hazardsGroup, 15.5f, 23.5f);
        CreateNoiseObstacle(hazardsGroup, 25.5f, 12.5f);

        CreateStrayDog(hazardsGroup, 35.0f, 8.5f);

        // ----------------------------------------------------
        // 12. ESPECTROS DURMIENTES (ÁNIMAS EN PENA - SPECTER AI)
        // ----------------------------------------------------
        GameObject spectersGroup = GetOrCreateChild(rootEnv, "Amenazas_Espectros");
        // Espectro 1: Pasillo Sur de Fosas y Osarios (X: 14 a 28, Y = 10.5)
        GameObject specter1 = CreateCemeterySpecter(spectersGroup, "Espectro_Anima_1", new Vector3(16.0f, 10.5f, 0f), 13.0f, 28.0f, 1.45f, screamSfx, ambientSfx);
        // Espectro 2: Pasillo Norte de Criptas y Mausoleos (X: 15 a 32, Y = 24.0)
        GameObject specter2 = CreateCemeterySpecter(spectersGroup, "Espectro_Anima_2", new Vector3(25.0f, 24.0f, 0f), 15.0f, 32.0f, 1.55f, screamSfx, ambientSfx);

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
            new Vector2( 3.5f, 14.0f), // Entrada Carlos
            new Vector2( 9.0f, 19.5f), // Cerca altar arco
            new Vector2(13.5f, 14.0f), // Glorieta ángel 1
            new Vector2(16.5f, 24.0f), // Pasillo norte mausoleo
            new Vector2(16.5f,  4.0f), // Pasillo sur cripta
            new Vector2(23.0f, 14.0f), // Cruce central
            new Vector2(25.5f, 20.0f), // Cripta norte
            new Vector2(28.0f, 16.5f), // Altar magno
            new Vector2(38.0f, 16.5f), // Cabaña sepulturero
            new Vector2(43.5f, 14.0f)  // Reja gótica salida
        };
        for (int i = 0; i < lanternPositions.Length; i++)
        {
            CreateLantern(lightsRoot, $"Farol_Tenue_{i+1}", lanternPositions[i]);
        }

        // ----------------------------------------------------
        // 16. CONTROLADOR DEL RITUAL DE LAS ÁNIMAS (CLÍMAX)
        // ----------------------------------------------------
        GameObject ritualCtrlObj = GetOrCreateChild(rootEnv, "Controlador_Ritual_Panteon");
        Level4RitualController ritualCtrl = GetOrAddComponent<Level4RitualController>(ritualCtrlObj);
        ritualCtrl.SetupReferences(
            gateObj:          gateObj,
            exitTrigger:      exitTrigger,
            gateSolid:        gateSolid,
            gLight:           gateLight,
            gLabel:           gateLabel,
            sp1:              specter1,
            sp2:              specter2,
            bell:             campanaSfx,
            portal:           portalSfx,
            frenzy:           screamSfx,
            gGlobalLight:     globalLight
        );

        // ----------------------------------------------------
        // 17. CARLOS (PLAYER)
        // ----------------------------------------------------
        SetupPlayer(new Vector2(3.5f, 14.0f), playerLayer);

        // ----------------------------------------------------
        // 18. LA LLORONA (IA PATRULLA LABERÍNTICA CLÍMAX)
        // ----------------------------------------------------
        GameObject wpsRoot = GetOrCreateChild(rootEnv, "Waypoints_Llorona");
        Transform[] waypoints = new Transform[]
        {
            CreateEmpty(wpsRoot.transform, "WP_1_Entrada",    8.0f, 14.0f),
            CreateEmpty(wpsRoot.transform, "WP_2_NorteMaus", 17.0f, 24.0f),
            CreateEmpty(wpsRoot.transform, "WP_3_CentroLab", 21.0f, 14.0f),
            CreateEmpty(wpsRoot.transform, "WP_4_SurCripta", 16.5f,  4.5f),
            CreateEmpty(wpsRoot.transform, "WP_5_AltarMagno",28.0f, 14.0f),
            CreateEmpty(wpsRoot.transform, "WP_6_Cabana",    37.0f, 15.0f),
            CreateEmpty(wpsRoot.transform, "WP_7_SurPanteon",30.0f,  5.0f)
        };

        SetupLlorona(
            pos:        new Vector2(21.0f, 14.0f),
            waypoints:  waypoints,
            wallsLayer: wallsLayer,
            lloronaLayer: lloronaLayer,
            pSpeed:     1.85f,
            cSpeed:     3.2f,
            vDist:      6.0f,
            vAngle:     115f
        );

        // ----------------------------------------------------
        // 19. TRIGGERS NARRATIVOS Y TUTORIALES
        // ----------------------------------------------------
        GameObject triggersRoot = GetOrCreateChild(rootEnv, "Triggers_Tutorial");
        CreateTutorialTrigger(triggersRoot.transform, "Tut_Inicio", new Vector2(3.5f, 14.0f), new Vector2(4.0f, 6.0f),
            "¡EL PANTEÓN DE LAS ÁNIMAS!\nLa Gran Reja este está sellada. Encuentra veladoras y consagra los 3 Altares [F] para romper el sello.",
            showOnStart: true);

        CreateTutorialTrigger(triggersRoot.transform, "Tut_Mausoleos", new Vector2(13.5f, 14.0f), new Vector2(3.0f, 6.0f),
            "¡Los lamentos retumban en las criptas!\nOcúltate dentro de los sarcófagos [E] ante el menor suspiro.");

        CreateTutorialTrigger(triggersRoot.transform, "Tut_Altar", new Vector2(28.0f, 14.0f), new Vector2(3.5f, 6.0f),
            "✦ El Gran Altar de Cempasúchil ✦\nLa luz sagrada crea un santuario donde La Llorona no puede alcanzarte.");

        CreateTutorialTrigger(triggersRoot.transform, "Tut_Salida", new Vector2(40.0f, 14.0f), new Vector2(3.0f, 6.0f),
            "¡La Gran Reja Gótica!\nSi el sello está roto, cruza sin detenerte... ¡¡hacia tu libertad!!");

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

        // GameManager nivel 4
        SetupGameManager(4);

        // Guardar escena
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Level4Builder] === NIVEL 4 (CLÍMAX) CONSTRUIDO Y GUARDADO EXITOSAMENTE ===");
    }

    // ========================================================
    // CONSTRUCTORES AUXILIARES CON GetOrAddComponent
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

        // 1. Collider sólido en la base para que Carlos NO camine por encima de la tumba de piedra
        BoxCollider2D solidBc = GetOrAddComponent<BoxCollider2D>(obj);
        solidBc.size = new Vector2(1.5f, 0.7f);
        solidBc.offset = new Vector2(0f, -0.2f);
        solidBc.isTrigger = false;

        // 2. Trigger circundante para interactuar con [E]
        GameObject triggerChild = GetOrCreateChild(obj, "Trigger_Escondite");
        triggerChild.transform.localPosition = Vector3.zero;
        if (hideSpotLayer >= 0) triggerChild.layer = hideSpotLayer;

        BoxCollider2D triggerBc = GetOrAddComponent<BoxCollider2D>(triggerChild);
        triggerBc.size = new Vector2(2.4f, 1.5f);
        triggerBc.offset = Vector2.zero;
        triggerBc.isTrigger = true;

        HideSpot hs = GetOrAddComponent<HideSpot>(triggerChild);
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("spotType").enumValueIndex = 3; // Porton / Sarcofago
        so.ApplyModifiedProperties();

        // Prompt TextMesh en el trigger
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

        // Collider sólido en las paredes de la cabaña
        BoxCollider2D solidBc = GetOrAddComponent<BoxCollider2D>(obj);
        solidBc.size = new Vector2(3.8f, 3.2f);
        solidBc.offset = new Vector2(0f, 0.4f);
        solidBc.isTrigger = false;

        // Puerta / Escondite en la entrada frontal
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

        // Collider sólido en la mesa de cantera del altar
        BoxCollider2D solidBc = GetOrAddComponent<BoxCollider2D>(obj);
        solidBc.size = new Vector2(1.8f, 0.8f);
        solidBc.offset = new Vector2(0f, -0.3f);
        solidBc.isTrigger = false;

        // Trigger de consagración [F]
        CircleCollider2D cc = GetOrAddComponent<CircleCollider2D>(obj);
        cc.isTrigger = true;
        cc.radius = 2.0f;

        ShrineNiche shrine = GetOrAddComponent<ShrineNiche>(obj);

        // Luz de Santuario consagrado (Cálida Dorada / Fuego Sagrado)
        GameObject lightObj = GetOrCreateChild(obj, "LuzSantuario");
        lightObj.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        Light2D l2d = GetOrAddComponent<Light2D>(lightObj);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.70f, 0.22f);
        l2d.intensity = 2.4f;
        l2d.pointLightOuterRadius = 4.5f;
        l2d.pointLightInnerRadius = 0.8f;
        l2d.enabled = false;
        if (lightObj.GetComponent<LightFlicker>() == null) lightObj.AddComponent<LightFlicker>();

        // Luz Guía (Beacon etéreo azul celeste visible antes de consagrar)
        GameObject beaconObj = GetOrCreateChild(obj, "LuzGuiaEtérea");
        beaconObj.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        Light2D bLight = GetOrAddComponent<Light2D>(beaconObj);
        bLight.lightType = Light2D.LightType.Point;
        bLight.color = new Color(0.40f, 0.85f, 1.0f);
        bLight.intensity = 1.4f;
        bLight.pointLightOuterRadius = 3.8f;
        bLight.pointLightInnerRadius = 0.5f;
        bLight.enabled = true;

        // Aura de santuario protector
        GameObject dome = GetOrCreateChild(obj, "AuraProtectora");
        dome.transform.localPosition = Vector3.zero;
        Light2D domeLight = GetOrAddComponent<Light2D>(dome);
        domeLight.lightType = Light2D.LightType.Point;
        domeLight.color = new Color(1.0f, 0.85f, 0.30f, 0.40f);
        domeLight.intensity = 1.5f;
        domeLight.pointLightOuterRadius = 3.5f;
        dome.SetActive(false);

        // Indicador de Interacción [F]
        GameObject promptObj = GetOrCreateChild(obj, "PromptAltar");
        promptObj.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        TextMesh tm = GetOrAddComponent<TextMesh>(promptObj);
        tm.text = $"[F] Consagrar {shrineTitle}";
        tm.fontSize = 24;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1.0f, 0.90f, 0.40f);
        MeshRenderer mr = promptObj.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 15;

        // Configuración Serializada del ShrineNiche
        SerializedObject so = new SerializedObject(shrine);
        so.FindProperty("shrineName").stringValue = shrineTitle;
        so.FindProperty("sanctuaryRadius").floatValue = 3.5f;
        so.FindProperty("shrineLight").objectReferenceValue = l2d;
        so.FindProperty("beaconLight").objectReferenceValue = bLight;
        so.FindProperty("sanctuaryDome").objectReferenceValue = dome;
        so.FindProperty("promptObject").objectReferenceValue = promptObj;
        so.FindProperty("promptTextMesh").objectReferenceValue = tm;
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

        // Luz tenue espectral 2D
        GameObject lightChild = GetOrCreateChild(specterObj, "Luz_Espectral");
        lightChild.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        Light2D l2d = GetOrAddComponent<Light2D>(lightChild);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.25f, 0.65f, 1.0f, 1.0f);
        l2d.pointLightOuterRadius = 3.8f;
        l2d.pointLightInnerRadius = 0.5f;
        l2d.intensity = 1.0f;

        // Collider trigger amplio para detección
        BoxCollider2D specterCol = GetOrAddComponent<BoxCollider2D>(specterObj);
        specterCol.size = new Vector2(3.0f, 3.5f);
        specterCol.offset = new Vector2(0f, -0.2f);
        specterCol.isTrigger = true;

        // Componente SpecterAI
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

        // Collider trigger para la meta
        BoxCollider2D bcTrigger = GetOrAddComponent<BoxCollider2D>(gateObj);
        bcTrigger.isTrigger = true;
        bcTrigger.size = new Vector2(3.2f, 4.5f);
        bcTrigger.offset = Vector2.zero;
        if (gateObj.GetComponent<LevelGoal>() == null) gateObj.AddComponent<LevelGoal>();
        exitTrigger = bcTrigger;

        // Barrera sólida bloqueante (hasta romper el sello)
        GameObject barrierObj = GetOrCreateChild(gateObj, "Barrera_Solida_Sello");
        barrierObj.transform.localPosition = Vector3.zero;
        if (wallsLayer >= 0) barrierObj.layer = wallsLayer;
        BoxCollider2D bcSolid = GetOrAddComponent<BoxCollider2D>(barrierObj);
        bcSolid.isTrigger = false;
        bcSolid.size = new Vector2(2.5f, 5.0f);
        gateSolid = bcSolid;

        // Luz de Sellado / Salvación
        GameObject lightObj = GetOrCreateChild(gateObj, "LuzSalvacion");
        lightObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        Light2D l2d = GetOrAddComponent<Light2D>(lightObj);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(0.65f, 0.15f, 0.95f); // Púrpura de sellado inicial
        l2d.intensity = 1.8f;
        l2d.pointLightOuterRadius = 5.5f;
        if (lightObj.GetComponent<LightFlicker>() == null) lightObj.AddComponent<LightFlicker>();
        gateLight = l2d;

        // Cartel luminoso del Sello
        GameObject labelGo = GetOrCreateChild(gateObj, "LabelMeta");
        labelGo.transform.localPosition = new Vector3(0f, 3.2f, 0f);
        TextMesh tm = GetOrAddComponent<TextMesh>(labelGo);
        tm.text = "✦ SELLO DE LAS ÁNIMAS ✦\nAltares Consagrados: 0/3";
        tm.fontSize = 26;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1.0f, 0.45f, 0.35f);
        MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 16;
        gateLabel = tm;
    }

    static void CreateCandlePickup(GameObject parent, string name, Sprite sprite, Vector3 pos)
    {
        GameObject go = CreatePiece(parent, name, pos, sprite, "Props", 5, new Vector3(0.70f, 0.70f, 1f));
        CircleCollider2D cc = GetOrAddComponent<CircleCollider2D>(go);
        cc.isTrigger = true;
        cc.radius = 0.55f;

        GameObject lightGo = GetOrCreateChild(go, "LuzVela");
        Light2D l2d = GetOrAddComponent<Light2D>(lightGo);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.75f, 0.30f);
        l2d.intensity = 1.3f;
        l2d.pointLightOuterRadius = 3.2f;

        if (go.GetComponent<Candle>() == null) go.AddComponent<Candle>();
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

    static void CreateNoiseObstacle(GameObject parent, float x, float y)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Botellas_charcolodo_Nvl1.png").OfType<Sprite>().FirstOrDefault(s => s.name.StartsWith("Botella"));
        GameObject go = GetOrCreateChild(parent, $"Basura_Ruido_{x:F0}_{y:F0}");
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = GetOrAddComponent<SpriteRenderer>(go);
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        BoxCollider2D bc = GetOrAddComponent<BoxCollider2D>(go);
        bc.isTrigger = true;
        bc.size = new Vector2(1.8f, 1.0f);

        if (go.GetComponent<NoiseObstacle>() == null) go.AddComponent<NoiseObstacle>();
    }

    static void CreateStrayDog(GameObject parent, float x, float y)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Perro/perro.png").OfType<Sprite>().FirstOrDefault();
        GameObject go = GetOrCreateChild(parent, "Perro_Callejero_Panteon");
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = GetOrAddComponent<SpriteRenderer>(go);
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        CircleCollider2D cc = GetOrAddComponent<CircleCollider2D>(go);
        cc.isTrigger = true;
        cc.radius = 0.9f;

        if (go.GetComponent<StrayDogAI>() == null) go.AddComponent<StrayDogAI>();

        GameObject eyeLight = GetOrCreateChild(go, "LuzOjos");
        Light2D l2d = GetOrAddComponent<Light2D>(eyeLight);
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.25f, 0.15f);
        l2d.intensity = 0.9f;
        l2d.pointLightOuterRadius = 1.8f;
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
        so.FindProperty("displayDuration").floatValue = showOnStart ? 5.5f : 4.5f;
        so.FindProperty("showOnStart").boolValue = showOnStart;
        so.ApplyModifiedProperties();
    }

    static void SetupPlayer(Vector2 pos, int playerLayer)
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
    }

    static void SetupLlorona(Vector2 pos, Transform[] waypoints, int wallsLayer, int lloronaLayer,
                              float pSpeed, float cSpeed, float vDist, float vAngle)
    {
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona == null)
        {
            GameObject lGo = new GameObject("Llorona");
            Rigidbody2D rb = lGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            lGo.AddComponent<CircleCollider2D>().radius = 0.5f;
            SpriteRenderer sr = lGo.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Llorona/llorona.png");
            sr.sortingOrder = 5;
            llorona = lGo.AddComponent<LloronaAI>();
            lGo.AddComponent<LloronaAnimation>();
        }
        llorona.transform.position = new Vector3(pos.x, pos.y, 0f);
        llorona.transform.rotation = Quaternion.identity;
        var lrb = llorona.GetComponent<Rigidbody2D>();
        if (lrb != null) { lrb.freezeRotation = true; lrb.gravityScale = 0f; }
        if (lloronaLayer >= 0) llorona.gameObject.layer = lloronaLayer;

        SerializedObject so = new SerializedObject(llorona);
        SerializedProperty wps = so.FindProperty("waypoints");
        wps.arraySize = waypoints.Length;
        for (int i = 0; i < waypoints.Length; i++)
            wps.GetArrayElementAtIndex(i).objectReferenceValue = waypoints[i];

        if (wallsLayer >= 0) so.FindProperty("obstacleMask").intValue = 1 << wallsLayer;

        LloronaSettings s = AssetDatabase.LoadAssetAtPath<LloronaSettings>("Assets/Settings/LloronaSettings_Level4.asset");
        if (s != null) so.FindProperty("settings").objectReferenceValue = s;

        so.FindProperty("patrolSpeed").floatValue = pSpeed;
        so.FindProperty("chaseSpeed").floatValue = cSpeed;
        so.FindProperty("viewDistance").floatValue = vDist;
        so.FindProperty("viewAngle").floatValue = vAngle;
        so.ApplyModifiedProperties();
    }

    static Transform CreateEmpty(Transform parent, string name, float x, float y)
    {
        GameObject go = GetOrCreateChild(parent.gameObject, name);
        go.transform.position = new Vector3(x, y, 0f);
        return go.transform;
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
