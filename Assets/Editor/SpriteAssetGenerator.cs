// Assets/Editor/SpriteAssetGenerator.cs
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Genera automáticamente sprites de alta calidad para nuevos obstáculos, enemigos secundarios,
/// tarjetas de interfaz y brújula de meta sin dependencias externas.
/// </summary>
public static class SpriteAssetGenerator
{
    [MenuItem("La Llorona/Generar Sprites del Juego")]
    public static void GenerateAllSprites()
    {
        Directory.CreateDirectory("Assets/Sprites/UI");
        Directory.CreateDirectory("Assets/Sprites/Environment");

        GenerateHudCard();
        GenerateGoalArrow();
        GeneratePuddleSprite();
        GenerateTrashNoiseSprite();
        GenerateStrayDogSprite();
        GenerateExitPortalSprite();

        AssetDatabase.Refresh();
        Debug.Log("[SpriteAssetGenerator] ¡Todos los sprites generados y actualizados con éxito!");
    }

    /// <summary>Tarjeta de HUD con bordes redondeados y marco sutil semitransparente.</summary>
    static void GenerateHudCard()
    {
        int w = 128, h = 128, r = 16, border = 3;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color bg = new Color(0.04f, 0.03f, 0.08f, 0.88f);
        Color borderColor = new Color(0.85f, 0.70f, 0.35f, 0.75f);
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Max(0, Mathf.Max(r - x, x - (w - 1 - r)));
                float dy = Mathf.Max(0, Mathf.Max(r - y, y - (h - 1 - r)));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > r)
                {
                    tex.SetPixel(x, y, clear);
                }
                else if (dist > r - border || x < border || x >= w - border || y < border || y >= h - border)
                {
                    tex.SetPixel(x, y, borderColor);
                }
                else
                {
                    tex.SetPixel(x, y, bg);
                }
            }
        }
        tex.Apply();
        SaveTexture(tex, "Assets/Sprites/UI/panel_hud_card.png", SpriteMeshType.FullRect, new Vector4(16, 16, 16, 16));
    }

    /// <summary>Flecha de brújula dorada y brillante para indicar la meta.</summary>
    static void GenerateGoalArrow()
    {
        int w = 64, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color gold = new Color(1.0f, 0.88f, 0.25f, 1f);
        Color glow = new Color(0.95f, 0.65f, 0.1f, 0.85f);
        Color dark = new Color(0.4f, 0.25f, 0.05f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float cx = x - 32f;
                float cy = y - 32f;

                // Triángulo apuntando hacia la derecha (0 grados)
                bool insideArrow = (cx >= -22 && cx <= 24 && Mathf.Abs(cy) <= (24 - cx) * 0.7f && cx > 0)
                                || (cx >= -22 && cx <= 4 && Mathf.Abs(cy) <= 8f);

                if (insideArrow)
                {
                    bool isBorder = (Mathf.Abs(cy) > 6f && cx <= 0) || (Mathf.Abs(cy) >= (24 - cx) * 0.7f - 2f) || cx <= -20;
                    tex.SetPixel(x, y, isBorder ? dark : (cy > 0 ? gold : glow));
                }
                else
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }
        tex.Apply();
        SaveTexture(tex, "Assets/Sprites/UI/flecha_meta.png");
    }

    /// <summary>Charco de lodo oscuro con brillo de agua estancada.</summary>
    static void GeneratePuddleSprite()
    {
        int w = 128, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color mud = new Color(0.12f, 0.09f, 0.06f, 0.90f);
        Color water = new Color(0.18f, 0.22f, 0.25f, 0.82f);
        Color highlight = new Color(0.35f, 0.45f, 0.50f, 0.65f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float nx = (x - 64f) / 60f;
                float ny = (y - 32f) / 28f;
                // Deformación orgánica
                float angle = Mathf.Atan2(ny, nx);
                float radius = 1.0f + 0.12f * Mathf.Sin(angle * 5f) + 0.08f * Mathf.Cos(angle * 3f);
                float dist = Mathf.Sqrt(nx * nx + ny * ny);

                if (dist > radius)
                {
                    tex.SetPixel(x, y, clear);
                }
                else if (dist > radius - 0.15f)
                {
                    tex.SetPixel(x, y, mud);
                }
                else
                {
                    bool isReflection = (ny > 0.1f && nx > -0.5f && nx < 0.3f && (x + y) % 6 < 3);
                    tex.SetPixel(x, y, isReflection ? highlight : water);
                }
            }
        }
        tex.Apply();
        SaveTexture(tex, "Assets/Sprites/Environment/charco_lodo.png");
    }

    /// <summary>Montón de latas y botellas rotas ruidosas.</summary>
    static void GenerateTrashNoiseSprite()
    {
        int w = 96, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color metal = new Color(0.72f, 0.72f, 0.78f, 0.95f);
        Color glass = new Color(0.30f, 0.65f, 0.45f, 0.90f);
        Color rust = new Color(0.60f, 0.35f, 0.20f, 0.95f);
        Color shadow = new Color(0.05f, 0.05f, 0.05f, 0.50f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        // Sombra de fondo
        for (int y = 8; y < 22; y++)
        {
            for (int x = 14; x < 82; x++)
            {
                float dx = (x - 48f) / 32f;
                float dy = (y - 15f) / 7f;
                if (dx * dx + dy * dy <= 1f) tex.SetPixel(x, y, shadow);
            }
        }

        // Latas y botellas esparcidas
        DrawCan(tex, 24, 20, 16, 22, metal, rust);
        DrawCan(tex, 44, 18, 20, 14, metal, metal);
        DrawBottle(tex, 60, 22, 14, 26, glass);
        DrawCan(tex, 36, 12, 14, 12, rust, rust);
        DrawGlassShards(tex, 16, 12);
        DrawGlassShards(tex, 72, 14);

        tex.Apply();
        SaveTexture(tex, "Assets/Sprites/Environment/basura_ruido.png");
    }

    /// <summary>Perro callejero territorial con ojos brillantes en la oscuridad.</summary>
    static void GenerateStrayDogSprite()
    {
        int w = 96, h = 96;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color fur = new Color(0.24f, 0.18f, 0.14f, 0.98f);
        Color furLight = new Color(0.38f, 0.28f, 0.20f, 0.98f);
        Color eyeGlow = new Color(0.95f, 0.25f, 0.15f, 1f); // Ojos rojos amenazantes
        Color eyeWhite = new Color(1f, 0.85f, 0.4f, 1f);
        Color nose = new Color(0.08f, 0.08f, 0.08f, 1f);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        // Cuerpo agazapado (elipse)
        for (int y = 20; y < 58; y++)
        {
            for (int x = 16; x < 72; x++)
            {
                float dx = (x - 42f) / 26f;
                float dy = (y - 38f) / 16f;
                if (dx * dx + dy * dy <= 1f)
                    tex.SetPixel(x, y, (dy > 0.2f) ? furLight : fur);
            }
        }

        // Cabeza
        for (int y = 40; y < 80; y++)
        {
            for (int x = 50; x < 88; x++)
            {
                float dx = (x - 68f) / 16f;
                float dy = (y - 58f) / 16f;
                if (dx * dx + dy * dy <= 1f)
                    tex.SetPixel(x, y, (dx > 0) ? furLight : fur);
            }
        }

        // Hocico
        for (int y = 44; y < 60; y++)
        {
            for (int x = 74; x < 94; x++)
            {
                float dx = (x - 82f) / 10f;
                float dy = (y - 52f) / 7f;
                if (dx * dx + dy * dy <= 1f)
                    tex.SetPixel(x, y, (x > 88) ? nose : fur);
            }
        }

        // Orejas puntiagudas alertas
        for (int y = 68; y < 90; y++)
        {
            int hEar = y - 68;
            for (int x = 54 + hEar / 3; x <= 64 - hEar / 3; x++) tex.SetPixel(x, y, fur);
            for (int x = 68 + hEar / 3; x <= 78 - hEar / 3; x++) tex.SetPixel(x, y, fur);
        }

        // Ojos amenazantes brillantes
        tex.SetPixel(72, 60, eyeGlow);
        tex.SetPixel(73, 60, eyeWhite);
        tex.SetPixel(72, 61, eyeGlow);
        tex.SetPixel(80, 60, eyeGlow);
        tex.SetPixel(81, 60, eyeWhite);
        tex.SetPixel(80, 61, eyeGlow);

        // Patas
        for (int y = 8; y < 24; y++)
        {
            for (int x = 22; x < 30; x++) tex.SetPixel(x, y, fur);
            for (int x = 38; x < 46; x++) tex.SetPixel(x, y, fur);
            for (int x = 56; x < 64; x++) tex.SetPixel(x, y, fur);
        }

        tex.Apply();
        SaveTexture(tex, "Assets/Sprites/Environment/perro_callejero.png");
    }

    /// <summary>Arco/Portal monumental de salida con faroles verdes de cantera.</summary>
    static void GenerateExitPortalSprite()
    {
        int w = 128, h = 160;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color stone = new Color(0.42f, 0.40f, 0.38f, 1f);
        Color stoneDark = new Color(0.24f, 0.22f, 0.20f, 1f);
        Color wood = new Color(0.32f, 0.20f, 0.12f, 1f);
        Color emeraldGlow = new Color(0.2f, 0.95f, 0.4f, 0.9f);
        Color emeraldCore = new Color(0.7f, 1.0f, 0.8f, 1f);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        // Columnas laterales de cantera
        for (int y = 0; y < 130; y++)
        {
            for (int x = 8; x < 28; x++)
            {
                bool isJoint = (y % 20 == 0);
                tex.SetPixel(x, y, isJoint ? stoneDark : stone);
            }
            for (int x = 100; x < 120; x++)
            {
                bool isJoint = (y % 20 == 0);
                tex.SetPixel(x, y, isJoint ? stoneDark : stone);
            }
        }

        // Arco superior
        for (int y = 110; y < 155; y++)
        {
            for (int x = 8; x < 120; x++)
            {
                float nx = (x - 64f) / 48f;
                float ny = (y - 110f) / 36f;
                float rOuter = nx * nx + ny * ny;
                if (rOuter <= 1.15f && rOuter >= 0.55f && y >= 110)
                {
                    bool isJoint = (x % 16 == 0);
                    tex.SetPixel(x, y, isJoint ? stoneDark : stone);
                }
            }
        }

        // Puerta entreabierta y luz esmeralda que surge del interior
        for (int y = 8; y < 110; y++)
        {
            for (int x = 32; x < 96; x++)
            {
                float distCenter = Mathf.Abs(x - 64f) / 32f;
                float v = (1f - distCenter);
                Color lightColor = Color.Lerp(emeraldGlow, emeraldCore, v);
                lightColor.a = 0.55f + 0.35f * v;
                tex.SetPixel(x, y, lightColor);
            }
        }

        // Letrero grabado superior "SALIDA"
        for (int y = 142; y < 156; y++)
        {
            for (int x = 36; x < 92; x++)
            {
                tex.SetPixel(x, y, stoneDark);
            }
        }

        tex.Apply();
        SaveTexture(tex, "Assets/Sprites/Environment/portal_meta.png");
    }

    // ── Utilidades de dibujo ────────────────────────────────────────────────

    static void DrawCan(Texture2D tex, int ox, int oy, int w, int h, Color c1, Color c2)
    {
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = (x < w / 3) ? c2 : c1;
                tex.SetPixel(ox + x, oy + y, c);
            }
        }
    }

    static void DrawBottle(Texture2D tex, int ox, int oy, int w, int h, Color c)
    {
        for (int y = 0; y < h; y++)
        {
            int curW = (y > h * 0.6f) ? w / 2 : w;
            int startX = (w - curW) / 2;
            for (int x = 0; x < curW; x++)
            {
                tex.SetPixel(ox + startX + x, oy + y, c);
            }
        }
    }

    static void DrawGlassShards(Texture2D tex, int ox, int oy)
    {
        Color shard = new Color(0.85f, 0.95f, 1f, 0.9f);
        tex.SetPixel(ox, oy, shard);
        tex.SetPixel(ox + 1, oy + 1, shard);
        tex.SetPixel(ox + 3, oy, shard);
        tex.SetPixel(ox + 5, oy + 2, shard);
    }

    static void SaveTexture(Texture2D tex, string path, SpriteMeshType meshType = SpriteMeshType.Tight, Vector4 border = default)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.spritePixelsPerUnit = 32f;
            if (border != default)
            {
                ti.spriteBorder = border;
            }
            ti.SaveAndReimport();
        }
    }
}
