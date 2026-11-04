#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class Level2TriggerWatcher : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string str in importedAssets)
        {
            if (str.Contains("TriggerBuildLevel2"))
            {
                Debug.Log("[Level2TriggerWatcher] >>> TRIGGER DETECTADO EN ASSETS: Construyendo los 4 pisos de Guanajuato... <<<");
                Level2MasterController.BuildFullVerticalLevel2();
                break;
            }
        }
    }
}
#endif
