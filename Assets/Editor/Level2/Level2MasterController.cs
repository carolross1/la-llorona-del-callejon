#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador Maestro para la Construcción del Nivel 2: Callejón Vertical de Guanajuato (5 Niveles).
/// Orquesta de forma desacoplada y modular:
/// - Generación modular de plataformas, cruces y escaleras (Level2ModularGenerator).
/// - Iluminación URP 2D ambiental y faroles de ruta (Level2Lighting).
/// - Configuración del HUD de alta fidelidad con distancia dinámica (Level2HUD).
/// - Calibración de la cámara (CameraFollow2D) con seguimiento vertical suave.
/// </summary>
[InitializeOnLoad]
public static class Level2MasterController
{
    const string ScenePath = "Assets/Scenes/Level2_Callejon.unity";
    const string TriggerFile = "Temp/RunLevel2Adapt.txt";

    static Level2MasterController()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    static void OnEditorUpdate()
    {
        if (File.Exists(TriggerFile))
        {
            try { File.Delete(TriggerFile); } catch { }
            BuildFullVerticalLevel2();
        }
    }

    [MenuItem("Tools/¡Construir Callejón Vertical Modular (5 Niveles)! %#l", priority = 10)]
    public static void BuildFullVerticalLevel2()
    {
        Debug.Log("[Level2MasterController] === CONSTRUYENDO CALLEJÓN VERTICAL MODULAR DE GUANAJUATO (5 NIVELES) ===");

        var scene = EditorSceneManager.OpenScene(ScenePath);
        if (!scene.IsValid())
        {
            Debug.LogError("[Level2MasterController] No se pudo abrir la escena: " + ScenePath);
            return;
        }

        try
        {
            LogSpriteSizes();
            CleanOldSceneGarbage(scene);

            GameObject rootEnv = new GameObject("Escenario_Callejon_Vertical_Modular");
            
            // 1. Generación de geometría física, niveles y conexiones modulares
            Level2ModularGenerator.GenerateLevel(rootEnv);

            // 2. Iluminación nocturna global y faroles guía
            Level2Lighting.ConfigureGlobalLight();

            // 3. Restaurar y configurar UICanvas (HUD, GoalCompass y Pantalla de Victoria)
            EnsureUICanvas(scene);
            Level2HUD.SetupHUD();

            // 4. Configuración de Carlos (Player) y Cámara suave
            ConfigurePlayerAndCamera();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Level2MasterController] ¡Nivel 2 Callejón Vertical Modular de 5 Niveles construido y guardado con éxito!");

            CaptureSceneScreenshot();
        }
        catch (Exception ex)
        {
            Debug.LogError("[Level2MasterController] Error durante la construcción: " + ex.ToString());
        }
    }

    static void LogSpriteSizes()
    {
        var sprites = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/Environment/Nivel 2/Sprites" });
        foreach (var guid in sprites)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp != null)
            {
                Debug.Log($"[SPRITE_INFO] {sp.name} | rect={sp.rect.width}x{sp.rect.height} | ppu={sp.pixelsPerUnit} | size={sp.bounds.size.x:F2}x{sp.bounds.size.y:F2}");
            }
        }
    }

    static void CaptureSceneScreenshot()
    {
        try
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            RenderTexture rt = new RenderTexture(1024, 576, 24);
            cam.targetTexture = rt;
            Texture2D tex = new Texture2D(1024, 576, TextureFormat.RGB24, false);
            // Captura 1: Vista inicial de Carlos en la calle empedrada (Tier 1)
            cam.transform.position = new Vector3(-4.0f, 1.40f, -10f);
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1024, 576), 0, 0);
            tex.Apply();
            File.WriteAllBytes("Temp/SceneScreenshot.png", tex.EncodeToPNG());

            // Captura 2: Vista de la Escalera Central de Cantera hacia Tier 2
            cam.transform.position = new Vector3(2.5f, 2.00f, -10f);
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1024, 576), 0, 0);
            tex.Apply();
            File.WriteAllBytes("Temp/SceneScreenshot_Stairs.png", tex.EncodeToPNG());

            // Captura 3: Vista de las Pasarelas Elevadas (Tier 2 y Tier 3)
            cam.transform.position = new Vector3(16.0f, 5.00f, -10f);
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1024, 576), 0, 0);
            tex.Apply();
            File.WriteAllBytes("Temp/SceneScreenshot_Terrace.png", tex.EncodeToPNG());

            // Captura 4: Vista de las Azoteas Coloniales (Tier 4)
            cam.transform.position = new Vector3(28.0f, 9.80f, -10f);
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1024, 576), 0, 0);
            tex.Apply();
            File.WriteAllBytes("Temp/SceneScreenshot_Tier4.png", tex.EncodeToPNG());

            // Captura 5: Vista de la Meta Monumental / Túnel Colonial (X = 57.0)
            cam.transform.position = new Vector3(55.0f, 1.80f, -10f);
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1024, 576), 0, 0);
            tex.Apply();
            File.WriteAllBytes("Temp/SceneScreenshot_Exit.png", tex.EncodeToPNG());

            // Restaurar cámara a posición de Carlos
            cam.transform.position = new Vector3(-6.0f, 1.40f, -10f);
            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);

            Debug.Log("[Level2MasterController] Screenshots de los 4 pisos y meta guardadas en Temp/");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Level2MasterController] No se pudo capturar screenshot: " + ex.Message);
        }
    }

    static void CleanOldSceneGarbage(UnityEngine.SceneManagement.Scene scene)
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

    static void EnsureUICanvas(UnityEngine.SceneManagement.Scene currentScene)
    {
        GameObject uiCanvas = GameObject.Find("UICanvas");
        if (uiCanvas == null)
        {
            Debug.Log("[Level2MasterController] UICanvas no encontrado en Nivel 2. Clonando desde Nivel 1...");
            var level1Scene = EditorSceneManager.OpenScene("Assets/Scenes/Level1_Mercado.unity", OpenSceneMode.Additive);
            GameObject sourceCanvas = null;
            foreach (var root in level1Scene.GetRootGameObjects())
            {
                if (root.name == "UICanvas")
                {
                    sourceCanvas = root;
                    break;
                }
            }

            if (sourceCanvas != null)
            {
                GameObject clone = UnityEngine.Object.Instantiate(sourceCanvas);
                clone.name = "UICanvas";
                SceneManager.MoveGameObjectToScene(clone, currentScene);
                uiCanvas = clone;
                Debug.Log("[Level2MasterController] ¡UICanvas clonado e integrado exitosamente en Nivel 2!");
            }
            EditorSceneManager.CloseScene(level1Scene, true);
        }

        // Conectar el objetivo de la brújula (GoalCompassUI) hacia la salida monumental
        GoalCompassUI compass = UnityEngine.Object.FindAnyObjectByType<GoalCompassUI>();
        if (compass != null)
        {
            GameObject goalObj = GameObject.Find("Salida_Tunel_Colonial");
            if (goalObj != null)
            {
                SerializedObject soComp = new SerializedObject(compass);
                var targetProp = soComp.FindProperty("target");
                if (targetProp != null)
                {
                    targetProp.objectReferenceValue = goalObj.transform;
                    soComp.ApplyModifiedProperties();
                }
            }
        }
    }

    static void ConfigurePlayerAndCamera()
    {
        // Posicionar a Carlos en la Plazuela de Inicio (Tier 1, Y = 0.0)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(-6.0f, 0.15f, 0f);
            player.transform.localScale = new Vector3(0.55f, 0.55f, 1.0f);

            SpriteRenderer playerSr = player.GetComponent<SpriteRenderer>();
            if (playerSr != null)
            {
                playerSr.sortingLayerName = "Characters";
                playerSr.sortingOrder = 10;
            }

            BoxCollider2D playerCol = player.GetComponent<BoxCollider2D>();
            if (playerCol != null)
            {
                playerCol.size = new Vector2(0.42f, 0.22f);
                playerCol.offset = new Vector2(0f, -0.3f);
                playerCol.edgeRadius = 0.04f;
            }
        }

        // Configuración de cámara con encuadre calibrado (idéntico al mockup de referencia)
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 2.85f;
            cam.transform.position = new Vector3(-6f, 1.40f, -10f);

            CameraFollow2D follow = cam.GetComponent<CameraFollow2D>();
            if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow2D>();

            follow.enabled = true;
            follow.SetBounds(new Vector2(-8.0f, 1.40f), new Vector2(58.0f, 10.50f));

            if (player != null)
            {
                SerializedObject soFollow = new SerializedObject(follow);
                var propTarget = soFollow.FindProperty("target");
                if (propTarget != null) propTarget.objectReferenceValue = player.transform;
                var propOffset = soFollow.FindProperty("offset");
                if (propOffset != null) propOffset.vector3Value = new Vector3(0f, 1.25f, -10f);
                var propSmooth = soFollow.FindProperty("smoothTime");
                if (propSmooth != null) propSmooth.floatValue = 0.25f;
                var propOrtho = soFollow.FindProperty("orthoSize");
                if (propOrtho != null) propOrtho.floatValue = 2.85f;
                soFollow.ApplyModifiedProperties();
            }
        }
    }
}
#endif
