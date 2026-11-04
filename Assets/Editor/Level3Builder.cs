// Assets/Editor/Level3Builder.cs
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
/// Constructor oficial y automatizado del Nivel 3: El Atrio de la Parroquia de Dolores.
/// Reconstrucción fiel con arte real:
/// - Fachada monumental completa de la Parroquia de Nuestra Señora de los Dolores (Dolores Hidalgo, Gto.)
/// - Gran Escalinata Monumental de cantera rosa con efecto interactivo de subida de escalones y sonido de piedra
/// - Amplia explanada despejada y solemne con fuente octagonal central
/// - Escondites temáticos colocados en los flancos laterales: Confesionarios coloniales, bancas de hierro, criptas y cipreses
/// - Cámara panorámica con zoom amplio (orthoSize 5.6) para apreciar la arquitectura completa del atrio y las torres
/// - Meta sagrada en las puertas superiores de la Parroquia
/// </summary>
public static class Level3Builder
{
    const string ScenePath = "Assets/Scenes/Level3_Atrio.unity";

    [MenuItem("La Llorona/Construir Nivel 3")]
    public static void BuildLevel3Scene()
    {
        Debug.Log("[Level3Builder] Iniciando reconstrucción oficial del Nivel 3 (El Atrio de Dolores)...");

        // 1. Asegurar slicing y recarga de sprites oficiales
        SpriteSheetSlicer.SliceNivel3();
        AssetDatabase.Refresh();

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Construir Nivel 3 Oficial");

        int wallsLayer = LayerMask.NameToLayer("Walls");
        int hideSpotLayer = LayerMask.NameToLayer("HideSpot");
        int playerLayer = LayerMask.NameToLayer("Player");
        int lloronaLayer = LayerMask.NameToLayer("Llorona");

        // Cargar sprites oficiales del Nivel 3
        Sprite catedralSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/catedral.png")
            .OfType<Sprite>().FirstOrDefault();

        Sprite[] accSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/accesorios.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] fuenteSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/fuente.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] rejaSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/rejaYfaroles.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] ofrendaSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/ofrendas.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();

        Sprite escalerasSprite = EnsureStairsSprite();

        // 2. Grid y Suelo con Tilemap
        GameObject gridGo = GameObject.Find("Grid") ?? new GameObject("Grid");
        if (gridGo.GetComponent<Grid>() == null) gridGo.AddComponent<Grid>();

        Tilemap suelo = GetOrCreateTilemap(gridGo.transform, "Suelo", 0, -1, false);
        Tilemap paredes = GetOrCreateTilemap(gridGo.transform, "Paredes", 1, wallsLayer, true);
        suelo.ClearAllTiles();
        paredes.ClearAllTiles();

        TileBase[] allTiles = LoadTiles();
        // Llenar el atrio: X de -2 a 38, Y de 0 a 24
        for (int x = -2; x <= 38; x++)
        {
            for (int y = 0; y <= 24; y++)
            {
                // Alternancia de baldosas de cantera y empedrado colonial
                TileBase t = ((x + y) % 5 == 0) ? allTiles[1] : allTiles[0];
                suelo.SetTile(new Vector3Int(x, y, 0), t);
            }
        }

        // Limpiar raíces obsoletas del Nivel 3 previo
        string[] oldRoots = new string[] { "Props", "Hazards", "Barricadas", "Decoracion", "Luces", "Limites", "Escalinata" };
        foreach (var r in oldRoots)
        {
            GameObject ex = GameObject.Find(r);
            if (ex != null) Undo.DestroyObjectImmediate(ex);
        }

        // 3. Fachada Monumental de la Parroquia de Nuestra Señora de los Dolores (Al Norte del atrio)
        Transform bgGroup = FreshRoot("Fondo_Monumental");
        if (catedralSprite != null)
        {
            GameObject catGo = new GameObject("Parroquia_Dolores_Hidalgo");
            catGo.transform.SetParent(bgGroup, false);
            catGo.transform.position = new Vector3(18.0f, 13.6f, 0f);
            catGo.transform.localScale = Vector3.one * 1.06f; // Proporción áurea que permite apreciar las torres completas

            SpriteRenderer srCat = catGo.AddComponent<SpriteRenderer>();
            srCat.sprite = catedralSprite;
            srCat.sortingOrder = 4; // Por encima de los escalones de cantera
            srCat.color = new Color(0.97f, 0.95f, 1.0f);

            // Muros sólidos que protegen las alas laterales de la fachada, dejando libre el gran pórtico central
            BoxCollider2D leftWallCol = catGo.AddComponent<BoxCollider2D>();
            leftWallCol.size = new Vector2(4.8f, 2.5f);
            leftWallCol.offset = new Vector2(-4.6f, 1.25f);

            BoxCollider2D rightWallCol = catGo.AddComponent<BoxCollider2D>();
            rightWallCol.size = new Vector2(4.8f, 2.5f);
            rightWallCol.offset = new Vector2(4.6f, 1.25f);

            if (wallsLayer >= 0) catGo.layer = wallsLayer;
        }

        // 4. Gran Escalinata Monumental (Efecto interactivo de subida de escalones)
        Transform escalinataGroup = FreshRoot("Escalinata");
        CreateEscalinataMonumental(escalinataGroup, escalerasSprite);

        // 5. Perímetro y Rejas
        Transform rejasGroup = FreshRoot("Rejas_Perimetro");
        Sprite rejaSup = rejaSprites.FirstOrDefault(s => s.name == "Reja_Superior") ?? rejaSprites.ElementAtOrDefault(0);
        Sprite rejaInf = rejaSprites.FirstOrDefault(s => s.name == "Reja_Inferior") ?? rejaSprites.ElementAtOrDefault(6);
        Sprite farolaPie = rejaSprites.FirstOrDefault(s => s.name == "Farola_Pie_N3") ?? rejaSprites.ElementAtOrDefault(5);

        // Reja sur inferior (dejando entrada central en X = 18)
        for (float rx = 2.5f; rx <= 13.5f; rx += 5.5f)
        {
            CreateFencePiece(rejasGroup, $"Reja_Sur_Oeste_{rx:0}", rejaInf, rx, 0.8f, wallsLayer, new Vector2(5.5f, 1.0f));
        }
        for (float rx = 22.5f; rx <= 33.5f; rx += 5.5f)
        {
            CreateFencePiece(rejasGroup, $"Reja_Sur_Este_{rx:0}", rejaInf, rx, 0.8f, wallsLayer, new Vector2(5.5f, 1.0f));
        }

        // Farolas de pie distribuidas en el perímetro del atrio
        float[] farolasX = new float[] { 5.5f, 11.5f, 24.5f, 30.5f };
        foreach (float fx in farolasX)
        {
            CreateStandingLantern(rejasGroup, $"Farola_Sur_{fx:0}", farolaPie, fx, 2.0f, wallsLayer);
            CreateStandingLantern(rejasGroup, $"Farola_Norte_{fx:0}", farolaPie, fx, 11.5f, wallsLayer);
        }

        // Límites invisibles sólidos
        Transform limits = FreshRoot("Limites_Atrio");
        CreateInvisibleWall(limits, "Lim_Oeste",  2.0f,  8.0f, 18f, wallsLayer);
        CreateInvisibleWall(limits, "Lim_Este",   34.0f,  8.0f, 18f, wallsLayer);
        CreateInvisibleWall(limits, "Lim_Sur",    18.0f,  0.2f,  1f, wallsLayer, 36.0f);
        CreateInvisibleWall(limits, "Lim_Norte",  18.0f, 16.5f,  1f, wallsLayer, 36.0f);

        // 6. Fuente Central Monumental (Espacio abierto y despejado en el atrio)
        Transform props = FreshRoot("Props_Atrio");
        Sprite fuenteSprite = fuenteSprites.FirstOrDefault(s => s.name == "Fuente_Octagonal_N3") ?? fuenteSprites.ElementAtOrDefault(0);
        if (fuenteSprite != null)
        {
            GameObject fGo = new GameObject("Fuente_Octagonal_Central");
            fGo.transform.SetParent(props, false);
            fGo.transform.position = new Vector3(18.0f, 6.2f, 0f);
            fGo.transform.localScale = Vector3.one * 1.15f;

            SpriteRenderer srF = fGo.AddComponent<SpriteRenderer>();
            srF.sprite = fuenteSprite;
            srF.sortingOrder = 3;

            CircleCollider2D cc = fGo.AddComponent<CircleCollider2D>();
            cc.radius = 2.2f;
            cc.offset = new Vector2(0f, -0.2f);
            if (wallsLayer >= 0) fGo.layer = wallsLayer;

            // Luz suave azulada reflejada en el agua
            GameObject lf = new GameObject("Luz_Agua_Fuente");
            lf.transform.SetParent(fGo.transform, false);
            Light2D l2df = lf.AddComponent<Light2D>();
            l2df.lightType = Light2D.LightType.Point;
            l2df.color = new Color(0.45f, 0.75f, 1.0f);
            l2df.intensity = 1.1f;
            l2df.pointLightOuterRadius = 4.8f;
        }

        // 7. Escondites Temáticos en los Flancos Laterales ([E])
        Sprite sConfesionario = accSprites.FirstOrDefault(s => s.name == "Confesionario_N3") ?? accSprites.ElementAtOrDefault(1);
        Sprite sBanca         = accSprites.FirstOrDefault(s => s.name == "Banca_Hierro_N3") ?? accSprites.ElementAtOrDefault(0);
        Sprite sCripta        = accSprites.FirstOrDefault(s => s.name == "Cripta_Cantera_N3") ?? accSprites.ElementAtOrDefault(2);
        Sprite sCipres        = accSprites.FirstOrDefault(s => s.name == "Cipres_Colonial_1") ?? accSprites.ElementAtOrDefault(4);
        Sprite sAltar         = accSprites.FirstOrDefault(s => s.name == "Altar_Atrio_N3") ?? accSprites.ElementAtOrDefault(8);

        // Confesionarios coloniales en las alas laterales
        CreateHideSpotOfficial(props, "Escondite_Confesionario_Oeste", sConfesionario,  7.5f, 9.5f, 1.15f, hideSpotLayer);
        CreateHideSpotOfficial(props, "Escondite_Confesionario_Este",  sConfesionario, 28.5f, 9.5f, 1.15f, hideSpotLayer);

        // Bancas de hierro forjado
        CreateHideSpotOfficial(props, "Escondite_Banca_SurOeste", sBanca,  7.5f, 4.0f, 1.1f, hideSpotLayer);
        CreateHideSpotOfficial(props, "Escondite_Banca_SurEste",  sBanca, 28.5f, 4.0f, 1.1f, hideSpotLayer);

        // Criptas de cantera
        CreateHideSpotOfficial(props, "Escondite_Cripta_NorteOeste", sCripta,  5.0f, 6.8f, 1.1f, hideSpotLayer);
        CreateHideSpotOfficial(props, "Escondite_Cripta_NorteEste",  sCripta, 31.0f, 6.8f, 1.1f, hideSpotLayer);

        // Altar al aire libre en la plazoleta lateral
        if (sAltar != null)
        {
            CreateSolidProp(props, "Altar_Atrio", sAltar, 7.0f, 12.2f, 1.15f, wallsLayer, new Vector2(4.6f, 1.6f), new Vector2(0f, 0.8f));
            GameObject lAltar = new GameObject("Luz_Altar");
            lAltar.transform.SetParent(props, false);
            lAltar.transform.position = new Vector3(7.0f, 12.6f, 0f);
            Light2D l2dAltar = lAltar.AddComponent<Light2D>();
            l2dAltar.lightType = Light2D.LightType.Point;
            l2dAltar.color = new Color(1.0f, 0.70f, 0.25f);
            l2dAltar.intensity = 1.6f;
            l2dAltar.pointLightOuterRadius = 4.8f;
            lAltar.AddComponent<LightFlicker>();
        }

        // Cipreses coloniales
        if (sCipres != null)
        {
            float[] cipresesX = new float[] { 4.2f, 5.5f, 30.5f, 31.8f };
            foreach (float cx in cipresesX)
            {
                CreateSolidProp(props, $"Cipres_{cx:0}", sCipres, cx, 8.0f, 1.25f, wallsLayer, new Vector2(0.9f, 0.8f), new Vector2(0f, 0.45f));
            }
        }

        // 8. Decals, Obstáculos y Coleccionables (Despejados del corredor central)
        Transform hazards = FreshRoot("Obstaculos_Ofrendas");
        Sprite sAguaBendita = ofrendaSprites.FirstOrDefault(s => s.name == "Charco_AguaBendita") ?? ofrendaSprites.ElementAtOrDefault(0);
        Sprite sTepalcate   = ofrendaSprites.FirstOrDefault(s => s.name == "Vidrios_Jarron_Roto") ?? ofrendaSprites.ElementAtOrDefault(5);
        Sprite sSahumador   = ofrendaSprites.FirstOrDefault(s => s.name == "Sahumador_Roto") ?? ofrendaSprites.ElementAtOrDefault(6);
        Sprite sVelaRoja    = ofrendaSprites.FirstOrDefault(s => s.name == "Vaso_Vela_Roja_1") ?? ofrendaSprites.ElementAtOrDefault(1);

        // 2 Charcos laterales (sin bloquear el paso central)
        CreateMudPuddleOfficial(hazards, sAguaBendita, 10.5f, 6.5f, 0.85f);
        CreateMudPuddleOfficial(hazards, sAguaBendita, 25.5f, 6.5f, 0.85f);

        // 2 Trampas de ruido tácticas
        CreateNoiseObstacleOfficial(hazards, sTepalcate, 11.5f, 3.8f);
        CreateNoiseObstacleOfficial(hazards, sSahumador, 24.5f, 3.8f);

        // Velas coleccionables de ofrenda
        CreateCandlePickupOfficial(hazards, "Vela_Ofrenda_1", sVelaRoja, new Vector2( 7.5f, 5.5f));
        CreateCandlePickupOfficial(hazards, "Vela_Ofrenda_2", sVelaRoja, new Vector2(18.0f, 3.8f));
        CreateCandlePickupOfficial(hazards, "Vela_Ofrenda_3", sVelaRoja, new Vector2(28.5f, 5.5f));

        // 9. Waypoints de Patrulla para La Llorona (Circuito amplio en la explanada del atrio)
        Transform wpRoot = FreshRoot("Waypoints");
        Transform[] wps = new Transform[6];
        wps[0] = CreateEmpty(wpRoot, "WP_Norte",    18.0f,  9.8f);
        wps[1] = CreateEmpty(wpRoot, "WP_Noreste",  23.5f,  8.5f);
        wps[2] = CreateEmpty(wpRoot, "WP_Sureste",  23.5f,  4.8f);
        wps[3] = CreateEmpty(wpRoot, "WP_Sur",      18.0f,  3.5f);
        wps[4] = CreateEmpty(wpRoot, "WP_Suroeste", 12.5f,  4.8f);
        wps[5] = CreateEmpty(wpRoot, "WP_Noroeste", 12.5f,  8.5f);

        // 10. META DEL NIVEL: Puertas Principales de la Parroquia de Dolores (Coronando las escaleras)
        Transform exitGroup = FreshRoot("Salida_Meta");
        CreateParroquiaEntranceGoal(exitGroup, 18.0f, 14.2f);

        // 11. Iluminación Global URP 2D (Noche clara en el atrio: 0.52f)
        EnsureGlobalLight(0.52f);

        // 12. Player (Carlos inicia en la entrada sur del atrio, mirando al norte hacia la Parroquia)
        SetupPlayer(new Vector2(18.0f, 1.8f), playerLayer);

        // 13. La Llorona (Inicia patrullando la explanada frente a las escaleras)
        SetupLloronaCircular(new Vector2(18.0f, 9.8f), wps, wallsLayer, lloronaLayer, 1.6f, 2.8f, 5.5f, 110.0f);

        // 14. Cámara Panorámica y Confiner 2D (Zoom amplio que permite ver la catedral completa y el atrio)
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5.6f;
        cam.transform.position = new Vector3(18.0f, 5.0f, -10f);
        cam.backgroundColor = new Color(0.04f, 0.03f, 0.08f);

        CameraFollow2D camFollow = cam.GetComponent<CameraFollow2D>() ?? cam.gameObject.AddComponent<CameraFollow2D>();
        camFollow.SetOrthoSize(5.6f);
        camFollow.SetOffset(new Vector3(0f, 2.2f, -10f));
        camFollow.SetBounds(new Vector2(10.0f, 5.0f), new Vector2(26.0f, 22.0f));

        // 15. Triggers de Tutorial narrativo
        Transform tutRoot = FreshRoot("TutorialTriggers");
        CreateTutorialTrigger(tutRoot, "Tutorial_Inicio",
            new Vector2(18.0f, 2.0f), new Vector2(8f, 3f),
            "¡El Atrio de la Parroquia de Dolores!\nSube las escaleras para entrar al santuario sagrado.\n• Recoge veladoras y pulsa [F] para orar y distraer a La Llorona.\n• Pulsa [E] para esconderte en bancas y confesionarios.",
            showOnStart: true);
        CreateTutorialTrigger(tutRoot, "Tutorial_Fuente",
            new Vector2(18.0f, 7.8f), new Vector2(8f, 3f),
            "La Llorona ronda la explanada.\nUsa las ofrendas [F] o espera oculta con [E] para alcanzar las escaleras de la iglesia.");
        CreateTutorialTrigger(tutRoot, "Tutorial_Parroquia",
            new Vector2(18.0f, 11.2f), new Vector2(8f, 3f),
            "¡Las escaleras del santuario!\nSube los escalones de cantera hacia las puertas de la Parroquia.");

        // 16. GameManager y AudioManager
        SetupGameManager(3);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.SaveScene(scene, ScenePath);
        UIBuilder.BuildLevel3UICanvas();
        Debug.Log("[Level3Builder] ¡Nivel 3 oficial (El Atrio de Dolores) reconstruido y guardado con éxito!");
    }

    static Sprite EnsureStairsSprite()
    {
        string spritePath = "Assets/Sprites/Environment/Nivel 3/escaleras_cantera.png";
        if (!File.Exists(spritePath))
        {
            int w = 512;
            int h = 256;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color stoneRosa = new Color(0.74f, 0.54f, 0.44f, 1f);
            Color stoneRiser = new Color(0.48f, 0.32f, 0.25f, 1f);
            Color stoneShadow = new Color(0.24f, 0.14f, 0.10f, 1f);
            Color stoneHighlight = new Color(0.92f, 0.82f, 0.74f, 1f);
            Color balustradeColor = new Color(0.68f, 0.48f, 0.38f, 1f);
            Color balustradeHighlight = new Color(0.88f, 0.76f, 0.68f, 1f);
            Color balustradeShadow = new Color(0.35f, 0.22f, 0.17f, 1f);

            int numSteps = 5;
            int stepHeight = h / numSteps; // ~51 px

            for (int y = 0; y < h; y++)
            {
                int localY = y % stepHeight;

                for (int x = 0; x < w; x++)
                {
                    // Pretiles / balaustradas laterales (columnas de cantera en los extremos)
                    if (x < 44 || x > (w - 45))
                    {
                        bool isBorder = (x == 0 || x == 43 || x == (w - 44) || x == (w - 1));
                        bool isInnerEdge = (x == 40 || x == (w - 41));

                        if (isBorder)
                        {
                            tex.SetPixel(x, y, balustradeShadow);
                        }
                        else if (isInnerEdge)
                        {
                            tex.SetPixel(x, y, balustradeHighlight);
                        }
                        else
                        {
                            float noise = Mathf.PerlinNoise(x * 0.12f, y * 0.12f) * 0.15f - 0.07f;
                            Color c = Color.Lerp(balustradeShadow, balustradeColor, (x < 44 ? (float)x / 44f : (float)(w - 1 - x) / 44f));
                            c = new Color(Mathf.Clamp01(c.r + noise), Mathf.Clamp01(c.g + noise), Mathf.Clamp01(c.b + noise), 1f);
                            tex.SetPixel(x, y, c);
                        }
                        continue;
                    }

                    // Escalones centrales (huella, contrahuella, bisel y juntas)
                    float blockNoise = Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 0.12f - 0.06f;
                    bool isJoint = (x % 72 == 0);

                    if (localY < 18) // Contrahuella (cara vertical del escalón en sombra)
                    {
                        Color c = isJoint ? stoneShadow : stoneRiser;
                        c = new Color(Mathf.Clamp01(c.r + blockNoise), Mathf.Clamp01(c.g + blockNoise), Mathf.Clamp01(c.b + blockNoise), 1f);
                        tex.SetPixel(x, y, c);
                    }
                    else if (localY < 22) // Bisel superior iluminado del escalón
                    {
                        Color c = isJoint ? stoneRiser : stoneHighlight;
                        tex.SetPixel(x, y, c);
                    }
                    else if (localY == 22) // Sombra de contacto
                    {
                        tex.SetPixel(x, y, stoneShadow);
                    }
                    else // Huella (superficie horizontal superior del escalón)
                    {
                        Color c = isJoint ? stoneShadow : stoneRosa;
                        float depthGrad = (float)(localY - 22) / (stepHeight - 22);
                        c = Color.Lerp(c, stoneRosa * 1.08f, depthGrad);
                        c = new Color(Mathf.Clamp01(c.r + blockNoise), Mathf.Clamp01(c.g + blockNoise), Mathf.Clamp01(c.b + blockNoise), 1f);
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(spritePath, bytes);
            AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);

            TextureImporter ti = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.spritePixelsPerUnit = 48f;
                ti.filterMode = FilterMode.Point;
                ti.spritePivot = new Vector2(0.5f, 0.05f);
                ti.SaveAndReimport();
            }
            UnityEngine.Object.DestroyImmediate(tex);
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
    }

    static void CreateEscalinataMonumental(Transform parent, Sprite stairsSprite)
    {
        if (stairsSprite == null) return;

        GameObject go = new GameObject("Escalinata_Monumental");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(18.0f, 11.4f, 0f);
        go.transform.localScale = new Vector3(1.05f, 0.95f, 1.0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = stairsSprite;
        sr.sortingOrder = 3;

        // Trigger del efecto de subir escaleras
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(9.8f, 2.9f);
        bc.offset = new Vector2(0f, 1.15f);

        // Componente de comportamiento para la subida
        go.AddComponent<StairClimbEffect>();

        // Farolas coloniales sobre los pretiles laterales de la escalinata
        Sprite farolaSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/Nivel 3/rejaYfaroles.png")
            .OfType<Sprite>().FirstOrDefault(s => s.name == "Farola_Pie_N3");

        if (farolaSprite != null)
        {
            CreateStairLantern(go.transform, "Farola_Pretil_Oeste", farolaSprite, -4.6f, 1.0f);
            CreateStairLantern(go.transform, "Farola_Pretil_Este", farolaSprite, 4.6f, 1.0f);
        }

        Undo.RegisterCreatedObjectUndo(go, "Escalinata_Monumental");
    }

    static void CreateStairLantern(Transform parent, string name, Sprite sprite, float localX, float localY)
    {
        GameObject lGo = new GameObject(name);
        lGo.transform.SetParent(parent, false);
        lGo.transform.localPosition = new Vector3(localX, localY, 0f);

        SpriteRenderer sr = lGo.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;

        GameObject lightGo = new GameObject("Luz");
        lightGo.transform.SetParent(lGo.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.82f, 0.45f);
        l2d.intensity = 1.8f;
        l2d.pointLightOuterRadius = 5.2f;
        lightGo.AddComponent<LightFlicker>();
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

    static void CreateHideSpotOfficial(Transform parent, string name, Sprite sprite, float x, float y, float scale, int layer)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.8f, 1.4f);
        col.offset = new Vector2(0f, 0.35f);
        if (layer >= 0) go.layer = layer;

        GameObject hp = new GameObject("HidePoint");
        hp.transform.SetParent(go.transform, false);
        hp.transform.localPosition = new Vector3(0f, 0.2f, 0f);

        HideSpot hs = go.AddComponent<HideSpot>();
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("hidePoint").objectReferenceValue = hp.transform;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateSolidProp(Transform parent, string name, Sprite sprite, float x, float y, float scale, int layer, Vector2 colSize, Vector2 colOffset)
    {
        if (sprite == null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = colSize;
        col.offset = colOffset;
        if (layer >= 0) go.layer = layer;

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void CreateMudPuddleOfficial(Transform parent, Sprite sprite, float x, float y, float scale = 1.0f)
    {
        GameObject go = new GameObject("Charco_AguaBendita");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 1.2f;

        go.AddComponent<MudPuddle>();
        Undo.RegisterCreatedObjectUndo(go, "Charco_AguaBendita");
    }

    static void CreateNoiseObstacleOfficial(Transform parent, Sprite sprite, float x, float y)
    {
        GameObject go = new GameObject("Tepalcates_Ruido");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(1.5f, 1.2f);

        go.AddComponent<NoiseObstacle>();
        Undo.RegisterCreatedObjectUndo(go, "Tepalcates_Ruido");
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
        col.radius = 1.3f;
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

    static void CreateParroquiaEntranceGoal(Transform parent, float x, float y)
    {
        GameObject go = new GameObject("Portal_Parroquia_Meta");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);

        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(5.0f, 2.2f);
        bc.offset = new Vector2(0f, 0f);
        go.AddComponent<LevelGoal>();

        // Luz divina dorada y acogedora de entrada a la Parroquia
        GameObject lightGo = new GameObject("Luz_Entrada_Parroquia");
        lightGo.transform.SetParent(go.transform, false);
        Light2D l2d = lightGo.AddComponent<Light2D>();
        l2d.lightType = Light2D.LightType.Point;
        l2d.color = new Color(1.0f, 0.88f, 0.48f);
        l2d.intensity = 2.6f;
        l2d.pointLightOuterRadius = 8.5f;
        lightGo.AddComponent<LightFlicker>();

        // Letrero visible encima de la puerta
        GameObject labelGo = new GameObject("LabelEntrada");
        labelGo.transform.SetParent(go.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        TextMesh tm = labelGo.AddComponent<TextMesh>();
        tm.text = "PUERTAS DE LA PARROQUIA (SANTUARIO)";
        tm.fontSize = 28;
        tm.characterSize = 0.11f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1.0f, 0.92f, 0.55f);
        MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
        mr.sortingOrder = 15;

        Undo.RegisterCreatedObjectUndo(go, "Portal_Parroquia_Meta");
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
        Light2D gl = glGo.GetComponent<Light2D>() ?? glGo.AddComponent<Light2D>();
        gl.lightType = Light2D.LightType.Global;
        gl.color = new Color(0.90f, 0.88f, 0.82f);
        gl.intensity = intensity;
    }

    static void SetupPlayer(Vector2 spawnPos, int layer)
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

        PlayerMovement pm = playerGo.GetComponent<PlayerMovement>() ?? playerGo.AddComponent<PlayerMovement>();
        if (playerGo.GetComponent<PlayerAnimation>() == null) playerGo.AddComponent<PlayerAnimation>();
        if (playerGo.GetComponent<HideSystem>() == null) playerGo.AddComponent<HideSystem>();

        CircleCollider2D col = playerGo.GetComponent<CircleCollider2D>() ?? playerGo.AddComponent<CircleCollider2D>();
        col.radius = 0.28f;
        col.offset = new Vector2(0f, -0.45f);

        Rigidbody2D rb = playerGo.GetComponent<Rigidbody2D>() ?? playerGo.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Candle.ResetInventory();
    }

    static void SetupLloronaCircular(Vector2 spawnPos, Transform[] wps, int wallsLayer, int lloronaLayer, float patrolSpeed, float chaseSpeed, float visionDist, float fov)
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

        LloronaAI ai = lloronaGo.GetComponent<LloronaAI>() ?? lloronaGo.AddComponent<LloronaAI>();
        ai.Waypoints = wps;

        SerializedObject so = new SerializedObject(ai);
        var pSpeed = so.FindProperty("patrolSpeed");
        if (pSpeed != null) pSpeed.floatValue = patrolSpeed;
        var cSpeed = so.FindProperty("chaseSpeed");
        if (cSpeed != null) cSpeed.floatValue = chaseSpeed;
        var oMask = so.FindProperty("obstacleMask");
        if (oMask != null) oMask.intValue = 1 << wallsLayer;
        so.ApplyModifiedProperties();
    }

    static void CreateTutorialTrigger(Transform parent, string name, Vector2 pos, Vector2 size, string text, bool showOnStart = false)
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
        var startProp = so.FindProperty("showOnStart");
        if (startProp != null) startProp.boolValue = showOnStart;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void SetupGameManager(int levelNumber)
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
