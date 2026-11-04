// Assets/Editor/SliceTriggerRunner.cs
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SliceTriggerRunner
{
    static SliceTriggerRunner()
    {
        EditorApplication.update += CheckAndRun;
    }

    static void CheckAndRun()
    {
        string reloadTrigger = "Temp/reload_trigger.txt";
        if (File.Exists(reloadTrigger))
        {
            File.Delete(reloadTrigger);
            Debug.Log("[SliceTriggerRunner] Solicitando recarga y compilación de scripts...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            EditorUtility.RequestScriptReload();
            return;
        }

        if (EditorApplication.isCompiling)
            return;

        string fixAnimTrigger = "Temp/fix_anim_trigger.txt";
        if (File.Exists(fixAnimTrigger))
        {
            try
            {
                File.Delete(fixAnimTrigger);
                Debug.Log("[SliceTriggerRunner] Ejecutando PlayerAnimationFixer...");
                string diag = PlayerAnimationFixer.FixPlayerAnimations();
                File.WriteAllText("Temp/anim_diagnostic.txt", diag);
                File.WriteAllText("Temp/fix_anim_done.txt", "DONE");
                Debug.Log("[SliceTriggerRunner] PlayerAnimationFixer finalizado con éxito.");
            }
            catch (System.Exception ex)
            {
                File.WriteAllText("Temp/fix_anim_error.txt", ex.ToString());
                Debug.LogError($"[SliceTriggerRunner] Error en PlayerAnimationFixer: {ex}");
            }
        }

        string sliceTrigger = "Temp/slice_trigger.txt";
        string l1Trigger = "Temp/build_level1_trigger.txt";
        string l2Trigger = "Temp/build_level2_trigger.txt";
        string allTrigger = "Temp/build_all_trigger.txt";
        string l3Trigger = "Temp/build_level3_trigger.txt";

        if (!File.Exists(sliceTrigger) && !File.Exists(l1Trigger) && !File.Exists(l2Trigger) && !File.Exists(allTrigger) && !File.Exists(l3Trigger))
            return;

        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        if (File.Exists(sliceTrigger))
        {
            try
            {
                File.Delete(sliceTrigger);
                Debug.Log("[SliceTriggerRunner] Ejecutando SliceAllSpriteSheets desde Unity...");
                SpriteSheetSlicer.SliceAllSpriteSheets();
                File.WriteAllText("Temp/slice_done.txt", "DONE");
                Debug.Log("[SliceTriggerRunner] Slicing finalizado con éxito.");
            }
            catch (System.Exception ex)
            {
                File.WriteAllText("Temp/slice_error.txt", ex.ToString());
                Debug.LogError($"[SliceTriggerRunner] Error slicing: {ex}");
            }
        }

        if (File.Exists(l1Trigger))
        {
            try
            {
                File.Delete(l1Trigger);
                Debug.Log("[SliceTriggerRunner] Construyendo Nivel 1 oficial...");
                Level1Builder.BuildLevel1Scene();
                File.WriteAllText("Temp/build_level1_done.txt", "DONE");
                Debug.Log("[SliceTriggerRunner] Nivel 1 construido con éxito.");
            }
            catch (System.Exception ex)
            {
                File.WriteAllText("Temp/build_level1_error.txt", ex.ToString());
                Debug.LogError($"[SliceTriggerRunner] Error construyendo Nivel 1: {ex}");
            }
        }

        if (File.Exists(l2Trigger))
        {
            try
            {
                File.Delete(l2Trigger);
                Debug.Log("[SliceTriggerRunner] Construyendo Nivel 2 oficial de 4 pisos (El Callejón del Beso)...");
                Level2MasterController.BuildFullVerticalLevel2();
                File.WriteAllText("Temp/build_level2_done.txt", "DONE");
                Debug.Log("[SliceTriggerRunner] Nivel 2 construido con éxito.");
            }
            catch (System.Exception ex)
            {
                File.WriteAllText("Temp/build_level2_error.txt", ex.ToString());
                Debug.LogError($"[SliceTriggerRunner] Error construyendo Nivel 2: {ex}");
            }
        }

        if (File.Exists(l3Trigger))
        {
            try
            {
                File.Delete(l3Trigger);
                Debug.Log("[SliceTriggerRunner] Construyendo Nivel 3 oficial (El Atrio de Dolores)...");
                Level3Builder.BuildLevel3Scene();
                File.WriteAllText("Temp/build_level3_done.txt", "DONE");
                Debug.Log("[SliceTriggerRunner] Nivel 3 construido con �xito.");
            }
            catch (System.Exception ex)
            {
                File.WriteAllText("Temp/build_level3_error.txt", ex.ToString());
                Debug.LogError($"[SliceTriggerRunner] Error construyendo Nivel 3: {ex}");
            }
        }
        if (File.Exists(allTrigger))
        {
            try
            {
                File.Delete(allTrigger);
                Debug.Log("[SliceTriggerRunner] Construyendo todo el proyecto...");
                LaLloronaMasterBuilder.BuildEverythingSilent();
                File.WriteAllText("Temp/build_all_done.txt", "DONE");
                Debug.Log("[SliceTriggerRunner] Proyecto completo construido con éxito.");
            }
            catch (System.Exception ex)
            {
                File.WriteAllText("Temp/build_all_error.txt", ex.ToString());
                Debug.LogError($"[SliceTriggerRunner] Error construyendo todo: {ex}");
            }
        }
    }
}

