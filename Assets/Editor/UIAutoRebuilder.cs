// Assets/Editor/UIAutoRebuilder.cs
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class UIAutoRebuilder
{
    static UIAutoRebuilder()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.delayCall += ExecuteRebuild;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.delayCall += ExecuteRebuild;
        }
    }

    [MenuItem("La Llorona/Regenerar Todo (Limpio)")]
    public static void ManualRebuild()
    {
        ExecuteRebuild();
    }

    static void ExecuteRebuild()
    {
        Debug.Log("[UIAutoRebuilder] Regenerando menús y HUD automáticamente...");
        try
        {
            UIBuilder.BuildAllScenesAndUI();
            Debug.Log("[UIAutoRebuilder] ¡Todas las escenas y el HUD fueron regenerados con éxito!");

            // Reabrir Level3_Atrio para dejarlo listo en el editor
            EditorSceneManager.OpenScene("Assets/Scenes/Level3_Atrio.unity", OpenSceneMode.Single);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[UIAutoRebuilder] Error durante la regeneración: " + ex.Message);
        }
        finally
        {
            // Auto-eliminación del script para que solo se ejecute una vez
            AssetDatabase.DeleteAsset("Assets/Editor/UIAutoRebuilder.cs");
        }
    }
}
