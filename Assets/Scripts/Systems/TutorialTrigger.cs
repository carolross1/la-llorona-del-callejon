// Assets/Scripts/Systems/TutorialTrigger.cs
using UnityEngine;

/// <summary>
/// Disparador en el escenario para mostrar instrucciones contextuales en el HUD
/// cuando Carlos ingresa en zonas clave. Puede configurarse para mostrarse
/// automáticamente al inicio de la escena (showOnStart = true).
/// </summary>
public class TutorialTrigger : MonoBehaviour
{
    [TextArea(2, 4)]
    [SerializeField] string tutorialMessage = "Presiona E para esconderte.";
    [SerializeField] float  displayDuration = 4.0f;
    [SerializeField] bool   triggerOnce     = true;
    [Tooltip("Si es verdadero, muestra el mensaje automáticamente 0.5 s después de cargar la escena.")]
    [SerializeField] bool   showOnStart     = false;

    bool hasTriggered;

    void Start()
    {
        if (showOnStart && !hasTriggered)
        {
            // Pequeño retraso para que el HUD esté inicializado
            Invoke(nameof(MostrarAlInicio), 0.6f);
        }
    }

    void MostrarAlInicio()
    {
        hasTriggered = true;
        if (HUD.Instance != null)
            HUD.Instance.ShowTutorial(tutorialMessage, displayDuration);
        if (triggerOnce)
            gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            if (HUD.Instance != null)
                HUD.Instance.ShowTutorial(tutorialMessage, displayDuration);

            if (triggerOnce)
                gameObject.SetActive(false);
        }
    }
}
