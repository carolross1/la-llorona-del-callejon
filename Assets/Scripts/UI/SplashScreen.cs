// Assets/Scripts/UI/SplashScreen.cs
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Pantalla de inicio (Splash) que presenta el logotipo del equipo y el título del juego.
/// Carga automáticamente el Menú Principal tras 3 segundos o al presionar cualquier tecla.
/// </summary>
public class SplashScreen : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] float displayDuration = 3.0f;
    [SerializeField] string targetScene = "MainMenu";

    bool isMoving;

    void Start()
    {
        StartCoroutine(WaitAndProceedRoutine());
    }

    void Update()
    {
        if (isMoving) return;

        // Omitir con teclado o ratón
        bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (keyPressed || mousePressed)
        {
            StopAllCoroutines();
            Proceed();
        }
    }

    IEnumerator WaitAndProceedRoutine()
    {
        yield return new WaitForSeconds(displayDuration);
        Proceed();
    }

    void Proceed()
    {
        if (isMoving) return;
        isMoving = true;

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(targetScene);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
    }
}
