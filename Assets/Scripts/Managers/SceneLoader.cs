// Assets/Scripts/Managers/SceneLoader.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gestiona la carga asíncrona de escenas con transición de fundido a negro (Fade In / Fade Out).
/// Persiste entre escenas y opera con tiempo no escalado para funcionar incluso con el juego pausado.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Transición")]
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] CanvasGroup fadeCanvasGroup;

    bool isTransitioning;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureFadeOverlay();
    }

    void EnsureFadeOverlay()
    {
        if (fadeCanvasGroup != null) return;

        // Si no está asignado, construir overlay visual automáticamente
        GameObject canvasGo = new GameObject("SceneLoader_OverlayCanvas");
        canvasGo.transform.SetParent(transform);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Por encima de toda la UI

        canvasGo.AddComponent<GraphicRaycaster>();
        fadeCanvasGroup = canvasGo.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;

        GameObject blackImg = new GameObject("BlackScreen");
        blackImg.transform.SetParent(canvasGo.transform, false);

        Image img = blackImg.AddComponent<Image>();
        img.color = Color.black;

        RectTransform rt = blackImg.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Carga una escena por nombre con fundido.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName, -1));
    }

    /// <summary>
    /// Carga una escena por índice de compilación con fundido.
    /// </summary>
    public void LoadScene(int buildIndex)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(null, buildIndex));
    }

    /// <summary>
    /// Recarga la escena activa actual.
    /// </summary>
    public void ReloadCurrentLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        LoadScene(currentIndex);
    }

    /// <summary>
    /// Carga el siguiente nivel en la secuencia.
    /// </summary>
    public void LoadNextLevel()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            LoadScene(next);
        else
            LoadScene("MainMenu");
    }

    IEnumerator TransitionRoutine(string sceneName, int buildIndex)
    {
        isTransitioning = true;
        EnsureFadeOverlay();

        fadeCanvasGroup.blocksRaycasts = true;

        // Fundido a negro (Fade Out)
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;

        // Asegurar que el tiempo esté reanudado antes de cargar
        Time.timeScale = 1f;

        // Carga asíncrona de la escena
        AsyncOperation op = !string.IsNullOrEmpty(sceneName)
            ? SceneManager.LoadSceneAsync(sceneName)
            : SceneManager.LoadSceneAsync(buildIndex);

        if (op != null)
        {
            while (!op.isDone)
                yield return null;
        }

        // Breve pausa para estabilizar cámara y assets
        yield return new WaitForSecondsRealtime(0.1f);

        // Fundido desde negro (Fade In)
        t = fadeDuration;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;

        isTransitioning = false;
    }
}
