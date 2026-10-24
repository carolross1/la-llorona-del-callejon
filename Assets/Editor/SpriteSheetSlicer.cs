// Assets/Editor/SpriteSheetSlicer.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Configura y recorta con máxima precisión milimétrica los spritesheets del proyecto:
/// la interfaz de usuario (interfaz.png) y los nuevos sprites temáticos de los niveles.
/// </summary>
public static class SpriteSheetSlicer
{
    [MenuItem("La Llorona/Recortar y Configurar Spritesheets")]
    public static void SliceAllSpriteSheets()
    {
        Debug.Log("[SpriteSheetSlicer] Iniciando recorte de spritesheets...");

        SliceInterfaz();
        SlicePuestosNvl1();
        SliceContenedoresSacosNvl1();
        SliceBotellasNvl1();
        SliceSalidas();
        SliceTilesAdoquinNvl2();
        SliceTilesNvl2();
        SlicePuertasNvl2();
        SliceAccesoriosNvl2();

        GenerateLevel2TileAssets();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SpriteSheetSlicer] ¡Todos los spritesheets fueron recortados y configurados exitosamente!");
    }

    public static void SliceInterfaz()
    {
        string path = "Assets/Sprites/UI/interfaz.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            // 0: Corazón rojo activo
            new SliceData("interfaz_0", new Rect(116, 1000, 369, 493), new Vector2(0.5f, 0.5f)),
            // 1: Corazón negro / vida perdida
            new SliceData("interfaz_1", new Rect(649, 1000, 352, 493), new Vector2(0.5f, 0.5f)),
            // 2: Vela encendida / icono ofrenda
            new SliceData("interfaz_2", new Rect(1209, 999, 310, 473), new Vector2(0.5f, 0.5f)),
            // 3: Ojo detector / tutorial
            new SliceData("interfaz_3", new Rect(1715, 990, 584, 461), new Vector2(0.5f, 0.5f)),
            // 4: Barra de madera horizontal (tarjetas y paneles)
            new SliceData("interfaz_4", new Rect(96, 634, 2208, 293), new Vector2(0.5f, 0.5f), new Vector4(40, 20, 40, 20)),
            // 5: Botón de madera normal
            new SliceData("interfaz_5", new Rect(89, 287, 708, 258), new Vector2(0.5f, 0.5f), new Vector4(35, 25, 35, 25)),
            // 6: Botón de madera iluminado / hover
            new SliceData("interfaz_6", new Rect(846, 278, 707, 276), new Vector2(0.5f, 0.5f), new Vector4(35, 25, 35, 25)),
            // 7: Botón de madera presionado
            new SliceData("interfaz_7", new Rect(1602, 287, 708, 258), new Vector2(0.5f, 0.5f), new Vector4(35, 25, 35, 25)),
            // 8..13: Pequeños letreros de texto del menú inferior
            new SliceData("interfaz_8", new Rect(173, 162, 142, 38), new Vector2(0.5f, 0.5f)),
            new SliceData("interfaz_9", new Rect(577, 163, 144, 37), new Vector2(0.5f, 0.5f)),
            new SliceData("interfaz_10", new Rect(921, 163, 98, 37), new Vector2(0.5f, 0.5f)),
            new SliceData("interfaz_11", new Rect(1383, 163, 96, 37), new Vector2(0.5f, 0.5f)),
            new SliceData("interfaz_12", new Rect(1672, 162, 108, 38), new Vector2(0.5f, 0.5f)),
            new SliceData("interfaz_13", new Rect(2155, 165, 86, 36), new Vector2(0.5f, 0.5f))
        };

        ApplySlicing(path, slices, 100f);
    }

    public static void SlicePuestosNvl1()
    {
        string path = "Assets/Sprites/Environment/Puestos_Nivl1.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("Puesto_0", new Rect(84, 198, 628, 641), new Vector2(0.5f, 0.25f)),
            new SliceData("Puesto_1", new Rect(821, 189, 635, 650), new Vector2(0.5f, 0.25f))
        };

        ApplySlicing(path, slices, 100f);
    }

    public static void SliceContenedoresSacosNvl1()
    {
        string path = "Assets/Sprites/Environment/Contenedores_Sacos_Nvl1.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("Sacos_0", new Rect(116, 571, 341, 398), new Vector2(0.5f, 0.3f)),
            new SliceData("Sacos_1", new Rect(572, 570, 393, 387), new Vector2(0.5f, 0.3f)),
            new SliceData("Sacos_2", new Rect(1079, 599, 335, 310), new Vector2(0.5f, 0.3f)),
            new SliceData("Sacos_3", new Rect(594, 88, 362, 411), new Vector2(0.5f, 0.3f)),
            new SliceData("Sacos_4", new Rect(1072, 76, 341, 419), new Vector2(0.5f, 0.3f)),
            new SliceData("Sacos_5", new Rect(137, 130, 334, 337), new Vector2(0.5f, 0.3f))
        };

        ApplySlicing(path, slices, 100f);
    }

    public static void SliceBotellasNvl1()
    {
        string path = "Assets/Sprites/Environment/Botellas_Nvl1.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("Botellas_0", new Rect(926, 761, 510, 223), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_1", new Rect(475, 765, 358, 191), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_2", new Rect(121, 789, 238, 154), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_3", new Rect(856, 463, 595, 272), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_4", new Rect(425, 501, 372, 197), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_5", new Rect(113, 531, 234, 159), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_6", new Rect(118, 264, 78, 189), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_7", new Rect(398, 264, 80, 188), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_8", new Rect(664, 279, 212, 132), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_9", new Rect(1058, 273, 244, 136), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_10", new Rect(1135, 48, 80, 199), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_11", new Rect(79, 49, 168, 179), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_12", new Rect(664, 49, 257, 165), new Vector2(0.5f, 0.5f)),
            new SliceData("Botellas_13", new Rect(359, 51, 241, 154), new Vector2(0.5f, 0.5f))
        };

        ApplySlicing(path, slices, 100f);
    }

    public static void SliceSalidas()
    {
        string path = "Assets/Sprites/Environment/Salidas.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("Salida_0", new Rect(919, 528, 407, 481), new Vector2(0.5f, 0.15f)),
            new SliceData("Salida_1", new Rect(156, 561, 569, 392), new Vector2(0.5f, 0.15f)),
            new SliceData("Salida_2", new Rect(151, 40, 543, 451), new Vector2(0.5f, 0.15f)),
            new SliceData("Salida_3", new Rect(873, 51, 512, 418), new Vector2(0.5f, 0.15f))
        };

        ApplySlicing(path, slices, 100f);
    }

    public static void SliceTilesAdoquinNvl2()
    {
        string path = "Assets/Sprites/Environment/tiles_adoquin_nvl2.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("Adoquin_0", new Rect(34, 571, 220, 238), new Vector2(0.5f, 0.5f)),
            new SliceData("Adoquin_1", new Rect(286, 571, 219, 238), new Vector2(0.5f, 0.5f)),
            new SliceData("Adoquin_2", new Rect(535, 571, 219, 238), new Vector2(0.5f, 0.5f)),
            new SliceData("Adoquin_3", new Rect(784, 571, 218, 238), new Vector2(0.5f, 0.5f)),
            new SliceData("Adoquin_4", new Rect(1031, 571, 219, 238), new Vector2(0.5f, 0.5f)),
            new SliceData("Adoquin_5", new Rect(1282, 571, 219, 237), new Vector2(0.5f, 0.5f)),
            new SliceData("Adoquin_6", new Rect(34, 286, 220, 238), new Vector2(0.5f, 0.5f)),
            new SliceData("Adoquin_7", new Rect(286, 286, 219, 240), new Vector2(0.5f, 0.5f)),
            new SliceData("Adoquin_8", new Rect(535, 286, 219, 238), new Vector2(0.5f, 0.5f)),
            new SliceData("Adoquin_9", new Rect(784, 286, 218, 238), new Vector2(0.5f, 0.5f)),
            new SliceData("Adoquin_10", new Rect(1031, 286, 219, 238), new Vector2(0.5f, 0.5f)),
            new SliceData("Adoquin_11", new Rect(1282, 286, 219, 238), new Vector2(0.5f, 0.5f))
        };

        ApplySlicing(path, slices, 200f);
    }

    public static void SliceTilesNvl2()
    {
        string path = "Assets/Sprites/Environment/tiles_nvl2.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("MuroNvl2_0", new Rect(25, 684, 223, 255), new Vector2(0.5f, 0.5f)),
            new SliceData("MuroNvl2_1", new Rect(276, 684, 222, 255), new Vector2(0.5f, 0.5f)),
            new SliceData("MuroNvl2_2", new Rect(528, 684, 477, 255), new Vector2(0.5f, 0.5f)),
            new SliceData("MuroNvl2_3", new Rect(1034, 684, 224, 255), new Vector2(0.5f, 0.5f)),
            new SliceData("MuroNvl2_4", new Rect(1286, 684, 223, 255), new Vector2(0.5f, 0.5f)),
            new SliceData("MuroNvl2_5", new Rect(26, 383, 223, 256), new Vector2(0.5f, 0.5f)),
            new SliceData("MuroNvl2_6", new Rect(275, 76, 1233, 563), new Vector2(0.5f, 0.5f))
        };

        ApplySlicing(path, slices, 200f);
    }

    public static void SlicePuertasNvl2()
    {
        string path = "Assets/Sprites/Environment/Puertas_nvl2.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("PuertaNvl2_0", new Rect(40, 281, 462, 523), new Vector2(0.5f, 0.2f)),
            new SliceData("PuertaNvl2_1", new Rect(541, 286, 453, 530), new Vector2(0.5f, 0.2f)),
            new SliceData("PuertaNvl2_2", new Rect(1034, 286, 458, 530), new Vector2(0.5f, 0.2f))
        };

        ApplySlicing(path, slices, 100f);
    }

    public static void SliceAccesoriosNvl2()
    {
        string path = "Assets/Sprites/Environment/accesorios_nvl2.png";
        if (!File.Exists(path)) return;

        SliceData[] slices = new[]
        {
            new SliceData("Accesorio_Farol", new Rect(365, 574, 256, 410), new Vector2(0.5f, 0.5f)),
            new SliceData("Accesorio_Maceta", new Rect(868, 543, 281, 436), new Vector2(0.5f, 0.5f)),
            new SliceData("Accesorio_BalconGrande", new Rect(265, 54, 444, 445), new Vector2(0.5f, 0.5f)),
            new SliceData("Accesorio_Balcon", new Rect(901, 94, 267, 394), new Vector2(0.5f, 0.5f))
        };

        ApplySlicing(path, slices, 100f);
    }

    public static void GenerateLevel2TileAssets()
    {
        string dir = "Assets/Tiles/Level2";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var adoquines = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/tiles_adoquin_nvl2.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();

        for (int i = 0; i < adoquines.Length; i++)
        {
            string assetPath = $"{dir}/adoquin_{i}.asset";
            UnityEngine.Tilemaps.Tile tile = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(assetPath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                tile.sprite = adoquines[i];
                AssetDatabase.CreateAsset(tile, assetPath);
            }
            else
            {
                tile.sprite = adoquines[i];
                EditorUtility.SetDirty(tile);
            }
        }

        var muros = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Environment/tiles_nvl2.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();

        for (int i = 0; i < muros.Length; i++)
        {
            string assetPath = $"{dir}/muro_{i}.asset";
            UnityEngine.Tilemaps.Tile tile = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(assetPath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                tile.sprite = muros[i];
                AssetDatabase.CreateAsset(tile, assetPath);
            }
            else
            {
                tile.sprite = muros[i];
                EditorUtility.SetDirty(tile);
            }
        }
    }

    static void ApplySlicing(string assetPath, SliceData[] slices, float ppu)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (ti == null)
        {
            Debug.LogError($"[SpriteSheetSlicer] No se pudo obtener TextureImporter para {assetPath}");
            return;
        }

        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = ppu;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.alphaIsTransparency = true;

        SpriteMetaData[] metaArray = new SpriteMetaData[slices.Length];
        for (int i = 0; i < slices.Length; i++)
        {
            metaArray[i] = new SpriteMetaData
            {
                name = slices[i].Name,
                rect = slices[i].Rect,
                alignment = (int)SpriteAlignment.Custom,
                pivot = slices[i].Pivot,
                border = slices[i].Border
            };
        }

        ti.spritesheet = metaArray;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        Debug.Log($"[SpriteSheetSlicer] {slices.Length} sub-sprites configurados en: {assetPath}");
    }

    struct SliceData
    {
        public string Name;
        public Rect Rect;
        public Vector2 Pivot;
        public Vector4 Border;

        public SliceData(string name, Rect rect, Vector2 pivot, Vector4 border = default)
        {
            Name = name;
            Rect = rect;
            Pivot = pivot;
            Border = border;
        }
    }
}
