// Assets/Scripts/Systems/TutorialTrigger.cs
using UnityEngine;

/// <summary>
/// Disparador en el escenario para mostrar instrucciones contextuales en el HUD
/// cuando Carlos ingresa en zonas clave de aprendizaje.
/// </summary>
public class TutorialTrigger : MonoBehaviour
{
    [TextArea(2, 4)]
    [SerializeField] string tutorialMessage = "Presiona E para esconderte.";
    [SerializeField] float displayDuration = 4.0f;
    [SerializeField] bool triggerOnce = true;

    bool hasTriggered;

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
