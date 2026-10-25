// Assets/Editor/SpriteSheetSlicer.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Corta y configura con máxima precisión milimétrica todas las hojas de sprites del proyecto
/// según la especificación oficial de PROMPT_VISUAL.md:
/// - Filtro Point (no filter)
/// - Sin compresión (Compression None)
/// - Pivotes correctos (Bottom Center para elementos verticales, Center para charcos/botellas/tiles)
/// - PPU normalizado respecto a Carlos (1.2 u de alto)
/// </summary>
public static class SpriteSheetSlicer
{
    [MenuItem("La Llorona/Cortar hojas de sprites")]
    public static void SliceAllSpriteSheets()
    {
        Debug.Log("[SpriteSheetSlicer] Iniciando corte y configuración oficial de sprites...");

        SliceInterfaz();
        SlicePuestosNvl1();
        SliceContenedoresSacosNvl1();
        SliceBotellasCharcolodoNvl1();
        SliceSalidas();
        SlicePerro();
        SliceTilesNvl2();
        SliceTilesAdoquinNvl2();
        SlicePuertasNvl2();
        SliceAccesoriosNvl2();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SpriteSheetSlicer] ¡Todas las hojas de sprites fueron cortadas y configuradas exitosamente!");
    }

    // ── 1. UI/interfaz.png (2400 × 1368) ─────────────────────────────────────
    public static void SliceInterfaz()
    {
        string path = "Assets/Sprites/UI/interfaz.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("Corazon_lleno",    new Rect(116, 770, 369, 493), new Vector2(0.5f, 0.5f)),
            new SliceData("Corazon_vacio",    new Rect(649, 770, 352, 493), new Vector2(0.5f, 0.5f)),
            new SliceData("Icono_vela",       new Rect(1209, 769, 310, 473), new Vector2(0.5f, 0.5f)),
            new SliceData("Icono_ojo",        new Rect(1715, 760, 584, 461), new Vector2(0.5f, 0.5f)),
            new SliceData("Barra_marco",      new Rect(96, 404, 2208, 293),  new Vector2(0.5f, 0.5f), new Vector4(40, 20, 40, 20)),
            new SliceData("Placa_normal",     new Rect(89, 57, 708, 258),    new Vector2(0.5f, 0.5f), new Vector4(35, 25, 35, 25)),
            new SliceData("Placa_resaltada",  new Rect(846, 48, 707, 276),   new Vector2(0.5f, 0.5f), new Vector4(35, 25, 35, 25)),
            new SliceData("Placa_presionada", new Rect(1602, 57, 708, 258),  new Vector2(0.5f, 0.5f), new Vector4(35, 25, 35, 25))
        };

        ApplySlicing(path, slices, 100f);
    }

    // ── 2. Environment/Puestos_Nivl1.png (1536 × 1024) ───────────────────────
    // Escala objetivo: 5 u de ancho -> PPU ≈ 126f
    public static void SlicePuestosNvl1()
    {
        string path = "Assets/Sprites/Environment/Puestos_Nivl1.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("Puesto_frutas",   new Rect(84, 198, 628, 641), new Vector2(0.5f, 0.12f)),
            new SliceData("Puesto_verduras", new Rect(821, 189, 635, 650), new Vector2(0.5f, 0.12f))
        };

        ApplySlicing(path, slices, 126f);
    }

    // ── 3. Environment/Contenedores_Sacos_Nvl1.png (1536 × 1024) ─────────────
    // Escala objetivo: huacales 1.8-2 u, canastas 1.2 u, costales 1.6 u -> PPU ≈ 190f
    public static void SliceContenedoresSacosNvl1()
    {
        string path = "Assets/Sprites/Environment/Contenedores_Sacos_Nvl1.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            // Fila 1 (arriba)
            new SliceData("Huacales_apilados",    new Rect(116, 571, 341, 398), new Vector2(0.5f, 0.15f)),
            new SliceData("Huacal_roto_paja",     new Rect(572, 570, 393, 387), new Vector2(0.5f, 0.15f)),
            new SliceData("Canasta_vacia",        new Rect(1079, 599, 335, 310), new Vector2(0.5f, 0.15f)),
            // Fila 2 (abajo)
            new SliceData("Canasta_chiles",       new Rect(137, 130, 334, 337), new Vector2(0.5f, 0.15f)),
            new SliceData("Costales_apilados",    new Rect(594, 88, 362, 411), new Vector2(0.5f, 0.15f)),
            new SliceData("Costal_maiz_abierto",  new Rect(1072, 76, 341, 419), new Vector2(0.5f, 0.15f))
        };

        ApplySlicing(path, slices, 190f);
    }

    // ── 4. Environment/Botellas_charcolodo_Nvl1.png (1536 × 1024) ───────────
    // Escala objetivo: charcos 1.4, 2.6, 3.8 u; botellas 0.55 u alto -> PPU ≈ 165f
    public static void SliceBotellasCharcolodoNvl1()
    {
        string path = "Assets/Sprites/Environment/Botellas_charcolodo_Nvl1.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            // Filas 1 y 2: Charcos (3 por fila: pequeño, mediano, grande)
            new SliceData("Charco_0", new Rect(121, 789, 238, 154), new Vector2(0.5f, 0.5f)),
            new SliceData("Charco_1", new Rect(475, 765, 358, 191), new Vector2(0.5f, 0.5f)),
            new SliceData("Charco_2", new Rect(800, 725, 636, 250), new Vector2(0.5f, 0.5f)),
            new SliceData("Charco_3", new Rect(113, 425, 337, 266), new Vector2(0.5f, 0.5f)),
            new SliceData("Charco_4", new Rect(425, 425, 372, 274), new Vector2(0.5f, 0.5f)),
            new SliceData("Charco_5", new Rect(856, 463, 595, 272), new Vector2(0.5f, 0.5f)),

            // Fila 3: Botellas de pie y acostadas
            new SliceData("Botella_verde",                     new Rect(79, 49, 168, 179),  new Vector2(0.5f, 0.5f)),
            new SliceData("Botella_ambar",                     new Rect(359, 51, 241, 154), new Vector2(0.5f, 0.5f)),
            new SliceData("Botella_transparente_acostada",     new Rect(664, 49, 257, 165), new Vector2(0.5f, 0.5f)),
            new SliceData("Botella_ambar_etiqueta_acostada",   new Rect(1135, 48, 80, 199), new Vector2(0.5f, 0.5f)),

            // Fila 4: Botella rota, vidrios rotos y caídas
            new SliceData("Botella_rota",    new Rect(118, 264, 78, 189),  new Vector2(0.5f, 0.5f)),
            new SliceData("Vidrios_rotos",   new Rect(398, 264, 80, 188),  new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_caidas", new Rect(664, 279, 212, 132), new Vector2(0.5f, 0.5f)),
            new SliceData("Botella_azul",    new Rect(1058, 273, 244, 136), new Vector2(0.5f, 0.5f))
        };

        ApplySlicing(path, slices, 165f);
    }

    // ── 5. Environment/Salidas.png (1536 × 1024) ─────────────────────────────
    // Escala objetivo: 7 a 8 u de ancho -> PPU ≈ 72f
    public static void SliceSalidas()
    {
        string path = "Assets/Sprites/Environment/Salidas.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            // Salida_0 (arriba izq): Arco de cantera del mercado al callejón (N1)
            new SliceData("Salida_0", new Rect(156, 561, 569, 392), new Vector2(0.5f, 0.0f)),
            // Salida_1 (arriba der): Pasaje angosto del callejón al atrio (N2)
            new SliceData("Salida_1", new Rect(919, 528, 407, 481), new Vector2(0.5f, 0.0f)),
            // Salida_2 (abajo izq): Portón de cantera y cruces del atrio al panteón (N3)
            new SliceData("Salida_2", new Rect(151, 40, 543, 451),  new Vector2(0.5f, 0.0f)),
            // Salida_3 (abajo der): Casa de Carlos con puerta y ventana (N4)
            new SliceData("Salida_3", new Rect(873, 51, 512, 418),  new Vector2(0.5f, 0.0f))
        };

        ApplySlicing(path, slices, 72f);
    }

    // ── 6. Perro/perro.png (2400 × 1340) ─────────────────────────────────────
    // Escala objetivo: vista lateral de 1.1 u de largo -> PPU ≈ 820f
    public static void SlicePerro()
    {
        string path = "Assets/Sprites/Perro/perro.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("Perro_frente",   new Rect(149, 175, 422, 902), new Vector2(0.5f, 0.05f)),
            new SliceData("Perro_espalda",  new Rect(749, 184, 372, 1023), new Vector2(0.5f, 0.05f)),
            new SliceData("Perro_lado_izq", new Rect(1329, 685, 912, 582), new Vector2(0.5f, 0.05f)),
            new SliceData("Perro_lado_der", new Rect(1389, 55, 903, 602),  new Vector2(0.5f, 0.05f))
        };

        ApplySlicing(path, slices, 820f);
    }

    // ── 7. Environment/tiles_nvl2.png (1536 × 1024) ──────────────────────────
    // Solo 7 utilizables: fila 1 (0..5) y fila 2 col 1 (6)
    public static void SliceTilesNvl2()
    {
        string path = "Assets/Sprites/Environment/tiles_nvl2.png";
        if (!File.Exists(path)) return;

        int tileW = 256;
        int tileH = 512;

        SliceData[] slices = new[]
        {
            new SliceData("Fachada_balcon_roja_0", new Rect(0 * tileW, 512, tileW, tileH), new Vector2(0.5f, 0.0f)),
            new SliceData("Fachada_ocre_1",        new Rect(1 * tileW, 512, tileW, tileH), new Vector2(0.5f, 0.0f)),
            new SliceData("Fachada_azul_2",        new Rect(2 * tileW, 512, tileW, tileH), new Vector2(0.5f, 0.0f)),
            new SliceData("Fachada_roja_3",        new Rect(3 * tileW, 512, tileW, tileH), new Vector2(0.5f, 0.0f)),
            new SliceData("Fachada_ocre_4",        new Rect(4 * tileW, 512, tileW, tileH), new Vector2(0.5f, 0.0f)),
            new SliceData("Fachada_morada_5",      new Rect(5 * tileW, 512, tileW, tileH), new Vector2(0.5f, 0.0f)),
            new SliceData("Fachada_ventana_roja_6", new Rect(0 * tileW,   0, tileW, tileH), new Vector2(0.5f, 0.0f))
        };

        ApplySlicing(path, slices, 160f);
    }

    // ── 8. Environment/tiles_adoquin_nvl2.png (1536 × 512) ───────────────────
    public static void SliceTilesAdoquinNvl2()
    {
        string path = "Assets/Sprites/Environment/tiles_adoquin_nvl2.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new SliceData[12];
        int w = 256;
        int h = 256;
        int idx = 0;
        for (int row = 1; row >= 0; row--)
        {
            for (int col = 0; col < 6; col++)
            {
                slices[idx] = new SliceData($"Adoquin_{idx}", new Rect(col * w, row * h, w, h), new Vector2(0.5f, 0.5f));
                idx++;
            }
        }

        ApplySlicing(path, slices, 160f);
    }

    // ── 9. Environment/Puertas_nvl2.png (1536 × 1024) ────────────────────────
    public static void SlicePuertasNvl2()
    {
        string path = "Assets/Sprites/Environment/Puertas_nvl2.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("Puerta_0", new Rect(60,  180, 420, 680), new Vector2(0.5f, 0.0f)),
            new SliceData("Puerta_1", new Rect(550, 180, 420, 680), new Vector2(0.5f, 0.0f)),
            new SliceData("Puerta_2", new Rect(1040,180, 420, 680), new Vector2(0.5f, 0.0f))
        };

        ApplySlicing(path, slices, 140f);
    }

    // ── 10. Environment/accesorios_nvl2.png (1536 × 1024) ────────────────────
    public static void SliceAccesoriosNvl2()
    {
        string path = "Assets/Sprites/Environment/accesorios_nvl2.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("Maceta_colgante",   new Rect(90,  550, 280, 400), new Vector2(0.5f, 0.5f)),
            new SliceData("Maceta_suelo_seca", new Rect(450, 550, 280, 360), new Vector2(0.5f, 0.0f)),
            new SliceData("Balcon_hierro",     new Rect(800, 520, 650, 380), new Vector2(0.5f, 0.0f)),
            new SliceData("Farol_pared",       new Rect(100,  80, 260, 380), new Vector2(0.5f, 0.5f))
        };

        ApplySlicing(path, slices, 150f);
    }

    // ── Aplicación mediante TextureImporter ───────────────────────────────────

    public struct SliceData
    {
        public string name;
        public Rect rect;
        public Vector2 pivot;
        public Vector4 border;

        public SliceData(string name, Rect rect, Vector2 pivot, Vector4 border = default)
        {
            this.name = name;
            this.rect = rect;
            this.pivot = pivot;
            this.border = border;
        }
    }

    static void ApplySlicing(string assetPath, SliceData[] slices, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"[SpriteSheetSlicer] No se encontró el TextureImporter en: {assetPath}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;

        SpriteMetaData[] meta = new SpriteMetaData[slices.Length];
        for (int i = 0; i < slices.Length; i++)
        {
            meta[i] = new SpriteMetaData
            {
                name = slices[i].name,
                rect = slices[i].rect,
                alignment = (int)SpriteAlignment.Custom,
                pivot = slices[i].pivot,
                border = slices[i].border
            };
        }

#pragma warning disable CS0618
        importer.spritesheet = meta;
#pragma warning restore CS0618

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        Debug.Log($"[SpriteSheetSlicer] Cortado exitoso: {assetPath} ({slices.Length} sprites, PPU {pixelsPerUnit}).");
    }
}
