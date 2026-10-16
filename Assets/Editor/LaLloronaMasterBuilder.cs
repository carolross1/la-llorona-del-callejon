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

        // 1. ScriptableObjects de dificultad
        DifficultySettingsBuilder.GenerateAllSettings();

        // 2. Nivel 1
        Level1Builder.BuildLevel1Scene();

        // 3. Nivel 2
        Level2Builder.BuildLevel2Scene();

        // 4. Nivel 3
        Level3Builder.BuildLevel3Scene();

        // 5. Escenas de menú y Canvas de UI
        UIBuilder.BuildAllScenesAndUI();

        EditorUtility.DisplayDialog(
            "La Llorona del Callejón",
            "¡TODO EL PROYECTO FUE CONSTRUIDO CON ÉXITO!\n\n" +
            "Se han generado y configurado en tu proyecto:\n" +
            "• Escena Splash (con temporizador de 3s y logo)\n" +
            "• Escena MainMenu (Jugar, Continuar, Selector, Opciones, Créditos, Salir)\n" +
            "• Escena LevelSelect (Nivel 1 al 4 con validación de progreso)\n" +
            "• Escena Credits (Atribución, herramientas y arte con IA)\n" +
            "• Nivel 1: El Mercado Vacío (40x16, iluminación 2D, 3 escondites, waypoints)\n" +
            "• Nivel 2: El Callejón del Beso (largo 50 en L, faroles parpadeantes)\n" +
            "• Nivel 3: El Atrio de la Parroquia (fuente central, patrulla circular, ofrendas de velas)\n" +
            "• Canvas HUD con 3 corazones, cronómetro, puntaje, tutorial y menús modales\n" +
            "• Registro automático de las 7 escenas en File > Build Settings\n\n" +
            "Para jugar: Abre Assets/Scenes/Splash.unity y presiona Play.",
            "¡Excelente!"
        );

        Debug.Log("[MasterBuilder] ¡Construcción total completada!");
    }

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
            "Haz clic en el botón verde para generar o actualizar automáticamente todas las escenas, " +
            "niveles, iluminación 2D, colisiones y menús del juego según las especificaciones de la rúbrica.",
            MessageType.Info
        );

        GUILayout.Space(15);
        GUI.backgroundColor = new Color(0.3f, 0.85f, 0.4f);
        if (GUILayout.Button("¡CONSTRUIR TODO EL PROYECTO (UN CLIC)!", GUILayout.Height(50)))
        {
            BuildEverything();
        }

        GUI.backgroundColor = Color.white;
        GUILayout.Space(15);
        GUILayout.Label("Generadores Individuales:", EditorStyles.boldLabel);

        if (GUILayout.Button("Construir Nivel 1 (El Mercado Vacío)", GUILayout.Height(30)))
            Level1Builder.BuildLevel1Scene();

        if (GUILayout.Button("Construir Nivel 2 (El Callejón del Beso)", GUILayout.Height(30)))
            Level2Builder.BuildLevel2Scene();

        if (GUILayout.Button("Construir Nivel 3 (El Atrio de la Parroquia)", GUILayout.Height(30)))
            Level3Builder.BuildLevel3Scene();

        if (GUILayout.Button("Generar Escenas de Menú y UI (Splash, Menú, HUD)", GUILayout.Height(30)))
            UIBuilder.BuildAllScenesAndUI();

        if (GUILayout.Button("Generar Dificultades (ScriptableObjects)", GUILayout.Height(30)))
            DifficultySettingsBuilder.GenerateAllSettings();
    }
}
