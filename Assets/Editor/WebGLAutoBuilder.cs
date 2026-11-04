// Assets/Editor/WebGLAutoBuilder.cs
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Compilador automatizado para generar el build WebGL listo para publicar en itch.io.
/// Configura Compression Format en Disabled, resolución 960x600, plantilla Default,
/// compila todas las escenas del juego y genera automáticamente el archivo BuildWeb.zip.
/// </summary>
public static class WebGLAutoBuilder
{
    const string TriggerFile = "Temp/RunWebGLBuild.txt";

    [InitializeOnLoadMethod]
    static void CheckAutoBuildTrigger()
    {
        if (File.Exists(TriggerFile))
        {
            try { File.Delete(TriggerFile); } catch { }
            EditorApplication.delayCall += () =>
            {
                Debug.Log("[WebGLAutoBuilder] Disparador automático detectado. Iniciando compilación WebGL...");
                BuildForItch(autoTriggered: true);
            };
        }
    }

    [MenuItem("Build WebGL/¡Compilar BuildWeb para itch.io! %#w", priority = 1)]
    public static void ManualBuild()
    {
        BuildForItch(autoTriggered: false);
    }

    public static bool BuildForItch(bool autoTriggered = false)
    {
        string projectPath = Directory.GetCurrentDirectory();
        string buildFolder = Path.Combine(projectPath, "BuildWeb");
        string zipPath = Path.Combine(projectPath, "BuildWeb.zip");

        Debug.Log("[WebGLAutoBuilder] === INICIANDO COMPILACIÓN WEBGL PARA ITCH.IO ===");

        // 1. Configurar PlayerSettings conforme a la guía de itch.io
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.template = "APPLICATION:Default";
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 600;
        PlayerSettings.runInBackground = true;

        // 2. Obtener escenas habilitadas
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled && !string.IsNullOrEmpty(s.path) && File.Exists(s.path))
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[WebGLAutoBuilder] No hay escenas habilitadas en EditorBuildSettings.");
            return false;
        }

        Debug.Log($"[WebGLAutoBuilder] Escenas a compilar ({scenes.Length}):\n" + string.Join("\n", scenes));

        // 3. Preparar carpeta limpia
        if (Directory.Exists(buildFolder))
        {
            try { Directory.Delete(buildFolder, true); }
            catch (Exception ex) { Debug.LogWarning("[WebGLAutoBuilder] No se pudo limpiar BuildWeb anterior: " + ex.Message); }
        }
        Directory.CreateDirectory(buildFolder);

        if (File.Exists(zipPath))
        {
            try { File.Delete(zipPath); } catch { }
        }

        // 4. Configurar opciones de compilación
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildFolder,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None
        };

        // 5. Ejecutar la compilación
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[WebGLAutoBuilder] ¡Compilación WebGL completada con éxito! ({summary.totalTime.TotalSeconds:F1}s)");

            // 6. Generar el archivo ZIP listo para itch.io
            try
            {
                Debug.Log("[WebGLAutoBuilder] Comprimiendo contenido a BuildWeb.zip...");
                ZipFile.CreateFromDirectory(buildFolder, zipPath, System.IO.Compression.CompressionLevel.Optimal, false);
                Debug.Log($"[WebGLAutoBuilder] ¡BuildWeb.zip generado correctamente en: {zipPath}!");

                if (!autoTriggered && !Application.isBatchMode)
                {
                    EditorUtility.RevealInFinder(zipPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[WebGLAutoBuilder] Error al crear BuildWeb.zip: " + ex.Message);
                return false;
            }
        }
        else
        {
            Debug.LogError($"[WebGLAutoBuilder] La compilación falló con estado: {summary.result}. Errores: {summary.totalErrors}");
            return false;
        }
    }
}
