// Assets/Scripts/Managers/SaveSystem.cs
using UnityEngine;

/// <summary>
/// Sistema de guardado y carga persistente usando PlayerPrefs.
/// Gestiona nivel desbloqueado, récords de puntaje, audio y calidad gráfica.
/// </summary>
public static class SaveSystem
{
    const string KeyUnlockedLevel = "LL_UnlockedLevel";
    const string KeyHighScorePrefix = "LL_HighScore_Level_";
    const string KeyMusicVolume = "LL_MusicVolume";
    const string KeySfxVolume = "LL_SfxVolume";
    const string KeyQualityLevel = "LL_QualityLevel";

    /// <summary>
    /// Obtiene el nivel máximo desbloqueado (mínimo nivel 1).
    /// </summary>
    public static int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(KeyUnlockedLevel, 1);
    }

    /// <summary>
    /// Guarda el nivel desbloqueado si es mayor al actual.
    /// </summary>
    public static void UnlockLevel(int level)
    {
        int current = GetUnlockedLevel();
        if (level > current)
        {
            PlayerPrefs.SetInt(KeyUnlockedLevel, level);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Obtiene el puntaje máximo registrado para un nivel dado.
    /// </summary>
    public static int GetHighScore(int level)
    {
        return PlayerPrefs.GetInt(KeyHighScorePrefix + level, 0);
    }

    /// <summary>
    /// Guarda un nuevo récord de puntaje si supera el anterior.
    /// </summary>
    public static bool TrySaveHighScore(int level, int score)
    {
        int current = GetHighScore(level);
        if (score > current)
        {
            PlayerPrefs.SetInt(KeyHighScorePrefix + level, score);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Obtiene el volumen de música (rango 0 a 1, defecto 1).
    /// </summary>
    public static float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(KeyMusicVolume, 0.8f);
    }

    /// <summary>
    /// Guarda el volumen de música.
    /// </summary>
    public static void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(KeyMusicVolume, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Obtiene el volumen de efectos sonoros (rango 0 a 1, defecto 1).
    /// </summary>
    public static float GetSfxVolume()
    {
        return PlayerPrefs.GetFloat(KeySfxVolume, 0.9f);
    }

    /// <summary>
    /// Guarda el volumen de efectos sonoros.
    /// </summary>
    public static void SetSfxVolume(float volume)
    {
        PlayerPrefs.SetFloat(KeySfxVolume, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Obtiene el índice de calidad gráfica guardado (o el actual de Unity).
    /// </summary>
    public static int GetQualityLevel()
    {
        return PlayerPrefs.GetInt(KeyQualityLevel, QualitySettings.GetQualityLevel());
    }

    /// <summary>
    /// Guarda y aplica el nivel de calidad gráfica.
    /// </summary>
    public static void SetQualityLevel(int level)
    {
        PlayerPrefs.SetInt(KeyQualityLevel, level);
        QualitySettings.SetQualityLevel(level, true);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Borra todos los datos guardados (útil para pruebas o reiniciar progreso).
    /// </summary>
    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
