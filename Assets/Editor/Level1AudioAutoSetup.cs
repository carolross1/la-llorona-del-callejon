using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Level1AudioAutoSetup
{
    static Level1AudioAutoSetup()
    {
        EditorApplication.delayCall += ExecuteSetup;
    }

    [MenuItem("La Llorona/Aplicar Configuración de Audios Nivel 1")]
    public static void ExecuteSetup()
    {
        string level1Path = "Assets/Scenes/Level1_Mercado.unity";
        Scene activeScene = EditorSceneManager.GetActiveScene();

        bool needToOpen = activeScene.path != level1Path;
        Scene targetScene = activeScene;

        if (needToOpen)
        {
            targetScene = EditorSceneManager.OpenScene(level1Path, OpenSceneMode.Single);
        }

        AudioClip n1Music = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Nivel1/Breath_Behind_the_Stall.mp3");
        AudioClip sfxPasosLentos = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/pasos_lentos.wav");
        AudioClip sfxPasosRapidos = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/pasos_rapidos.wav");
        AudioClip sfxPerro = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/perro.mp3");
        AudioClip sfxLlorona = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/llorona_nivel1.mpeg.wav");
        AudioClip sfxVidrio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Nivel1/vidrio.mp3");

        // 1. AudioManager
        AudioManager am = Object.FindAnyObjectByType<AudioManager>();
        if (am == null)
        {
            GameObject amGo = new GameObject("AudioManager");
            am = amGo.AddComponent<AudioManager>();
            Undo.RegisterCreatedObjectUndo(amGo, "Crear AudioManager");
        }

        SerializedObject soAm = new SerializedObject(am);
        soAm.FindProperty("musicaNivel").objectReferenceValue = n1Music;
        soAm.FindProperty("musicaNivel1").objectReferenceValue = n1Music;
        soAm.FindProperty("pasosLentos").objectReferenceValue = sfxPasosLentos;
        soAm.FindProperty("pasosRapidos").objectReferenceValue = sfxPasosRapidos;
        soAm.FindProperty("sonidoPerro").objectReferenceValue = sfxPerro;
        soAm.FindProperty("lloronaGrito").objectReferenceValue = sfxLlorona;
        soAm.FindProperty("sonidoVidrio").objectReferenceValue = sfxVidrio;
        soAm.ApplyModifiedProperties();
        EditorUtility.SetDirty(am);

        // 2. Player Footsteps
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerAnimation pa = player.GetComponent<PlayerAnimation>() ?? player.AddComponent<PlayerAnimation>();
            SerializedObject soPa = new SerializedObject(pa);
            soPa.FindProperty("pasosLentos").objectReferenceValue = sfxPasosLentos;
            soPa.FindProperty("pasosRapidos").objectReferenceValue = sfxPasosRapidos;
            soPa.ApplyModifiedProperties();
            EditorUtility.SetDirty(pa);
        }

        // 3. Perro Callejero (sonido de perro.mp3)
        StrayDogAI dog = Object.FindAnyObjectByType<StrayDogAI>();
        if (dog != null)
        {
            SerializedObject soDog = new SerializedObject(dog);
            soDog.FindProperty("barkSfx").objectReferenceValue = sfxPerro;
            soDog.ApplyModifiedProperties();
            EditorUtility.SetDirty(dog);
        }

        // 4. Llorona (llorona_nivel1.mpeg.wav)
        LloronaAI llorona = Object.FindAnyObjectByType<LloronaAI>();
        if (llorona != null)
        {
            SerializedObject soLlorona = new SerializedObject(llorona);
            soLlorona.FindProperty("screamSfx").objectReferenceValue = sfxLlorona;
            soLlorona.ApplyModifiedProperties();
            EditorUtility.SetDirty(llorona);
        }

        // 5. Botellas (sonido de vidrio.mp3)
        NoiseObstacle[] botellas = Object.FindObjectsByType<NoiseObstacle>(FindObjectsInactive.Include);
        foreach (var b in botellas)
        {
            if (b != null)
            {
                SerializedObject soB = new SerializedObject(b);
                soB.FindProperty("glassBreakSound").objectReferenceValue = sfxVidrio;
                soB.ApplyModifiedProperties();
                EditorUtility.SetDirty(b);
            }
        }

        EditorSceneManager.MarkSceneDirty(targetScene);
        EditorSceneManager.SaveScene(targetScene);
        Debug.Log("[Level1AudioAutoSetup] ¡Todos los audios de Nivel 1 configurados y guardados exitosamente!");
    }
}
