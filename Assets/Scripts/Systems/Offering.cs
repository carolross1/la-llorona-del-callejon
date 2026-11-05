// Assets/Scripts/Systems/Offering.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Representa una ofrenda (vela encendida) colocada en el suelo para distraer a La Llorona.
/// Al colocarse pacifica a La Llorona y se mantiene encendida durante la oracion.
/// </summary>
public class Offering : MonoBehaviour
{
    [Header("Configuración de Distracción")]
    [SerializeField] float attractionRadius = 30.0f;
    [SerializeField] float distractionDuration = 7.0f;

    [Header("Componentes Visuales")]
    [SerializeField] Light2D candleLight;
    [SerializeField] SpriteRenderer spriteRenderer;

    bool hasTriggered;

    void Start()
    {
        if (candleLight == null) candleLight = GetComponentInChildren<Light2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        TriggerDistraction();
    }

    public void TriggerDistraction()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        LloronaAI llorona = FindAnyObjectByType<LloronaAI>();
        if (llorona != null)
        {
            llorona.Distract(transform.position, distractionDuration);
        }

        StartCoroutine(ExtinguishRoutine());
    }

    IEnumerator ExtinguishRoutine()
    {
        yield return new WaitForSeconds(distractionDuration);

        // Apagar luz gradualmente
        if (candleLight != null)
        {
            float startInt = candleLight.intensity;
            float t = 0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime;
                candleLight.intensity = Mathf.Lerp(startInt, 0f, t);
                yield return null;
            }
        }

        Destroy(gameObject, 0.5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}
