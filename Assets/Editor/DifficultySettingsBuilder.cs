// Assets/Editor/DifficultySettingsBuilder.cs
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Genera los ScriptableObjects de LloronaSettings para cada nivel
/// según la tabla de dificultad progresiva de la rúbrica y GDD.
///
/// Nivel 1: patrolSpeed 1.2, chaseSpeed 2.0, viewDistance 4.0, viewAngle 90
/// Nivel 2: patrolSpeed 1.5, chaseSpeed 2.5, viewDistance 5.0, viewAngle 100
/// Nivel 3: patrolSpeed 1.6, chaseSpeed 2.8, viewDistance 5.5, viewAngle 110
/// Nivel 4: patrolSpeed 2.0, chaseSpeed 3.4, viewDistance 6.5, viewAngle 120
/// </summary>
public static class DifficultySettingsBuilder
{
    [MenuItem("La Llorona/Generar Dificultades (ScriptableObjects)")]
    public static void GenerateAllSettings()
    {
        Directory.CreateDirectory("Assets/Settings");

        CreateOrUpdateSettings(1, 1.2f, 2.0f, 4.0f, 90f, 1.5f, 2.0f, 3.0f);
        CreateOrUpdateSettings(2, 1.5f, 2.5f, 5.0f, 100f, 1.8f, 1.8f, 3.0f);
        CreateOrUpdateSettings(3, 1.6f, 2.8f, 5.5f, 110f, 2.0f, 1.5f, 3.5f);
        CreateOrUpdateSettings(4, 2.0f, 3.4f, 6.5f, 120f, 2.2f, 1.2f, 4.0f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DifficultySettingsBuilder] ¡Los 4 ScriptableObjects de dificultad han sido generados en Assets/Settings/!");
    }

    static void CreateOrUpdateSettings(int level, float pSpeed, float cSpeed, float vDist, float vAngle, float hDist, float wTime, float lsTime)
    {
        string path = $"Assets/Settings/LloronaSettings_Level{level}.asset";
        LloronaSettings asset = AssetDatabase.LoadAssetAtPath<LloronaSettings>(path);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<LloronaSettings>();
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject so = new SerializedObject(asset);
        so.FindProperty("patrolSpeed").floatValue = pSpeed;
        so.FindProperty("chaseSpeed").floatValue = cSpeed;
        so.FindProperty("viewDistance").floatValue = vDist;
        so.FindProperty("viewAngle").floatValue = vAngle;
        so.FindProperty("hearDistance").floatValue = hDist;
        so.FindProperty("waitTime").floatValue = wTime;
        so.FindProperty("loseSightTime").floatValue = lsTime;
        so.FindProperty("runNoiseMultiplier").floatValue = 1.5f;
        so.ApplyModifiedProperties();
    }
}
