// Assets/Editor/LaLloronaMasterBuilder.cs
using UnityEditor;
using UnityEngine;

/// <summary>
/// Panel de control maestro y ejecutor de un solo clic para construir
/// todas las escenas, niveles, interfaces, iluminación y ajustes de La Llorona del Callejón.
/// </summary>
public class LaLloronaMasterBuilder : EditorWindow
{
    [MenuItem("La Llorona/¡CONSTRUIR TODO EL PROYECTO (Un Clic)!")]
    public static void BuildEverything()
    {
        Debug.Log("[MasterBuilder] Iniciando construcción total del proyecto...");

        // 0. Sprites procedurales de alta calidad para HUD, brújula, obstáculos y enemigos
        SpriteAssetGenerator.GenerateAllSprites();

        // 1. ScriptableObjects de dificultad (requeridos antes de los niveles)
        DifficultySettingsBuilder.GenerateAllSettings();

        // 2. Niveles jugables con obstáculos y portales de meta
        Level1Builder.BuildLevel1Scene();
        Level2Builder.BuildLevel2Scene();
        Level3Builder.BuildLevel3Scene();
        Level4Builder.BuildLevel4Scene();

        // 3. Escenas de menú, HUD reestilizado y Build Settings
        UIBuilder.BuildAllScenesAndUI();

        EditorUtility.DisplayDialog(
            "La Llorona del Callejón",
            "¡TODO EL PROYECTO FUE CONSTRUIDO CON ÉXITO!\n\n" +
            "Se han generado y configurado:\n" +
            "• HUD Reestilizado: 3 módulos superiores despejados, legibles con Outline y sombras.\n" +
            "• Brújula de Navegación: flecha dorada en tiempo real hacia la salida con distancia.\n" +
            "• Nuevos Obstáculos: charcos de lodo ralentizante y latas ruidosas.\n" +
            "• Enemigo Secundario: Perro callejero territorial que ladra y alerta a La Llorona.\n" +
            "• Salida Monumental: portal de cantera con luz esmeralda y letrero claro en todos los niveles.\n" +
            "• 4 Niveles jugables completos (Mercado, Callejón, Atrio, Panteón).\n" +
            "• Escenas Splash, MainMenu, LevelSelect, Credits y Canvas de juego.\n\n" +
            "Para jugar: abre Assets/Scenes/Splash.unity y presiona ▶ Play.",
            "¡Excelente!"
        );

        Debug.Log("[MasterBuilder] ¡Construcción total completada!");
    }

    // ── Ventana de panel de control ──────────────────────────────────────────

    [MenuItem("La Llorona/Panel de Control")]
    public static void OpenControlPanel()
    {
        GetWindow<LaLloronaMasterBuilder>("Panel La Llorona");
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("La Llorona del Callejón — Generador Maestro", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Haz clic en el botón verde para generar o reconstruir automáticamente " +
            "todas las escenas, niveles, iluminación 2D, colisiones, obstáculos, HUD y menús del juego.",
            MessageType.Info
        );

        GUILayout.Space(15);
        GUI.backgroundColor = new Color(0.25f, 0.82f, 0.38f);
        if (GUILayout.Button("¡CONSTRUIR TODO EL PROYECTO (UN CLIC)!", GUILayout.Height(52)))
            BuildEverything();

        GUI.backgroundColor = Color.white;
        GUILayout.Space(18);
        GUILayout.Label("Generadores Individuales:", EditorStyles.boldLabel);

        if (GUILayout.Button("Generar Sprites del Juego (HUD, Brújula, Obstáculos)", GUILayout.Height(28)))
            SpriteAssetGenerator.GenerateAllSprites();

        if (GUILayout.Button("Nivel 1 — El Mercado Vacío", GUILayout.Height(30)))
            Level1Builder.BuildLevel1Scene();

        if (GUILayout.Button("Nivel 2 — El Callejón del Beso", GUILayout.Height(30)))
            Level2Builder.BuildLevel2Scene();

        if (GUILayout.Button("Nivel 3 — El Atrio de la Parroquia", GUILayout.Height(30)))
            Level3Builder.BuildLevel3Scene();

        if (GUILayout.Button("Nivel 4 — El Panteón de las Ánimas", GUILayout.Height(30)))
            Level4Builder.BuildLevel4Scene();

        GUILayout.Space(8);
        if (GUILayout.Button("Escenas de Menú y HUD (Splash, Menú, LevelSelect, Credits)", GUILayout.Height(30)))
            UIBuilder.BuildAllScenesAndUI();

        if (GUILayout.Button("ScriptableObjects de Dificultad (LloronaSettings x4)", GUILayout.Height(30)))
            DifficultySettingsBuilder.GenerateAllSettings();

        // ── Utilidades de prueba ──────────────────────────────────────────────
        GUILayout.Space(14);
        GUILayout.Label("Utilidades de Prueba:", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
        if (GUILayout.Button("🔓 Desbloquear Todos los Niveles (PlayerPrefs)", GUILayout.Height(34)))
            UnlockAllLevels();

        GUI.backgroundColor = new Color(1f, 0.65f, 0.2f);
        if (GUILayout.Button("🗑  Resetear Progreso (borrar PlayerPrefs)", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Resetear progreso",
                "¿Seguro? Se borrarán todos los PlayerPrefs del juego.", "Sí, borrar", "Cancelar"))
                ResetProgress();
        }

        GUI.backgroundColor = Color.white;
        GUILayout.Space(10);
        GUILayout.Label("Abrir escena directamente:", EditorStyles.miniLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Splash"))      OpenScene("Assets/Scenes/Splash.unity");
        if (GUILayout.Button("MainMenu"))    OpenScene("Assets/Scenes/MainMenu.unity");
        if (GUILayout.Button("LevelSelect")) OpenScene("Assets/Scenes/LevelSelect.unity");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Nivel 1")) OpenScene("Assets/Scenes/Level1_Mercado.unity");
        if (GUILayout.Button("Nivel 2")) OpenScene("Assets/Scenes/Level2_Callejon.unity");
        if (GUILayout.Button("Nivel 3")) OpenScene("Assets/Scenes/Level3_Atrio.unity");
        if (GUILayout.Button("Nivel 4")) OpenScene("Assets/Scenes/Level4_Cementerio.unity");
        GUILayout.EndHorizontal();
    }

    // ── Acciones del menú ────────────────────────────────────────────────────

    [MenuItem("La Llorona/Utilidades/Desbloquear Todos los Niveles")]
    public static void UnlockAllLevels()
    {
        UnityEngine.PlayerPrefs.SetInt("LL_UnlockedLevel", 4);
        UnityEngine.PlayerPrefs.Save();
        Debug.Log("[MasterBuilder] ✅ Todos los niveles desbloqueados (LL_UnlockedLevel = 4).");
        EditorUtility.DisplayDialog("Niveles desbloqueados",
            "¡Listo! Los 4 niveles están disponibles en Selección de Nivel.\n" +
            "Abre LevelSelect.unity y presiona Play para verificarlo.", "OK");
    }

    [MenuItem("La Llorona/Utilidades/Resetear Progreso (PlayerPrefs)")]
    public static void ResetProgress()
    {
        UnityEngine.PlayerPrefs.DeleteAll();
        UnityEngine.PlayerPrefs.Save();
        Debug.Log("[MasterBuilder] 🗑 PlayerPrefs borrados — progreso reseteado.");
        EditorUtility.DisplayDialog("Progreso reseteado",
            "PlayerPrefs eliminados. El juego empezará desde cero (Nivel 1 bloqueado).", "OK");
    }

    static void OpenScene(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            EditorUtility.DisplayDialog("Escena no encontrada",
                $"No existe: {path}\n\nEjecuta primero ¡CONSTRUIR TODO EL PROYECTO!", "OK");
            return;
        }
        if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
    }
}
