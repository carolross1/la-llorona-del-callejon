#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Módulo de Configuración del HUD para el Nivel 2.
/// Asegura que se use exclusivamente el HUD oficial diseñado por el equipo (UIBuilder.BuildLevel2UICanvas),
/// elimina elementos duplicados y garantiza que todas las referencias a vidas, tiempo, puntos y estamina
/// estén conectadas estrictamente para que el tiempo corra y las vidas se resten en pantalla.
/// </summary>
public static class Level2HUD
{
    public static void SetupHUD()
    {
        Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            UIBuilder.BuildLevel2UICanvas();
            canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;
        }

        HUD hud = UnityEngine.Object.FindAnyObjectByType<HUD>();

        // 1. Eliminar cualquier elemento duplicado o empalmado que compita con el HUD oficial
        string[] duplicateNames = {
            "HUD_Izquierda_Estado",
            "HUD_Centro_Tiempo",
            "HUD_Centro_TiempoPuntos",
            "HUD_Derecha_Objetivo",
            "HUD_Inferior_Corazones_Duplicados"
        };

        foreach (var dName in duplicateNames)
        {
            Transform dUnderCanvas = canvas.transform.Find(dName);
            if (dUnderCanvas != null && (hud == null || dUnderCanvas.gameObject != hud.gameObject))
            {
                UnityEngine.Object.DestroyImmediate(dUnderCanvas.gameObject);
            }

            if (hud != null)
            {
                Transform dUnderHud = hud.transform.Find(dName);
                if (dUnderHud != null)
                {
                    UnityEngine.Object.DestroyImmediate(dUnderHud.gameObject);
                }
            }
        }

        // 2. Verificar estructura del HUD oficial de Carol Rios
        Transform leftPlaque = hud != null ? hud.transform.Find("HUD_Placa_Izquierda") : null;
        Transform centerMedallon = hud != null ? hud.transform.Find("HUD_Medallon_Centro") : null;
        Transform rightPlaque = hud != null ? hud.transform.Find("HUD_Placa_Derecha") : null;

        if (hud == null || leftPlaque == null || centerMedallon == null || rightPlaque == null)
        {
            UIBuilder.BuildLevel2UICanvas();
            hud = UnityEngine.Object.FindAnyObjectByType<HUD>();
            if (hud == null) return;
            leftPlaque = hud.transform.Find("HUD_Placa_Izquierda");
            centerMedallon = hud.transform.Find("HUD_Medallon_Centro");
            rightPlaque = hud.transform.Find("HUD_Placa_Derecha");
        }

        // 3. Garantizar el enlazado estricto de todas las referencias de HUD.cs
        SerializedObject soHud = new SerializedObject(hud);

        // Enlazar los 3 corazones de vida
        Transform heartsRoot = leftPlaque != null ? leftPlaque.Find("Corazones_Vida") : null;
        if (heartsRoot != null)
        {
            SerializedProperty hpProp = soHud.FindProperty("heartImages");
            hpProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                Transform hChild = heartsRoot.Find($"Corazon_{i + 1}");
                Image hImg = hChild != null ? hChild.GetComponent<Image>() : null;
                hpProp.GetArrayElementAtIndex(i).objectReferenceValue = hImg;
            }
        }

        Sprite activeHeart = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD_Corazon_Lleno.png");
        Sprite lostHeart = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/HUD_Corazon_Vacio.png");
        if (activeHeart == null || lostHeart == null)
        {
            var uiSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/UI/interfaz.png").OfType<Sprite>().ToArray();
            if (activeHeart == null) activeHeart = uiSprites.FirstOrDefault(s => s.name == "Corazon_lleno");
            if (lostHeart == null) lostHeart = uiSprites.FirstOrDefault(s => s.name == "Corazon_vacio");
        }

        soHud.FindProperty("activeHeartSprite").objectReferenceValue = activeHeart;
        soHud.FindProperty("lostHeartSprite").objectReferenceValue = lostHeart;
        soHud.FindProperty("activeHeartColor").colorValue = Color.white;
        soHud.FindProperty("lostHeartColor").colorValue = Color.white;

        // Enlazar Tiempo y Puntos en Medallón Centro
        if (centerMedallon != null)
        {
            Transform tChild = centerMedallon.Find("TimerText");
            if (tChild != null) soHud.FindProperty("timerText").objectReferenceValue = tChild.GetComponent<Text>();

            Transform scChild = centerMedallon.Find("ScoreText");
            if (scChild != null) soHud.FindProperty("scoreText").objectReferenceValue = scChild.GetComponent<Text>();
        }

        // Enlazar Objetivo y Ojo en Placa Derecha
        if (rightPlaque != null)
        {
            Transform objChild = rightPlaque.Find("ObjectiveText");
            if (objChild != null) soHud.FindProperty("objectiveText").objectReferenceValue = objChild.GetComponent<Text>();

            Transform eyeChild = rightPlaque.Find("IconoOjo_Alerta");
            if (eyeChild != null) soHud.FindProperty("eyeIconImage").objectReferenceValue = eyeChild.GetComponent<Image>();
        }

        // Enlazar Estamina en Placa Izquierda
        if (leftPlaque != null)
        {
            Transform stamRoot = leftPlaque.Find("Estamina_Barra");
            if (stamRoot != null)
            {
                soHud.FindProperty("staminaCanvasGroup").objectReferenceValue = stamRoot.GetComponent<CanvasGroup>();
                Transform fillChild = stamRoot.Find("MarcoBarra/Fill");
                if (fillChild != null)
                {
                    soHud.FindProperty("staminaFillImage").objectReferenceValue = fillChild.GetComponent<Image>();
                }
            }
        }

        soHud.ApplyModifiedProperties();
        EditorUtility.SetDirty(hud);
        Debug.Log("[Level2HUD] ¡Todas las referencias de HUD vinculadas exitosamente (vidas, tiempo, puntos, estamina)!");
    }
}
#endif
