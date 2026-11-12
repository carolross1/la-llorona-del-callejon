#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Módulo de Configuración del HUD para el Nivel 2.
/// Asegura que se use exclusivamente el HUD oficial diseñado por Carol Rios (UIBuilder.BuildLevel2UICanvas),
/// elimina cualquier resto del HUD antiguo (HUD_Placa_Izquierda, HUD_Medallon_Centro, HUD_Placa_Derecha)
/// y garantiza que todas las referencias de HUD.cs estén correctamente vinculadas para que el tiempo corra
/// y las vidas se resten en pantalla de forma fluida.
/// </summary>
public static class Level2HUD
{
    public static void SetupHUD()
    {
        // 1. Reconstruir UICanvas oficial de Carol Rios
        UIBuilder.BuildLevel2UICanvas();

        Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        HUD hud = UnityEngine.Object.FindAnyObjectByType<HUD>();
        if (hud == null) return;

        // 2. Eliminar cualquier elemento remanente del HUD antiguo/propio
        string[] oldCustomHudNames = {
            "HUD_Placa_Izquierda",
            "HUD_Medallon_Centro",
            "HUD_Placa_Derecha",
            "HUD_Inferior_Corazones_Duplicados"
        };

        foreach (var oldName in oldCustomHudNames)
        {
            Transform tCanvas = canvas.transform.Find(oldName);
            if (tCanvas != null && tCanvas.gameObject != hud.gameObject)
            {
                UnityEngine.Object.DestroyImmediate(tCanvas.gameObject);
            }

            Transform tHud = hud.transform.Find(oldName);
            if (tHud != null)
            {
                UnityEngine.Object.DestroyImmediate(tHud.gameObject);
            }
        }

        // 3. Verificar y asegurar vinculación estricta de componentes de HUD.cs con el HUD oficial de Carol
        Transform topBar = hud.transform.Find("HUD_BarraSuperior");
        if (topBar == null) return;

        Transform leftCluster = topBar.Find("HUD_Izquierda_Estado");
        Transform centerCluster = topBar.Find("HUD_Centro_TiempoPuntos");
        Transform rightCluster = topBar.Find("HUD_Derecha_Objetivo");

        SerializedObject soHud = new SerializedObject(hud);

        // Corazones de vida oficiales de Carol
        if (leftCluster != null)
        {
            Transform heartsRoot = leftCluster.Find("Corazones_Vida");
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

            // Estamina oficial de Carol
            Transform stamRoot = leftCluster.Find("Estamina_Barra");
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

        // Tiempo y Puntos oficiales de Carol
        if (centerCluster != null)
        {
            Transform timerChild = centerCluster.Find("TimerText");
            if (timerChild != null)
            {
                soHud.FindProperty("timerText").objectReferenceValue = timerChild.GetComponent<Text>();
            }

            Transform scoreChild = centerCluster.Find("ScoreText");
            if (scoreChild != null)
            {
                soHud.FindProperty("scoreText").objectReferenceValue = scoreChild.GetComponent<Text>();
            }
        }

        // Objetivo y Alerta oficial de Carol
        if (rightCluster != null)
        {
            Transform objChild = rightCluster.Find("ObjectiveText");
            if (objChild != null)
            {
                soHud.FindProperty("objectiveText").objectReferenceValue = objChild.GetComponent<Text>();
            }

            Transform eyeChild = rightCluster.Find("IconoOjo_Alerta");
            if (eyeChild != null)
            {
                soHud.FindProperty("eyeIconImage").objectReferenceValue = eyeChild.GetComponent<Image>();
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

        soHud.ApplyModifiedProperties();
        EditorUtility.SetDirty(hud);
        Debug.Log("[Level2HUD] HUD oficial de Carol Rios configurado y vinculado exitosamente para Nivel 2.");
    }
}
#endif
