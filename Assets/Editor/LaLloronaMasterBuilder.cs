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

        // 1. ScriptableObjects de dificultad (requeridos antes de los niveles)
        DifficultySettingsBuilder.GenerateAllSettings();

        // 2. Niveles jugables
        Level1Builder.BuildLevel1Scene();
        Level2Builder.BuildLevel2Scene();
        Level3Builder.BuildLevel3Scene();
        Level4Builder.BuildLevel4Scene();

        // 3. Escenas de menú, HUD y registrar escenas en Build Settings
        UIBuilder.BuildAllScenesAndUI();

        EditorUtility.DisplayDialog(
            "La Llorona del Callejón",
            "¡TODO EL PROYECTO FUE CONSTRUIDO CON ÉXITO!\n\n" +
            "Se han generado y configurado:\n" +
            "• Escena Splash (logo + temporizador 3 s)\n" +
            "• Escena MainMenu (Jugar, Continuar, Opciones, Créditos, Salir)\n" +
            "• Escena LevelSelect (4 niveles con validación de desbloqueo)\n" +
            "• Escena Credits (atribución de arte IA)\n" +
            "• Nivel 1: El Mercado Vacío   (40×16, 3 escondites)\n" +
            "• Nivel 2: El Callejón del Beso (L-shape 50u, faroles parpadeantes)\n" +
            "• Nivel 3: El Atrio de la Parroquia (fuente, patrulla circular, velas)\n" +
            "• Nivel 4: El Panteón de las Ánimas (38×28, 4 escondites, escape final)\n" +
            "• Canvas HUD en todos los niveles (corazones, cronómetro, puntaje)\n" +
            "• 4 ScriptableObjects de dificultad\n" +
            "• Registro de las 8 escenas en File > Build Settings\n\n" +
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
        GUILayout.Label("La Llorona del Callejón — Generador", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Haz clic en el botón verde para generar o reconstruir automáticamente " +
            "todas las escenas, niveles, iluminación 2D, colisiones y menús del juego.",
            MessageType.Info
        );

        GUILayout.Space(15);
        GUI.backgroundColor = new Color(0.25f, 0.82f, 0.38f);
        if (GUILayout.Button("¡CONSTRUIR TODO EL PROYECTO (UN CLIC)!", GUILayout.Height(52)))
            BuildEverything();

        GUI.backgroundColor = Color.white;
        GUILayout.Space(18);
        GUILayout.Label("Generadores Individuales:", EditorStyles.boldLabel);

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
    }
}
