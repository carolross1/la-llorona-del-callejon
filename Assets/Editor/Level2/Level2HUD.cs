#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Módulo de Configuración del HUD para el Nivel 2.
/// Genera exclusivamente el HUD oficial colonial HD diseñado por Carol Rios (UIBuilder.BuildLevel2UICanvas).
/// </summary>
public static class Level2HUD
{
    public static void SetupHUD()
    {
        UIBuilder.BuildLevel2UICanvas();
        Debug.Log("[Level2HUD] HUD colonial HD oficial de Carol Rios generado para Nivel 2.");
    }
}
#endif
